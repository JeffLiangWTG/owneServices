using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCargoOutturnBillsController))]
	sealed class AirCargoOutturnBillsControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new AirCargoOutturnBillsController();
			AssertEquals("Should return a CusUnderbond type", typeof(CusUnderbond), controller.TypeOfTopLevelBusinessObject);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCargoOutturnBillsController;

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}
	}
}
