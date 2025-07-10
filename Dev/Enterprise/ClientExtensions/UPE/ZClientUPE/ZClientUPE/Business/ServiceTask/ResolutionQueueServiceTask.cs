using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.GSSi;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService("ZU7", "UPS Resolution Queue Processor", "CSP",
	typeof(ResolutionQueueServiceTask),
	MinimumPeriod = "1second",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "8seconds"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public class ResolutionQueueServiceTask : UPEServiceTask
	{
		/* This was originally named the Process Queue Batch Processor. */
		public ResolutionQueueServiceTask()
		{
		}

		public ResolutionQueueServiceTask(ILogger logger)
			: base(logger)
		{
		}

		public override void RunTask(CancellationToken token)
		{
			var branch = UPETools.Instance.UPECustomisationBranches(false).FirstOrDefault(b => b.Country.Code == CountryCodes.Australia);
			if (branch != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					Notify($"====== Start Processing Queue Events (Login Branch: {branch.GB_Code}) ======");
					try
					{
						BusinessObjectFactory factory = new BusinessObjectFactory();
						ZDateTime previousHighWaterMark = UPEDataRegistry.Instance.ProcessQueueProcessorHWM;
						ZDateTime proposedHighWaterMark = GetHighWaterMark(previousHighWaterMark);

						Notify("Current HWM= " + previousHighWaterMark.ToString());

						var finalHighWaterMark = SetResolutionCodeForFinalisedCusHAWBs(previousHighWaterMark, proposedHighWaterMark, factory, token);

						factory.Save();

						UPEDataRegistry.Instance.ProcessQueueProcessorHWM = finalHighWaterMark;

						Notify("New HWM=" + finalHighWaterMark.ToString());
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Notify(ex.Message);
						throw;
					}
				}
			}
		}

		#region Implementation

		ZDateTime SetResolutionCodeForFinalisedCusHAWBs(ZDateTime previousHighWaterMark, ZDateTime proposedhighWaterMark, BusinessObjectFactory factory, CancellationToken token)
		{
			var highWaterMark = proposedhighWaterMark;
			ZQuery filter = ConstructFilter(previousHighWaterMark, proposedhighWaterMark);
			UPEProcessQueueLog[] logs = factory.Load<UPEProcessQueueLog>(filter);

			if (logs.Any())
			{
				Notify($"{logs.Length} Queue Events to be processed.");
			}

			var recCount = 0;
			foreach (UPEProcessQueueLog log in logs.OrderBy(l => l.SL_PostedTimeUtc))
			{
				Notify($"Processing Queue Event with SL_PostedTimeUTC {log.SL_PostedTimeUtc} (Record: {++recCount}/{logs.Length}).");

				if (token.IsCancellationRequested)
				{
					highWaterMark = Env.Time.GetLocalTimeFromUtc(log.SL_PostedTimeUtc.ToDateTime());
					Notifications.Notify(new WarningNotification($"Service Task Run is cancelled. HWM will be set to {highWaterMark}."));
					return highWaterMark;
				}

				var branchPks = UPETools.Instance.UPECustomisationBranches(true).Select(branch => branch.PK).ToArray();
				var finalisedCusHawbs = log.ProcessQueue.GetFinalisedCusHAWBs().Where(cusHawb => cusHawb.MAWB.CM_GB.In(branchPks) ||
					cusHawb.Declaration.JE_GB.In(branchPks));

				if (!finalisedCusHawbs.Any())
				{
					Notify($"Queue Event with PK '{log.PK}' belongs to a non-finalised shipment.");
				}

				foreach (UPECusHAWB finalisedCusHAWB in finalisedCusHawbs)
				{
					if (token.IsCancellationRequested)
					{
						highWaterMark = Env.Time.GetLocalTimeFromUtc(log.SL_PostedTimeUtc.ToDateTime());
						Notifications.Notify(new WarningNotification($"Service Task Run is cancelled. HWM will be set to {highWaterMark}."));
						return highWaterMark;
					}

					ReleaseShipmentAndAssociatedSplitShipments(finalisedCusHAWB, factory);
				}
			}

			return highWaterMark;
		}

		protected virtual void ReleaseShipmentAndAssociatedSplitShipments(UPECusHAWB finalisedCusHAWB, BusinessObjectFactory factory)
		{
			Notify($"Processing Shipment with TrackingId '{finalisedCusHAWB.CS_HAWB}'; Short TrackingId '{finalisedCusHAWB.WayBillShort}'; MAWB '{finalisedCusHAWB.MAWB?.CM_MAWB}'.");

			var latestStatusLog = finalisedCusHAWB.CurrentQueue.GetLogs().MostRecentLogByPostedTime(Events.QueueChanged);
			var latestLogIsNotResolution = finalisedCusHAWB.CurrentQueue.LastLogWithResolutionCode == null || latestStatusLog.PK != finalisedCusHAWB.CurrentQueue.LastLogWithResolutionCode.PK;
			var alreadyonResolutionPkList = AddedResolutionShipmentPKs.Exists(p => p == finalisedCusHAWB.PK);

			if (latestLogIsNotResolution && !alreadyonResolutionPkList)
			{
				var allSplitShipmentsCompleted = AddResolutionCodeLogIfAllSplitShipmentsCompleted(factory, finalisedCusHAWB);
				if (allSplitShipmentsCompleted)
				{
					finalisedCusHAWB.CurrentQueue.AddResolutionCodeLog();
					SendMessage(finalisedCusHAWB);
					AddedResolutionShipmentPKs.Add(finalisedCusHAWB.PK);
					Notify($"Shipment is moved to Resolution Queue.");
				}
				else
				{
					Notify($@"Shipment will not be moved to Resolution Queue. Refer to Shipment Info for details.
Shipment Info:
latestStatusLog={latestStatusLog.PK}|{latestStatusLog.SL_Reference}
lastResolutionLog= {finalisedCusHAWB.CurrentQueue.LastLogWithResolutionCode?.PK}
latestLogIsNotResolution={latestLogIsNotResolution}
alreadyonResolutionPkList={alreadyonResolutionPkList}
allSplitShipmentsCompleted={allSplitShipmentsCompleted}");
				}
			}
			else
			{
				Notify($@"Shipment will not be moved to Resolution Queue. Refer to Shipment Info for details.
Shipment Info:
latestStatusLog={latestStatusLog.PK}|{latestStatusLog.SL_Reference}
lastResolutionLog= {finalisedCusHAWB.CurrentQueue.LastLogWithResolutionCode?.PK}
latestLogIsNotResolution={latestLogIsNotResolution}
alreadyonResolutionPkList={alreadyonResolutionPkList}");
			}
		}

		void SendMessage(UPECusHAWB finalisedCusHAWB)
		{
			UPEProcessQueueLog resolutionLog = (UPEProcessQueueLog)finalisedCusHAWB.CurrentQueue.LastLogWithResolutionCode;

			var uniqueTrackingNumbers = new UniqueList<string>();
			uniqueTrackingNumbers.Add(finalisedCusHAWB.CS_HAWB);
			foreach (UPEJobRelatedWayBill wayBill in finalisedCusHAWB.ChildJobRelatedWayBills)
			{
				uniqueTrackingNumbers.Add(wayBill.EB_WaybillNumber);
			}

			foreach (string trackingNumber in uniqueTrackingNumbers)
			{
				GSSiManager.Instance.CreateResolutionMessage(finalisedCusHAWB.Declaration, finalisedCusHAWB.Factory, trackingNumber, resolutionLog, finalisedCusHAWB.Destination);
				GSSiManager.Instance.CreateResolutionMessage(finalisedCusHAWB, finalisedCusHAWB.Factory, trackingNumber, resolutionLog, finalisedCusHAWB.Destination);
			}
		}

		protected ZQuery ConstructFilter(ZDateTime lowerBound, ZDateTime upperBound)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, Env.Time.GetUtcFromLocalTime(lowerBound.ToDateTime()));
			result.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThan, Env.Time.GetUtcFromLocalTime(upperBound.ToDateTime()));
			result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.QueueChanged.Code);
			result.AddToFilter(EncodedReferenceFilter);
			result.IsNoLock = false;
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "StartsWith critera cannot be written with an IN statement")]
		ZQuery EncodedReferenceFilter
		{
			get
			{
				if (fEncodedReferenceFilter == null)
				{
					fEncodedReferenceFilter = new ZQuery();

					foreach (ZString completedQueueName in CommercialQueueCodeDescriptionPairList.CompletedQueueNames)
					{
						string encodedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, completedQueueName);
						fEncodedReferenceFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, encodedReference);
					}

					foreach (ZString completedQueueName in CustomsQueueCodeDescriptionPairList.CompletedQueueNames)
					{
						string encodedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, completedQueueName);
						fEncodedReferenceFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, encodedReference);
					}
				}
				return fEncodedReferenceFilter;
			}
		}
		ZQuery fEncodedReferenceFilter;

		ZDateTime GetHighWaterMark(ZDateTime previousHighWaterMark)
		{
			ZDateTime result = GetRoundedDownCurrentTime();

			TimeSpan timeDifferences = result - previousHighWaterMark;
			if (timeDifferences.TotalHours > UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer)
			{
				result = previousHighWaterMark.AddHours(UPEDataRegistry.Instance.ProcessQueueProcessorTimeBuffer);
			}

			return result;
		}

		ZDateTime GetRoundedDownCurrentTime()
		{
			ZDateTime result = ZDateTime.Now;
			return result.AddMilliseconds(-result.Millisecond).AddSeconds(-UPEDataRegistry.Instance.ProcessQueueProcessorRunTimeOffSet);
		}

		bool AddResolutionCodeLogIfAllSplitShipmentsCompleted(BusinessObjectFactory factory, UPECusHAWB finalisedCusHAWB)
		{
			if (!finalisedCusHAWB.WayBillShort.IsEmpty)
			{
				UPECusHAWB[] otherSplitShipments = GetSplitShipments(finalisedCusHAWB.WayBillShort, finalisedCusHAWB.PK, factory);
				if (otherSplitShipments.Length > 0)
				{
					if (!AllShipmentsAreReadyForRelease(otherSplitShipments))
					{
						return false;
					}

					Notify($"All related Split Shipments of short TrackingID '{finalisedCusHAWB.WayBillShort}' are ready to Release.");
					foreach (var splitShipment in otherSplitShipments)
					{
						AddResolutionCode(splitShipment);
					}
				}
			}

			return true;
		}

		UPECusHAWB[] GetSplitShipments(ZString shortTrackingNumber, ZGuid cusHawbPK, BusinessObjectFactory factory)
		{
			var query = UPEUtility.GetDuplicateShipmentQuery(shortTrackingNumber);
			query.AddToFilter(CusHAWBSchema.PK, SQLComparisonOperator.NotEqual, cusHawbPK);

			return factory.Load<UPECusHAWB>(query);
		}

		void AddResolutionCode(UPECusHAWB splitShipment)
		{
			if (!AddedResolutionShipmentPKs.Exists(p => p == splitShipment.PK))
			{
				splitShipment.CurrentQueue.AddResolutionCodeLog();
				SendMessage(splitShipment);
				AddedResolutionShipmentPKs.Add(splitShipment.PK);
				Notify($"Move related Split Shipment on MAWB '{splitShipment.MAWB?.CM_MAWB}; ShipmentPK '{splitShipment.PK}' to Resolution Queue.");
			}
		}

		bool AllShipmentsAreReadyForRelease(UPECusHAWB[] cusHAWBs)
		{
			foreach (var cusHAWB in cusHAWBs)
			{
				if (!cusHAWB.CurrentQueue.AllQueuesCompleted)
				{
					return false;
				}
			}
			return true;
		}

		public List<ZGuid> AddedResolutionShipmentPKs
		{
			get
			{
				if (addedResolutionShipmentPKs == null)
				{
					addedResolutionShipmentPKs = new List<ZGuid>();
				}
				return addedResolutionShipmentPKs;
			}
		}
		List<ZGuid> addedResolutionShipmentPKs;

		#endregion
	}
}
