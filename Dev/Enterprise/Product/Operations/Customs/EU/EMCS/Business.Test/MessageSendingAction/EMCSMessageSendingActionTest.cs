using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseMessageSendingObject;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSMessageSendingAction))]
	sealed class EMCSMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessagePreview()
		{
			AssertEquals("Message text", messageSendingAction.MessageCreated("Message text"));
			messageSendingAction.PreviewMessage += new MessagePreviewEventHandler(OnMessageCreated);
			AssertEquals("Tested: Message text", messageSendingAction.MessageCreated("Message text"));

			void OnMessageCreated(MessageEventArgs args)
			{
				args.MessageText = "Tested: " + args.MessageText;
			}
		}

		public void TestEADNumber_Caption()
		{
			AssertEquals("EADNumberInfo should have correct Caption", "EAD Number", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.EADNumberInfo).Caption);
		}

		public void TestRegistrationStatus_Caption()
		{
			AssertEquals("RegistrationStatusInfo should have correct Caption", "Registration Status", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.RegistrationStatusInfo).Caption);
		}

		public void TestDeclarant_Caption()
		{
			AssertEquals("DeclarantTypeInfo should have correct Caption", "Declarant Type", DataBoundResourceStrings.GetDataForProperty(messageSendingAction.DeclarantTypeInfo).Caption);
		}

		EMCSMessageSendingAction messageSendingAction;
		protected override void SetUp()
		{
			base.SetUp();
			var jobDeclaration = Factory.New<EMCSJobDeclaration>();
			messageSendingAction = new EMCSMessageSendingAction(jobDeclaration);
		}

		protected override BusinessObject GetNewBusinessObject() => messageSendingAction;
	}
}
