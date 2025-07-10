using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Bi.Development.Common.SQL;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CargoWise.Bi.Development.SchemaSync.Parser;

#pragma warning disable CW1161 // Res.GetString Analyzer

public class DataTypeInfo
{
	public string Name { get; private set; }
	public short MaxLength { get; private set; }
	public byte Precision { get; private set; }
	public byte Scale { get; private set; }

	public static DataTypeInfo Create(DataTypeReference dataType)
	{
		var typeName = GetTypeName(dataType);

		if (SQLConstants.ExcludedTypes.Contains(typeName))
		{
			return null;
		}

		// ScriptDom considers geography a user defined type
		if (typeName == "geography")
		{
			return new DataTypeInfo()
			{
				Name = typeName,
				MaxLength = -1,
				Precision = 0,
				Scale = 0
			};
		}

		// ScriptDom has XML as a seperate class
		if (typeName == "xml")
		{
			return new DataTypeInfo()
			{
				Name = typeName,
				MaxLength = -1,
				Precision = 0,
				Scale = 0
			};
		}

		if (!(dataType is SqlDataTypeReference sqlDataType))
		{
			throw new NotImplementedException("There is no implementation for custom data types yet");
		}

		switch (sqlDataType.SqlDataTypeOption)
		{
			case SqlDataTypeOption.DateTimeOffset:
				return CreateForDateTimeOffset(sqlDataType);
			case SqlDataTypeOption.Decimal:
			case SqlDataTypeOption.Numeric:
				return CreateForDecimal(sqlDataType);
			default:
				break;
		}

		var parameter = sqlDataType.Parameters.FirstOrDefault();
		if (parameter != null)
		{
			if (parameter is MaxLiteral)
			{
				return new DataTypeInfo()
				{
					Name = typeName,
					MaxLength = -1,
					Precision = 0,
					Scale = 0
				};
			}

			if (parameter is IntegerLiteral integerLiteral)
			{
				return new DataTypeInfo()
				{
					Name = typeName,
					MaxLength = short.Parse(integerLiteral.Value),
					Precision = 0,
					Scale = 0
				};
			}

			throw new NotSupportedException($"Parameter type: {parameter.GetType()} is not supported.");
		}

		return new DataTypeInfo()
		{
			Name = typeName,
			MaxLength = GetDataTypeMaxLength(sqlDataType),
			Precision = GetDataTypePrecision(sqlDataType),
			Scale = GetDataTypeScale(sqlDataType),
		};
	}

	static byte GetDataTypeScale(SqlDataTypeReference dataType)
	{
		switch (dataType.SqlDataTypeOption)
		{
			case SqlDataTypeOption.DateTime:
				return 3;
			case SqlDataTypeOption.DateTime2:
				return 7;
			case SqlDataTypeOption.Money:
				return 4;
			default:
				return 0;
		}
	}

	static byte GetDataTypePrecision(SqlDataTypeReference dataType)
	{
		switch (dataType.SqlDataTypeOption)
		{
			case SqlDataTypeOption.SmallDateTime:
				return 16;
			case SqlDataTypeOption.DateTime:
				return 23;
			case SqlDataTypeOption.DateTime2:
				return 27;
			case SqlDataTypeOption.Date:
			case SqlDataTypeOption.Int:
				return 10;
			case SqlDataTypeOption.BigInt:
			case SqlDataTypeOption.Money:
				return 19;
			case SqlDataTypeOption.TinyInt:
				return 3;
			case SqlDataTypeOption.SmallInt:
				return 5;
			case SqlDataTypeOption.Bit:
				return 1;
			default:
				return 0;
		}
	}

	static short GetDataTypeMaxLength(SqlDataTypeReference dataType)
	{
		switch (dataType.SqlDataTypeOption)
		{
			case SqlDataTypeOption.UniqueIdentifier:
				return 16;
			case SqlDataTypeOption.DateTime:
			case SqlDataTypeOption.DateTime2:
				return 8;
			case SqlDataTypeOption.SmallDateTime:
			case SqlDataTypeOption.Int:
				return 4;
			case SqlDataTypeOption.Date:
				return 3;
			case SqlDataTypeOption.Char:
				return 3;
			case SqlDataTypeOption.VarChar:
				return 3;
			case SqlDataTypeOption.BigInt:
				return 8;
			case SqlDataTypeOption.SmallInt:
				return 2;
			case SqlDataTypeOption.TinyInt:
				return 1;
			case SqlDataTypeOption.Money:
				return 8;
			case SqlDataTypeOption.Bit:
				return 1;
			default:
				return 0;
		}
	}

	static short GetMaxLengthForDecimal(byte precision)
	{
		if (precision <= 9)
		{
			return 5;
		}

		if (precision <= 19)
		{
			return 9;
		}

		if (precision <= 28)
		{
			return 13;
		}

		if (precision <= 38)
		{
			return 17;
		}

		throw new IndexOutOfRangeException($"Precision '{precision}' is out of range for decimal type");
	}

	static DataTypeInfo CreateForDecimal(SqlDataTypeReference dataType)
	{
		var precisionParameter = dataType.Parameters.FirstOrDefault();
		var scaleParameter = dataType.Parameters.Skip(1).FirstOrDefault();

		var precision = precisionParameter == null ? (byte)18 : byte.Parse(precisionParameter.Value);
		var scale = scaleParameter == null ? (byte)0 : byte.Parse(scaleParameter.Value);

		return new DataTypeInfo()
		{
			Name = "decimal",
			MaxLength = GetMaxLengthForDecimal(precision),
			Precision = precision,
			Scale = scale
		};
	}

	static (byte precision, byte scale, short columnLength) GetDateTimeOffsetMetadata(short fractionalSecondsPrecision)
	{
		switch (fractionalSecondsPrecision)
		{
			case 0:
				return (26, 0, 8);
			case 1:
				return (28, 1, 8);
			case 2:
				return (29, 2, 8);
			case 3:
				return (30, 3, 9);
			case 4:
				return (31, 4, 9);
			case 5:
				return (32, 5, 10);
			case 6:
				return (33, 6, 10);
			case 7:
				return (34, 7, 10);
			default:
				throw new ArgumentOutOfRangeException(nameof(fractionalSecondsPrecision), fractionalSecondsPrecision, "DateTimeOffset precision should be 0-7");
		}
	}

	static DataTypeInfo CreateForDateTimeOffset(SqlDataTypeReference dataType)
	{
		var precisionParameter = dataType.Parameters.FirstOrDefault();
		var offsetPrecision = precisionParameter == null ? (short)7 : short.Parse(precisionParameter.Value);

		var (precision, scale, columnLength) = GetDateTimeOffsetMetadata(offsetPrecision);

		return new DataTypeInfo()
		{
			Name = "datetimeoffset",
			MaxLength = columnLength,
			Precision = precision,
			Scale = scale,
		};
	}

	static string GetTypeName(DataTypeReference dataType)
	{
		var normalisedTypeName = dataType.Name.BaseIdentifier.Value.ToLower();

		foreach (var (sourceType, destType) in typeMappings)
		{
			if (normalisedTypeName == sourceType)
			{
				return destType;
			}
		}

		return normalisedTypeName;
	}

	readonly static ImmutableArray<(string sourceType, string destType)> typeMappings = ImmutableArray.Create(
		("rowversion", "timestamp"),
		("geometry", "geography")
	);
}

#pragma warning restore CW1161 // Res.GetString Analyzer
