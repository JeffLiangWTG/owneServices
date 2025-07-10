namespace Enterprise.Customs.AU.Module
{
	partial class InstrumentNumberFilterControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Type";
			zTextBoxColumnStyleInfo1.ColumnName = "IN_Type";
			zTextBoxColumnStyleInfo2.Caption = "Number";
			zTextBoxColumnStyleInfo2.ColumnName = "IN_Number";
			zTextBoxColumnStyleInfo3.Caption = "Tariff Numbers Relevant";
			zTextBoxColumnStyleInfo3.ColumnName = "TariffGroupRelevant";
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "IN_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "IN_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Caption = "Declaration Date";
			zDateEditColumnStyleInfo3.ColumnName = "IN_DeclarationDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Caption = "Revocation Date";
			zDateEditColumnStyleInfo4.ColumnName = "IN_RevocationDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo4.Caption = "Concessional Item No";
			zTextBoxColumnStyleInfo4.ColumnName = "IN_ConcessionalItemNumber";
			zTextBoxColumnStyleInfo5.Caption = "Instrument Category Code";
			zTextBoxColumnStyleInfo5.ColumnName = "IN_InstrumentCategoryCode";
			zTextBoxColumnStyleInfo6.Caption = "Linked Number";
			zTextBoxColumnStyleInfo6.ColumnName = "IN_LinkedNumber";
			zTextBoxColumnStyleInfo7.Caption = "Linked Type";
			zTextBoxColumnStyleInfo7.ColumnName = "IN_LinkedType";
			zTextBoxColumnStyleInfo8.Caption = "Tariff Validation Type";
			zTextBoxColumnStyleInfo8.ColumnName = "IN_TariffValidationType";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 391, true);
			this.FilteredGrid.TabIndex = 6;
			// 
			// InstrumentNumberFilterControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Business.CMR.ReferenceFiles.CMRInstrument";
			this.Name = "InstrumentNumberFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}

#endregion

	}
}
