using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(DocumentsSendingAction))]
	class DocumentsSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessagingObject()
		{
			AssertEquals(exitReport, documentsSendingAction.MessagingObject);
		}

		public void TestMovementReference()
		{
			var consignment = Factory.New<CusExitConsignment>();
			consignment.CXC_MovementReference = "MRN001";
			exitReport.CER_CXC_Consignment = consignment.PK;
			AssertEquals("MovementReference", "MRN001", documentsSendingAction.MovementReference);
		}

		public void TestMovementReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(documentsSendingAction.MovementReferenceInfo);
			AssertEquals("MovementReferenceInfo caption", "Movement Reference Number", resData.Caption);
			AssertEquals("MovementReferenceInfo ShortCaption", "MRN", resData.ShortCaption);
		}

		public void TestLocalReference()
		{
			var consignment = Factory.New<CusExitConsignment>();
			consignment.CXC_UniqueConsignmentReference = "LRN001";
			exitReport.CER_CXC_Consignment = consignment.PK;
			AssertEquals("LRN001", documentsSendingAction.LocalReference);
		}

		public void TestLocalReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(documentsSendingAction.LocalReferenceInfo);
			AssertEquals("LocalReferenceInfo caption", "Local Reference Number", resData.Caption);
			AssertEquals("LocalReferenceInfo ShortCaption", "LRN", resData.ShortCaption);
		}

		public void TestLookups()
		{
			AssertType("Should have created a correct Lookups.", typeof(DocumentsSendingActionLookups), documentsSendingAction.Lookups);
		}

		public void TestSenderType()
		{
			AssertType("Should have defined a correct SenderType.", typeof(DocumentsSender), documentsSendingAction.CreateSender());
		}

		public void TestValidationType()
		{
			AssertType("Should have created a correct Validation.", typeof(DocumentsSendingActionValidation), documentsSendingAction.Validation);
		}

		public void TestMessagePreview()
		{
			AssertEquals("Message text", documentsSendingAction.MessageCreated("Message text"));
			documentsSendingAction.PreviewMessage += OnMessageCreated;
			AssertEquals("Tested: Message text", documentsSendingAction.MessageCreated("Message text"));

			void OnMessageCreated(MessageEventArgs args)
			{
				args.MessageText = "Tested: " + args.MessageText;
			}
		}

		protected override BusinessObject GetNewBusinessObject() => documentsSendingAction;

		protected override void SetUp()
		{
			base.SetUp();
			exitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			documentsSendingAction = new DocumentsSendingAction(exitReport);
		}

		CusExitReport exitReport;
		DocumentsSendingAction documentsSendingAction;
	}
}
