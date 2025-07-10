namespace Enterprise.Customs.AU.Module
{
	partial class AirCTOExportFilterStripControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Manifest Type";
			zTextBoxColumnStyleInfo1.ColumnName = "ED_ManifestType";
			zTextBoxColumnStyleInfo2.Caption = "Folio Reference";
			zTextBoxColumnStyleInfo2.ColumnName = "ED_FolioReference";
			zTextBoxColumnStyleInfo3.Caption = "Flight Number";
			zTextBoxColumnStyleInfo3.ColumnName = "ED_FlightNumber";
			zDateEditColumnStyleInfo1.Caption = "Departure Date";
			zDateEditColumnStyleInfo1.ColumnName = "ED_DepartureDate";
			zTextBoxColumnStyleInfo4.Caption = "Port Of Departure";
			zTextBoxColumnStyleInfo4.ColumnName = "ED_RL_NKPortOfDeparture";
			zTextBoxColumnStyleInfo5.Caption = "Port Of Destination";
			zTextBoxColumnStyleInfo5.ColumnName = "ED_RL_NKPortOfDestination";
			zTextBoxColumnStyleInfo6.Caption = "CAN";
			zTextBoxColumnStyleInfo6.ColumnName = "ED_CAN";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 264, true);
			// 
			// AirCTOExportFilterStripControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AirCTOExportCustomsManifestHeader";
			this.Name = "AirCTOExportFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
