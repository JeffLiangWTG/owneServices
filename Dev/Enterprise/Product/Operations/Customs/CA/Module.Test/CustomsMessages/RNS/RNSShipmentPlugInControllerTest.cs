using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(RNSShipmentPlugInController))]
	sealed class RNSShipmentPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(RNSShipmentPlugInController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.RNSShipmentPlugIn;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
