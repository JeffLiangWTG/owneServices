using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Intrastat.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Intrastat.Module.Testing
{
	[TestedType(typeof(IntrastatTransactionsController))]
	sealed class IntrastatTransactionsControllerTest : ZControllerBasherTest
	{
		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(CusIntrastatHeader), Controller.TypeOfTopLevelBusinessObject);
		}

		public override Type ControllerToBashType => typeof(IntrastatTransactionsController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.IntrastatTransactionsController;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = EU.Intrastat.Business.Testing.IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData<CusIntrastatHeader>();
			Factory.Save();
			return header;
		}
	}
}
