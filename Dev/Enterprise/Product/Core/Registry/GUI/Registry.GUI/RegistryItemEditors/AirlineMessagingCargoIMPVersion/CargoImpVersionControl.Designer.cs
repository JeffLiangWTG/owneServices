using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class CargoImpVersionControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private ZGrid airlineImpVersionsGrid;
		private ZDropEdit defaultVersionDropEdit;

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
			components = new System.ComponentModel.Container();
			this.SuspendLayout();
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion.CargoImpVersionConfiguration);

			defaultVersionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			defaultVersionDropEdit.Name = "defaultVersionDropEdit";
			defaultVersionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			defaultVersionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			defaultVersionDropEdit.TabIndex = 1;
			this.BindingSource.SetBindingMember(this.defaultVersionDropEdit, "DefaultImpVersion");

			this.airlineImpVersionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.airlineImpVersionsGrid.CaptionVisible = false;

			this.airlineImpVersionsGrid.GridId = "8F0C10EA-9359-42E3-869A-F3B2C9EAE304";
			this.airlineImpVersionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.airlineImpVersionsGrid.LayoutKey = "AirlineImpVersionGrid";
			this.airlineImpVersionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.airlineImpVersionsGrid.Name = "AirlineImpVersionGrid";
			this.airlineImpVersionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 360, true);
			this.airlineImpVersionsGrid.TabIndex = 2;
			this.BindingSource.SetBindingMember(this.airlineImpVersionsGrid, "AirlineImpVersionMappings");

			var airlinePrefixColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			airlinePrefixColumn.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("089CBADF-4287-4812-9F59-FE1853AA4903", "Airline");
			airlinePrefixColumn.ColumnName = "AirlinePrefix";
			airlinePrefixColumn.IsMandatory = true;
			airlinePrefixColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			airlinePrefixColumn.DefaultCollectionIndex = 1;
			this.airlineImpVersionsGrid.ColumnStyles.Add(airlinePrefixColumn);

			var impVersionColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			impVersionColumn.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("558A40D9-3203-4677-86C8-2137D13F3ACB", "Message Version");
			impVersionColumn.ColumnName = "ImpVersion";
			impVersionColumn.IsMandatory = true;
			impVersionColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			impVersionColumn.DefaultCollectionIndex = 2;
			this.airlineImpVersionsGrid.ColumnStyles.Add(impVersionColumn);

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.airlineImpVersionsGrid);
			this.Controls.Add(this.defaultVersionDropEdit);
			this.Name = "CargoImpVersionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 460, true);
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.airlineImpVersionsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
