using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI
{
	class DatSubmissionsProvider : IDatSubmissionsProvider
	{
		public DatSubmissionsProvider()
		{
			autoTestConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection();
			if (CanProvideSubmissions)
			{
				crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(autoTestConnection);
				factoryProvider = new (new BusinessObjectFactory { NameForDebugging = nameof(DatSubmissionsProvider) });
			}
		}

		public DatSubmissionBatch GetNextBatch()
		{
			factoryProvider.CreateNewAndReclaimMemoryWithoutSave();
			var submissions = GetAllPendingSubmissions(crikeyDataAccess, factoryProvider.Current)
				.Where(s => s.UserHeaderPK == default || !submissionsThatCouldNotBeProcessed.Contains(s.UserHeaderPK))
				.Take(BatchSize + 1)
				.ToArray();

			var isLastBatch = submissions.Length <= BatchSize;

			if (!isLastBatch)
			{
				submissions = submissions.Take(BatchSize).ToArray();
			}

			return new DatSubmissionBatch(submissions, factoryProvider.Current, isLastBatch);
		}

		public void UpdateStatusToNotified(EDIShelvesetInfo submission)
		{
			if (submission == null)
			{
				throw new ArgumentNullException(nameof(submission));
			}
			var newStatus = submission.GetNotifiedEquivalentOfCurrentStatus();
			if (newStatus is not null)
			{
				crikeyDataAccess.UpdateStatus(submission, newStatus);
			}
		}

		public bool CanProvideSubmissions => autoTestConnection != null;

		public void NotifySubmissionCouldNotBeProcessed(EDIShelvesetInfo submission)
		{
			if (submission.UserHeaderPK != default)
			{
				submissionsThatCouldNotBeProcessed.Add(submission.UserHeaderPK);
			}
		}

		public void Dispose()
		{
			autoTestConnection?.Dispose();
		}

		static IEnumerable<EDIShelvesetInfo> GetAllPendingSubmissions(ICrikeyDataAccess crikeyDataAccess, BusinessObjectFactory factory)
		{
			return GetSubmissionsByStatus(crikeyDataAccess, factory, ShelfStatuses.CheckedIn)
				.Concat(GetSubmissionsByStatus(crikeyDataAccess, factory, ShelfStatuses.Rejected))
				.Concat(GetSubmissionsByStatus(crikeyDataAccess, factory, ShelfStatuses.RejectedForPendingAspectData))
				.Concat(GetSubmissionsByStatus(crikeyDataAccess, factory, ShelfStatuses.Passed))
				.Concat(GetSubmissionsByStatus(crikeyDataAccess, factory, ShelfStatuses.DeploymentJobFailed));
		}

		static IEnumerable<EDIShelvesetInfo> GetSubmissionsByStatus(ICrikeyDataAccess crikeyDataAccess, BusinessObjectFactory factory, string submissionStatus)
		{
			return crikeyDataAccess.GetScheduledShelvesByStatus(submissionStatus, factory);
		}

		const int BatchSize = 50;

		readonly DbConnection autoTestConnection;
		readonly ICrikeyDataAccess crikeyDataAccess;
		readonly BusinessObjectFactoryProvider factoryProvider;
		readonly HashSet<Guid> submissionsThatCouldNotBeProcessed = [];
	}
}
