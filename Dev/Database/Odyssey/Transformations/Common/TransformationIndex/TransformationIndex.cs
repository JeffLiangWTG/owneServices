using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public interface ITransformationIndexProvider
	{
		TransformationIndexProvider IndexProvider { get; }
	}

	public class TransformationIndexProvider : IEnumerable<IndexInfo>
	{
		public TransformationIndexProvider(DataTransformation transformation)
		{
			if (transformation == null || string.IsNullOrWhiteSpace(transformation.UserDescription))
			{
				throw new ArgumentNullException(nameof(transformation), "TransformationIndexProvider cannot be constructed with a blank description");
			}

			Description = transformation.UserDescription;
		}

		public string Description { get; }

		#region Add

		public TransformationIndexProvider Add(IEnumerable<IndexInfo> newIndexes)
		{
			this.indexes.AddRange(newIndexes);

			return this;
		}

		public TransformationIndexBuilder New(ITableSchema tableSchema)
		{
			return New(tableSchema.SqlSchemaName, tableSchema.TableName);
		}

		public TransformationIndexBuilder New(string schemaName, string tableName)
		{
			if (string.IsNullOrWhiteSpace(schemaName))
			{
				throw new ArgumentException("Parameter value should not be empty", nameof(schemaName));
			}

			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException("Parameter value should not be empty", nameof(tableName));
			}

			return TransformationIndexBuilder.New(this, schemaName, tableName, GetNewIndexName());
		}

		string GetNewIndexName()
		{
			return string.Format(CultureInfo.InvariantCulture,
				@"{0}_{1}_{2}"
				, IndexInfo.MANUALLY_CREATED_WTG_INDEX_PREFIX
				, new string(Description.Take(100).ToArray())
				, this.indexes.Count + 1
				);
		}

		#endregion // Add

		#region Builder

		public class TransformationIndexBuilder : IndexInfo.Builder
		{
			TransformationIndexBuilder(TransformationIndexProvider provider, string schemaName, string tableName, string indexName)
				: base(schemaName, tableName, indexName)
			{
				this.provider = provider;
			}

			internal static TransformationIndexBuilder New(TransformationIndexProvider provider, string schemaName, string tableName, string indexName)
			{
				return new TransformationIndexBuilder(provider, schemaName, tableName, indexName);
			}

			readonly TransformationIndexProvider provider;

			public override IndexInfo GetInfo()
			{
				var index = base.GetInfo();
				provider.indexes.Add(index);
				return index;
			}
		}

		#endregion // Builder

		#region MISC

		public void CreateIndexes(DbConnection connection, IUpgradeTaskWorkflowLogger logger = null)
		{
			logger?.ActivateSubtaskProgress(this.indexes.Count);

			foreach (var index in this.indexes)
			{
				logger?.StartTask("    (+) CREATE " + index.Definition);

				index.Create(connection);
			}
		}

		public void DropIndexes(DbConnection connection, IUpgradeTaskWorkflowLogger logger = null)
		{
			logger?.ActivateSubtaskProgress(this.indexes.Count);

			foreach (var index in this.indexes)
			{
				logger?.StartTask("    (-) DROP " + index.Definition);

				index.Drop(connection);
			}
		}

		#endregion // MISC

		#region Interface IEnumerable

		public IEnumerator<IndexInfo> GetEnumerator()
		{
			return this.indexes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion // Interface IEnumerable

		#region Implementation

		readonly List<IndexInfo> indexes = new List<IndexInfo>();

		#endregion // Implementation
	}
}
