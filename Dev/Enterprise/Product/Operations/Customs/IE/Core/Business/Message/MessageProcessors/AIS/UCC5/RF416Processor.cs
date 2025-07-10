using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using RF416Provider = Enterprise.Customs.IE.Messaging.UCC5.RF416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	class RF416Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, RF416Provider>
	{
		public RF416Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, RF416Provider provider) => LogicalStatusList.Codes.Invalid;

		protected override string MessageFriendlyNameCore => CommonResStrings.RF416MessageFriendlyName;

		protected override Type MessageInterpreterType => typeof(RF416MessageInterpreter);

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;
	}
}
