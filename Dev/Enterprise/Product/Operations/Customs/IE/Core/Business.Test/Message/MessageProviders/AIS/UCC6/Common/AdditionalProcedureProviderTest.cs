using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class AdditionalProcedureProviderTest : DataProviderTestCase<AdditionalProcedureProvider>
	{
		public void TestICcQualifierAdditionalProcedure()
		{
			Assert("Should implement ICcQualifierAdditionalProcedure", Provider is ICcQualifierAdditionalProcedure);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("SequenceNumber", "1", GetProvider().SequenceNumber);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", GetProvider().CcQualifier);
		}

		public void TestAdditionalProcedure()
		{
			SetUpTestData();
			AssertEquals("AdditionalProcedure", "F48", Provider.AdditionalProcedure);
		}

		protected override AdditionalProcedureProvider GetProvider()
		{
			SetUpTestData();
			return new AdditionalProcedureProvider(1, additionalProdureCode);
		}

		void SetUpTestData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			additionalProdureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProdureCode.CY_Code = "1234F48";
		}

		JobComInvoiceLine invoiceLine;
		AdditionalProcedureCode additionalProdureCode;
	}
}
