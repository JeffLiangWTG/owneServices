using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailValidation))]
	sealed class CusLineTariffDetailValidationTest : Customs.Business.Testing.CusLineTariffDetailValidationTest
	{
		public void TestCheckBZ_Type()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(tariffDetail.BZ_TypeInfo, "A", "L");
		}

		public void TestCheckBZ_Tariff()
		{
			tariffDetail.BZ_Type = "L";
			ValidationTestHelper.AssertInvalidCodeMessageError(tariffDetail.BZ_TariffInfo, "L155512", "L121040");
		}

		public void TestCheckBZ_ExemptionReductionCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(tariffDetail.BZ_ExemptionReductionCodeInfo, "E03", "E01");
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
