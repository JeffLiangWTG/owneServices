using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	public class EMCSJobDeclarationValidationTest : EU.EMCS.Business.Testing.EMCSJobDeclarationValidationTest
	{
		public void TestCheckJE_CustomsProfile_Mandatory()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_CustomsProfile = "";
			AssertHasError(declaration.JE_CustomsProfileInfo, "Please enter a Certificate Identifier.");

			declaration.JE_CustomsProfile = "Kerry Woollen Mill";
			AssertNoError(declaration.JE_CustomsProfileInfo, "Please enter a Certificate Identifier.");
		}

		public void TestCertificateIdentifierListValidation()
		{
			declaration.JE_CustomsProfile = "Kerry Woollen Mill";
			AssertListValidationInvalidCodeError(declaration.JE_CustomsProfileInfo, true);

			declaration.JE_CustomsProfile = certificateIdentifier1.GP_MailBoxID;
			AssertListValidationInvalidCodeError(declaration.JE_CustomsProfileInfo, false);
		}

		public void TestValidCertificateIdentifierChosen()
		{
			declaration.JE_CustomsProfile = certificateIdentifier1.GP_MailBoxID;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertHasWarning("Certificate Identifier is not valid", declaration.JE_CustomsProfileInfo, "Certificate status does not appear to be valid.");

			declaration.JE_CustomsProfile = certificateIdentifier2.GP_MailBoxID;
			declaration.Validation.ValidateJE_CustomsProfile();
			AssertNoWarning("Certificate Identifier is valid", declaration.JE_CustomsProfileInfo, "Certificate status does not appear to be valid.");
		}

		public new void TestCheckJE_MessageSubType()
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDelivery);

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertHasMessageError(declaration.JE_MessageSubTypeInfo, "Customs Office (Office of Delivery) should only be entered when Destination Type = 6 - Destination - Export.");

			declaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			declaration.Validation.ValidateJE_MessageSubType();
			AssertNoMessageError(declaration.JE_MessageSubTypeInfo, "Customs Office (Office of Delivery) should only be entered when Destination Type = 6 - Destination - Export.");
		}

		protected override void SetUp()
		{
			certificateIdentifier1 = Factory.New<EMCSGlbCompanyCredential>();
			certificateIdentifier1.GP_MailBoxID = "DUBLIN WAREHOUSE 1";
			certificateIdentifier1.GP_CertificateSerialNumber = "A0492387J";
			certificateIdentifier1.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			certificateIdentifier1.GP_GC = GlbCompany.CurrentCompany.PK;

			certificateIdentifier2 = Factory.New<EMCSGlbCompanyCredential>();
			certificateIdentifier2.GP_MailBoxID = "KILKENNY WAREHOUSE";
			certificateIdentifier2.GP_CertificateSerialNumber = "754927U38TZ";
			certificateIdentifier2.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			certificateIdentifier2.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;

		EMCSGlbCompanyCredential certificateIdentifier1;
		EMCSGlbCompanyCredential certificateIdentifier2;
	}
}
