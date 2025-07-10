using System;
using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(RNSMFTallyPlugInController))]
	sealed class RNSMFTallyPlugInControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(RNSMFTallyPlugInController);

		protected override string CountryCode => Constants.CountryCodes.Canada;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.RNSMFTallyPlugIn;
	}
}
