using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class GUACODSender
	{
		public GUACODSender(SendAccessCodeViewModel sendAccessCode)
		{
			this.sendAccessCode = Argument.NotNull(sendAccessCode, nameof(sendAccessCode));
			guaranteeHeader = this.sendAccessCode.GuaranteeHeader;
			var messageDetails = NctsMessageBuilderLoader.Instance.GetMessageDetails(NctsMessageTypeList.Codes.GUACOD);
			messageHeaderProvider = (INCTSMessageHeader)Activator.CreateInstance(messageDetails.ProviderType, this.sendAccessCode);
			messageBuilder = (IProduceMessageXml)Activator.CreateInstance(messageDetails.MessageBuilderType, messageHeaderProvider);
		}
		readonly SendAccessCodeViewModel sendAccessCode;
		readonly CusGuaranteeHeader guaranteeHeader;
		readonly INCTSMessageHeader messageHeaderProvider;
		readonly IProduceMessageXml messageBuilder;

		public void Send()
		{
			var message = sendAccessCode.GuaranteeHeader.Factory.New<AtlasEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			message.EM_MessageSubType = NctsMessageSubTypeList.Codes.GuaranteeAccessHandling;
			message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
			message.EM_LinkedObject = sendAccessCode.GuaranteeHeader;
			message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
			message.SetLogbookLocalReferenceNumber(sendAccessCode.GuaranteeNumber);
			message.CreateOrUpdateNote(LogbookHelper.LogbookGUAMainAccessCode, sendAccessCode.NewMainAccessCode);
			message.SetLogbookEORIBranchSuffix(messageHeaderProvider.InterchangeSender.EoriBranchSuffix);
			message.EM_LinkedObject = guaranteeHeader;

			message.Factory.Save();
		}
	}
}
