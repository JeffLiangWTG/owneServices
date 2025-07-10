using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class PeriodForDischargeProviderTest : DataProviderTestCase<PeriodForDischargeProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("EntryInstruction missing", () => new PeriodForDischargeProvider(null));
		}

		public void TestPeriod()
		{
			AssertEquals("Period", "1", Provider.Period);
		}

		public void TestAutomaticExtension()
		{
			AssertEquals("AutomaticExtension", true, Provider.AutomaticExtension);
		}

		public void TestDetails()
		{
			AssertEquals("Details", "12", Provider.Details);
		}

		protected override PeriodForDischargeProvider GetProvider()
		{
			SetUpTestData();
			return new PeriodForDischargeProvider(entryInstruction);
		}

		void SetUpTestData()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PeriodForDischarge = 1;
			entryInstruction.ZG_PeriodForDischargeAutoExtension = true;
			entryInstruction.PeriodForDischargeDetails = "12";
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
	}
}
