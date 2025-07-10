using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class ExportCusEntryInstructionCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestExportSupportingDocuments()
	{
		entryInstruction.ClearanceByEntryLine = false;
		var wrapper = GetNewWrapper();
		var supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(wrapper.SupportingDocuments), supportingDocuments);
		AssertEquals("[PRE-CONDITION] Count", 0, supportingDocuments.Count);
		var supportingDocument = entryInstruction.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "1001";
		supportingDocument.CSI_YearOfIssue = "2023";
		supportingDocument.CSI_RN_NKCountryCode = "IT";
		supportingDocument.CSI_ReferenceNumber = "12345";
		wrapper = GetNewWrapper();
		supportingDocuments = wrapper.SupportingDocuments;
		AssertNotNull(nameof(wrapper.SupportingDocuments), supportingDocuments);
		AssertEquals("Count", 1, supportingDocuments.Count);
		var supportingDocumentReturnedByWrapper = supportingDocuments.First();
		AssertType<SupportingDocumentWrapper>(supportingDocumentReturnedByWrapper);
		AssertEquals("Reference Number", "2023-IT-12345", supportingDocumentReturnedByWrapper.ReferenceNumber);
	}

	public void TestExportAdditionalReferences()
	{
		var wrapper = GetNewWrapper();
		var additionalReferences = wrapper.AdditionalReferences;
		AssertNotNull(nameof(wrapper.AdditionalReferences), additionalReferences);
		AssertEquals("[PRE-CONDITION] Count", 0, additionalReferences.Count);

		AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.AdditionalReference, code: "12345", referenceNumber: "REF13323");
		AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.AdditionalReference, code: "44564", referenceNumber: "R12213123");
		AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.TransportDocument, code: "TR211", referenceNumber: "TRefj3");

		wrapper = GetNewWrapper();
		additionalReferences = wrapper.AdditionalReferences;
		AssertNotNull(nameof(wrapper.AdditionalReferences), additionalReferences);
		AssertEquals("Count", 2, additionalReferences.Count);
		AssertContainsExactElementsInAnyOrder("ReferenceType", new[] { "12345", "44564" }, additionalReferences.Select(a => a.ReferenceType));
		AssertContainsExactElementsInAnyOrder("ReferenceNumber", new[] { "REF13323", "R12213123" }, additionalReferences.Select(a => a.ReferenceNumber));
	}

	public void TestExportAdditionalInformation()
	{
		var wrapper = GetNewWrapper();
		var additionalInformation = wrapper.AdditionalInformation;
		AssertNotNull(nameof(wrapper.AdditionalInformation), additionalInformation);
		AssertEquals("[PRE-CONDITION] Count", 0, additionalInformation.Count);

		AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.AdditionalInformation, code: "12345", description: "INF_DESCRIPTION_1");
		AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.AdditionalInformation, code: "IN22F", description: "R12213123");
		AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.TransportDocument, code: "TR211", referenceNumber: "TRefj3");

		wrapper = GetNewWrapper();
		additionalInformation = wrapper.AdditionalInformation;
		AssertNotNull(nameof(wrapper.AdditionalInformation), additionalInformation);
		AssertEquals("Count", 2, additionalInformation.Count);
		AssertContainsExactElementsInAnyOrder("Codes", new[] { "12345", "IN22F" }, additionalInformation.Select(a => a.Code));
		AssertContainsExactElementsInAnyOrder("Descriptions", new[] { "INF_DESCRIPTION_1", "R12213123" }, additionalInformation.Select(a => a.Description));
	}

	public void TestExportTransportDocuments_IsTransitionPeriodAES30()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			AssertEquals("Precondition: IsTransitionPeriodAES30", true, declaration.IsTransitionPeriodAES30);

			var wrapper = GetNewWrapper();
			var transportDocuments = wrapper.TransportDocuments;
			AssertNotNull(nameof(wrapper.TransportDocuments), transportDocuments);
			AssertEquals("[PRE-CONDITION] Count", 0, transportDocuments.Count);

			AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.TransportDocument, code: "12345", referenceNumber: "INF_DESCRIPTION_1");
			AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.AdditionalInformation, code: "IN22F", referenceNumber: "R12213123");
			AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.TransportDocument, code: "TR211", referenceNumber: "TRefj3");

			wrapper = GetNewWrapper();
			transportDocuments = wrapper.TransportDocuments;
			AssertNotNull(nameof(wrapper.TransportDocuments), transportDocuments);
			AssertEquals("Count", 2, transportDocuments.Count);
			AssertContainsExactElementsInAnyOrder("DocumentType", new[] { "12345", "TR211" }, transportDocuments.Select(a => a.DocumentType));
			AssertContainsExactElementsInAnyOrder("ReferenceNumber", new[] { "INF_DESCRIPTION_1", "TRefj3" }, transportDocuments.Select(a => a.ReferenceNumber));
		}
	}

	public void TestExportTransportDocuments_IsNotTransitionPeriodAES30()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Precondition: IsTransitionPeriodAES30", false, declaration.IsTransitionPeriodAES30);

			AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.TransportDocument, code: "Code2", referenceNumber: "Ref2");
			AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.AdditionalInformation, code: "Code3", referenceNumber: "Ref3");
			AddAdditionalInfo(AdditionalInfoSubTypeList.Codes.TransportDocument, code: "Code1", referenceNumber: "Ref1");

			var invoiceHeader = declaration.Invoices.AddNew();
			var headerAdditionalInfo = invoiceHeader.AdditionalInfos.AddNew("Code2", "Ref2");
			headerAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var lineAdditionalInfo = invoiceHeader.AdditionalInfos.AddNew("Code1", "Ref1Dif");
			lineAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			var wrapper = GetNewWrapper();
			var transportDocuments = wrapper.TransportDocuments;
			AssertContainsExactElementsInExactOrder("DocumentType", new[] { "Code1", "Code1", "Code2" }, transportDocuments.Select(a => a.DocumentType));
			AssertContainsExactElementsInExactOrder("ReferenceNumber", new[] { "Ref1", "Ref1Dif", "Ref2" }, transportDocuments.Select(a => a.ReferenceNumber));
		});
	}

	public void TestExportAuthorizations()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull(nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations), wrapper.Authorizations);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations)} count", 0, wrapper.Authorizations.Count);

		entryInstruction.CusAuthorizationUsages.AddNew();
		entryInstruction.CusAuthorizationUsages.AddNew();

		wrapper = GetNewWrapper();
		var authorizations = wrapper.Authorizations;
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations)} count", 2, authorizations.Count);
		AssertSame(nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations), authorizations, wrapper.Authorizations);
		AssertEquals($"{nameof(ICusEntryInstructionCustomsMessageWrapper.Authorizations)} Type", true, authorizations.All(x => x is CustomsCodeAuthorizationWrapper));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	IExportCusEntryInstructionCustomsMessageWrapper GetNewWrapper() => new ExportCusEntryInstructionCustomsMessageWrapper(entryInstruction);

	void AddAdditionalInfo(string subType, string code, string referenceNumber = "", string description = "")
	{
		var addInfo = entryInstruction.AdditionalInfos.AddNew();
		addInfo.CSI_SubType = subType;
		addInfo.CSI_Code = code;
		addInfo.CSI_ReferenceNumber = referenceNumber;
		addInfo.CSI_Description = description;
	}
}
