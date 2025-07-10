using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business
{
	public abstract class JobDeclarationB3SendingStrategy
	{
		public JobDeclarationB3SendingStrategy(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration");
			Declaration = declaration;
		}

		protected readonly JobDeclaration Declaration;

		public static JobDeclarationB3SendingStrategy GetJobDeclarationB3SendingStrategy(JobDeclaration declaration)
		{
#if DEBUG
			if (Globals.IsTest && StrategiesForTesting != null)
			{
				var strategy = StrategiesForTesting.Find(p => p.Declaration == declaration);
				if (strategy != null)
				{
					return strategy;
				}
			}
#endif

			if (declaration.IsImport)
			{
				if (declaration.IsLVS)
				{
					return new CONJobDeclarationB3SendingStrategy(declaration);
				}
				else
				{
					return new HVSJobDeclarationB3SendingStrategy(declaration);
				}
			}

			return null;
		}

		#region B3 Sending Delay Threshold

		public abstract ZString B3SendingDelayThresholdType
		{
			get;
		}

		public abstract ZInt B3SendingDelayThreshold
		{
			get;
		}

		#endregion

		#region B3 Late Sending Failsafe Warning Threshold

		public abstract ZString B3LateSendingFailsafeWarningThresholdType
		{
			get;
		}

		public abstract ZInt B3LateSendingFailsafeWarningThreshold
		{
			get;
		}

		#endregion

		#region B3 Late Sending Warning Schedule Date

		public ZDateTime B3LateSendingWarningScheduleDate
		{
			get { return ShouldAddB3LateSendingWarningEvent ? GetB3LateSendingWarningScheduleDate() : ZDateTime.Empty; }
		}

		protected abstract ZDateTime GetB3LateSendingWarningScheduleDate();

		#endregion

		#region Scheduled B3 Auto-Sending Date

		public ZDateTime ScheduledB3AutoSendingDate
		{
			get
			{
				return ShouldAutoSendB3Message ? GetScheduledB3AutoSendingDate() : ZDateTime.Empty;
			}
		}

		protected abstract ZDateTime GetScheduledB3AutoSendingDate();

		#endregion

		#region Should Add B3 Late Sending Warning Event

		public bool ShouldAddB3LateSendingWarningEvent
		{
			get
			{
				return AllowedMessageTypeList.Contains(Declaration.JE_MessageType)
					&& (Declaration.IsCADEnabled ? AllowedMessageSubTypeListForCAD.Contains(CADEntryTypeList.ConvertToShortCode(Declaration.JE_MessageSubType)) : AllowedMessageSubTypeList.Contains(Declaration.JE_MessageSubType))
					&& !Declaration.CA_CSAEntry
					&& Declaration.JE_EntryAuthorisationDate.IsValid
					&& Declaration.CA_K84AccountingDate.IsEmpty
					&& HasAB3MessageAndNotAccepted(true)
					&& B3LateSendingFailsafeWarningThresholdType != DelayIntervalTypeCodes.Codes.None;
			}
		}

		#endregion

		#region Should Auto Send B3 Message

		public virtual bool ShouldAutoSendB3Message
		{
			get
			{
				return AllowedMessageTypeList.Contains(Declaration.JE_MessageType)
					&& (Declaration.IsCADEnabled ? AllowedMessageSubTypeListForCAD.Contains(CADEntryTypeList.ConvertToShortCode(Declaration.JE_MessageSubType)) : AllowedMessageSubTypeList.Contains(Declaration.JE_MessageSubType))
					&& Declaration.JE_EntryAuthorisationDate.IsValid
					&& Declaration.CA_K84AccountingDate.IsEmpty
					&& HasAB3MessageAndNotAccepted(false)
					&& B3SendingDelayThresholdType != DelayIntervalTypeCodes.Codes.None;
			}
		}

		#endregion

		#region Utility

		internal static readonly ImmutableArray<string> AllowedMessageTypeList = ImmutableArray.Create(
				JobMessageTypeList.Codes.Import,
				JobMessageTypeList.Codes.LowValueShipments
			);

		internal static readonly ImmutableArray<string> AllowedMessageSubTypeList = ImmutableArray.Create(
			LowValueShipmentsTypes.Codes.TotalConsolidation,
			LowValueShipmentsTypes.Codes.ConsolidationByImporter,
			B3EntryTypeList.Codes.Confirming,
			B3EntryTypeList.Codes.Warehouse10,
			B3EntryTypeList.Codes.ReWarehouse13,
			B3EntryTypeList.Codes.ExWarehouse20,
			B3EntryTypeList.Codes.ExWarehouse21,
			B3EntryTypeList.Codes.ExWarehouse22,
			B3EntryTypeList.Codes.TransferOfGoods30
		);

		internal static readonly ImmutableArray<string> AllowedMessageSubTypeListForCAD = ImmutableArray.Create(
			LowValueShipmentsTypes.Codes.TotalConsolidation,
			LowValueShipmentsTypes.Codes.ConsolidationByImporter,
			CADEntryTypeList.Codes.Confirming,
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.Warehouse101),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.Warehouse102),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ReWarehouse131),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ReWarehouse132),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse201),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse211),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse212),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse213),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse214),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse215),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.ExWarehouse216),
			CADEntryTypeList.Codes.ExWarehouse22,
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.TransferOfGoods301),
			CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.TransferOfGoods302)
			);

		bool HasAB3MessageAndNotAccepted(bool includePending)
		{
			var entryHead = Declaration.B3EntryHeader;
			return entryHead != null
				&& !entryHead.IsClearedB3CorCAD
				&& (includePending || entryHead.CH_Status != MessageStatusList.Codes.AwaitingOriginal);
		}

		protected ZString GetDelayIntervalType(ZString importerDelayIntervalType, ZString registryDelayIntervalType)
		{
			return importerDelayIntervalType.IsEmpty || importerDelayIntervalType == DelayIntervalTypeCodes.Codes.Default
				? registryDelayIntervalType : importerDelayIntervalType;
		}

		protected ZInt GetDelayInterval(ZString delayIntervalType, ZString importerDelayIntervalType, ZInt importerDelayInterval, ZInt registryDelayInterval)
		{
			return delayIntervalType == DelayIntervalTypeCodes.Codes.None ? new ZInt(0) :
				(importerDelayIntervalType.IsEmpty || importerDelayIntervalType == DelayIntervalTypeCodes.Codes.Default)
					? registryDelayInterval : importerDelayInterval;
		}

		protected ZDateTime GetStatementDate()
		{
			var date = Declaration.JE_EntryAuthorisationDate;
			return date.IsValid ? new ZDateTime(date.Year, date.Month, 25).GetNearestWorkingdayAfter(Declaration.CAHolidaysApplicableToThisDeclaration) : ZDateTime.Empty;
		}

		protected ZDateTime GetSendingCalendarDay(ZInt threshold)
		{
			var date = Declaration.JE_EntryAuthorisationDate.AddMonths(1);
			return date.IsValid ? new ZDateTime(date.Year, date.Month, threshold).GetNearestWorkingdayBefore(Declaration.CAHolidaysApplicableToThisDeclaration) : ZDateTime.Empty;
		}

		protected ZDateTime GetLateSendingWarningScheduledDate(ZDateTime date)
		{
			var schedualedDate = date.AddWorkingDaysFromNearestWorkingDay(B3LateSendingFailsafeWarningThreshold, Declaration.CAHolidaysApplicableToThisDeclaration).Date.AddHours(11);
			return EnvProxy.Instance.Time.GetUtcFromUnlocoTime("CAOTT", schedualedDate.ToDateTime());
		}

		protected ZDateTime GetB3AutoSendingScheduledDate(ZDateTime date)
		{
			return date.IsValid ? date.AddWorkingDaysFromNearestWorkingDay(B3SendingDelayThreshold, Declaration.CAHolidaysApplicableToThisDeclaration) : ZDateTime.Empty;
		}

		protected DelayFactorRegistryBusinessObject FallBackSendingDelayThresholds()
		{
			return Declaration.JE_GB.IsValid ?
				CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.GetFallBackValueAtAllLevels(Declaration.CompanyPK.ToGuid(), Declaration.JE_GB.ToGuid(), Guid.Empty)
				: CACustomsDataRegistry.Instance.EntryAutomaticSendingDelayThresholds.Value;
		}

		protected DelayFactorRegistryBusinessObject FallBackFailsafeWarningThresholds()
		{
			return Declaration.JE_GB.IsValid ?
				CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.GetFallBackValueAtAllLevels(Declaration.CompanyPK.ToGuid(), Declaration.JE_GB.ToGuid(), Guid.Empty)
				: CACustomsDataRegistry.Instance.EntryLateSendingFailsafeWarningThresholds.Value;
		}

		#endregion

		#region ForTesting
#if DEBUG
		[ThreadStatic]
		public static List<JobDeclarationB3SendingStrategy> StrategiesForTesting;
#endif
		#endregion
	}
}
