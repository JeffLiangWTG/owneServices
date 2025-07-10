using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public abstract class ResponseMessageProcessor : CustomsMessageProcessor, IErrorNotification
	{
		protected ResponseMessageProcessor(LoggingInformation logger, ZString messageTypeCode, ZString messageTypeDescription)
			: base(logger, messageTypeCode, messageTypeDescription)
		{
		}

		protected ResponseMessageProcessor(LoggingInformation logger, Func<Enterprise.Messaging.Business.EDIMessage, string> getMessageType, ZString messageTypeDescription)
			: base(logger, getMessageType, messageTypeDescription)
		{
		}

		#region Implementation of IErrorNotification

		void IErrorNotification.SendError(EmailDef email)
		{
			SendErrorReport(EmailResponseLinkedObject, email);
		}

		void IErrorNotification.SendErrorToPostMaster(EmailDef email)
		{
			if (ErrorEmailGroup != Env.Registry.PostMasterGroup)
			{
				SendReport(email, EmailResponseLinkedObject, Core.Constants.EmailTo.NominatedGroup, Env.Registry.PostMasterGroup);
			}
		}

		void IErrorNotification.LogError(string errorMessage)
		{
			Logger.LogError(errorMessage);
		}

		string IErrorNotification.MessageProcessorName
		{
			get { return MessageFriendlyName; }
		}

		#endregion

		protected GlbBranch GetMessageBranchAndSetOnMessage(IEDIFACTMessageAttachee linkedObject, Enterprise.Messaging.Business.EDIMessage message)
		{
			Enterprise.Messaging.Business.EDIMessage lastSendMessage =
				linkedObject.Messages.GetLastMessage(message.EM_ApplicationCode, message.EM_MessageType, Enterprise.Messaging.Business.EDIMessage.Direction.Transmit);

			GlbBranch messageBranch = (lastSendMessage != null ? lastSendMessage.Branch : message.Branch) ?? GlbBranch.CurrentBranch;
			message.EM_GB = messageBranch.PK;
			return messageBranch;
		}

		protected static string MessageSender
		{
			get { return Res.GetString("7b243257-4fcb-4f0c-8834-ba1507eb5fa1", "from the CBSA") + " "; }
		}

		protected abstract BusinessObject EmailResponseLinkedObject { get; }
	}
}
