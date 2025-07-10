using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DataTransfer.Native.DB.Validators;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.DB
{
	/// <summary>
	/// Column in Database, represent Column structure in Database
	/// Should be immutable after being initialized.
	/// </summary>
	public class ColumnDef : IEquatable<ColumnDef>, IColumnDef
	{
		public ColumnDef(Table table)
		{
			this.table = table;
		}

		public ColumnDef(Table table, string name, string dataType)
		{
			this.name = name;
			this.table = table;
			this.dataType = dataType;
		}

		readonly Table table;

		public string Name
		{
			get { return name ?? string.Empty; }
			internal set { name = value; }
		}
		string name;

		public string DataType
		{
			get { return dataType ?? string.Empty; }
			internal set { dataType = value; }
		}
		string dataType;

		public Table Table
		{
			get
			{
				return table;
			}
		}

		public object DefaultValue { get; internal set; }
		public bool DoesNotRequireAValue { get; internal set; }
		public bool Nullable { get; internal set; }
		public int Length { get; internal set; }
		public int Scale { get; internal set; }
		public int Precision { get; internal set; }

		#region Human Name

		// TODO change it to IPropertyDef.PropertyName
		public string HumanName
		{
			get { return humanName.IsEmpty() ? humanName = GetHumanName() : humanName; }
		}
		string humanName;

		#endregion

		#region Column Type

		public ColumnType Type
		{
			get
			{
				if (type == ColumnType.Unknown)
				{
					var helper = new ColumnTypeHelper();
					helper.ColumnDef = this;
					type = helper.GetColumnType();
				}
				return type;
			}
		}
		ColumnType type;

		#endregion

		IColumnValidator validator;
		public IColumnValidator Validator
		{
			get
			{
				if (validator == null)
				{
					validator = GetValidatorFromType();
				}

				return validator;
			}
		}

		IColumnValidator GetValidatorFromType()
		{
			IColumnValidator colValidator = null;

			switch (dataType)
			{
				case DbDataType.SmallDateTime:
					colValidator = new DateTimeColumnValidator(dataType, Nullable, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
					break;

				case DbDataType.DateTime:
					colValidator = new DateTimeColumnValidator(dataType, Nullable, DateTime.MinValue, DateTime.MaxValue);
					break;

				case DbDataType.Date:
					colValidator = new DateTimeColumnValidator(dataType, Nullable, SqlDateTime.MinValue.Value, SqlDateTime.MaxValue.Value);
					break;
			}

			return colValidator;
		}

		#region Check Data Type

		internal bool DoesDataTypeRepresentBoolean()
		{
			return XsdBooleanDataTypeHelper.XsdIsBool(this);
		}

		#endregion

		#region IEquatable

		public override bool Equals(object obj)
		{
			if (!(obj is ColumnDef))
			{
				return false;
			}
			return Equals((ColumnDef)obj);
		}

		public bool Equals(ColumnDef other)
		{
			if (other == null)
			{
				return false;
			}

			if (other.Name != Name || other.DataType != DataType || other.Table.Name != Table.Name)
			{
				return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			int hashName = Name == null ? 0 : Name.GetHashCode();
			int hashDataType = DataType == null ? 0 : DataType.GetHashCode();
			int hashOwnerTable = Table == null ? 0 : Table.GetHashCode();

			return hashName ^ hashDataType ^ hashOwnerTable;
		}

		#endregion

		#region Formating

		public override string ToString()
		{
			return string.Format("{0} : {1}", Table.Name, name);
		}

		#endregion

		#region Implementation

		#region Human Name && Prefix

		internal string GetHumanName()
		{
			string result = Name;

			if (Type == ColumnType.NaturalKey)
			{
				result = result.Replace("_NK", "_");
			}

			result = ColumnNameHelper.RemovePrefix(result);

			if (Type == ColumnType.NaturalKey || Type == ColumnType.ForeignKey)
			{
				string temp = TableNameHelper.GetTableNameFromPrefix(result);
				result = temp.IsEmpty() ? result : temp;
			}

			result = ColumnNameHelper.TransformNumberToString(result);

			return result;
		}

		#endregion

		#endregion

	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
	public static class BoolDefaultValue
	{
		public const string True = "('Y')";
		public const string False = "('N')";
	}

	public enum ColumnType
	{
		Unknown,
		NormalColumn,
		TableCode,
		TableName,
		NaturalKey,
		PrimaryKey,
		ForeignKey,
		PolymorphicKey
	}

	public static class DbDataType
	{
		#region SuppressResourceStringsCheckRegion
		public const string Unknown = "Unknown";
		public const string Bit = "bit";
		public const string Char = "char";
		public const string Text = "text";
		public const string Money = "money";
		public const string Decimal = "decimal";
		public const string Short = "tinyint";
		public const string Integer = "int";
		public const string Date = "date";
		public const string DateTime = "datetime";
		public const string SmallDateTime = "smalldatetime";
		public const string DateTimeOffset = "datetimeoffset";
		public const string Geography = "geography";
		public const string UniqueIdentifier = "uniqueidentifier";
		public const string Image = "image";
		public const string Binary = "binary";
		public const string VarBinary = "varbinary";
		public const string Xml = "xml";
		public const string Time = "time";
		#endregion
	}
}
