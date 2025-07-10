using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumARegisterController))]
	public class SumARegisterControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DE.SumARegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
