using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoOutturnBillsController))]
	sealed class SeaCargoOutturnBillsControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			var controller = new SeaCargoOutturnBillsController();
			AssertEquals(ModuleIDs.Customs.AU.SeaCargoOutturnBills, controller.ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new SeaCargoOutturnBillsController();
			AssertEquals("Should return a CusOutturn type", typeof(CusOutturnHeader), controller.TypeOfTopLevelBusinessObject);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.SeaCargoOutturnBillsController;

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}
	}
}
