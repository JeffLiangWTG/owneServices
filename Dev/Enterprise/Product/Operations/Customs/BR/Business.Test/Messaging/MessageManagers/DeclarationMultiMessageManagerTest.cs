using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DeclarationMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetAllMessageManagers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.EntryNumber = ZString.Empty;
			header.CH_EntryStatus = ZString.Empty;
			var messageSendingObjectParent = new ExportJobDeclarationMessageSendingObjectParent(declaration);
			AssertEquals(1, messageSendingObjectParent.SendingObjectsCollection.Count);
			AssertEquals(ZBool.True, messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend);
			AssertEquals(ZBool.False, messageSendingObjectParent.SendingObjectsCollection[0].Header.CanSendRectification);
			var manager = new DeclarationMultiMessageManagerForTesting(messageSendingObjectParent);
			AssertSame("The TopLevelBizObjToManage should be the jobDeclaration", declaration, manager.TopLevelBizObjToManage);
			var sendingObject = manager.GetAllMessageManagers_Exposed().First();
			AssertType<ExportDeclarationMessageManager>("A ExportDeclarationMessageManager should been created for sending object", sendingObject);
		}

		public void TestGetAllSingleMessageManagersDelegateForAmendmentDetection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			entryInstruction.CEI_Description = "Description";
			var cusHeader = declaration.CustomsEntryHeaders.AddNew();
			cusHeader.CH_EntryStatus = "CUS";
			cusHeader.CH_CEI_Instruction = entryInstruction.PK;
			cusHeader.EntryNumber = "B42150";
			var entryLine1 = cusHeader.MergedLines.AddNew();
			var entryLine2 = cusHeader.MergedLines.AddNew();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = "BRL";
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "AB10";
			invoiceLine1.JI_LinePrice = 10m;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "CD20";
			invoiceLine2.JI_LinePrice = 20m;
			var messageSender = new ExportDeclarationMessageSendingObject(cusHeader);
			var messageManager = new ExportDeclarationMessageManager(messageSender);
			Factory.Save();

			AssertEquals("ExportDeclarationMessageManager - RequiresAmendment should be false", false, messageManager.RequiresAmendment());
			invoiceLine1.JI_LinePrice = 98m;
			invoiceLine2.JI_LinePrice = 12m;
			var messageSendingObjectParent = new ExportJobDeclarationMessageSendingObjectParent(declaration);
			AssertEquals(1, messageSendingObjectParent.SendingObjectsCollection.Count);
			AssertEquals(ZBool.True, messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend);
			AssertEquals(ZBool.True, messageSendingObjectParent.SendingObjectsCollection[0].Header.CanSendRectification);
			var manager = new DeclarationMultiMessageManagerForTesting(messageSendingObjectParent);
			var result = manager.DetermineRequiredMessagesWithPendingChanges();
			AssertEquals("MessageTypesToSend has messages to send", ZBool.True, result.HasMessagesToSend);
			AssertEquals("MessageTypesToSend has amendments", "amendment(s)", result.MessageTypesToSend);
		}

		public void TestGetDeferredAmendmentSavingOptions()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var manager = new DeclarationMultiMessageManager(new ExportJobDeclarationMessageSendingObjectParent(declaration)) as IMessageManager;
			var savingOptions = manager.GetDeferredAmendmentSavingOptions();
			AssertType<DeclarationDeferredAmendmentSavingOptions>(savingOptions);
			Assert("SavingOptions is not cancelled for EXP", !savingOptions.IsCancelled);

			manager = new DeclarationMultiMessageManager(new ImportLicenseMessageSendingObjectParent(declaration));
			savingOptions = manager.GetDeferredAmendmentSavingOptions();
			AssertType<NoGuiDeferredAmendmentSavingOptions>(savingOptions);
			Assert("SavingOptions is cancelled for LIC (not allow saving)", savingOptions.IsCancelled);
		}

		public void TestProcessWhenChangesAreSavedWithoutSending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.ActiveEntryHeaders.AddNew().CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			declaration.ActiveEntryHeaders.AddNew().CH_Status = BRMessageStatusList.Codes.AwaitingResponse;

			var parent = new ExportJobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject1 = parent.SendingObjectsCollection.First() as ExportDeclarationMessageSendingObject;
			var sendingObject2 = parent.SendingObjectsCollection.Last() as ExportDeclarationMessageSendingObject;
			var manager = new DeclarationMultiMessageManager(parent) as IMessageManager;
			var information = manager.GetRequiredMessagesInformation();
			information.AddAmendmentBridgeForTesting(new[] { new ExportDeclarationMessageManager(sendingObject1) });
			var savingOptions = new DeclarationDeferredAmendmentSavingOptions(declaration);
			savingOptions.SendAmendment = true;
			manager.ProcessWhenChangesAreSavedWithoutSending(savingOptions, information);
			AssertNull("DAP event should not be added", declaration.Logs.MostRecentLog);
			AssertEquals(BRMessageStatusList.Codes.AwaitingResponse, declaration.ActiveEntryHeaders[0].CH_Status);

			savingOptions.SaveWithEntryChanges = true;
			information.AmendmentWithdrawalReason.ReasonText = "Created Declaration Amendment Queued Event";
			manager.ProcessWhenChangesAreSavedWithoutSending(savingOptions, information);

			var log = declaration.Logs.MostRecentLog;
			AssertEquals("DAP event should be added", "DAP", log.SL_SE_NKEvent);
			AssertEquals("Amendment Reason should be set as Reference", "Created Declaration Amendment Queued Event", log.SL_Reference);
			AssertEquals("Message Status of the first entry", BRMessageStatusList.Codes.NotSent, sendingObject1.Header.CH_Status);
			AssertEquals("Message Status of the second entry", BRMessageStatusList.Codes.AwaitingResponse, sendingObject2.Header.CH_Status);
		}

		#region Implementation

		class DeclarationMultiMessageManagerForTesting : DeclarationMultiMessageManager
		{
			public DeclarationMultiMessageManagerForTesting(IJobDeclarationMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
			{
			}

			public SingleMessageManager[] GetAllMessageManagers_Exposed()
			{
				return GetAllMessageManagers();
			}
		}

		#endregion
	}
}
