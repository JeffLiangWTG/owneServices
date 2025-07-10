using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class IntoWarehouseInvoiceLineSupportingDocumentsManagerCustomsDecisionsTest : InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest
{
	protected override ZString ProcedureCode => "7100";
	protected override ZString ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc => "C517";
	protected override ZString AuthorisationType => "CWP";
	protected override ZString DeclarationMessageType => "IMP";
	protected override ZString[] DefaultDocumentTypes => new ZString[] { "C517", "C518", "C519" };
	protected override ZString ProcedureTypeDescription => "Into Warehouse";

	public void TestWhenAuthorisationLocRuleDoesNotMatchWithCcp()
	{
		locRule.CPR_ValueFrom = "6543A";
		Factory.Save();

		SetupRequiredFields();

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionIsEmpty();
	}

	public void TestWhenEntryInstructionIsEmpty()
	{
		SetupRequiredFields();
		invoiceLine.JI_CEI = ZGuid.Empty;

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionIsEmpty();
	}

	public void TestWhenCustomsWarehouseInEmpty()
	{
		SetupRequiredFields();
		entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionIsEmpty();
	}

	protected override void SetupRequiredFields()
	{
		base.SetupRequiredFields();

		invoiceLine.JI_CEI = entryInstruction.PK;
		entryInstruction.CEI_OA_Warehouse2 = customsWarehouseOrganization.MainAddress.PK;
	}
}

sealed class InwardProcessingInvoiceLineSupportingDocumentsManagerCustomsDecisionsTest : InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest
{
	protected override ZString ProcedureCode => "5100";
	protected override ZString ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc => "C601";
	protected override ZString AuthorisationType => "IPO";
	protected override ZString DeclarationMessageType => "IMP";
	protected override ZString[] DefaultDocumentTypes => new ZString[] { "C601" };
	protected override ZString ProcedureTypeDescription => "Inward Processing";
}

sealed class OutwardProcessingInvoiceLineSupportingDocumentsManagerCustomsDecisionsTest : InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest
{
	protected override ZString ProcedureCode => "2100";
	protected override ZString ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc => "C019";
	protected override ZString AuthorisationType => "OPO";
	protected override ZString DeclarationMessageType => "EXP";
	protected override ZString[] DefaultDocumentTypes => new ZString[] { "C019" };
	protected override ZString ProcedureTypeDescription => "Outward Processing";
}

