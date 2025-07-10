using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoStandAloneController))]
	sealed class SeaCargoStandAloneControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			AssertEquals(Env.Security.AUCustomsSCAImportModify, new SeaCargoStandAloneController().CheckPointForDeleteExposedForTest);
			AssertEquals(Env.Security.AUCustomsSCAImportModify, new SeaCargoStandAloneController().CheckPointForEditExposedForTest);
			AssertEquals(Env.Security.AUCustomsSCAImportModify, new SeaCargoStandAloneController().CheckPointForNewExposedForTest);
			AssertEquals(Env.Security.AUCustomsSCAImportModify, new SeaCargoStandAloneController().CheckPointForViewExposedForTest);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new SeaCargoStandAloneController();
			AssertEquals("Should return a CusSCAOceanBill type", typeof(CusSCAOceanBill), controller.TypeOfTopLevelBusinessObject);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.SeaCargoStandAloneController;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			Factory.Save();
			return oceanBill;
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}
	}
}
