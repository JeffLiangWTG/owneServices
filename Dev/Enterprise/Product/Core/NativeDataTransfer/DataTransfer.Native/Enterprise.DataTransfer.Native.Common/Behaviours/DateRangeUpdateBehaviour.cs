using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.Behaviours
{
	class DateRangeUpdateBehaviour : IEntityBehaviour
	{
		public DateRangeUpdateBehaviour(AncillaryImportServices sessionServices)
		{
			this.sessionServices = sessionServices;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "readonly string")]
		protected readonly AncillaryImportServices sessionServices;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		protected const string EntityDefinitionDateRangeAttributesIncorrect = "DateRangeStartField or DateRangeEndField attribute values don't have correspondent fields in xml that is being imported.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		protected const string EntityStartDateCanNotBeEmpty = "Entity start date can not be empty.";

		#region IEntityBehaviour

		public bool CanBeAppliedToAction(EntityAction action) => action == EntityAction.INSERT;

		protected IRowRepository RowRepository;

		public void Apply(BehaviourContext behaviourContext)
		{
			var criteriaColumnsComparer = HashSet<string>.CreateSetComparer();
			RowRepository = new RowRepository(behaviourContext.EntityContext.GetOrCreateBehaviourRowFactorySavedFirst());
			foreach (var entityDefinitionGroup in behaviourContext.Entities.GroupBy(x => x.Definition))
			{
				var definition = entityDefinitionGroup.Key;
				var tableSchema = EnterpriseSchema.GetTableSchema(definition.TableName);
				var startDateSchemaColumn = tableSchema.GetSchemaColumn(definition.DateRangeStartField);
				var endDateSchemaColumn = tableSchema.GetSchemaColumn(definition.DateRangeEndField);
				var deletedPks = behaviourContext.DeletedEntities
					.Where(x => x.Definition == definition)
					.Select(x => x.InternalPK).ToHashSet();

				foreach (var parentGroup in entityDefinitionGroup.GroupBy(x => x.Parent))
				{
					var allEntityCriteriasForParent = BuildEntityCriteriaList(definition, parentGroup, behaviourContext.Converter);

					if (allEntityCriteriasForParent.Any(x => x.StartDate.IsEmpty))
					{
						ThrowIfMissingStartDate();
					}

					// It's possible different entities have different sets of elements.
					// E.g., One RateEntry may have a <ViaLRC> element, and another may be missing that element altogether.
					// Deal with this by processing entities in groups of the same elements.
					// It means an incoming rate can't expire another incoming rate with different elements, but this seems reasonable.
					foreach (var criteriaColumnNamesGroup in allEntityCriteriasForParent.GroupBy(x => x.CriteriaColumnNames, criteriaColumnsComparer))
					{
						var columnNamesOrdered = criteriaColumnNamesGroup.Key.ToList();
						var criteriaToEntity = BuildCriteriaToEntity(criteriaColumnNamesGroup);
						UpdateDateRangeForIncomingEntities(behaviourContext, criteriaToEntity);

						foreach (var dateRangeGroup in criteriaColumnNamesGroup.GroupBy(x => (x.StartDate, x.EndDate)))
						{
							var startDate = dateRangeGroup.Key.StartDate;
							IEnumerable<DateRangeEntityAndCriteriaSet> dateRangeEntities = dateRangeGroup;
							var entityCount = dateRangeEntities.Count();
							var columnValues = BuildColumnValueMap(dateRangeEntities);

							var filterQuery = new ZQuery();
							var endDate = dateRangeGroup.Key.EndDate;
							var dateRangeFilter = GetDateRangeFilter(startDateSchemaColumn, endDateSchemaColumn, startDate, endDate);
							filterQuery.AddToFilter(dateRangeFilter);
							AddOtherValuesFilter(filterQuery, tableSchema, entityCount, columnValues);

							var matchingRowsInDb = RowRepository.Load(definition.TableName, filterQuery);
							if (matchingRowsInDb.Any())
							{
								var rowsToUpdate = ExcludeDeletedRows(tableSchema, matchingRowsInDb, deletedPks);
								if (rowsToUpdate.Any())
								{
									UpdateDateRangeForAllEntities(behaviourContext, dateRangeEntities, rowsToUpdate, columnNamesOrdered, startDate, endDate);
								}
							}
						}
					}
				}
			}
			RowRepository.Save();
			RowRepository = null;
		}

		public static IList<DataRow> ExcludeDeletedRows(ITableSchema schema, IList<DataRow> rowsInDb, HashSet<Guid> deletedPks)
		{
			if (deletedPks.Count == 0)
			{
				return rowsInDb;
			}

			var result = new List<DataRow>(rowsInDb.Count);
			var pkColumnName = schema.PK.Name;
			foreach (var row in rowsInDb)
			{
				if (!deletedPks.Contains((Guid)row[pkColumnName]))
				{
					result.Add(row);
				}
			}
			return result;
		}

		/// <summary>
		/// Incoming rates can expire other incoming rates that occur earlier in the import XML.
		/// </summary>
		void UpdateDateRangeForIncomingEntities(BehaviourContext behaviourContext, Dictionary<IColumnValueSet, List<DateRangeEntityAndCriteriaSet>> map)
		{
			foreach (var matchingList in map.Values)
			{
				if (matchingList.Count > 1)
				{
					// There are multiple entities with the same criteria.
					// Each entity is compared with all previous entities in the import data
					// to check if the dates overlap.
					// If so, the DataRow for the previous entity can be updated
					// to eliminate the overlap.
					for (int i = 1; i < matchingList.Count; ++i)
					{
						var entity2 = matchingList[i];
						var start2 = entity2.StartDate;
						var end2 = entity2.EndDate;
						for (int j = 0; j < i; ++j)
						{
							var entity1 = matchingList[j];
							var start1 = entity1.StartDate;
							var end1 = entity1.EndDate;
							if (HasOverlappingDates(start1, end1, start2, end2))
							{
								ThrowIfDateRangeConflict(behaviourContext, entity2.Entity, start2, end2, start1, end1);
								var row1 = behaviourContext.MainRowRepository.Show(entity1.Entity.InternalPK, entity1.Entity.Definition.Table);
								UpdateStartOrEndDate(behaviourContext, entity2.Entity, start2, end2, row1, start1);
							}
						}
					}
				}
			}
		}

		protected virtual void ThrowIfMissingStartDate()
		{
			throw new NativeXMLUserVisibleException(EntityStartDateCanNotBeEmpty);
		}

		static bool HasOverlappingDates(ZDateTime start1, ZDateTime end1, ZDateTime start2, ZDateTime end2)
		{
			return (start1.IsEmpty || end2.IsEmpty || start1 <= end2) &&
				   (start2.IsEmpty || end1.IsEmpty || start2 <= end1);
		}

		/// <summary>
		/// Build dictionary for fast matching of column values to DataRow
		/// </summary>
		static Dictionary<IColumnValueSet, List<DataRow>> BuildCriteriaToMatchingRows(
			IList<DataRow> matchingRowsInDb,
			List<string> columnNames)
		{
			var columnSet = new DataTableColumnSet(matchingRowsInDb[0].Table, columnNames);
			var result = new Dictionary<IColumnValueSet, List<DataRow>>();
			foreach (var row in matchingRowsInDb)
			{
				var valueSet = new DataRowColumnValueSet(row, columnSet);
				if (!result.TryGetValue(valueSet, out var rowList))
				{
					result.Add(valueSet, new List<DataRow>(1) { row });
				}
				else
				{
					rowList.Add(row);
				}
			}
			return result;
		}

		static Dictionary<IColumnValueSet, List<DateRangeEntityAndCriteriaSet>> BuildCriteriaToEntity(
			IEnumerable<DateRangeEntityAndCriteriaSet> list)
		{
			var result = new Dictionary<IColumnValueSet, List<DateRangeEntityAndCriteriaSet>>();
			foreach (var elem in list)
			{
				if (!result.TryGetValue(elem, out var matchingList))
				{
					result.Add(elem, new List<DateRangeEntityAndCriteriaSet>(1) { elem });
				}
				else
				{
					matchingList.Add(elem);
				}
			}
			return result;
		}

		List<DateRangeEntityAndCriteriaSet> BuildEntityCriteriaList(IEntityDefinition definition, IEnumerable<IEntity> entities, ERConverter converter)
		{
			List<DateRangeEntityAndCriteriaSet> list = new List<DateRangeEntityAndCriteriaSet>(entities.Count());
			foreach (var entity in entities)
			{
				var criteriaList = CriteriaHelper.GetCriteriaUsingAllTagsSpecifiedInData(entity, converter,
					definition.DateRangeStartField, definition.DateRangeEndField);
				list.Add(new DateRangeEntityAndCriteriaSet(this, entity, criteriaList));
			}
			return list;
		}

		static void AddOtherValuesFilter(ZQuery filterQuery, ITableSchema tableSchema, int entityCount, Dictionary<string, HashSet<object>> columnValues)
		{
			foreach (var column in columnValues)
			{
				var values = column.Value;
				var schemaColumn = tableSchema.GetSchemaColumn(column.Key);
				if (values.Count == 1)
				{
					filterQuery.AddToFilter(schemaColumn, values.First());
				}
				// Don't bother filtering if there are many entities and many distinct values (more than 1 distict value for every 5 rows)
				else if (values.Count < 50 || values.Count <= entityCount / 5)
				{
					filterQuery.AddToFilter(schemaColumn, values);
				}
			}
		}

		/// <summary>
		/// Build a map from column name to the set of values in that column.
		/// </summary>
		static Dictionary<string, HashSet<object>> BuildColumnValueMap(IEnumerable<DateRangeEntityAndCriteriaSet> list)
		{
			var columnValues = new Dictionary<string, HashSet<object>>();
			foreach (var entityCriteria in list)
			{
				foreach (var criteria in entityCriteria.CriteriaListOrderedByName)
				{
					var columnName = criteria.ColumnName;
					if (!columnValues.TryGetValue(columnName, out var valueMap))
					{
						valueMap = new HashSet<object>();
						columnValues.Add(columnName, valueMap);
					}

					valueMap.Add(criteria.Value);
				}
			}

			return columnValues;
		}

		/// <summary>
		/// An ordered set of columns from a DataTable.
		/// Used for fast lookup of values by index.
		/// </summary>
		sealed class DataTableColumnSet
		{
			public DataTableColumnSet(DataTable table, IList<string> columnNames)
			{
				columnIndices = new int[columnNames.Count];
				int index = 0;
				foreach (var name in columnNames)
				{
					columnIndices[index++] = table.Columns.IndexOf(name);
				}
			}
			readonly int[] columnIndices;

			public int ColumnCount => columnIndices.Length;
			public int GetDataRowColumnIndex(int index) => columnIndices[index];
		}

		sealed class DataRowColumnValueSet : IColumnValueSet
		{
			public DataRowColumnValueSet(DataRow row, DataTableColumnSet columnSet)
			{
				this.Row = row;
				this.columnSet = columnSet;
				hashCode = CalculateHashCode();
			}
			public DataRow Row;
			readonly DataTableColumnSet columnSet;
			readonly int hashCode;

			public int ColumnCount => columnSet.ColumnCount;
			public bool Equals(IColumnValueSet other) => ColumnValueSetComparer.IsEqual(this, other);
			public object GetValue(int columnSetIndex) => Row[columnSet.GetDataRowColumnIndex(columnSetIndex)];
			public override int GetHashCode() => hashCode;

			int CalculateHashCode()
			{
				int result = 0;
				int numColumns = columnSet.ColumnCount;
				for (int i = 0; i < numColumns; ++i)
				{
					var columnIndex = columnSet.GetDataRowColumnIndex(i);
					var val = Row[columnIndex];
					result = AppendHashCode(result, val);
				}
				return result;
			}

			public static int AppendHashCode(int hash, object val) => ColumnValueSetComparer.AppendHashCode(hash, val);
		}

		protected virtual ZQuery GetDateRangeFilter(SchemaColumn startDateSchemaColumn, SchemaColumn endDateSchemaColumn, ZDateTime startDate, ZDateTime endDate)
		{
			var filterQuery = new ZQuery();
			var startDateQuery = new ZQuery(endDateSchemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
			startDateQuery.AddToFilter(JoinCondition.Or, endDateSchemaColumn, DBNull.Value);

			filterQuery.AddToFilter(startDateQuery);

			if (!endDate.IsEmpty)
			{
				filterQuery.AddToFilter(startDateSchemaColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, endDate);
			}
			return filterQuery;
		}

		class DateRangeEntityAndCriteriaSet : EntityAndCriteria
		{
			public DateRangeEntityAndCriteriaSet(DateRangeUpdateBehaviour behaviour, IEntity entity, IEnumerable<Criteria> criteriaList)
				: base(entity, criteriaList)
			{
				StartDate = behaviour.GetDate(entity, entity.Definition.DateRangeStartField);
				EndDate = behaviour.GetDate(entity, entity.Definition.DateRangeEndField);
				CriteriaColumnNames = new HashSet<string>(CriteriaListOrderedByName.Select(x => x.ColumnName));
			}

			public ZDateTime StartDate { get; set; }
			public ZDateTime EndDate { get; set; }
			public HashSet<string> CriteriaColumnNames { get; }
		}

		#endregion

		void UpdateDateRangeForAllEntities(
			BehaviourContext behaviourContext,
			IEnumerable<DateRangeEntityAndCriteriaSet> entityCriteriaList,
			IList<DataRow> matchingRowsInDb,
			List<string> columnNamesOrdered,
			ZDateTime startDate,
			ZDateTime endDate)
		{
			var criteriaToRows = BuildCriteriaToMatchingRows(matchingRowsInDb, columnNamesOrdered);

			foreach (var entityAndCriteriaSet in entityCriteriaList)
			{
				var entity = entityAndCriteriaSet.Entity;

				if (criteriaToRows.TryGetValue(entityAndCriteriaSet, out var matchingRows))
				{
					UpdateDateRange(behaviourContext, entity, matchingRows, startDate, endDate);
				}
			}
		}

		protected virtual void UpdateDateRange(BehaviourContext behaviourContext, IEntity entity, IEnumerable<DataRow> rows, ZDateTime entityFrom, ZDateTime entityTo)
		{
			CheckAnyRowDateRangeIsWithinEntityDateRange(behaviourContext, entity, rows, entityFrom, entityTo);

			foreach (var row in rows)
			{
				var rowFrom = GetDate(row[entity.Definition.DateRangeStartField]);
				UpdateStartOrEndDate(behaviourContext, entity, entityFrom, entityTo, row, rowFrom);
			}
		}

		protected virtual void UpdateStartOrEndDate(BehaviourContext behaviourContext, IEntity entity, ZDateTime entityFrom, ZDateTime entityTo, DataRow row, ZDateTime rowFrom)
		{
			string propertyToUpdate;
			DateTime newDate;

			if (rowFrom < entityFrom)
			{
				propertyToUpdate = entity.Definition.DateRangeEndField;
				newDate = entityFrom.AddDays(-1).ToDateTime();
			}
			else
			{
				propertyToUpdate = entity.Definition.DateRangeStartField;
				newDate = entityTo.AddDays(1).ToDateTime();
			}

			var originalDate = row[propertyToUpdate] == DBNull.Value ? DateTime.MinValue : (DateTime)row[propertyToUpdate];
			row[propertyToUpdate] = newDate;
			propertyToUpdate = propertyToUpdate.Remove(0, entity.Definition.TablePrefix.Length + 1);

			sessionServices.Logger.Log(LogType.Information,
				LogHelper.GetExpirationInfoLogMessage(behaviourContext, entity, propertyToUpdate, originalDate, newDate));
		}

		protected void CheckAnyRowDateRangeIsWithinEntityDateRange(BehaviourContext behaviourContext, IEntity entity, IEnumerable<DataRow> rows, ZDateTime entityFrom, ZDateTime entityTo)
		{
			foreach (var row in rows)
			{
				var rowFrom = GetDate(row[entity.Definition.DateRangeStartField]);
				var rowTo = GetDate(row[entity.Definition.DateRangeEndField]);

				ThrowIfDateRangeConflict(behaviourContext, entity, entityFrom, entityTo, rowFrom, rowTo);
			}
		}

		protected virtual void ThrowIfDateRangeConflict(BehaviourContext behaviourContext, IEntity entity, ZDateTime entityFrom, ZDateTime entityTo, ZDateTime rowFrom, ZDateTime rowTo)
		{
			if (rowFrom >= entityFrom && (entityTo.IsEmpty || rowTo <= entityTo))
			{
				string formattedMessage = LogHelper.GetDateRangeConflictLogMessage(behaviourContext, entity, rowFrom, rowTo, entityFrom, entityTo);

				throw new NativeXMLUserVisibleException(formattedMessage);
			}
		}

		protected virtual ZDateTime GetDate(IEntity entity, string fieldName)
		{
			var field = entity.Properties.FirstOrDefault(x => x.Definition.ColumnDef.Name == fieldName) ?? throw new NativeXMLUserVisibleException(EntityDefinitionDateRangeAttributesIncorrect);
			return GetDate(field.Value).Date;
		}

		protected static ZDateTime GetDate(object date)
		{
			if (date is DBNull)
			{
				return ZDateTime.Empty;
			}

			if (date is string dateString)
			{
				if (string.IsNullOrEmpty(dateString))
				{
					return ZDateTime.Empty;
				}

				ZDateTime result;
				if (!ZDateTime.TryParseISO8601Date(dateString, out result))
				{
					throw new NativeXMLUserVisibleException(string.Format("Invalid date format: {0}", date));
				}

				return result;
			}

			return (DateTime)date;
		}
	}
}
