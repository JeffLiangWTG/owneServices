using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ClientControllerIDTest : TestCase
	{
		public void TestConstructor()
		{
			ClientControllerID controllerID = new ClientControllerID("TestClientControllerID");
			AssertNotNull("ClientControllerID", controllerID);
			AssertEquals("ID", "TestClientControllerID", controllerID.Name);
		}
	}
}
