using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(CusEntryHeaderMessageSendingAction))]
	public abstract class CusEntryHeaderMessageSendingActionTest<TSendingAction> : NonPersistentBusinessObjectTestCase
		where TSendingAction : CusEntryHeaderMessageSendingAction
	{
		protected abstract Type ExpectedLookupsType { get; }

		protected abstract Type ExpectedSenderType { get; }

		protected abstract Type ExpectedValidationType { get; }

		public void TestLookups()
		{
			AssertType("Should have created a correct Lookups.", ExpectedLookupsType, testSendingAction.Lookups);
		}

		public void TestSenderType()
		{
			AssertType("Should have defined a correct SenderType.", ExpectedSenderType, testSendingAction.CreateSender());
		}

		public void TestValidationType()
		{
			AssertType("Should have created a correct Validation.", ExpectedValidationType, testSendingAction.Validation);
		}

		public void TestMessagePreview()
		{
			AssertEquals("Message text", testSendingAction.MessageCreated("Message text"));
			testSendingAction.PreviewMessage += OnMessageCreated;
			AssertEquals("Tested: Message text", testSendingAction.MessageCreated("Message text"));

			void OnMessageCreated(MessageEventArgs args)
			{
				args.MessageText = "Tested: " + args.MessageText;
			}
		}

		protected TSendingAction testSendingAction;

		protected override void SetUp()
		{
			base.SetUp();
			var cusEntryHeader = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			testSendingAction = (TSendingAction)Activator.CreateInstance(typeof(TSendingAction), cusEntryHeader);
		}
	}
}
