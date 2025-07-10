using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class LineNumberAssignerTest : TestCaseWithFactory
	{
		public void TestShouldCompletelyReassignNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = MessageProcessorConstants.EntryStatusCodes.Cleared;
			AssertEquals(false, entryHeader.ShouldCompletelyReassignNumbers);
		}
	}
}
