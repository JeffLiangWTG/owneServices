using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Business;
using IM429Provider = Enterprise.Customs.IE.Messaging.UCC5.IM429Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM429Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM429Provider>
	{
		public IM429Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.IM429MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM429Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM429Provider provider) => AISEntryStatusList.Codes.Released;

		protected override Type MessageInterpreterType => typeof(IM429MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM429Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				aisMessageAttachee.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.Released, ZDateTime.Now.ToOffset()));
				aisMessageAttachee.PopulateConfirmedDutiesAndTaxes(provider.GoodsItems);
			}
		}

		protected override string GetEmailSubject(AISUCC5InboundEDIMessage message) => CommonResStrings.IM429MessageFriendlyName;
	}
}
