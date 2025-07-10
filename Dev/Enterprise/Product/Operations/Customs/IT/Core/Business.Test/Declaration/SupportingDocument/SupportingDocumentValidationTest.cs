using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocumentValidation))]
sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_RN_NKCountryCode()
	{
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoNotifications(supportingDocument.CSI_RN_NKCountryCodeInfo);
		supportingDocument.CSI_RN_NKCountryCode = "#@";
		AssertHasMessageErrorContaining(supportingDocument.CSI_RN_NKCountryCodeInfo, ListValidation.InvalidCodeMessageError);
		supportingDocument.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
		AssertNoNotifications(supportingDocument.CSI_RN_NKCountryCodeInfo);
	}

	public void TestCheckCSI_RN_NKCountryCode_MandatoryValidation()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Country);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Country Code", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_RN_NKCountryCode = "IT";
		AssertNoMessageErrorContaining("YYY requires Country Code and it is set", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Country Code", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_RN_NKCountryCode = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Country Code", supportingDocument.CSI_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_Status()
	{
		supportingDocument.CSI_Status = ZString.Empty;
		AssertNoMessageErrorContaining(supportingDocument.CSI_StatusInfo, ListValidation.InvalidCodeMessageError);

		supportingDocument.CSI_Status = "XXX";
		AssertHasMessageErrorContaining(supportingDocument.CSI_StatusInfo, ListValidation.InvalidCodeMessageError);

		supportingDocument.CSI_Status = AvailabilityTypeList.Codes.DER;
		AssertNoMessageErrorContaining(supportingDocument.CSI_StatusInfo, ListValidation.InvalidCodeMessageError);

		supportingDocument.CSI_Status = AvailabilityTypeList.Codes.PAP;
		AssertNoMessageErrorContaining(supportingDocument.CSI_StatusInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckCSI_Code()
	{
		supportingDocument.CSI_Code = "10YY";
		AssertHasMessageErrorContaining("10YY document should not be used", supportingDocument.CSI_CodeInfo, "Instead of document 10YY use field Third Quantity, 10YY will be added automatically in the Message to Customs");

		supportingDocument.CSI_Code = "ABCD";
		AssertNoMessageErrorContaining("Other document codes should be allowed", supportingDocument.CSI_CodeInfo, "Instead of document 10YY use field Third Quantity, 10YY will be added automatically in the Message to Customs");
	}

	public void TestCheckCSI_Value_WhenCodeIs60YY_ValueShouldBePositive()
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		invoiceLine.JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
		AssertEquals("[PRE-CONDITION] EXP UCC6", expected: true, declaration.IsUCC6AndIsExport);
		AssertNotNull("[PRE-CONDITION] Has instruction", supportingDocument.Instruction);

		AssertEntityValidation(supportingDocument)
			.WhenProperty(x => x.CSI_Code, Is.EqualTo("60YY"))
			.WhenProperty(x => x.CSI_Value, Is.EqualTo(0m))
			.WhenValidating(supportingDocument.Validation.ValidateCSI_Value)
			.ShouldCheckThat(x => x.CSI_ValueInfo, Has.MandatoryValueCannotBeZero);

		AssertEntityValidation(supportingDocument)
			.WhenProperty(x => x.CSI_Code, Is.EqualTo("60YY"))
			.WhenProperty(x => x.CSI_Value, Is.EqualTo(-1m))
			.WhenValidating(supportingDocument.Validation.ValidateCSI_Value)
			.ShouldCheckThat(x => x.CSI_ValueInfo, Has.MandatoryValueCannotBeNegative);
	}

	public void TestCheckCSI_RX_NKCurrency_WhenCodeIs60YY_CurrencyIsMandatory()
	{
		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "XML";
		invoiceLine.JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
		AssertEquals("[PRE-CONDITION] EXP UCC6", expected: true, declaration.IsUCC6AndIsExport);
		AssertNotNull("[PRE-CONDITION] Has instruction", supportingDocument.Instruction);
		AssertEntityValidation(supportingDocument)
			.WhenProperty(x => x.CSI_Code, Is.EqualTo("60YY"))
			.WhenValidating(() => supportingDocument.Validation.ValidateCSI_RX_NKCurrency())
			.ShouldCheckThat(x => x.CSI_RX_NKCurrencyInfo, Has.MandatoryYouHaveNotEntered);
	}

	public void TestCheckCSI_ReferenceNumber_CheckCodeFormatFor39YY()
	{
		var expectedMessageError = "Port Code for document 39YY must start with \"--\" (e.g. \"--ITTRS\")";

		CombineAssertions(() =>
		{
			supportingDocument.CSI_Code = "39YY";
			supportingDocument.CSI_ReferenceNumber = "AH3";
			AssertHasMessageErrorContaining("ReferenceNumber format is incorrect", supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			supportingDocument.CSI_ReferenceNumber = "--AH3";
			AssertNoMessageErrorContaining("ReferenceNumber format is correct", supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			supportingDocument.CSI_Code = "ZZZ";
			supportingDocument.CSI_ReferenceNumber = "AH3";
			AssertNoMessageErrorContaining("ReferenceNumber format is correct", supportingDocument.CSI_ReferenceNumberInfo, expectedMessageError);
		});
	}

	public void TestCheckCSI_ReferenceNumber_WhenDeclarationIsImport()
	{
		const string expectedMessageErrorForImport = "Reference + Year of Issue + Country Code exceeds the maximum allowed length in the declaration message.";

		supportingDocument.CSI_ReferenceNumber = "A0001";
		AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_ReferenceNumber = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);
	}

	public void TestCheckCSI_ReferenceNumberWithYearAndCountry()
	{
		var expectedMessageErrorForImport = "Reference + Year of Issue + Country Code exceeds the maximum allowed length in the declaration message.";

		supportingDocument.CSI_YearOfIssue = "";
		supportingDocument.CSI_RN_NKCountryCode = "";

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 36);
		AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 35);
		AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_YearOfIssue = "";
		supportingDocument.CSI_RN_NKCountryCode = "IT";

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 33);
		AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 32);
		AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_YearOfIssue = "2022";
		supportingDocument.CSI_RN_NKCountryCode = "";

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 31);
		AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 30);
		AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_YearOfIssue = "2022";
		supportingDocument.CSI_RN_NKCountryCode = "IT";

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 28);
		AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);

		supportingDocument.CSI_ReferenceNumber = new ZString('A', 27);
		AssertNoMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, expectedMessageErrorForImport);
	}

	public void TestCheckCSI_ReferenceNumber_MandatoryValidation()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.ReferenceNumber);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_ReferenceNumber = ZString.Empty;
		supportingDocument.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageErrorContaining("YYY requires Reference Number", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_ReferenceNumber = "1234";
		AssertNoMessageErrorContaining("YYY requires Reference Number and it is set", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Reference Number", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_ReferenceNumber = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Reference Number", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_ReferenceNumberForRexNumber()
	{
		var expectedSupplierDocumentRequireRexNumberWarningMessage = "This document Reference is different from the REX code of the Supplier (IEREX12345AB)";
		var expectedSupplierNeedRexNumberWarningMessage = "Cannot validate this document Reference because the Supplier has no REX code. Consider adding the REX code to the Supplier>Config>Registration Numbers";

		var supplierWithRexNumber = Factory.New<OrgHeader>();
		var rexNumber = supplierWithRexNumber.CustomsCodes.AddNew();
		rexNumber.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.RegisteredExporterNumber;

		var supplierWithNoRexCode = Factory.New<OrgHeader>();

		var declaration = Factory.New<JobDeclaration>();

		void AssertIfValidationLogicShouldBeApplied(string assertionMessage, ZPropertyInfo referenceNumberInfo, bool validationLogicShouldBeApplied, string expectedWarning)
		{
			if (validationLogicShouldBeApplied)
			{
				AssertHasWarningContaining(assertionMessage, referenceNumberInfo, expectedWarning);
			}
			else
			{
				AssertNoWarningContaining(assertionMessage, referenceNumberInfo, expectedWarning);
			}
		}

		void AssertRexValidationOnReferenceNumber(string assertionMessage, SupportingDocument supportingDocument, bool validationLogicShouldBeApplied)
		{
			CombineAssertions($"Assert Reference Number on {assertionMessage}", () =>
			{
				rexNumber.OK_CustomsRegNo = "IEREX12345AB";
				declaration.JE_OH_Supplier = supplierWithRexNumber.PK;

				supportingDocument.CSI_Code = "CXXX";
				supportingDocument.CSI_ReferenceNumber = "Ref.No";
				AssertNoWarningContaining("When Code is not C100, RexNo is not required", supportingDocument.CSI_ReferenceNumberInfo, expectedSupplierDocumentRequireRexNumberWarningMessage);

				supportingDocument.CSI_Code = "C100";
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertIfValidationLogicShouldBeApplied("When Code is C100 and Reference Number is not equal to Supplier REX", supportingDocument.CSI_ReferenceNumberInfo, validationLogicShouldBeApplied, expectedSupplierDocumentRequireRexNumberWarningMessage);

				supportingDocument.CSI_ReferenceNumber = "";
				AssertIfValidationLogicShouldBeApplied("When Code is C100 and Reference Number is empty", supportingDocument.CSI_ReferenceNumberInfo, validationLogicShouldBeApplied, expectedSupplierDocumentRequireRexNumberWarningMessage);

				supportingDocument.CSI_ReferenceNumber = "IEREX12345AB";
				AssertNoWarningContaining("When Code is C100 and Reference Number is equal to Supplier REX, No warning expected", supportingDocument.CSI_ReferenceNumberInfo, expectedSupplierDocumentRequireRexNumberWarningMessage);
				AssertNoWarningContaining("When Code is C100 and Supplier has REX number, No warning expected", supportingDocument.CSI_ReferenceNumberInfo, expectedSupplierNeedRexNumberWarningMessage);

				rexNumber.OK_CustomsRegNo = ZString.Empty;
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoWarningContaining("When Code is C100 but Supplier REX is Empty, No warning expected", supportingDocument.CSI_ReferenceNumberInfo, expectedSupplierDocumentRequireRexNumberWarningMessage);
				AssertIfValidationLogicShouldBeApplied("When Code is C100 and Supplier has a empty REX Number", supportingDocument.CSI_ReferenceNumberInfo, validationLogicShouldBeApplied, expectedSupplierNeedRexNumberWarningMessage);

				declaration.JE_OH_Supplier = supplierWithNoRexCode.PK;

				supportingDocument.CSI_Code = "C100";
				supportingDocument.CSI_ReferenceNumber = "IEREX12345AB";
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertIfValidationLogicShouldBeApplied("When Code is C100 and Supplier has not REX Number", supportingDocument.CSI_ReferenceNumberInfo, validationLogicShouldBeApplied, expectedSupplierNeedRexNumberWarningMessage);

				supportingDocument.CSI_Code = "C200";
				supportingDocument.CSI_ReferenceNumber = "IEREX12345AB";
				AssertNoWarningContaining("When Code is not C100 and Supplier has not REX Number, No warning expected", supportingDocument.CSI_ReferenceNumberInfo, expectedSupplierNeedRexNumberWarningMessage);
			});
		}

		var invoice = declaration.Invoices.AddNew();
		var supportingDocumentOnInvoice = invoice.SupportingDocuments.AddNew();
		AssertRexValidationOnReferenceNumber("When Merge is not done, no warning message expected in Invoice Header Supporting Documents", supportingDocumentOnInvoice, validationLogicShouldBeApplied: false);

		var invoiceLine = invoice.InvoiceLines.AddNew();
		var supportingDocumentOnInvoiceLine = invoiceLine.SupportingDocuments.AddNew();
		AssertRexValidationOnReferenceNumber("When Merge is not done, no warning message expected in Invoice Line Supporting Documents", supportingDocumentOnInvoiceLine, validationLogicShouldBeApplied: false);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		AssertRexValidationOnReferenceNumber("When invoice line has primary preference different from 200, no warning message expected in Invoice Line Supporting Documents", supportingDocumentOnInvoiceLine, validationLogicShouldBeApplied: false);
		AssertRexValidationOnReferenceNumber("When invoice header does not have any invoice line whith primary preference different = 200, no warning message expected in Invoice Line Supporting Documents", supportingDocumentOnInvoice, validationLogicShouldBeApplied: false);

		invoiceLine.JI_PrimaryPreference = "200";
		AssertRexValidationOnReferenceNumber("Invoice Line Supporting Documents", supportingDocumentOnInvoiceLine, validationLogicShouldBeApplied: true);
		AssertRexValidationOnReferenceNumber("Invoice Header Supporting Documents", supportingDocumentOnInvoice, validationLogicShouldBeApplied: true);
	}

	public void TestCheckCSI_DateOfIssue_MandatoryValidation()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Year);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertHasMessageErrorContaining("YYY requires Date Of Issue", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_DateOfIssue = ZDateTime.Now;
		AssertNoMessageErrorContaining("YYY requires Date Of Issue and it is set", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Date Of Issue", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Date Of Issue", supportingDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_Quantity_MandatoryValidation()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.Quantity);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_Quantity = ZDecimal.Zero;
		AssertHasMessageErrorContaining("YYY requires Quantity", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Quantity = 1m;
		AssertNoMessageErrorContaining("YYY requires Quantity and it is set", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_Quantity = ZDecimal.Zero;
		AssertNoMessageErrorContaining("NNN doesn't require Quantity", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_Quantity = ZDecimal.Zero;
		AssertNoMessageErrorContaining("UND doesn't require Quantity", supportingDocument.CSI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_UnitOfQuantity_MandatoryValidation()
	{
		SetUpRefCusCodesForAttributeName(RefCusCodeListAttributeName.UnitOfQuantity);

		supportingDocument.CSI_Code = "YYY";
		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertHasMessageErrorContaining("YYY requires Unit Of Quantity", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_UnitOfQuantity = "KG";
		AssertNoMessageErrorContaining("YYY requires Unit Of Quantity and it is set", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "NNN";
		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertNoMessageErrorContaining("NNN doesn't require Unit Of Quantity", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);

		supportingDocument.CSI_Code = "UND";
		supportingDocument.CSI_UnitOfQuantity = ZString.Empty;
		AssertNoMessageErrorContaining("UND doesn't require Unit Of Quantity", supportingDocument.CSI_UnitOfQuantityInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_UnitOfQuantity_ListValidation()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CTM", "Carats", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocument.CSI_UnitOfQuantityInfo, "XX", "CTM");
	}

	public void TestCheckU165CodeWhenRelatedEntryLineCustomsValueIsLessThan6000()
	{
		var expectedWarningMessage = ValidationCaptions.SupportingDocument.DocumentU165WasUsedButU164WouldBeBetter;

		supportingDocument.CSI_Code = "U165";
		AssertNoWarningContaining("No warning expected, When Merge is not already done", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoiceLine.JI_PrimaryPreference = "200";
		entryLine.CL_CustomsValue = 7000m;
		supportingDocument.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Customs Value of related entry line is greater than 6000€", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		entryLine.CL_CustomsValue = 5000m;
		supportingDocument.Validation.ValidateCSI_Code();
		AssertHasWarningContaining("Warning expected when Customs Value of related entry line is less than 6000€", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		invoiceLine.JI_PrimaryPreference = "300";
		supportingDocument.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Preference does not start with 2xx", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		var supportingDocumentOnInvoiceLine = invoiceHeader.SupportingDocuments.AddNew();
		supportingDocumentOnInvoiceLine.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Parent is not a InvoiceLine", supportingDocumentOnInvoiceLine.CSI_CodeInfo, expectedWarningMessage);
	}

	public void TestCheckU164CodeWhenRelatedEntryLineCustomsValueIsGreaterThan6000()
	{
		var expectedWarningMessage = ValidationCaptions.SupportingDocument.DocumentU164WasUsedButU165WouldBeBetter;

		supportingDocument.CSI_Code = "U164";
		AssertNoWarningContaining("No warning expected, When Merge is not already done", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoiceLine.JI_PrimaryPreference = "200";
		entryLine.CL_CustomsValue = 5000m;
		supportingDocument.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Customs Value of related entry line is less than 6000€", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		entryLine.CL_CustomsValue = 7000m;
		supportingDocument.Validation.ValidateCSI_Code();
		AssertHasWarningContaining("Warning expected when Customs Value of related entry line is greater than 6000€", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		entryLine.CL_CustomsValue = 6000m;
		supportingDocument.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Customs Value of related entry line is equal than 6000€", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		invoiceLine.JI_PrimaryPreference = "300";
		supportingDocument.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Preference does not start with 2xx", supportingDocument.CSI_CodeInfo, expectedWarningMessage);

		var supportingDocumentOnInvoiceLine = invoiceHeader.SupportingDocuments.AddNew();
		supportingDocumentOnInvoiceLine.Validation.ValidateCSI_Code();
		AssertNoWarningContaining("No warning expected, When Parent is not a InvoiceLine", supportingDocumentOnInvoiceLine.CSI_CodeInfo, expectedWarningMessage);
	}

	public void TestCheckCSI_CodeWhenPreferenceDoesNotStartWith2()
	{
		var expectedWarningMessage = "Consider removing this document or checking for the used preference";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		CombineAssertions(() =>
		{
			foreach (var code in new[] { "C100", "U164", "U165", "U166" })
			{
				invoiceLine.JI_PrimaryPreference = "400";
				supportingDocument.CSI_Code = code;
				AssertHasWarningContaining($"Warning expected when code is {code} but primary prefernce does not start with 2xx", supportingDocument.CSI_CodeInfo, expectedWarningMessage);
				invoiceLine.JI_PrimaryPreference = "200";
				supportingDocument.Validation.ValidateCSI_Code();
				AssertNoWarningContaining($"No warning expected, when code is {code} and Primary Preference is 200", supportingDocument.CSI_CodeInfo, expectedWarningMessage);
			}
		});
	}

	public void TestCheckCSI_ReferenceNumber_DeclarationOfIntentFormat()
	{
		var expectedMessage = ValidationCaptions.SupportingDocument.DeclarationOfIntentNumberFormatNotValid;

		supportingDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		supportingDocument.CSI_ReferenceNumber = "";
		AssertHasWarningContaining("01DI - empty reference number", supportingDocument.CSI_ReferenceNumberInfo, expectedMessage);

		supportingDocument.CSI_ReferenceNumber = "20123111223312345123456";
		AssertNoWarningContaining("01DI - valid reference number", supportingDocument.CSI_ReferenceNumberInfo, expectedMessage);

		supportingDocument.CSI_ReferenceNumber = "ffdfsdfsdf";
		AssertHasWarningContaining("01DI - invalid reference number", supportingDocument.CSI_ReferenceNumberInfo, expectedMessage);

		supportingDocument.CSI_ReferenceNumber = "X";
		AssertHasWarningContaining("01DI - placeholder", supportingDocument.CSI_ReferenceNumberInfo, expectedMessage);

		supportingDocument.CSI_Code = "N380";
		supportingDocument.CSI_ReferenceNumber = "aabbccddeeff";
		AssertNoWarningContaining("N380 - invalid reference number", supportingDocument.CSI_ReferenceNumberInfo, expectedMessage);
	}

	public void TestCheckCSI_ReferenceNumber_DeclarationOfIntentUniquePerEntry()
	{
		var expectedMessage = ValidationCaptions.SupportingDocument.ThereShouldBeOnlyOneDistinct01DIDocumentPerEntry;

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader1.MergedLines.AddNew();
		var invoice1 = declaration.Invoices.AddNew();
		var supportingDocument1 = invoice1.SupportingDocuments.AddNew();
		var supportingDocument2 = invoice1.SupportingDocuments.AddNew();
		var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
		var entryLine2 = entryHeader2.MergedLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var supportingDocument3 = invoice2.SupportingDocuments.AddNew();
		var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		supportingDocument1.CSI_ReferenceNumber = "20123111223312345123456";
		supportingDocument1.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		AssertNoWarningContaining("Only 1 01DI document", supportingDocument1.CSI_CodeInfo, expectedMessage);

		supportingDocument2.CSI_ReferenceNumber = "20123111223312345123456";
		supportingDocument2.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;

		supportingDocument3.CSI_ReferenceNumber = "AABBCC";
		supportingDocument3.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;

		supportingDocument1.Validation.ValidateCSI_Code();
		supportingDocument2.Validation.ValidateCSI_Code();
		supportingDocument3.Validation.ValidateCSI_Code();
		CombineAssertions("2 01DI documents having the same reference number in the same entry", () =>
		{
			AssertNoWarningContaining($"{nameof(supportingDocument1)} - {entryHeader1}", supportingDocument1.CSI_CodeInfo, expectedMessage);
			AssertNoWarningContaining($"{nameof(supportingDocument2)} - {entryHeader1}", supportingDocument2.CSI_CodeInfo, expectedMessage);
			AssertNoWarningContaining($"{nameof(supportingDocument3)} - {entryHeader2}", supportingDocument3.CSI_CodeInfo, expectedMessage);
		});

		supportingDocument2.CSI_ReferenceNumber = "XXYYZZ";
		supportingDocument1.Validation.ValidateCSI_Code();
		supportingDocument2.Validation.ValidateCSI_Code();
		supportingDocument3.Validation.ValidateCSI_Code();
		CombineAssertions("2 01DI documents having a different reference number in the same entry", () =>
		{
			AssertHasWarningContaining($"{nameof(supportingDocument1)} - {entryHeader1}", supportingDocument1.CSI_CodeInfo, expectedMessage);
			AssertHasWarningContaining($"{nameof(supportingDocument2)} - {entryHeader1}", supportingDocument2.CSI_CodeInfo, expectedMessage);
			AssertNoWarningContaining($"{nameof(supportingDocument3)} - {entryHeader2}", supportingDocument3.CSI_CodeInfo, expectedMessage);
		});
	}

	public void TestCheckCSI_DateOfIssueIsValidZDateRange()
	{
		supportingDocument.CSI_DateOfIssue = ZDateTime.Today.AddYears(-11);
		AssertNoErrors(supportingDocument.CSI_DateOfIssueInfo);
		AssertHasWarningContaining(supportingDocument.CSI_DateOfIssueInfo, "is more than 1 year old");
	}

	public void TestCheckCSI_UnitOfQuantity_IfQuantityIsPresentThenUnitOfQuantityMustBeFilled()
	{
		const string expectedMessage = "Rule C0298: If Quantity is present then Unit of Quantity must be filled";

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				supportingDocument.CSI_Quantity = 0m;
				supportingDocument.CSI_UnitOfQuantity = "";
				AssertNoMessageErrorContaining("When both Quantity and UnitOfQuantity are empty", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);

				supportingDocument.CSI_UnitOfQuantity = "KG";
				AssertNoMessageErrorContaining("When Quantity is empty and UnitOfQuantity is filled", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);

				supportingDocument.CSI_Quantity = 1m;
				supportingDocument.CSI_UnitOfQuantity = "";
				AssertHasMessageErrorContaining("When Quantity is filled and UnitOfQuantity is empty", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);

				supportingDocument.CSI_UnitOfQuantity = "KG";
				AssertNoMessageErrorContaining("When both Quantity and UnitOfQuantity are filled", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);
			});
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			supportingDocument.CSI_Quantity = 1m;
			supportingDocument.CSI_UnitOfQuantity = "";
			AssertNoMessageErrorContaining("When Declaration is not UCC6, Quantity is filled and UnitOfQuantity is empty", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);
		}
	}

	public void TestCheckCSI_UnitOfQuantity_IfQuantityIsEmptyThenAlsoUnitOfQuantityMustBeEmpty()
	{
		const string expectedMessage = "Rule C0298: If Quantity is empty then also Unit of Quantity must be empty";

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			CombineAssertions("When Declaration is UCC6", () =>
			{
				supportingDocument.CSI_Quantity = 0m;
				supportingDocument.CSI_UnitOfQuantity = "";
				AssertNoMessageErrorContaining("When both Quantity and UnitOfQuantity are empty", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);

				supportingDocument.CSI_UnitOfQuantity = "KG";
				AssertHasMessageErrorContaining("When Quantity is empty and UnitOfQuantity is filled", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);

				supportingDocument.CSI_Quantity = 1m;
				supportingDocument.CSI_UnitOfQuantity = "";
				AssertNoMessageErrorContaining("When Quantity is filled and UnitOfQuantity is empty", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);

				supportingDocument.CSI_UnitOfQuantity = "KG";
				AssertNoMessageErrorContaining("When both Quantity and UnitOfQuantity are filled", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);
			});
		}

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			supportingDocument.CSI_Quantity = 0m;
			supportingDocument.CSI_UnitOfQuantity = "KG";
			AssertNoMessageErrorContaining("When Declaration is not UCC6, Quantity is empty and UnitOfQuantity is filled", supportingDocument.CSI_UnitOfQuantityInfo, expectedMessage);
		}
	}

	#region Implementation

	void SetUpRefCusCodesForAttributeName(ZString attributeName)
	{
		const string importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var dataGroupingIT = helper.CreateNewOrGetExistingDataGrouping("IT", "Italy");
		helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Import Code Type");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, importCodeType, dataGroupingIT.ZZZ_DataGrouping);

		var cusCodeYYY = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, importCodeType, "YYY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeYYY.Attributes.AddNew(attributeName, "Y");

		var cusCodeNNN = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, importCodeType, "NNN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeNNN.Attributes.AddNew(attributeName, "N");

		helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, importCodeType, "UND", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		supportingDocument = invoiceLine.SupportingDocuments.AddNew();
	}

	SupportingDocument supportingDocument;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoiceHeader;
	JobDeclaration declaration;

	#endregion
}
