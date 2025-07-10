using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS328Processor : AISMessageProcessor<AISInboundEDIMessage, TS328Provider>
	{
		public TS328Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.TS328MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS328Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS328Provider provider) => AISEntryStatusList.Codes.Accepted;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, TS328Provider provider)
		{
			if (messageAttachee is TemporaryStorageHeader temporaryStorageHeader)
			{
				if (!provider.MRN.IsEmpty)
				{
					temporaryStorageHeader.MRN = provider.MRN;
				}
				if (!provider.DateOfAcceptance.IsEmpty)
				{
					temporaryStorageHeader.CustomsStatusDate = provider.DateOfAcceptance;
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(TS328MessageInterpreter);
	}
}
