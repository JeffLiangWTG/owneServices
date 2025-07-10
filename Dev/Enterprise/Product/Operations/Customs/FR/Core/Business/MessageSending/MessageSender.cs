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
	public abstract class MessageSender<TObjectToSend> : IMessageSender where TObjectToSend : IMessageSendingObject
	{
		protected MessageSender(TObjectToSend objectToSend, ErrorCollector errorCollector)
		{
			this.objectToSend = Argument.NotNull(objectToSend, nameof(objectToSend));
			this.errorCollector = Argument.NotNull(errorCollector, nameof(errorCollector));
		}

		protected ZString GetEDIMessageText()
		{
			var builderManager = GetBuilderManager();
			var messageBuilder = builderManager.NewMessageBuilder(objectToSend);
			messageType = builderManager.BuilderType;
			return messageBuilder.GetMessage();
		}

		protected abstract MessageBuilderManager<TObjectToSend> GetBuilderManager();

		bool IMessageSender.SendAndThrowExceptionIfAny(out string result)
		{
			result = SendAndThrowExceptionIfAny();
			return result == MessageSendSuccessful;
		}

		public string SendAndThrowExceptionIfAny()
		{
			return Send(throwException: true, false);
		}

		bool IMessageSender.Send(bool shouldDelaySave, out string result)
		{
			result = Send(shouldDelaySave);
			return result == MessageSendSuccessful;
		}

		public string Send(bool shouldDelaySave = false)
		{
			return Send(throwException: false, shouldDelaySave);
		}

		string Send(bool throwException, bool shouldDelaySave)
		{
			var result = ZString.Empty;
			try
			{
				if (PreSend())
				{
					if (objectToSend is DeltaIEJobDeclarationMessageSendingObject deltaIEObjectToSend && !deltaIEObjectToSend.ShouldSend)
					{
						result = FormattableString.Invariant($"{MessageUnselectedEntryNotSent}\n{errorCollector.GetErrorsAsString()}");
					}
					else
					{
						var messageContent = GetEDIMessageText();
						if (errorCollector.ErrorCount == 0)
						{
							PreCreateEdiMessage();
							var message = CreateEdiMessage(messageContent);
							PostCreateEdiMessage(message);

							if (!shouldDelaySave && !IsAutoSendCustomsMessageProcessor)
							{
								objectToSend.MessagesOwner.Factory.Save();
							}

							result = MessageSendSuccessful;
						}
						else
						{
							result = FormattableString.Invariant($"{MessageSendFailure}\n{errorCollector.GetErrorsAsString()}");
						}
					}
				}
				else
				{
					result = FormattableString.Invariant($"{MessageSendFailure}\n{errorCollector.GetErrorsAsString()}");
				}
			}
			catch (ZSaveException ex)
			{
				if (throwException)
				{
					throw;
				}
				ZExceptionReporting.HandleSaveException(ex);
				result = FormattableString.Invariant($"{MessageCreateFailure}\n{ex.Message}");
				errorCollector.AddError(result);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (throwException)
				{
					throw;
				}
				ErrorReporter.ReportOnce(MessageSendFailure, ex);
				result = FormattableString.Invariant($"{MessageSendFailure}\n{ex.Message}");
				errorCollector.AddError(result);
			}
			finally
			{
				PostSend(result == MessageSendSuccessful);
			}
			return result;
		}

		protected virtual bool PreSend()
		{
			return true;
		}

		protected virtual void PreCreateEdiMessage()
		{
		}

		protected virtual void PostSend(ZBool result)
		{
		}

		protected virtual void PostCreateEdiMessage(EDIMessage message)
		{
		}

		protected virtual Type GetTypeOfMessageToSend()
		{
			return typeof(FREDIMessage);
		}

		protected FREDIMessage CreateEdiMessage(ZString messageContent)
		{
			var message = objectToSend.MessagesOwner.Messages.AddNew(GetTypeOfMessageToSend());
			message.EM_MessageText = messageContent;
			message.EM_ApplicationCode = ApplicationCode;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.MessageNumberStrategy = new FRMessageNumberStrategy(message.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = objectToSend.MessageType;
			MessageDecorator?.Invoke(message);
			message.Saved += Message_Saved;
			message.Saving += Message_Saving;
			return message;
		}

		public Action<EDIMessage> MessageDecorator { get; set; }

		public Action<EDIMessage> MessageDecoratorForSaving { get; set; }

		protected virtual ZString ApplicationCode => FREDIMessage.ApplicationCodes.FRCustomsMessage;

		protected virtual void Message_Saving(EDIMessage message)
		{
			MessageDecoratorForSaving?.Invoke(message);
		}

		void Message_Saved(EDIMessage message, bool saveSucceeded)
		{
			message.Saved -= Message_Saved;

			if (!saveSucceeded && !message.IsInDatabase)
			{
				if (!message.IsDeleted)
				{
					message.Delete();
				}
			}
		}

		protected readonly ErrorCollector errorCollector;
		protected readonly TObjectToSend objectToSend;
		public ZString messageType = ZString.Empty;

		public bool IsAutoSendCustomsMessageProcessor { get; set; }

		public static ZString MessageSendSuccessful => Res.GetString("ce845aeb-0e1f-43db-9141-b835dbdc9841", "Message sent successfully");

		public static ZString MessageSendFailure => Res.GetString("135bf4fb-2ed8-45ba-8f3f-37c5f5187915", "Failed to send message");

		public static ZString MessageCreateFailure => Res.GetString("7f348837-c65b-4b0d-817b-a04d705feada", "Failed to create message");

		public static ZString MessageUnselectedEntryNotSent => Res.GetString("CB012919-7FC1-49B2-88D0-0FB4602D4331", "Unselected entry not sent");
	}
}
