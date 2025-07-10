using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public abstract class ExportDeclarationSender
	{
		protected ExportDeclarationSender(CusEntryHeader entryHeader, ZString messageName)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			outboundMessageDetails = ExportDeclarationMessageBuilderLoader.Instance.GetOutboundMessageDetailsForCurrentVersion(messageName);
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly OutboundMessageDetails outboundMessageDetails;

		public void Send()
		{
			var messageBuilder = (IProduceMessageXml)Activator.CreateInstance(outboundMessageDetails.MessageBuilderType, DataProvider);
			var message = entryHeader.Factory.New<AesEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = MessageSubType;
			message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
			message.EM_LinkedObject = entryHeader;
			message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
			message.SetLogbookRegistrationNumber(DataProvider.AESHeader.MRN);
			message.SetLogbookEORIBranchSuffix(DataProvider.InterchangeSender.EoriBranchSuffix);
			message.SetLogbookLocalReferenceNumber(DataProvider.AESHeader.LocalReferenceNumber);

			entryHeader.Messages.Add(message);
			entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.Sent;
		}

		protected abstract ZString MessageSubType { get; }

		protected abstract IAESMessageHeader DataProvider { get; }
	}
}
