using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CAHTSTariffBulkChangeController))]
	sealed class CATariffBulkChangeControllerTest : ZSingletonControllerBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.HTSTariffBulkChange;
	}
}
