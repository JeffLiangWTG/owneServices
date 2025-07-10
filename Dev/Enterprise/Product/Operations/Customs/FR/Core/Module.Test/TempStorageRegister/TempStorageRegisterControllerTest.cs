using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.TempStorageRegister.Testing
{
	[TestedType(typeof(TempStorageRegisterController))]
	class TempStorageRegisterControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.EU.TempStorageRegister, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(CusTempStorageRegHeader), controller.TypeOfTopLevelBusinessObject);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.TempStorageRegister;

		protected override string CountryCode => Core.Constants.CountryCodes.France;

		protected override void SetUp()
		{
			base.SetUp();
			controller = (TempStorageRegisterController)Controller;
		}
		TempStorageRegisterController controller;
	}
}
