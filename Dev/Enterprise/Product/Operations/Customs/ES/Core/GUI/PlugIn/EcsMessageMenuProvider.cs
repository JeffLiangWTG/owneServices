using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ES.Business.MessageSending.ECSMessageSender;

namespace Enterprise.Customs.ES.GUI
{
	public class EcsMessageMenuProvider : EU.GUI.PlugIn.EcsMessageMenuProvider
	{
		public EcsMessageMenuProvider(EU.Business.CusExitControlHeader exitHeader) : base(exitHeader)
		{
		}

		new CusExitControlHeader ExitHeader => (CusExitControlHeader)base.ExitHeader;

		string LockCutomsFileMenuItemCaption
			=> ResString.GetMultilingualString("147A632A-1A4A-443A-84B4-A1E2DF9A53E7", "Lock Declaration");

		string UnlockCutomsFileMenuItemCaption
			=> ResString.GetMultilingualString("8C6464CF-61EB-45D4-A25E-DD4E8ABF8BD0", "Unlock Declaration");

		void OnLockMenuClick(object sender, EventArgs e)
		{
			if (Globals.IsTest)
			{
				WriteLockLog(new BusinessObject[] { ExitHeader }, string.Empty);
			}
			else
			{
				using (var form = new Customs.GUI.CustomsWriteToLogForm(ExitHeader, new BusinessObject[] { ExitHeader }, LockCutomsFileMenuItemCaption, WriteLockLog))
				{
					form.ShowDialog();
				}
			}
		}

		void OnUnlockMenuClick(object sender, EventArgs e)
		{
			if (Globals.IsTest)
			{
				WriteUnlockLog(new BusinessObject[] { ExitHeader }, string.Empty);
			}
			else
			{
				using (var form = new Customs.GUI.CustomsWriteToLogForm(ExitHeader, new BusinessObject[] { ExitHeader }, UnlockCutomsFileMenuItemCaption, WriteUnlockLog))
				{
					form.ShowDialog();
				}
			}
		}

		void WriteLockLog(BusinessObject[] businessObjects, ZString reference)
		{
			var message = ExitHeader.LockExitHeader(reference);
			if (!message.IsEmpty)
			{
				Globals.Message.ShowInformation(message);
			}
			RefreshLockMenus();
		}

		void WriteUnlockLog(BusinessObject[] businessObjects, ZString reference)
		{
			var message = ExitHeader.UnlockExitHeader(reference);
			if (!message.IsEmpty)
			{
				Globals.Message.ShowInformation(message);
			}
			RefreshLockMenus();
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			yield return new ZMenuItem(ResString.GetMultilingualString("B54D36ED-2DF7-408B-BCA1-BF2B66E4F7D4", "Arrive at Exit Location"), ArriveAtExitLocationClick);
			yield return new ZMenuItem(ResString.GetMultilingualString("4C3DE95F-C739-4018-865B-C11ED6970E89", "Download EAL Clearance Document"), DownloadEALClick);
			yield return new ZMenuItem("-");

			var configures = CustomsDataRegistry.Instance.DeclarationLockForEdit.Value;
			if (configures.Any() && Env.Security.LockOrUnlockFileForEdit.IsAllowed)
			{
				lockCustomsFileMenuItem = new ZMenuItem(LockCutomsFileMenuItemCaption, OnLockMenuClick) { Name = nameof(lockCustomsFileMenuItem) };
				unlockCustomsFileMenuItem = new ZMenuItem(UnlockCutomsFileMenuItemCaption, OnUnlockMenuClick) { Name = nameof(unlockCustomsFileMenuItem) };
				yield return lockCustomsFileMenuItem;
				yield return unlockCustomsFileMenuItem;
				RefreshLockMenus();
			}
		}

		void RefreshLockMenus()
		{
			if (lockCustomsFileMenuItem != null && unlockCustomsFileMenuItem != null)
			{
				var islockOrUnlockMenuVisible = IsLockCustomsFileMenuVisible();
				var isDeclarationLocked = ExitHeader != null && ((ICustomsFileParent)ExitHeader).IsLocked;

				lockCustomsFileMenuItem.Visible = islockOrUnlockMenuVisible && ExitHeader != null && !isDeclarationLocked;
				unlockCustomsFileMenuItem.Visible = islockOrUnlockMenuVisible && ExitHeader != null && isDeclarationLocked;
			}
		}

		ZMenuItem lockCustomsFileMenuItem;
		ZMenuItem unlockCustomsFileMenuItem;

		bool IsLockCustomsFileMenuVisible()
		{
			var declarationType = (ExitHeader as ICustomsFileParent)?.DeclarationType;
			return !string.IsNullOrWhiteSpace(declarationType) && CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Cast<DeclarationLockConfig>().Any(c => c.DeclarationType == declarationType.Value);
		}

		public override bool CreateArrivalMessages() => SendECSMessageToCustoms();

