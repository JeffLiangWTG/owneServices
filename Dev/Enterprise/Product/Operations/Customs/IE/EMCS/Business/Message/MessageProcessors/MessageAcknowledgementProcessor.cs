using System;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class MessageAcknowledgementProcessor : IE.Business.MessageAcknowledgementProcessor
	{
		public MessageAcknowledgementProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override void ProcessMessageCore(EDIMessage targetMessage)
		{
			linkedEMCSDeclaration = (EMCSJobDeclaration)targetMessage.EM_LinkedObject;

			base.ProcessMessageCore(targetMessage);

			var transaction = GetMappedData(targetMessage).transactionProvider;
			if (transaction != null)
			{
				targetMessage.EM_MessageInterpretation = GetInterpretation(transaction);
			}
		}

		ZString GetInterpretation(ITransaction transaction)
		{
			var interpretation = ZString.Empty;
			if (!string.IsNullOrEmpty(transaction.MessageStatus) && !string.IsNullOrEmpty(transaction.TransactionIdStatus))
			{
				var htmlBuilder = new ZStringBuilder((NoResString)"Message Acknowledgement received.");
				htmlBuilder.AppendLine();
				htmlBuilder.Append($"Message Status: {transaction.MessageStatus.ToUpper()}");
				htmlBuilder.AppendLine();
				htmlBuilder.Append($"Transaction ID Status: {transaction.TransactionIdStatus.ToUpper()}");

				interpretation = htmlBuilder.ToStringWithDelimiterBetweenAppends(HtmlResponseEmailGenerator.HtmlConstants.Br);
			}
			return interpretation;
		}

		protected sealed override IRegistryItem GetEmailGroupRegistryItem()
		{
			if (linkedEMCSDeclaration != null)
			{
				return linkedEMCSDeclaration.IsConsignor ? EmcsCustomsDataRegistry.Instance.EmcsSendConsignorAcknowledgements : EmcsCustomsDataRegistry.Instance.EmcsSendConsigneeAcknowledgements;
			}
			else
			{
				return base.GetEmailGroupRegistryItem();
			}
		}

		protected override void SendFailureNotification<T>(EDIMessage originalMessage, ITransaction transaction, EDIMessage targetMessage)
		{
			if (originalMessage.EM_LinkedObject is EMCSJobDeclaration emcsDeclaration)
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(
					factory: originalMessage.Factory,
					relatedJob: emcsDeclaration,
					messageTypeInSubject: originalMessage.MessageTypeWithDescription,
					body: ((T)Activator.CreateInstance(typeof(T), originalMessage, transaction)).GetInterpretation(),
					isFailure: true,
					branchForEmailLogo: emcsDeclaration.Branch,
					sourceBusinessObject: emcsDeclaration,
					getEmailAddressToSendTo: () => GetEmailAddressToSendToFromQueuedUser(originalMessage)
				);
			}
		}

		EMCSJobDeclaration linkedEMCSDeclaration;
	}
}
