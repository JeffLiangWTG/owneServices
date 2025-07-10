using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public partial class ItineraryForManifestHeaderUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.itineraryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.itineraryGrid)).BeginInit();
			this.itineraryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderBase);
			// 
			// itineraryGrid
			// 
			this.itineraryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.itineraryGrid, "Itinerary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderBase)(null)).Itinerary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.ICS.Business.RouteEntry)(((System.Collections.IList)(((Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderBase)(null)).Itinerary)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.ICS.Business.RouteEntry)(((System.Collections.IList)(((Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderBase)(null)).Itinerary)).SyncRoot)).CY_Data)));
			this.itineraryGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.itineraryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.itineraryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.itineraryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itineraryGrid.GridId = "FF3551D5-9620-492E-891B-CEB562CD3789";
			this.itineraryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.itineraryGrid.LayoutKey = "IcsItineraryGrid";
			this.itineraryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itineraryGrid.Name = "itineraryGrid";
			this.itineraryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 343, true);
			this.itineraryGrid.TabIndex = 0;
			// 
			// ItineraryForManifestHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.itineraryGrid);
			this.Name = "ItineraryForManifestHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 343, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.itineraryGrid)).EndInit();
			this.itineraryGrid.ResumeLayout(false);
			this.itineraryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		ZGrid itineraryGrid;

		#endregion
	}
}
