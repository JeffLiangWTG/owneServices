using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Engine;
using Enterprise.AuditDataServices.ArchiveManager.Helpers;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.ArchiveManager.Subscribers
{
	[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "We are querying metadata from sys tables")]
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "It is SQL and log messages")]
	public class DeleteOrphanSubscriber : TableValuePairSubscriber
	{
		public override string Code => "DOS";

		public override string Description => "Delete Orphan Subscriber";

		List<Target> targetList;

		readonly object addTargetsMutex = new object();

		readonly object initialiseStructuresMutex = new object();

		public DatabaseGraphTraverser DatabaseTraverser { get; set; } = new();

		public List<Target> TargetList
		{
			get
			{
				lock (addTargetsMutex)
				{
					if (targetList == null)
					{
						targetList = AddTargets(out _);
					}

					return targetList;
				}
			}
			set
			{
				targetList = value;
			}
		}

		Dictionary<string, List<Target>> validOrphanTablesByTablePrefix;

		public Dictionary<string, List<Target>> ValidOrphanTablesByTablePrefix
		{
			get
			{
				lock (initialiseStructuresMutex)
				{
					if (validOrphanTablesByTablePrefix == null)
					{
						InitialiseStructuresUsingConstraints();
					}

					return validOrphanTablesByTablePrefix;
				}
			}
		}

		public override bool IsRequired()
			=> SystemDataRegistry.Instance.BiIsRequiredDeleteOrphanSubscriber.Value;

		public override void ProcessChanges(ILogger logger, Dictionary<IChangedTableSchema, List<string>> changedTableColumnValues)
		{
			var anyOrphansDeleted = false;

			foreach (var kvp in changedTableColumnValues)
			{
				var changedTable = kvp.Key;

				var allChangedValues = kvp.Value;

				while (allChangedValues.Any())
				{
					const int batchSize = 1000;
					var currentBatch = allChangedValues.Take(batchSize).ToList();
					allChangedValues.RemoveRange(0, allChangedValues.Count >= batchSize ? batchSize : allChangedValues.Count);
					var tablePrefix = Schema.GetPrefixFromColumnName(changedTable.PkName);
					var restrictedValidOrphans = ValidOrphanTablesByTablePrefix.ContainsKey(tablePrefix) ? ValidOrphanTablesByTablePrefix[tablePrefix] :
						Enumerable.Empty<Target>();

					foreach (Target target in restrictedValidOrphans.Union(UnrestrictedTargets))
					{
						var tableNameOrPrefix = (target.ParentTableColumnStoresCode ? Schema.GetPrefixFromColumnName(changedTable.PkName) : changedTable.TableName).ToUpperInvariant();
						var rowExists = false;
						var captureRowExistsQuery = GetSelectSqlForQueryOnTarget(target);

						using (var cmd = Db.Connection.Command(captureRowExistsQuery))
						{
							cmd.AddParameter("@TableNameOrPrefix", SqlDbType.VarChar, 128, tableNameOrPrefix);
							cmd.AddTableValuedParameter("@tvp", "dbo.TVP_uniqueidentifier", currentBatch.Distinct().Select(s => new Guid(s)));
							rowExists = cmd.ExecuteScalar() != null;
						}

						if (!rowExists)
						{
							continue;
						}

						try
						{
							var listOfDeleteOrphanpks = new List<string>();
							var deleteSql = GetDeleteSqlForQueryOnTarget(target);
							string message;

							var orphansThatHaveOtherRelationships = OrphansThatHaveChildrenOfTheirOwn.Concat(NonconventionalOrphans);

							if (orphansThatHaveOtherRelationships.Contains(target.TableName))
							{
								var changedValuesString = string.Join("','", currentBatch);
								changedValuesString = $"'{changedValuesString}'";
								var query = GetSqlForQueryOnTarget(@"SELECT * FROM", changedValuesString, target);

								using (var cmd = Db.Connection.Command(query))
								{
									cmd.AddParameter("@TableNameOrPrefix", SqlDbType.VarChar, 128, tableNameOrPrefix);
									using (var reader = cmd.ExecuteReader())
									{
										while (reader.Read())
										{
											listOfDeleteOrphanpks.Add("'" + Convert.ToString(reader[target.PKName], CultureInfo.InvariantCulture) + "'");
										}
									}
								}

								var sqlForInsertIntoArchiveMainItemQueue = $@"
INSERT INTO dbo.ArchiveMainItemQueue (AIM_PK, AIM_ParentTableCode, AIM_ParentID, AIM_StageName,
AIM_SystemCreateTimeUtc, AIM_SystemCreateUser, AIM_SystemLastEditTimeUtc, AIM_SystemLastEditUser)
SELECT NEWID(), @TableCodeForThisTarget, tmp.value, @StageName, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'
FROM @DeleteOrphanPKsTVP tmp
";
								var listOfDeleteOrphanPksAsParsedGuids = listOfDeleteOrphanpks.Select(x => Guid.Parse(x.Replace("'", "")));
								var schemaForCurrentOrphan = GetNewSchemaResolver().GetTableSchema(target.TableName);

								using (var cmd = Db.Connection.Command(sqlForInsertIntoArchiveMainItemQueue))
								{
									cmd.AddParameter("@TableCodeForThisTarget", SqlDbType.NVarChar, 3, schemaForCurrentOrphan.PK.ColumnPrefix);
									cmd.AddParameter("@StageName", SqlDbType.NVarChar, 125, Description);
									cmd.AddTableValuedParameter("@DeleteOrphanPKsTVP", "dbo.TVP_uniqueidentifier", listOfDeleteOrphanPksAsParsedGuids);

									cmd.ExecuteNonQuery();
								}

								var allRelatedOrphanRecords = new Dictionary<string, List<Guid>>();

								foreach (var pk in listOfDeleteOrphanPksAsParsedGuids)
								{
									var archiveSet = new ArchiveSet(null, Description, Guid.Empty, new ArchiveItem(schemaForCurrentOrphan.PK, pk), new ArchiveableType(schemaForCurrentOrphan.PK, null), new ZQuery());
									archiveSet.LoadUnsafe(SuffixForDOPRelationTempTable);

									archiveSet.
										GetArchiveItems().
										Where(item => item.PK != pk).
										ForEach(item =>
										{
											if (!allRelatedOrphanRecords.ContainsKey(item.PKColumn.TableName))
											{
												allRelatedOrphanRecords[item.PKColumn.TableName] = new List<Guid>();
											}

											allRelatedOrphanRecords[item.PKColumn.TableName].Add(item.PK);
										});

									var purgeAction = new PurgeAction(archiveSet, null);
									using (var transactionManager = new MultiTransactionManager<DeleteOrphanSubscriber>(this, new List<ITransactionStarter> { purgeAction }))
									{
										purgeAction.Execute();
										transactionManager.CommitTransaction();
										anyOrphansDeleted = true;
									}
								}

								var finalListOfDeleteOrphanpks = string.Join(",", listOfDeleteOrphanpks);
								message = allRelatedOrphanRecords.Count > 0 ? $"Deleted from: [{target.TableName} - ({finalListOfDeleteOrphanpks})] and related records from " +
									$"{string.Join(", ", allRelatedOrphanRecords.Select(deletedKvp => $"[{deletedKvp.Key} - ({string.Join(",", deletedKvp.Value.Select(deletedGuid => "'" + Convert.ToString(deletedGuid, CultureInfo.InvariantCulture) + "'"))})]"))}"
									: $"Deleted from: [{target.TableName} - ({finalListOfDeleteOrphanpks})]";
							}

							else
							{
								using (var command = Db.Connection.Command(deleteSql))
								{
									command.AddParameter("@TableNameOrPrefix", SqlDbType.VarChar, 128, tableNameOrPrefix);
									command.AddTableValuedParameter("@tvp", "dbo.TVP_uniqueidentifier", currentBatch.Distinct().Select(s => new Guid(s)));
									command.CommandType = CommandType.Text;
									using (var reader = command.ExecuteReader())
									{
										while (reader.Read())
										{
											listOfDeleteOrphanpks.Add("'" + Convert.ToString(reader["id"], CultureInfo.InvariantCulture) + "'");
										}

										anyOrphansDeleted = true;
									}
									var finalListOfDeleteOrphanpks = string.Join(",", listOfDeleteOrphanpks);

									message = $"Deleted from: [{target.TableName} - ({finalListOfDeleteOrphanpks})]";
								}
							}

							logger.Log(LogType.Information, message);
						}
						catch (Exception e)
						{
							logger.Log(LogType.Debug, $"Failed to clean up orphans in table '{target.TableName}'. Error:{System.Environment.NewLine}{e}");
						}
					}
				}
			}

			if (anyOrphansDeleted)
			{
				logger.Log(LogType.Information, "Nudging archive manager cleanup.");
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("ACL");
			}
		}

		string GetSelectSqlForQueryOnTarget(Target target)
		{
			var sql = "SELECT TOP 1 NULL FROM " + target.TableName + " JOIN @tvp AS tmp ON tmp.Value=" + target.ParentIDColumn + " WHERE " + target.ParentTableColumn + "= @TableNameOrPrefix";
			return sql;
		}

		string GetDeleteSqlForQueryOnTarget(Target target)
		{
			var sql = "DECLARE @deletedIds TABLE (id uniqueidentifier);" +
							"DELETE " + target.TableName +
							" OUTPUT DELETED." + target.PKName + " INTO @deletedIds" +
							" FROM @tvp AS tmp JOIN " + target.TableName + " on tmp.Value=" + target.ParentIDColumn +
							" AND " + target.ParentTableColumn + "= @TableNameOrPrefix" + ";SELECT id FROM @deletedIds";
			return sql;
		}

		string GetSqlForQueryOnTarget(string prefix, string changedValues, Target target)
		{
			var stringBuilder = new StringBuilder(prefix + " " + target.TableName + " WHERE " + target.ParentTableColumn + "= @TableNameOrPrefix" +
							" And " + target.ParentIDColumn + " In ( " + changedValues + ")");

			if (string.Equals(target.TableName, CusOutturnSchema.Constants.TableName, StringComparison.OrdinalIgnoreCase))
			{
				_ = stringBuilder.Append(" AND C5_ParentTableCode = 'JS'");
			}

			return stringBuilder.ToString();
		}

		IEnumerable<Target> unrestrictedTargets;

		public IEnumerable<Target> UnrestrictedTargets
		{
			get
			{
				lock (initialiseStructuresMutex)
				{
					if (unrestrictedTargets == null)
					{
						unrestrictedTargets = TargetList.Where(t => !RestrictedTargets.Contains(t));
					}

					return unrestrictedTargets;
				}
			}
		}

		public static string SQLForGettingAllParentTableColumns => @"SELECT t.name AS table_name,
SCHEMA_NAME(t.schema_id) AS schema_name,
c.name AS column_name,
isc.character_maximum_length AS maxLength
FROM sys.tables AS t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN information_schema.columns isc ON (isc.column_name = c.name AND isc.table_name = t.name)
WHERE (c.name LIKE '%_%ParentTableCode' OR c.name LIKE '%_ParentTableCode' OR c.name LIKE '%_%TableCode' OR c.name LIKE '%_TableCode'
OR c.name LIKE '%_ParentTable' OR c.name LIKE '%_Table' OR c.name LIKE '%_%ParentTableName' OR c.name LIKE '%_ParentTableName'
OR c.name = 'TE_EntityTableCodeFrom' OR c.name = 'TE_EntityTableCodeTo')
AND (SCHEMA_NAME(t.schema_id) != 'CDC' AND isc.TABLE_SCHEMA != 'cdc'
AND isc.DATA_TYPE = 'varchar' AND (isc.CHARACTER_MAXIMUM_LENGTH = 3 OR isc.CHARACTER_MAXIMUM_LENGTH = 35))
AND table_name NOT IN ('StmLoginFailureLog', 'AccCurrencyAdjustmentQueue', 'AccTaxConfiguration',
				'AccTransactionComplianceReportQueue', 'AccTransactionPostingToGLDQueue', 'ArchiveMainItemQueue', 'JobChargePostingQueue', 'JobChargeTarget',
				'EDIMessage', 'ProcessQueue', 'ProcessTaskNotification', 'StmALogQueue', 'StmALogQueueWTE', 'StmJobQueue',
				'StmQueueState', 'P4PlanLineItem', 'GenCustomColumnDefinition', 'GenRegCertAccredMaintList', 'GlbHoliday',
				'GlbPasswordHistory', 'GlbPersonPrimaryRelationship', 'GlbStaffHoliday', 'GlbWorkTime', 'JobDocumentDelivery', 'JobHeader',
				'StmAccessToken', 'StmMenuItem', 'StmModuleFilter', 'StmModuleFilterUserData', 'StmNumberRangeMatchingDetail',
				'StmPrintJob', 'StmProcessQueue', 'StmServiceHeartBeat', 'StmUniversalCopy', 'StmScheduleTask', 'AccCommissionHeader',
				'OrgSales', 'RelatedActivityPivot', 'VoteExamSurveyQuestion', 'AsycudaManifestHeader', 'CusAuthorizationUsage',
				'CusEntryCPDec', 'CusExitHeader', 'CusInBondHeader', 'CusPollingTransaction', 'CusSCAOceanBill',
				'CusUnderbond', 'JobComInvoiceLine', 'JPAFRHeader', 'LandedCostHistory', 'StorageDocs', 'BarcodeRuleSet', 'WhsItemDispatchConsignment',
				'WhsItemDispatchLoadList', 'WhsItemReceiveASN', 'WhsItemReceiveConsignment', 'DtbBookingConsolidation', 'JobCartage',
				'GteGateMovementBooking', 'JobMawb', 'JobScheduleChange', 'StmActivityLog', 'BMNCNShape', 'StmServiceMutex', 'RateEntry',
				'RateLines', 'EntityStaffRestriction')
AND column_name NOT IN ('P9_ReferencedTableCode', 'ES_CurrentContextTableCode', 'PJ_SourceTableCode')
AND table_name NOT LIKE 'Ref%'";

		public List<Target> AddTargets(out Dictionary<string, string> invalidMatches)
		{
			var resolver = GetNewSchemaResolver();
			var addedTargets = new List<Target>();
			invalidMatches = [];

			using (var reader = Db.Connection.Command(SQLForGettingAllParentTableColumns).ExecuteReader())
			{
				while (reader.Read())
				{
					try
					{
						var tableName = reader["table_name"] as string;
						var parentTableColumnName = reader["column_name"] as string;
						var schema = resolver.GetTableSchema(tableName);
						if (schema != null)
						{
							var allColumns = schema.All;
							if (TryToMapColumnToExplicitlyNamedParent(out var parentIDColumn, parentTableColumnName, allColumns, invalidMatches))
							{
								var parentTableColumn = schema.GetSchemaColumn(parentTableColumnName);
								var parentTableColumnStoresCode = reader["maxLength"] as int? == 3;

								var target = new Target(
									parentTableColumn.Name,
									parentIDColumn.Name,
									parentTableColumn.TableName,
									parentTableColumn.TableSchema.PK.Name,
									parentTableColumnStoresCode);

								addedTargets.Add(target);
							}
						}
					}
					catch (Exception ex)
					{
						ErrorReporter.ReportOnce("DOPSubscriberAddTargetException", $"Encountered an exception while trying to add a target with DOP " +
							$"with current reader row.", ex);
					}
				}
			}

			return addedTargets;
		}

		bool TryToMapColumnToExplicitlyNamedParent(out SchemaColumn parentIDColumn, string parentTableColumnName, SchemaColumnCollection all, Dictionary<string, string> invalidMatches)
		{
			var nameOfIDColumn = TryToMapColumnToExplicitlyNamedParent(parentTableColumnName, all.Where(c => c is SchemaGuidColumn).Select(c => c.Name), invalidMatches);
			parentIDColumn = all.FirstOrDefault(c => c.Name == nameOfIDColumn);
			return parentIDColumn != null && !invalidMatches.ContainsKey(parentTableColumnName);
		}

		string TryToMapColumnToExplicitlyNamedParent(string parentTableColumnName, IEnumerable<string> allGuidcolumns, Dictionary<string, string> invalidMatches)
		{
			var allValidIDColumnCandidates = allGuidcolumns.Where(c => !IsConstrainedForeignKey(c) && !c.EndsWith("_PK"));
			var allPossibleIDColumnSuffixes = new string[] { "ParentID", "Parent", "ParentGuid", "ID", "Guid", "ForeignKey", "Foreign_Key", "PK",
			"JobId", "UniqueID", "IdFrom", "IdTo" };
			foreach (var suffix in new string[] { "Table", "ParentTable", "ParentTableName", "TableCode", "TableCodeFrom", "TableCodeTo" })
			{
				if (parentTableColumnName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				{
					var match = allValidIDColumnCandidates.FirstOrDefault(c => allPossibleIDColumnSuffixes.Any(idSuffix =>
					c.EndsWith(idSuffix, StringComparison.OrdinalIgnoreCase) &&
					parentTableColumnName.Replace(suffix, idSuffix).Equals(c, StringComparison.OrdinalIgnoreCase)));
					if (match != null)
					{
						return match;
					}
				}
			}

			var resultsFromTryingToMatchWithoutReplacingSuffix =
				allValidIDColumnCandidates.Where(c => allPossibleIDColumnSuffixes.Any(idSuffix => c.EndsWith(idSuffix, StringComparison.OrdinalIgnoreCase)));
			var result = resultsFromTryingToMatchWithoutReplacingSuffix.Any()
				? resultsFromTryingToMatchWithoutReplacingSuffix.MaxBy(c => LengthOfCommonPrefix(c, parentTableColumnName)) : string.Empty;

			invalidMatches[parentTableColumnName] = result;

			return result;
		}

		bool IsConstrainedForeignKey(string columnName)
		{
			var colNameParts = columnName.Split('_');
			return (colNameParts.Length >= 3 && colNameParts[0].Length >= 2 && colNameParts[0].Length <= 3 && colNameParts[1].Length >= 2 && colNameParts[1].Length <= 3);
		}

		int LengthOfCommonPrefix(params string[] strings)
		{
			return new string(strings.MinBy(s => s.Length).TakeWhile((c, i) => strings.All(s => s[i] == c)).ToArray()).Length;
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		string SqlForGettingRestrictedOrphanColumns => $@"SELECT c.name AS columnName, t.name AS tableName, definition, cc.name AS constraintName
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.check_constraints cc ON cc.parent_object_id = c.object_id AND cc.parent_column_id = c.column_id
AND definition LIKE '([[]' + c.Name + ']=''%'
WHERE s.name NOT IN ('sys', 'cdc')
AND c.name IN ({string.Join(",", TargetList.Select(t => "'" + t.ParentTableColumn + "'").ToArray())})
AND definition IS NOT NULL";

		ICollection<Target> restrictedTargets;

		ICollection<Target> RestrictedTargets
		{
			get
			{
				lock (initialiseStructuresMutex)
				{
					if (restrictedTargets == null)
					{
						InitialiseStructuresUsingConstraints();
					}

					return restrictedTargets;
				}
			}
		}

		ICollection<string> orphansThatHaveChildrenOfTheirOwn;

		public ICollection<string> OrphansThatHaveChildrenOfTheirOwn
		{
			get
			{
				lock (initialiseStructuresMutex)
				{
					if (orphansThatHaveChildrenOfTheirOwn == null)
					{
						InitialiseStructuresForOrphansWithOwnChildren();
					}

					return orphansThatHaveChildrenOfTheirOwn;
				}
			}
		}

		ICollection<string> nonconventionalOrphans;

		public ICollection<string> NonconventionalOrphans
		{
			get
			{
				lock (initialiseStructuresMutex)
				{
					if (nonconventionalOrphans == null)
					{
						InitialiseStructuresForNonConventional();
					}

					return nonconventionalOrphans;
				}
			}
		}

		const string SuffixForDOPRelationTempTable = "DOT";

		void CreateArchiveRelationshipsTempTable()
		{
			var archiveRelationships = DatabaseTraverser.TraverseDatabaseToFindAllRelevantRelationships(OrphansThatHaveChildrenOfTheirOwn).ToList();
			archiveRelationships = AddNonConventionalArchiveRelationships(archiveRelationships);
			ArchiveTableHelper.CreateTempTables(archiveRelationships, SuffixForDOPRelationTempTable, Description);
		}

		public List<ArchiveableRelationship> AddNonConventionalArchiveRelationships(List<ArchiveableRelationship> conventionalArchiveRelationships)
		{
			var result = conventionalArchiveRelationships;

			result.Add(new ArchiveableRelationship(CusOutturnSchema.Constants.TableName, CusOutturnSchema.PK, CusOutturnSchema.C5_C6, CusOutturnHeaderSchema.Constants.TableName, CusOutturnHeaderSchema.PK, CusOutturnHeaderSchema.PK, true));
			result.Add(new ArchiveableRelationship(CusOutturnHeaderSchema.Constants.TableName, CusOutturnHeaderSchema.PK, CusOutturnHeaderSchema.PK, CusOutturnSchema.Constants.TableName, CusOutturnSchema.PK, CusOutturnSchema.C5_C6, false));
			result.Add(new ArchiveableRelationship(CusOutturnHeaderSchema.Constants.TableName, CusOutturnHeaderSchema.PK, CusOutturnHeaderSchema.PK, CusUnderbondSchema.Constants.TableName, CusUnderbondSchema.PK, CusUnderbondSchema.C4_C6, false));
			result.Add(new ArchiveableRelationship(CusUnderbondSchema.Constants.TableName, CusUnderbondSchema.PK, CusUnderbondSchema.PK, CusOutturnSchema.Constants.TableName, CusOutturnSchema.PK, CusOutturnSchema.C5_C4_Underbond, false));

			return result;
		}

		void InitialiseStructuresUsingConstraints()
		{
			validOrphanTablesByTablePrefix = new Dictionary<string, List<Target>>();
			restrictedTargets = new List<Target>();

			using (var reader = Db.Connection.Command(SqlForGettingRestrictedOrphanColumns).ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader["tableName"] as string;
					var columnName = reader["columnName"] as string;
					var constraintDefinition = reader["definition"] as string;
					var constraintName = reader["constraintName"] as string;

					var targetCorrespondingToCurrentRow = TargetList.FirstOrDefault(t => t.ParentTableColumn == columnName);

					if (targetCorrespondingToCurrentRow == null)
					{
						continue;
					}

					restrictedTargets.Add(targetCorrespondingToCurrentRow);

					foreach (var match in Regex.Matches(constraintDefinition, "\\[[A-Z0-9]{2,3}_[A-Za-z0-9]{1,}\\]='[A-Za-z0-9]{1,}'"))
					{
						var table = Regex.Match((match as Match).Value, "='[A-Za-z0-9]{1,}'").Value.Replace("=", "").Replace("'", "");

						if (!targetCorrespondingToCurrentRow.ParentTableColumnStoresCode)
						{
							table = GetNewSchemaResolver().GetTableSchema(table).PK.ColumnPrefix;
						}

						if (table == null)
						{
							ErrorReporter.ReportOnce($"Constraint with name {constraintName} and definition {constraintDefinition} contained an invalid table" +
								$" name");
						}

						if (!validOrphanTablesByTablePrefix.ContainsKey(table))
						{
							validOrphanTablesByTablePrefix[table] = new List<Target>();
						}

						validOrphanTablesByTablePrefix[table].Add(targetCorrespondingToCurrentRow);
					}
				}
			}
		}

		void InitialiseStructuresForOrphansWithOwnChildren()
		{
			var sqlForGettingAllOrphanTablesThatHaveChildrenOfTheirOwn = $@"SELECT 
	object_name(fk.referenced_object_id) AS ParentTable, t.name AS ChildTable, c.name AS ForeignKeyColumn
from
	sys.foreign_key_columns AS fk
inner join 
	sys.tables AS t on fk.parent_object_id = t.object_id
inner join 
	sys.columns AS c on fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
where 
	object_name(fk.referenced_object_id) IN ({string.Join(",", TargetList.Select(t => "'" + t.TableName + "'"))})";
			var namesOfTargetsThatHaveTheirOwnOrphans = new HashSet<string>();

			using (var reader = Db.Connection.Command(sqlForGettingAllOrphanTablesThatHaveChildrenOfTheirOwn).ExecuteReader())
			{
				while (reader.Read())
				{
					namesOfTargetsThatHaveTheirOwnOrphans.Add(reader["ParentTable"] as string);
				}
			}

			orphansThatHaveChildrenOfTheirOwn = TargetList.Where(x => namesOfTargetsThatHaveTheirOwnOrphans.Contains(x.TableName)).Select(x => x.TableName).ToHashSet();
			CreateArchiveRelationshipsTempTable();
		}

		void InitialiseStructuresForNonConventional()
		{
			nonconventionalOrphans = new List<string>()
			{
				CusOutturnSchema.Constants.TableName
			};
		}
	}
}