		bool SendECSMessageToCustoms()
		{
			var succeeded = false;

			if (CheckDeclarationBeforeSending(out var broker))
			{
				var factory = new BusinessObjectFactory();
				var newFactoryExitHeader = factory.Load<CusExitControlHeader>(ExitHeader.PK);
				newFactoryExitHeader.Reload();

				var messageSendingObject = new ECSMessageSendingObject(newFactoryExitHeader, broker);
				messageSendingObject.ShouldEditMessage = MessageEditHelper.GetShouldEditMessagePopUpResponse(DialogResult.No);

				bool continueWithSend = true;

				var exitHeaderWrapper = new ECSExitHeaderMessageSendingObjectParent(messageSendingObject);

				using (var form = GetMessageSendingForm(exitHeaderWrapper))
				{
					continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
				}

				if (continueWithSend)
				{
					var sender = new ECSMessageSender(exitHeaderWrapper);
					var messageBuildersData = sender.GetMessageBuildersData();

					if (exitHeaderWrapper.ShouldEditMessage)
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
						var result = SendAndSaveDetails(factory, sender, messageBuildersData);

						if (!result.IsEmpty)
						{
							Globals.Message.Show(result);
						}
					}
				}
			}

			return succeeded;
		}

		ZString SendAndSaveDetails(BusinessObjectFactory factory, ECSMessageSender sender, List<MessageBuilderData> messageBuildersData)
		{
			try
			{
				sender.Send(messageBuildersData);
				factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				sender.messagesWithCreateFailure++;
			}

			return SetResultMessage(sender);
		}

		ZString SetResultMessage(ECSMessageSender sender)
		{
			var result = new ZString();

			if (sender.messagesSent > 0)
			{
				result = GetMessageSendSuccessful(sender.messagesSent);
			}
			if (sender.messagesWithCreateFailure > 0)
			{
				result += System.Environment.NewLine + GetMessageCreateFailure(sender.messagesWithCreateFailure);
			}
			if (sender.messagesWithSendFailure > 0)
			{
				result += System.Environment.NewLine + GetGetMessageSendFailure(sender.messagesWithSendFailure);
			}
			return result;
		}

		ZString GetMessageSendSuccessful(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{FFB1CC2B-301E-401D-AA3B-EDB6FC06D397}", "{0} Messages sent successfully.", numberOfMessages) : Res.GetString("{A4344229-95F8-405D-A5FB-ADB935D52931}", "{0} Message sent successfully.", numberOfMessages);

		ZString GetGetMessageSendFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{51DE65CD-F74A-4B86-B151-F49E43A69008}", "Failed to send {0} messages.", numberOfMessages) : Res.GetString("{88D2AB08-C0E2-4937-A508-73E7A1AC1745}", "Failed to send {0} message.", numberOfMessages);

		ZString GetMessageCreateFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{EA7F5A29-62D4-401B-BD45-D227499DC6E7}", "Failed to create {0} messages.", numberOfMessages) : Res.GetString("{B4DEF275-23A2-46D9-BCDB-B22CF13E74D9}", "Failed to create {0} message.", numberOfMessages);

		void DownloadEALClick(object sender, EventArgs e)
		{
			if (PreSaveBeforeSendingMessages(sender as ZMenuItem) && CheckDeclarationBeforeSending(out var broker))
			{
				var messageSendingObject = new ECSMessageSendingObject(ExitHeader, broker);
				SendDocumentRequestToCustoms(((ICertificateProvider)messageSendingObject).CertificateName);
			}
		}

		void SendDocumentRequestToCustoms(ZString certificateName)
		{
			int messagesSentCount = ZInt.Zero;
			try
			{
				var factory = new BusinessObjectFactory();
				var newFactoryExitHeader = factory.Load<CusExitControlHeader>(ExitHeader.PK);
				newFactoryExitHeader.Reload();

				foreach (CusExitDetail exitDetail in newFactoryExitHeader.CusExitDetails)
				{
					if (!exitDetail.CED_MovementReferenceNumber.IsEmpty && !certificateName.IsEmpty)
					{
						var ealDocRequest = new ArrivalAtExitDocumentRequest(exitDetail, certificateName);
						messagesSentCount += ealDocRequest.RequestMissingDocuments();
					}
				}
				factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Globals.Message.Show(ResString.GetMultilingualString("518159E5-1489-44F2-8D9A-4CC65E9B8487", "{0} Document Capture request(s) created", messagesSentCount.ToString(Culture.Current)));
		}

		bool CheckDeclarationBeforeSending(out GlbStaff broker)
		{
			broker = null;
			bool continueWithSend = false;

			if (!ExitHeader.CusExitDetails.Any())
			{
				Globals.Message.Show(Res.GetString("547167C4-F4D4-4B8D-AB6A-84B8FE02F70B", "No details exist – Please add details before attempting to send a message."));
			}
			else
			{
				broker = ExitHeader.CustomsAgent;
				if (broker == null || CertificateHasMessageErrors())
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("60871F3B-BABD-4D8F-BF5D-77FFF01BF882", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate."));
				}
				else
				{
					continueWithSend = true;
				}
			}
			return continueWithSend;

			ZBool CertificateHasMessageErrors()
			{
				ExitHeader.Validation.ValidateCEH_CustomsProfile();
				return ExitHeader.CEH_CustomsProfileInfo.HasMessageErrors();
			}
		}

		protected virtual ECSMessageSendingForm GetMessageSendingForm(ECSExitHeaderMessageSendingObjectParent wrapper) => new ECSMessageSendingForm(wrapper);

		protected virtual MessageEditForm GetMessageEditForm() => new MessageEditForm();
	}
}
