using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	sealed class COManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var manifestTypes = new COManifestTypes().All;
			AssertEquals(1, manifestTypes.Count);

			var man = manifestTypes.FirstOrDefault(x => x.Code == COManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));
		}
	}
}
