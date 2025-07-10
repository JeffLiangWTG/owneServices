using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Sql;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common.Operations
{
	public class RetrieveOperation
	{
		public RetrieveOperation(AncillaryImportServices sessionServices)
		{
			entitySetBuilder = new EntitySetBuilder();
			this.sessionServices = sessionServices;
		}
		readonly AncillaryImportServices sessionServices;
		readonly EntitySetBuilder entitySetBuilder;

		public IEntity FindByInternalPK(Guid internalPK, IEntityDefinition definition, Func<DataTable, DataRow> filterOnMultiRowResult = null)
		{
			var row = FindRowByInternalPK(internalPK, definition, filterOnMultiRowResult);
			return entitySetBuilder.BuildEntitySet(row, definition, sessionServices);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public IEntity FindByCandidateKey(EntityCriteria entityCriteria, IEntityDefinition definition)
		{
			var criteria = CriteriaConverter.Convert(definition, entityCriteria);
			if (criteria.ColumnDefinition.DataType == DbDataType.UniqueIdentifier)
			{
				if (!ZGuid.TryParse(criteria.Value, out var key))
				{
					var errMsg = string.Format(CultureInfo.InvariantCulture, "The value {0} is not a valid Unique Identifier.", criteria.Value);
					throw new NativeXMLUserVisibleException(errMsg);
				}
			}
			var row = FindRowByCriteria(criteria, definition);
			return entitySetBuilder.BuildEntitySet(row, definition, sessionServices);
		}

		public IEnumerable<IEntity> FindByCriterias(IEnumerable<EntityCriteria> entityCriterias, EntitySetDefinition info)
		{
			var rootDef = info.Root;
			var pkName = rootDef.Table.Columns.PrimaryKey.Name;

			IEnumerable<DataRow> result = new List<DataRow>();
			var lookups = entityCriterias.ToLookup(c => c.EntityName, c => c);
			int groupIndex = 0;
			foreach (var group in lookups)
			{
				IEntityDefinition definition = info.Entities.FindDefinition(group.Key);
				var criterias = CriteriaConverter.Convert(info, group);
				var dataRows = new SelectWithCriteria(rootDef, definition, criterias.ToArray()).GetDataSet();
				if (groupIndex == 0)
				{
					result = dataRows;
				}
				else
				{
					result = result.Join(dataRows, r => r[pkName], j => j[pkName], (r, j) => r);
				}

				result = result.Distinct();
				groupIndex++;
			}

			return result.Select(row => entitySetBuilder.BuildEntitySet(row, rootDef, sessionServices));
		}

		static DataRow FindRowByInternalPK(Guid internalPK, IEntityDefinition definition, Func<DataTable, DataRow> filterOnMultiRowResult = null)
		{
			return FindRowByCriteria(new Criteria
			{
				ColumnDefinition = definition.Id.ColumnDef,
				ColumnName = definition.Id.ColumnDef.Name,
				Value = internalPK.ToString()
			}, definition, filterOnMultiRowResult);
		}

		static DataRow FindRowByCriteria(Criteria criteria, IEntityDefinition definition, Func<DataTable, DataRow> filterOnMultiRowResult = null)
		{
			var command = new FindByKeyStatement(definition.TableName, criteria.ColumnDefinition, criteria.ColumnName, criteria.Value).GetCommand(Connection);
			return command.ExecuteDataRow(filterOnMultiRowResult);
		}

		static DbConnection Connection => DataSetContext.Connection;
	}
}
