using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	public static class ArchiveTableHelper
	{
		public static void MarkPKAsIgnored(Guid mainArchiveItemPK)
		{
			try
			{
				var findPKSQL = "SELECT TOP 1 AIM_PK FROM dbo.ArchiveMainItemQueue WHERE AIM_ParentID = @MainArchiveItemPK";
				Guid? pkInMainQueue;

				using (var command = Db.Connection.Command(findPKSQL))
				{
					_ = command.AddParameter("@MainArchiveItemPK", SqlDbType.UniqueIdentifier, mainArchiveItemPK);
					pkInMainQueue = command.ExecuteScalar() as Guid?;
				}

				if (pkInMainQueue.HasValue)
				{
					var updateSQL = @$"
UPDATE dbo.ArchiveMainItemQueue SET AIM_IsLoading = 0, AIM_IsLoaded = 1, AIM_IsSkipped = 1, AIM_SystemLastEditTimeUtc = GETUTCDATE(), AIM_SystemLastEditUser = '~BP'
WHERE AIM_ParentID = @MainArchiveItemPK;
DELETE FROM dbo.ArchiveRelatedItemQueue WHERE ARQ_AIM_MainItem = @PKInMainQueue;";

					using (var command = Db.Connection.Command(updateSQL))
					{
						_ = command.AddParameter("@MainArchiveItemPK", SqlDbType.UniqueIdentifier, mainArchiveItemPK);
						_ = command.AddParameter("@PKInMainQueue", SqlDbType.UniqueIdentifier, pkInMainQueue.Value);
						_ = command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportDeveloperExceptionOnce("ArchiveManagerMarkPKASIgnoredException", "Failed to mark archive an archive item as ignored", ex);
			}
		}

		#region LoadedArchiveSets

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Updating queue table for which no bizo exists")]
		public static void MarkArchiveSetAsFullyLoaded(IArchiveSet archiveSet)
		{
			var sql = $"UPDATE dbo.ArchiveMainItemQueue SET AIM_IsLoading = 0, AIM_IsLoaded = 1, AIM_SystemLastEditTimeUtc = GETUTCDATE(), AIM_SystemLastEditUser = '~BP' WHERE AIM_ParentID = @PK";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, archiveSet.MainArchiveItem.PK);
				_ = command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Updating queue table for which no bizo exists")]
		public static bool IsAnyArchiveItemAlreadyLoaded(IArchiveSet archiveSet)
		{
			var archiveSetMainItemFK = archiveSet.MainItemFK;
			var sql = $"SELECT COUNT(*) FROM dbo.ArchiveRelatedItemQueue " +
				$"WHERE ((ARQ_LoadedID IN (SELECT ARQ_LoadedID FROM dbo.ArchiveRelatedItemQueue WHERE ARQ_AIM_MainItem = @archiveSetMainQueueFK))" +
				$" OR ARQ_LoadedID = @mainArchiveItemPK) AND ARQ_AIM_MainItem != @archiveSetMainQueueFK";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@archiveSetMainQueueFK", SqlDbType.UniqueIdentifier, archiveSetMainItemFK);
				cmd.AddParameter("@mainArchiveItemPK", SqlDbType.UniqueIdentifier, archiveSet.MainArchiveItem.PK);

				return (cmd.ExecuteScalar() as int?).Value > 0;
			}
		}

		#endregion

		#region CreateTempTables

		public static void CreateTempTables(ArchiveStage archiveStage)
		{
			CreateTempTables(archiveStage.ArchiveableRelationships.Values.SelectMany(t => t).ToList(), ArchiveManager.SystemSpecificSuffix, archiveStage.StageNameForQueries);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Temp table query - no bizos involved")]
		public static void CreateTempTables(List<ArchiveableRelationship> archiveRelationshipList, string suffixForArchiveTable, string stageName)
		{
			var sanitizedStageName = stageName.Replace("_", "").Replace(" ", "");
			var obtainLockResult = Db.Connection.TryGetLock($"ArchiveManagerCreateRelationship{sanitizedStageName}", TimeSpan.FromMilliseconds(0), out var sqlLock);

			if (obtainLockResult)
			{
				using (sqlLock)
				{
					var relationshipTableName = $"ArchiveRelationship{suffixForArchiveTable}{sanitizedStageName}";
					var relationshipTableExistsSQL = $"select count(*) from sys.tables where name = '{relationshipTableName}'";
					var relationshipTableExists = (Db.Connection.ExecuteScalar(relationshipTableExistsSQL) as int?) == 1;

					if (relationshipTableExists && ArchiveManagerDataRegistry.Instance.IsArchiveRelationshipsTableUpToDateForStage(suffixForArchiveTable, sanitizedStageName))
					{
						return;
					}

					var builder = new StringBuilder(2000);
					_ = builder.AppendLine(string.Format(@$"
			IF object_id('dbo.[{relationshipTableName}]') is not null
			BEGIN
				TRUNCATE TABLE dbo.[{relationshipTableName}]
			END
			ELSE
			BEGIN
				CREATE TABLE dbo.[{relationshipTableName}]
				(
					ParentTypeName nvarchar(125) NOT NULL,
					ParentTableName nvarchar(125) NOT NULL,
					ParentTableCode nvarchar(3) NOT NULL,
					ParentTablePKColumn nvarchar(125) NOT NULL,
					ParentTableMainPKColumn nvarchar(125) NOT NULL,
					ChildTypeName nvarchar(125) NOT NULL,
					ChildTableName nvarchar(125) NOT NULL,
					ChildTablecode nvarchar(3) NOT NULL,
					ChildTableFKColumn nvarchar(125) NOT NULL,
					ChildTableMainPKColumn nvarchar(125) NOT NULL,
					IsReversed bit NOT NULL
				)
			END
			"));

					foreach (var relationship in archiveRelationshipList)
					{
						_ = builder.Append(string.Format(@$"EXEC sp_executesql N'INSERT INTO
dbo.[ArchiveRelationship{suffixForArchiveTable}{sanitizedStageName}]
VALUES (@ParentTypeName, @ParentTableName, @ParentTableCode, @ParentTablePKColumn, @ParentTableMainPKColumn,
@ChildTypeName, @ChildTableName, @ChildTablecode, @ChildTableFKColumn, @ChildTableMainPKColumn, @IsReversed)',
									N'@ParentTypeName nvarchar(125),
									@ParentTableName nvarchar(125),
									@ParentTablePKColumn nvarchar(125),
									@ParentTableCode nvarchar(3),
									@ParentTableMainPKColumn nvarchar(125),
									@ChildTypeName nvarchar(125),
									@ChildTableName nvarchar(125),
									@ChildTablecode nvarchar(3),
									@ChildTableFKColumn nvarchar(125),
									@ChildTableMainPKColumn nvarchar(125),
									@IsReversed bit', "));

						_ = builder.Append((NoResString)"@ParentTypeName = N'" + relationship.ParentName + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ParentTableName = N'" + relationship.ParentPKColumn.TableName + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ParentTableCode = N'" + relationship.ParentPKColumn.ColumnPrefix + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ParentTablePKColumn = N'" + relationship.ParentKeyColumnReferencedByChild.Name + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ParentTableMainPKColumn = N'" + relationship.ParentPKColumn.Name + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ChildTypeName = N'" + relationship.ChildName + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ChildTableName = N'" + relationship.ChildFKColumn.TableName + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ChildTableCode = N'" + relationship.ChildFKColumn.ColumnPrefix + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ChildTableFKColumn = N'" + relationship.ChildFKColumn.Name + (NoResString)"', ");
						_ = builder.Append((NoResString)"@ChildTableMainPKColumn = N'" + relationship.ChildPKColumn.Name + (NoResString)"', ");
						_ = builder.Append((NoResString)"@IsReversed = " + (relationship.IsReversed ? (NoResString)"1" : (NoResString)"0"));
						_ = builder.AppendLine();
						_ = builder.AppendLine();
					}

					using (var command = Db.Connection.Command(builder.ToString()))
					{
						_ = command.ExecuteNonQuery();
					}

					ArchiveManagerDataRegistry.Instance.SetLastVersionRelationshipsTableWasCreatedForStage(suffixForArchiveTable, sanitizedStageName);
				}
			}
		}

		#endregion

		public static int GetNumberOfItemsInMainArchiveQueue()
		{
			var result = Db.Connection.ExecuteScalar("select count(*) from dbo.ArchiveMainItemQueue") as int?;

			return result ?? 0;
		}

		public static int GetNumberOfItemsInRelatedRecordsArchiveQueue()
		{
			var result = Db.Connection.ExecuteScalar("select count(*) from dbo.ArchiveRelatedItemQueue") as int?;

			return result ?? 0;
		}
	}
}
