using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestDate(2022, 03, 16)]
[TestedType(typeof(GlbCertificatePasswordLookups))]
sealed class GlbCertificatePasswordLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCertificateAuthorities()
	{
		CombineAssertions(() =>
		{
			RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
			var certificateAuthorities = lookups.CertificateAuthorities;
			AssertEquals("Count", 1, certificateAuthorities.Count);
			AssertSame("Cached", certificateAuthorities, lookups.CertificateAuthorities);
			AssertEquals("CodesAsString", "eMudhra", certificateAuthorities.CodesAsString);
		});
	}

	public void TestChipsets()
	{
		CombineAssertions(() =>
		{
			RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
			var chipsets = lookups.Chipsets;
			AssertEquals("Count", 1, chipsets.Count);
			AssertSame("Cached", chipsets, lookups.Chipsets);
			AssertEquals("Code", "WatchData", chipsets[0].Code);
			AssertEquals("Description", "TRUSTKEYP11_ND_v34.dll", chipsets[0].Description);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var password = Factory.New<GlbCertificatePassword>();
		lookups = password.Lookups;
	}

	GlbCertificatePasswordLookups lookups;
}
