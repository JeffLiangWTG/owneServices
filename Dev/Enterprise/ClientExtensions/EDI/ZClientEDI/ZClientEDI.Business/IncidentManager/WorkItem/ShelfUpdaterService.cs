using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ShelfUpdaterService : IService
	{
		#region GetInstance

		protected ShelfUpdaterService(BusinessObjectFactory factory)
		{
			this.factory = factory;
			factory.Saved += Factory_Saved;
		}

		public static ShelfUpdaterService GetInstance(BusinessObjectFactory factory)
		{
			var result = LastShelfUpdaterService.Target as ShelfUpdaterService;
			if (result == null || result.factory != factory)
			{
				result = factory.ServiceContainer.GetService<ShelfUpdaterService>();
				if (result == null)
				{
					result = new ShelfUpdaterService(factory);
					factory.ServiceContainer.AddService(result);
				}
				LastShelfUpdaterService.Target = result;
			}
			return result;
		}

		static WeakReference LastShelfUpdaterService
		{
			get { return lastShelfUpdaterService ?? (lastShelfUpdaterService = new WeakReference(null)); }
		}

		[ThreadStatic]
		static WeakReference lastShelfUpdaterService;

		#endregion

		#region Factory Saving / Saved

		void Factory_Saved(BusinessObjectFactory saveFactory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				if ((SubmissionsToSchedule.Count > 0) || (SubmissionsToRemove.Count > 0))
				{
					using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
					{
						if (connection != null)
						{
							var crikeyDataAccess = CrikeyDataAccessFactory.GetInstance(connection);
							var shelfScheduler = new SaveInTransactionDelegateAction(connection, delegate
							{
								foreach (var shelfToDelete in SubmissionsToRemove)
								{
									crikeyDataAccess.RemoveFromSchedule(shelfToDelete);
								}

								foreach (var shelfToAdd in SubmissionsToSchedule)
								{
									shelfToAdd.Schedule(crikeyDataAccess);
								}

								return ChangedTableNames.Empty;
							});
							try
							{
								BusinessObjectFactory.SaveTogether(shelfScheduler);
							}
							catch (System.Data.SqlClient.SqlException)
							{
								SubmissionsToSchedule.ForEach(x => x.RelatedProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended);
								foreach (var submission in SubmissionsToRemove)
								{
									if (!submission.RelatedProcessTask.IsDeleted)
									{
										submission.RelatedProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
									}
								}
								throw;
							}
						}
					}
				}
			}

			SubmissionsToSchedule.Clear();
			SubmissionsToRemove.Clear();
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;

		public List<EDIShelvesetInfo> SubmissionsToSchedule => submissionsToSchedule ?? (submissionsToSchedule = new List<EDIShelvesetInfo>());
		List<EDIShelvesetInfo> submissionsToSchedule;

		public List<EDIShelvesetInfo> SubmissionsToRemove => submissionsToRemove ?? (submissionsToRemove = new List<EDIShelvesetInfo>());
		List<EDIShelvesetInfo> submissionsToRemove;

		#endregion
	}
}
