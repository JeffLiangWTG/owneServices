using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(HouseBilleManifestController))]
	sealed class HouseBilleManifestControllerTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(HouseBilleManifestController);

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CA.CAHouseBilleManifest;

		protected override Type GetBusinessObjectType() => typeof(CusCAeMHMaster);
	}
}
