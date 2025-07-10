using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.AuditLogs;
using Enterprise.DataTransfer.Native.Common.Behaviours;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Stat;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.EntityRepositories
{
	public sealed class EntityRepository : IEntityRepository
	{
		public EntityRepository(IEntityContext context, AncillaryImportServices sessionServices)
		{
			this.context = context;
			statistics = context.Statistics;
			logFieldUpdater = new LogFieldUpdater();
			this.sessionServices = sessionServices;
		}

		readonly IEntityContext context;
		readonly LogFieldUpdater logFieldUpdater;
		readonly StatisticsImpl statistics;
		readonly AncillaryImportServices sessionServices;
		ERConverter converter;
		IRowRepository rowRepository;
		List<IEntity> entitiesToApplyBehaviour;

		#region Insert

		public IEntity Insert(IEntity entity)
		{
			InsertRow(entity);
			return entity;
		}

		DataRow InsertRow(IEntity entity)
		{
			entity.InternalPK = context.AlwaysUseInternalPK && entity.InternalPK != Guid.Empty
				? entity.InternalPK
				: Guid.NewGuid();
			entity.Action = EntityAction.INSERT;
			var table = entity.Definition.Table;
			var row = rowRepository.Create(table, entity.InternalPK);
			converter.UpdateRowFromEntity(entity, context, row);

			logFieldUpdater.Update(row, table, EntityAction.INSERT);

			statistics.Add(new DBEntity(entity.Definition.EntityName, entity.InternalPK, Stat.DBEntity.DbAction.Insert));

			if (entity.Definition.Behaviour != null)
			{
				entitiesToApplyBehaviour.Add(entity);
			}

			InsertAssociation(entity);
			return row;
		}

		IEntity InsertAssociation(IEntity entity)
		{
			var table = entity.Definition.Table;
			var row = rowRepository.Show(entity.InternalPK, table);
			converter.SetAssociationRows(row, entity, association => rowRepository.Create(association.JunctionTable));
			return entity;
		}

		#endregion

		#region Update

		public IEntity Update(IEntity entity)
		{
			try
			{
				var row = FindRow(entity);
				UpdateRowFromEntity(entity, row);
				return entity;
			}
			catch (InvalidOperationException)
			{
				if (entity.Definition.IsUpdateOrInsert)
				{
					return Insert(entity);
				}
				throw;
			}
		}

		public void SetModified(IEntity entity)
		{
			var row = FindRow(entity);
			if (row.RowState != DataRowState.Modified)
			{
				row.SetModified();
			}
		}

		void UpdateRowFromEntity(IEntity entity, DataRow row)
		{
			if (CheckEntityBeforeProcess(entity, row))
			{
				var rowHasAlreadyBeenModified = (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added);

				UpdateAssociation(entity, row);

				var table = entity.Definition.Table;
				entity.InternalPK = (Guid)row[entity.Definition.Id.ColumnDef.Name];
				entity.Action = EntityAction.UPDATE;
				converter.UpdateRowFromEntity(entity, context, row);

				if (row.RowState != DataRowState.Unchanged)
				{
					logFieldUpdater.Update(row, table, EntityAction.UPDATE);
					if (!rowHasAlreadyBeenModified)
					{
						statistics.Add(new DBEntity(entity.Definition.EntityName, entity.InternalPK, Stat.DBEntity.DbAction.Update));
					}
				}
				else
				{
					if (!rowHasAlreadyBeenModified)
					{
						statistics.Add(new DBEntity(entity.Definition.EntityName, entity.InternalPK, Stat.DBEntity.DbAction.Unchanged));
					}
				}
			}
		}

		void UpdateAssociation(IEntity entity, DataRow row)
		{
			converter.SetAssociationRows(row, entity, association => FindJunctionRow(entity.Parent.InternalPK, entity.InternalPK, association));
		}

		#endregion

		#region Delete

		public IEntity Delete(IEntity entity)
		{
			var row = FindRow(entity);
			if (CheckEntityBeforeProcess(entity, row))
			{
				DeleteAssociation(entity);
				var pkName = row.Table.PrimaryKey[0];
				var index = row.Table.Columns.IndexOf(pkName);
				var rowPK = (Guid)row.ItemArray[index];
				if (entity.InternalPK == Guid.Empty || entity.InternalPK != rowPK)
				{
					entity.InternalPK = rowPK;
				}

				entity.Action = EntityAction.DELETE;
				converter.UpdateRowFromEntity(entity, context, row, isForDelete: true);
				rowRepository.Delete(row);

				statistics.Add(new DBEntity(entity.Definition.EntityName, entity.InternalPK, Stat.DBEntity.DbAction.Delete));

				if (entity.Definition.Behaviour != null)
				{
					entitiesToApplyBehaviour.Add(entity);
				}
			}
			return entity;
		}

		void DeleteAssociation(IEntity entity)
		{
			//Remove Many - Many Association
			var definition = entity.Definition;
			var association = definition.MainAssociation;
			if (association != null && association is ManyToManyAssociation)
			{
				var junctionRow = FindJunctionRow(entity.Parent.InternalPK, entity.InternalPK, (ManyToManyAssociation)association);
				if (junctionRow != null)
				{
					rowRepository.Delete(junctionRow);
				}
			}
		}

		void DeleteChildren(DataRow[] rows, SchemaColumn parentPkColumn, SchemaColumn childRelationColumn, string childTableName)
		{
			foreach (var row in rows.WhereNotNull())
			{
				var query = new ZQuery(childRelationColumn, row[parentPkColumn.Name]);
				var childrenRows = rowRepository.Load(childTableName, query);
				childrenRows.ForEach(childRow => rowRepository.Delete(childRow));
			}
		}

		#endregion

		#region Find Row

		public DataRow FindRow(IEntity entity, bool throwExceptionOnNotFound = true)
		{
			DataRow result = null;
			var rows = new DataRow[1];
			var criteriasNotFound = new IEnumerable<Criteria>[1];
			if (FindRows(new[] { entity }, criteriasNotFound, rows))
			{
				result = rows[0];
			}
			else if (throwExceptionOnNotFound)
			{
				throw new NativeXMLUserVisibleException(CreateEntityNotFoundExceptionMessage(entity, criteriasNotFound[0]));
			}
			return result;
		}

		DataRow FindHTIRow(IEntity entity)
		{
			DataRow row = null;
			var table = entity.Definition.Table;
			var criteriaList = CriteriaHelper.GetCriteriaUsingAllTagsSpecifiedInData(entity, converter);
			if (rowRepository is RowRepository repository && criteriaList.Any())
			{
				FixCodeMappedReferences(criteriaList);

				var foundRows = repository.ShowAll(criteriaList, table);
				if (foundRows != null && foundRows.Length > 0)
				{
					var attrs1 = GetHTIAttributes(entity, "AT1");
					var attrs2 = GetHTIAttributes(entity, "AT2");
					var attrs3 = GetHTIAttributes(entity, "AT3");

					foreach (var foundRow in foundRows)
					{
						ZGuid foundRowPK = ZGuid.Empty;
						if (ZGuid.TryParse(foundRow[CusClassPartPivotSchema.Constants.PK], out foundRowPK) &&
							IsHTIAttributesEqualToEntityAttributes(foundRowPK, attrs1, "AT1") &&
							IsHTIAttributesEqualToEntityAttributes(foundRowPK, attrs2, "AT2") &&
							IsHTIAttributesEqualToEntityAttributes(foundRowPK, attrs3, "AT3"))
						{
							row = foundRow;
							break;
						}
					}
				}
			}
			return row;
		}

		DataRow FindJunctionRow(Guid parentKey, Guid childKey, ManyToManyAssociation association)
		{
			var junctionTable = association.JunctionTable;
			var parentKeyName = association.ParentJunctionFk.Name; // Has Junction Table
			var childKeyName = association.ChildJunctionFk.Name; // Has Junction Table

			var criteria1 = new Criteria
			{
				TableName = junctionTable.Name,
				ColumnName = parentKeyName,
				Value = parentKey
			};

			var criteria2 = new Criteria
			{
				TableName = junctionTable.Name,
				ColumnName = childKeyName,
				Value = childKey
			};
			return rowRepository.Show(new[] { criteria1, criteria2 }, junctionTable);
		}

		DataRow FindRowByCriteriaList(Table table, IEnumerable<Criteria> criteria)
		{
			if (criteria != null)
			{
				var criteriaList = criteria as IList<Criteria> ?? criteria.ToList();
				if (criteriaList.Count > 0)
				{
					FixCodeMappedReferences(criteriaList);
					return rowRepository.Show(criteriaList, table);
				}
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		static string CreateEntityNotFoundExceptionMessage(IEntity entity, IEnumerable<Criteria> criteriaList)
		{
			var parentName = DataBoundResourceStrings.GetTableDescriptiveName(entity.EntityName);
			var stringBuilder = new StringBuilder();
			if (entity.InternalPK != Guid.Empty)
			{
				stringBuilder.Append("[PK:").Append(entity.InternalPK).Append("]");
				if (criteriaList != null && criteriaList.Any())
				{
					stringBuilder.Append(" OR ");
				}
			}
			if (criteriaList != null)
			{
				if (entity.Parent != null && entity.Parent.InternalPK != Guid.Empty)
				{
					criteriaList = criteriaList.Where(c => !(c.ColumnName.Contains("_" + entity.Parent.Definition.TablePrefix) && c.Value.ToString() == entity.Parent.InternalPK.ToString()));
				}

				foreach (var criteria in criteriaList)
				{
					stringBuilder.Append("[" + criteria.ColumnName.Substring(criteria.ColumnName.IndexOf('_') + 1) + ":" + criteria.Value.ToString() + "]");
				}
			}

			return string.IsNullOrEmpty(stringBuilder.ToString())
				? string.Format(CultureInfo.InvariantCulture, "Unable to find {0} to update using the details provided.", parentName)
				: string.Format(CultureInfo.InvariantCulture, "There is no {0} with the following values: {1}.", parentName, stringBuilder.ToString());
		}

		static void FixCodeMappedReferences(IEnumerable<Criteria> criteriaList)
		{
			foreach (var criteria in criteriaList)
			{
				var codeMapping = EDICodeMapper.FindGlobalCodeMapping(criteria.TableName, criteria.ColumnName.Substring(3));
				if (codeMapping != null && !string.IsNullOrEmpty(codeMapping.ForeignCode))
				{
					if (codeMapping.ForeignCode == criteria.Value.ToString())
					{
						if (codeMapping.LocalCode != codeMapping.ForeignCode)
						{
							criteria.Value = codeMapping.LocalCode;
						}
					}
				}
			}
		}

		bool IsPivotChildTypeHTI(IEntity entity) => entity.EntityName == "CusClassPartPivot" && entity.HasProperty("ChildType") && entity["ChildType"].ToString() == "HTI";

		IEnumerable<string> GetHTIAttributes(IEntity obj, ZString attributeName) => obj.Children
			.Where(elem => elem.GetPropertyOrBlankString("AttributeName") == attributeName)
			.Select(elem => elem.GetPropertyOrBlankString("AttributeValue1")).Distinct();

		bool IsHTIAttributesEqualToEntityAttributes(ZGuid foundRowPK, IEnumerable<string> entityAttributes, ZString attributeName)
		{
			var query = new ZQuery(new ZQuery(CusAttributeFilterSchema.BG_CI, foundRowPK), JoinCondition.And, new ZQuery(CusAttributeFilterSchema.BG_AttributeName, attributeName));
			var foundAttrs = rowRepository.Load(CusAttributeFilterSchema.Constants.TableName, query)
				.Select(x => x[CusAttributeFilterSchema.Constants.BG_AttributeValue1].ToString()).Distinct();
			return (new HashSet<string>(entityAttributes)).SetEquals(foundAttrs);
		}

		/// <summary>
		/// Bulk find.
		/// Entities must have the same criteria:
		/// - the same table
		/// - same property names
		/// - same parent names
		/// An InvalidOperationException is thrown in RowRepository if the criteria have different names.
		/// </summary>
		/// <param name="entityList">list of entities to find</param>
		/// <param name="criteriaNotFound">array of matching criteria to be populated if entity not found, for diagnostic purposes. Length must be at least entity count.</param>
		/// <param name="rows">array of matching rows to be populated. Length must be at least entity count.</param>
		/// <returns>true if all entities found, false otherwise</returns>
		bool FindRows(IList<IEntity> entityList, IEnumerable<Criteria>[] criteriaNotFound, DataRow[] rows)
		{
			var firstEntity = entityList.First();
			var table = firstEntity.Definition.Table;
			int rowsFound = 0;

			// Internal PK lookup done one by one for now.
			for (int i = 0; i < entityList.Count; ++i)
			{
				var entity = entityList[i];
				var row = entity.InternalPK != Guid.Empty
					? rowRepository.Show(entity.InternalPK, table)
					: null;
				if (row != null)
				{
					rows[i] = row;
					++rowsFound;
				}
			}

			if (rowsFound == entityList.Count)
			{
				return true;
			}

			for (int i = 0; i < entityList.Count; ++i)
			{
				if (rows[i] == null && !CanFindByCriteria(entityList[i]))
				{
					return false;
				}
			}

			if (table.CandidateKeyConstraints.Any())
			{
				// Candidate key lookup - done one by one for now.
				for (int i = 0; i < entityList.Count; ++i)
				{
					if (rows[i] == null)
					{
						var entity = entityList[i];
						var row = CriteriaHelper.GetCriteriaFromCandidateKey(entity, converter)
								.Select(criterias => FindRowByCriteriaList(table, criterias))
								.FirstOrDefault(r => r != null);
						if (row != null)
						{
							rows[i] = row;
							++rowsFound;
						}
					}
				}
			}

			if (rowsFound == entityList.Count)
			{
				return true;
			}

			if (IsPivotChildTypeHTI(firstEntity))
			{
				return FindOneByOne(entityList, rows, FindHTIRow);
			}

			switch (firstEntity.EntityName)
			{
				case WhsProductParamsByWhsAndClientSchema.Constants.TableName:
					return FindOneByOneForSpecifiedColumnNames(entityList, criteriaNotFound, rows, "W3_OH", "W3_WW", "W3_OP");

				case StmNoteSchema.Constants.TableName:
					return FindNotes(entityList, criteriaNotFound, rows);

				case UNDGSubstanceSchema.Constants.TableName:
					return FindUNDGSubstance(entityList, criteriaNotFound, rows);

				case RateEntrySchema.Constants.TableName when PropertyHasValue(firstEntity, "ProviderReferenceID"):
				{
					var result = FindOneByOneForSpecifiedColumnNames(entityList, criteriaNotFound, rows, RateEntrySchema.Constants.TI_ProviderReferenceID, RateEntrySchema.Constants.TI_TH);

					// Last resort, in case rates with ProviderReferenceIDs want to update existing ones having blank IDs: find rows by all tags
					return result || FindByAllTagsSpecifiedInData(entityList, criteriaNotFound, rows);
				}

				case RateLinesSchema.Constants.TableName when PropertyHasValue(firstEntity, "ProviderReferenceID"):
				{
					var result = FindOneByOneForSpecifiedColumnNames(entityList, criteriaNotFound, rows, RateLinesSchema.Constants.TL_ProviderReferenceID, RateLinesSchema.Constants.TL_TI);

					// Last resort, in case rates with ProviderReferenceIDs want to update existing one having blank IDs: find rows by all tags
					result = result || FindByAllTagsSpecifiedInData(entityList, criteriaNotFound, rows);

					// Given the RateLineItems have no ReferenceID,
					// their rows should be deleted so that the entities can be imported straight away.
					DeleteChildren(rows, RateLinesSchema.PK, RateLineItemsSchema.TM_TL, RateLineItemsSchema.Constants.TableName);

					return result;
				}

				default:
					return FindByAllTagsSpecifiedInData(entityList, criteriaNotFound, rows);
			}
		}

		static bool PropertyHasValue(IEntity entity, string propertyName)
		{
			var value = entity.GetPropertyOrBlankString(propertyName);
			return !string.IsNullOrEmpty(value);
		}

		/// <summary>
		/// Find by all tags for those entities not yet found (have a null entry in the rows array).
		/// </summary>
		bool FindByAllTagsSpecifiedInData(IList<IEntity> entityList, IEnumerable<Criteria>[] criteriasNotFound, DataRow[] rows)
		{
			var totalEntityCount = entityList.Count;
			var criterias = new List<IEnumerable<Criteria>>(totalEntityCount);
			var parentKeyColumnName = ParentKeyColumnName(entityList[0]);
			for (int i = 0; i < totalEntityCount; ++i)
			{
				if (rows[i] == null)
				{
					var entity = entityList[i];
					var criteriaList = CriteriaHelper.GetCriteriaUsingAllTagsSpecifiedInData(entity, converter);
					if (!HasCriteriaOtherThanParent(criteriaList, parentKeyColumnName))
					{
						return false;
					}
					FixCodeMappedReferences(criteriaList);
					criterias.Add(criteriaList);
				}
			}

			var loadedRows = rowRepository.LoadMany(criterias, entityList[0].Definition.Table);
			int rowIndex = 0;
			bool allFound = true;
			for (int i = 0; i < totalEntityCount; ++i)
			{
				if (rows[i] == null)
				{
					var loadedRow = loadedRows[rowIndex];
					if (loadedRow != null)
					{
						rows[i] = loadedRow;
					}
					else
					{
						criteriasNotFound[i] = criterias[rowIndex];
						allFound = false;
					}

					rowIndex++;
				}
			}
			return allFound;
		}

		bool HasCriteriaOtherThanParent(IEnumerable<Criteria> criteriaList, string parentKeyColumnName)
			=> criteriaList.Any(x => parentKeyColumnName == null || x.ColumnName != parentKeyColumnName);

		static string ParentKeyColumnName(IEntity entity)
		{
			if (entity.Parent == null)
			{
				return null;
			}
			var association = entity.Definition.AssociationCollection.FirstOrDefault(x => x.To == entity.Parent.Definition);
			return association != null && !(association is ManyToManyAssociation)
				? association.GetRelation(entity.Parent.Definition).Keys[0].FromKey.Name
				: null;
		}

		class EntityIndexColumnNames
		{
			public EntityIndexColumnNames(IEntity entity, int index, string[] columnNames)
			{
				this.Entity = entity;
				this.Index = index;
				this.ColumnNames = columnNames;
			}
			public IEntity Entity;
			public int Index;
			public string[] ColumnNames;
		}

		/// <summary>
		/// Find by specified column names for those entities not yet found (have a null entry in the rows array).
		/// Supports each entity having a different set of column names.
		/// Does a bulk search for each group with the same set of column names.
		/// </summary>
		bool FindBySpecifiedColumnNames(
			IList<IEntity> entityList,
			IEnumerable<Criteria>[] criteriasNotFound,
			DataRow[] rows,
			Func<IEntity, string[]> getColumnNames,
			bool isWithCriteriaFromRelatedEntity)
		{
			var table = entityList[0].Definition.Table;
			var entityListExtended = new List<EntityIndexColumnNames>(entityList.Count);
			for (int i = 0; i < entityList.Count; ++i)
			{
				if (rows[i] == null)
				{
					var entity = entityList[i];
					entityListExtended.Add(new EntityIndexColumnNames(entity, i, getColumnNames(entity)));
				}
			}

			bool allFound = true;
			foreach (var columnNameGroup in entityListExtended.GroupBy(x => x.ColumnNames))
			{
				var groupList = columnNameGroup.ToList();
				var criterias = new List<IEnumerable<Criteria>>(groupList.Count);
				for (int i = 0; i < groupList.Count; ++i)
				{
					criterias.Add(CriteriaHelper.GetCriteriaFromSpecifiedColumnNames(groupList[i].Entity, converter, isWithCriteriaFromRelatedEntity, columnNameGroup.Key));
				}

				var loadedRows = rowRepository.LoadMany(criterias, table);
				for (int i = 0; i < groupList.Count; ++i)
				{
					int outerIndex = groupList[i].Index;
					var loadedRow = loadedRows[i];
					if (loadedRow != null)
					{
						rows[outerIndex] = loadedRow;
					}
					else
					{
						criteriasNotFound[outerIndex] = criterias[i];
						allFound = false;
					}
				}
			}
			return allFound;
		}

		bool CanFindByCriteria(IEntity entity)
		{
			// NB: entity.DisableNativeEnginesOwnNaturalKeyMatch allows an interceptor to tell this engine not to bother trying to find a row if it could not find one.
			// See Product (OrgSupplierPart) for examples.
			return !entity.DisableNativeEnginesOwnNaturalKeyMatch
				&& (!context.AlwaysUseInternalPK || entity.InternalPK == Guid.Empty || entity.Action == EntityAction.EMPTY);
		}

		bool FindNotes(IList<IEntity> entityList, IEnumerable<Criteria>[] criteriasNotFound, DataRow[] rows)
		{
			var customColumnNames = new string[] { "ST_IsCustomDescription", "ST_Description" };
			var systemColumnNames = new string[] { "ST_IsCustomDescription", "ST_Description", "ST_NoteType", "ST_NoteContext" };
			Func<IEntity, string[]> entityToColumns = (entity) => GetNoteIsCustomDescription(entity)
				? customColumnNames
				: systemColumnNames;

			return FindBySpecifiedColumnNames(entityList, criteriasNotFound, rows, entityToColumns, true);
		}

		static bool GetNoteIsCustomDescription(IEntity entity)
		{
			var isCustomDescriptionFlag = false;
			var isCustomDescriptionProperty = entity.Properties.FirstOrDefault(p => p.Name == "IsCustomDescription");
			if (isCustomDescriptionProperty != null)
			{
				var isCustomDescription = isCustomDescriptionProperty.Value;
				isCustomDescriptionFlag = isCustomDescription is bool
					? (bool)isCustomDescription
					: 0 == string.Compare((string)isCustomDescription, "TRUE", StringComparison.OrdinalIgnoreCase);
			}
			return isCustomDescriptionFlag;
		}

		bool FindUNDGSubstance(IList<IEntity> entityList, IEnumerable<Criteria>[] criteriasNotFound, DataRow[] rows)
		{
			var emptyCodeColumnNames = new string[] { "DG_Standard", "DG_UniqueRecordId" };
			var codeColumnNames = new string[] { "DG_PK", "DG_Code", "DG_Standard" };
			Func<IEntity, string[]> entityToColumns = (entity) => GetUNDGSubstanceCodeIsEmpty(entity)
				? emptyCodeColumnNames
				: codeColumnNames;

			return FindBySpecifiedColumnNames(entityList, criteriasNotFound, rows, entityToColumns, true);
		}

		static bool GetUNDGSubstanceCodeIsEmpty(IEntity entity)
		{
			var codeValue = entity.Properties.FirstOrDefault(p => p.Name == "Code");
			return string.IsNullOrEmpty(codeValue?.Value?.ToString());
		}

		bool FindOneByOneByCriteriaList(IList<IEntity> entityList, IEnumerable<Criteria>[] criteriasNotFound, DataRow[] rows, Func<IEntity, IEnumerable<Criteria>> getCriteriaList)
		{
			bool allFound = true;
			for (int i = 0; i < entityList.Count; ++i)
			{
				if (rows[i] == null)
				{
					var entity = entityList[i];
					var criteriaList = getCriteriaList(entity);
					var row = FindRowByCriteriaList(entity.Definition.Table, criteriaList);
					if (row != null)
					{
						rows[i] = row;
					}
					else
					{
						criteriasNotFound[i] = criteriaList;
						allFound = false;
					}
				}
			}

			return allFound;
		}

		bool FindOneByOne(IList<IEntity> entityList, DataRow[] rows, Func<IEntity, DataRow> getRow)
		{
			bool allFound = true;
			for (int i = 0; i < entityList.Count; ++i)
			{
				if (rows[i] == null)
				{
					var entity = entityList[i];
					var row = getRow(entity);
					if (row != null)
					{
						rows[i] = row;
					}
					else
					{
						allFound = false;
					}
				}
			}
			return allFound;
		}

		bool FindOneByOneForSpecifiedColumnNames(IList<IEntity> entityList, IEnumerable<Criteria>[] criteriasNotFound, DataRow[] rows, params string[] columnNames)
		{
			return FindOneByOneByCriteriaList(entityList, criteriasNotFound, rows, (entity) => CriteriaHelper.GetCriteriaFromSpecifiedColumnNames(entity, converter, false, columnNames));
		}

		#endregion

		#region MergeBatch

		public void MergeBatch(IList<IEntity> entityList)
		{
			var totalEntityCount = entityList.Count;
			var rows = new DataRow[totalEntityCount];
			var criteriasNotFound = new IEnumerable<Criteria>[totalEntityCount];
			FindRows(entityList, criteriasNotFound, rows);
			var inserted = new Dictionary<IColumnValueSet, EntityRowAndCriteria>();
			for (int i = 0; i < totalEntityCount; ++i)
			{
				var entity = entityList[i];
				var row = rows[i];
				if (row != null)
				{
					UpdateRowFromEntity(entity, row);
				}
				else
				{
					var entityAndCriteria = new EntityRowAndCriteria(entity, criteriasNotFound[i]);
					if (!inserted.TryGetValue(entityAndCriteria, out var dupe))
					{
						row = InsertRow(entity);
						entityAndCriteria.Row = row;
						inserted.Add(entityAndCriteria, entityAndCriteria);
					}
					else
					{
						// Batch contains duplicates.
						UpdateRowFromEntity(entity, dupe.Row);
					}
				}
			}

#if DEBUG
			statistics.UpdateBatchMergeCountForTest(entityList[0].Definition.EntityName);
#endif
		}

		/// <summary>
		/// An EntityAndCriteria with a DataRow
		/// </summary>
		class EntityRowAndCriteria : EntityAndCriteria
		{
			public EntityRowAndCriteria(IEntity entity, IEnumerable<Criteria> criteriaList)
				: base(entity, criteriaList)
			{
			}

			public DataRow Row { get; set; }
		}

		#endregion

		#region IEntityRepository Members

		public IStatistics Statistics
		{
			get { return statistics; }
		}

		public void OpenSession()
		{
			converter = new ERConverter(context.Connection, sessionServices);
			rowRepository = new RowRepository(context.Connection, context.RowFactory);
			entitiesToApplyBehaviour = new List<IEntity>();
		}

		public void CloseSession()
		{
			if (entitiesToApplyBehaviour.Count > 0)
			{
				BehaviourApplicator.ApplyBehaviour(entitiesToApplyBehaviour, rowRepository, sessionServices, converter, context);
				entitiesToApplyBehaviour.Clear();
			}

			rowRepository.Save();
		}

		public IEntity Find(IEntity entity)
		{
			var row = FindRow(entity);
			return ERConverter.Revert(row, entity);
		}

		#endregion

		bool CheckEntityBeforeProcess(IEntity entity, DataRow row)
		{
			if (entity.TableName == RefCountrySchema.Constants.TableName)
			{
				sessionServices.Logger.Log(Enterprise.Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Warning: Cannot use Native XML to update/delete {0}.", entity.TableName));
				return false;
			}
			if (NativeXMLIsSystemValidator.IsSystemRowNonEditable(entity, row))
			{
				sessionServices.Logger.Log(Enterprise.Integration.LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Warning: Cannot use Native XML to update/delete System {0}.", entity.TableName));
				return false;
			}
			return true;
		}
	}
}
