namespace Enterprise.DbUpgrader.Schema
{
	using System;
	using System.Data;
	using System.Text;

	public class ColumnChangeMetadata
	{
		public ColumnChangeMetadata(DataRow row)
		{
			tableSchema = row["TabSchema"].ToString().Trim();
			tableName = row["TabName"].ToString().Trim();
			columnName = row["ColName"].ToString().Trim();

			columnIdObj = (row.Table.Columns.Contains(ColIdDataColumn)) ? row[ColIdDataColumn] : null;

			var dataTypeDesc = (row.Table.Columns.Contains(ColTypeDataColumn)) ? row[ColTypeDataColumn].ToString().Trim() : null;
			dataType = new DataTypeInfo(dataTypeDesc);
			nullClause = (row.Table.Columns.Contains(NullableDataColumn)) ? row[NullableDataColumn].ToString().Trim() : null;
			isComputedColumnChange = row.Table.Columns.Contains(ComputedColumnDataColumn) && row[ComputedColumnDataColumn] != DBNull.Value && Convert.ToBoolean(row[ComputedColumnDataColumn]);
			isPersistedComputedColumn = row.Table.Columns.Contains(PersistedComputedDataColumn) && row[PersistedComputedDataColumn] != DBNull.Value && Convert.ToBoolean(row[PersistedComputedDataColumn]);
			IsSparse = row.Table.Columns.Contains(SparseDataColum) && row[SparseDataColum] != DBNull.Value && Convert.ToBoolean(row[SparseDataColum]);
			OldDefaultName = row.Table.Columns.Contains(OldDefNameColumn) && row[OldDefNameColumn] != DBNull.Value ? (string)row[OldDefNameColumn] : "";

			lengthObj = (row.Table.Columns.Contains(LenghDataColumn)) ? row[LenghDataColumn] : null;
			precisionObj = (row.Table.Columns.Contains(PrecisionDataColumn)) ? row[PrecisionDataColumn] : null;
			scaleObj = (row.Table.Columns.Contains(ScaleDataColumn)) ? row[ScaleDataColumn] : null;
			defaultObj = (row.Table.Columns.Contains(DefaultDataColumn)) ? row[DefaultDataColumn] : null;
			xmlSchemaObj = (row.Table.Columns.Contains(XmlSchemaDataColumn)) ? row[XmlSchemaDataColumn] : null;
			identitySeedObj = (row.Table.Columns.Contains(IdentitySeedDataColumn)) ? row[IdentitySeedDataColumn] : null;
			identityIncrementObj = (row.Table.Columns.Contains(IdentityIncrementDataColumn)) ? row[IdentityIncrementDataColumn] : null;
			identityChangedObj = (row.Table.Columns.Contains(IdentityChangedDataColumn)) ? row[IdentityChangedDataColumn] : null;
			computedDefinitionObj = (row.Table.Columns.Contains(ComputedDefinitionDataColumn)) ? row[ComputedDefinitionDataColumn] : null;

			if (row.Table.Columns.Contains(OldColTypeDataColumn))
			{
				oldDataType = new DataTypeInfo(row["OldType"].ToString().Trim());
				oldPrecisionObj = row["OldPrecision"];
				oldLengthObj = row["OldLength"];
			}

			columnExtendedProperty = (row.Table.Columns.Contains(ColExtendedPropertyDataColumn)) ? row[ColExtendedPropertyDataColumn] : null;
			tableExtendedProperty = (row.Table.Columns.Contains(TabExtendedPropertyDataColumn)) ? row[TabExtendedPropertyDataColumn] : null;
		}

		const string ColIdDataColumn = "ColId";
		const string ColTypeDataColumn = "ColType";
		const string NullableDataColumn = "ColNullOrNotNull";
		const string LenghDataColumn = "ColLength";
		const string PrecisionDataColumn = "ColPrecision";
		const string ScaleDataColumn = "ColScale";
		const string DefaultDataColumn = "ColDefault";
		const string XmlSchemaDataColumn = "ColXmlSchema";
		const string OldColTypeDataColumn = "OldType";
		const string IdentitySeedDataColumn = "IdentitySeed";
		const string IdentityIncrementDataColumn = "IdentityIncrement";
		const string IdentityChangedDataColumn = "IdentityChanged";
		const string ComputedColumnDataColumn = "IsComputedColumnChange";
		const string ComputedDefinitionDataColumn = "ComputedColumnDefinition";
		const string PersistedComputedDataColumn = "IsPersistedComputedColumn";
		const string ColExtendedPropertyDataColumn = "ColExtProp";
		const string TabExtendedPropertyDataColumn = "TabExtProp";
		const string SparseDataColum = "ColSparse";
		const string OldDefNameColumn = "OldDefName";

		public string TableName
		{
			get { return tableName; }
		}
		readonly string tableName;

		public string TableSchema
		{
			get { return tableSchema; }
		}
		readonly string tableSchema;

		public string ColumnName
		{
			get { return columnName; }
		}
		readonly string columnName;

