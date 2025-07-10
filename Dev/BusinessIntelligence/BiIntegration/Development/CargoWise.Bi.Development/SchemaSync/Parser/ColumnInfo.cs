using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Development.Common.SQL;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CargoWise.Bi.Development.SchemaSync.Parser;

public class ColumnInfo
{
	public string Name { get; private set; }
	public DataTypeInfo DataTypeInfo { get; private set; }
	public bool Nullable { get; private set; }
	public bool IsPrimaryKey { get; private set; }

	public static ColumnInfo Create(ColumnDefinition column, List<string> pkColumns)
	{
		// Skip computed columns
		if (column.ComputedColumnExpression != null)
		{
			return null;
		}

		var dataTypeInfo = DataTypeInfo.Create(column.DataType);
		// Skip excluded data types
		if (dataTypeInfo == null)
		{
			return null;
		}

		// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FBusinessIntelligence%2FBiIntegration%2FDevelopment%2FCargoWise.Bi.Development%2FCommon%2FSQL%2FMainDbSchemaQuery.sql&version=GBmaster&line=71&lineEnd=71&lineStartColumn=1&lineEndColumn=29&lineStyle=plain&_a=contents
		if (dataTypeInfo.Name == "nchar" || dataTypeInfo.Name == "nvarchar")
		{
			// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FBusinessIntelligence%2FBiIntegration%2FDevelopment%2FCargoWise.Bi.Development%2FCommon%2FSQL%2FMainDbSchemaQuery.sql&version=GBmaster&line=26&lineEnd=26&lineStartColumn=1&lineEndColumn=133&lineStyle=plain&_a=contents
			if (dataTypeInfo.MaxLength * 2 > SQLConstants.MaxColumnLength)
			{
				return null;
			}
		}
		else if (dataTypeInfo.MaxLength > SQLConstants.MaxColumnLength)
		{
			return null;
		}

		var columnName = column.ColumnIdentifier.Value;
		var isPrimaryKey = false;

		if (pkColumns.Contains(columnName))
		{
			// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FBusinessIntelligence%2FBiIntegration%2FDevelopment%2FCargoWise.Bi.Development%2FCommon%2FSQL%2FMainDbSchemaQuery.sql&version=GBmaster&line=7&lineEnd=7&lineStartColumn=3&lineEndColumn=34&lineStyle=plain&_a=contents
			pkColumns.Clear();
			isPrimaryKey = true;
		}
		else if (column.Constraints.Any(
			constraint => constraint is UniqueConstraintDefinition uniqueConstraint &&
			uniqueConstraint.IsPrimaryKey))
		{
			// the fact that SQL server allows this is disgusting
			// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FClientExtensions%2FEDI%2FZClientEDI%2FZClientEDI%2FClientConfiguration%2FEDIClientDbSchemaUpgradeInfo.cs&version=GBmaster&line=4333&lineEnd=4342&lineStartColumn=1&lineEndColumn=2&lineStyle=plain&_a=contents
			var uniqueConstraint = column.Constraints.OfType<UniqueConstraintDefinition>().Single();
			var constrainedColumn = uniqueConstraint.Columns.SingleOrDefault();
			if (constrainedColumn == null)
			{
				isPrimaryKey = true;
			}
			else if (constrainedColumn.Column.MultiPartIdentifier.Identifiers.Last().Value == columnName)
			{
				isPrimaryKey = true;
			}
		}

		var nullableConstraint = (NullableConstraintDefinition)column.Constraints.FirstOrDefault(
			constraint => constraint is NullableConstraintDefinition);
		var nullable = IsNullable(column);

		return new ColumnInfo()
		{
			Name = columnName,
			DataTypeInfo = dataTypeInfo,
			Nullable = nullable,
			IsPrimaryKey = isPrimaryKey
		};
	}

	static bool IsNullable(ColumnDefinition column)
	{
		var nullableConstraint = (NullableConstraintDefinition)column.Constraints.FirstOrDefault(
			constraint => constraint is NullableConstraintDefinition);
		if (nullableConstraint != null)
		{
			return nullableConstraint.Nullable;
		}

		// assume a default of true even though its unideal
		// sysname and timestamp are not nullable by default but these types are explicitly ignored
		// is it possible to have the value change based on ANSI_NULL_DEFAULT
		// setting but realistically we aren't changing that
		// ideally EDI scripts explicitly specify NULL or NOT NULL
		return true;
	}
}
