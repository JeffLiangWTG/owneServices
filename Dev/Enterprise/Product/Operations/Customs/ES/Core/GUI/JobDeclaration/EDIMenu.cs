using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using JobDeclarationMessageSendingObject = Enterprise.Customs.ES.Business.MessageSending.JobDeclarationMessageSendingObject;

namespace Enterprise.Customs.ES.GUI
{
	public class EDIMenu : EU.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get => (JobDeclaration)base.Declaration;
			set => base.Declaration = value;
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Remove(SADHDataEntryFormMenuItem);
			MenuItems.Remove(SingleLineEntryMenuItem);

			sendToCustomsMenuItem = new ZMenuItem(Res.GetString("C7033E6B-5425-4692-AB55-7E6AAA3E6F21", "Send to Customs"), SendToCustomsMenuItem_Click);
			MenuItems.Add(sendToCustomsMenuItem);

			captureFromCustomsMenuItem = new ZMenuItem(Res.GetString("68C21AC0-97A6-4057-9905-84A627AE4FF9", "Capture from Customs"), CaptureFromCustomsMenuItem_Click);
			MenuItems.Add(captureFromCustomsMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			var declaration = Declaration;
			var isBuiltin = declaration != null && !declaration.ShowSubmitMenuItem;

			sendToCustomsMenuItem.Visible = isBuiltin;
			captureFromCustomsMenuItem.Visible = isBuiltin;
		}

		#region MenuItems

