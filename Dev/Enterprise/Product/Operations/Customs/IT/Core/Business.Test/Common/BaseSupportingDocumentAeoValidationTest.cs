using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class BaseSupportingDocumentAeoValidationTest : BusinessObjectValidationTestCase
{
	protected void AssertAeoCertificateValidation(ISupportingDocumentCollection<SupportingDocument> supportingDocumentCollection, OrgHeader organisation, ZString organisationName, ZString aeoCertificateCode)
	{
		var missingMessage = $"This document references the AEO number of the {organisationName}, but its Organization has no AEO code. Please consider adding the AEO in Organization > Config > Registration Numbers/Codes.";
		var unmatchedMessage = $"This document Reference does not match the AEO Registration Number of the {organisationName}.";

		var supportingDocument = supportingDocumentCollection.AddNew();

		supportingDocument.CSI_Code = "XXX";
		supportingDocument.CSI_ReferenceNumber = "WRONG AEO";
		AssertNoWarnings("No warning expected when Code is not an AEO Certificate codes", supportingDocument.CSI_ReferenceNumberInfo);

		supportingDocument.CSI_Code = aeoCertificateCode;
		supportingDocument.Validation.ValidateCSI_ReferenceNumber();
		AssertHasWarning($"Warning expected when {organisationName} has no AEO Code but Document is an AEO Certificate", supportingDocument.CSI_ReferenceNumberInfo, missingMessage);

		organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "TRUE AEO");
		supportingDocument.Validation.ValidateCSI_ReferenceNumber();
		AssertHasWarning($"Warning expected when {organisationName} has a different AEO Reference from Document", supportingDocument.CSI_ReferenceNumberInfo, unmatchedMessage);

		supportingDocument.CSI_ReferenceNumber = "TRUE AEO";
		AssertNoWarnings("No warning expected", supportingDocument.CSI_ReferenceNumberInfo);
	}
}
