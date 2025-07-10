using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ClientModuleIDTest : TestCase
	{
		public void TestConstructor()
		{
			ClientModuleIdentifier moduleID = new ClientModuleIdentifier(TestClientModuleId.TestClientModuleID, (NoResString)"TestClientModuleID");
			AssertNotNull("ClientModuleID", moduleID);
			AssertEquals("ID", "TestClientModuleID", moduleID.Name);
		}
	}
}
