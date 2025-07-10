using System;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Business
{
	public class MessageSender : Common.MessageSender
	{
		public MessageSender(JobDeclarationMessageSendingObject sendingObject)
			: base(sendingObject.Header.Declaration)
		{
			this.sendingObject = sendingObject;
			this.entryHeader = sendingObject.Header as CusEntryHeader;
			this.procedureCode = sendingObject.GetProcedureCode();

			declaration = entryHeader.Declaration;
		}

		public MessageSender(MessageVisualObject visualObject)
			: this(GetSendingObject(visualObject))
		{
			this.visualObject = visualObject;
		}

		readonly JobDeclarationMessageSendingObject sendingObject;
		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly ZString procedureCode;
		readonly MessageVisualObject visualObject;

		public void SendMessageAndLog()
		{
			if (SendMessage())
			{
				if (procedureCode == JPProcedureCodeList.Codes.IDA || procedureCode == JPProcedureCodeList.Codes.EDA)
				{
					declaration.LogCustomsCommenced(procedureCode);
				}

				if (procedureCode != JPProcedureCodeList.Codes.MSX)
				{
					declaration.AddEventWithReference(Events.InterchangeSent, MessageData);
				}
			}
		}

		public EDIMessage ManualExportMessage()
		{
			var message = CreateEDIMessage();

			if (message != null)
			{
				message.EM_ApplicationReference = EDIMessage.FlatFile;
				NACCSStateMachineBuilder.Build(entryHeader).TransitOn(NACCSStateMachineBuilder.BuildRequest(message));
			}

			return message;
		}

		void LogAuthorised(EDIMessage message)
		{
			if (visualObject?.HasChangedValues() ?? false)
			{
				declaration.AddEventWithReference(Events.Authorised, string.Format(IStmALogExtensions.Reference.SendingWithOverriddenValues, message.EM_MessageNum));
			}
		}

		protected override bool GenerateMessage()
		{
			var message = CreateEDIMessage();

			if (message != null)
			{
				if (procedureCode == JPProcedureCodeList.Codes.MSX)
				{
					CreateMessageAttach(message);
				}

				EDIInterchange.CreateFromMessage(message);

				message.EM_FormattedMessageTextInfo.RefreshBinding();
				NACCSStateMachineBuilder.Build(entryHeader).TransitOn(NACCSStateMachineBuilder.BuildRequest(message));

				return true;
			}
			else
			{
				return false;
			}
		}

		EDIMessage CreateEDIMessage()
		{
			var message = entryHeader.Factory.New<EDIMessage>();
			message.ProcedureCode = procedureCode;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_SendWithMessageErrors = declaration.HasMessageErrors;
			message.EM_GP = declaration.Credential?.PK ?? ZGuid.Empty;

			try
			{
				message.EM_MessageData = MessageData;
				entryHeader.Messages.Add(message);

				message.EM_MessageNumInfo.ValueChanged += (s, e) =>
				{
					var newHeader = JPMessageUtils.ConvertStringToMessage(JPMessageUtils.ConvertMessageToString(JPMessageUtils.ExtractHeader(message.EM_MessageData)).Replace(EDIMessage.MessageNumberPlaceHolder.PadRight(EDIMessage.MessageReferenceLength), message.EM_MessageNum.Right(EDIMessage.MessageReferenceLength)));
					Array.Copy(newHeader, message.EM_MessageData, newHeader.Length);

					var interchange = message.Interchange;
					if (interchange != null)
					{
						interchange.EI_InterchangeNum = message.EM_MessageNum;
						interchange.EI_BodyData = message.EM_MessageData;
					}
					LogAuthorised(message);
				};
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				message.Delete();

				if (ex is JPMessageSchemaException)
				{
					var errors = new System.Collections.Specialized.StringCollection { ex.Message };
					MessageInitiator.MessageSendErrorAlert(errors);

					return null;
				}
				else
				{
					throw;
				}
			}

			return message;
		}

		void CreateMessageAttach(EDIMessage message)
		{
			if (sendingObject is MSXMessageSendingObject msxMessageSendingObject)
			{
				foreach (MSXMessageSendingObjectAttachment attachment in msxMessageSendingObject.Attachments)
				{
					var messageAttach = message.MessageAttachments.AddNew();
					messageAttach.EG_StorageDocsGuid = attachment.File;
					messageAttach.EG_FileName = attachment.Document.FileName;
					messageAttach.EG_EdiMsgDocType = attachment.Type;
				}
			}
		}

		const string ErrorMessageDeclarationMustHasInvoice = "Could not find the Invoice for this messaging operation.";

		protected override bool ValidateJob()
		{
			var result = base.ValidateJob();
			if (result)
			{
				var declaration = entryHeader.Declaration;

				if (declaration != null && declaration.Invoices.Count == 0 && !declaration.IsECRSendingInProgress)
				{
					MessageInitiator.NotifyUserOfAnInvalidOperation(ErrorMessageDeclarationMustHasInvoice);
					result = false;
				}
			}
			return result;
		}

		protected override string SuccessfulSendNotification => Res.GetString("49F4732F-614D-4986-917A-7508E2F67BE5", "Message queued for sending.");

		static JobDeclarationMessageSendingObject GetSendingObject(MessageVisualObject visualObject)
		{
			return visualObject?.ContentProvider is MessageContentProvider messageContentProvider ? messageContentProvider.SendingObject : null;
		}

		byte[] MessageData => messageData ??= (visualObject?.BuildMessage() ?? NACCSMessageBuilder.BuildNACCSMessage(sendingObject));
		byte[] messageData;

		protected override bool ShouldNotifyUserOfASuccessfulSend => false;
	}
}
