using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoHouseController))]
	sealed class SeaCargoHouseControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			var testController = new SeaCargoHouseController();
			AssertEquals(typeof(CusSCAHouse), testController.TypeOfTopLevelBusinessObject);
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.SeaCargoHouseController;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var houseBill = Factory.NewWithValidTestData<CusSCAHouse>();
			houseBill.CA_CB = oceanBill.PK;
			Factory.Save();
			return houseBill;
		}

		IDisposable enableSeaCargoHouseFunc;
		protected override void SetUp()
		{
			base.SetUp();
			enableSeaCargoHouseFunc = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.SeaCargoHouse, Enterprise.Core.Constants.CountryCodes.Australia, ZDateTime.Now, true);
		}

		protected override void TearDown()
		{
			enableSeaCargoHouseFunc?.Dispose();
			base.TearDown();
		}
	}
}
