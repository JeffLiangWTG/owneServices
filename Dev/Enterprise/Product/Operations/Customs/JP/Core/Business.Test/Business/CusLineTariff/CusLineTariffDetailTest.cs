using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	sealed class CusLineTariffDetailTest : Customs.Business.Testing.CusLineTariffDetailTest
	{
		public void TestLookups()
		{
			AssertType<CusLineTariffDetailLookups>(tariffDetail.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusLineTariffDetailValidation>(tariffDetail.Validation);
		}

		public void TestBZ_Tariff()
		{
			tariffDetail.BZ_Type = ZString.Empty;
			AssertEquals("BZ_Tariff should be readonly when BZ_Type is empty", true, tariffDetail.BZ_TariffInfo.ReadOnly);

			tariffDetail.BZ_Type = "L";
			AssertEquals("BZ_Tariff should not be readonly when BZ_Type is not empty", false, tariffDetail.BZ_TariffInfo.ReadOnly);

			tariffDetail.BZ_Tariff = "L111120";
			tariffDetail.BZ_Type = ZString.Empty;
			AssertEquals("BZ_Tariff should be set to empty when BZ_Type is set to empty", ZString.Empty, tariffDetail.BZ_Tariff);
		}

		public void TestRateFormula()
		{
			tariffDetail.BZ_Type = "L";
			tariffDetail.BZ_Tariff = "L121040";
			AssertEquals("15%", tariffDetail.RateFormula);
		}

		public void TestBZ_Value()
		{
			tariffDetail.BZ_ExemptionReductionCode = ZString.Empty;
			AssertEquals("BZ_Value should be readonly when BZ_ExemptionReductionCode is empty", true, tariffDetail.BZ_ValueInfo.ReadOnly);

			tariffDetail.BZ_ExemptionReductionCode = "E01";
			AssertEquals("BZ_Value should not be readonly when BZ_ExemptionReductionCode is not empty", false, tariffDetail.BZ_ValueInfo.ReadOnly);

			tariffDetail.BZ_Value = 99999999999 + 1;
			AssertEquals("The maximum value of BZ_Value should be 99999999999", new ZDecimal(99999999999), tariffDetail.BZ_Value);

			tariffDetail.BZ_ExemptionReductionCode = ZString.Empty;
			AssertEquals("BZ_Value Should be set to zero when BZ_ExemptionReductionCode is set to empty", 0m, tariffDetail.BZ_Value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new CusLineTariffDetailTestHelper();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoices = declaration.Invoices.AddNew();
			var invoiceLine = invoices.JobComInvoiceLines.AddNew();
			tariffDetail = invoiceLine.DomesticConsumptionTaxes.AddNew();
			helper.PrepareTestData(Factory);
		}

		CusLineTariffDetail tariffDetail;
	}
}
