using System;
using System.Threading;
using CargoWise.ActiveDirectory;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.WTGADUserSynchronization
{
	class WTGADUserSynchronizer
	{
		public WTGADUserSynchronizer(IDirectorySearcher directorySearcher, ILogger logger, CancellationToken token)
		{
			CargoWise.Common.Argument.NotNull(directorySearcher, "directorySearcher");
			CargoWise.Common.Argument.NotNull(logger, "logger");

			this.directorySearcher = directorySearcher;
			this.logger = logger;
			this.token = token;
		}

		readonly IDirectorySearcher directorySearcher;
		readonly ILogger logger;
		readonly CancellationToken token;

		public event EventHandler<SyncProgressEventArgs> ProgressUpdated;

		public void Synchronise()
		{
			ShowMessage("Gathering user(s) to sync");

			var zQuery = GetStaffToSynchroniseQuery();
			var factory = new BusinessObjectFactory() { NameForDebugging = "WTGADUserSynchronizer" };
			var staffs = factory.Load<EDIGlbStaff>(zQuery);
			var totalUnits = staffs.Length;
			var unitsComplete = 0;

			ShowMessage($"Synchronizing {totalUnits} user(s)");

			foreach (var staff in staffs)
			{
				if (token.IsCancellationRequested)
				{
					logger.Warning("Synchronization is cancelled");
					return;
				}

				var user = new WTGADUser(staff, directorySearcher, logger);
				user.Synchronise();
				UpdateProgress(++unitsComplete, totalUnits, string.Empty, "Completed");
			}
		}

		void ShowMessage(string taskname)
		{
			UpdateProgress(0, 0, taskname, string.Empty);
		}

		void UpdateProgress(int unitsComplete, int totalUnits, string taskname, string details)
		{
			if (ProgressUpdated != null)
			{
				var percent = 0;
				if (totalUnits > 0)
				{
					percent = (int)(unitsComplete / (double)totalUnits * 100);
				}
				var info = new SyncProgressEventArgs { OverallPercentComplete = percent, TaskName = taskname, TaskDetails = details, LogType = LogType.Information };
				ProgressUpdated(this, info);
			}
		}

		ZQuery GetStaffToSynchroniseQuery()
		{
			var unlinkedStaffQuery = GetUnlinkedStaffQuery();
			var linkedStaffQuery = GetLinkedStaffQuery();

			var query = new ZQuery();
			query.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, false);
			query.AddToFilter(GlbStaffSchema.GS_IsResource, false);
			query.AddToFilter(new ZQuery(unlinkedStaffQuery, JoinCondition.Or, linkedStaffQuery));

			return query;
		}

		ZQuery GetUnlinkedStaffQuery()
		{
			var unlinkedStaffQuery = new ZDBOnlyQuery(typeof(EDIGlbStaff));

			var adGuidNotPresentQuery = new ZDBOnlySubQuery(typeof(EdiGlbStaffEx), EdiGlbStaffExSchema.GS9_GS);
			adGuidNotPresentQuery.AddToFilter(EdiGlbStaffExSchema.GS9_EdiActiveDirectoryObjectGuid, null);
			unlinkedStaffQuery.AddSubQuery(adGuidNotPresentQuery, JoinCondition.Or);

			var withoutEdiStaffQuery = new ZDBOnlySubQuery(typeof(EdiGlbStaffEx), EdiGlbStaffExSchema.GS9_GS, true);
			unlinkedStaffQuery.AddSubQuery(withoutEdiStaffQuery, JoinCondition.Or);

			var commonQuery = new ZQuery();
			commonQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			commonQuery.AddToFilter(GlbStaffSchema.GS_CanLogin, true);

			unlinkedStaffQuery.AddToFilter(commonQuery, JoinCondition.And);

			return unlinkedStaffQuery;
		}

		ZQuery GetLinkedStaffQuery()
		{
			var linkedStaffQuery = new ZDBOnlyQuery(typeof(EDIGlbStaff));
			linkedStaffQuery.AddToFilter(GlbStaffSchema.GS_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, EDIDataRegistry.Instance.LastSuccessfulSyncForEDIUTC.Value);

			var adGuidPresentQuery = new ZDBOnlySubQuery(typeof(EdiGlbStaffEx), EdiGlbStaffExSchema.GS9_GS);
			adGuidPresentQuery.AddToFilter(EdiGlbStaffExSchema.GS9_EdiActiveDirectoryObjectGuid, SQLComparisonOperator.NotEqual, null);
			linkedStaffQuery.AddSubQuery(adGuidPresentQuery, JoinCondition.And);

			return linkedStaffQuery;
		}
	}
}
