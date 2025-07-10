namespace Enterprise.Registry.GUI
{
	partial class GlowOpportunityScopePrioritySequenceRegistryControl
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
			this.GlowOpportunityScopePrioritySequenceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowOpportunityScopePrioritySequenceGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.GlowOpportunityScopePrioritySequenceCollection);
			// 
			// GlowOpportunityStageRegistryGrid
			// 
			this.GlowOpportunityScopePrioritySequenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GlowOpportunityScopePrioritySequenceGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.GlowOpportunityScopePrioritySequence)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.GlowOpportunityScopePrioritySequence)(null)).Description)));
			this.GlowOpportunityScopePrioritySequenceGrid.CaptionVisible = false;

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3DA40234-16DA-430B-B09F-9EBB8AFD631E", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			this.GlowOpportunityScopePrioritySequenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			this.GlowOpportunityScopePrioritySequenceGrid.CopySelectedRowsAllowed = false;
			this.GlowOpportunityScopePrioritySequenceGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.GlowOpportunityScopePrioritySequenceGrid.GridId = "AE5FC2A9-EA88-4ECB-B4A9-08036B2CFF29";
			this.GlowOpportunityScopePrioritySequenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlowOpportunityScopePrioritySequenceGrid.LayoutKey = "GlowOpportunityStageRegistryGrid"; 
			this.GlowOpportunityScopePrioritySequenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlowOpportunityScopePrioritySequenceGrid.Name = "GlowOpportunityStageRegistryGrid";
			this.GlowOpportunityScopePrioritySequenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 389, true);
			this.GlowOpportunityScopePrioritySequenceGrid.TabIndex = 0;
			// 
			// UpButton
			// 
			this.UpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("D7CFB90E-01AD-4A7C-8619-5E7CDC5A835E", "Move Up");
			this.UpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 3, true);
			this.UpButton.Name = "UpButton";
			this.UpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpButton.TabIndex = 1;
			this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// DownButton
			// 
			this.DownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("93A24C97-3BF3-49F8-9622-D0441B8DA82C", "Move Down");
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
			this.Controls.Add(this.GlowOpportunityScopePrioritySequenceGrid);
			this.Controls.Add(this.DownButton);
			this.Controls.Add(this.UpButton);
			this.Name = "GlowOpportunityStageRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GlowOpportunityScopePrioritySequenceGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid GlowOpportunityScopePrioritySequenceGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpButton;
		private Enterprise.ZArchitecture.GUI.ZButton DownButton;
	}
}
