using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SupportingDocumentAeoValidationTest : BaseSupportingDocumentAeoValidationTest
{
	public void TestCheckAEOCertificate_AgainstSupplier()
	{
		var supplier = Factory.New<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		AssertAeoCertificateValidation(invoice.SupportingDocuments, supplier, "Declaration Supplier", UniversalReferenceConstants.SupportingDocumentTypes.Y022);
	}

	public void TestCheckAEOCertificate_AgainstImporter()
	{
		var importer = Factory.New<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		AssertAeoCertificateValidation(invoice.SupportingDocuments, importer, "Declaration Importer", UniversalReferenceConstants.SupportingDocumentTypes.Y023);
	}

	public void TestCheckAEOCertificate_AgainstDeclarant()
	{
		var declarant = Factory.New<OrgHeader>();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.RemoveAndDeleteAll();

		AssertAeoCertificateValidation(invoice.SupportingDocuments, declarant, "Declaration Declarant", UniversalReferenceConstants.SupportingDocumentTypes.Y024);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
