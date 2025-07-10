using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public partial class OrgSupplierPartFormCustomsControlGlobal : Customs.GUI.OrgSupplierPartFormCustomsControlGlobal
	{
		public OrgSupplierPartFormCustomsControlGlobal()
		{
			InitializePivotGridNewColumns();
		}

		void InitializePivotGridNewColumns()
		{
			var countryOfOrigin = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			countryOfOrigin.ColumnName = nameof(CusClassPartPivot.CI_RN_NKCountryOfOrigin);
			countryOfOrigin.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			countryOfOrigin.CaptionResourceString = Res.GetData("7201C7AD-2090-43E0-8620-3416038986F8", "ORG", "Goods Origin", "Country/Region of Origin of the goods");
			PivotGrid.ColumnStyles.Add(countryOfOrigin);

			var primaryPreference = new ZArchitecture.ZTextBoxColumnStyleInfo();
			primaryPreference.ColumnName = nameof(CusClassPartPivot.CI_PrimaryPreference);
			primaryPreference.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(primaryPreference);

			var tradeControlOrderAppendix = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			tradeControlOrderAppendix.ColumnName = nameof(CusClassPartPivot.CI_TradeControlOrderAppendix);
			tradeControlOrderAppendix.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(tradeControlOrderAppendix);

			var fEFTAArticle48 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			fEFTAArticle48.ColumnName = nameof(CusClassPartPivot.CI_FEFTAArticle48);
			fEFTAArticle48.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(fEFTAArticle48);

			var storageType = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			storageType.ColumnName = nameof(CusClassPartPivot.CI_StorageType);
			storageType.CaptionResourceString = Res.GetData("JPImportInvoiceLineUserControl|CI_StorageType", "Storage Type");
			storageType.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(storageType);

			var advanceRulingOnClassification = new ZArchitecture.ZTextBoxColumnStyleInfo();
			advanceRulingOnClassification.ColumnName = nameof(CusClassPartPivot.CI_AdvanceRulingOnClassification);
			advanceRulingOnClassification.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			PivotGrid.ColumnStyles.Add(advanceRulingOnClassification);

			var advanceRulingOnOrigin = new ZArchitecture.ZTextBoxColumnStyleInfo();
			advanceRulingOnOrigin.ColumnName = nameof(CusClassPartPivot.CI_AdvanceRulingOnOrigin);
			advanceRulingOnOrigin.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			PivotGrid.ColumnStyles.Add(advanceRulingOnOrigin);

			var dutyReductionExemptionRefundCode = new ZArchitecture.ZTextBoxColumnStyleInfo();
			dutyReductionExemptionRefundCode.ColumnName = nameof(CusClassPartPivot.CI_DutyReductionExemptionRefundCode);
			dutyReductionExemptionRefundCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(dutyReductionExemptionRefundCode);

			var domesticConsumptionTaxExemptionCode = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			domesticConsumptionTaxExemptionCode.ColumnName = nameof(CusClassPartPivot.CI_DomesticConsumptionTaxExemptionCode);
			domesticConsumptionTaxExemptionCode.CaptionResourceString = Res.GetData("6e2a144e-5ec9-47b3-b581-305088b73ce6", "Consumption Tax Exemption");
			domesticConsumptionTaxExemptionCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			PivotGrid.ColumnStyles.Add(domesticConsumptionTaxExemptionCode);

			var domesticConsumptionTaxExemptionIsPartial = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			domesticConsumptionTaxExemptionIsPartial.ColumnName = nameof(CusClassPartPivot.CI_DomesticConsumptionTaxExemptionIsPartial);
			domesticConsumptionTaxExemptionIsPartial.CaptionResourceString = Res.GetData("af82a91f-2c11-40fe-9100-5500f7a1db7e", "Partially Applied");
			domesticConsumptionTaxExemptionIsPartial.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(domesticConsumptionTaxExemptionIsPartial);

			var dutyReductionAmount = new ZArchitecture.ZTextBoxColumnStyleInfo();
			dutyReductionAmount.ColumnName = nameof(CusClassPartPivot.CI_DutyReductionAmount);
			dutyReductionAmount.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			PivotGrid.ColumnStyles.Add(dutyReductionAmount);
		}
	}
}
