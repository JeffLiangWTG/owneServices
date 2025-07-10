using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS333Processor : AISMessageProcessor<AISInboundEDIMessage, TS333Provider>
	{
		public TS333Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => CommonResStrings.TS333MessageFriendlyName;

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS333Provider provider) => HasFunctionalErrors(provider) ? LogicalStatusList.Codes.Invalid : base.GetLogicalStatus(message, messageAttachee, provider);

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS333Provider provider) => HasFunctionalErrors(provider) ? AISEntryStatusList.Codes.Rejected : AISEntryStatusList.Codes.Registered;

		protected override Type MessageInterpreterType => typeof(TS333MessageInterpreter);

		bool HasFunctionalErrors(TS333Provider provider)
		{
			return provider.FunctionalErrors.Any();
		}
	}
}
