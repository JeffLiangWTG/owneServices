using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(CancellationSendingAction))]
	sealed class CancellationSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReason()
		{
			AssertEquals("Reason", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.ReasonInfo).Caption);
			AssertEquals("2", messageSendingAction.Reason);
		}

		public void TestInformation()
		{
			AssertEquals("Information", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.InformationInfo).Caption);
			AssertEquals("Information Text", messageSendingAction.Information);
		}

		public void TestLookups()
		{
			AssertType<CancellationSendingActionLookups>(messageSendingAction.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CancellationSendingActionValidation>(messageSendingAction.Validation);
		}

		public void TestMaxLength()
		{
			AssertEquals(3, messageSendingAction.ReasonInfo.MaxLength);
			AssertEquals(350, messageSendingAction.InformationInfo.MaxLength);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Cancellation of EAD", messageSendingAction.HumanReadableName);
		}

		CancellationSendingAction messageSendingAction;
		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			messageSendingAction = new CancellationSendingAction(jobDeclaration);
			messageSendingAction.Reason = "2";
			messageSendingAction.Information = "Information Text";
		}

		protected override BusinessObject GetNewBusinessObject() => messageSendingAction;
	}
}
