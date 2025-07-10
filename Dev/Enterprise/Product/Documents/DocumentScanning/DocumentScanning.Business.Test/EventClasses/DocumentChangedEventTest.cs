using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocumentChangedEventArgsTest : TestCaseWithFactory
	{
		public void TestDescription()
		{
			DocumentChangedEventArgs testArgs = new DocumentChangedEventArgs(Events.EditedARecord, "Edited");
			AssertEquals("Edited", testArgs.Description);
		}

		public void TestChangeType()
		{
			DocumentChangedEventArgs testArgs = new DocumentChangedEventArgs(Events.EditedARecord, "Edited");
			AssertEquals(Events.EditedARecord, testArgs.ChangeType);
		}
	}
}
