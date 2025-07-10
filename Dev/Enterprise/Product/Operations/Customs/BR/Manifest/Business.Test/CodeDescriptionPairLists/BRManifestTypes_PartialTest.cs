using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	public class BRManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var manifestTypes = new BRManifestTypes().All;
			AssertEquals(2, manifestTypes.Count);

			var mer = manifestTypes.FirstOrDefault(x => x.Code == BRManifestTypes.Codes.MER);
			AssertEquals(true, mer.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, mer.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(false, mer.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
			AssertEquals(true, mer.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));

			var mucr = manifestTypes.FirstOrDefault(x => x.Code == BRManifestTypes.Codes.MUCR);
			AssertEquals(true, mucr.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, mucr.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(true, mucr.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
			AssertEquals(true, mucr.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));
		}
	}
}
