using System.Data;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public class SerializedDataColumn
{
	public SerializedDataColumn()
	{
	}

	public SerializedDataColumn(DataColumn column)
	{
		ColumnName = column.ColumnName;
		DataTypeName = column.DataType.AssemblyQualifiedName ?? string.Empty;
		AllowDbNull = column.AllowDBNull;
		AutoIncrement = column.AutoIncrement;
		AutoIncrementSeed = column.AutoIncrementSeed;
		AutoIncrementStep = column.AutoIncrementStep;
		Unique = column.Unique;
		MaxLength = column.MaxLength;
		Ordinal = column.Ordinal;
	}

	public string ColumnName { get; set; } = string.Empty;
	public string DataTypeName { get; set; } = string.Empty;
	public bool AllowDbNull { get; set; }
	public bool AutoIncrement { get;set; }
	public long AutoIncrementSeed { get; set; }
	public long AutoIncrementStep { get; set; }
	public bool Unique { get; set; }
	public int MaxLength { get; set; }
	public int Ordinal { get; set; }

	public DataColumn ToDataColumn()
	{
		var dataColumn = new DataColumn(ColumnName, GetDataType())
		{
			AllowDBNull = AllowDbNull,
			AutoIncrement = AutoIncrement,
			AutoIncrementSeed = AutoIncrementSeed,
			AutoIncrementStep = AutoIncrementStep,
			Unique = Unique,
			MaxLength = MaxLength,
		};

		return dataColumn;
	}

	public Type GetDataType() => Type.GetType(DataTypeName) ?? typeof(string);
}
