using System;
using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public abstract class ZDataUtils
	{
		#region Constants

		public const DataViewRowState CurrentRowsNoDeletedFilter = DataViewRowState.CurrentRows;
		public const DataViewRowState CurrentRowsIncludingDeletedFilter = CurrentRowsNoDeletedFilter | DataViewRowState.Deleted;

		#endregion

		#region Get Primary Key Name / Value

		public static string GetPKNameFromTable(DataTable table)
		{
			return DataUtils.GetPkNameFromTable(table);
		}

		public static Guid GetPK(DataRow row)
		{
			return DataUtils.GetPk(row);
		}

		#endregion

		#region FindRowByPKIncludingDeleted

		[Obsolete("Z now uses ZDataTable - this supports finding deleted rows quickly")]
		public static DataRow FindRowByPKIncludingDeleted(ZDataTable table, Guid pK)
		{
			DataRow result = table.Rows.Find(pK);
			if (result == null)
			{
				string pKFilter = ZDataUtils.GetPKNameFromTable(table) + " = '" + pK.ToString() + "'";
				DataRow[] rowsWithCorrectPK = table.Select(pKFilter, "", ZDataUtils.CurrentRowsIncludingDeletedFilter);

				if (rowsWithCorrectPK.Length == 1)
				{
					result = rowsWithCorrectPK[0];
				}
				else if (rowsWithCorrectPK.Length > 1)
				{
					throw new ApplicationException(table.TableName + " has got duplicate PKs!");
				}
			}

			return result;
		}

		#endregion

		#region Get Foreign Key (from Dependent Table + Master Object)

		public static string GetFKNameFromDependentTableAndPKColumnName(DataTable dependentTable, string pKColumnName)
		{
			string dependentPK = CargoWise.Schema.Schema.GetPrefixFromColumnName(ZDataUtils.GetPKNameFromTable(dependentTable));
			string masterPK = CargoWise.Schema.Schema.GetPrefixFromColumnName(pKColumnName);

			return string.Intern(dependentPK + "_" + masterPK);
		}

		#endregion

		#region AppendFilter

		public static string CombineFilters(string baseFilter, string additionalFilter)
		{
			return CombineFilters(baseFilter, additionalFilter, "AND");
		}

		public static string CombineFilters(string baseFilter, string additionalFilter, string joinCondition)
		{
			string result = baseFilter;

			if (string.IsNullOrEmpty(baseFilter))
			{
				result = additionalFilter;
			}
			else if (!string.IsNullOrEmpty(additionalFilter))
			{
				result = baseFilter + " " + joinCondition + " (" + additionalFilter.Trim() + ")";
			}

			return result;
		}

		#endregion

		#region GetUsernameOfLastModifier

		public static string GetUsernameAndTimeOfLastModification(DataRow row, bool isAutoLogged)
		{
			ILastEditQuery query = ObjectFactory.Get<ILastEditQuery>();
			return query.GetUserNameAndTimeOfLastEditOrDeleteOfARecord(row, isAutoLogged);
		}

		#endregion

		#region IsRowAccessible

		public static bool IsDataInRowAccessible(DataRow row)
		{
			return DataUtils.IsDataInRowAccessible(row);
		}

		#endregion

		#region TopN

		public static DataRow[] TopN(DataRow[] rows, int n)
		{
			int resultLength = n;
			if (resultLength > rows.Length)
			{
				resultLength = rows.Length;
			}

			DataRow[] result = new DataRow[resultLength];

			for (int i = 0; i < resultLength; i++)
			{
				result[i] = rows[i];
			}

			return result;
		}

		#endregion

		#region RowPersistent

		public static bool ShouldRowBeSaved(DataRow row)
		{
			return DataUtils.ShouldRowBeSaved(row);
		}

		public static void SetShouldRowBeSaved(DataRow row, bool isPersistent)
		{
			object key = ZDataUtils.GetPK(row);
			Hashtable nonPersistentRowsHash = (Hashtable)row.Table.ExtendedProperties[DataUtils.NonPersistentRowsExtendedPropertyName];

			if (nonPersistentRowsHash == null)
			{
				nonPersistentRowsHash = new Hashtable();
				row.Table.ExtendedProperties[DataUtils.NonPersistentRowsExtendedPropertyName] = nonPersistentRowsHash;
			}

			if (isPersistent && nonPersistentRowsHash[key] != null)
			{
				nonPersistentRowsHash.Remove(key);
			}
			else if (!isPersistent && nonPersistentRowsHash[key] == null)
			{
				nonPersistentRowsHash[key] = true;
			}
		}

		#endregion

		#region DoesUpdateRowHavePersistentChangesFromOriginal

		public static bool DoesUpdateRowHavePersistentChangesFromOriginal(IApplicationSchemaResolver resolver, DataRow row)
		{
			if (DataUtils.DoesUpdateRowHavePersistentChangesFromOriginal(resolver, row))
			{
				return true;
			}
			foreach (DataColumn column in row.Table.Columns)
			{
				if (resolver.SchemaColumnExists(column.ColumnName, row.Table.TableName) && row is ZDataRow zDataRow && zDataRow.HasSource(column.ColumnName))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsRowValueChanged(DataRow row, DataColumn column)
		{
			return DataUtils.IsRowValueChanged(row, column) || (row is ZDataRow zDataRow && zDataRow.HasSource(column.ColumnName));
		}

		#endregion

		#region CountOfAllThatMatchQuery

		[Obsolete("Use Factory.GetDatabaseCount instead")]
		public static int GetCountOfAllThatMatchQueryInDB(Type bizOType, ZQuery query)
		{
			return new BusinessObjectFactory().GetDatabaseCount(bizOType, query);
		}

		#endregion

		#region Get TableName from DB + TableName

		public static ZString GetTableNameFromDbAndTableName(ZString fullyQualifiedTableName)
		{
			return DataUtils.GetTableNameFromDbAndTableName(fullyQualifiedTableName);
		}

		#endregion

		#region GetColumnSourceValueIfSet

		public static object GetSourceValueIfSet(DataRow row, object value, SchemaColumn column)
		{
			if (LazyLoading.LoadRequired(value) && row is ZDataRow zDataRow)
			{
				object source;
				if (column.IsBinary)
				{
					source = zDataRow.GetStreamSource(column.Name);
				}
				else
				{
					source = zDataRow.GetReaderSource(column.Name);
				}
				if (source != null)
				{
					value = source;
				}
			}
			return value;
		}

		#endregion
	}
}
