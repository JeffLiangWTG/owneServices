using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS333Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS333Provider>
	{
		public TS333Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS333MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS333Provider provider) => HasFunctionalErrors(provider) ? LogicalStatusList.Codes.Invalid : base.GetLogicalStatus(message, messageAttachee, provider);

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS333Provider provider) => HasFunctionalErrors(provider) ? AISEntryStatusList.Codes.Rejected : AISEntryStatusList.Codes.Registered;

		protected override Type MessageInterpreterType => typeof(TS333MessageInterpreter);

		bool HasFunctionalErrors(Messaging.UCC5.TS333Provider provider)
		{
			return provider.FunctionalErrors.Any();
		}
	}
}
