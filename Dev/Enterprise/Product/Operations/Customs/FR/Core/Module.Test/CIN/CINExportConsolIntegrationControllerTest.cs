using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.CIN.Testing
{
	[TestedType(typeof(CINExportConsolIntegrationController))]
	class CINExportConsolIntegrationControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.France; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.FR.CINExportConsolIntegrationController;
		}
	}
}
