using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.Testing
{
	class InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet : InboundInterchangeProcessorForTesting
	{
		public InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZString GetReasonForCannotProcessInterchangeCore() => reason;

		public ZString reason = "Some Reason";
	}
}
