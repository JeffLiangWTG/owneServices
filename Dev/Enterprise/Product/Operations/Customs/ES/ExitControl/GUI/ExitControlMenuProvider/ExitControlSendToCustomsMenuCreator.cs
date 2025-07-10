using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.ES.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.ES.ExitControl.Business.ExitControlMessageSender;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ExitControlSendToCustomsMenuCreator : EU.ExitControl.GUI.ExitControlSendToCustomsMenuCreator
	{
		public ExitControlSendToCustomsMenuCreator(CusExitHeader header) : base(header)
		{
		}

		protected new CusExitHeader header => (CusExitHeader)base.header;
		BusinessObjectFactory newFactory;

		GlbStaff broker;

		protected override bool HasValidSystemSettings() => CheckDeclarationBeforeSending(out broker);

		protected override void OnMessageSendingFormOk(EU.ExitControl.Business.ExitControlMessageSendingObjectParent sendingParent)
		{
			var continueWithSend = true;

			var esSendingParent = (ExitControlMessageSendingObjectParent)sendingParent;
			var sender = new ExitControlMessageSender(esSendingParent);
			var messageBuildersData = sender.GetMessageBuildersData();

			if (esSendingParent.ShouldEditMessage)
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
			else
			{
				foreach (var builderData in messageBuildersData)
				{
					var messageBuilder = builderData.MessageBuilder;
					var messageText = messageBuilder.UnsignedMessageText;
					builderData.ObjectToSend.MessageCreated(messageText);
				}
			}

			if (continueWithSend)
			{
				var result = SendAndSaveDetails(newFactory, sender, messageBuildersData);

				if (!result.IsEmpty)
				{
					Globals.Message.Show(result);
				}
			}
		}

		ZString SendAndSaveDetails(BusinessObjectFactory factory, ExitControlMessageSender sender, List<MessageBuilderData> messageBuildersData)
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

		ZString SetResultMessage(ExitControlMessageSender sender)
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

		bool CheckDeclarationBeforeSending(out GlbStaff broker)
		{
			broker = null;
			var continueWithSend = false;

			if (header.CusExitReports.Count == 0)
			{
				Globals.Message.Show(CommonPromptMessages.NoReportErrorMessage);
			}
			else
			{
				broker = header.CustomsAgent;
				if (broker == null || CertificateHasMessageErrors())
				{
					Globals.Message.ShowError(CommonPromptMessages.CredentialsErrorMessage);
				}
				else
				{
					continueWithSend = true;
				}
			}
			return continueWithSend;

			ZBool CertificateHasMessageErrors()
			{
				header.Validation.ValidateCXH_CustomsProfile();
				return header.CXH_CustomsProfileInfo.HasMessageErrors();
			}
		}

		ZString GetMessageSendSuccessful(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{E878C7E1-BFFD-42E7-9951-B3A3F873755C}", "{0} Messages sent successfully.", numberOfMessages) : Res.GetString("{90C599E0-6288-49C5-BAC6-2AD322EA2F30}", "{0} Message sent successfully.", numberOfMessages);

		ZString GetGetMessageSendFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{C63E4CB8-37AB-4EEF-AAB1-1D480B45B85C}", "Failed to send {0} messages.", numberOfMessages) : Res.GetString("{53CEB001-E79F-430B-875B-10618AA13F9A}", "Failed to send {0} message.", numberOfMessages);

		ZString GetMessageCreateFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{31A29FF9-664B-49A2-ACF0-38D2845E4EE8}", "Failed to create {0} messages.", numberOfMessages) : Res.GetString("{02B8C0C5-B36E-4BDA-B7BA-4A53D49A9546}", "Failed to create {0} message.", numberOfMessages);

		protected virtual MessageEditForm GetMessageEditForm() => new MessageEditForm();

		protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetMessageSendingParent()
		{
			newFactory = new BusinessObjectFactory();
			var newFactoryExitHeader = newFactory.Load<CusExitHeader>(header.PK);
			newFactoryExitHeader.Reload();
			var sendingObject = new MessageSendingObject(newFactoryExitHeader, broker);
			sendingObject.ShouldEditMessage = MessageEditHelper.GetShouldEditMessagePopUpResponse(DialogResult.No);
			return new ExitControlMessageSendingObjectParent(sendingObject);
		}
	}
}
