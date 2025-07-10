using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(SumARegisterController))]
sealed class SumARegisterControllerTest : ZControllerBasherTest
{
	protected override ControllerID GetControllerID() => ControllerIDs.Customs.SumARegister;

	protected override string CountryCode => string.Empty;
}
