using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class ProcedureProviderTest : DataProviderTestCase<ProcedureProvider>
	{
		public void TestIProcedure()
		{
			Assert("Should implement IProcedure", Provider is IProcedure);
		}

		public void TestRequestedProcedure()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "78";
			AssertEquals("78", Provider.RequestedProcedure);

			invoiceLine.JI_Procedure = "90";
			AssertEquals("90", GetProvider().RequestedProcedure);
		}

		public void TestPreviousProcedure()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "0055001";
			AssertEquals("55", Provider.PreviousProcedure);

			invoiceLine.JI_Procedure = "0077001";
			AssertEquals("77", GetProvider().PreviousProcedure);
		}

		protected override ProcedureProvider GetProvider()
		{
			SetUpTestData();
			return new ProcedureProvider(invoiceLine);
		}

		void SetUpTestData()
		{
			if (instruction == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
			}
		}

		CusEntryInstruction instruction;
		JobComInvoiceLine invoiceLine;
	}
}
