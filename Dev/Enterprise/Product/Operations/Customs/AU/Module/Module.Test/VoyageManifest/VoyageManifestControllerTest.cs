using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(VoyageManifestController))]
	sealed class VoyageManifestControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.VoyageManifest;

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var tranHead = Factory.New<CusSeaManTranHead>();
			tranHead.BT_VoyageNum = "124";
			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = "TestVesselName";
			vessel.RV_LloydsNumber = "12345";
			tranHead.BT_VesselName = vessel.RV_Code;
			return tranHead;
		}
	}
}
