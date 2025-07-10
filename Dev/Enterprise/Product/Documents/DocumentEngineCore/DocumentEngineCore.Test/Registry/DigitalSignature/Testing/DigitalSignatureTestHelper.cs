using CargoWise.IO;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	public static class DigitalSignatureTestHelper
	{
		const string Certificate_Valid_Filename = "Certificate_Valid.pfx";
		const string Certificate_ValidWithPassword_FileName = "Certificate_ValidWithPassword (the password is p455w0rd).pfx";
		const string Certificate_ValidWithPassword_Password = "p455w0rd";

		public static DigitalSignatureRegistry GetDigitalSignatureRegistry()
		{
			return GetDigitalSignatureRegistry(Certificate_Valid_Filename, string.Empty);
		}

		public static DigitalSignatureRegistry GetDigitalSignatureRegistry_WithPassword()
		{
			return GetDigitalSignatureRegistry(Certificate_ValidWithPassword_FileName, Certificate_ValidWithPassword_Password);
		}

		static DigitalSignatureRegistry GetDigitalSignatureRegistry(string certificateFilename, string password)
		{
			var digitalSignatureRegistry = new DigitalSignatureRegistry();
			var resourceRetriever = new EmbeddedResourceRetriever();
			digitalSignatureRegistry.DigitalSignature = resourceRetriever.GetBytes(certificateFilename);
			digitalSignatureRegistry.CertificatePassword = password;
			digitalSignatureRegistry.SignatureDetailsEmail = "Sango@Sango.com";
			digitalSignatureRegistry.SignatureDetailsLocation = "Sango";
			digitalSignatureRegistry.SignatureDetailsName = "Sango";

			return digitalSignatureRegistry;
		}
	}
}
