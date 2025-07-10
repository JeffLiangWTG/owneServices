using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ExportRestrictionValidation))]
class ExportRestrictionValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Code_NS30003()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		Restriction.Validation.ValidateCSI_Code();
		AssertNoRowMessageError("No error when Ordinary", Restriction, PassarValidationMessages.MessageNS30003_Restriction);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		Restriction.Validation.ValidateCSI_Code();
		AssertHasRowMessageError("[NS30003] No error when Simplify", Restriction, PassarValidationMessages.MessageNS30003_Restriction);

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Restriction.Validation.ValidateCSI_Code();
		AssertNoRowMessageError("No error when EDA", Restriction, PassarValidationMessages.MessageNS30003_Restriction);
	}

	public void TestCheckCSI_Reference_NS30120() => CombineAssertions(() =>
	{
		Restriction.CSI_ReferenceNumber = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		AssertNoMessageError("Uppercase", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30120);

		Restriction.CSI_ReferenceNumber = "ABCDEFG%$JKLMNOPQRSTUVWXYZ";
		AssertHasMessageError("Uppercase, ivalid", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30120);

		Restriction.CSI_ReferenceNumber = "abcdefghijklmnopqrstuvwxyz";
		AssertNoMessageError("Lowercase", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30120);

		Restriction.CSI_ReferenceNumber = "abc=$!ghijklmnopqrstuvwxyz";
		AssertHasMessageError("Lowercase, invalid", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30120);

		Restriction.CSI_ReferenceNumber = "01232456789-./";
		AssertNoMessageError("Numbers, special signs", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30120);

		Restriction.CSI_ReferenceNumber = "012+*456789-./";
		AssertHasMessageError("Numbers, special signs, invalid", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30120);
	});

	public void TestCheckCSI_Reference_NS30001() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		RefCusCodeTestHelper.CreateRestrictionCodeWithPermitNumberAttribute(Factory);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAttributeY;
		Restriction.CSI_ReferenceNumber = ZString.Empty;
		AssertHasMessageErrorContaining("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == false and CSI_ReferenceNumber.isEmpty, you have not entered error", Restriction.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		Restriction.CSI_ReferenceNumber = "123";
		AssertNoMessageErrorContaining("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == false and CSI_ReferenceNumber.isNotEmpty,no message error", Restriction.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesY;
		Restriction.CSI_ReferenceNumber = ZString.Empty;
		AssertHasMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isEmpty and CSI_Description.isEmpty, show message error mandatory", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isEmpty and CSI_Description.isEmpty, no message error NotApplicable", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitNumberNotBoth);

		Restriction.CSI_ReferenceNumber = "123";
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isNotEmpty and CSI_Description.isEmpty,no message error mandatory", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isNotEmpty and CSI_Description.isEmpty, no message error NotApplicable", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitNumberNotBoth);

		Restriction.CSI_Description = "123";
		Restriction.Validation.ValidateCSI_ReferenceNumber();
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isNotEmpty and CSI_Description.isNotEmpty,no message error mandatory", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);
		AssertHasMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isNotEmpty and CSI_Description.isNotEmpty,show message error NotApplicable", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitNumberNotBoth);

		Restriction.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isEmpty and CSI_Description.isNotEmpty,no message error", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == true and  PermitExceptionReasonAllowed == true and CSI_ReferenceNumber.isEmpty and CSI_Description.isNotEmpty, no message error NotApplicable", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitNumberNotBoth);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesN;
		Restriction.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageError("if the CSI_Code has the attribute PermitNumberAllowed == false and CSI_ReferenceNumber.isEmpty,no message error", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitNumber);

		Restriction.CSI_ReferenceNumber = "123";
		AssertHasMessageError("if the CSI_Code has the attribute PermitNumberAllowed == false and CSI_ReferenceNumber.isNotEmpty, not applicable Message error", Restriction.CSI_ReferenceNumberInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitNumber);
	});

	public void TestCheckCSI_Description_NS30001() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRestrictionCodeWithPermitNumberAttribute(Factory);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAndPermitExceptionReasonAttributesY;
		Restriction.Validation.ValidateCSI_Description();
		AssertHasMessageError("Empty permit number and permit exception reason", Restriction.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);

		Restriction.CSI_Description = "R2";
		Restriction.Validation.ValidateCSI_Description();
		AssertNoError("Empty permit number and filled permit exception reason", Restriction.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30001_MandatoryPermitNumberOrPermitExceptionReason);

		Restriction.CSI_ReferenceNumber = "R1";
		Restriction.Validation.ValidateCSI_Description();
		AssertHasMessageError("Filled both permit number and permit exception reason", Restriction.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30001_RequiredJustOneOfPermitNumberAndPermitExceptionReason);

		Restriction.CSI_Description = ZString.Empty;
		Restriction.Validation.ValidateCSI_Description();
		AssertNoError("Filled just permit number", Restriction.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30001_RequiredJustOneOfPermitNumberAndPermitExceptionReason);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithPermitNumberAllowedAttributeY;
		Restriction.CSI_Description = "R2";
		Restriction.Validation.ValidateCSI_Description();
		AssertHasMessageError("Permit exception reason not applicable", Restriction.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitExceptionReason);

		Restriction.CSI_Description = ZString.Empty;
		Restriction.Validation.ValidateCSI_Description();
		AssertNoError("Permit exception reason not applicable", Restriction.CSI_DescriptionInfo, PassarValidationMessages.MessageNS30001_NotApplicablePermitExceptionReason);
	});

	public void TestCheckCSI_CodeNS30116() => CombineAssertions(() =>
	{
		const string messageError = "[NS30116] You have not entered Additional Information.";

		RefCusCodeTestHelper.CreateRestrictionCodeAdditionalInformationAttribute(Factory);
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		Restriction.CSI_Code = ZString.Empty;
		AssertNoRowMessageError("Empty", Restriction, messageError);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithAdditionalInformationAttributeN;
		AssertNoRowMessageError("AdditionalInformationAttribute = N, empty AdditionalInformations", Restriction, messageError);

		var addInfo1 = Restriction.AdditionalInformations.AddNew();
		addInfo1.CY_Code = ZString.Empty;
		var addInfo2 = Restriction.AdditionalInformations.AddNew();
		addInfo2.CY_Code = ZString.Empty;
		AssertNoRowMessageError("AdditionalInformationAttribute = N, empty CY_Code", Restriction, messageError);

		addInfo1.CY_Code = "123";
		addInfo2.CY_Code = "456";
		AssertNoRowMessageError("AdditionalInformationAttribute = N, 2 CY_Code", Restriction, messageError);

		Restriction.AdditionalInformations.RemoveAll();
		Restriction.Validation.ValidateAll();
		AssertNoRowMessageError("AdditionalInformationAttribute = N,  cleared AdditionalInformations", Restriction, messageError);

		Restriction.CSI_Code = RefCusCodeTestHelper.RestrictionCodeWithAdditionalInformationAttributeY;
		AssertHasRowMessageError("AdditionalInformationAttribute = Y, empty AdditionalInformations", Restriction, messageError);

		addInfo1 = Restriction.AdditionalInformations.AddNew();
		addInfo1.CY_Code = ZString.Empty;
		addInfo2 = Restriction.AdditionalInformations.AddNew();
		addInfo2.CY_Code = ZString.Empty;
		AssertHasRowMessageError("AdditionalInformationAttribute = Y, empty CY_Code", Restriction, messageError);

		addInfo1.CY_Code = "123";
		AssertNoRowMessageError("AdditionalInformationAttribute = Y, 1 CY_Code", Restriction, messageError);

		addInfo1.CY_Code = ZString.Empty;
		AssertHasRowMessageError("AdditionalInformationAttribute = Y, empty CY_Code", Restriction, messageError);

		addInfo1.CY_Code = "123";
		addInfo2.CY_Code = "456";
		AssertNoRowMessageError("AdditionalInformationAttribute = Y, 2 CY_Code", Restriction, messageError);

		Restriction.AdditionalInformations.RemoveAll();
		Restriction.Validation.ValidateAll();
		AssertHasRowMessageError("AdditionalInformationAttribute = Y, cleared AdditionalInformations", Restriction, messageError);
	});

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction entryInstruction;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= CreateNewInvoiceLine();
	JobComInvoiceLine jobComInvoiceLine;

	Restriction Restriction => restriction ??= JobComInvoiceLine.Restrictions.AddNew();
	Restriction restriction;

	JobComInvoiceLine CreateNewInvoiceLine()
	{
		var invLine = InvoiceHeader.InvoiceLines.AddNew();
		invLine.JI_CEI = EntryInstruction.PK;
		return invLine;
	}
}
