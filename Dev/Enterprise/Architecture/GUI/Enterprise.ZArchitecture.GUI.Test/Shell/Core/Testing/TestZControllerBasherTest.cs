using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestedType(typeof(DummyController))]
	sealed class TestZControllerBasherTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bO = Factory.New(typeof(DummyBusinessObject));
			Factory.Save();
			return bO;
		}

		protected override ControllerID GetControllerID()
		{
			return DummyControllerIDs.Dummy;
		}
	}
}