		public string GetColumnId()
		{
			return (columnIdObj == null || columnIdObj == DBNull.Value) ? null : columnIdObj.ToString();
		}
		readonly object columnIdObj;

		public bool IsNullable
		{
			get { return !(String.Compare(nullClause, NotNull, StringComparison.OrdinalIgnoreCase) == 0); }
		}
		readonly string nullClause;
		const string NotNull = "NOT NULL";

		public bool IsSparse { get; }
		const string Sparse = "SPARSE";

		public string OldDefaultName { get; }

		public string GetDefaultClause()
		{
			return (defaultObj == null || defaultObj == DBNull.Value) ? null : defaultObj.ToString();
		}
		readonly object defaultObj;

		public string GetLengthClause()
		{
			return (lengthObj == null || lengthObj == DBNull.Value) ? null : lengthObj.ToString();
		}
		readonly object lengthObj;

		public DataTypeInfo DataType
		{
			get { return dataType; }
		}
		readonly DataTypeInfo dataType;

		public DataTypeInfo OldDataType
		{
			get { return oldDataType; }
		}
		readonly DataTypeInfo oldDataType;

		string GetIdentityClause()
		{
			return (identitySeedObj == null || identitySeedObj == DBNull.Value) ?
				null :
				String.Format("IDENTITY({0},{1})", identitySeedObj.ToString(), identityIncrementObj.ToString());
		}
		readonly object identitySeedObj;
		readonly object identityIncrementObj;

		public bool IdentityChanged
		{
			get { return (identityChangedObj is bool) && (bool)identityChangedObj; }
		}
		readonly object identityChangedObj;

		public bool IsComputedColumnChange
		{
			get { return isComputedColumnChange; }
		}
		readonly bool isComputedColumnChange;

		public string GetComputedColumnDefinition()
		{
			var result = new StringBuilder();

			if (computedDefinitionObj != null && computedDefinitionObj != DBNull.Value)
			{
				result.Append(computedDefinitionObj.ToString());

				if (isPersistedComputedColumn)
				{
					result.Append(" PERSISTED ");

					if (!IsNullable)
					{
						result.Append(NotNull);
					}
				}
			}

			return result.ToString();
		}
		readonly object computedDefinitionObj;
		readonly bool isPersistedComputedColumn;

		public string GetColumnExtendedProperty()
		{
			return (columnExtendedProperty == null || columnExtendedProperty == DBNull.Value) ? null : columnExtendedProperty.ToString();
		}
		readonly object columnExtendedProperty;

		public string GetTableExtendedProperty()
		{
			return (tableExtendedProperty == null || tableExtendedProperty == DBNull.Value) ? null : tableExtendedProperty.ToString();
		}
		readonly object tableExtendedProperty;

		#region Column Declaration

		public string NameAndTypeDeclaration
		{
			get { return nameAndTypeDeclaration ?? (nameAndTypeDeclaration = GetColumnNameAndTypeDeclaration()); }
		}
		string nameAndTypeDeclaration;

		public string ColumnDeclaration => IsSparse && IsNullable
			? NameAndTypeDeclaration + " " + Sparse + " " + nullClause
			: NameAndTypeDeclaration + " " + nullClause;

		public string DefaultConstraintName
		{
			get
			{
				return (GetDefaultClause() == null)
					? String.Empty
					: CalculatedDefaultConstraintName;
			}
		}

		public string CalculatedDefaultConstraintName
		{
			get { return "DF_" + tableName + "_" + columnName; }
		}

		public string FullAddColumnDeclaration
		{
			get
			{
				if (fullAddColumnDeclaration == null)
				{
					if (IsComputedColumnChange)
					{
						fullAddColumnDeclaration = "[" + columnName + "] AS " + GetComputedColumnDefinition();
					}
					else
					{
						string tempColumnDeclaration = ColumnDeclaration;
						string defaultClause = GetDefaultClause();

						if (defaultClause == null)
						{
							string identityClause = GetIdentityClause();

							if (identityClause != null)
							{
								tempColumnDeclaration += " " + identityClause;
							}
						}
						else
						{
							tempColumnDeclaration += " CONSTRAINT [" + CalculatedDefaultConstraintName + "] DEFAULT " + defaultClause;
						}

						fullAddColumnDeclaration = tempColumnDeclaration;
					}
				}

				return fullAddColumnDeclaration;
			}
		}
		string fullAddColumnDeclaration;

		string GetColumnNameAndTypeDeclaration()
		{
			return String.Format("[{0}] {1}", columnName, GetFullTypeDeclaration());
		}

