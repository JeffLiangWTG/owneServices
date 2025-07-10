using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.PlugIn.Testing;

[TestedType(typeof(AECustomsCargoManifestController))]
class AECustomsCargoManifestControllerTest : ZControllerBasherTest
{
	protected override string CountryCode
	{
		get
		{
			return Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates;
		}
	}

	protected override ControllerID GetControllerID()
	{
		return ControllerIDs.Customs.CargoManifestPlugInForConsol;
	}
}
