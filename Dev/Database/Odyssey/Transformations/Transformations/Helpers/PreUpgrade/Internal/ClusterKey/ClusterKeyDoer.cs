using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformations
{
	abstract class ClusterKeyDoer
	{
		public static ClusterKeyDoer New(IUpgradeTaskWorkflowLogger logger, bool isOnline)
		{
			return isOnline ? new OnlineClusterKeyDoer(logger) : new OfflineClusterKeyDoer(logger);
		}

		protected ClusterKeyDoer(IUpgradeTaskWorkflowLogger logger)
		{
			this.logger = logger;
		}

		protected readonly IUpgradeTaskWorkflowLogger logger;

		public void Do(SchemaPKColumn topLevelTablePK, IEnumerable<KeyDefinition> definitions, string numberFountainName = null)
		{
			ValidateKeyDefinitions(topLevelTablePK, definitions);
			DoCore(topLevelTablePK, definitions, numberFountainName);
		}

		void ValidateKeyDefinitions(SchemaPKColumn topLevelTablePK, IEnumerable<KeyDefinition> definitions)
		{
			CheckNoDuplicatedChildren(definitions);
			CheckNoGapsOrCircularReferences(topLevelTablePK, definitions);
		}

		void CheckNoDuplicatedChildren(IEnumerable<KeyDefinition> definitions)
		{
			var duplicatedChildTables = definitions.GroupBy(kd => kd.ChildColumn.TableName, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1);

			if (duplicatedChildTables.Any())
			{
				var concatenatedDuplicatedChildren = string.Join("", duplicatedChildTables.Select(kdg => $"\r\n\t[{kdg.Key}]"));
				throw new ArgumentException("The following cluster key child tables are duplicated:" + concatenatedDuplicatedChildren);
			}
		}

		void CheckNoGapsOrCircularReferences(SchemaPKColumn topLevelTablePK, IEnumerable<KeyDefinition> definitions)
		{
			var remainingDefinitions = new List<KeyDefinition>(definitions);
			CheckNoCircularReferences(topLevelTablePK.TableSchema, remainingDefinitions);

			if (remainingDefinitions.Any())
			{
				var disconnectedDefinitions = string.Join("", remainingDefinitions.Select(rkd => $"\r\n\t[{rkd.PKColumn.TableName}/{rkd.ChildColumn.TableName}]"));
				throw new ArgumentException("The following cluster key definitions do not have a path connecting to top level table:" + disconnectedDefinitions);
			}
		}

		/// <summary>
		/// Recurvely checks for circular references in the cluster key definitions.
		/// If at the end of the recursion, some definitions haven't been checked, it means there are gaps in the key definition tree.
		/// </summary>
		void CheckNoCircularReferences(ITableSchema subTreeRoot, List<KeyDefinition> remainingDefinitions)
		{
			var keyDefinitionAsChild = remainingDefinitions.Find(kd => kd.ChildColumn.TableSchema == subTreeRoot);

			if (keyDefinitionAsChild != null)
			{
				throw new ArgumentException($"Cluster Key Definition [{keyDefinitionAsChild.PKColumn.TableName}/{keyDefinitionAsChild.ChildColumn.TableName}] causes a circular reference.");
			}

			var keyDefinitionsAsParent = remainingDefinitions.Where(kd => kd.PKColumn.TableSchema == subTreeRoot).ToList();

			foreach (var parentKey in keyDefinitionsAsParent)
			{
				remainingDefinitions.Remove(parentKey);
				CheckNoCircularReferences(parentKey.ChildColumn.TableSchema, remainingDefinitions);
			}
		}

		void DoCore(SchemaPKColumn topLevelTablePK, IEnumerable<KeyDefinition> definitions, string numberFountainName)
		{
			if (DbObjectCreator.TableExists(Db.Connection, topLevelTablePK.TableName))
			{
				CreateClusterKeyColumn(topLevelTablePK);

				var midLevelClusterKeyMasterTables = definitions.Where(kd => kd.IsMidLevelMaster).Select(kd => kd.ChildColumn.TableSchema);
				var maxClusterKey = GetMaxKeyFromAllTablesSharingNumberSequence(topLevelTablePK, midLevelClusterKeyMasterTables);
				maxClusterKey = PopulateTopLevelMasterClusterKey(topLevelTablePK, maxClusterKey);

				// Used to simulate concurrent user changes while running online transformations (DEBUG conditional).
				InsertTestDataForOnlineTransformation();

				foreach (var definition in definitions)
				{
					if (DbObjectCreator.ColumnExists(Db.Connection, definition.ChildTableName, definition.ChildFkColumnName))
					{
						CreateClusterKeyColumn(definition.ChildColumn);

						var ckWorkerPopulateStrategy = GetClusterKeyWorkerPopulateStrategy(definition);
						ckWorkerPopulateStrategy.PopulateClusterKeys();

						if (definition.IsMidLevelMaster)
						{
							// Populate MidLevelMasterClusterKeys later than Worker.PopulateClusterKeys()
							// so worker records can get cluster key from parent 
							maxClusterKey = PopulateMidLevelMasterClusterKey(definition, maxClusterKey);
						}
					}
				}

				PerformFinalOfflineTransformationSteps(numberFountainName, topLevelTablePK, maxClusterKey);
			}
		}

		void CreateClusterKeyColumn(SchemaGuidColumn column)
		{
			var tableName = column.TableName;
			var ckColumnName = column.ColumnPrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;

			if (!DbObjectCreator.ColumnExists(Db.Connection, tableName, ckColumnName))
			{
				logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Creating cluster key column {0}", ckColumnName));
				Db.Connection.ExecuteNonQuery($"ALTER TABLE {tableName} ADD {ckColumnName} INT NOT NULL DEFAULT 0");
			}
		}

		int GetMaxKeyFromAllTablesSharingNumberSequence(SchemaPKColumn topLevelTablePK, IEnumerable<ITableSchema> midLevelClusterKeyMasterTables)
		{
			var topLevelClusterKey = topLevelTablePK.ColumnPrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;
			var result = ClusterKeyDbHelper.GetMaxClusterKeyValueSafe(Db.Connection, topLevelTablePK.TableName, topLevelClusterKey);

			foreach (var table in midLevelClusterKeyMasterTables)
			{
				var clusterKeyColumn = table.PK.ColumnPrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;
				var maxValue = ClusterKeyDbHelper.GetMaxClusterKeyValueSafe(Db.Connection, table.TableName, clusterKeyColumn);
				result = Math.Max(result, maxValue);
			}

			return result;
		}

		int PopulateTopLevelMasterClusterKey(SchemaPKColumn topLevelTablePK, int maxClusterKey)
		{
			var topLevelMasterPopulateStrategy = new TopLevelClusterKeyMasterPopulateStrategy(logger, UpgMode, topLevelTablePK);
			return topLevelMasterPopulateStrategy.PopulateClusterKeys(maxClusterKey);
		}

		int PopulateMidLevelMasterClusterKey(KeyDefinition definition, int maxClusterKey)
		{
			var midLevelMasterPopulateStrategy = new MidLevelClusterKeyMasterPopulateStrategy(logger, UpgMode, definition);
			return midLevelMasterPopulateStrategy.PopulateClusterKeys(maxClusterKey);
		}

		protected IClusterKeyWorkerPopulateStrategy GetClusterKeyWorkerPopulateStrategy(KeyDefinition definition)
		{
			return UpgMode == UpgradeMode.Offline
				? new SimpleClusterKeyPopulateStrategy(logger, definition)
				: new OnlineClusterKeyWorkerPopulateStrategy(logger, definition, UpgMode);
		}

		protected abstract UpgradeMode UpgMode { get; }
		protected virtual void PerformFinalOfflineTransformationSteps(string numberFountainName, SchemaPKColumn topLevelTablePK, int maxClusterKey) { }

		[Conditional("DEBUG")]
		protected virtual void InsertTestDataForOnlineTransformation() { }

		internal class OnlineClusterKeyDoer : ClusterKeyDoer
		{
			public OnlineClusterKeyDoer(IUpgradeTaskWorkflowLogger logger) : base(logger) { }

			protected override UpgradeMode UpgMode => UpgradeMode.Online;
		}

		class OfflineClusterKeyDoer : ClusterKeyDoer
		{
			public OfflineClusterKeyDoer(IUpgradeTaskWorkflowLogger logger) : base(logger) { }

			protected override UpgradeMode UpgMode => UpgradeMode.Offline;

			protected override void PerformFinalOfflineTransformationSteps(string numberFountainName, SchemaPKColumn topLevelTablePK, int maxClusterKey)
			{
				Argument.NotNullOrEmpty(numberFountainName, nameof(numberFountainName));
				UpdateNumberFountain(numberFountainName, topLevelTablePK, maxClusterKey + 1);
			}

			void UpdateNumberFountain(string numberFountainName, SchemaPKColumn topLevelTablePK, int nextClusterKeyValue)
			{
				if (DbObjectCreator.TableExists(Db.Connection, "StmNums"))
				{
					logger.ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Adding number fountain {0}", numberFountainName));

					var updateNumberFountainSql = $"DECLARE @nextVal INT = {nextClusterKeyValue};";

					if (Db.Connection.Exists($"FROM dbo.StmNums WHERE SN_Name = '{numberFountainName}'"))
					{
						updateNumberFountainSql += $@"
							UPDATE dbo.StmNums SET SN_Value = @nextVal WHERE SN_Name = '{numberFountainName}' AND SN_VALUE < @nextVal;
							UPDATE dbo.StmNumberCache SET SG_IsUsed = 1 WHERE SG_SN IN (SELECT SN_ID from dbo.StmNums WHERE SN_Name = '{numberFountainName}') AND SG_Value < @nextVal;";
					}
					else
					{
						updateNumberFountainSql += $@"
							INSERT INTO dbo.StmNums (SN_Name, SN_Value, SN_MinimumValue) VALUES ('{numberFountainName}', @nextVal, 1);";
					}

					Db.Connection.ExecuteNonQuery(updateNumberFountainSql);
				}
			}
		}
	}
}
