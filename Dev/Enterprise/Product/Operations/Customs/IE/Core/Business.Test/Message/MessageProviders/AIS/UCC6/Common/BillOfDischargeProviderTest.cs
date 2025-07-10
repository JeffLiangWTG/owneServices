using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class BillOfDischargeProviderTest : DataProviderTestCase<BillOfDischargeProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("EntryInstruction missing", () => new BillOfDischargeProvider(null));
		}

		public void TestDetails()
		{
			AssertEquals("Details", "23", Provider.Details);
		}

		public void TestDeadline()
		{
			AssertEquals("Deadline", "1", Provider.Deadline);
		}

		public void TestUseOfTheBillOfDischarge()
		{
			AssertEquals("UseOfTheBillOfDischarge", true, Provider.UseOfTheBillOfDischarge);
		}

		protected override BillOfDischargeProvider GetProvider()
		{
			SetUpTestData();
			return new BillOfDischargeProvider(entryInstruction);
		}

		void SetUpTestData()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_BillOfDischargeIsNecessary = true;
			entryInstruction.ZG_BillOfDischargeDeadline = 1;
			entryInstruction.BillOfDischargeDetails = "23";
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
	}
}
