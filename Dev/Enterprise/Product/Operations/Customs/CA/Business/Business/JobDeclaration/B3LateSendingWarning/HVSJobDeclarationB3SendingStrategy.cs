using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class HVSJobDeclarationB3SendingStrategy : JobDeclarationB3SendingStrategy
	{
		public HVSJobDeclarationB3SendingStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region B3 Sending Delay Threshold

		public override ZString B3SendingDelayThresholdType
		{
			get
			{
				var importerAddInfo = Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return FallBackSendingDelayThresholds().HVSDelayIntervalType;
				}
				else
				{
					return GetDelayIntervalType(importerAddInfo.ZO_HVSDelayIntervalTypeAutoSend, FallBackSendingDelayThresholds().HVSDelayIntervalType);
				}
			}
		}

		public override ZInt B3SendingDelayThreshold
		{
			get
			{
				var importerAddInfo = Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return FallBackSendingDelayThresholds().HVSDelayInterval;
				}
				else
				{
					return GetDelayInterval(B3SendingDelayThresholdType, importerAddInfo.ZO_HVSDelayIntervalTypeAutoSend, importerAddInfo.ZO_HVSDelayIntervalAutoSend,
						FallBackSendingDelayThresholds().HVSDelayInterval);
				}
			}
		}

		#endregion

		#region B3 Late Sending Failsafe Warning Threshold

		public override ZString B3LateSendingFailsafeWarningThresholdType
		{
			get
			{
				var importerAddInfo = Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return FallBackFailsafeWarningThresholds().HVSDelayIntervalType;
				}
				else
				{
					return GetDelayIntervalType(importerAddInfo.ZO_HVSDelayIntervalTypeFailSafe,
						FallBackFailsafeWarningThresholds().HVSDelayIntervalType);
				}
			}
		}

		public override ZInt B3LateSendingFailsafeWarningThreshold
		{
			get
			{
				var importerAddInfo = Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return FallBackFailsafeWarningThresholds().HVSDelayInterval;
				}
				else
				{
					return GetDelayInterval(B3LateSendingFailsafeWarningThresholdType, importerAddInfo.ZO_HVSDelayIntervalTypeFailSafe, importerAddInfo.ZO_HVSDelayIntervalFailSafe,
						FallBackFailsafeWarningThresholds().HVSDelayInterval);
				}
			}
		}

		#endregion

		#region B3 Late Sending Warning Schedule Date

		protected override ZDateTime GetB3LateSendingWarningScheduleDate()
		{
			switch (B3LateSendingFailsafeWarningThresholdType)
			{
				case DelayIntervalTypeCodes.Codes.DAR:
					return GetLateSendingWarningScheduledDate(Declaration.JE_EntryAuthorisationDate);
				default:
					return ZDateTime.Empty;
			}
		}

		#endregion

		#region Auto B3 Send Date

		protected override ZDateTime GetScheduledB3AutoSendingDate()
		{
			switch (B3SendingDelayThresholdType)
			{
				case DelayIntervalTypeCodes.Codes.DAR:
					return GetB3AutoSendingScheduledDate(Declaration.JE_EntryAuthorisationDate);
				default:
					return ZDateTime.Empty;
			}
		}

		#endregion
	}
}
