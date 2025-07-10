using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(SupportingDocumentValidation))]
sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Code()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentCodes(Factory);

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(SupportingDocument.CSI_CodeInfo, new ZString[] { RefCusCodeTestHelper.ValidImportSupportingDocumentCode, RefCusCodeTestHelper.InvalidSupportingDocumentCode }, new ZString[] { RefCusCodeTestHelper.ValidExportSupportingDocumentCode });

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(SupportingDocument.CSI_CodeInfo, new ZString[] { RefCusCodeTestHelper.ValidExportSupportingDocumentCode, RefCusCodeTestHelper.InvalidSupportingDocumentCode }, new ZString[] { RefCusCodeTestHelper.ValidImportSupportingDocumentCode });
	}

	public void TestCheckCSI_Code_R290_Header()
	{
		var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		invoiceLine2.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryNotInDevelopingCountriesTradeGroup;

		var invoiceLine3 = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
		invoiceLine3.JI_CountryOfOrigin = RefCusTradeGroupTestHelper.CountryInDevelopingCountriesTradeGroup;

		var supportingDocument = InvoiceHeader.SupportingDocuments.AddNew();
		TestCSI_Code_R290(supportingDocument);
	}

	public void TestCheckCSI_Code_R290_Line()
	{
		TestCSI_Code_R290(SupportingDocument);
	}

	void TestCSI_Code_R290(SupportingDocument supportingDocument)
	{
		RefCusTradeGroupTestHelper.CreateTradeGroups(Factory);
		RefCusCodeTestHelper.CreateSupportingDocumentsList(Factory);

		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

		var certificateCodes = RefCusCodeListLoader.GetGSPCertificateCodes(Factory, ZDateTime.Today).CodesAsString;
		var message = ValidationMessages.Plausi.GetMessageR290_2(certificateCodes);

		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(SupportingDocument.CSI_ReferenceNumberInfo);
		});
	}

	public void TestCheckCSI_DateOfIssueRule227a()
	{
		RefCusCodeTestHelper.CreateOriginDocumentCodes(Factory);

		var supportingDocumentInvoiceLine = InvoiceLine.SupportingDocuments.AddNew();
		supportingDocumentInvoiceLine.CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.SupportingDocument;
		var supportingDocumentInvoiceHeader = InvoiceHeader.SupportingDocuments.AddNew();
		supportingDocumentInvoiceHeader.CSI_Type = Common.CH.CusSupportingInfoTypeList.Codes.SupportingDocument;

		CombineAssertions(() =>
		{
			foreach (var originDocumentCode in SupportingDocument.Lookups.ValidOriginDocumentCodes.GetAllCodes())
			{
				Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

				supportingDocumentInvoiceHeader.CSI_Code = originDocumentCode;
				supportingDocumentInvoiceHeader.CSI_DateOfIssue = ZDate.Empty;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supportingDocumentInvoiceHeader.CSI_DateOfIssueInfo);

				supportingDocumentInvoiceHeader.CSI_DateOfIssue = ZDate.Today;
				AssertNoMessageErrorContaining("Supporting Document Invoice Header: DateOfIssue entered", supportingDocumentInvoiceHeader.CSI_DateOfIssueInfo, "You have not entered");

				supportingDocumentInvoiceLine.CSI_Code = originDocumentCode;
				supportingDocumentInvoiceLine.CSI_DateOfIssue = ZDate.Empty;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supportingDocumentInvoiceLine.CSI_DateOfIssueInfo);

				Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

				supportingDocumentInvoiceLine.CSI_DateOfIssue = ZDate.Empty;
				AssertNoMessageErrorContaining("Supporting Document Invoice Header: DateOfIssue entered", supportingDocumentInvoiceLine.CSI_DateOfIssueInfo, "You have not entered");
			}
		});
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= InvoiceHeader.JobComInvoiceLines.AddNew();
	JobComInvoiceLine invoiceLine;

	SupportingDocument SupportingDocument => supportingDocument ??= InvoiceLine.SupportingDocuments.AddNew();
	SupportingDocument supportingDocument;
}
