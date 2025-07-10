using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using WTG.DevTools.SourceControl;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class CrikeyDataAccess : ICrikeyDataAccess
	{
		public CrikeyDataAccess(DbConnection crikeyConnection)
		{
			connection = crikeyConnection ?? throw new ArgumentNullException(nameof(crikeyConnection));
		}

		public void AddToSchedule(EDIShelvesetInfo shelf)
		{
			var processTask = shelf.RelatedProcessTask;
			if (processTask != null)
			{
				var parent = processTask.Parent;
				const string cmdText = "EXEC ScheduleTask @UserName, @ShelfName, null, @ActionType, @ProcessTask, @Comments, @WorkItem = @WorkItem, @WorkItemNumber = @WorkItemNumber";
				using (var cmd = connection.Command(cmdText))
				{
					cmd.AddParameter("@UserName", SqlDbType.VarChar, 35, shelf.Owner);
					cmd.AddParameter("@ShelfName", SqlDbType.VarChar, 1024, shelf.Name);
					cmd.AddParameter("@ActionType", SqlDbType.VarChar, 3, shelf.ActionType);
					cmd.AddParameter("@ProcessTask", SqlDbType.UniqueIdentifier, shelf.RelatedProcessTask.PK.ToGuid());
					cmd.AddParameter("@Comments", SqlDbType.NVarChar, shelf.Comments);
					cmd.AddParameter("@WorkItem", SqlDbType.UniqueIdentifier, parent == null ? DBNull.Value : parent.PK.ToGuid());
					cmd.AddParameter("@WorkItemNumber", SqlDbType.VarChar, 10, parent == null ? DBNull.Value : parent.WKI_WorkItemNumber.ToString());
					cmd.ExecuteNonQuery();
				}
			}
		}

		public bool IsShelfScheduled(EDIShelvesetInfo shelf)
		{
			using (var cmd = connection.Command("SELECT COUNT(UH_PK) FROM UserTestHeader WHERE UH_P9 = @ProcessTask AND UH_Status IN ('QUE', 'QBD')"))
			{
				cmd.AddParameter("@ProcessTask", SqlDbType.UniqueIdentifier, shelf.RelatedProcessTask.PK.ToGuid());
				return ((int)cmd.ExecuteScalar() > 0);
			}
		}

		public void UpdateStatus(EDIShelvesetInfo shelf, string status)
		{
			string cmdText = @"UPDATE UserTestHeader SET UH_Status = @Status WHERE UH_PK = @UserTestPK";
			using (var cmd = connection.Command(cmdText))
			{
				cmd.AddParameter("@Status", SqlDbType.NChar, status);
				cmd.AddParameter("@UserTestPK", SqlDbType.UniqueIdentifier, shelf.UserHeaderPK);
				cmd.ExecuteNonQuery();
				shelf.Status = status;
			}
		}

		public IList<EDIShelvesetInfo> GetScheduledShelvesByStatus(string status, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			var result = new List<EDIShelvesetInfo>();

			using (var cmd = connection.Command(@"
DECLARE @uhTable TABLE (U1_Name varchar(35) not null, UH_PK uniqueidentifier not null, UH_ShelfName varchar(64) null, UH_Type char(3) not null, UH_P9 uniqueidentifier null, UH_Status varchar(3) null, UH_NotificationEmail varchar(64) null, UT_Branch varchar(128) null);
;

INSERT INTO @uhTable
SELECT DISTINCT U1_Name, UH_PK, UH_ShelfName, UH_Type, UH_P9, UH_Status, UH_NotificationEmail, (select top 1 UT_Branch from UserTest where UT_UH = UH_PK and UT_IsBranchInShelfChanges = 1) UT_Branch
FROM UserTestHeader JOIN [User] ON UH_U1 = [User].U1_PK 
WHERE UH_Status = @Status AND (UH_P9 IS NOT NULL OR UH_Type in ('UCC', 'UCB'))
;

-- UH
SELECT U1_Name, UH_PK, UH_ShelfName, UH_Type, UH_P9, UH_Status, UH_NotificationEmail, UT_Branch
FROM @uhTable
;

-- AR
SELECT DISTINCT UH_PK, AR_AS, AR_PK, AR_Capability, AS_NAME
FROM AspectData 
	JOIN AspectReviewData ON ARD_AD = AD_PK
	JOIN AspectReview on ARD_AR = AR_PK
	JOIN UserTest on UT_UH = AR_UH
	LEFT JOIN Aspect ON AR_AS = AS_PK
	JOIN @uhTable ON AR_UH = UH_PK
	WHERE
		(
			(UT_ProcessingError LIKE 'Pending aspect data found%' and AR_Capability is not null)
			or AR_LMSLearningUnitID is not null
		)
	AND AD_Status = 'PEN'
;

-- GitPulls
WITH GitPulls AS (
	SELECT UH_PK,
		UT_TargetRepository,
		UT_PullRequestId,
		UT_Title,
		UT_Status,
		ROW_NUMBER() OVER (
			PARTITION BY UH_PK, UT_TargetRepository, UT_PullRequestId, UT_Title ORDER BY (
				CASE WHEN UT_Status = 'CIN' THEN 0
				WHEN UT_Status = 'REJ' THEN 1
				WHEN UT_Status = 'PAS' THEN 2
				ELSE 3
			END)) AS RowNum
	FROM UserTest
	JOIN @uhTable ON UT_UH = UH_PK
	WHERE (UT_TargetRepository is not null) and (UT_PullRequestId is not null)
)
SELECT * FROM GitPulls Where RowNum = 1

"))
			{
				cmd.AddParameter("@Status", SqlDbType.VarChar, 3, status);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var userName = (string)reader["U1_Name"];
						var userHeaderPK = (Guid)reader["UH_PK"];
						var shelfName = GetValueWithFallback<string>(reader["UH_ShelfName"], null);
						var type = (string)reader["UH_Type"];
						var processTaskPK = GetValueWithFallback(reader["UH_P9"], Guid.Empty);
						var shelfStatus = (string)reader["UH_Status"];
						var primaryBranch = GetValueWithFallback<string>(reader["UT_Branch"], null);
						var notificationEmail = GetValueWithFallback<string>(reader["UH_NotificationEmail"], null);

						var processTask = processTaskPK == Guid.Empty ? null : factory.Load<WorkItemProcessTask>(processTaskPK);
						result.Add(new EDIShelvesetInfo(userName, shelfName, type, processTask)
						{
							UserHeaderPK = userHeaderPK,
							Status = shelfStatus,
							PrimaryBranch = primaryBranch,
							NotificationEmail = notificationEmail,
						});
					}

					// aspect reviews
					if (reader.NextResult())
					{
						while (reader.Read())
						{
							var uhPk = GetValueWithFallback(reader["UH_PK"], Guid.Empty);
							var aspectPK = GetValueWithFallback(reader["AR_AS"], Guid.Empty);
							var aspectReviewPK = GetValueWithFallback(reader["AR_PK"], Guid.Empty);
							var aspectReviewCapability = GetValueWithFallback<string>(reader["AR_Capability"], null);
							var aspectName = GetValueWithFallback<string>(reader["AS_Name"], null);

							var shelf = result.FirstOrDefault(x => x.UserHeaderPK == uhPk);
							if (shelf != null)
							{
								shelf.AspectReviews.Add(new AspectReviewSummary(aspectReviewPK, aspectPK, aspectReviewCapability, aspectName));
							}
						}
					}

					// git pulls
					if (reader.NextResult())
					{
						while (reader.Read())
						{
							var uhPk = GetValueWithFallback(reader["UH_PK"], Guid.Empty);
							var targetRepository = GetValueWithFallback(reader["UT_TargetRepository"], string.Empty);
							var pullRequestId = (int?)reader["UT_PullRequestId"];
							var title = GetValueWithFallback(reader["UT_Title"], string.Empty);
							var utStatus = GetValueWithFallback(reader["UT_Status"], string.Empty);

							var shelf = result.FirstOrDefault(x => x.UserHeaderPK == uhPk);
							if (shelf != null)
							{
								shelf.DatGitPullRequests.Add(new DatGitPullRequest(
								targetRepository,
								pullRequestId,
								title,
								utStatus));
							}
						}
					}
				}
			}

			return result;
		}

		public EDIShelvesetInfo LoadShelfByProcessTask(WorkItemProcessTask processTask)
		{
			var processTaskPK = processTask.PK.ToGuid();

			using (var cmd = connection.Command(@"
	SELECT TOP 1 U1_Name, UH_PK, UH_ShelfName, UH_Type, UH_P9, UH_Status
	FROM UserTestHeader JOIN [User] ON UH_U1 = [User].U1_PK 
	WHERE UH_P9 = @ProcessTaskPK
	ORDER BY UH_DateRecordAdded DESC"))
			{
				cmd.AddParameter("@ProcessTaskPK", SqlDbType.UniqueIdentifier, processTaskPK);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var userName = (string)reader["U1_Name"];
						var userTestPK = (Guid)reader["UH_PK"];
						var shelfName = (string)reader["UH_ShelfName"];
						var type = (string)reader["UH_Type"];
						var status = reader["UH_Status"];
						var shelfStatus = (status == null || status == DBNull.Value) ? "" : (string)status;

						return new EDIShelvesetInfo(userName, shelfName, type, processTask)
						{
							UserHeaderPK = userTestPK,
							Status = shelfStatus,
						};
					}
				}
			}

			return null;
		}

		public void RemoveFromSchedule(EDIShelvesetInfo shelf)
		{
			if (shelf.RelatedProcessTask != null)
			{
				const string cmdText = "EXEC XT_RemoveScheduledTask @ProcessTask";
				using (var cmd = connection.Command(cmdText))
				{
					cmd.AddParameter("@ProcessTask", SqlDbType.UniqueIdentifier, shelf.RelatedProcessTask.PK.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}

		static T GetValueWithFallback<T>(object value, T fallbackValue)
		{
			return (value == null) || (value == DBNull.Value) ? fallbackValue : (T)value;
		}

		readonly DbConnection connection;
	}
}