		protected ZMenuItem sendToCustomsMenuItem;
		void SendToCustomsMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				if (CheckDeclarationBeforeSending(out var broker))
				{
					GenericSendToCustoms(broker, ShowMessageSendingForm);
				}
			}
		}

		protected ZMenuItem captureFromCustomsMenuItem;
		void CaptureFromCustomsMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckHasChanges())
			{
				if (CheckDeclarationBeforeSending(out var broker))
				{
					var certificateName = GenericSendToCustoms(broker, SelectEntriesForCustomsQuery);
					SendDocumentRequestToCustoms(certificateName);
				}
			}
		}

		#endregion

		#region SendToCustoms

		bool ShowMessageSendingForm(JobDeclarationMessageSendingObjectParent decWrapper)
		{
			using (var form = GetMessageSendingForm(decWrapper))
			{
				return ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
			}
		}

		bool SelectEntriesForCustomsQuery(JobDeclarationMessageSendingObjectParent decWrapper)
		{
			foreach (JobDeclarationMessageSendingObject sendingObject in decWrapper.SendingObjectsCollection)
			{
				var entryHeader = sendingObject.Header;
				if (!entryHeader.IsWaitingForResponse
					&& !entryHeader.MovementReferenceNumber.IsEmpty
					&& entryHeader.CSVClearance.IsEmpty)
				{
					if (sendingObject.IsImport && sendingObject.IsDeclarationH2 && sendingObject.IsEntryInstructionABZ)
					{
						sendingObject.ShouldSend = true;
						sendingObject.MessageType = DeclarationMessageTypeList.Codes.DvdH2Query;
					}
					else if (sendingObject.IsImport && sendingObject.IsEntryInstructionABXYZ && entryHeader.CH_EntryStatus != MessageProcessorConstants.EntryStatusCodes.IncompletePreDeclaration)
					{
						sendingObject.ShouldSend = true;
						sendingObject.MessageType = DeclarationMessageTypeList.Codes.ImportQuery;
					}
					else if (sendingObject.IsExport && sendingObject.IsEntryInstructionABXYZ)
					{
						sendingObject.ShouldSend = true;
						sendingObject.MessageType = DeclarationMessageTypeList.Codes.ExportQuery;
					}
					else if (sendingObject.IsPOUS && sendingObject.IsEntryInstructionT2LOrT2C)
					{
						sendingObject.ShouldSend = true;
						sendingObject.MessageType = DeclarationMessageTypeList.Codes.T2lQueryPous;
					}
					else if (entryHeader.IsImportUCC6 && !sendingObject.IsDeclarationH2 && sendingObject.IsEntryInstructionABCXYZ)
					{
						sendingObject.ShouldSend = true;
						sendingObject.MessageType = DeclarationMessageTypeList.Codes.ImportH1Query;
					}
				}
			}

			return true;
		}

		bool CheckDeclarationBeforeSending(out GlbStaff broker)
		{
			broker = null;
			var continueWithSend = false;

			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if ((!needMerge || PerformMerge()) && PreSaveDeclaration(Declaration))
			{
				continueWithSend = true;

				if (Declaration.CustomsEntryHeaders.Count == 0)
				{
					Globals.Message.Show(Res.GetString("33A3148E-FF59-4B79-B1D4-C951E9A2C537", "No entries exist – Please generate entries before attempting to send a message."));
					continueWithSend = false;
				}

				if (continueWithSend)
				{
					broker = Declaration.CusAgent;
					if (broker == null || CertificateHasMessageErrors())
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("2204F761-C002-4779-A246-154EFE4AC7BA", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate under the miscellaneous options."));
						continueWithSend = false;
					}
				}
			}
			return continueWithSend;

			ZBool CertificateHasMessageErrors()
			{
				Declaration.Validation.ValidateJE_CustomsProfile();
				return Declaration.JE_CustomsProfileInfo.HasMessageErrors();
			}
		}

		ZString GenericSendToCustoms(GlbStaff broker, Func<JobDeclarationMessageSendingObjectParent, bool> identifyMessagesToSend)
		{
			var factory = new BusinessObjectFactory();
			var newFactoryDeclaration = factory.Load<JobDeclaration>(Declaration.PK);
			newFactoryDeclaration.Reload();

			var messageSendingObject = new MessageSendingObject(newFactoryDeclaration, broker);
			messageSendingObject.ShouldEditMessage = MessageEditHelper.GetShouldEditMessagePopUpResponse(DialogResult.No);

			var decWrapper = new JobDeclarationMessageSendingObjectParent(messageSendingObject);

			var continueWithSend = identifyMessagesToSend(decWrapper) && newFactoryDeclaration.CheckCredit();

			if (continueWithSend)
			{
				var sender = new ESMessageSender(decWrapper);
				var messageBuildersData = sender.GetMessageBuildersData();

				if (decWrapper.ShouldEditMessage)
				{
					foreach (var builderData in messageBuildersData)
					{
						var messageBuilder = builderData.MessageBuilder;
						var messageText = messageBuilder.UnsignedMessageText;
						using (var editForm = GetMessageEditForm())
						{
							(messageText, continueWithSend) = editForm.EditMessage(messageText);
						}
						messageBuilder.UnsignedMessageText = messageText;
						if (!continueWithSend)
						{
							break;
						}
					}
				}

				if (continueWithSend)
				{
					messageBuildersData = ES.Business.CusTempStorage.InventoryManagementHelper.InventoryManagementAction(factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrors);

					var result = SendAndSaveEntries(factory, messageBuildersData);

					if (!result.IsEmpty)
					{
						Globals.Message.Show(result);
						var parentForm = GetMainMenu()?.GetForm();
						if (parentForm is JobDeclarationForm)
						{
							((JobDeclarationForm)parentForm).SetLocationOfGoodsUserControlReadOnly();
						}
					}
				}
			}

			foreach (CusEntryHeader entryHeader in newFactoryDeclaration.CustomsEntryHeaders)
			{
				entryHeader.SetDefaultValidationMode();
			}

			return ((ICertificateProvider)messageSendingObject).CertificateName;
		}

		List<MessageBuilderData> ReserveTemporaryStorageGoodsWhenErrors(CusEntryHeader entryHeader, ZString errorMessage, ZString errorMessageVINs, IEnumerable<DataToReserveTSGoods> dataToReserve, MessageBuilderData builderData, List<MessageBuilderData> messageBuildersDataToContinue)
		{
			var resultMessage = false;
			var resultMessageVINs = false;
			var erroMessageTextToAsk = Res.GetString("34e4742e-57a4-440d-bb5e-37b86e2b41d1", "Do you want to cancel this declaration to check?");
			var erroMessageTextToAskForVINs = Res.GetString("83015C38-6FE9-4B03-9599-E24D80033EF3", "Would you like to cancel this action and check the gross weight declared for the vehicles?");

			var messageContainsTextToAsk = errorMessage.Contains(erroMessageTextToAsk);
			var messageVINsContainsTextToAsk = errorMessageVINs.Contains(erroMessageTextToAskForVINs);

			if (messageContainsTextToAsk)
			{
				resultMessage = Globals.Message.Show(
								errorMessage,
								Res.GetString("6B8F15D4-4949-4A75-829B-D5E7C3F5E38C", "Temporary Storage Management"),
								MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;
				if (!resultMessage)
				{
					messageBuildersDataToContinue.Remove(builderData);
					return messageBuildersDataToContinue;
				}
			}

			if (messageVINsContainsTextToAsk)
			{
				resultMessageVINs = Globals.Message.Show(
								errorMessageVINs,
								Res.GetString("FCF30ED1-2448-4279-ACDA-E95D69EE5C0A", "Temporary Storage Management"),
								MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;
				if (!resultMessageVINs)
				{
					messageBuildersDataToContinue.Remove(builderData);
					return messageBuildersDataToContinue;
				}
			}

			if ((messageContainsTextToAsk && resultMessage && messageVINsContainsTextToAsk && resultMessageVINs) ||
				(!messageContainsTextToAsk && messageVINsContainsTextToAsk && resultMessageVINs) ||
				(messageContainsTextToAsk && resultMessage && !messageVINsContainsTextToAsk))
			{
				EU.Business.TemporaryStorageHelper.ReserveTemporaryStorageGoods(entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType, dataToReserve);
				return messageBuildersDataToContinue;
			}

			if (!messageContainsTextToAsk || !messageVINsContainsTextToAsk)
			{
				var strMessage = ZString.Empty;
				if (!messageContainsTextToAsk)
				{
					strMessage = errorMessage;
				}
				if (!messageVINsContainsTextToAsk)
				{
					if (!strMessage.IsEmpty)
					{
						strMessage += System.Environment.NewLine;
					}
					strMessage += errorMessageVINs;
				}
				messageBuildersDataToContinue.Remove(builderData);
				Globals.Message.Show(strMessage);
			}

			return messageBuildersDataToContinue;
		}

		ZString SendAndSaveEntries(BusinessObjectFactory factory, List<MessageBuilderData> messageBuildersData)
		{
			MessagesInfo messagesInfo = null;
			try
			{
				messagesInfo = ESMessageSender.Send(messageBuildersData);
				factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				messagesInfo.MessagesWithCreateFailure++;
			}

			return SetResultMessage(messagesInfo);
		}

		ZString SetResultMessage(MessagesInfo messagesInfo)
		{
			var result = new ZString();

			if (messagesInfo.MessagesSent > 0)
			{
				result = GetMessageSendSuccessful(messagesInfo.MessagesSent);
			}
			if (messagesInfo.MessagesWithCreateFailure > 0)
			{
				result += System.Environment.NewLine + GetMessageCreateFailure(messagesInfo.MessagesWithCreateFailure);
			}
			if (messagesInfo.MessagesWithSendFailure > 0)
			{
				result += System.Environment.NewLine + GetGetMessageSendFailure(messagesInfo.MessagesWithSendFailure);
			}
			return result;
		}

		static ZString GetMessageSendSuccessful(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{FFB1CC2B-301E-401D-AA3B-EDB6FC06D397}", "{0} Messages sent successfully.", numberOfMessages) : Res.GetString("{A4344229-95F8-405D-A5FB-ADB935D52931}", "{0} Message sent successfully.", numberOfMessages);

		static ZString GetGetMessageSendFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{51DE65CD-F74A-4B86-B151-F49E43A69008}", "Failed to send {0} messages.", numberOfMessages) : Res.GetString("{88D2AB08-C0E2-4937-A508-73E7A1AC1745}", "Failed to send {0} message.", numberOfMessages);

		static ZString GetMessageCreateFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{EA7F5A29-62D4-401B-BD45-D227499DC6E7}", "Failed to create {0} messages.", numberOfMessages) : Res.GetString("{B4DEF275-23A2-46D9-BCDB-B22CF13E74D9}", "Failed to create {0} message.", numberOfMessages);

		void SendDocumentRequestToCustoms(ZString certificateName)
		{
			int messagesSentCount = ZInt.Zero;
			try
			{
				var factory = new BusinessObjectFactory();
				var newFactoryDeclaration = factory.Load<JobDeclaration>(Declaration.PK);
				newFactoryDeclaration.Reload();

				foreach (var entryHeader in newFactoryDeclaration.CustomsEntryHeaders)
				{
					var isImportT2LPOUS2 = Declaration.IsImport && entryHeader.IsT2L && entryHeader.ZG_POUSVersion > 1;
					var mrnCode = !entryHeader.MovementReferenceNumber.IsEmpty && !entryHeader.IsT2C && !isImportT2LPOUS2 ? entryHeader.MovementReferenceNumber : entryHeader.T2CMovementReferenceNumber;
					if (!mrnCode.IsEmpty && !certificateName.IsEmpty)
					{
						var isNotT2LorT2CEntry = entryHeader.EntryInstruction != null && !entryHeader.EntryInstruction.IsT2L && !entryHeader.EntryInstruction.IsT2C;

						if (Declaration.IsExport && entryHeader.IsExsSubStyle)
						{
							var exsDocRequest = new EXSDocumentRequest(entryHeader, certificateName);
							messagesSentCount += exsDocRequest.RequestMissingDocuments();
						}
						else if (entryHeader.IsExportNoUCC6 && isNotT2LorT2CEntry)
						{
							var exportDocRequest = new ExportDocumentRequest(entryHeader, certificateName);
							messagesSentCount += exportDocRequest.RequestMissingDocuments();
						}
						else if (entryHeader.IsExportUCC6 && isNotT2LorT2CEntry)
						{
							var exportAESDocRequest = new ExportAESDocumentRequest(entryHeader, certificateName);
							messagesSentCount += exportAESDocRequest.RequestMissingDocuments();
						}
						else if (Declaration.IsImport && entryHeader.IsH2Style)
						{
							var importDocRequest = new DVDDocumentRequest(entryHeader, certificateName);
							messagesSentCount += importDocRequest.RequestMissingDocuments();
						}
						else if (Declaration.IsUCC6AndIsImport && isNotT2LorT2CEntry)
						{
							var importDocRequest = new ImportH1DocumentRequest(entryHeader, certificateName);
							messagesSentCount += importDocRequest.RequestMissingDocuments();
						}
						else if (Declaration.IsImport && isNotT2LorT2CEntry)
						{
							var importDocRequest = new ImportDocumentRequest(entryHeader, certificateName);
							messagesSentCount += importDocRequest.RequestMissingDocuments();
						}
						else if (Declaration.IsExport && entryHeader.IsT2L)
						{
							var t2lExpeditionDocRequest = new T2LExpeditionDocumentRequest(entryHeader, certificateName);
							messagesSentCount += t2lExpeditionDocRequest.RequestMissingDocuments();
						}
						else if (Declaration.IsImport && entryHeader.IsT2C)
						{
							var t2lClearanceDocRequest = new T2LClearanceDocumentRequest(entryHeader, certificateName);
							messagesSentCount += t2lClearanceDocRequest.RequestMissingDocuments();
						}
						else if (isImportT2LPOUS2)
						{
							var t2lReceptionDocRequest = new T2LReceptionDocumentRequest(entryHeader, certificateName);
							messagesSentCount += t2lReceptionDocRequest.RequestMissingDocuments();
						}
					}
				}
				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Globals.Message.Show(ResString.GetMultilingualString("6E8F0B72-D343-4A31-B573-67C6A374F36B", "{0} Document Capture request(s) created", messagesSentCount.ToString(Culture.Current)));
		}

		protected override DataTransfer.Universal.JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
			=> new JobDeclarationUniversalMessagingHelper(wrapper);

		protected virtual MessageSendingForm GetMessageSendingForm(JobDeclarationMessageSendingObjectParent decWrapper) => new MessageSendingForm(decWrapper);

		protected virtual MessageEditForm GetMessageEditForm() => new MessageEditForm();

		protected override void PerformGenerateEntriesCore()
		{
			base.PerformGenerateEntriesCore();
			TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(Declaration);

			var docManagerAEODoc = new AEODocumentManager(Declaration, new AEODocumentMessageBoxProvider());
			docManagerAEODoc.AddUpdateOrRemoveAEODocumentInEntryInstructions();

			if (Declaration.IsImport)
			{
				var docManager9015Doc = new DocumentManager9015(Declaration, new Document9015MessageBoxProvider());
				docManager9015Doc.Remove9015DocumentsIfNeeded();
			}
		}
		#endregion
	}
}
