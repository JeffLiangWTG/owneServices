using System.Data;

namespace CargoWise.Data.SqlProxy.Interface.Test;

[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
class JsonHelperTest
{
	[TestCaseSource(typeof(JsonConverterTestCaseSources), nameof(JsonConverterTestCaseSources.PrimitiveTypesTestData))]
	public void ValuesWithPrecisionsArePreservedAfterSerializationDeserializationProcess(object? value)
	{
		var jsonString = JsonHelper.ToJson(value);
		var deserializedValue = JsonHelper.FromJson(jsonString, value?.GetType() ?? typeof(object));

		Assert.That(jsonString, Is.TypeOf<string>());
		Assert.That(deserializedValue, Is.EqualTo(value));
	}

	[Test]
	public void ObjectArraySerializationAndDeserialization()
	{
		var itemArray = JsonConverterTestCaseSources.PrimitiveTypesTestData().Cast<object>().ToArray();
		var dataRows = new List<object[]>() { itemArray };
		var dataTypes = itemArray.Select(x => x?.GetType() ?? typeof(object)).ToArray();

		// Act
		var jsonString = JsonHelper.SerializeObjectArray(dataRows);
		var deserializedItems = JsonHelper.DeserializeObjectArray(jsonString, dataTypes).ToArray();

		// Assert
		Assert.That(deserializedItems.Length, Is.EqualTo(dataRows.Count));

		for (var i = 0; i < itemArray.Length; i++)
		{
			Assert.That(deserializedItems[0].Length, Is.EqualTo(itemArray.Length));
			Assert.That(deserializedItems[0][i], Is.EqualTo(itemArray[i]), $"dataType: {dataTypes[i]} at index of: {i}");
		}
	}

	[Test]
	public void ObjectArrayStringToDateTimeDeserialization()
	{
		var jsonString = @"[
	  [
		""2023-09-10T19:07:59.1230000""
	  ]
	]";
		var types = new[] { typeof(DateTime) };

		var result = JsonHelper.DeserializeObjectArray(jsonString, types);
		if (result?.FirstOrDefault()?.FirstOrDefault() is DateTime actualDateTime)
		{
			var expectedDateTime = new DateTime(year: 2023, month: 9, day: 10, hour: 19, minute: 7, second: 59, millisecond: 123);
			Assert.That(actualDateTime, Is.EqualTo(expectedDateTime));
		}
	}

	[TestCase("A", typeof(char), 'A')]
	[TestCase(" ", typeof(char), ' ')]
	[TestCase("False", typeof(bool), false)]
	[TestCase("True", typeof(bool), true)]
	[TestCase("false", typeof(bool), false)]
	[TestCase("true", typeof(bool), true)]
	[TestCase("0", typeof(bool), false)]
	[TestCase("1", typeof(bool), true)]
	public void SpecialConversions(string? jsonString, Type targetType, object expectedValue)
	{
		Assert.That(JsonHelper.FromJson(jsonString, targetType), Is.EqualTo(expectedValue));
	}

	[TestCase("F34B1044-428F-421B-B919-F90D85B6DF4D")]
	[TestCase("{710f9a23-3e4f-438d-b1a4-fb6f6cdf5afb}")]
	public void GuidDeserialization(string? guidString)
	{
		Assert.That(JsonHelper.FromJson(guidString, typeof(Guid)), Is.TypeOf<Guid>());
	}

	[TestCase("null")]
	[TestCase("{}")]
	public void NullJsonStringToNullDeserialization(string? jsonString)
	{
		var targetTypes = JsonConverterTestCaseSources.SqlEquivalentTypes().ToArray();

		Assert.Multiple(() =>
		{
			foreach (var targetType in targetTypes)
			{
				Assert.That(
					JsonHelper.FromJson(jsonString, targetType),
					Is.EqualTo(targetType == typeof(DBNull) ? DBNull.Value : null),
					$"jsonString: {jsonString}, Target type: {targetType}, Expected Value: {null}");
			}
		});
	}

	[TestCase("null")]
	[TestCase("{}")]
	public void NullJsonStringToDbNullDeserialization(string? jsonString)
	{
		var targetTypes = JsonConverterTestCaseSources.SqlEquivalentTypes().ToArray();

		Assert.Multiple(() =>
		{
			foreach (var targetType in targetTypes)
			{
				Assert.That(
					JsonHelper.FromJson(jsonString, targetType, treatNullAsDbNull: true),
					Is.EqualTo(DBNull.Value),
					$"jsonString: {jsonString}, Target type: {targetType}, Expected Value: {DBNull.Value}");
			}
		});
	}

	[TestCaseSource(typeof(JsonConverterTestCaseSources), nameof(JsonConverterTestCaseSources.SqlEquivalentTypes))]
	public void EmptyStringDeserialization(Type targetType)
	{
		var emptyString = string.Empty;
		object? expectedValue;

		if (targetType == typeof(DBNull))
		{
			expectedValue = DBNull.Value;
		}
		else
		{
			expectedValue = targetType == typeof(string)
				? string.Empty
				: (object?)null;
		}

		Assert.That(JsonHelper.FromJson(emptyString, targetType), Is.EqualTo(expectedValue));
	}

	[Test]
	public void Epsilon()
	{
		var epsilonValue = double.Epsilon;
		Assert.That(JsonHelper.FromJson(JsonHelper.ToJson(epsilonValue), typeof(double)), Is.EqualTo(epsilonValue));
	}

	[Test]
	public void NotNumberValue()
	{
		var values = new[]
		{
			double.NegativeInfinity,
			double.PositiveInfinity,
			double.NaN,
			float.PositiveInfinity,
			float.NegativeInfinity,
			float.NaN,
		};

		foreach (var value in values)
		{
			Assert.That(JsonHelper.FromJson(JsonHelper.ToJson(value), value.GetType()), Is.EqualTo(value));
		}
	}

	[TestCaseSource(typeof(JsonConverterTestCaseSources), nameof(JsonConverterTestCaseSources.SqlDataTypes))]
	public void SqlDataTypesAreMappedToSystemDataTypes(string sqlDataType)
	{
		Assert.That(JsonHelper.ParseSqlType(sqlDataType), Is.Not.Null);
	}

	[Test]
	public void SqlDbTypesAreMappedToSystemDataTypes([Values] SqlDbType sqlDbType)
	{
		Assert.That(JsonHelper.ParseSqlType(sqlDbType.ToString()), Is.Not.Null);
	}
}
