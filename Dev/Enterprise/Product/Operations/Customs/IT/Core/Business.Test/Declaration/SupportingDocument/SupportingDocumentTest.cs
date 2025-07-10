using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocument))]
sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
{
	public void TestCSI_YearOfIssue()
	{
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "ASD";
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "1";
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "11";
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "111";
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "1111";
		AssertEquals(new ZDateTime(1111, 1, 1), supportingDocument.CSI_DateOfIssue);
		AssertEquals("1111", supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "11111";
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_YearOfIssue = "2010";
		AssertEquals(new ZDateTime(2010, 1, 1), supportingDocument.CSI_DateOfIssue);
		AssertEquals("2010", supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_DateOfIssue = new ZDateTime(2017, 11, 14, 2, 3, 4);
		AssertEquals(new ZDateTime(2017, 1, 1), supportingDocument.CSI_DateOfIssue);
		AssertEquals("2017", supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_DateOfIssue = ZDateTime.Invalid;
		AssertEquals(ZDateTime.Invalid, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertEquals(ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
		AssertEquals(ZString.Empty, supportingDocument.CSI_YearOfIssue);
	}

	public void TestUnitOfQuantityFieldType()
	{
		AssertEquals(nameof(FieldType.TextDropEdit), supportingDocument.UnitOfQuantityFieldType);
	}

	public void TestIsLine()
	{
		Assert("IsLine should be always true", supportingDocument.IsLine);
	}

	public void TestCSI_ReferenceNumber2MaxLength()
	{
		AssertEquals("CSI_ReferenceNumber2 MaxLength", 70, supportingDocument.CSI_ReferenceNumber2Info.MaxLength);
	}

	public void TestCSI_ReferenceNumberMaxLength()
	{
		AssertEquals("CSI_ReferenceNumber MaxLength", 100, supportingDocument.CSI_ReferenceNumberInfo.MaxLength);
	}

	public void TestIsLineOnly()
	{
		Assert("IsLineOnly should be always true", supportingDocument.IsLineOnly);
	}

	[ExpectNoExceptions]
	public void TestSetCSI_CodeTriggersOtherFieldsValidation()
	{
		var mockSupportingDocument = Factory.NewMoq<SupportingDocument>();
		var mockSupportingDocumentValidation = new Mock<SupportingDocumentValidation>(mockSupportingDocument.Object);
		mockSupportingDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockSupportingDocumentValidation.Object);
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_RN_NKCountryCode");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_ReferenceNumber");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_DateOfIssue");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_Quantity");
		mockSupportingDocumentValidation.Protected().Setup("CheckCSI_UnitOfQuantity");

		mockSupportingDocument.Object.CSI_Code = "XXX";
		mockSupportingDocumentValidation.VerifyAll();
	}

	public void TestEntriesLinkProvider()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		AssertNotNull(supportingDocument.EntriesLinkProvider);
	}

	public void TestEntriesLinkProviderResetOnChangeParent()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		var entriesLinkProvider1 = supportingDocument.EntriesLinkProvider;
		Assert("No reset", ReferenceEquals(entriesLinkProvider1, supportingDocument.EntriesLinkProvider));

		supportingDocument.CSI_ParentID = ZGuid.NewZGuid();
		var entriesLinkProvider2 = supportingDocument.EntriesLinkProvider;
		Assert("CSI_ParentID changed", !ReferenceEquals(entriesLinkProvider1, entriesLinkProvider2));

		supportingDocument.CSI_ParentTableCode = "XX";
		Assert("CSI_ParentTableCode changed", !ReferenceEquals(entriesLinkProvider2, supportingDocument.EntriesLinkProvider));
	}

	public void TestAsISupportingDocument()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		supportingDocument.CSI_Code = "A";
		supportingDocument.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		supportingDocument.CSI_YearOfIssue = "2021";
		supportingDocument.CSI_ReferenceNumber = "1";
		supportingDocument.CSI_Quantity = 1m;
		supportingDocument.CSI_UnitOfQuantity = "X";
		supportingDocument.CSI_Status = SADConstants.CertificateFlag.DER;
		CombineAssertions(() =>
		{
			var asInterface = (ISupportingDocument)supportingDocument;
			AssertEquals(nameof(asInterface.CountryOfIssue), Core.Constants.CountryCodes.Italy, asInterface.CountryOfIssue);
			AssertEquals(nameof(asInterface.Quantity), 1m, asInterface.Quantity);
			AssertEquals(nameof(asInterface.ReferenceNumber), "1", asInterface.ReferenceNumber);
			AssertEquals(nameof(asInterface.Status), SADConstants.CertificateFlag.DER, asInterface.Status);
			AssertEquals(nameof(asInterface.Type), "A", asInterface.Type);
			AssertEquals(nameof(asInterface.UnitOfQuantity), "X", asInterface.UnitOfQuantity);
			AssertEquals(nameof(asInterface.YearOfIssue), "2021", asInterface.YearOfIssue);
		});
	}

	public void TestCSI_LineNoMaxLength()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		AssertEquals("Max length", 4, supportingDocument.CSI_LineNoInfo.MaxLength);
	}

	public void TestCSI_LineNoResourceStringData()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_LineNoInfo);
		AssertNotNull("ResourceStringData", resourceStringData);
		CombineAssertions(() =>
		{
			AssertEquals("ShortCaption", "Item No.", resourceStringData.ShortCaption);
			AssertEquals("MediumCaption", "Line Item No.", resourceStringData.MediumCaption);
			AssertEquals("Caption", "Line Item Number", resourceStringData.Caption);
			AssertEquals("FullDescription", "Document Line Item Number", resourceStringData.FullDescription);
		});
	}

	public void TestCSI_ValueResourceStringData()
	{
		CombineAssertions(() =>
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ValueInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption of CSI_Value", "Value", resourceStringData.Caption);
			AssertEquals("Full Caption of CSI_Value", "[12 03 014 000] Value", resourceStringData.FullDescription);
		});
	}

	public void TestCSI_DateOfExpiryResourceStringData()
	{
		CombineAssertions(() =>
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_DateOfExpiryInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("Caption of CSI_DateOfExpiry", "Date of Expiry", resourceStringData.Caption);
			AssertEquals("Full Caption of CSI_DateOfExpiry", "[12 03 011 000] Date of Expiry", resourceStringData.FullDescription);
		});
	}

	public void TestDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		AssertSame(declaration, declarationSupportingDocument.Declaration);
		AssertSame(declaration, invoiceSupportingDocument.Declaration);
		AssertSame(declaration, invoiceLineSupportingDocument.Declaration);

		var orphanSupportingDocument = Factory.New<SupportingDocument>();
		AssertNull("The document doesn't have a parent declaration", orphanSupportingDocument.Declaration);
	}

	public void TestGetNewValidation()
	{
		CombineAssertions(() =>
		{
			var supportingDocumentAtDeclarationLevel = declaration.SupportingDocuments.AddNew();
			AssertType<SupportingDocumentValidation>("Supporting Document Validation at Declaration Level", supportingDocumentAtDeclarationLevel.Validation);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var supportingDocumentAtEntryInstructionLevel = entryInstruction.SupportingDocuments.AddNew();
			AssertType<SupportingDocumentValidation>("Supporting Document Validation at Entry Instruction Level - IMP", supportingDocumentAtEntryInstructionLevel.Validation);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var supportingDocumentAtEntryInstructionLevel2 = entryInstruction.SupportingDocuments.AddNew();
			AssertType<EntryInstructionExportSupportingDocumentValidation>("Supporting Document Validation at Entry Instruction Level - EXP", supportingDocumentAtEntryInstructionLevel2.Validation);

			var supportingDocumentAtInvoiceHeaderLevel = declaration.Invoices.AddNew().SupportingDocuments.AddNew();
			AssertType<SupportingDocumentValidation>("Supporting Document Validation at Invoice Header Level", supportingDocumentAtInvoiceHeaderLevel.Validation);
		});
	}

	#region Implementation

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		yield return declaration.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		yield return invoice.SupportingDocuments.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.SupportingDocuments.AddNew();
		var product = factory.New<OrgSupplierPart>();
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
		yield return pivot.SupportingDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
	}

	SupportingDocument supportingDocument;
	JobComInvoiceHeader invoiceHeader;
	JobDeclaration declaration;

	#endregion
}
