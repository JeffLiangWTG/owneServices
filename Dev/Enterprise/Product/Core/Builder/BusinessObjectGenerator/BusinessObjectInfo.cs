using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.BuildTools;
using CargoWise.Data;

namespace Enterprise.BusinessObjectGenerator
{
	public class BusinessObjectInfo
	{
		public BusinessObjectInfo(
			string fileName,
			bool isInZArchitectureSolution,
			bool isPersistent,
			string @namespace,
			string namespaceOfSchema,
			string baseClassName,
			string sqlSchemaName,
			DataTable table,
			DataTable decimalScaleTable,
			DataTable foreignKeysTable,
			DataTable dateTimeOffsetTable,
			string refDbCountry,
			RefDbTypeEnum? refDbType,
			Dictionary<string, string> dbTypes,
			HashSet<string> uniqueKeys,
			HashSet<string> canForceUpdateNaturalKeyCacheColumns,
			BuildXmlBizOEntryCollection masterFiles,
			string pkIndex,
			string[] indexes,
			bool masterFileReference,
			bool preventDelete,
			string[] literalOnlyColumns,
			IEnumerable<string> nonBlankFilteredIndexColumns,
			Dictionary<string, ITableInfo> tables,
			bool convertZStringToWesternEuropeanCharacters = false)
		{
			this.FileName = fileName;
			this.IsInZArchitectureSolution = isInZArchitectureSolution;
			this.IsPersistent = isPersistent;
			this.Namespace = @namespace;
			this.NamespaceOfSchema = namespaceOfSchema;
			this.BaseClassName = baseClassName;
			this.SqlSchemaName = (string.IsNullOrWhiteSpace(sqlSchemaName)) ? Db.SqlDbOwnerSchema : sqlSchemaName;
			this.Table = table;
			this.DecimalScaleTable = decimalScaleTable;
			this.ForeignKeysTable = foreignKeysTable;
			this.DateTimeOffsetScaleTable = dateTimeOffsetTable;
			this.RefDbCountry = refDbCountry;
			this.RefDbType = refDbType;
			this.DbTypes = dbTypes;
			this.UniqueKeys = uniqueKeys;
			this.CanForceUpdateNaturalKeyCacheColumns = canForceUpdateNaturalKeyCacheColumns;
			this.MasterFiles = masterFiles;
			this.PkIndex = pkIndex;
			this.Indexes = indexes;
			this.MasterFileReference = masterFileReference;
			this.PreventDelete = preventDelete;
			this.LiteralOnlyColumns = literalOnlyColumns;
			this.NonBlankFilteredIndexColumns = nonBlankFilteredIndexColumns;
			this.Tables = tables;
			this.ConvertZStringToWesternEuropeanCharacters = convertZStringToWesternEuropeanCharacters;
		}

		public readonly string FileName;
		public readonly bool IsInZArchitectureSolution;
		public readonly bool IsPersistent;
		public readonly string Namespace;
		public readonly string NamespaceOfSchema;
		public readonly string BaseClassName;
		public readonly string SqlSchemaName;
		public readonly DataTable Table;
		public readonly DataTable DecimalScaleTable;
		public readonly DataTable ForeignKeysTable;
		public readonly DataTable DateTimeOffsetScaleTable;
		public readonly string RefDbCountry;
		public readonly RefDbTypeEnum? RefDbType;
		public readonly BuildXmlBizOEntryCollection MasterFiles;
		internal readonly Dictionary<string, string> DbTypes;
		internal readonly HashSet<string> UniqueKeys;

		internal HashSet<string> CanForceUpdateNaturalKeyCacheColumns { get; private set; }

		public readonly string PkIndex;
		public readonly string[] Indexes;
		public readonly bool MasterFileReference;
		public readonly bool PreventDelete;
		public readonly string[] LiteralOnlyColumns;
		public readonly IEnumerable<string> NonBlankFilteredIndexColumns;
		public readonly Dictionary<string, ITableInfo> Tables;
		public readonly bool ConvertZStringToWesternEuropeanCharacters;

		#region Class Names

		public BusinessObjectClassNames ClassNames
		{
			get
			{
				if (fClassNames == null)
				{
					fClassNames = new BusinessObjectClassNames(Table.TableName);
				}
				return fClassNames;
			}
		}

		BusinessObjectClassNames fClassNames;

		#endregion

		#region PK Column Name

		public string PKColumnName
		{
			get { return PrimaryKeyColumnName(Table); }
		}

		/// <summary>
		/// The name of the primary key column.
		/// </summary>
		protected string PrimaryKeyColumnName(DataTable table)
		{
			if (table.PrimaryKey.Length == 0)
			{
				var columnName = table.Columns[0].ColumnName;
				if (!(columnName.EndsWith("_PK") && (columnName.Length == 5 || columnName.Length == 6)))
				{
					throw new ArgumentException("Table \"" + table.TableName + "\" has no primary key.");
				}
			}

			return table.Columns[0].ColumnName;
		}

		#endregion

		public string TableName
		{
			get
			{
				return (RefDbType == null) ?
					Table.TableName :
					RefDbTableNameResolver.GetRefDbTableSynonym(RefDbType.Value, RefDbCountry, Table.TableName);
			}
		}
	}

	#region BusinessObjectClassNames class

	public class BusinessObjectClassNames
	{
		public BusinessObjectClassNames(string tableName)
		{
			fBusinessObject = tableName;
		}

		public string BusinessObject
		{
			get { return fBusinessObject; }
		}

		public string BusinessObjectAuto
		{
			get { return "Auto" + BusinessObject; }
		}

		public string Schema
		{
			get { return BusinessObject + "Schema"; }
		}

		public string Lookups
		{
			get { return BusinessObject + "Lookups"; }
		}

		public string LookupsAuto
		{
			get { return BusinessObjectAuto + "Lookups"; }
		}

		public string Validation
		{
			get { return BusinessObject + "Validation"; }
		}

		public string ValidationAuto
		{
			get { return BusinessObjectAuto + "Validation"; }
		}

		readonly string fBusinessObject;
	}

	#endregion
}
