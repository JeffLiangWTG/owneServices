using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class EntryInstructionChangingEventTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var oldEntryInstruction = Factory.New<CusEntryInstruction>();
			var newEntryInstruction = Factory.New<CusEntryInstruction>();
			var changeEvent = new EntryInstructionChangingEvent(oldEntryInstruction, newEntryInstruction);

			CombineAssertions(() =>
			{
				AssertSame("OldEntryInstruction", oldEntryInstruction, changeEvent.OldEntryInstruction);
				AssertSame("NewEntryInstruction", newEntryInstruction, changeEvent.NewEntryInstruction);
			});
		}
	}
}
