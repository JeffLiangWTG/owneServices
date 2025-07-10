using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.AcknowledgementMessage;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AcknowledgementMessageProcessor : NCTSMessageProcessor<IAcknowledgementMessageDataProvider>
	{
		public AcknowledgementMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override Type MessageInterpreterType => typeof(AcknowledgementMessageInterpreter);

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.Acknowledgement };

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.Acknowledgement;

		protected override BusinessObject FindParentOfMessage(BEMessage message, IAcknowledgementMessageDataProvider messageDataProvider) => NctsMessageHelper.LocateLinkedObjectByEdiInterchange(message.Interchange);

		protected override IAcknowledgementMessageDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<AcknowledgementMessage, AcknowledgementMessageDataProvider>();

		protected override void ProcessMessageCore(BEMessage message, IAcknowledgementMessageDataProvider messageDataProvider)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
			CusEntryNumber cidEntryNum = null;
			if (nctsHeader.ArrivalMovementHeader != null)
			{
				cidEntryNum = CusEntryNumber.LoadOrCreate(nctsHeader.ArrivalMovementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader.CountryCode);
			}
			else if (nctsHeader.MovementHeader != null)
			{
				cidEntryNum = CusEntryNumber.LoadOrCreate(nctsHeader.MovementHeader, CusEntryNumberTypes.EU.CorrelationIdentifier, nctsHeader.CountryCode);
			}

			if (cidEntryNum != null)
			{
				cidEntryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				cidEntryNum.CE_EntryNum = messageDataProvider.CorrelationId;
				cidEntryNum.CE_EntryLineReference = messageDataProvider.CorrelationId;

				message.EM_Status = EDIMessage.Status.ProcessedOK;
			}
		}
	}
}
