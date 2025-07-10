using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class MCommodityType04ProviderTest : DataProviderTestCase<MCommodityType04Provider>
	{
		public void TestIMCommodity()
		{
			Assert("Should implement IMCommodity", Provider is IMCommodity);
		}

		public void TestDescriptionOfGoods()
		{
			SetUpTestData();
			entryLine.CL_Description = "ABC";
			AssertEquals("DescriptionOfGoods", "ABC", GetProvider().DescriptionOfGoods);
		}

		public void TestCusCode()
		{
			SetUpTestData();
			invoiceLine.ZG_CusNumber = "123";
			AssertEquals("CusCode", "123", GetProvider().CusCode);
		}

		public void TestQuotaOrderNumber()
		{
			SetUpTestData();
			invoiceLine.JI_ConcessionOrder = "Quo";
			AssertEquals("QuotaOrderNumber", "Quo", GetProvider().QuotaOrderNumber);
		}

		public void TestCommodityCode()
		{
			SetUpTestData();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_SupplementaryCode1 = "123";
			invoiceLine.JI_SupplementaryCode2 = "456";
			invoiceLine.AdditionalSupplementaryCodes.AddNew().CY_Code = "789";
			invoiceLine.CusLineTariffDetails.AddNew().BZ_Tariff = "AIS";

			CombineAssertions(() =>
			{
				AssertEquals("HarmonizedSystemSubheadingCode", "123456", Provider.CommodityCode.HarmonizedSystemSubheadingCode);
				AssertEquals("QuotaOrderNumber", "78", Provider.CommodityCode.CombinedNomenclatureCode);
				AssertEquals("TaricCode", "90", Provider.CommodityCode.TaricCode);
				AssertEquals("TaricAdditionalCode", "123456789", Provider.CommodityCode.TaricAdditionalCode.FirstOrDefault().TaricAdditionalCode);
				AssertEquals("TaricAdditionalCode", "1", Provider.CommodityCode.TaricAdditionalCode.FirstOrDefault().SequenceNumber);
				AssertEquals("SequenceNumber", "1", Provider.CommodityCode.NationalAdditionalCode.FirstOrDefault().SequenceNumber);
				AssertEquals("NationalAdditionalCode", "AIS", Provider.CommodityCode.NationalAdditionalCode.FirstOrDefault().NationalAdditionalCode);
			});
		}

		public void TestGoodsMeasure()
		{
			SetUpTestData();
			invoiceLine.JI_Weight = 12;
			invoiceLine.JI_WeightUQ = "KG";
			invoiceLine.JI_NetWeight = 12;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsSecondQuantity = 12;

			CombineAssertions(() =>
			{
				AssertEquals("GrossMass", 12m, Provider.GoodsMeasure.GrossMass);
				AssertEquals("NetMass", 12m, Provider.GoodsMeasure.NetMass);
				AssertEquals("SupplementaryUnits", 12m, Provider.GoodsMeasure.SupplementaryUnits);
			});
		}

		public void TestInvoiceLine()
		{
			SetUpTestData();
			invoiceLine.JI_LinePrice = 12;
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			CombineAssertions(() =>
			{
				AssertEquals("Amount", 12m, Provider.InvoiceLine.Amount);
				AssertEquals("Currency", "EUR", Provider.InvoiceLine.Currency);
			});
		}

		public void TestCalculationOfTaxes()
		{
			SetUpTestData();
			invoiceLine.JI_PrimaryPreference = "12";
			invoiceLine.JI_CL = entryLine.PK;
			var cusEntryLineFee = entryLine.Fees.AddNew();
			cusEntryLineFee.CF_ChargeAmount = 1;

			CombineAssertions(() =>
			{
				AssertEquals("12", Provider.CalculationOfTaxes.Preference);
				AssertEquals(1m, Provider.CalculationOfTaxes.TotalDutiesAndTaxesAmount);
				AssertEquals(1, Provider.CalculationOfTaxes.DutiesAndTaxes.Count);
			});
		}

		protected override MCommodityType04Provider GetProvider()
		{
			SetUpTestData();
			return new MCommodityType04Provider(entryLineWrapper);
		}

		void SetUpTestData()
		{
			if (entryLineWrapper == null)
			{
				var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
				entryLineWrapper = testBizObjs.entryLineWrapper;
				entryLineWrapper.Declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				entryLine = entryLineWrapper.EntryLine;
				invoiceLine = entryLineWrapper.RandomInvoiceLine;
				invoice = entryLineWrapper.RandomInvoiceHeader;
			}
		}

		EntryLineWrapper entryLineWrapper;
		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoice;
	}
}
