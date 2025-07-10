using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(MonthlyClosingController))]
	class MonthlyClosingControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DE.MonthlyClosing;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
