namespace Enterprise.Customs.AU.Module
{
	partial class CMRCodeListsFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Code Type";
			zTextBoxColumnStyleInfo1.ColumnName = "CI_CodeType";
			zTextBoxColumnStyleInfo2.Caption = "Code";
			zTextBoxColumnStyleInfo2.ColumnName = "CI_Code";
			zTextBoxColumnStyleInfo3.Caption = "Short Desc";
			zTextBoxColumnStyleInfo3.ColumnName = "CI_Name";
			zTextBoxColumnStyleInfo4.Caption = "Long Desc";
			zTextBoxColumnStyleInfo4.ColumnName = "CI_Description";
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "CI_Startdate";
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "CI_EndDate";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Version";
			zCalcEditColumnStyleInfo1.ColumnName = "CI_Version";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 414, true);
			this.FilteredGrid.TabIndex = 12;
			// 
			// CMRCodeListsFilterControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRCodeLists";
			this.Name = "CMRCodeListsFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 440, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
