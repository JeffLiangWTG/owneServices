using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlController))]
	class ExitControlControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(CusExitHeader), Controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.ExitControl;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
