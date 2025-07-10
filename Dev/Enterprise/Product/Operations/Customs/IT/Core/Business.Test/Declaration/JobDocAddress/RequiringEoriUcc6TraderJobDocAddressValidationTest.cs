using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class RequiringEoriUcc6TraderJobDocAddressValidationTest : TraderJobDocAddressValidationTest
{
	public void TestCheckOrganisationPK_MandatoryCodeEORValidation_EUOrganization()
	{
		var expectedMessage = "EORI code for Importer is mandatory: please press F3, go to details -> config and fill a 'Registration Number / code' with Type ='EOR'";
		var declaration = Factory.New<JobDeclaration>();
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.MainAddress.OA_RN_NKCountryCode = "IT";

		AssertEquals("Pre: for MessageType default(EXP), IsUCC6", false, declaration.Configuration.IsUCC6(declaration));

		declaration.JE_MessageType = "IMP";
		AssertEquals("Pre: for MessageType = IMP, IsUCC6", true, declaration.Configuration.IsUCC6(declaration));

		importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		declaration.JE_OH_Importer = importer.PK;
		AssertHasMessageErrorContaining("When no Customs Code added for NaturalPersonIndividual", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessage);

		importer.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
		declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining("When Customs Code added for NaturalPersonIndividual", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessage);

		importer.CustomsCodes.RemoveAndDeleteAll();

		importer.OH_Category = OrgConstants.Category.Business;
		declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
		AssertHasMessageErrorContaining("When no Customs Code added for Business", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessage);

		importer.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
		declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining("When EORI Customs Code added for Business", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessage);

		importer.CustomsCodes.RemoveAndDeleteAll();
		importer.CustomsCodes.AddNew("TCU", "TCU CODE", "US");
		declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
		AssertNoMessageErrorContaining("When TCU Customs Code added for Business", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, expectedMessage);
	}

	public void TestCheckOrganisationPK_MandatoryCodeEORValidation_NotEUOrganization()
	{
		var expectedMessage = "Exporter has no EORI or TCU code. Please consider adding the 'EOR' or 'TCU' code in Organization > Config > Registration Numbers/Codes";
		var declaration = Factory.New<JobDeclaration>();
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.MainAddress.OA_RN_NKCountryCode = "US";

			exporter.OH_Category = OrgConstants.Category.Business;
			declaration.ExporterDocAddress.OrganisationPK = exporter.PK;

			declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("When no Customs Code added", declaration.ExporterDocAddress.OrganisationPKInfo, expectedMessage);

			exporter.CustomsCodes.AddNew("TCU", "TCU CODE", "US");
			declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("With TCU Code", declaration.ExporterDocAddress.OrganisationPKInfo, expectedMessage);

			exporter.CustomsCodes.RemoveAndDeleteAll();

			exporter.CustomsCodes.AddNew("EOR", "EORI CODE", "US");
			declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("With EOR Code", declaration.ExporterDocAddress.OrganisationPKInfo, expectedMessage);
		}
	}
}
