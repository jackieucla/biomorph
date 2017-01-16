﻿using HelixToolkit.Wpf;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Biomorpher.IGA;
using Biomorpher;
using Grasshopper.Kernel;
using MahApps.Metro.Controls;
using System.Windows.Controls.Primitives;

namespace Biomorpher
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class BiomorpherWindow : MetroWindow
    {

        // Fields
        private int generation;
        private int popSize;
        private Population population;
        private PopHistory popHistory;
        private List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders;
        private bool GO;
        private BiomorpherComponent owner;
        private double mutateProbability;

        //Grid myGrid;
        //List<UserControl1> myUserControls;

        // Constructor
        public BiomorpherWindow(BiomorpherComponent Owner)
        {
            // Set the component passed here to a field
            owner = Owner;

            // Get sliders
            sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();
            owner.GetSliders(sliders);
            
            // GA things
            generation = 0;
            popSize = 12;               // TODO: The population number needs to come from the user
            mutateProbability = 0.01;   // TODO: The mutate probability needs to come from the user
            population = new Population(popSize, sliders.Count);
            popHistory = new PopHistory();

            // Get the phenotypes for the first time... 
            // note that this should probably be somewhere else later, AFTER the user has engaged with the interface.
            // because we don't know what population size they want yet.
            GetPhenotypes();
 
            // Initial Window things
            InitializeComponent();
            Topmost = true;

            List<Mesh> rDesigns = getRepresentativePhenotypes(population);
            create3dViewportGrid(rDesigns);



            //myUserControls = new List<UserControl1>();

            // 1. create the grid?
            // 2. display current population here
            // 3. add user controls?
            // 4. add them as children to this window?

        }

        /// <summary>
        /// Gets the phenotype information for all the current chromosomes
        /// </summary>
        public void GetPhenotypes()
        {
            // Get geometry for each chromosome in the initial population
            for (int i = 0; i < population.chromosomes.Length; i++)
            {
                owner.canvas.Document.Enabled = false;                  // Disable the solver before tweaking sliders
                owner.SetSliders(population.chromosomes[i], sliders);   // Change the sliders
                owner.canvas.Document.Enabled = true;                   // Enable the solver again
                owner.ExpireSolution(true);                             // Now expire the main component and recompute
                owner.GetGeometry(population.chromosomes[i]);           // Get the new geometry for this particular chromosome
            }
        }

        /// <summary>
        /// When this gets called (probably via a button being triggered) we advance a generation 
        /// </summary>
        public void Run()
        {
            // 1. Create new populaltion using user selection
            population.RoulettePop();

            // 2. Mutate population using user preferences
            population.MutatePop(mutateProbability);

            // 3. Get geometry for each chromosome
            GetPhenotypes();

            // 4. Advance the generation counter and store the population historically.
            popHistory.AddPop(population);
            generation++;
            
        }


        public void Exit()
        {
            // Set sliders and get geometry for a chosen chromosome

            // Close the window
        }





        //UI methods
        public void create3dViewportGrid(List<Mesh> meshes)
        {
            //Create grid 3x4 layout
            int rowCount = 3;
            int columnCount = 4;
            Grid grid = createGrid(rowCount, columnCount, Tab1_primary.Width, Tab1_primary.Height);


            //For each grid cell: create border, dock panel and add checkbox and 3d viewport controls
            for(int i=0; i<meshes.Count; i++)
            {
                Border border = new Border();
                border.Padding = new Thickness(5);

                DockPanel dp = new DockPanel();

                //Checkbox
                string name = "cb_design_" + i;
                CheckBox cb = createCheckBox(name);
                cb.HorizontalAlignment = HorizontalAlignment.Right;

                DockPanel.SetDock(cb, Dock.Top);
                dp.Children.Add(cb);

                //3d viewport
                Viewport3d vp3d = new Viewport3d(meshes[i]);
                dp.Children.Add(vp3d);

                border.Child = dp;



                //add dockpanel to grid
                Grid.SetRow(border, (int)(i / 4));
                Grid.SetColumn(border, i % 4);
                grid.Children.Add(border);
            }

            //add to primary area of tab 1
            Tab1_primary.Child = grid;
        }



        //Create Grid control
        private Grid createGrid(int rowCount, int columnCount, double width, double height)
        {
            Grid grid = new Grid();
            grid.Width = width;
            grid.Height = height;
            grid.ShowGridLines = true;

            for (int i = 0; i < rowCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < columnCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            return grid;
        }


        //Create checkbox control
        private CheckBox createCheckBox(string name)
        {
            CheckBox cb = new CheckBox();
            cb.Name = name;
            return cb;
        }


        //To do: change to get centroids from K-means clustering
        private List<Mesh> getRepresentativePhenotypes(Population pop)
        {
            List<Mesh> phenotypes = new List<Mesh>();

            Chromosome[] chromosomes = pop.chromosomes;
            for(int i=0; i<12; i++)
            {
                phenotypes.Add(chromosomes[i].phenotype[0]);
            }

            return phenotypes;
        }







    }
}
