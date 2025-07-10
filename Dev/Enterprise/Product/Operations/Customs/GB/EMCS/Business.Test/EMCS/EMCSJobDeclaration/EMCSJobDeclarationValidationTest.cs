using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSJobDeclarationValidationTest : EU.EMCS.Business.Testing.EMCSJobDeclarationValidationTest
	{
		public void TestCheckJE_CustomsProfile_Mandatory()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_CustomsProfile = "";
			AssertHasError(declaration.JE_CustomsProfileInfo, "Please enter a Credential.");

			declaration.JE_CustomsProfile = "Profile 1";
			AssertNoError(declaration.JE_CustomsProfileInfo, "Please enter a Credential.");
		}

		public void TestCertificateIdentifierListValidation()
		{
			declaration.JE_CustomsProfile = null;
			AssertListValidationInvalidCodeError(declaration.JE_CustomsProfileInfo, false);
		}

		public void TestValidCertificateIdentifierChosen()
		{
			declaration.JE_CustomsProfile = credential1.Code;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasWarning("Credential is not valid", declaration.JE_CustomsProfileInfo, "Credential status does not appear to be valid.");
		}

		protected override void SetUp()
		{
			credential1 = new CodeDescriptionPair("LONDON WAREHOUSE 1", "A0492387J");

			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
		CodeDescriptionPair credential1;
	}
}
