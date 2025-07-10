using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CommonPreviousDocumentRuleNR0048ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CommonPreviousDocumentRuleNR0048Validation(null));
		}

		public void TestCheckCSI_ReferenceNumber_Length18Characters()
		{
			const string expectedMessageError = "[NR0048] This type of document must contain an MRN Number. Length must be 18 characters.";
			var previousDocument = CreateCommonPreviousDocument();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions("NR0048 is Active", () =>
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0048Active));
					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					previousDocument.CSI_ReferenceNumber = "24NO3650013BYLQKJ2";
					AssertNoMessageError("Type is in N830, content is a valid MRN", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_ReferenceNumber = "12345678901234567";
					AssertHasMessageError("Type is N830, length is Less than 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_ReferenceNumber = "1234567890123456789";
					AssertHasMessageError("Type is N830, length is more than 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_ReferenceNumber = "1 3456789012345678";
					AssertNoMessageError("Type is N830, length is 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_ReferenceNumber = ZString.Empty;
					AssertNoMessageError("Type is N830, When empty", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N822;
					previousDocument.CSI_ReferenceNumber = "12345678901234567";
					AssertNoMessageError("Type is not N830, Length is not 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
				});

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleNR0048Active));
				previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				previousDocument.CSI_ReferenceNumber = "12345678901234567";
				AssertNoMessageError("Type is N830, Length is not 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
			}
		}

		public void TestCheckCSI_ReferenceNumber_NoSpacesAllowed()
		{
			const string expectedMessageError = "[NR0048] This type of document must contain an MRN Number. Spaces are not allowed.";
			const string referenceNumberContainsSpaces = "1 3456789012345678";
			var previousDocument = CreateCommonPreviousDocument();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				CombineAssertions("NR0048 is Active", () =>
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleNR0048Active));
					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;

					previousDocument.CSI_ReferenceNumber = "24NO3650013BYLQKJ2";
					AssertNoMessageError("Type is in the list N830, content is a valid MRN", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
					AssertHasMessageError("Type is N830, Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_ReferenceNumber = "1234567890123456";
					AssertNoMessageError("Type is N830,Not Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N822;
					previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
					AssertNoMessageError("Type is not N830, Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
				});

				ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleNR0048Active));
				previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
				AssertNoMessageError("Type is N830, Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
			}
		}

		CommonPreviousDocument CreateCommonPreviousDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			return bill.PreviousDocuments.AddNew();
		}
	}
}
