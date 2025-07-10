using System.IO;
using System.Linq;
using CargoWise.Bi.Development.Common.SQL;
using CargoWise.Bi.Development.SchemaSync.Parser;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing.Parser;

public class DataTypeInfoTest : TestCase
{
	public void TestInfoIsNullForExcludedDataTypes()
	{
		foreach (var excludedType in SQLConstants.ExcludedTypes)
		{
			if (excludedType == "cursor")
			{
				continue;
			}

			var info = GetDataTypeInfoFromDefinition(excludedType);
			AssertEquals("We should get null for excluded types.", null, info);
		}
	}

	public void TestCorrectValuesForDateTimeOffsetDefault()
	{
		var info = GetDataTypeInfoFromDefinition("datetimeoffset");

		CombineAssertions(() =>
		{
			AssertEquals((byte)34, info.Precision);
			AssertEquals((byte)7, info.Scale);
			AssertEquals((short)10, info.MaxLength);
		});
	}

	public void TestCorrectValuesForDateTimeOffsetSpecified()
	{
		var info = GetDataTypeInfoFromDefinition("datetimeoffset(3)");

		CombineAssertions(() =>
		{
			AssertEquals((byte)30, info.Precision);
			AssertEquals((byte)3, info.Scale);
			AssertEquals((short)9, info.MaxLength);
		});
	}

	public void TestCorrectValuesForDecimalWithoutParameters()
	{
		var info = GetDataTypeInfoFromDefinition("decimal");

		CombineAssertions(() =>
		{
			AssertEquals((byte)18, info.Precision);
			AssertEquals((byte)0, info.Scale);
			AssertEquals((short)9, info.MaxLength);
		});
	}

	public void TestCorrectValuesForDecimalWithScale()
	{
		var info = GetDataTypeInfoFromDefinition("decimal(2)");

		CombineAssertions(() =>
		{
			AssertEquals((byte)2, info.Precision);
			AssertEquals((byte)0, info.Scale);
			AssertEquals((short)5, info.MaxLength);
		});
	}

	public void TestCorrectValuesForDecimalWithParameters()
	{
		var info = GetDataTypeInfoFromDefinition("decimal(20, 2)");

		CombineAssertions(() =>
		{
			AssertEquals((byte)20, info.Precision);
			AssertEquals((byte)2, info.Scale);
			AssertEquals((short)13, info.MaxLength);
		});
	}

	public void TestVarcharMaxLength()
	{
		var info = GetDataTypeInfoFromDefinition("varchar(max)");

		CombineAssertions(() =>
		{
			AssertEquals((byte)0, info.Precision);
			AssertEquals((byte)0, info.Scale);
			AssertEquals((short)-1, info.MaxLength);
		});
	}

	public void TestVarcharWithSpecifiedLength()
	{
		var info = GetDataTypeInfoFromDefinition("varchar(123)");

		CombineAssertions(() =>
		{
			AssertEquals((byte)0, info.Precision);
			AssertEquals((byte)0, info.Scale);
			AssertEquals((short)123, info.MaxLength);
		});
	}

	DataTypeInfo GetDataTypeInfoFromDefinition(string typeDefinition)
	{
		var createTableScript = $@"
		CREATE TABLE DummyTable (
			COL {typeDefinition}
		);";

		var parser = new TSql160Parser(true);
		var statementList = parser.ParseStatementList(
			new StringReader(createTableScript),
			out var errors);

		AssertEquals("There should be no parse errors.", errors.Count, 0);

		var createTableStatement = statementList.Statements.Single() as CreateTableStatement;
		var dataType = createTableStatement.Definition.ColumnDefinitions.Single().DataType;

		return DataTypeInfo.Create(dataType);
	}
}
