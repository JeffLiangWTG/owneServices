using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
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

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			AssertEquals(true, controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.TempStorageRegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Latvia;

		protected override void SetUp()
		{
			base.SetUp();
			controller = (TempStorageRegisterController)Controller;
		}
		TempStorageRegisterController controller;
	}
}
