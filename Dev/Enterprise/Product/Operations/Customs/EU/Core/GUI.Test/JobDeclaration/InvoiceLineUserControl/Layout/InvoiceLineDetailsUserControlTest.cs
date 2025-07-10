using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class InvoiceLineDetailsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestFormattedProcedureCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.FormattedProcedureCodeFindBox);
		}

		public void TestUnformattedProcedureCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.UnformattedProcedureCodeFindBox);
		}

		public void TestDestinationCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.DestinationCodeFindBox);
		}

		public void TestDispatchCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.DispatchCodeFindBox);
		}

		public void TestDestinationUsingZZRefCusCodeListCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.DestinationUsingZZRefCusCodeListCodeFindBox);
		}

		public void TestSupplementaryCode1DropEdit()
		{
			AssertType<ZDropEdit>(control.SupplementaryCode1DropEdit);
		}

		public void TestSupplementaryCode2DropEdit()
		{
			AssertType<ZDropEdit>(control.SupplementaryCode2DropEdit);
		}

		public void TestAdditionalSupplementaryCodesUserControl()
		{
			AssertType<AdditionalSupplementaryCodesUserControl>(control.AdditionalSupplementaryCodesUserControl);
		}

		public void TestCountryOfSupplyCodeFindBox()
		{
			var countryOfSupplyCodeFindBox = control.CountryOfSupplyCodeFindBox;
			AssertType<ZCodeFindBox>(countryOfSupplyCodeFindBox);
			AssertEquals(ModuleIDs.NotAssigned, countryOfSupplyCodeFindBox.ModuleID);
		}

		public void TestAdditionalProcedureCodesUserControl()
		{
			AssertType<AdditionalProcedureCodesUserControl>(control.AdditionalProcedureCodesUserControl);
		}

		public void TestPreferenceCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.PreferenceCodeDropEdit);
		}

		public void TestQuotaDropEdit()
		{
			AssertType<ZDropEdit>(control.QuotaDropEdit);
		}

		public void TestSecondQuotaDropEdit()
		{
			AssertType<ZDropEdit>(control.SecondQuotaDropEdit);
		}

		public void TestAdditionalSupplementaryCodesAndGDMUserControl()
		{
			AssertType<AdditionalSupplementaryCodesAndGDMUserControl>(control.AdditionalSupplementaryCodesAndGDMUserControl);
		}

		public void TestCusNumberCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.CusNumberCodeFindBox);
		}

		public void TestQuotaWithCheckLinkUserControl()
		{
			AssertType<QuotaWithCheckLinkUserControl>(control.QuotaWithCheckLinkUserControl);
		}

		public void TestValuationAdjustmentPercentageCalcEdit()
		{
			AssertType<ZCalcEdit>(control.ValuationAdjustmentPercentageCalcEdit);
		}

		public void TestTransactionNatureDropEdit()
		{
			AssertType<ZDropEdit>(control.TransactionNatureDropEdit);
		}

		public void TestUsedGoodsCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.UsedGoodsCodeDropEdit);
		}

		public void TestReturnToOriginCheckBox()
		{
			AssertType<ZCheckBox>(control.ReturnToOriginCheckBox);
		}

		public void TestSecondaryTreatedProductCheckBox()
		{
			AssertType<ZCheckBox>(control.SecondaryTreatedProductCheckBox);
		}

		public void TestReturningGoodsReasonCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.ReturningGoodsReasonCodeDropEdit);
		}

		public void TestReturningGoodsReasonDetailTextBox()
		{
			AssertType<ZTextBox>(control.ReturningGoodsReasonDetailTextBox);
		}

		public void TestExportUnionThreadCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.ExportUnionThreadCodeFindBox);
		}

		public void TestExportUnionPackCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.ExportUnionPackCodeFindBox);
		}

		public void TestExportUnionProductionYearCalcEdit()
		{
			AssertType<ZCalcEdit>(control.ExportUnionProductionYearCalcEdit);
		}

		public void TestExportUnionDeferredInstallmentTextBox()
		{
			AssertType<ZTextBox>(control.ExportUnionDeferredInstallmentTextBox);
		}

		public void TestExportUnionEcologicalCheckBox()
		{
			AssertType<ZCheckBox>(control.ExportUnionEcologicalCheckBox);
		}

		public void TestInwardProcessingLicenseLineNumberTextBox()
		{
			AssertType<ZTextBox>(control.InwardProcessingLicenseLineNumberTextBox);
		}

		public void TestEntryExitPurposeCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.EntryExitPurposeCodeDropEdit);
		}

		public void TestEntryExitPurposeDetailTextBox()
		{
			AssertType<ZTextBox>(control.EntryExitPurposeDetailTextBox);
		}

		public void TestProcessingDescriptionLongTextControl()
		{
			AssertType<LongTextControl>(control.ProcessingDescriptionLongTextControl);
		}

		public void TestSupplementaryCode1AndGDMUserControl()
		{
			AssertType<SupplementaryCode1AndGDMUserControl>(control.SupplementaryCode1AndGDMUserControl);
		}

		public void TestBorderTradeStateCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.BorderTradeStateCodeFindBox);
		}

		public void TestExportUnionAdditionalTariffCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.ExportUnionAdditionalTariffCodeFindBox);
		}

		public void TestExcessStockCheckBox()
		{
			AssertType<ZCheckBox>(control.ExcessStockCheckBox);
		}

		public void TestCommercialPaymentCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.CommercialPaymentCodeDropEdit);
		}

		public void TestValuationCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.ValuationCodeFindBox);
		}

		public void TestNationalAdditionalCode1DropEdit()
		{
			AssertType<ZDropEdit>(control.NationalAdditionalCode1DropEdit);
		}

		public void TestNationalAdditionalCode2DropEdit()
		{
			AssertType<ZDropEdit>(control.NationalAdditionalCode2DropEdit);
		}

		public void TestNationalAdditionalCodesUserControl()
		{
			AssertType<NationalAdditionalCodesUserControl>(control.NationalAdditionalCodesUserControl);
		}

		public void TestRegionOfDestinationDropEdit()
		{
			AssertType<ZDropEdit>(control.RegionOfDestinationDropEdit);
		}

		public void TestPriceTypeDropEdit()
		{
			AssertType<ZDropEdit>(control.PriceTypeDropEdit);
		}

		InvoiceLineDetailsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
