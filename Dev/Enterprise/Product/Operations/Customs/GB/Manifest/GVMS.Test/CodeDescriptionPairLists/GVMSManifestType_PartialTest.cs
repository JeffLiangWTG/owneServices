using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GVMSManifestType_PartialTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var gvmsManifestTypes = new GVMSManifestType().All;
			AssertEquals(1, gvmsManifestTypes.Count);

			var gvms = gvmsManifestTypes.FirstOrDefault(x => x.Code == GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms);
			AssertEquals(true, gvms.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.ShippingLine));
			AssertEquals(true, gvms.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));
			AssertEquals(true, gvms.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Road));
			AssertEquals(true, gvms.ApplicableTransportModes.Contains(Core.Constants.TransportModes.RollOnRollOff));
		}
	}
}
