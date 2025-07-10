using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class SendsMessagesToCustomsGUITest : TestCaseWithFactory
	{
		public void TestShouldSendCustomsMessagesForDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_BGMReference = "TEST_1";
			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_BGMReference = "TEST_2";

			IMessageManager messageManager = new DeclarationMultiMessageManager(new ImportLicenseMessageSendingObjectParent(declaration));
			var savingOptions = messageManager.GetDeferredAmendmentSavingOptions();
			var sendsMessages = new SendsMessagesToCustomsGUI();
			var info = messageManager.GetRequiredMessagesInformation();
			info.AddAmendmentBridgeForTesting(new[] { new ImportLicenseMessageManager(new ImportLicenseMessageSendingObject(entryHeader1)), new ImportLicenseMessageManager(new ImportLicenseMessageSendingObject(entryHeader2)) });
			var result = sendsMessages.ShouldSendCustomsMessages(info, savingOptions, messageManager);
			var notification = UnitTestUserNotification.Instance.LastMessage.Text;
			Assert("ShouldSendCustomsMessages should always be false when ImportLicenseMultiMessageManager", !result);
			AssertEquals("ShouldSendCustomsMessages should always be false when ImportLicenseMultiMessageManager",
				"The following Entry Header(s) already contains an Import License number and cannot be edited.\r\n • TEST_1\r\n • TEST_2", notification);

			UnitTestUserNotification.Instance.ClearMessages();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			messageManager = new DeclarationMultiMessageManager(new ExportJobDeclarationMessageSendingObjectParent(declaration));
			savingOptions = messageManager.GetDeferredAmendmentSavingOptions();
			sendsMessages = new SendsMessagesToCustomsGUI();
			info = messageManager.GetRequiredMessagesInformation();
			sendsMessages.ShouldSendCustomsMessages(info, savingOptions, messageManager);
			notification = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNull(notification);
		}

		public void TestShouldSendCustomsMessagesForGoodsCatalog()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();

			var messageManager = new GoodsCatalogMultiMessageManager(new GoodsCatalogMessageSendingObject(goodsCatalog)) as IMessageManager;
			var sendsMessages = new SendsMessagesToCustomsGUI();
			sendsMessages.ShouldSendCustomsMessages(messageManager.GetRequiredMessagesInformation(), messageManager.GetDeferredAmendmentSavingOptions(), messageManager);
			var notification = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNull(notification);
		}

		public void TestDoActionsBeforeSendingRequiredMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "TEST_1";

			var messageManager = (declaration as IMessageManageableBizObj).GetMessageManagerForAmendmentDetection();
			var sendsMessages = new SendsMessagesToCustomsGUIForTesting();
			var exportDeclarationSendingObj = new ExportDeclarationMessageSendingObject(entryHeader);

			var info = messageManager.GetRequiredMessagesInformation();
			info.AddAmendmentBridgeForTesting(new[] { new ExportDeclarationMessageManager(exportDeclarationSendingObj) });
			info.AmendmentWithdrawalReason.IsCancelled = true;
			info.AmendmentWithdrawalReason.ReasonText = "TEST_REASON";
			AssertEquals("Response should be No", ContinueWithSave.No, sendsMessages.DoActionsBeforeSendingRequiredMessages_Exposed(messageManager, info));
			Assert("VOCReason should be Empty", exportDeclarationSendingObj.VOCReason.IsEmpty);

			info.AmendmentWithdrawalReason.IsCancelled = false;
			info.AmendmentWithdrawalReason.ReasonText = "TEST_REASON";
			AssertEquals("Response should be Yes", ContinueWithSave.Yes, sendsMessages.DoActionsBeforeSendingRequiredMessages_Exposed(messageManager, info));
			AssertEquals("VOCReason should be ", "TEST_REASON", exportDeclarationSendingObj.VOCReason);
		}

		public void TestGetBackDoorForSavingForm()
		{
			using (var form = new SendsMessagesToCustomsGUIForTesting().GetBackDoorForSavingFormForTesting(new RequiredMessagesInformation(Factory.New<JobDeclaration>()), new DeclarationDeferredAmendmentSavingOptions(Factory.NewWithValidTestData<JobDeclaration>())))
			{
				AssertType<BackdoorForSavingOnAmendmentForm>(form);
			}
			using (var form = new SendsMessagesToCustomsGUIForTesting().GetBackDoorForSavingFormForTesting(new RequiredMessagesInformation(Factory.New<JobDeclaration>()), new DeferredAmendmentSavingOptions()))
			{
				AssertType<Customs.GUI.BackdoorForSavingOnAmendmentForm>(form);
			}
			using (var form = new SendsMessagesToCustomsGUIForTesting().GetBackDoorForSavingFormForTesting(new RequiredMessagesInformation(Factory.New<CusGoodsCatalog>()), new CatalogDeferredAmendmentSavingOptions()))
			{
				AssertType<CatalogBackdoorForSavingOnAmendmentForm>(form);
			}
		}

		sealed class SendsMessagesToCustomsGUIForTesting : SendsMessagesToCustomsGUI
		{
			public ContinueWithSave DoActionsBeforeSendingRequiredMessages_Exposed(IMessageManager manager, RequiredMessagesInformation detectionResult)
			{
				return DoActionsBeforeSendingRequiredMessages(manager, detectionResult);
			}

			public ZForm GetBackDoorForSavingFormForTesting(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions) => GetBackDoorForSavingForm(detectionResult, savingOptions);
		}
	}
}
