using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Messaging.MessageProcessors
{
	/*
	 * THIS IS FOR CUSTOMS TEAM USE ONLY.
	 * IT WILL ADD ALL RECIPIENTS FOR SYSTEM COMMUNICATION, THAT IS IT WILL IGNORE THE EMAIL DESTINATION OVERRIDE FROM THE REGISTRY.
	 */
	public abstract class CustomsMessageProcessor : MessageProcessor
	{
		protected CustomsMessageProcessor(LoggingInformation logger, Func<EDIMessage, string> getMessageType, string messageFriendlyName)
			: base(logger, getMessageType, messageFriendlyName)
		{
		}

		protected CustomsMessageProcessor(LoggingInformation logger, string messageType3CharCode, string messageFriendlyName)
			: this(logger, x => messageType3CharCode, messageFriendlyName)
		{
		}

		protected override void AddRecipientCore(EmailDef emailDef, string email, RecipientDef.RecipientTypes type)
		{
			emailDef.AddRecipientForSystemCommunication(email, type);
		}
	}
}
