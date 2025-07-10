using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public abstract class NctsHeaderSender
	{
		protected NctsHeaderSender(NctsHeaderMessageSendingObject sendingObject)
		{
			Argument.NotNull(sendingObject, nameof(sendingObject));
			nctsHeader = Argument.NotNull(sendingObject.NctsHeader, nameof(sendingObject.NctsHeader)) as NctsHeader;
			var messageDetails = NctsMessageBuilderLoader.Instance.GetMessageDetails(sendingObject.MessageType);
			if (messageDetails.ProviderType.GetInterface(nameof(INCTSMessageHeader)) != null)
			{
				messageHeaderProvider = (INCTSMessageHeader)Activator.CreateInstance(messageDetails.ProviderType, nctsHeader);
			}
			messageBuilder = (IProduceMessageXml)Activator.CreateInstance(messageDetails.MessageBuilderType, messageHeaderProvider);
		}
		protected readonly NctsHeader nctsHeader;
		readonly INCTSMessageHeader messageHeaderProvider;
		readonly IProduceMessageXml messageBuilder;

		public bool Send()
		{
			var shouldContinueToSend = PreSend();
			if (shouldContinueToSend)
			{
				var message = nctsHeader.Factory.New<AtlasEDIMessage>();
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
				message.EM_MessageSubType = MessageSubType;
				message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
				message.EM_LinkedObject = nctsHeader.IsPhase5Departure ? nctsHeader.MovementHeader : nctsHeader;
				message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
				message.SetLogbookRegistrationNumber(LogbookRegistrationNumber);
				message.SetLogbookEORIBranchSuffix(messageHeaderProvider.InterchangeSender.EoriBranchSuffix);
				message.SetLogbookLocalReferenceNumber(LocalReferenceNumber);

				nctsHeader.EffectiveMessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
				if (ShouldSetPhaseStatus(out var phase))
				{
					nctsHeader.CommonMovementHeader.BM_Phase = phase;
				}

				if (ShouldSetCustomsStatus(out var customsStatus))
				{
					nctsHeader.CommonMovementHeader.BM_CustomsStatus = customsStatus;
				}

				if (nctsHeader.IsPhase5Departure)
				{
					nctsHeader.MovementHeader.Messages.Add(message);
				}
				else
				{
					nctsHeader.Messages.Add(message);
				}
			}

			return shouldContinueToSend;
		}

		protected abstract string MessageSubType { get; }

		protected abstract string LocalReferenceNumber { get; }

		protected abstract string LogbookRegistrationNumber { get; }

		protected virtual bool ShouldSetPhaseStatus(out ZString status)
		{
			status = ZString.Empty;
			return false;
		}

		protected virtual bool ShouldSetCustomsStatus(out ZString status)
		{
			status = ZString.Empty;
			return false;
		}

		protected INCTSHeader DataProvider => messageHeaderProvider.Header;

		protected virtual bool PreSend() => true;
	}
}
