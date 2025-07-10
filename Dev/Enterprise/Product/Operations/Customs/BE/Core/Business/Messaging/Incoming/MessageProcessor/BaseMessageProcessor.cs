using System;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public abstract class BaseMessageProcessor<T> : BranchCustomsApplicationTypeMessageProcessor where T : class
{
	protected BaseMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected sealed override string ApplicationCodeCore => ApplicationCodeList.Codes.BECustoms;

	protected virtual Type MessageInterpreterType => null;

	protected void DiscardMessage(BEMessage message, string log)
	{
		Logger.LogError(log);
		message.EM_Status = EDIMessage.Status.Discarded;
		message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, log);
	}

	protected sealed override void PreProcessMessageCore(EDIMessage message)
	{
		if (message is BEMessage beMessage && beMessage.EM_Status == EDIMessage.Status.Queued)
		{
			T messageDataProvider;
			try
			{
				messageDataProvider = GetMessageDataProvider(beMessage);
			}
			catch (Exception ex)
			{
				beMessage.EM_Status = EDIMessage.Status.Error;
				var error = ex.Message;
				if (ex.InnerException is XmlSchemaValidationException || ex.InnerException is InvalidOperationException)
				{
					error += " - " + ex.InnerException.Message;
				}
				beMessage.Notes.AddNew(true, Constants.MessageProcessingNotes.ExceptionLog, error);
				Logger.LogError(Res.GetString("1A4A4975-B37E-4476-9D17-BEE891DCEEEB", "Something went wrong: {3}. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to ERROR.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, error));
				return;
			}

			if (message.EM_LinkedObject == null)
			{
				message.EM_LinkedObject = FindParentOfMessage(beMessage, messageDataProvider);
			}
			var parentOfMessage = beMessage.EM_LinkedObject;

			if (parentOfMessage != null)
			{
				var branchPk = GetBranchPkFromJobBO(parentOfMessage);
				if (branchPk.IsValid)
				{
					message.EM_GB = branchPk;
				}

				if (CheckMessageSequenceIsValid(beMessage))
				{
					PreProcessMessageWhenBOFoundCore(beMessage, messageDataProvider);

					if (message.EM_Status == EDIMessage.Status.Queued)
					{
						message.EM_Status = EDIMessage.Status.PreProcessedOK;
					}
					else
					{
						InterpretDiscardedMessage(beMessage, messageDataProvider);
					}
				}
				else
				{
					PreProcessMessageWhenSequenceIsWrong(beMessage, messageDataProvider);
				}
			}
			else
			{
				Logger.LogError(Res.GetString("cafd5b38-586c-494b-bf7e-fb6025b57bff", "Unable to find a linked business object for message (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to {3}.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, StatusForUnableToFindALinkedBusinessObject));
				message.EM_Status = StatusForUnableToFindALinkedBusinessObject;
				var noteForUnableToFindALinkedBusinessObject = NoteForUnableToFindALinkedBusinessObject(messageDataProvider);
				if (!noteForUnableToFindALinkedBusinessObject.IsEmpty)
				{
					message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, noteForUnableToFindALinkedBusinessObject);
				}
			}
		}
	}

	protected virtual ZString StatusForUnableToFindALinkedBusinessObject => EDIMessage.Status.Failed;

	protected virtual ZString NoteForUnableToFindALinkedBusinessObject(T messageDataProvider) => ZString.Empty;

	protected sealed override void ProcessMessageCore(EDIMessage message)
	{
		if (message is BEMessage beMessage && message.EM_Status == EDIMessage.Status.PreProcessedOK)
		{
			var messageDataProvider = GetMessageDataProvider(beMessage);
			ProcessMessageCore(beMessage, messageDataProvider);
			InterpretMessage(beMessage, messageDataProvider);
			UpdateGuaranteeTransactionsIfNeeded(beMessage, messageDataProvider);
		}
	}

	void PreProcessMessageWhenSequenceIsWrong(BEMessage message, T messageDataProvider)
	{
		if (message.EM_RetryCount >= 5)
		{
			var messageSequenceInvalidMessage = MessageSequenceInvalidMessage != null ? $"({MessageSequenceInvalidMessage})" : string.Empty;
			var log = Res.GetString("AD51C21F-F00A-4B18-B305-543EB152C03D", "The message was discarded. Reason: the message was received in wrong sequence {0}, retried {1} times.", messageSequenceInvalidMessage, message.EM_RetryCount);
			DiscardMessage(message, log);
			InterpretDiscardedMessage(message, messageDataProvider);
		}
		else
		{
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddSeconds(message.EM_RetryCount * 60 + 5);
			message.EM_RetryCount++;
			var log = Res.GetString("F906448E-6708-4F26-A368-1A0C80E14E0C", "The message was set back to QUEUED. Reason: the message was received in wrong sequence. Retry Count: {0}. Next Process Time: {1}", message.EM_RetryCount, message.EM_HeldUntilDate);
			Logger.Log(log);
			message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, log);
		}
	}

	protected virtual string MessageSequenceInvalidMessage => null;

	void InterpretMessage(BEMessage message, T messageDataProvider)
	{
		if (MessageInterpreterType is Type interpreterType && interpreterType.IsSubclassOfRawGeneric(typeof(BaseMessageInterpreter<>)))
		{
			var interpreter = (BaseMessageInterpreter<T>)Activator.CreateInstance(interpreterType);
			message.EM_MessageInterpretation = interpreter.Interpret(messageDataProvider, message);
		}
	}

	void InterpretDiscardedMessage(BEMessage message, T messageDataProvider)
	{
		if (MessageInterpreterType is Type interpreterType && interpreterType.IsSubclassOfRawGeneric(typeof(BaseMessageInterpreter<>)))
		{
			var interpreter = (BaseMessageInterpreter<T>)Activator.CreateInstance(interpreterType);
			message.EM_MessageInterpretation = interpreter.InterpretDiscardedMessage(messageDataProvider, message);
		}
	}

	bool CheckMessageSequenceIsValid(BEMessage message) => CheckMessageSequenceIsValidCore(message);

	protected virtual bool CheckMessageSequenceIsValidCore(BEMessage message) => true;

	protected virtual void PreProcessMessageWhenBOFoundCore(BEMessage message, T messageDataProvider)
	{
	}

	protected internal abstract T GetMessageDataProvider(BEMessage message);

	protected abstract void ProcessMessageCore(BEMessage message, T messageDataProvider);

	protected abstract BusinessObject FindParentOfMessage(BEMessage message, T messageDataProvider);

	protected abstract ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject);

	protected virtual void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, T messageDataProvider) { }
}
