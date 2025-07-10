using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ExplanationOnDelaySendingAction))]
	sealed class ExplanationOnDelaySendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestExplanationCode()
		{
			messageSendingAction.ExplanationCode = "0";
			AssertEquals("Explanation Code", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.ExplanationCodeInfo).Caption);
			AssertEquals("0", messageSendingAction.ExplanationCode);
		}

		public void TestInformation()
		{
			messageSendingAction.Information = "TEST INFORMATION";
			AssertEquals("Information", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.InformationInfo).Caption);
			AssertEquals("TEST INFORMATION", messageSendingAction.Information);
		}

		public void TestMessageRole()
		{
			messageSendingAction.MessageRole = "1";
			AssertEquals("Message Role", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.MessageRoleInfo).Caption);
			AssertEquals("1", messageSendingAction.MessageRole);
		}

		public void TestLookups()
		{
			AssertType<ExplanationOnDelaySendingActionLookups>(messageSendingAction.Lookups);
		}

		public void TestValidation()
		{
			AssertType<ExplanationOnDelaySendingActionValidation>(messageSendingAction.Validation);
		}

		public void TestMaxLength()
		{
			AssertEquals(1, messageSendingAction.ExplanationCodeInfo.MaxLength);
			AssertEquals(350, messageSendingAction.InformationInfo.MaxLength);
			AssertEquals(1, messageSendingAction.MessageRoleInfo.MaxLength);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Explanation On Delay", messageSendingAction.HumanReadableName);
		}

		ExplanationOnDelaySendingAction messageSendingAction;
		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			messageSendingAction = new ExplanationOnDelaySendingAction(jobDeclaration);
		}

		protected override BusinessObject GetNewBusinessObject() => messageSendingAction;
	}
}
