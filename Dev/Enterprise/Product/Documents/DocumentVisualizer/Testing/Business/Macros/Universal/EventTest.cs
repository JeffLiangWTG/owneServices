using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class EventTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var stmEvent = Factory.LoadTop1<StmEvent>(new ZQuery());

			AssertNotNull("prerequisite", stmEvent);

			var eventInfo = new EventInfo(stmEvent);

			AssertEquals("Code", eventInfo.Code, stmEvent.SE_Code);
			AssertEquals("Description", eventInfo.Description, stmEvent.SE_Desc);
		}
	}
}