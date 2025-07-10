using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	public sealed class GlbILStaffExternalPasswordLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCertificateAuthoritiesList()
		{
			var password = Factory.New<GlbILStaffExternalPassword>();
			var lookups = password.Lookups;
			var certificateAuthorities = lookups.CertificateAuthoritiesList;

			AssertNotNull(certificateAuthorities);
			AssertContainsExactElementsInAnyOrder("Certificate Authoritied should contain expected entries", new string[] { "COM", "PER" }, certificateAuthorities.GetAllCodes());

			var password1 = Factory.New<GlbILStaffExternalPassword>();
			var lookups1 = password1.Lookups;
			var certificateAuthorities1 = lookups1.CertificateAuthoritiesList;
			AssertSame("Both lists should be the same instance", certificateAuthorities, certificateAuthorities1);
		}
	}
}
