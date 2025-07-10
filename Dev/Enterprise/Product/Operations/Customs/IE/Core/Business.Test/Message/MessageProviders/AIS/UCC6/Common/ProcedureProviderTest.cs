using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class ProcedureProviderTest : DataProviderTestCase<ProcedureProvider>
	{
		public void TestIProcedure()
		{
			Assert("Should implement IProcedure", Provider is IProcedure);
		}

		public void TestRequestedProcedure()
		{
			SetUpTestData();
			AssertEquals("RequestedProcedure", "12", Provider.RequestedProcedure);
		}

		public void TestPreviousProcedure()
		{
			SetUpTestData();
			AssertEquals("PreviousProcedure", "34", Provider.PreviousProcedure);
		}

		public void TestAdditionalProcedure()
		{
			SetUpTestData();
			AssertEquals("AdditionalProcedure", "F48", Provider.AdditionalProcedure.FirstOrDefault().AdditionalProcedure);
			AssertEquals("SequenceNumber", "1", Provider.AdditionalProcedure.FirstOrDefault().SequenceNumber);
		}

		protected override ProcedureProvider GetProvider()
		{
			SetUpTestData();
			return new ProcedureProvider(invoiceLine);
		}

		void SetUpTestData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "1234";
			var additionalProcedure = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedure.CY_Code = "1234F48";
		}

		JobComInvoiceLine invoiceLine;
	}
}
