using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(RNSConsolPlugInController))]
	sealed class RNSConsolPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(RNSConsolPlugInController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.RNSConsolPlugIn;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
