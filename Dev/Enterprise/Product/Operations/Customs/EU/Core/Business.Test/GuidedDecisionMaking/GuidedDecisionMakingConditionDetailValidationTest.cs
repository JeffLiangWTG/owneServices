using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingConditionDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDateOfIssue()
		{
			var conditionDetail = new GuidedDecisionMakingConditionDetail();
			var warningMessage = "Please enter a Date of Issue.";

			CombineAssertions(() =>
			{
				var info = conditionDetail.DateOfIssueInfo;
				AssertNoWarningContaining("default", info, warningMessage);

				conditionDetail.IsTicked = true;
				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				AssertNoWarningContaining("SupportingDocumentNoReferenceNumber type", info, warningMessage);

				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				AssertHasWarningContaining("SupportingDocument type with no date of issue", info, warningMessage);

				conditionDetail.IsTicked = false;
				AssertNoWarningContaining("SupportingDocument type with no date of issue but not ticked", info, warningMessage);

				conditionDetail.IsTicked = true;
				AssertHasWarningContaining("SupportingDocument type with no date of issue", info, warningMessage);

				conditionDetail.DateOfIssue = new ZDateTime(2023, 06, 22);
				AssertNoWarningContaining("SupportingDocument type with date of issue and ticked", info, warningMessage);

				conditionDetail.DateOfIssue = ZDateTime.Empty;
				AssertHasWarningContaining("SupportingDocument type with no date of issue", info, warningMessage);
			});
		}

		public void TestCheckReference()
		{
			var conditionDetail = new GuidedDecisionMakingConditionDetail();
			var warningMessage = "Please enter a Reference.";

			CombineAssertions(() =>
			{
				var info = conditionDetail.ReferenceInfo;
				AssertNoNotifications("default", info);

				conditionDetail.IsTicked = true;
				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				AssertNoNotifications("SupportingDocumentNoReferenceNumber type", info);

				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				AssertHasWarningContaining("SupportingDocument type with no reference", info, warningMessage);

				conditionDetail.IsTicked = false;
				AssertNoNotifications("SupportingDocument type with no reference but not ticked", info);

				conditionDetail.IsTicked = true;
				AssertHasWarningContaining("SupportingDocument type with no reference", info, warningMessage);

				conditionDetail.Reference = "test";
				AssertNoNotifications("SupportingDocument type with reference and ticked", info);

				conditionDetail.Reference = "";
				AssertHasWarningContaining("SupportingDocument type with no reference", info, warningMessage);
			});
		}
	}
}
