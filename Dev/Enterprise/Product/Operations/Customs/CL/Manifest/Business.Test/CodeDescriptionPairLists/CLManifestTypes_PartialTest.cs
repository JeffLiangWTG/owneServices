using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Customs.CL.Manifest.Business.CLCustomsDataRegistry;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class CLManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var clmanifestTypes = new CLManifestTypes().All;
			AssertEquals(1, clmanifestTypes.Count);

			var man = clmanifestTypes.FirstOrDefault(x => x.Code == CLManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));

			Instance.CLManifestShowAIRFunctions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			clmanifestTypes = new CLManifestTypes().All;
			man = clmanifestTypes.FirstOrDefault(x => x.Code == CLManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
		}
	}
}
