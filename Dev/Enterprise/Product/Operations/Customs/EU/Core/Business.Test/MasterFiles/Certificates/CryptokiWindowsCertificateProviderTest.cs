namespace Enterprise.Customs.EU.Business.Testing
{
	class CryptokiWindowsCertificateProviderTest : CryptokiCertificateProviderTest
	{
		public override void TestGetCertificateList()
		{
			AssertNotNull(
				"GetWindowsCertificateList certificate",
				cryptokiCertificateProvider.GetCertificateList("WINDOWS"));
		}

		public override void TestReadCertificate()
		{
			AssertNull(
				"ReadWindowsCertificate with non-existent certificate",
				cryptokiCertificateProvider.ReadCertificate("WINDOWS", System.Array.Empty<byte>()));
		}

		protected override string CerificateSourceToTest => "Windows";
	}
}
