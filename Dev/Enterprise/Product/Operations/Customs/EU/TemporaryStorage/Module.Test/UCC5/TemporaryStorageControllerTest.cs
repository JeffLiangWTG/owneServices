using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageController))]
	class TemporaryStorageControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(TemporaryStorageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}
