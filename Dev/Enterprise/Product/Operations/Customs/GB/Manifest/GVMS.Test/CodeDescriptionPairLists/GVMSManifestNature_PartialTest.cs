using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GVMSManifestNature_PartialTest : TestCaseWithFactory
	{
		public void TestNatureFullNameMapping()
		{
			GVMSManifestNature.NatureFullNameMapping.TryGetValue(GVMSManifestNature.Codes.Import, out var result1);
			AssertEquals("UK_INBOUND", result1);

			GVMSManifestNature.NatureFullNameMapping.TryGetValue(GVMSManifestNature.Codes.Export, out var result2);
			AssertEquals("UK_OUTBOUND", result2);

			GVMSManifestNature.NatureFullNameMapping.TryGetValue(GVMSManifestNature.Codes.GBtoNI, out var result3);
			AssertEquals("GB_TO_NI", result3);

			GVMSManifestNature.NatureFullNameMapping.TryGetValue(GVMSManifestNature.Codes.NItoGB, out var result4);
			AssertEquals("NI_TO_GB", result4);
		}
	}
}
