using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOImportController))]
	sealed class AirCTOImportControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCTOImport;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var flightMaster = Factory.New<CTOCusMAWB>();
			var masterBill = flightMaster.ChildBills.AddNew();
			Factory.Save();
			return masterBill;
		}
	}
}
