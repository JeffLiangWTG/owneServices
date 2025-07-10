using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class DummyJobDeclarationB3SendingStrategy : JobDeclarationB3SendingStrategy
	{
		public DummyJobDeclarationB3SendingStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public ZString B3SendingDelayThresholdTypeForTesting = DelayIntervalTypeCodes.Codes.None;
		public ZInt B3SendingDelayThresholdForTesting = 0;
		public ZString B3LateSendingFailsafeWarningThresholdTypeForTesting = DelayIntervalTypeCodes.Codes.None;
		public ZInt B3LateSendingFailsafeWarningThresholdForTesting = 0;
		public bool ShouldAutoSendB3MessageForTesting;
		public ZDateTime ScheduledB3AutoSendingDateForTesting;
		public ZDateTime B3LateSendingWarningScheduleDateForTesting;

		public override ZString B3SendingDelayThresholdType
		{
			get { return B3SendingDelayThresholdTypeForTesting; }
		}

		public override ZInt B3SendingDelayThreshold
		{
			get { return B3SendingDelayThresholdForTesting; }
		}

		public override ZString B3LateSendingFailsafeWarningThresholdType
		{
			get { return B3LateSendingFailsafeWarningThresholdTypeForTesting; }
		}

		public override ZInt B3LateSendingFailsafeWarningThreshold
		{
			get { return B3LateSendingFailsafeWarningThresholdForTesting; }
		}

		public override bool ShouldAutoSendB3Message
		{
			get { return ShouldAutoSendB3MessageForTesting; }
		}

		protected override ZDateTime GetB3LateSendingWarningScheduleDate()
		{
			return B3LateSendingWarningScheduleDateForTesting;
		}

		protected override ZDateTime GetScheduledB3AutoSendingDate()
		{
			return ScheduledB3AutoSendingDateForTesting;
		}
	}
}
