using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingCondition))]
	sealed class GuidedDecisionMakingConditionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsSatisfied()
		{
			var condition = new GuidedDecisionMakingCondition();

			var conditionDetail1_1 = condition.ConditionDetails.AddNew();
			conditionDetail1_1.LogicalGroup = 1;
			conditionDetail1_1.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
			var conditionDetail1_2 = condition.ConditionDetails.AddNew();
			conditionDetail1_2.LogicalGroup = 1;
			conditionDetail1_2.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;

			var conditionDetail2_1 = condition.ConditionDetails.AddNew();
			conditionDetail2_1.LogicalGroup = 2;
			conditionDetail2_1.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;
			var conditionDetail2_2 = condition.ConditionDetails.AddNew();
			conditionDetail2_2.LogicalGroup = 2;
			conditionDetail2_2.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;

			CombineAssertions("Condition should not be satisifed because none of its logical group is satisfied", () =>
			{
				AssertEquals("Precondition: default conditionDetail1", false, conditionDetail1_1.IsSatisfied);
				AssertEquals("Precondition: default conditionDetail1", false, conditionDetail1_1.IsTicked);
				AssertEquals("Precondition: default conditionDetail2", false, conditionDetail1_2.IsSatisfied);
				AssertEquals("Precondition: default conditionDetail1", false, conditionDetail1_2.IsTicked);
				AssertEquals("Precondition: default conditionDetail1", false, conditionDetail2_1.IsSatisfied);
				AssertEquals("Precondition: default conditionDetail1", false, conditionDetail2_1.IsTicked);
				AssertEquals("Precondition: default conditionDetail2", false, conditionDetail2_2.IsSatisfied);
				AssertEquals("Precondition: default conditionDetail2", false, conditionDetail2_2.IsTicked);
				AssertEquals("Condition should not be satisfied", false, condition.IsSatisfied);
			});

			conditionDetail1_1.IsTicked = true;
			CombineAssertions("Condition should not be satisifed when one of its logical group is not satisfied", () =>
			{
				AssertEquals("One of condition details within first logical group should be satisfied", true, condition.ConditionDetails.Where(x => x.LogicalGroup == 1).Any(x => x.IsSatisfied));
				AssertEquals("None of condition details within second logical group should be satisfied", false, condition.ConditionDetails.Where(x => x.LogicalGroup == 2).Any(x => x.IsSatisfied));
				AssertEquals("Condition should not be satisifed when one of its logical group is not satisfied", false, condition.IsSatisfied);
			});

			conditionDetail2_1.IsTicked = true;
			CombineAssertions("Condition should be satisifed when all of its logical group is satisfied", () =>
			{
				AssertEquals("One of condition details within first logical group should be satisfied", true, condition.ConditionDetails.Where(x => x.LogicalGroup == 1).Any(x => x.IsSatisfied));
				AssertEquals("One of condition details within second logical group should be satisfied", true, condition.ConditionDetails.Where(x => x.LogicalGroup == 2).Any(x => x.IsSatisfied));
				AssertEquals("Condition should be satisifed when all of its logical group is satisfied", true, condition.IsSatisfied);
			});
		}

		public void TestIsSatisfied_BySameConditionalDetail()
		{
			var condition1 = new GuidedDecisionMakingCondition();
			var condition2 = new GuidedDecisionMakingCondition();
			var conditionDetail = new GuidedDecisionMakingConditionDetail();
			condition1.ConditionDetails.Add(conditionDetail);
			condition2.ConditionDetails.Add(conditionDetail);
			conditionDetail.Type = Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber;

			CombineAssertions(() =>
			{
				Assert("condition1 is the parent of conditionDetail", conditionDetail.Parents.Contains(condition1));
				Assert("condition2 is the parent of conditionDetail", conditionDetail.Parents.Contains(condition2));
				conditionDetail.IsTicked = false;
				Assert("conditionDetail should be unsatisfied", !conditionDetail.IsSatisfied);
				Assert("condition1 should be unsatisfied", !condition1.IsSatisfied);
				Assert("condition2 should be unsatisfied", !condition2.IsSatisfied);

				conditionDetail.IsTicked = true;
				Assert("conditionDetail should be satisfied", conditionDetail.IsSatisfied);
				Assert("condition1 should be satisfied", condition1.IsSatisfied);
				Assert("condition2 should be satisfied", condition2.IsSatisfied);
			});
		}

		public void TestConditionDescription()
		{
			var condition = (GuidedDecisionMakingCondition)GetNewBusinessObject();
			condition.ConditionType = "RAT1";
			condition.ConditionTypeDescription = "Test Rate Condition Type";
			condition.ConditionSatisfactionType = "C1";
			condition.InformationValue = ZString.Empty;

			var expectedDescriptionText = @"RAT1 - Test Rate Condition Type
Description: C1";

			AssertEquals("DocumentCondition.ConditionDescription should be correctly displayed.", expectedDescriptionText, condition.ConditionDescription);

			condition.InformationValue = "ABC";

			expectedDescriptionText = @"RAT1 - Test Rate Condition Type
Description: C1
Information: ABC";

			AssertEquals("DocumentCondition.ConditionDescription should be correctly displayed.", expectedDescriptionText, condition.ConditionDescription);
		}

		public void TestIsSatisfiedTextForBinding()
		{
			var condition = (GuidedDecisionMakingCondition)GetNewBusinessObject();
			condition.InformationValue = ZString.Empty;
			var conditionDetail = new Mock<GuidedDecisionMakingConditionDetail>(condition);
			conditionDetail.Setup(d => d.IsSatisfied).Returns(false);
			condition.ConditionDetails.Add(conditionDetail.Object);

			Assert("Prerequisite: IsSatisfied should be false as inner conditionDetail.IsSatisfied is false.", !condition.IsSatisfied);
			AssertEquals("IsSatisfiedTextForBinding should be 'NO' as the condition is not satisfied and not InformationCondition.", "NO", condition.IsSatisfiedTextForBinding);

			conditionDetail.Setup(d => d.IsSatisfied).Returns(true);
			Assert("Prerequisite: IsSatisfied should be true as inner conditionDetail.IsSatisfied is true.", condition.IsSatisfied);
			AssertEquals("IsSatisfiedTextForBinding should be 'YES' as the condition is not satisfied and not InformationCondition.", "YES", condition.IsSatisfiedTextForBinding);

			condition.InformationValue = "INF";
			conditionDetail.Setup(d => d.IsSatisfied).Returns(false);
			Assert("Prerequisite: IsSatisfied should be false as inner conditionDetail.IsSatisfied is false.", !condition.IsSatisfied);
			AssertEquals("IsSatisfiedTextForBinding should be 'Optional' as the condition is InformationCondition.", "Optional", condition.IsSatisfiedTextForBinding);

			conditionDetail.Setup(d => d.IsSatisfied).Returns(true);
			Assert("Prerequisite: IsSatisfied should be true as inner conditionDetail.IsSatisfied is true.", condition.IsSatisfied);
			AssertEquals("IsSatisfiedTextForBinding should be 'Optional' as the condition is InformationCondition.", "Optional", condition.IsSatisfiedTextForBinding);
		}

		public void TestFieldsMaxLength()
		{
			var condition = (GuidedDecisionMakingCondition)GetNewBusinessObject();
			AssertEquals("ConditionTypeMaxLength: ", 6, condition.ConditionTypeInfo.MaxLength);
			AssertEquals("ConditionTypeDescriptionMaxLength: ", 500, condition.ConditionTypeDescriptionInfo.MaxLength);
			AssertEquals("ConditionSatisfactionTypeMaxLength: ", 4000, condition.ConditionSatisfactionTypeInfo.MaxLength);
			AssertEquals("InformationValueMaxLength: ", 500, condition.InformationValueInfo.MaxLength);
			AssertEquals("IsSatisfiedTextForBindingMaxLength: ", 15, condition.IsSatisfiedTextForBindingInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject() => new GuidedDecisionMakingCondition();
	}
}
