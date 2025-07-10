using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(RNSCFSShipmentPlugInController))]
	sealed class RNSCFSShipmentPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(RNSCFSShipmentPlugInController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
