using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.BuildTools;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.DB.Helpers;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoPropertyList : AutoCode
	{
		public AutoPropertyList(BusinessObjectInfo info)
		{
			this.Info = info;
		}

		#region Code for PK

		public string CodeForPK
		{
			get { return AutoPK.Code; }
		}

		#endregion

		#region Code for Default Values

		public string CodeForDefaultValues
		{
			get
			{
				ArrayList text = new ArrayList();

				foreach (AutoProperty property in Properties)
				{
					if (property.IsComputed)
					{
						continue;
					}
					text.Add(property.CodeForDefaultValue);
				}

				return NewLineSeparatedText(text);
			}
		}

		#endregion

		#region Code for calling each property's validate

		public string CodeForCallingEachPropertyValidate(int tabsToPrefixEachLine)
		{
			ArrayList text = new ArrayList();
			string spacing = new string('	', tabsToPrefixEachLine);

			foreach (AutoProperty property in Properties)
			{
				if (property.GenerateValidation)
				{
					text.Add(spacing + property.CodeForCallingValidateMethod);
				}
			}

			return NewLineSeparatedText(text);
		}

		public string CodeForCallingEachPropertyValidateOnValidationObject(int tabsToPrefixEachLine)
		{
			ArrayList text = new ArrayList();
			string spacing = new string('	', tabsToPrefixEachLine);

			foreach (AutoProperty property in Properties)
			{
				if (property.GenerateValidation)
				{
					text.Add(spacing + property.CodeForCallingValidateMethodOnValidateObject);
				}
			}

			return NewLineSeparatedText(text);
		}

		#endregion

		#region Code for Properties

		public string CodeForProperties
		{
			get
			{
				ArrayList text = new ArrayList();

				foreach (AutoProperty property in Properties)
				{
					if (property.IsComputed)
					{
						continue;
					}
					text.Add(property.Code);
				}

				return NewLineSeparatedText(text);
			}
		}

		#endregion

		#region Longest Column-Name Length

		/// <summary>
		/// The length of the longest column name.
		/// </summary>
		protected int LongestColumnNameLength
		{
			get
			{
				if (fLongestColumnNameLength == -1)
				{
					int result = 0;
					foreach (DataColumn column in Info.Table.Columns)
					{
						if (result < column.ColumnName.Length)
						{
							result = column.ColumnName.Length;
						}
					}
					fLongestColumnNameLength = result;
				}

				return fLongestColumnNameLength;
			}
		}

		int fLongestColumnNameLength = -1;

		#endregion

		#region Creating Auto-Property Objects

		public bool HasIsValidProperty
		{
			get { return hasIsValidProperty; }
		}
		bool hasIsValidProperty;

		protected virtual ArrayList CreateProperties()
		{
			var result = new ArrayList(Info.Table.Columns.Count);
			var prefix = Info.PKColumnName.Split('_')[0];
			var excludedColumns = BuildXml.Instance.ExcludedColumns.Select(columnName => columnName.StartsWith("_") ? prefix + columnName : columnName).ToHashSet();

			foreach (DataColumn column in Info.Table.Columns)
			{
				if (!excludedColumns.Contains(column.ColumnName))
				{
					if (column.ColumnName == Info.PKColumnName)
					{
						AutoPK = new AutoPropertyPK(Info, column);
					}
					else if (AutoIsValidProperty.MatchesNamingRequirements(column.ColumnName))
					{
						hasIsValidProperty = true;
						result.Add(new AutoIsValidProperty(Info, column, LongestColumnNameLength));
					}
					else if (AutoIsCancelledProperty.MatchesNamingRequirements(column.ColumnName))
					{
						result.Add(new AutoIsCancelledProperty(Info, column, LongestColumnNameLength));
					}
					else
					{
						AutoProperty property;

						if (column.DataType == typeof(decimal)) // this is true for money or decimal (though we don't need it for money!)
						{
							property = CreateDecimalProperty(column);
						}
						else if (column.DataType == typeof(DateTimeOffset))
						{
							property = CreateDateTimeOffsetProperty(column);
						}
						else
						{
							// FkBizOIsInSameAssemblyAsThisProperty(ForeignKeyTableName) TODO : Generate related bizo for bizo in same solution as us

							string foreignKeyTableName = GetForeignKeyTableName(column, Info.ForeignKeysTable);
							var entry = Info.MasterFiles[foreignKeyTableName];
							var propertyType = GetPropertyTypeFromForeignKeyTableName(foreignKeyTableName);

							if (entry != null && (!Info.IsInZArchitectureSolution || entry.LivesInZArchitecture) && !string.IsNullOrEmpty(propertyType))
							{
								property = CreateFkProperty(column, propertyType, foreignKeyTableName);
							}
							else if (IsLookupColumnFromView(column) && !string.IsNullOrEmpty(GetLookupTableName(column)))
							{
								property = CreateFkProperty(column, GetLookupTableName(column), GetLookupTableName(column));
							}
							else
							{
								property = CreateStandardProperty(column);
							}
						}
						result.Add(property);
					}
				}
			}

			SortProperties(result);

			return result;
		}

		string GetPropertyTypeFromForeignKeyTableName(string foreignKeyTableName)
		{
			if (!string.IsNullOrEmpty(foreignKeyTableName))
			{
				var objectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(TableNameHelper.GetPrefixFromTableName(foreignKeyTableName), reportUnknownPrefix: false);
				if (objectType != null)
				{
					var matchingName = GetTypeInHierarchyMatchingTableName(foreignKeyTableName, objectType);
					if (matchingName != null)
					{
						return matchingName;
					}

					if (!objectType.IsAbstract)
					{
						return objectType.Name;
					}

					var secondObjectType = objectType.BaseType;
					if (secondObjectType != null && !secondObjectType.IsAbstract)
					{
						return secondObjectType.Name;
					}
					else if (HasTypeDecider(objectType))
					{
						return objectType.Name;
					}
				}
			}
			return string.Empty;
		}

		string GetTypeInHierarchyMatchingTableName(string foreignKeyTableName, Type objectType)
		{
			if (objectType == null)
			{
				return null;
			}
			if (objectType.Name.StartsWith("Auto", StringComparison.Ordinal))
			{
				return null;
			}
			if (objectType.Name == foreignKeyTableName)
			{
				return objectType.Name;
			}
			else
			{
				return GetTypeInHierarchyMatchingTableName(foreignKeyTableName, objectType.BaseType);
			}
		}

		bool HasTypeDecider(Type t)
		{
			var typeDeciderInfo = t.GetField("TypeDecider", BindingFlags.Public | BindingFlags.Static);
			return (TypeDecider)typeDeciderInfo?.GetValue(null) != null;
		}

		#region FKs for vw_List_ views

		bool IsLookupColumnFromView(DataColumn column)
		{
			bool isView = Regex.IsMatch(Info.Table.TableName, @"^vw_list", RegexOptions.IgnoreCase);
			bool isForeignKeyLikeColumnName = Regex.IsMatch(column.ColumnName, @"^\w{2}_\w{2}(_[a-zA-Z0-9]+)?$", RegexOptions.IgnoreCase);

			return isView && isForeignKeyLikeColumnName;
		}

		/// <summary>
		/// Gets name of the table from a Lookup column name.
		/// Say, the column name in a view is is JS_OH_SomeField
		/// It should work out that it points to OrgHeader table
		/// </summary>
		/// <param name="column">Lookup column in a vw_list_view</param>
		/// <returns>Table name if found and blank if not</returns>
		string GetLookupTableName(DataColumn column)
		{
			string result = "";
			if (column.ColumnName.Length >= 5)
			{
				string tableName = GetTableNameByPKFieldName(column.ColumnName.Split('_')[1] + "_PK");
				BuildXmlBizOEntry entry = Info.MasterFiles[tableName];
				if (entry != null)
				{
					result = tableName;
				}
			}
			return result;
		}

		/// <summary>
		/// Gets table name by the name of primary key field
		/// </summary>
		/// <param name="pKFieldName">Name of the PK field</param>
		/// <returns>Table name if table exists and empty string if it doesn't</returns>
		string GetTableNameByPKFieldName(string pKFieldName)
		{
			if (TablesByPKNames.ContainsKey(pKFieldName))
			{
				return (string)TablesByPKNames[pKFieldName];
			}
			else
			{
				string tableName = "";
				string sql = string.Format(@"
					select so.name from 
					sys.objects so
					join sys.columns sc on so.object_id = sc.object_id
					where sc.name = '{0}' and so.type = 'U' and schema_id = schema_id('DBO')", pKFieldName);
				DataTable tableNames = ZArchitecture.Core.Utilities.GetDataTableFromQuery(sql);
				if (tableNames.Rows.Count == 1)
				{
					tableName = (string)tableNames.Rows[0][0];
					TablesByPKNames[pKFieldName] = tableName;
				}
				return tableName;
			}
		}

		readonly Hashtable TablesByPKNames = new Hashtable();

		#endregion

		protected void SortProperties(ArrayList list)
		{
			list.Sort();
		}

		protected virtual AutoProperty CreateDecimalProperty(DataColumn column)
		{
			int decimalPrecision = GetDecimalPrecision(column, Info.DecimalScaleTable);
			int decimalScale = GetDecimalScale(column, Info.DecimalScaleTable);

			return new AutoProperty(Info, column, LongestColumnNameLength, decimalPrecision, decimalScale);
		}
		protected virtual AutoProperty CreateDateTimeOffsetProperty(DataColumn column)
		{
			int dateTimeOffsetScale = GetDateTimeOffsetScale(column, Info.DateTimeOffsetScaleTable);

			return new AutoProperty(Info, column, LongestColumnNameLength, dateTimeOffsetScale);
		}

		protected virtual AutoPropertyFK CreateFkProperty(DataColumn column, string propertyType, string foreignKeyTableName)
		{
			AutoPropertyFK result;
			bool isMasterFileFK = Info.MasterFiles.Contains(foreignKeyTableName);

			if (propertyType == "OrgAddress" && column.DataType == typeof(Guid))
			{
				result = CreateAddressFkProperty(column, propertyType, isMasterFileFK);
			}
			else
			{
				result = CreateNonAddressFkProperty(column, propertyType, isMasterFileFK);
			}

			return result;
		}

		protected virtual AutoPropertyOrgAddressFK CreateAddressFkProperty(DataColumn column, string propertyType, bool isMasterFileFK)
		{
			return new AutoPropertyOrgAddressFK(Info, column, LongestColumnNameLength, propertyType, isMasterFileFK);
		}

		protected virtual AutoPropertyFK CreateNonAddressFkProperty(DataColumn column, string propertyType, bool isMasterFileFK)
		{
			return new AutoPropertyFK(Info, column, LongestColumnNameLength, propertyType, isMasterFileFK);
		}

		protected virtual AutoProperty CreateStandardProperty(DataColumn column)
		{
			return new AutoProperty(Info, column, LongestColumnNameLength);
		}

		#endregion

		#region Decimal Precision & Scale

		protected int GetDateTimeOffsetScale(DataColumn column, DataTable dateTimeOffsetScaleTable)
		{
			foreach (DataRow row in dateTimeOffsetScaleTable.Rows)
			{
				if ((string)row[0] == column.ColumnName)
				{
					return byte.Parse(row[1].ToString(), CultureInfo.InvariantCulture);
				}
			}

			return 0;
		}

		protected int GetDecimalPrecision(DataColumn column, DataTable decimalScaleTable)
		{
			foreach (DataRow row in decimalScaleTable.Rows)
			{
				if ((string)row[0] == column.ColumnName)
				{
					return (byte)row[1];
				}
			}

			return 0;
		}

		protected int GetDecimalScale(DataColumn column, DataTable decimalScaleTable)
		{
			foreach (DataRow row in decimalScaleTable.Rows)
			{
				if ((string)row[0] == column.ColumnName)
				{
					return byte.Parse(row[2].ToString());
				}
			}

			return 0;
		}

		#endregion

		#region Foreign Keys / Natural Foreign Keys

		public string GetForeignKeyTableName(DataColumn column, DataTable table)
		{
			string result = null;
			string columnName = column.ColumnName;

			if (IsNaturalKey(columnName))
			{
				result = NaturalKeyTableName(column.ColumnName);

				if (result == null)
				{
					result = NaturalKeyViewName(column.ColumnName);
				}
			}
			else
			{
				foreach (DataRow row in table.Rows)
				{
					if ((string)row[0] == columnName)
					{
						result = (string)row[1];
						break;
					}
				}
			}

			return result;
		}

		bool IsNaturalKey(string columnName)
		{
			return (columnName.IndexOf("_NK") != -1);
		}

		protected string NaturalKeyTableName(string columnName)
		{
			var tablePrefix = columnName.Split('_')[1];

			return TableNamesByPrefix.TryGetValue(tablePrefix, out var name) ? name : null;
		}

		static Dictionary<string, string> TableNamesByPrefix => tableNamesByPrefix ??= BuildTableNamesByPrefixDictionary();

		static Dictionary<string, string> BuildTableNamesByPrefixDictionary()
		{
			var result = new Dictionary<string, string>();

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT distinct SUBSTRING(col.name, 1, CHARINDEX('_', col.name)-1) prefix, tab.name
FROM sys.tables tab
INNER JOIN sys.columns col ON tab.object_id = col.object_id
INNER JOIN sys.schemas s ON tab.schema_id = s.schema_id
WHERE col.name LIKE '%[_]%'
AND s.name NOT IN ('{0}')
AND tab.name NOT LIKE 'Client%'", string.Join("', '", Db.SqlReservedSchemas));

			using var cmd = Db.Connection.Command(sqlText);
			using var reader = cmd.ExecuteReader();
			while (reader.Read())
			{
				var prefix = reader.GetString(0);
				var name = reader.GetString(1);
				if (!result.ContainsKey(prefix))
				{
					result.Add(prefix, name);
				}
			}

			return result;
		}

		[ThreadStatic]
		static Dictionary<string, string> tableNamesByPrefix;

		protected string NaturalKeyViewName(string columnName)
		{
			string tablePrefix = columnName.Split('_')[1];

			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
SELECT TOP 1 v.name
FROM sys.views v
INNER JOIN sys.columns col ON v.object_id = col.object_id
INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
WHERE col.name LIKE '{0}[_]%'
AND s.name NOT IN ('{1}')", tablePrefix, string.Join("', '", Db.SqlReservedSchemas));

			return CargoWise.Data.Db.Connection.ExecuteScalar(sqlText) as string;
		}

		#endregion

		public IEnumerable<AutoProperty> Properties
		{
			get
			{
				return properties ?? (properties = (AutoProperty[])CreateProperties().ToArray(typeof(AutoProperty)));
			}
		}
		AutoProperty[] properties;

		protected BusinessObjectInfo Info;
		protected internal AutoPropertyPK AutoPK;
	}
}
