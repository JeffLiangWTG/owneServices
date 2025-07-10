using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Customs.MX.Manifest.Business.MXCustomsDataRegistry;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class MXManifestTypesTest : TestCaseWithFactory
	{
		public void TestTransportMode()
		{
			var mxmanifestTypes = new MXManifestTypes().All;
			AssertEquals(1, mxmanifestTypes.Count);

			var man = mxmanifestTypes.FirstOrDefault(x => x.Code == MXManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableManifestStyles.Contains(ApplicationCodeTypeList.Codes.Consolidator));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.All));
			AssertEquals(false, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Sea));

			Instance.MXManifestShowAIRFunctions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			mxmanifestTypes = new MXManifestTypes().All;
			man = mxmanifestTypes.FirstOrDefault(x => x.Code == MXManifestTypes.Codes.MAN);
			AssertEquals(true, man.ApplicableTransportModes.Contains(Core.Constants.TransportModes.Air));
		}
	}
}