		public string GetFullTypeDeclaration()
		{
			string result = "";

			if (DataType.IsTypeWithLength)
			{
				// Column Length
				string columnLength = lengthObj.ToString();
				int value;
				if (Int32.TryParse(columnLength, out value) && value == -1)
				{
					columnLength = "max";
				}

				result = String.Format("({0})", columnLength);
			}
			else if (DataType.IsDecimalType)
			{
				// Column Precision and Scale
				result = String.Format("({0}, {1})", precisionObj.ToString(), scaleObj.ToString());
			}
			else if (DataType.IsDateTimeOffsetType)
			{
				result = !scaleObj.Equals(DBNull.Value) ? $"({scaleObj})" : string.Empty;
			}
			else if (DataType.SqlType == SqlDbType.Xml)
			{
				// Column XML Schema
				if (xmlSchemaObj != null && xmlSchemaObj != DBNull.Value)
				{
					result = String.Format("(CONTENT {0})", xmlSchemaObj.ToString());
				}
			}

			return String.Format("{0}{1}", DataType.Description, result);
		}

		#endregion

		public bool IsNewLengthLessThanOld()
		{
			int oldColumnLength = (oldLengthObj == null || oldLengthObj == DBNull.Value) ? -1 : Convert.ToInt32(oldLengthObj);
			int newColumnLength = (lengthObj == null || lengthObj == DBNull.Value) ? -1 : Convert.ToInt32(lengthObj);

			return (newColumnLength > 0 && (oldColumnLength < 0 || oldColumnLength > newColumnLength));
		}

		public bool IsNewPrecisionLessThanOld()
		{
			int oldPrecisionValue = (oldPrecisionObj == null || oldPrecisionObj == DBNull.Value) ? -1 : Convert.ToInt32(oldPrecisionObj);
			int newPrecisionValue = (precisionObj == null || precisionObj == DBNull.Value) ? -1 : Convert.ToInt32(precisionObj);

			return (oldPrecisionValue > 0 && newPrecisionValue > 0 && oldPrecisionValue > newPrecisionValue);
		}

		readonly object precisionObj;
		readonly object scaleObj;
		readonly object xmlSchemaObj;

		// Old Column Metadata - if change = alter column
		readonly object oldPrecisionObj;

		public string GetOldLengthClause()
		{
			return (oldLengthObj == null || oldLengthObj == DBNull.Value) ? null : oldLengthObj.ToString();
		}
		readonly object oldLengthObj;
	}

	public class DataTypeInfo
	{
		public DataTypeInfo(string dataTypeName)
		{
			this.description = dataTypeName;
			sqlType = GetSqlType(dataTypeName);
		}

		public string Description
		{
			get { return description; }
		}
		readonly string description;

		public SqlDbType SqlType
		{
			get { return sqlType; }
		}
		readonly SqlDbType sqlType;

		public bool IsCharType
		{
			get
			{
				return (
					sqlType == SqlDbType.Char
					|| sqlType == SqlDbType.VarChar
					|| sqlType == SqlDbType.NChar
					|| sqlType == SqlDbType.NVarChar);
			}
		}

		public bool IsCharOrTextType
		{
			get
			{
				return (
				IsCharType
				|| sqlType == SqlDbType.Text
				|| sqlType == SqlDbType.NText);
			}
		}

		public bool IsBinaryOrImageType
		{
			get
			{
				return (
				sqlType == SqlDbType.Binary
				|| sqlType == SqlDbType.VarBinary
				|| sqlType == SqlDbType.Image);
			}
		}

		public bool IsIntegerType
		{
			get
			{
				return (
				sqlType == SqlDbType.TinyInt
				|| sqlType == SqlDbType.SmallInt
				|| sqlType == SqlDbType.Int
				|| sqlType == SqlDbType.BigInt);
			}
		}

		public bool IsDecimalType
		{
			get
			{
				return (sqlType == SqlDbType.Decimal);
			}
		}

		public bool IsIntergerOrDecimalType
		{
			get
			{
				return (
				IsIntegerType
				|| IsDecimalType);
			}
		}

		public bool IsDateTimeType
		{
			get
			{
				return
					sqlType == SqlDbType.Date
					|| sqlType == SqlDbType.DateTime
					|| sqlType == SqlDbType.SmallDateTime;
			}
		}

		public bool IsDateTimeOffsetType
		{
			get
			{
				return
					sqlType == SqlDbType.DateTimeOffset;
			}
		}

		public bool IsTypeWithLength
		{
			get
			{
				return (
				IsCharType
				|| sqlType == SqlDbType.Binary
				|| sqlType == SqlDbType.VarBinary);
			}
		}

		SqlDbType GetSqlType(string dataTypeDesc)
		{
			SqlDbType result = SqlDbType.Udt;

			if (!String.IsNullOrEmpty(dataTypeDesc))
			{
				if (dataTypeDesc.Equals("numeric", StringComparison.OrdinalIgnoreCase))
				{
					result = SqlDbType.Decimal;
				}
				else
				{
					foreach (SqlDbType sqlType in Enum.GetValues(typeof(SqlDbType)))
					{
						if (String.Compare(dataTypeDesc, Enum.GetName(typeof(SqlDbType), sqlType), StringComparison.OrdinalIgnoreCase) == 0)
						{
							result = sqlType;
							break;
						}
					}
				}
			}

			return result;
		}
	}
}
