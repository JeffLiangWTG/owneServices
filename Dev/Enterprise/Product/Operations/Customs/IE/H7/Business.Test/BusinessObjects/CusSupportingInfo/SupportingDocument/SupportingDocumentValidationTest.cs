using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(SupportingDocumentValidation))]
	sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
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
			var supportingDoc = packedItem.SupportingDocuments.AddNew();

			supportingDoc.CSI_Code = SupportingDocumentCodes.H7InvalidCodes[0];
			AssertHasMessageError("Should error on invalid code", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR600006RuleMessage());

			supportingDoc.CSI_Code = SupportingDocumentCodes._N741;
			AssertNoMessageError("Should not error on valid code", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR600006RuleMessage());

			supportingDoc.CSI_Code = SupportingDocumentCodes.H7InvalidCodesV1Only[0];
			AssertHasMessageError("Should error on invalid code for V1", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20318RuleMessage());

			supportingDoc.CSI_Code = SupportingDocumentCodes._N741;
			AssertNoMessageError("Should not error on valid code for V1", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20318RuleMessage());

			header.AMA_ApplicationCode = "LV2";
			supportingDoc.CSI_Code = SupportingDocumentCodes.H7InvalidCodesV1Only[0];
			AssertNoMessageError("Should not BR20318 error for V2", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20318RuleMessage());

			header.AMA_ApplicationCode = "LV1";
			supportingDoc.CSI_Code = SupportingDocumentCodes._1A01;
			AssertHasMessageError("Should error on 1A01 code for V1", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			supportingDoc.CSI_Code = SupportingDocumentCodes._1A05;
			AssertHasMessageError("Should error on 1A05 code for V1", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			supportingDoc.CSI_Code = SupportingDocumentCodes._N741;
			AssertNoMessageError("Should not error for code not 1A01, 1A05", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			header.AMA_ApplicationCode = "LV2";
			supportingDoc.CSI_Code = SupportingDocumentCodes._1A05;
			AssertNoMessageError("Should not BR2040 error for V2", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2040RuleMessage());

			supportingDoc.CSI_ReferenceNumber = "test";
			supportingDoc.CSI_Code = ZString.Empty;
			AssertHasMessageError("Should error on empty code when reference entered", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20313RuleMessage(supportingDoc.CSI_CodeInfo));

			supportingDoc.CSI_Code = "1111";
			AssertNoMessageError("Should not error when both code and reference entered", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20313RuleMessage(supportingDoc.CSI_CodeInfo));

			supportingDoc.CSI_Code = "2222";
			AssertHasMessageError("Should error when code is not in list", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);

			supportingDoc.CSI_Code = "1111";
			AssertNoMessageError("Should not error when code is in list", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);

			var supportingDoc2 = packedItem.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = SupportingDocumentCodes._U164;
			supportingDoc2.CSI_Code = SupportingDocumentCodes._U166;
			supportingDoc.Validation.ValidateCSI_Code();
			AssertHasMessageError("Should BR20312 error on invalid combination", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20312RuleMessage());
			AssertHasMessageError("Should BR20312 error on invalid combination", supportingDoc2.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20312RuleMessage());

			header.AMA_ApplicationCode = "LV1";
			supportingDoc.Validation.ValidateCSI_Code();
			supportingDoc2.Validation.ValidateCSI_Code();
			AssertNoMessageError("Should not BR20312 error for V1", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20312RuleMessage());
			AssertNoMessageError("Should not BR20312 error for V1", supportingDoc2.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20312RuleMessage());

			header.AMA_ApplicationCode = "LV2";
			supportingDoc.CSI_Code = SupportingDocumentCodes._U165;
			supportingDoc2.CSI_Code = SupportingDocumentCodes._U167;
			supportingDoc.Validation.ValidateCSI_Code();
			AssertNoMessageError("Should not BR20312 error for U165 and U167", supportingDoc.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20312RuleMessage());
			AssertNoMessageError("Should not BR20312 error for U165 and U167", supportingDoc2.CSI_CodeInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20312RuleMessage());
		}

		#endregion

		#region Bill

		public void TestValidateCSI_Code_InvalidCertificateCode()
		{
			AssertInvalidCodeList(["C644", "C640", "C678", "N853", "C100"], "[BR20318] Invalid Certificate Code.");
		}

		public void TestValidateCSI_Code_InvalidDocumentType()
		{
			AssertInvalidCodeList(["1A01", "1A05"], "[BR2040] Invalid Document Type.");
		}

		public void TestValidateCSI_Code_RequireAtLeastOneForNonC08()
		{
			SetUpTestData();
			var requireAtleastOneForNonC08Error = "[BR2037] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.";
			var requiredCodeListForNonC08 = new List<string> { "D005", "D008", "N325", "N380", "N864", "N935" };
			AssertApplyToBothV1AndV2(() =>
			{
				bill.ABL_Procedure = "C07";
				document.CSI_Code = "1234";
				AssertHasMessageError(document.CSI_CodeInfo, requireAtleastOneForNonC08Error);

				foreach (var code in requiredCodeListForNonC08)
				{
					document.CSI_Code = code;
					AssertNoMessageError(document.CSI_CodeInfo, requireAtleastOneForNonC08Error);
				}

				bill.ABL_Procedure = "C08";
				document.CSI_Code = "1234";
				AssertNoMessageError(document.CSI_CodeInfo, requireAtleastOneForNonC08Error);
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
			Factory.Save();

			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_Code = "2222";
				AssertHasMessageError(document.CSI_CodeInfo, "[BR0020] The code you have selected is not in the list.");

				document.CSI_Code = "1111";
				AssertNoMessageError(document.CSI_CodeInfo, "[BR0020] The code you have selected is not in the list.");
				AssertNoMessageError(document.CSI_CodeInfo, "[BR20313] You have not entered a Type.");

				document.CSI_Code = string.Empty;
				AssertHasMessageError(document.CSI_CodeInfo, "[BR20313] You have not entered a Type.");
			});
		}

		public void TestValidateCSI_Code_RequireDocumentForCode1D24_V1()
		{
			SetUpTestData();
			var require1D24ReferenceTypeError = "[BR20319] Supporting Document Type '1D24' is required.";
			header.AMA_ApplicationCode = "LV1";
			document.CSI_Code = "1234";
			AssertHasMessageError(document.CSI_CodeInfo, require1D24ReferenceTypeError);

			AssertApplyOnlyToV1(require1D24ReferenceTypeError);

			document.CSI_Code = "1D24";
			AssertNoMessageError(document.CSI_CodeInfo, require1D24ReferenceTypeError);
		}

		public void TestValidateCSI_Code_DuplicatedID24_V1()
		{
			SetUpTestData();
			var duplicatedReferenceTypeError = "[BR20319] There should be only one instance of Supporting Document Type '1D24' per Bill.";
			header.AMA_ApplicationCode = "LV1";
			document.CSI_Code = "1D24";
			AssertNoMessageError(document.CSI_CodeInfo, duplicatedReferenceTypeError);

			var anotherDoc = bill.SupportingDocuments.AddNew();
			anotherDoc.CSI_Code = "1D24";
			document.Validation.ValidateCSI_Code();
			AssertHasMessageError("2 reference docs with code 1D24", document.CSI_CodeInfo, duplicatedReferenceTypeError);

			AssertApplyOnlyToV1(duplicatedReferenceTypeError);

			anotherDoc.CSI_Code = "1234";
			document.Validation.ValidateCSI_Code();
			AssertNoMessageError(document.CSI_CodeInfo, duplicatedReferenceTypeError);
			bill.SupportingDocuments.Delete(anotherDoc);
		}

		public void TestValidateCSI_Code_InvalidAuthorizationType_V1()
		{
			SetUpTestData();
			var invalidAuthorizationTypeError = "[BR600006] Invalid Authorization Type.";
			var invalidAuthorizationCodeList = new List<string> { "1D02", "1D03", "1D04", "1Q75" };
			header.AMA_ApplicationCode = "LV1";
			foreach (var code in invalidAuthorizationCodeList)
			{
				document.CSI_Code = code;
				AssertHasMessageError(document.CSI_CodeInfo, invalidAuthorizationTypeError);
			}

			AssertApplyOnlyToV1(invalidAuthorizationTypeError);

			document.CSI_Code = "1234";
			AssertNoMessageError(document.CSI_CodeInfo, invalidAuthorizationTypeError);
		}

		void AssertInvalidCodeList(List<string> invalidCodeList, string errorMessage)
		{
			SetUpTestData();
			AssertApplyToBothV1AndV2(() =>
			{
				foreach (var code in invalidCodeList)
				{
					document.CSI_Code = code;
					AssertHasMessageError(document.CSI_CodeInfo, errorMessage);
				}

				document.CSI_Code = "1234";
				AssertNoMessageError(document.CSI_CodeInfo, errorMessage);
			});
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
			var supportingDoc = packedItem.SupportingDocuments.AddNew();

			supportingDoc.CSI_Code = SupportingDocumentCodes._N741;
			supportingDoc.CSI_ReferenceNumber = "111";
			AssertHasMessageError("Should error when reference length is not 11 for N741", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2038RuleMessage());

			supportingDoc.CSI_ReferenceNumber = "11111111111";
			AssertNoMessageError("Should not error when reference length is 11 for N741", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2038RuleMessage());

			header.AMA_ApplicationCode = "LV2";
			supportingDoc.CSI_ReferenceNumber = "111";
			AssertNoMessageError("Should not BR2038 error for V2", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2038RuleMessage());

			header.AMA_ApplicationCode = "LV1";
			supportingDoc.CSI_Code = SupportingDocumentCodes._1D96;
			supportingDoc.CSI_ReferenceNumber = "0";
			AssertHasMessageError("Should error when reference is not 1 for 1D96", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2316RuleMessage());

			supportingDoc.CSI_ReferenceNumber = "1";
			AssertNoMessageError("Should not error when reference is 1 for 1D96", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2316RuleMessage());

			header.AMA_ApplicationCode = "LV2";
			supportingDoc.CSI_ReferenceNumber = "0";
			AssertNoMessageError("Should not BR2316 error for V2", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR2038RuleMessage());

			supportingDoc.CSI_Code = SupportingDocumentCodes._U164;
			supportingDoc.CSI_ReferenceNumber = "123";
			AssertHasMessageError("Should error when reference is not YYYYMMDD format for U164, U165, U166, U167", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20311RuleMessage());

			supportingDoc.CSI_ReferenceNumber = "20250101";
			AssertNoMessageError("Should not error when reference is YYYYMMDD format for U164, U165, U166, U167", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20311RuleMessage());

			header.AMA_ApplicationCode = "LV1";
			supportingDoc.CSI_ReferenceNumber = "123";
			AssertNoMessageError("Should not BR20311 error for V1", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20311RuleMessage());

			supportingDoc.CSI_Code = "1111";
			supportingDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageError("Should error on reference when code entered", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20313RuleMessage(supportingDoc.CSI_ReferenceNumberInfo));

			supportingDoc.CSI_ReferenceNumber = "123";
			AssertNoMessageError("Should not error when both reference and code entered", supportingDoc.CSI_ReferenceNumberInfo, supportingDoc.ValidationConfiguration.ValidationMessage.GetBR20313RuleMessage(supportingDoc.CSI_ReferenceNumberInfo));
		}

		#endregion

		#region Bill

		public void TestValidateCSI_ReferenceNumber_InvalidFormatForTypeN741()
		{
			SetUpTestData();
			var referenceFormatForCodeN741Error = "[BR2038] Reference Number should contain 11 digits when Type is 'N741'.";
			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_Code = "N741";
				document.CSI_ReferenceNumber = "12345";
				AssertHasMessageError(document.CSI_ReferenceNumberInfo, referenceFormatForCodeN741Error);

				document.CSI_ReferenceNumber = "ABCdefghijk";
				AssertHasMessageError(document.CSI_ReferenceNumberInfo, referenceFormatForCodeN741Error);

				document.CSI_ReferenceNumber = "12345678901";
				AssertNoMessageError(document.CSI_ReferenceNumberInfo, referenceFormatForCodeN741Error);

				document.CSI_Code = "1234";
				document.CSI_ReferenceNumber = "12345";
				AssertNoMessageError(document.CSI_ReferenceNumberInfo, referenceFormatForCodeN741Error);
			});
		}

		public void TestValidateCSI_ReferenceNumber_EmptyCheck()
		{
			SetUpTestData();
			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_ReferenceNumber = string.Empty;
				AssertHasMessageError(document.CSI_ReferenceNumberInfo, "[BR20313] You have not entered a Reference Number.");

				document.CSI_ReferenceNumber = "ABC";
				AssertNoMessageError(document.CSI_ReferenceNumberInfo, "[BR20313] You have not entered a Reference Number.");
			});
		}

		public void TestValidateCSI_ReferenceNumber_InvalidReferenceForCode1D24_V1()
		{
			AssertReferenceNumberFormat("1D24", "123456", "202503060304", "[BR20319] Reference must be in format 'yyyyMMddHHmm' when Supporting Document Type is '1D24'.");
		}

		public void TestValidateCSI_ReferenceNumber_InvalidReferenceForCode1D96_V1()
		{
			AssertReferenceNumberFormat("1D96", "2", "1", "[BR2316] Invalid Reference Number. It should be '1' when Type is '1D96'.");
		}

		void AssertReferenceNumberFormat(string code, string incorrectReferenceNumber, string correctReferenceNumber, string errorMessage)
		{
			SetUpTestData();
			header.AMA_ApplicationCode = "LV1";
			document.CSI_Code = code;
			document.CSI_ReferenceNumber = incorrectReferenceNumber;
			AssertHasMessageError(document.CSI_ReferenceNumberInfo, errorMessage);

			document.CSI_Code = "1234";
			document.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"apply only to Code {code}", document.CSI_ReferenceNumberInfo, errorMessage);

			document.CSI_Code = code;
			AssertApplyOnlyToV1(errorMessage, true);

			document.CSI_ReferenceNumber = correctReferenceNumber;
			AssertNoMessageError(document.CSI_ReferenceNumberInfo, errorMessage);
		}

		#endregion

		#endregion

		void AssertApplyToBothV1AndV2(Action validateErrorCase)
		{
			header.AMA_ApplicationCode = "LV1";
			validateErrorCase();

			header.AMA_ApplicationCode = "LV2";
			validateErrorCase();
		}

		void AssertApplyOnlyToV1(string errorMessage, bool isReference = false)
		{
			header.AMA_ApplicationCode = "LV2";
			if (isReference)
			{
				document.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("apply only to LV1", document.CSI_ReferenceNumberInfo, errorMessage);
			}
			else
			{
				document.Validation.ValidateCSI_Code();
				AssertNoMessageError("apply only to LV1", document.CSI_CodeInfo, errorMessage);
			}

			header.AMA_ApplicationCode = "LV1";
		}

		void SetUpTestData()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			document = bill.SupportingDocuments.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		SupportingDocument document;
	}
}
