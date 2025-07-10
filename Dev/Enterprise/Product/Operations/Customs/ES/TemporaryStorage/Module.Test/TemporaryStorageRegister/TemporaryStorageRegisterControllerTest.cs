using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageRegisterController))]
	public class TemporaryStorageRegisterControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.ES.TemporaryStorageRegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
