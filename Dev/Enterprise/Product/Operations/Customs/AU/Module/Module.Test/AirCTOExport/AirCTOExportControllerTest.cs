using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOExportController))]
	sealed class AirCTOExportControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.AirCTOExport;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			Factory.Save();
			return header;
		}
	}
}
