using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TempStoragePremisesController))]
	public class TempStoragePremisesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.TempStoragePremises;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
