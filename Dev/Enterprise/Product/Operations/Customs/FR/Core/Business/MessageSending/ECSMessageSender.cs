using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class ECSMessageSender
	{
		protected readonly CusExitDetail objectToSend;

		public ECSMessageSender(CusExitDetail exitDetail, ZString messageSubType, ErrorCollector errorCollector)
		{
			objectToSend = Argument.NotNull(exitDetail, nameof(exitDetail));
			this.errorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
			this.messageSubType = messageSubType;
		}
		readonly ZString messageSubType;

		protected ZString GetEDIMessageText()
		{
			return MessageBuilderManager.NewMessageBuilder(messageSubType).GetMessage();
		}

		public string Send()
		{
			ZString result;

			try
			{
				var messageContent = GetEDIMessageText();

				if (ErrorCollector.ErrorCount == 0)
				{
					var message = CreateEdiMessage(messageContent);

					objectToSend.Factory.Save();

					result = MessageSendSuccessful;
				}
				else
				{
					result = FormattableString.Invariant($"{MessageSendFailure}\n{ErrorCollector.GetErrorsAsString()}");
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				result = MessageCreateFailure;
				ErrorCollector.AddError(result);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = FormattableString.Invariant($"{MessageSendFailure}\n{ex.Message}");
				ErrorCollector.AddError(result);
			}

			return result;
		}

		EDIMessage CreateEdiMessage(ZString messageContent)
		{
			var message = MessageBuilderManager.AddNewMessage(messageSubType);
			message.EM_MessageText = messageContent;
			message.EM_Status = EDIMessage.Status.Queued;
			message.MessageNumberStrategy = new FRMessageNumberStrategy(message.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			message.Saved += message_Saved;

			return message;
		}

		#region Events

		void message_Saved(EDIMessage message, bool saveSucceeded)
		{
			message.Saved -= message_Saved;

			if (!saveSucceeded && !message.IsInDatabase && !message.IsDeleted)
			{
				message.Delete();
			}
		}

		#endregion

		ErrorCollector ErrorCollector => errorCollector ?? (errorCollector = new ErrorCollector());
		ErrorCollector errorCollector;

		ECSMessageBuilderManager MessageBuilderManager => messageBuilderManager ?? (messageBuilderManager = new ECSMessageBuilderManager(objectToSend));
		ECSMessageBuilderManager messageBuilderManager;

		public static ZString MessageSendSuccessful => Res.GetString("ce845aeb-0e1f-43db-9141-b835dbdc9841", "Message sent successfully");

		public static ZString MessageSendFailure => Res.GetString("135bf4fb-2ed8-45ba-8f3f-37c5f5187915", "Failed to send message");

		public static ZString MessageCreateFailure => Res.GetString("7f348837-c65b-4b0d-817b-a04d705feada", "Failed to create message");
	}
}
