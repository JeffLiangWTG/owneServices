using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingConditionDetail))]
	sealed class GuidedDecisionMakingConditionDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsSatisfied()
		{
			var conditionDetail = new GuidedDecisionMakingConditionDetail();

			CombineAssertions(() =>
			{
				Assert("ConditionDetail.IsSatisfied should be false by default.", !conditionDetail.IsSatisfied);

				conditionDetail.IsTicked = true;
				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				Assert("If ticked, ConditionDetail.IsSatisfied should be true when the Type is SupportingDocumentNoReferenceNumber.", conditionDetail.IsSatisfied);

				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				Assert("If ticked, ConditionDetail.IsSatisfied should be false when the Type is SupportingDocument but no reference text is entered.", !conditionDetail.IsSatisfied);
				conditionDetail.Reference = "test";
				Assert("If ticked, ConditionDetail.IsSatisfied should be true when the Type is SupportingDocument and reference text is entered.", conditionDetail.IsSatisfied);

				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.PresentationOfSupportingDoc;
				Assert("If ticked, ConditionDetail.IsSatisfied should be false when the Type is PresentationOfSupportingDoc.", !conditionDetail.IsSatisfied);

				conditionDetail.IsTicked = false;
				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				Assert("If not ticked, ConditionDetail.IsSatisfied should be false.", !conditionDetail.IsSatisfied);
			});
		}

		public void TestReferenceAndDateOfIssueReadOnly()
		{
			var conditionDetail = new GuidedDecisionMakingConditionDetail();

			CombineAssertions(() =>
			{
				var referenceInfo = conditionDetail.ReferenceInfo;
				var dateOfIssueInfo = conditionDetail.DateOfIssueInfo;
				AssertEquals("default", true, referenceInfo.ReadOnly);
				AssertEquals("default", true, dateOfIssueInfo.ReadOnly);

				conditionDetail.IsTicked = true;
				AssertEquals("SupportingDocument type and ticked", false, referenceInfo.ReadOnly);
				AssertEquals("SupportingDocument type and ticked", false, dateOfIssueInfo.ReadOnly);

				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
				AssertEquals("SupportingDocumentNoReferenceNumber type", true, referenceInfo.ReadOnly);
				AssertEquals("SupportingDocumentNoReferenceNumber type", true, dateOfIssueInfo.ReadOnly);

				conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument;
				AssertEquals("SupportingDocument type and ticked", false, referenceInfo.ReadOnly);
				AssertEquals("SupportingDocument type and ticked", false, dateOfIssueInfo.ReadOnly);
			});
		}

		public void TestCodeReadOnly()
		{
			var conditionDetail = new GuidedDecisionMakingConditionDetail();
			AssertEquals("default", true, conditionDetail.CodeInfo.ReadOnly);
		}

		public void TestFieldsMaxLength()
		{
			var conditionDetail = (GuidedDecisionMakingConditionDetail)GetNewBusinessObject();
			AssertEquals("CodeMaxLength: ", 15, conditionDetail.CodeInfo.MaxLength);
			AssertEquals("ReferenceMaxLength: ", 100, conditionDetail.ReferenceInfo.MaxLength);
			AssertEquals("TypeMaxLength: ", 6, conditionDetail.TypeInfo.MaxLength);
			AssertEquals("TypeDescriptionMaxLength: ", 500, conditionDetail.TypeDescriptionInfo.MaxLength);
		}

		public void TestLookups()
		{
			var conditionDetail = (GuidedDecisionMakingConditionDetail)GetNewBusinessObject();
			AssertType<GuidedDecisionMakingConditionDetailLookups>(conditionDetail.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => new GuidedDecisionMakingConditionDetail();
	}
}
