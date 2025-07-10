using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class EconomicConditionsProviderTest : DataProviderTestCase<EconomicConditionsProvider>
	{
		public void TestProcessingProcedure()
		{
			AssertEquals("ProcessingProcedure", "A", Provider.ProcessingProcedure);
		}

		public void TestDetails()
		{
			AssertEquals("Details", "Procedure", Provider.Details);
		}

		protected override EconomicConditionsProvider GetProvider() => new EconomicConditionsProvider(entryInstruction);

		protected override void SetUp()
		{
			base.SetUp();

			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.ZG_ProcessingProcedureCode = "A";
			entryInstruction.ProcessingProcedureDetails = "Procedure";
		}
		CusEntryInstruction entryInstruction;
	}
}
