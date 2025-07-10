using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IEConstants = Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.GUI
{
	public class EDIMenu : EU.GUI.EDIMenu
	{
		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected override void AddAdditionalMenuItems()
		{
			sendToCustomsMenuItem = new ZMenuItem(ResString.GetMultilingualString("77D8AB0E-4261-4088-8CC8-B1811E7B11C2", "Send to Customs"));
			sendToCustomsMenuItem.Click += SendToCustomsMenuItem_Click;
			MenuItems.Add(sendToCustomsMenuItem);

			singleSupportingDocumentsMenuItem = new ZMenuItem(uploadSupportingDocumentsText, UploadSupportingDocumentsMenu_Click);
			MenuItems.Add(singleSupportingDocumentsMenuItem);

			multipleSupportingDocumentsMenuItem = new ZMenuItem(uploadSupportingDocumentsText);
			MenuItems.Add(multipleSupportingDocumentsMenuItem);

			uploadDocumentsMenuItem = new ZMenuItem(uploadDocumentsText, UploadDocumentsMenu_Click);
			MenuItems.Add(uploadDocumentsMenuItem);

			refundApplicationMenuItem = new ZMenuItem(refundApplicationText, RefundApplicationMenu_Click);
			MenuItems.Add(refundApplicationMenuItem);

			depositRefundApplicationMenuItem = new ZMenuItem(depositRefundApplicationText, DepositRefundApplicationMenu_Click);
			MenuItems.Add(depositRefundApplicationMenuItem);
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItems.Remove(SADHDataEntryFormMenuItem);
			MenuItems.Remove(SingleLineEntryMenuItem);
			dataMenuItem.MenuItems.Remove(AutoPopulateAuthorizationsMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			var declaration = Declaration;
			var sendToCustomsEnabled = false;
			var multipleEntries = false;
			var uploadSupportingDocumentsEnabled = false;
			var uploadDocumentsEnabled = false;
			var refundApplicationEnabled = false;
			var depositRefundApplicationEnabled = false;
			if (declaration != null)
			{
				var isExport = declaration.IsExport;
				var isImport = declaration.IsImport;
				var isAvailableForImport = !isExport && declaration.IsImport && IECustomsDataRegistry.Instance.IsDirectSendToCustomsForImportEnabled.Value;
				if (!declaration.IsDeclarationIntegrated)
				{
					sendToCustomsEnabled = isExport || isAvailableForImport;
					uploadSupportingDocumentsEnabled = isExport;
					uploadDocumentsEnabled = isAvailableForImport;
					refundApplicationEnabled = isImport;
					depositRefundApplicationEnabled = isImport;
				}
				multipleEntries = declaration.ActiveEntryHeaders.Count > 1;
			}

			sendToCustomsMenuItem.Visible = sendToCustomsEnabled;
			singleSupportingDocumentsMenuItem.Visible = uploadSupportingDocumentsEnabled && !multipleEntries;

			multipleSupportingDocumentsMenuItem.Visible = uploadSupportingDocumentsEnabled && multipleEntries;
			if (multipleSupportingDocumentsMenuItem.Visible)
			{
				RefreshMultipleSupportingDocumentsMenuItem();
			}

			uploadDocumentsMenuItem.Visible = uploadDocumentsEnabled;

			refundApplicationMenuItem.Visible = refundApplicationEnabled;
			depositRefundApplicationMenuItem.Visible = depositRefundApplicationEnabled;
		}

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		#region Send To Customs

		ZMenuItem sendToCustomsMenuItem;

		string GetMessageSentText(int count)
		{
			return Res.GetString("C1B99F1A-2657-4915-BA79-37DDDB322711", "{0} message(s) queued for sending.", count);
		}

		void SendToCustomsMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckBeforeSending())
			{
				if (Declaration.IsImport)
				{
					SendImportMessages(Declaration);
				}
				else
				{
					SendExportMessages(Declaration);
				}
				Declaration.RecalculateValidationModesOnAllLevel();
			}
		}

		bool CheckBeforeSending() => MergeAndPreSave(Declaration) && Declaration.Company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired() && HasEntries;

		bool HasEntries
		{
			get
			{
				var hasEntries = true;
				if (!Declaration.ActiveEntryHeaders.Any())
				{
					Globals.Message.ShowInformation(Res.GetString("91047D45-C27A-41B7-8B03-7467C9C5CEBF", "No entries exist – Please generate entries before attempting to send a message to customs."));
					hasEntries = false;
				}
				return hasEntries;
			}
		}

		bool MergeAndPreSave(JobDeclaration declaration)
		{
			var needMerge = declaration != null && (!declaration.IsMergeDone || declaration.MergeManager.RequiresMerge);
			return (!needMerge || PerformMerge()) && PreSaveDeclaration(declaration);
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				Globals.Message.ShowInformation(GetMessageSentText(messagesCreated));
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		#region SendMessages

		void SendImportMessages(JobDeclaration declaration)
		{
			if (declaration.IsUCC5)
			{
				SendMessages(declaration, new AISUCC5MessageSendingActionParent(declaration));
			}
			else
			{
				SendMessages(declaration, new AISMessageSendingActionParent(declaration));
			}
		}

		void SendExportMessages(JobDeclaration declaration) => SendMessages(declaration, new AESMessageSendingActionParent(declaration));

		void SendMessages<TSendingAction>(JobDeclaration declaration, CusEntryHeaderMessageSendingActionParent<TSendingAction> sendingParent)
			where TSendingAction : CusEntryHeaderMessageSendingAction
		{
			using (var messageSendingForm = new MessageSendingForm<TSendingAction>(sendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK && declaration.CheckCredit())
				{
					var messageSent = 0;
					var shouldSendActions = sendingParent.SendingObjectsCollection.Cast<CusEntryHeaderMessageSendingAction>()
						.Where(action => action.ShouldSend);

					shouldSendActions.ForEach(action =>
					{
						var duplicationPossible = action.EntryHeader.DuplicationPossible;
						var statusBeforeSending = action.MessageStatus;
						
						if (action.CreateSender().Send() != null)
						{
							var authLogNeeded = false;
							var authLogRef = string.Empty;
							if (statusBeforeSending.EqualsIgnoringCase(LogicalStatusList.Codes.Sent))
							{
								authLogNeeded = true;
								authLogRef = IEConstants.SendingMessageEventList.Reference;
							}
							else if (duplicationPossible)
							{
								authLogNeeded = true;
								authLogRef = IEConstants.SendingMessageEventList.Resubmit + action.EntryHeader.CH_EntryStatus;
							}
							if (authLogNeeded)
							{
								declaration.Logs.AddNew(
									eventType: ZArchitecture.Business.Events.Authorised,
									reference: authLogRef,
									dateTime: DateTimeOffset.Now,
									parameters: GetEventParameters(action.LocalReferenceNumber, messageSendingForm.ConfirmReason)
								);
							}

							messageSent++;
						}
					});

					if (messageSent > 0)
					{
						TrySaveAndShowMessage(declaration.Factory, messageSent);
					}
				}
			}
		}

		KeyValuePair<string, string>[] GetEventParameters(string lrn, string reason) => new[]
		{
			new KeyValuePair<string, string>(IEConstants.SendingMessageEventList.RFN, lrn),
			new KeyValuePair<string, string>(IEConstants.SendingMessageEventList.RES, reason)
		};

		#endregion

		#region Send Documents: Methods & MenuItems

		ResourceString uploadSupportingDocumentsText => ResString.GetMultilingualString("9AB8CA1D-BAFD-4EFB-BB25-E349D8F2A4ED", "Upload Supporting Documents");
		ResourceString uploadDocumentsText => ResString.GetMultilingualString("C98E265E-6070-430C-A431-B4FFA6455AC8", "Upload Documents");

		ZMenuItem singleSupportingDocumentsMenuItem;
		ZMenuItem multipleSupportingDocumentsMenuItem;
		ZMenuItem uploadDocumentsMenuItem;

		void RefreshMultipleSupportingDocumentsMenuItem()
		{
			if (Declaration is JobDeclaration declaration)
			{
				multipleSupportingDocumentsMenuItem.MenuItems.Clear();

				foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
				{
					var mrn = entry.MovementReferenceNumber;
					var caption = mrn.IsEmpty ? entry.CH_BGMReference : mrn;
					if (!caption.IsEmpty)
					{
						var subItem = new ZMenuItem(mrn.IsEmpty ? entry.CH_BGMReference : mrn, UploadSupportingDocumentsMenu_Click);
						multipleSupportingDocumentsMenuItem.MenuItems.Add(subItem);
					}
				}
			}
		}

		void UploadSupportingDocumentsMenu_Click(object sender, EventArgs e)
		{
			if (CheckBeforeSending())
			{
				UploadSupportingDocuments(Declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			}
		}

		void UploadSupportingDocuments(JobDeclaration declaration, string messageType)
		{
			var sendingParent = new DocumentsSendingActionParent(declaration, messageType);
			using (var messageSendingForm = new DocumentsSendingForm(sendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					var messageSent = 0;
					sendingParent.SendingObjectsCollection.Cast<DocumentsSendingAction>().Where(action => action.ShouldSend).ForEach(
						action =>
						{
							var sender = action.CreateSender();
							if (sender.Send() != null)
							{
								messageSent++;
							}
						});
					if (messageSent > 0)
					{
						TrySaveAndShowMessage(declaration.Factory, messageSent);
					}
				}
			}
		}

		void UploadDocumentsMenu_Click(object sender, EventArgs e)
		{
			if (CheckBeforeSending())
			{
				UploadDocuments(Declaration, AISUploadDocumentsMessageTypeList.Codes.IM483);
			}
		}

		void UploadDocuments(JobDeclaration declaration, string messageType)
		{
			var sendingParent = new UploadDocumentsSendingActionParent(declaration, messageType);
			using (var messageSendingForm = new AISDocumentsUploadForm(sendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					var messageSent = 0;
					sendingParent.SendingObjectsCollection.Cast<UploadDocumentsSendingAction>().Where(action => action.ShouldSend).ForEach(
						action =>
						{
							var sender = action.CreateSender();
							if (sender.Send() != null)
							{
								messageSent++;
							}
						});
					if (messageSent > 0)
					{
						TrySaveAndShowMessage(declaration.Factory, messageSent);
					}
				}
			}
		}

		#endregion

		#endregion

		#region Send Refund Application
		ResourceString refundApplicationText => ResString.GetMultilingualString("EF7A597F-F3C3-4BD0-A533-8ACCFFE97E49", "Send Refund Application");
		ZMenuItem refundApplicationMenuItem;

		void RefundApplicationMenu_Click(object sender, EventArgs e)
		{
			if (CheckBeforeSending())
			{
				SendRefundApplications(Declaration);
			}
		}

		void SendRefundApplications(JobDeclaration declaration)
		{
			var sendingParent = new RefundApplicationMessageSendingActionParent(declaration);
			using (var messageSendingForm = new RefundApplicationSendingForm(sendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					var messageSent = 0;
					sendingParent.SendingObjectsCollection.Cast<RefundApplicationMessageSendingAction>().Where(action => action.ShouldSend).ForEach(
						action =>
						{
							var sender = action.CreateSender();
							if (sender.Send() != null)
							{
								messageSent++;
							}
						});
					if (messageSent > 0)
					{
						TrySaveAndShowMessage(declaration.Factory, messageSent);
					}
				}
			}
		}
		#endregion

		#region Send Deposit Refund Application
		ResourceString depositRefundApplicationText => ResString.GetMultilingualString("A8AC4CAA-0B92-4F9D-922A-8CC4084F23CA", "Send Deposit Refund Application");
		ZMenuItem depositRefundApplicationMenuItem;

		void DepositRefundApplicationMenu_Click(object sender, EventArgs e)
		{
			if (CheckBeforeSending())
			{
				SendDepositRefundApplications(Declaration);
			}
		}

		void SendDepositRefundApplications(JobDeclaration declaration)
		{
			var sendingParent = new DepositRefundApplicationMessageSendingActionParent(declaration);
			using (var messageSendingForm = new DepositRefundApplicationSendingForm(sendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					var messageSent = 0;
					sendingParent.SendingObjectsCollection.Cast<DepositRefundApplicationMessageSendingAction>().Where(action => action.ShouldSend).ForEach(
						action =>
						{
							var sender = action.CreateSender();
							if (sender.Send() != null)
							{
								messageSent++;
							}
						});
					if (messageSent > 0)
					{
						TrySaveAndShowMessage(declaration.Factory, messageSent);
					}
				}
			}
		}
		#endregion
	}
}
