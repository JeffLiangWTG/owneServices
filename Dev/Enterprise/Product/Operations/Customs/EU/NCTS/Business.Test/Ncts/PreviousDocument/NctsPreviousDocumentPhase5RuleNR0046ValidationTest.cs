using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentPhase5RuleNR0046ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsPreviousDocumentPhase5RuleNR0046Validation(null));
		}

		public void TestCheckCSI_ReferenceNumber_Length18Characters() => CombineAssertions(() =>
		{
			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			validationDeciderTestContext.EnableRule(decider => decider.IsRuleNR0046Active);

			const string expectedMessageError = "[NR0046] This type of document must contain an MRN Number. Length must be 18 characters.";

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N822;
			previousDocument.CSI_ReferenceNumber = "24NO3650013BYLQKJ2";
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830], content is a valid MRN", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = "12345678901234567";
			AssertHasMessageError("Type is in the list [ N820, N821, N822, N830], length is Less than 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = "1234567890123456789";
			AssertHasMessageError("Type is in the list [ N820, N821, N822, N830], length is more than 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = "1 3456789012345678";
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830], length is 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830], When empty", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C651;
			previousDocument.CSI_ReferenceNumber = "12345678901234567";
			AssertNoMessageError("Type is not in the list [ N820, N821, N822, N830], Length is not 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			validationDeciderTestContext.DisableRule(decider => decider.IsRuleNR0046Active);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = "12345678901234567";
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830], Length is not 18", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
		});

		public void TestCheckCSI_ReferenceNumber_NoSpacesAllowed() => CombineAssertions(() =>
		{
			using var validationDeciderTestContext = new NctsPreviousDocumentValidationDeciderTestContext<INctsPreviousDocumentDeparturePhase5ValidationDecider>(Factory);
			validationDeciderTestContext.EnableRule(decider => decider.IsRuleNR0046Active);

			const string expectedMessageError = "[NR0046] This type of document must contain an MRN Number. Spaces are not allowed.";
			const string referenceNumberContainsSpaces = "1 3456789012345678";

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N821;
			previousDocument.CSI_ReferenceNumber = "24NO3650013BYLQKJ2";
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830], content is a valid MRN", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
			AssertHasMessageError("Type is in the list [ N820, N821, N822, N830], Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = "1234567890123456";
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830],Not Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C651;
			previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
			AssertNoMessageError("Type is not in the list [ N820, N821, N822, N830], Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			validationDeciderTestContext.DisableRule(decider => decider.IsRuleNR0046Active);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
			AssertNoMessageError("Type is in the list [ N820, N821, N822, N830], Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
		});

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}

		NctsPreviousDocument previousDocument;
	}
}
