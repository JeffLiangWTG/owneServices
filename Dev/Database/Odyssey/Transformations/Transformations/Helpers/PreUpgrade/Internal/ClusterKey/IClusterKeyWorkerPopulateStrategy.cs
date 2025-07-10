using System;
using System.Collections.Immutable;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DbUpgrader.Transformations
{
	public interface IClusterKeyWorkerPopulateStrategy
	{
		void PopulateClusterKeys();
	}

	abstract class ClusterKeyWorkerPopulateStrategy : IClusterKeyWorkerPopulateStrategy
	{
		protected ClusterKeyWorkerPopulateStrategy(IUpgradeTaskWorkflowLogger logger, KeyDefinition ckDefinition)
		{
			this.logger = logger;
			this.ckDefinition = ckDefinition;
		}

		protected readonly IUpgradeTaskWorkflowLogger logger;

		protected readonly KeyDefinition ckDefinition;

		public void PopulateClusterKeys()
		{
			CreateSupportingIndexes();
			RunUpdateInBatchesUntilCompletion();
		}

		void CreateSupportingIndexes()
		{
			var parentIndexName = $"IX_{ckDefinition.ParentPkColumnName}_NonZero_{ckDefinition.ParentClusterKey}";

			CreateSupportingIndex(
				ckDefinition.ParentTableName,
				parentIndexName,
				ckDefinition.ParentPkColumnName,
				ckDefinition.ParentClusterKey,
				$"{ckDefinition.ParentClusterKey} <> 0"
			);

			var childIndexName = $"IX_{ckDefinition.ChildFkColumnName}_Zero_{ckDefinition.ChildClusterKey}";

			CreateSupportingIndex(
				ckDefinition.ChildTableName,
				childIndexName,
				ckDefinition.ChildFkColumnName,
				ckDefinition.ChildClusterKey,
				$"{ckDefinition.ChildClusterKey} = 0"
			);
		}

		void CreateSupportingIndex(string tableName, string indexName, string columnName, string includeColumn, string filter)
		{
			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(Db.Connection, logger, tableName, indexName, columnName, includeColumn, filter);
		}

		protected abstract void RunUpdateInBatchesUntilCompletion();

		[ThreadSafe]
		protected static ImmutableHashSet<string> AuditExclusions = ImmutableHashSet.Create(
			StringComparer.OrdinalIgnoreCase,
			"DummyBizO",
			"DummyDependentBizo",
			"DummyPivot");

		protected bool ShouldAddAuditTrail(string tableName) => !AuditExclusions.Contains(tableName);
	}
}
