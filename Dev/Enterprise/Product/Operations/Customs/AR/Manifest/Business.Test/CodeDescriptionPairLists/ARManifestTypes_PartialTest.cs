using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Customs.AR.Manifest.Business.ARCustomsDataRegistry;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class ARManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var manifestTypes = new ARManifestTypes().All;
			AssertEquals(1, manifestTypes.Count);

			var man = manifestTypes.FirstOrDefault(x => x.Code == ARManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));

			Instance.ARManifestShowAIRFunctions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			manifestTypes = new ARManifestTypes().All;
			man = manifestTypes.FirstOrDefault(x => x.Code == ARManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
		}
	}
}
