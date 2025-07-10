using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocumentValidation))]
	sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCSI_Code_PackedItem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var previousDoc = packedItem.PreviousDocuments.AddNew();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "DC40I");
			helper.CreateCusCodeList(previousDoc.DataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			Factory.Save();

			previousDoc.CSI_Code = "AAA";
			AssertHasMessageError("Should error on invalid code", previousDoc.CSI_CodeInfo, previousDoc.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);

			previousDoc.CSI_ReferenceNumber = "Test";
			previousDoc.CSI_Code = ZString.Empty;
			AssertHasMessageError("Should error on empty code when reference exists", previousDoc.CSI_CodeInfo, previousDoc.ValidationConfiguration.ValidationMessage.GetBR2010RuleMessage(previousDoc.CSI_CodeInfo));

			previousDoc.CSI_Code = "ABC";
			AssertNoMessageErrors(previousDoc.CSI_CodeInfo);
		}

		public void TestValidateCSI_Code_Bill()
		{
			SetUpTestData();
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUCC6V1, "DC40I", "1111", "1111", yesterday, tomorrow);
			Factory.Save();

			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_Code = "2222";
				AssertHasMessageError(document.CSI_CodeInfo, "[BR0020] The code you have selected is not in the list.");

				document.CSI_Code = "1111";
				AssertNoMessageError(document.CSI_CodeInfo, "[BR0020] The code you have selected is not in the list.");
				AssertNoMessageError(document.CSI_CodeInfo, "[BR2010] You have not entered a Previous Document type.");

				document.CSI_Code = string.Empty;
				AssertHasMessageError(document.CSI_CodeInfo, "[BR2010] You have not entered a Previous Document type.");
			});
		}

		public void TestValidateCSI_ReferenceNumber_PackedItem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var previousDoc = packedItem.PreviousDocuments.AddNew();

			previousDoc.CSI_Code = "1111";
			previousDoc.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageError("Should error on empty reference when code exists", previousDoc.CSI_ReferenceNumberInfo, previousDoc.ValidationConfiguration.ValidationMessage.GetBR2010RuleMessage(previousDoc.CSI_ReferenceNumberInfo));

			previousDoc.CSI_ReferenceNumber = "Test";
			AssertNoMessageErrors(previousDoc.CSI_ReferenceNumberInfo);
		}

		public void TestValidateCSI_ReferenceNumber_Bill()
		{
			SetUpTestData();
			AssertApplyToBothV1AndV2(() =>
			{
				document.CSI_ReferenceNumber = string.Empty;
				AssertHasMessageError(document.CSI_ReferenceNumberInfo, "[BR2010] You have not entered a Previous Document reference number.");

				document.CSI_ReferenceNumber = "1234";
				AssertNoMessageError(document.CSI_ReferenceNumberInfo, "[BR2010] You have not entered a Previous Document reference number.");
			});
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
			document = bill.PreviousDocuments.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		PreviousDocument document;
	}
}
