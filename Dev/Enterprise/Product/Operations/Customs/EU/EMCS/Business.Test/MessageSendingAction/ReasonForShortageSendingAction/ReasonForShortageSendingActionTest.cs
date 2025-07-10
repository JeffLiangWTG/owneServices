using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReasonForShortageSendingAction))]
	sealed class ReasonForShortageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGeneralExplanation()
		{
			messageSendingAction.GeneralExplanation = "Test Explanation";
			AssertEquals("General Explanation", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.GeneralExplanationInfo).Caption);
			AssertEquals("Test Explanation", messageSendingAction.GeneralExplanation);
		}

		public void TestValidation()
		{
			AssertType<ReasonForShortageSendingActionValidation>(messageSendingAction.Validation);
		}

		public void TestMaxLength()
		{
			AssertEquals(350, messageSendingAction.GeneralExplanationInfo.MaxLength);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Reason for Shortage", messageSendingAction.HumanReadableName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			messageSendingAction = new ReasonForShortageSendingAction(jobDeclaration);
		}

		ReasonForShortageSendingAction messageSendingAction;

		protected override BusinessObject GetNewBusinessObject() => messageSendingAction;
	}
}
