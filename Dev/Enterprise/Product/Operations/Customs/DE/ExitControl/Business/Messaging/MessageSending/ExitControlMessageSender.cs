using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public abstract class ExitControlMessageSender
	{
		protected ExitControlMessageSender(ExitControlMessageSendingObject sendingObject)
		{
			SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			ExitReport = (CusExitReport)SendingObject.MessagingObject;

			var messageDetails = ExitDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion(MessageType);
			if (messageDetails != null)
			{
				var providerType = messageDetails.ProviderType;
				if (providerType.GetInterface(nameof(IExitMessageHeader)) != null)
				{
					messageHeaderProvider = (IExitMessageHeader)Activator.CreateInstance(providerType, ExitReport);
					messageBuilder = (IProduceMessageXml)Activator.CreateInstance(messageDetails.MessageBuilderType, messageHeaderProvider);
				}
			}

			Argument.NotNull(messageHeaderProvider, nameof(messageHeaderProvider));
			Argument.NotNull(messageBuilder, nameof(messageBuilder));
		}
		readonly IExitMessageHeader messageHeaderProvider;
		readonly IProduceMessageXml messageBuilder;

		public void Send()
		{
			PreSend();

			var message = ExitReport.Factory.New<AesEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = EDIMessageTypeList.Codes.AES;
			message.EM_MessageSubType = MessageSubType;
			message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
			message.EM_LinkedObject = ExitReport;
			message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
			message.SetLogbookRegistrationNumber(LogbookRegistrationNumber);
			message.SetLogbookEORIBranchSuffix(messageHeaderProvider.InterchangeSender.EoriBranchSuffix);
			message.SetLogbookLocalReferenceNumber(LocalReferenceNumber);
			ExitReport.Messages.Add(message);

			ExitReport.CER_MessageStatus = LogicalStatusList.Codes.Sent;
		}

		protected string MessageSubType => ExportMessageSubTypeList.Codes.EXT;

		protected ExitControlMessageSendingObject SendingObject { get; }

		protected CusExitReport ExitReport { get; }

		protected abstract string MessageType { get; }

		protected virtual string LocalReferenceNumber => ExitReport.Consignment.CXC_LocalReference;

		protected virtual string LogbookRegistrationNumber => ExitReport.Consignment.CXC_MovementReference;

		protected IExitHeader DataProvider => messageHeaderProvider.ExitHeader;

		protected virtual void PreSend()
		{
		}
	}
}
