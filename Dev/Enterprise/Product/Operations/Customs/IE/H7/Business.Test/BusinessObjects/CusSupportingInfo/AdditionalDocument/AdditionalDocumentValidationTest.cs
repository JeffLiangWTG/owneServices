using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentValidation))]
	sealed class AdditionalDocumentValidationTest : BusinessObjectValidationTestCase
	{
		#region CSI_Code

		#region PackedItem

		public void TestValidateCSI_Code()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUCC6V1, "DC44I", "1111", "1111", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "2222", "2222", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var additionalDocument = packedItem.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			additionalDocument.CSI_Code = IE.Business.Constants.AdditionalReferenceCodes._1A01;
			AssertHasMessageError("Should error on 1A01 code", additionalDocument.CSI_CodeInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			additionalDocument.CSI_Code = IE.Business.Constants.AdditionalReferenceCodes._1A05;
			AssertHasMessageError("Should error on 1A05 code", additionalDocument.CSI_CodeInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.Validation.ValidateCSI_Code();
			AssertNoMessageError("Should not error for non-AdditionalReference types", additionalDocument.CSI_CodeInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument.CSI_ReferenceNumber = "test";
			additionalDocument.CSI_Code = ZString.Empty;
			AssertHasMessageError("Should error on empty code when reference entered", additionalDocument.CSI_CodeInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2048RuleMessage(additionalDocument.CSI_CodeInfo));

			additionalDocument.CSI_Code = "2222";
			AssertHasMessageError("Should error when code is not in list", additionalDocument.CSI_CodeInfo, additionalDocument.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);

			additionalDocument.CSI_Code = "1111";
			AssertNoMessageErrors(additionalDocument.CSI_CodeInfo);
		}

		#endregion

		#region Bill

		public void TestValidateCSI_Code_TransportDocumentRequired_V2()
		{
			SetUpTestData();
			var requireATranspotDocError = "[BR1106] A Transport Document Reference must be entered either at Bill or Item level with at least one of the following Full Type codes: 'N235', 'N271', 'N703', 'N704', 'N705', 'N710', 'N714', 'N720', 'N722', 'N730', 'N740', 'N741', 'N750', 'N760', 'N785', 'N787', 'N952', 'N955'.";
			header.AMA_ApplicationCode = "LV2";
			document.CSI_SubType = "TRA";
			document.CSI_Code = "1234";
			AssertHasMessageError("there is not a transport doc with accepted code", document.CSI_CodeInfo, requireATranspotDocError);

			AssertApplyOnlyToV2(requireATranspotDocError);

			document.CSI_SubType = "REF";
			document.CSI_Code = "N235";
			AssertNoMessageError("there is not a transport document", document.CSI_CodeInfo, requireATranspotDocError);

			document.CSI_SubType = "TRA";
			var acceptedCodeListForSubtypeTRA = new List<string> { "N235", "N271", "N703", "N704", "N705", "N710", "N714", "N720", "N722", "N730", "N740", "N741", "N750", "N760", "N785", "N787", "N952", "N955" };
			foreach (var code in acceptedCodeListForSubtypeTRA)
			{
				document.CSI_Code = code;
				AssertNoMessageError("there is a valid transpot doc on bill level", document.CSI_CodeInfo, requireATranspotDocError);
			}

			var item = bill.PackedItems.AddNew();
			var itemDocument = item.AdditionalDocuments.AddNew();
			itemDocument.CSI_SubType = "TRA";
			itemDocument.CSI_Code = "N235";
			document.CSI_Code = "1234";
			AssertNoMessageError("there is no valid transaport doc on item level", document.CSI_CodeInfo, requireATranspotDocError);
		}

		public void TestValidateCSI_Code_InvalidAuthorization_V2()
		{
			AssertInvalidCodeList(["1D02", "1D03", "1D04", "1Q75"], "[BR600006] Invalid Authorization Type.");
		}

		public void TestValidateCSI_Code_InvalidDocument_V2()
		{
			AssertInvalidCodeList(["1A01", "1A05"], "[BR2040] Invalid Document Type.");
		}

		public void TestValidateCSI_Code_RequireID24_V2()
		{
			SetUpTestData();
			var require1D24ReferenceTypeError = "[BR20319] Additional Reference Type '1D24' is required.";
			header.AMA_ApplicationCode = "LV2";
			document.CSI_SubType = "REF";
			document.CSI_Code = "1234";
			AssertHasMessageError("there is not a ref doc with code 1D24", document.CSI_CodeInfo, require1D24ReferenceTypeError);

			AssertApplyOnlyToV2(require1D24ReferenceTypeError);

			document.CSI_SubType = "TRA";
			document.CSI_Code = "1D24";
			AssertNoMessageError("there is not a reference document", document.CSI_CodeInfo, require1D24ReferenceTypeError);

			document.CSI_SubType = "REF";
			document.CSI_Code = "1D24";
			AssertNoMessageError(document.CSI_CodeInfo, require1D24ReferenceTypeError);
		}

		public void TestValidateCSI_Code_DuplicatedID24_V2()
		{
			SetUpTestData();
			var duplicatedReferenceTypeError = "[BR20319] There should be only one instance of Additional Reference Type '1D24' per Bill.";
			header.AMA_ApplicationCode = "LV2";
			document.CSI_SubType = "REF";
			document.CSI_Code = "1D24";
			AssertNoMessageError(document.CSI_CodeInfo, duplicatedReferenceTypeError);

			var anotherDoc = bill.AdditionalDocuments.AddNew();
			anotherDoc.CSI_SubType = "REF";
			anotherDoc.CSI_Code = "1D24";
			document.Validation.ValidateCSI_Code();
			AssertHasMessageError("2 reference docs with code 1D24", document.CSI_CodeInfo, duplicatedReferenceTypeError);

			AssertApplyOnlyToV2(duplicatedReferenceTypeError);

			anotherDoc.CSI_Code = "1234";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError(document.CSI_CodeInfo, duplicatedReferenceTypeError);
			bill.AdditionalDocuments.Delete(anotherDoc);
		}

		public void TestValidateCSI_Code_Require1A06()
		{
			SetUpTestData();
			var require1A06ReferenceTypeError = "[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.";
			AssertApplyToBothV1AndV2(() =>
			{
				bill.ABL_Procedure = EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49;
				document.CSI_SubType = "REF";
				document.CSI_Code = "1234";
				AssertHasMessageError("there is not a ref doc with 1A06 Code", document.CSI_CodeInfo, require1A06ReferenceTypeError);

				document.CSI_SubType = "TRA";
				document.Validation.ValidateCSI_Code();
				AssertNoMessageError("there is not a ref doc", document.CSI_CodeInfo, require1A06ReferenceTypeError);

				document.CSI_SubType = "REF";
				bill.ABL_Procedure = "C07";
				document.Validation.ValidateCSI_Code();
				AssertNoMessageError("no need to validate it for other procedure", document.CSI_CodeInfo, require1A06ReferenceTypeError);

				bill.ABL_Procedure = EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49;
				document.CSI_Code = "1A06";
				AssertNoMessageError(document.CSI_CodeInfo, require1A06ReferenceTypeError);
			});
		}

		public void TestValidateCSI_Code_ListValidationAndEmptyCheck()
		{
			SetUpTestData();
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUCC6V1, "DC44I", "1111", "1111", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "2222", "2222", yesterday, tomorrow);
			Factory.Save();

			var invalidCodeError = "The code you have selected is not in the list.";
			var emptyCodeError = "[BR2048] You have not entered a type.";
			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_SubType = "REF";
				document.CSI_Code = "2222";
				AssertHasMessageError("Should use IE5 for data grouping, so 2222 is not a valid code", document.CSI_CodeInfo, "[BR0020] " + invalidCodeError);

				document.CSI_Code = "1111";
				AssertNoMessageError(document.CSI_CodeInfo, "[BR0020] " + invalidCodeError);

				document.CSI_Code = string.Empty;
				AssertHasMessageError(document.CSI_CodeInfo, emptyCodeError);

				document.CSI_SubType = "TRA";
				document.Validation.ValidateCSI_Code();
				AssertNoMessageError(document.CSI_CodeInfo, emptyCodeError);
			});
		}

		void AssertInvalidCodeList(List<string> invalidCodeList, string errorMessage)
		{
			SetUpTestData();
			header.AMA_ApplicationCode = "LV2";
			document.CSI_SubType = "REF";
			foreach (var code in invalidCodeList)
			{
				document.CSI_Code = code;
				AssertHasMessageError(document.CSI_CodeInfo, errorMessage);
			}

			AssertApplyOnlyToV2(errorMessage);

			document.CSI_SubType = "TRA";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError("apply only to Subtype REF", document.CSI_CodeInfo, errorMessage);

			document.CSI_SubType = "REF";
			document.CSI_Code = "N235";
			AssertNoMessageError(document.CSI_CodeInfo, errorMessage);
		}

		#endregion

		#endregion

		#region CSI_ReferenceNumber

		#region PackedItem

		public void TestValidateCSI_ReferenceNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var additionalDocument = packedItem.AdditionalDocuments.AddNew();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument.CSI_Code = "1111";
			additionalDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageError("Should error on empty reference when code entered", additionalDocument.CSI_ReferenceNumberInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2048RuleMessage(additionalDocument.CSI_ReferenceNumberInfo));

			additionalDocument.CSI_ReferenceNumber = "test";
			AssertNoMessageError(additionalDocument.CSI_ReferenceNumberInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2048RuleMessage(additionalDocument.CSI_ReferenceNumberInfo));

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = "1111";
			additionalDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageError("Should not error for non-AdditionalReference types", additionalDocument.CSI_ReferenceNumberInfo, additionalDocument.ValidationConfiguration.ValidationMessage.GetBR2048RuleMessage(additionalDocument.CSI_ReferenceNumberInfo));

			additionalDocument.CSI_ReferenceNumber = "12345678901234567890123456789012345678901234567890123456789012345678901";
			AssertHasMessageError(additionalDocument.CSI_ReferenceNumberInfo, "Reference number length cannot exceed 70 alphanumeric characters.");
		}

		#endregion

		#region Bill

		public void TestValidateCSI_ReferenceNumber_ReferenceFormatForCodeN741_V2()
		{
			var referenceFormatForCodeN741Error = "[BR2038] Reference Number should contain 11 digits when Type is 'N741'.";
			AssertReferenceNumberFormat("N741", "TRA", "1234", "12345678901", referenceFormatForCodeN741Error, () =>
			{
				document.CSI_ReferenceNumber = "ABCDEFGHIJK";
				AssertHasMessageError("reference number should be numeric only", document.CSI_ReferenceNumberInfo, referenceFormatForCodeN741Error);
			});
		}

		public void TestValidateCSI_ReferenceNumber_InvalidReferenceForCode1D96_V2()
		{
			AssertReferenceNumberFormat("1D96", "REF", "2", "1", "[BR2316] Invalid Reference Number. It should be '1' when Type is '1D96'.", () => { });
		}

		public void TestValidateCSI_ReferenceNumber_InvalidReferenceForCode1D24_V2()
		{
			AssertReferenceNumberFormat("1D24", "REF", "123456", "202503060304", "[BR20319] Reference must be in format 'yyyyMMddHHmm' when Additional Reference Type is '1D24'.", () => { });
		}

		public void TestValidateCSI_ReferenceNumber_EmptyCheck()
		{
			SetUpTestData();
			var emptyReferenceNumberError = "[BR2048] You have not entered a reference number.";
			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_SubType = "REF";
				document.CSI_ReferenceNumber = string.Empty;
				AssertHasMessageError(document.CSI_ReferenceNumberInfo, emptyReferenceNumberError);

				document.CSI_SubType = "TRA";
				document.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError(document.CSI_ReferenceNumberInfo, emptyReferenceNumberError);
			});
		}

		void AssertReferenceNumberFormat(string code, string subType, string incorrectReferenceNumber, string correctReferenceNumber, string errorMessage, Action additionalTestCases)
		{
			SetUpTestData();
			header.AMA_ApplicationCode = "LV2";
			document.CSI_Code = code;
			document.CSI_SubType = subType;
			document.CSI_ReferenceNumber = incorrectReferenceNumber;
			AssertHasMessageError(document.CSI_ReferenceNumberInfo, errorMessage);

			additionalTestCases();

			document.CSI_Code = "1234";
			document.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"apply only to Code {code}", document.CSI_ReferenceNumberInfo, errorMessage);

			document.CSI_Code = code;
			document.CSI_SubType = "ABC";
			document.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"apply only to SubType {subType}", document.CSI_ReferenceNumberInfo, errorMessage);

			document.CSI_Code = code;
			AssertApplyOnlyToV2(errorMessage, true);

			document.CSI_ReferenceNumber = correctReferenceNumber;
			AssertNoMessageError(document.CSI_ReferenceNumberInfo, errorMessage);
		}

		#endregion

		#endregion

		#region CSI_SubType

		public void TestValidateCSI_SubType()
		{
			SetUpTestData();
			var invalidCodeError = "The code you have selected is not in the list.";
			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_SubType = "ABC";
				AssertHasMessageError(document.CSI_SubTypeInfo, invalidCodeError);

				document.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertNoMessageError(document.CSI_SubTypeInfo, invalidCodeError);
			});
		}

		#endregion

		void AssertApplyOnlyToV2(string errorMessage, bool isReference = false)
		{
			header.AMA_ApplicationCode = "LV1";
			if (isReference)
			{
				document.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("apply only to LV2", document.CSI_ReferenceNumberInfo, errorMessage);
			}
			else
			{
				document.Validation.ValidateCSI_Code();
				AssertNoMessageError("apply only to LV2", document.CSI_CodeInfo, errorMessage);
			}

			header.AMA_ApplicationCode = "LV2";
		}

		void AssertApplyToBothV1AndV2(Action validateErrorCase)
		{
			header.AMA_ApplicationCode = "LV1";
			validateErrorCase();

			header.AMA_ApplicationCode = "LV2";
			validateErrorCase();
		}

		void SetUpTestData()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			document = bill.AdditionalDocuments.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		AdditionalDocument document;
	}
}
