using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CONJobDeclarationB3SendingStrategy : JobDeclarationB3SendingStrategy
	{
		public CONJobDeclarationB3SendingStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region B3 Sending Delay Threshold

		public override ZString B3SendingDelayThresholdType
		{
			get
			{
				var importerAddInfo = Declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation ? null : Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return GetDelayIntervalType(DelayIntervalTypeCodes.Codes.Default, FallBackSendingDelayThresholds().CONDelayIntervalType);
				}
				else
				{
					return GetDelayIntervalType(importerAddInfo.ZO_CONDelayIntervalTypeAutoSend,
						FallBackSendingDelayThresholds().CONDelayIntervalType);
				}
			}
		}

		public override ZInt B3SendingDelayThreshold
		{
			get
			{
				var importerAddInfo = Declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation ? null : Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return GetDelayInterval(B3SendingDelayThresholdType, DelayIntervalTypeCodes.Codes.Default, 0,
						FallBackSendingDelayThresholds().CONDelayInterval);
				}
				else
				{
					return GetDelayInterval(B3SendingDelayThresholdType, importerAddInfo.ZO_CONDelayIntervalTypeAutoSend, importerAddInfo.ZO_CONDelayIntervalAutoSend,
						FallBackSendingDelayThresholds().CONDelayInterval);
				}
			}
		}

		#endregion

		#region B3 Late Sending Failsafe Warning Threshold

		public override ZString B3LateSendingFailsafeWarningThresholdType
		{
			get
			{
				var importerAddInfo = Declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation ? null : Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return GetDelayIntervalType(DelayIntervalTypeCodes.Codes.Default,
						FallBackFailsafeWarningThresholds().CONDelayIntervalType);
				}
				else
				{
					return GetDelayIntervalType(Declaration.ImporterAddInfo.ZO_CONDelayIntervalTypeFailSafe,
						FallBackFailsafeWarningThresholds().CONDelayIntervalType);
				}
			}
		}

		public override ZInt B3LateSendingFailsafeWarningThreshold
		{
			get
			{
				var importerAddInfo = Declaration.JE_MessageSubType == LowValueShipmentsTypes.Codes.TotalConsolidation ? null : Declaration.ImporterAddInfo;
				if (importerAddInfo == null)
				{
					return GetDelayInterval(B3LateSendingFailsafeWarningThresholdType, DelayIntervalTypeCodes.Codes.Default, 0,
						FallBackFailsafeWarningThresholds().CONDelayInterval);
				}
				else
				{
					return GetDelayInterval(B3LateSendingFailsafeWarningThresholdType, importerAddInfo.ZO_CONDelayIntervalTypeFailSafe, importerAddInfo.ZO_CONDelayIntervalFailSafe,
						FallBackFailsafeWarningThresholds().CONDelayInterval);
				}
			}
		}

		#endregion

		#region B3 Late Sending Warning Schedule Date

		protected override ZDateTime GetB3LateSendingWarningScheduleDate()
		{
			switch (B3LateSendingFailsafeWarningThresholdType)
			{
				case DelayIntervalTypeCodes.Codes.DAY:
					return GetSendingCalendarDay(B3LateSendingFailsafeWarningThreshold);
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
				case DelayIntervalTypeCodes.Codes.DAY:
					return GetSendingCalendarDay(B3SendingDelayThreshold);
				default:
					return ZDateTime.Empty;
			}
		}

		#endregion
	}
}
