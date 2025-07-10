using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class RegistrationIdentifierTest : TestCase
	{
		public void TestToString()
		{
			RegistrationIdentifier identifier = new RegistrationIdentifier("Dummy");
			AssertEquals("Dummy", identifier.ToString());
		}

		public void TestName()
		{
			RegistrationIdentifier identifier = new RegistrationIdentifier("Dummy");
			AssertEquals("Dummy", identifier.Name);
		}

		public void TestEqualsOnSameObject()
		{
			ModuleIdentifier moduleID1 = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Name");
			ModuleIdentifier moduleID2 = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Name");
			AssertEquals(true, moduleID1.Equals(moduleID2));
		}

		public void TestComparingDifferentTypes()
		{
			ModuleIdentifier moduleID = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Name");
			ControllerID controllerID = new ControllerID("Name");
			AssertEquals("moduleID == controllerID", false, moduleID.Equals(controllerID));
		}

		public void TestEqualsWithBool()
		{
			ModuleIdentifier moduleID = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Name");
			AssertEquals(false, moduleID.Equals(true));
		}
	}
}
