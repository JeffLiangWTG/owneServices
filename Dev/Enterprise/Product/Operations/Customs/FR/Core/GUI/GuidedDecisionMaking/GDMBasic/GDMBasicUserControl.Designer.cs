namespace Enterprise.Customs.FR.GUI.GDM
{
	partial class GDMBasicUserControl
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
		private void InitializeComponent()
		{
			this.RegionOrTerritoryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RegionOrTerritoryOfDestinationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.GDM.GuidedDecisionMakingBasic);
			//
			// RegionOrTerritoryOfDestinationDropEdit
			// 
			this.RegionOrTerritoryOfDestinationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RegionOrTerritoryOfDestinationDropEdit, "RegionOrTerritoryOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.GDM.GuidedDecisionMakingBasic)(null)).RegionOrTerritoryOfDestination)));
			this.RegionOrTerritoryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 241, true);
			this.RegionOrTerritoryOfDestinationDropEdit.Name = "RegionOrTerritoryOfDestinationDropEdit";
			this.RegionOrTerritoryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.RegionOrTerritoryOfDestinationDropEdit.TabIndex = 9;
			// 
			// GDMBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RegionOrTerritoryOfDestinationDropEdit);
			this.Name = "GDMBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 288, true);
			this.Controls.SetChildIndex(this.RegionOrTerritoryOfDestinationDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RegionOrTerritoryOfDestinationDropEdit.ResumeLayout(true);
			this.RegionOrTerritoryOfDestinationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit RegionOrTerritoryOfDestinationDropEdit;
	}
}
