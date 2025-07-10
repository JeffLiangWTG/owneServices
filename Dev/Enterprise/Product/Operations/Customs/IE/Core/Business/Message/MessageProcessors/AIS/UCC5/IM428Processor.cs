using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using IM428Provider = Enterprise.Customs.IE.Messaging.UCC5.IM428Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM428Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM428Provider>
	{
		public IM428Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("69F0DDFE-DECB-4747-9BB6-CAABC09076D6", "IM428: MRN allocation message");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM428Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM428Provider provider) => AISEntryStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(IM428MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM428Provider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader && provider.DeclarationAcceptanceDate.TryParseToDate(out var acceptanceDate))
			{
				entryHeader.MovementReferenceNumberSetter(provider.MovementReferenceNumber, acceptanceDate);
			}
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.PopulateConfirmedDutiesAndTaxes(provider.GoodsItems);
			}
		}
	}
}
