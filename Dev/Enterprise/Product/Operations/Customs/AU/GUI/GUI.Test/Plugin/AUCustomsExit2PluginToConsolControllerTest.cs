using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.PlugIn.Testing
{
	[TestedType(typeof(AUCustomsExit2PluginToConsolController))]
	sealed class AUCustomsExit2PluginToConsolControllerTest : ZControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CargoManifestPlugInForConsol;
	}
}
