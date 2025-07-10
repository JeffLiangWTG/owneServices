namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DepartureMovementsTabGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MovementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).BeginInit();
			this.MovementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsDepartureMovementHeaderCollection<Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader>);
			// 
			// MovementsGrid
			// 
			this.MovementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MovementsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)))));
			this.MovementsGrid.CaptionVisible = false;
			this.MovementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementsGrid.GridId = "74cd0c16-8bb8-4a80-b525-7d6472dd5f16";
			this.MovementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MovementsGrid.LayoutKey = "MovementsGrid";
			this.MovementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MovementsGrid.Name = "MovementsGrid";
			this.MovementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 265, true);
			this.MovementsGrid.TabIndex = 0;
			// 
			// Phase5DepartureMovementsTabGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MovementsGrid);
			this.Name = "Phase5DepartureMovementsTabGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).EndInit();
			this.MovementsGrid.ResumeLayout(false);
			this.MovementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid MovementsGrid;
	}
}

