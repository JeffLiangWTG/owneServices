namespace Enterprise.Registry.GUI
{
	partial class GlowOpportunityStageRegistryControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.GlowOpportunityStageRegistryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowOpportunityStageRegistryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.GlowOpportunityStageCollection);
			// 
			// GlowOpportunityStageRegistryGrid
			// 
			this.GlowOpportunityStageRegistryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GlowOpportunityStageRegistryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.GlowOpportunityStage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowOpportunityStage)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowOpportunityStage)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.GlowOpportunityStage)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.GlowOpportunityStage)(null)).WinProbability)));
			this.GlowOpportunityStageRegistryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a64018cb-4945-422a-8e6c-773bb803c014", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3e132d62-b562-40c7-b4b1-7f6a556cd78a", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b2d3993a-13d1-49f4-99d8-a6fb3be66e63", "Enabled");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4b249581-df48-4abd-9710-1cc83110edb4", "Win Probability %");
			zCalcEditColumnStyleInfo1.ColumnName = "WinProbability";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.GlowOpportunityStageRegistryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GlowOpportunityStageRegistryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GlowOpportunityStageRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.GlowOpportunityStageRegistryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);

			this.GlowOpportunityStageRegistryGrid.CopySelectedRowsAllowed = false;
			this.GlowOpportunityStageRegistryGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.GlowOpportunityStageRegistryGrid.GridId = "7db321cd-37ee-4fd3-8418-47380533d313";
			this.GlowOpportunityStageRegistryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlowOpportunityStageRegistryGrid.LayoutKey = "GlowOpportunityStageRegistryGrid";
			this.GlowOpportunityStageRegistryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlowOpportunityStageRegistryGrid.Name = "GlowOpportunityStageRegistryGrid";
			this.GlowOpportunityStageRegistryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 389, true);
			this.GlowOpportunityStageRegistryGrid.TabIndex = 0;
			// 
			// UpButton
			// 
			this.UpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d1834e3b-ddec-4073-890e-88d743ca5978", "Move Up");
			this.UpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 3, true);
			this.UpButton.Name = "UpButton";
			this.UpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpButton.TabIndex = 1;
			this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// DownButton
			// 
			this.DownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4be81931-8f8f-4257-9493-0909c791c6ad", "Move Down");
			this.DownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 32, true);
			this.DownButton.Name = "DownButton";
			this.DownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DownButton.TabIndex = 2;
			this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
			// 
			// GlowOpportunityStageRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GlowOpportunityStageRegistryGrid);
			this.Controls.Add(this.DownButton);
			this.Controls.Add(this.UpButton);
			this.Name = "GlowOpportunityStageRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowOpportunityStageRegistryGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid GlowOpportunityStageRegistryGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpButton;
		private Enterprise.ZArchitecture.GUI.ZButton DownButton;
	}
}
