using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CommodityWrapperTest : DataProviderTestCase<CommodityWrapper>
	{
		public void TestCalculationOfTaxes()
		{
			AssertEquals("Count should reflect number of fees of the entry line.", 2, Provider.CalculationOfTaxes.DutiesAndTaxe.Count);
			AssertEquals("CcQualifier should equal CountryCodes.France.", CountryCodes.France, Provider.CalculationOfTaxes.DutiesAndTaxe.First().CcQualifier);
			AssertEquals("NationalTaxType should equal collection of fee.NationalFeeTypeCode.", "N001", Provider.CalculationOfTaxes.DutiesAndTaxe.First().NationalTaxType);
			AssertEquals("PayableTaxAmount should equal collection of fee.CF_ChargeAmount.", 3d, Provider.CalculationOfTaxes.DutiesAndTaxe.First().PayableTaxAmount);
			AssertEquals("TaxType should equal collection of fee.CF_ChargeType.", "A01", Provider.CalculationOfTaxes.DutiesAndTaxe.First().TaxType);
			AssertEquals("Preference should equal line.JI_PrimaryPreference.", "100", Provider.CalculationOfTaxes.Preference);
			AssertEquals("TotalDutiesAndTaxesAmount should equal sum of fees amount.", 7d, Provider.CalculationOfTaxes.TotalDutiesAndTaxesAmount);
		}

		public void TestCommodityCode()
		{
			AssertEquals("CombinedNomenclatureCode should equal 7th and 8th chars from any invoice line JI_Tariff.", "22", Provider.CommodityCode.CombinedNomenclatureCode);
			AssertEquals("HarmonizedSystemSubheadingCode should equal first 6  chars from any invoice line JI_Tariff.", "111111", Provider.CommodityCode.HarmonizedSystemSubheadingCode);
			AssertEquals("NationalAdditionalCode has only 1 element because .", 1, Provider.CommodityCode.NationalAdditionalCode.Count);
			AssertEquals("CcQualifier should equal CountryCodes.France.", CountryCodes.France, Provider.CommodityCode.NationalAdditionalCode.First().CcQualifier);
			AssertEquals("NationalAdditionalCode should equal any invoice JI_SupplementaryCode2.", "V905", Provider.CommodityCode.NationalAdditionalCode.First().NationalAdditionalCode);
			AssertEquals("TaricAdditionalCode has only 1 element.", 1, Provider.CommodityCode.TaricAdditionalCode.Count);
			AssertEquals("TaricAdditionalCode should equal line.JI_SupplementaryCode1.", "YYY", Provider.CommodityCode.TaricAdditionalCode.First().TaricAdditionalCode);
			AssertEquals("TaricCode should equal 9th and 10th chars from any invoice line JI_Tariff.", "33", Provider.CommodityCode.TaricCode);
		}

		public void TestCusCode()
		{
			AssertEquals("CusCode should equal line.ZG_CusNumber", "X1", Provider.CusCode);
		}

		public void TestDescriptionOfGoods()
		{
			AssertEquals("DescriptionOfGoods should equal line.JI_Description", "SOME GOODS", Provider.DescriptionOfGoods);
		}

		public void TestGoodsMeasure()
		{
			AssertEquals("GrossMass should equal line.JI_Weight.", 1d, Provider.GoodsMeasure.GrossMass);
			AssertEquals("NationalMeasurementUnitAndQualifier should map Customs quantities.", 2, Provider.GoodsMeasure.NationalSupplementaryUnits.Count);
			AssertEquals("NetMass should equal line.JI_Weight.", 3d, Provider.GoodsMeasure.NetMass);
			AssertEquals("SupplementaryUnits should equal line.JI_CustomsSecondQuantity.", 4d, Provider.GoodsMeasure.SupplementaryUnits);
		}

		public void TestInvoiceLine()
		{
			AssertEquals("ItemAmountInvoiced should equal line.JI_LinePrice.", 5d, Provider.InvoiceLine.ItemAmountInvoiced);
		}

		public void TestQuotaOrderNumber()
		{
			AssertEquals("QuotaOrderNumber should equal line.JI_ConcessionOrder", "TEST3", Provider.QuotaOrderNumber);
		}

		protected override CommodityWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_Tariff = "1111112233";
			line.JI_SupplementaryCode2 = "V905";
			line.JI_SupplementaryCode1 = "YYY";
			line.ZG_CusNumber = "X1";
			line.JI_Weight = 1d;
			line.JI_CustomsThirdUnitQty = "KG";
			line.JI_CustomsThirdQuantity = 2d;
			line.JI_NetWeight = 3d;
			line.JI_CustomsSecondQuantity = 4d;
			line.JI_LinePrice = 5d;
			line.JI_ConcessionOrder = "TEST3";
			line.JI_PrimaryPreference = "100";

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryLine = (CusEntryLine)line.CusEntryLine;
			entryLine.CL_Description = "SOME GOODS";
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeAmount = 3d;
			fee1.NationalFeeTypeCode = "N001";
			fee1.CF_ChargeType = "A01";

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeAmount = 4d;
			fee2.NationalFeeTypeCode = "N002";
			fee2.CF_ChargeType = "A02";

			return CommodityWrapper.New(entryLine);
		}
	}
}