abstract class InvoiceLineSupportingDocumentsManagerAddCustomsDecisionsTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when invoice line is null", () => new InvoiceLineSupportingDocumentsManager(null));
	}

	public void TestWhenAuthorisationHasDoc()
	{
		SetupRequiredFields();
		AddDocRuleToAuthorisation();

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionContainsOnlyOne(DocType, AuthorisationNumber);
	}

	public void TestOverwriteReference()
	{
		SetupRequiredFields();
		AddDocRuleToAuthorisation();

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		var preexistingSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		preexistingSupportingDocument.CSI_Code = DocType;
		preexistingSupportingDocument.CSI_ReferenceNumber = "PRE EXISTING NO.";

		invoiceLine.SupportingDocumentsManager.AddCustomsDecisionsSupportingDocumentIfApplicable();

		AssertSupportingDocumentCollectionContainsOnlyOne(DocType, AuthorisationNumber);

		preexistingSupportingDocument.CSI_ReferenceNumber = "PRE EXISTING NO.";
		var anotherPreexistingSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
		anotherPreexistingSupportingDocument.CSI_Code = DocType;
		anotherPreexistingSupportingDocument.CSI_ReferenceNumber = "A-PRE EXISTING NO.";

		invoiceLine.SupportingDocumentsManager.AddCustomsDecisionsSupportingDocumentIfApplicable();

		invoiceLine.SupportingDocuments.Reload(true);
		var actualSupportingDocuments = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == DocType);

		AssertEquals($"Supporting documents collection of type {DocType} should be", 2, actualSupportingDocuments.Count());
		AssertContainsExactElementsInAnyOrder("Expected Supporting documents", new ZString[] { "PRE EXISTING NO.", "AUTH. NUMBER" }, actualSupportingDocuments.Select(x => x.CSI_ReferenceNumber).ToArray());
	}

	public void TestWhenAuthorisationDoesNotHaveDoc()
	{
		SetupRequiredFields();

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionContainsOnlyOne(ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc, AuthorisationNumber);
	}

	public void TestWhenRequiredFieldsAreNotFilled()
	{
		SetupRequiredFields();
		invoiceLine.JI_Procedure = ZString.Empty;

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionIsEmpty();
	}

	public void TestWhenAuthorisationHasDifferentHolder()
	{
		SetupRequiredFields();

		FillAuthOwner(Factory.NewWithValidTestData<OrgHeader>());
		InvokeDefaultCustomsDecisionSupportingDocument();
		AssertSupportingDocumentCollectionIsEmpty();

		declaration.JE_OH_Importer = ZGuid.Empty;
		declaration.JE_OH_Supplier = ZGuid.Empty;
		InvokeDefaultCustomsDecisionSupportingDocument();
		AssertSupportingDocumentCollectionIsEmpty();
	}

	public void TestWhenAuthorisationHasIncompatibleType()
	{
		authorisation.CPH_Type = "AUT";
		Factory.Save();

		SetupRequiredFields();

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionIsEmpty();
	}

	public void TestWhenHolderHasMoreAuthorisations()
	{
		var anotherAuth = Factory.NewAuthorisation(authOwner, "AAUTH.NUM2", AuthorisationType);
		var locRule = anotherAuth.CusAuthorisationRules.AddNew();
		locRule.CPR_RuleCode = "LOC";
		locRule.CPR_ValueFrom = CCpCode;

		Factory.Save();

		SetupRequiredFields();

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionContainsOnlyOne(ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc, "AAUTH.NUM2");
	}

	public void TestWhenHolderIsDeclarant()
	{
		Factory.Save();

		SetupRequiredFields();
		AddDocRuleToAuthorisation();

		declaration.JE_OH_Importer = ZGuid.Empty;
		declaration.JE_OH_Supplier = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = authOwner.MainAddress.PK;

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionContainsOnlyOne(DocType, AuthorisationNumber);
	}

	public void TestWhenHolderIsBothOwnerAndDeclarat()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();

		var anotherAuth = Factory.NewAuthorisation(declarant, "AAUTH.NUM2", AuthorisationType);
		var locRule = anotherAuth.CusAuthorisationRules.AddNew();
		locRule.CPR_RuleCode = "LOC";
		locRule.CPR_ValueFrom = CCpCode;

		Factory.Save();

		SetupRequiredFields();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		InvokeDefaultCustomsDecisionSupportingDocument();

		AssertSupportingDocumentCollectionContainsOnlyOne(ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc, AuthorisationNumber);
	}

	public void TestMessageWarningWhenAuthorisationHadDocRuleButInvoiceLineMissingDocument()
	{
		invoiceLine.SupportingDocumentsManager.ValidateCustomsDecisionsSupportingDocument();
		AssertEquals($"Invoice Line should not have the message error: {ExpectedMessageError}", false, invoiceLine.RowMessageErrors.Any(x => x.Message == ExpectedMessageError));

		SetupRequiredFields();

		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine.SupportingDocumentsManager.ValidateCustomsDecisionsSupportingDocument();
		AssertEquals($"Invoice Line should have following message error: {ExpectedMessageError}", true, invoiceLine.RowMessageErrors.Any(x => x.Message == ExpectedMessageError));

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc;
		invoiceLine.RemoveRowMessageError(ExpectedMessageError);
		invoiceLine.SupportingDocumentsManager.ValidateCustomsDecisionsSupportingDocument();
		AssertEquals($"Invoice Line should not have the message error: {ExpectedMessageError}", false, invoiceLine.RowMessageErrors.Any(x => x.Message == ExpectedMessageError));

		AddDocRuleToAuthorisation();
		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine.SupportingDocumentsManager.ValidateCustomsDecisionsSupportingDocument();
		AssertEquals($"Invoice Line should have following message error: {ExpectedMessageError}", true, invoiceLine.RowMessageErrors.Any(x => x.Message == ExpectedMessageError));

		supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = DocType;
		invoiceLine.SupportingDocumentsManager.ValidateCustomsDecisionsSupportingDocument();
		invoiceLine.RemoveRowMessageError(ExpectedMessageError);
		AssertEquals($"Invoice Line should not have the message error: {ExpectedMessageError}", false, invoiceLine.RowMessageErrors.Any(x => x.Message == ExpectedMessageError));
	}

	protected void AssertSupportingDocumentCollectionIsEmpty() => AssertEquals("Supporting documents should be empty", false, invoiceLine.SupportingDocuments.Any());

	void AssertSupportingDocumentCollectionContainsOnlyOne(ZString documentType, ZString documentReference) => AssertSupportingDocumentCollectionContainsOnlyOne(invoiceLine, documentType, documentReference);

	public static void AssertSupportingDocumentCollectionContainsOnlyOne(JobComInvoiceLine invoiceLine, ZString documentType, ZString documentReference)
	{
		var actualSupportingDocuments = invoiceLine.SupportingDocuments.Cast<SupportingDocument>().Where(x => x.CSI_Code == documentType);

		AssertEquals($"Supporting documents collection should contain only one {documentType} document", 1, actualSupportingDocuments.Count());

		var expectedSupportingDocument = actualSupportingDocuments.Single();
		AssertEquals($"Supporting documents collection should contain {documentType}", true, expectedSupportingDocument != null);
		AssertEquals($"{documentType} Supporting document Reference Number", documentReference, expectedSupportingDocument.CSI_ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		(customsWarehouseOrganization, authOwner, authorisation, locRule) = SetupAndGetDataForDefaultingSupportingDocument(Factory, AuthorisationNumber, AuthorisationType);

		Factory.Save();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = DeclarationMessageType;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
	}

	public static (OrgHeader CustomsWarehouseOrganization, OrgHeader Importer, CusAuthorisationHeader Authorisation, CusAuthorisationRule LocRule) SetupAndGetDataForDefaultingSupportingDocument(BusinessObjectFactory factory, ZString authNumber, ZString authType)
	{
		var referenceDataHelper = new ITUniversalReferenceTestDataHelper(factory);
		referenceDataHelper.CreateRefCusProcedure71ForCurrentCountry();
		referenceDataHelper.CreateRefCusProcedure51ForCurrentCountry();
		referenceDataHelper.CreateRefCusProcedure21And22ForCurrentCountry();

		var customsWarehouseOrganization = factory.New<OrgHeader>();
		customsWarehouseOrganization.OH_Code = "CWP";

		var ccpCode = customsWarehouseOrganization.CustomsCodes.AddNew("CCP", CCpCode);
		ccpCode.OK_OA_PremisesAddress = customsWarehouseOrganization.MainAddress.PK;

		var importer = factory.New<OrgHeader>();
		importer.OH_Code = "COD";

		var authorisation = factory.NewAuthorisation(importer, authNumber, authType);

		var locRule = authorisation.CusAuthorisationRules.AddNew();
		locRule.CPR_RuleCode = "LOC";
		locRule.CPR_ValueFrom = CCpCode;

		return (customsWarehouseOrganization, importer, authorisation, locRule);
	}

	public static void AddDocRuleToAuthorisation(CusAuthorisationHeader authorisation, string docType)
	{
		var docRule = authorisation.CusAuthorisationRules.AddNew();
		docRule.CPR_RuleCode = "DOC";
		docRule.CPR_ValueFrom = docType;
	}

	protected OrgHeader customsWarehouseOrganization;
	OrgHeader authOwner;
	CusAuthorisationHeader authorisation;
	protected CusAuthorisationRule locRule;
	JobDeclaration declaration;
	protected CusEntryInstruction entryInstruction;
	protected JobComInvoiceLine invoiceLine;

	protected virtual void SetupRequiredFields()
	{
		FillAuthOwner(authOwner);
		entryInstruction.CEI_Procedure = ProcedureCode.SubstringSafe(2);
		invoiceLine.JI_Procedure = ProcedureCode;
	}

	protected virtual void FillAuthOwner(OrgHeader orgHeader)
	{
		if (declaration.IsImport)
		{
			declaration.JE_OH_Importer = orgHeader.PK;
		}
		else if (declaration.IsExport)
		{
			declaration.JE_OH_Supplier = orgHeader.PK;
		}
	}

	protected abstract ZString ProcedureCode { get; }
	protected abstract ZString ExpectedSupportingDocumentTypeForAuthorisationWithNoDoc { get; }
	protected abstract ZString AuthorisationType { get; }
	ZString ExpectedMessageError => ZString.Format("For {0} procedure add document of type {1}", ProcedureTypeDescription, GetExpectedDocumentsTypeForMessageError());
	protected abstract ZString[] DefaultDocumentTypes { get; }
	protected abstract ZString ProcedureTypeDescription { get; }
	protected abstract ZString DeclarationMessageType { get; }

	void AddDocRuleToAuthorisation() => AddDocRuleToAuthorisation(authorisation, DocType);

	ZString GetExpectedDocumentsTypeForMessageError()
	{
		var expectedDocumentsTypeForMessageError = ZString.Empty;
		if (authorisation.CusAuthorisationRules.Any(x => x.CPR_RuleCode == "DOC"))
		{
			expectedDocumentsTypeForMessageError = DocType;
		}
		else
		{
			var count = DefaultDocumentTypes.Length;
			expectedDocumentsTypeForMessageError = count > 1
				? ZString.Format("{0} or {1}", ZString.Join(", ", DefaultDocumentTypes.Take(count - 1).ToArray()), DefaultDocumentTypes.Last())
				: DefaultDocumentTypes.Single();
		}
		return expectedDocumentsTypeForMessageError;
	}

	protected void InvokeDefaultCustomsDecisionSupportingDocument()
	{
		invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
		invoiceLine.SupportingDocumentsManager.AddCustomsDecisionsSupportingDocumentIfApplicable();
	}

	const string CCpCode = "1234Y";
	const string AuthorisationNumber = "AUTH. NUMBER";
	const string DocType = "C120";
}
