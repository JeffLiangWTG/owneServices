using CargoWise.IO;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DigitalSignatureRegistry))]
	sealed class DigitalSignatureRegistryTest : RegistryBusinessObjectTemplateTestCase<DigitalSignatureRegistry>
	{
		protected override DigitalSignatureRegistry GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		DigitalSignatureRegistry signature;

		protected override DigitalSignatureRegistry GetBusinessObjectToSerialise()
		{
			signature = new DigitalSignatureRegistry();
			var resourceRetriever = new EmbeddedResourceRetriever();
			signature.DigitalSignature = resourceRetriever.GetBytes("Certificate_Valid.pfx");

			var result = (DigitalSignatureRegistry)GetNewBusinessObject();
			result.CertificateFileName = "Certificate_Valid.pfx";
			result.SignatureDetailsEmail = "Sango@Sango.com";
			result.SignatureDetailsLocation = "Sango";
			result.SignatureDetailsName = "Sango";
			return result;
		}

		protected override void TearDown()
		{
			signature?.Delete();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		public void TestRunPreSaveValidation()
		{
			var bizObj = new DigitalSignatureRegistry();

			bizObj.RunPreSaveValidation();
			AssertHasErrors(bizObj.SignatureDetailsNameInfo);

			bizObj.SignatureDetailsEmail = "SangoSango.com";
			bizObj.SignatureDetailsName = "Sango";

			bizObj.RunPreSaveValidation();
			AssertHasErrors(bizObj.SignatureDetailsEmailInfo);
			AssertNoErrors(bizObj.SignatureDetailsNameInfo);

			bizObj.SignatureDetailsEmail = "Sango@Sango.com";
			bizObj.SignatureDetailsName = "Sango";
			bizObj.RunPreSaveValidation();
			AssertNoErrors(bizObj.SignatureDetailsEmailInfo);
			AssertNoErrors(bizObj.SignatureDetailsNameInfo);

			bizObj.SignatureDetailsEmail = "Sango@Sango.com";
			bizObj.SignatureDetailsName = "Sango.";
			bizObj.RunPreSaveValidation();
			AssertNoErrors(bizObj.SignatureDetailsEmailInfo);
			AssertHasErrors(bizObj.SignatureDetailsNameInfo);
		}
	}
}
