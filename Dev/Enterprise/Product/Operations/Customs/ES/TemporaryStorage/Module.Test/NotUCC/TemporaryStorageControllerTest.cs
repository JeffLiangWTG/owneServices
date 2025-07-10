using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageController))]
	public class TemporaryStorageControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(TemporaryStorageController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
