using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService("ZU6", "Matching Activities UPE Service Task", "CSP",
	typeof(MatchingActivitiesServiceTask),
	MinimumPeriod = "1second",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "8seconds"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public class MatchingActivitiesServiceTask : UPEServiceTask
	{
		public MatchingActivitiesServiceTask()
		{
		}

		public MatchingActivitiesServiceTask(ILogger logger)
			: base(logger)
		{
		}

		public override void RunTask(CancellationToken token)
		{
			var branches = UPETools.Instance.UPECustomisationBranches(true);
			var branch = branches.FirstOrDefault(b => b.Country.Code == CountryCodes.Australia);
			if (branch != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					ZDateTime previousHighWaterMark = UPEDataRegistry.Instance.MatchingActivitiesHWM;
					ZDateTime highWaterMark = GetRoundedDownCurrentTime();
					ZQuery timestampFilter = GetTimeStampFilter(previousHighWaterMark, highWaterMark);

					var dateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";
					Notifications.Notify(new InfoNotification($"------Start Matching Activities between {previousHighWaterMark.ToString(dateTimeFormat)} and {highWaterMark.ToString(dateTimeFormat)} for {string.Join(", ", branches.Select(b => b.GB_Code))} branches------"));

					var approvals = GetOrgMatchApprovals(factory, timestampFilter);
					var approvalCount = approvals.Count;
					Notifications.Notify(new InfoNotification($"{approvalCount} potential record(s) found from Organisation Matching"));
					int count = 0;
					var isCancellationRequested = false;
					foreach (var approval in approvals.OrderBy(a => a.P2_RelatedDateForPatternMatch))
					{
						if (token.IsCancellationRequested)
						{
							isCancellationRequested = true;
							UPEDataRegistry.Instance.MatchingActivitiesHWM = approval.P2_RelatedDateForPatternMatch;
							Notifications.Notify(new WarningNotification($"------Cancel Matching Activities & Set HWM to {approval.P2_RelatedDateForPatternMatch.ToString(dateTimeFormat)}------"));
							break;
						}

						Notifications.Notify(new InfoNotification($"Processing {++count}/{approvalCount} with Tracking No - \"{approval.P2_Reference}\" & P2_RelatedDateForPatternMatch - \"{approval.P2_RelatedDateForPatternMatch.ToString(dateTimeFormat)}\""));

						var orgPatternMatchAddress = factory.Load<OrgPatternMatchAddress>(approval.P2_ParentID);
						if (orgPatternMatchAddress != null)
						{
							var uPECusHAWB = factory.Load<UPECusHAWB>(orgPatternMatchAddress.P3_ParentID);
							if (uPECusHAWB != null)
							{
								Notifications.Notify(new InfoNotification($"Running match approved activities on Air Cargo House \"{uPECusHAWB.CS_MessageReference}\"{System.Environment.NewLine}" +
									$"Master Branch - \"{uPECusHAWB.MAWB?.Branch?.GB_Code}\"{System.Environment.NewLine}" +
									$"Declaration Branch - \"{uPECusHAWB.Declaration?.Branch?.GB_Code}\"{System.Environment.NewLine}" +
									$"Consignor Matched - \"{uPECusHAWB.Consignor != null}\" on \"{uPECusHAWB.Consignor?.OH_Code}\"{System.Environment.NewLine}" +
									$"Importer or Consignee Matched - \"{!uPECusHAWB.ImporterOrConsigneeMatchedOrgPK.IsEmpty}\" on \"{factory.Load<UPEOrgHeader>(uPECusHAWB.ImporterOrConsigneeMatchedOrgPK)?.OH_Code}\"{System.Environment.NewLine}" +
									$"MAWB - \"{uPECusHAWB.MAWB?.CM_MAWB}\"{System.Environment.NewLine}" +
									$"HAWB - \"{uPECusHAWB.CS_HAWB}\"{System.Environment.NewLine}" +
									$"Declaration - \"{uPECusHAWB.Declaration?.JobNumber}\""));

								if (branches.Select(b => b.PK).Any(b => b == uPECusHAWB.MAWB?.CM_GB || b == uPECusHAWB.Declaration?.JE_GB))
								{
									RunMatchApprovedActivities(uPECusHAWB);
									UPESaveConcurrencyExceptionResolver.HandleException(() => { factory.Save(); }, Notifications);
									if (director.matchApprovedForSplitShipment)
									{
										Notifications.Notify(new InfoNotification($"Match approved for split shipment"));
									}
									if (director.newlyCreatedJobDeclaration != null)
									{
										Notifications.Notify(new InfoNotification($"Declaration \"{director.newlyCreatedJobDeclaration.JobNumber}\" created"));
									}
									if (director.freightRateIsCalculated)
									{
										Notifications.Notify(new InfoNotification($"Freight rate is calculated for \"{uPECusHAWB.Declaration.JobNumber}\""));
									}
									Notifications.Notify(new InfoNotification("Matching Activities performed for " + uPECusHAWB.CS_HAWB));
								}
							}
							else
							{
								Notifications.Notify(new InfoNotification($"Air Cargo House is not found."));
							}
						}
						Notifications.Notify(new InfoNotification($"Finish processing {count}/{approvalCount}"));
					}

					if (!isCancellationRequested)
					{
						UPEDataRegistry.Instance.MatchingActivitiesHWM = highWaterMark;
						Notifications.Notify(new InfoNotification("------Finish Matching Activities------"));
					}
				}
			}
		}

		UPECusHAWBOrgMatchApprovedDirector director;
		protected virtual void RunMatchApprovedActivities(UPECusHAWB uPECusHAWB)
		{
			director = new UPECusHAWBOrgMatchApprovedDirector(uPECusHAWB);
			director.RunMatchApprovedActivities();
		}

		protected ZQuery GetTimeStampFilter(ZDateTime startDate, ZDateTime endDate)
		{
			ZQuery result = new ZQuery(OrgMatchApprovalSchema.P2_RelatedDateForPatternMatch, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
			result.AddToFilter(OrgMatchApprovalSchema.P2_RelatedDateForPatternMatch, SQLComparisonOperator.LessThan, endDate);
			return result;
		}

		protected OrgMatchApprovalCollection GetOrgMatchApprovals(BusinessObjectFactory factory, ZQuery timestampFilter)
		{
			OrgMatchApprovalCollection result = new OrgMatchApprovalCollection(factory);
			result.Load(timestampFilter);
			return result;
		}

		ZDateTime GetRoundedDownCurrentTime()
		{
			ZDateTime result = ZDateTime.Now;
			return result.AddMilliseconds(-result.Millisecond).AddSeconds(-60);
		}
	}
}
