using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(SumARegisterReadOnlyController))]
sealed class SumARegisterReadOnlyControllerTest : ZControllerBasherTest
{
	protected override ControllerID GetControllerID() => ControllerIDs.Customs.SumARegisterReadOnly;

	protected override string CountryCode => string.Empty;
}
