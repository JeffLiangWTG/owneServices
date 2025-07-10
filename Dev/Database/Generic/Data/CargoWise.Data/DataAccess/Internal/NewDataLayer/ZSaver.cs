using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Integration;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.EntityFramework
{
	public abstract class ZSaver : ZPersistentOperationPerformer
	{
		protected ZSaver(DataSet data)
			: base(data)
		{
		}

		public const string TableDoesNotExistIdentifier = "***Table Does Not Exist***"; // exception message

		readonly List<DataRow> RowsToUndelete = new List<DataRow>();
		public IChangedTableNames Save()
		{
			IList<DataRow> rows = GetModifiedPersistentRowsInSaveOrder(GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>());

			var changedTables = new ChangedTableNames(rows.Select(x => x.Table.TableName).Distinct().ToList());

			foreach (var row in RowsToUndelete)
			{
				row.RejectChanges();

				var fkColumnNames = ZRowRelationshipManager.GetFKColumnNames(row.Table);
				foreach (var fkColumnName in fkColumnNames)
				{
					if (row.Table.Columns[fkColumnName].AllowDBNull)
					{
						row[fkColumnName] = DBNull.Value;
					}
					else
					{
						row[fkColumnName] = Guid.Empty;
					}
				}
			}

			if (rows.Count > 0)
			{
				try
				{
					while (rows.Count > 0)
					{
						try
						{
							SaveRows(rows);
							break;
						}
						catch (ZDataConcurrencyException ex) when (HasOnlyLightValidationChange(ex.Row) && RowExistsInDatabaseForConcurrencyHandling(ex.Row))
						{
							var exceptionRow = ex.Row;
							var indexOfRow = rows.IndexOf(exceptionRow);
							if (indexOfRow != -1)
							{
								var rowsWithSuccess = rows.Take(indexOfRow).ToList();

								ex.RecoverAction?.Invoke(rowsWithSuccess.Select(DataUtils.GetPk).ToList());

								if (ex.RecoverAction == null && rowsWithSuccess.Any(HasDataSourceColumn))
								{
									throw;
								}

								rows = rows.Skip(indexOfRow).ToList();
							}
							rows.Remove(exceptionRow);
							exceptionRow.RejectChanges();
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RowsToUndelete.ForEach(x => x.Delete());

					throw;
				}
			}

			if (RowsToUndelete.Count > 0)
			{
				foreach (var row in RowsToUndelete)
				{
					row.Delete();
				}
				SaveRows(RowsToUndelete);
			}

			RowsToUndelete.Clear();

			return changedTables;
		}

		protected internal bool HasOnlyLightValidationChange(DataRow row)
		{
			if (row == null)
			{
				return false;
			}

			var columns = row.Table.Columns;
			var lightValidationColumn = columns.Cast<DataColumn>().FirstOrDefault(IsLightValidationColumn);
			if (lightValidationColumn == null)
			{
				return false;
			}

			for (int i = 0; i < columns.Count; i++)
			{
				var col = columns[i];
				if (col != lightValidationColumn && !ZUpdateCommandBuilderBase.IsSystemColumn(col) && IsRowValueChanged(row, col))
				{
					// Do not allow auto-merging light validation in concurrency check if there are other non-system column changes
					return false;
				}
			}

			return true;
		}

		protected virtual bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return DataUtils.IsRowValueChanged(row, column);
		}

		protected internal static bool IsLightValidationColumn(DataColumn column)
		{
			var columnName = column.ColumnName;
			var prefixLength = columnName.IndexOf('_');
			if (prefixLength > 0)
			{
				columnName = columnName.Substring(prefixLength);
			}

			return columnName.Equals(Schema.Schema.IsValidColumnSuffix, StringComparison.OrdinalIgnoreCase);
		}

		protected virtual bool HasDataSourceColumn(DataRow row)
		{
			return false;
		}

		protected internal bool RowExistsInDatabaseForConcurrencyHandling(DataRow row)
		{
			if (row == null)
			{
				return false;
			}

			try
			{
				return RowExistsInDatabaseCore(row);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return false;
			}
		}

		protected abstract bool RowExistsInDatabaseCore(DataRow row);

		public enum SaveOperation
		{
			InsertUpdate,
			Delete
		}

		protected abstract void SaveRows(IList<DataRow> rows);

		public List<DataRow> GetModifiedPersistentRowsInSaveOrder(IApplicationSchemaResolver schemaResolver)
		{
			var insertUpdateRows = new List<DataRow>();
			var deleteRows = new List<DataRow>();
			var tables = GetTableSaveOrderBasedOnRelationships();

			for (int i = tables.Length - 1; i >= 0; i--)
			{
				deleteRows.AddRange(GetOrderedPersistentRows(tables[i], SaveOperation.Delete, schemaResolver));
			}

			foreach (DataTable table in tables)
			{
				insertUpdateRows.AddRange(GetOrderedPersistentRows(table, SaveOperation.InsertUpdate, schemaResolver));
			}

			return CombineInsertsDeletesAndUpdates(insertUpdateRows, deleteRows);
		}

		static DataTable GetFirstTableThatRequiresDeletesAfterUpdates(IEnumerable<DataRow> insertUpdateRows, IEnumerable<DataRow> deleteRows)
		{
			DataTable lastTable = null;
			IEnumerable<string> lastTableFKs = null;
			Dictionary<object, object> oldFKs = new Dictionary<object, object>();

			foreach (DataRow row in insertUpdateRows)
			{
				if (row.HasVersion(DataRowVersion.Original) && row.RowState != DataRowState.Deleted) // update
				{
					if (row.Table != lastTable)
					{
						lastTableFKs = ZRowRelationshipManager.GetFKColumnNames(row.Table);
						lastTable = row.Table;
					}
					Debug.Assert(lastTableFKs != null);
					foreach (string fkColumnName in lastTableFKs)
					{
						object originalFK = row[fkColumnName, DataRowVersion.Original];
						if (originalFK != row[fkColumnName]) // FK value has changed
						{
							oldFKs[originalFK] = null;
						}
					}
				}
			}

			lastTable = null;
			string lastTablePK = null;

			foreach (DataRow row in deleteRows)
			{
				if (row.Table != lastTable)
				{
					lastTablePK = row.Table.PrimaryKey[0].ColumnName;
					lastTable = row.Table;
				}
				Debug.Assert(lastTablePK != null);
				object deletedPK = row[lastTablePK, DataRowVersion.Original];
				if (oldFKs.ContainsKey(deletedPK))
				{
					return lastTable;
				}
			}

			return null;
		}

		List<DataRow> CombineInsertsDeletesAndUpdates(List<DataRow> insertUpdateRows, List<DataRow> deleteRows)
		{
			if (deleteRows.Count == 0)
			{
				return insertUpdateRows;
			}
			if (insertUpdateRows.Count == 0)
			{
				return deleteRows;
			}

			List<DataRow> result = new List<DataRow>();
			List<DataRow> deletesBeforeUpdates = new List<DataRow>();
			List<DataRow> deletesAfterUpdates = new List<DataRow>();

			DataTable firstTableToBeDeletedAfterUpdates = GetFirstTableThatRequiresDeletesAfterUpdates(insertUpdateRows, deleteRows);
			bool allFurtherDeletesShouldBeAfterUpdates = false;

			foreach (DataRow deleteRow in deleteRows)
			{
				if (allFurtherDeletesShouldBeAfterUpdates ||
					deleteRow.Table == firstTableToBeDeletedAfterUpdates)
				{
					deletesAfterUpdates.Add(deleteRow);
					allFurtherDeletesShouldBeAfterUpdates = true;
				}
				else
				{
					deletesBeforeUpdates.Add(deleteRow);
				}
			}

			result.AddRange(deletesBeforeUpdates);
			result.AddRange(insertUpdateRows);
			result.AddRange(deletesAfterUpdates);

			return result;
		}

		protected IEnumerable<DataRow> GetOrderedPersistentRows(DataTable table, SaveOperation operation, IApplicationSchemaResolver schemaResolver)
		{
			var selfReferentialFKColumnNames = ZRowRelationshipManager.GetSelfReferentialFKsForTable(table);
			if (selfReferentialFKColumnNames.Length > 0)
			{
				var orderer = new ZSelfReferentialRowOrderer(new ZDataRowCollection(table.Rows), selfReferentialFKColumnNames, operation, this, schemaResolver);
				return orderer.GetOrderedPersistentRows();
			}
			else
			{
				return GetPersistentRows(table, operation, schemaResolver).OrderBy(x => x.RowState == DataRowState.Added ? 1 : 0);
			}
		}

		IEnumerable<DataRow> GetPersistentRows(DataTable table, SaveOperation operation, IApplicationSchemaResolver schemaResolver)
		{
			foreach (DataRow row in table.Select("", "", GetDataViewRowStateForSaveOperation(operation)))
			{
				if (ShouldSaveRow(row, operation, schemaResolver))
				{
					yield return row;
				}
			}
		}

		internal bool ShouldSaveRow(DataRow row, SaveOperation operation, IApplicationSchemaResolver schemaResolver)
		{
			bool result = false;

			if (row != null && DataUtils.ShouldRowBeSaved(row) && row.Table.Columns.Count > 0)
			{
				if (operation == ZSaver.SaveOperation.InsertUpdate)
				{
					if (row.RowState == DataRowState.Added)
					{
						result = true;
					}
					else if (row.RowState == DataRowState.Modified)
					{
						result = DoesUpdateRowHavePersistentChangesFromOriginal(schemaResolver, row);
					}
				}
				else if (operation == ZSaver.SaveOperation.Delete)
				{
					result = row.RowState == DataRowState.Deleted;
				}
				else
				{
					throw new ApplicationException("Unknown save operation: " + operation.ToString());
				}
			}

			return result;
		}

		protected virtual bool DoesUpdateRowHavePersistentChangesFromOriginal(IApplicationSchemaResolver resolver, DataRow row)
		{
			return DataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, row);
		}

		DataViewRowState GetDataViewRowStateForSaveOperation(SaveOperation operation)
		{
			if (operation == SaveOperation.Delete)
			{
				return DataViewRowState.Deleted;
			}
			else if (operation == SaveOperation.InsertUpdate)
			{
				return DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
			}
			else
			{
				throw new NotSupportedException("Unknown Save operation");
			}
		}

		#region Saving Order Between Different Tables

		/// <summary>
		/// Get an array of tables in insert/update order based upon relationships.
		/// </summary>
		DataTable[] GetTableSaveOrderBasedOnRelationships()
		{
			return GetTableSaveOrderBasedOnRelationships(GetTablesThatNeedToBeSaved());
		}

		static DataTable[] GetTableSaveOrderBasedOnRelationships(DataTable[] tables)
		{
			DataTable[] result;
			var tableNames = new HashSet<string>(tables.Select(table => table.TableName));
			string[] sortedNames;
			if (CachedSortedTables.TryGetValue(tableNames, out sortedNames))
			{
				var query = from tableName in sortedNames
							join table in tables
							on tableName equals table.TableName
							select table;
				result = query.ToArray();
			}
			else
			{
				result = ZTableSaveOrderComparer.Sort(tables);
				sortedNames = result.Select(table => table.TableName).ToArray();
				CachedSortedTables[tableNames] = sortedNames;
			}
#if DEBUG
			LastTableSaveOrder = sortedNames;
#endif
			return result;
		}

		static Dictionary<HashSet<string>, string[]> CachedSortedTables
		{
			get { return cachedSortedTables ?? (cachedSortedTables = new Dictionary<HashSet<string>, string[]>(HashSet<string>.CreateSetComparer())); }
		}
		[ThreadStatic]
		static Dictionary<HashSet<string>, string[]> cachedSortedTables;

#if DEBUG
		[ThreadStatic]
		public static string[] LastTableSaveOrder;
#endif

		public DataTable[] GetTablesThatNeedToBeSaved()
		{
			List<DataTable> result = new List<DataTable>();
			foreach (DataTable table in Data.Tables)
			{
				foreach (DataRow row in table.Rows)
				{
					if (row.RowState != DataRowState.Unchanged && DataUtils.ShouldRowBeSaved(row))
					{
						result.Add(row.Table);
						break;
					}
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Save Order Within One Table - Self Referential Tables

		internal class ZSelfReferentialRowOrderer
		{
			public ZSelfReferentialRowOrderer(ZDataRowCollection rowsFromTableToOrder, string[] selfReferentialFKNames, SaveOperation saveOperation, ZSaver saver, IApplicationSchemaResolver schemaResolver)
			{
				this.Rows = rowsFromTableToOrder;
				this.FKNames = selfReferentialFKNames;
				this.SaveOperation = saveOperation;
				this.saver = saver;
				this.SchemaResolver = schemaResolver;
			}

			public IEnumerable<DataRow> GetOrderedPersistentRows()
			{
				List<DataRow> orderedRows = new List<DataRow>();

				saver.RowsToUndelete.Clear();

				Dictionary<Guid, SelfReferentialNode> pkToNodeHash = MakePKToNodeHash();

				while (pkToNodeHash.Values.Count > 0)
				{
					List<SelfReferentialNode> nodesWithNoParents = GetNodesWithNoParents(pkToNodeHash);
					if (nodesWithNoParents.Count == 0)
					{
						if (SaveOperation == ZSaver.SaveOperation.InsertUpdate)
						{
							SelfReferentialNode arbitraryNode = pkToNodeHash.Values.First();
							orderedRows.Add(arbitraryNode.Row);
							pkToNodeHash.Remove(arbitraryNode.PK);
						}
						else if (SaveOperation == ZSaver.SaveOperation.Delete)
						{
							SelfReferentialNode arbitraryNode = pkToNodeHash.Values.First();
							orderedRows.Add(arbitraryNode.Row);
							pkToNodeHash.Remove(arbitraryNode.PK);
							saver.RowsToUndelete.Add(arbitraryNode.Row);
						}
						else
						{
							#region SuppressResourceStringsCheckRegion
							string error = "Unable to establish row save order - circular references detected.\r\n\r\n";
							error += "FK Names: " + string.Join(", ", FKNames) + "\r\n";
							error += "Save Operation: " + SaveOperation + "\r\n";
							error += "Row Count: " + Rows.Count + "\r\n";

							error += "Row Details:\r\n";
							foreach (DataRow row in Rows)
							{
								error += "Row State: " + row.RowState + "\r\n";
								error += "Table Name: " + row.Table.TableName + "\r\n";
							}
							#endregion

							throw new NotSupportedException(error);
						}
					}

					foreach (SelfReferentialNode node in nodesWithNoParents)
					{
						orderedRows.Add(node.Row);
						pkToNodeHash.Remove(node.PK);
					}
				}

				if (SaveOperation == ZSaver.SaveOperation.Delete)
				{
					orderedRows.Reverse();
				}

				return orderedRows;
			}

			List<SelfReferentialNode> GetNodesWithNoParents(Dictionary<Guid, SelfReferentialNode> pkToNodeHash)
			{
				List<SelfReferentialNode> result = new List<SelfReferentialNode>();

				foreach (SelfReferentialNode node in pkToNodeHash.Values)
				{
					if (!node.HasInsertedOrDeletedMasterInHash(pkToNodeHash))
					{
						result.Add(node);
					}
				}

				return result;
			}

			Dictionary<Guid, SelfReferentialNode> MakePKToNodeHash()
			{
				var pkToNodeHash = new Dictionary<Guid, SelfReferentialNode>();
				foreach (DataRow row in Rows)
				{
					if (ShouldSaveRow(row))
					{
						var pk = DataUtils.GetPk(row);
						pkToNodeHash[pk] = new SelfReferentialNode(row, pk, FKNames);
					}
				}
				return pkToNodeHash;
			}

			protected bool ShouldSaveRow(DataRow row)
			{
				return saver.ShouldSaveRow(row, SaveOperation, SchemaResolver);
			}

			protected string[] FKNames;
			protected SaveOperation SaveOperation;
			protected ZDataRowCollection Rows;
			protected ZSaver saver;
			protected IApplicationSchemaResolver SchemaResolver { get; }

			class SelfReferentialNode
			{
				public SelfReferentialNode(DataRow row, Guid pk, string[] fKNames)
				{
					Row = row;
					PK = pk;
					MastersPKs = GetMastersPKs(fKNames, row, PK).ToArray();
				}

				Guid[] MastersPKs { get; }

				public Guid PK { get; }

				public DataRow Row { get; }

				public bool HasInsertedOrDeletedMasterInHash(Dictionary<Guid, SelfReferentialNode> hash)
				{
					return MastersPKs.Any(pk => hash.TryGetValue(pk, out SelfReferentialNode node) && (node.Row.RowState == DataRowState.Added || node.Row.RowState == DataRowState.Deleted));
				}

				static IEnumerable<Guid> GetMastersPKs(string[] fkNames, DataRow row, Guid pk)
				{
					foreach (var fkName in fkNames)
					{
						var fkValue = row.RowState == DataRowState.Deleted ? row[fkName, DataRowVersion.Original] : row[fkName];

						if (fkValue != DBNull.Value)
						{
							var guid = (Guid)fkValue;
							if (guid != Guid.Empty && guid != pk)
							{
								yield return guid;
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
