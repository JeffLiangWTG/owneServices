using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	public class PreviousInbondMovementTest : TestCaseWithFactory
	{
		public void TestReturnsCorrectValues()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "123456";
			CombineAssertions(() =>
			{
				var previousInbondMovement = new PreviousInbondMovement(entryHeader.CusEntryNumber);
				AssertEquals($"CusEntryHeader returned", entryHeader, previousInbondMovement.EntryHeader);
				AssertEquals($"Entry number returned", "123456", previousInbondMovement.Number);
				foreach (var entryStatus in new[] { "PPW", "PDS", "PPS" })
				{
					entryHeader.CusEntryNumber.CE_EntryStatus = entryStatus;
					entryHeader.Declaration.JE_DeltaMode = "G1";
					AssertEquals($"Type for Entry Status: {entryStatus} / Delta Mode: G1", "2", previousInbondMovement.Type);
					entryHeader.Declaration.JE_DeltaMode = "G2";
					AssertEquals($"Type for Entry Status: {entryStatus} / Delta Mode: G2", "5", previousInbondMovement.Type);
				}
				foreach (var entryStatus in new[] { "RGA", "RGM", "ERR" })
				{
					entryHeader.CusEntryNumber.CE_EntryStatus = entryStatus;
					entryHeader.Declaration.JE_DeltaMode = "G1";
					AssertEquals($"Type for Entry Status: {entryStatus} / Delta Mode: G1", "1", previousInbondMovement.Type);
					entryHeader.Declaration.JE_DeltaMode = "G2";
					AssertEquals($"Type for Entry Status: {entryStatus} / Delta Mode: G2", "4", previousInbondMovement.Type);
				}
			});
		}
	}
}
