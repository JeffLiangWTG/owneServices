using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SumARegisterReadOnlyController))]
	public class SumARegisterReadOnlyControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.DE.SumARegisterReadOnly;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
