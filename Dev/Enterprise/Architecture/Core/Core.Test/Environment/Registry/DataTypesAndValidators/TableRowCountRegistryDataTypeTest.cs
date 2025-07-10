using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(TableRowCountRegistryDataType))]
	sealed class TableRowCountRegistryDataTypeTest : RegistryDataTypeTestCase<TableRowCountRegistryDataType>
	{
		public void TestSerialization()
		{
			var dataBefore = new TableRowCountDataType();
			dataBefore.Dict = new Dictionary<string, long>();
			dataBefore.Dict.Add("table1", 10);
			dataBefore.Dict.Add("table2", 20);
			dataBefore.Dict.Add("table3", 30);

			var registryDataType = new TableRowCountRegistryDataType();
			var dataDuring = registryDataType.Serialise(dataBefore);
			var dataAfter = registryDataType.Deserialise(dataDuring);

			AssertEquals(dataBefore.UtcTime.ToString(TableRowCountRegistryDataType.TimeFormat), dataAfter.UtcTime.ToString(TableRowCountRegistryDataType.TimeFormat));
			AssertEquals(3, dataAfter.Dict.Count);
			AssertEquals(10, dataAfter.Dict["table1"]);
			AssertEquals(20, dataAfter.Dict["table2"]);
			AssertEquals(30, dataAfter.Dict["table3"]);
		}

		protected override TableRowCountRegistryDataType GetNewDataType()
		{
			return new TableRowCountRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsData = (TableRowCountDataType)lhs;
			var rhsData = (TableRowCountDataType)rhs;

			AssertEquals(message, lhsData.UtcTime.ToString(TableRowCountRegistryDataType.TimeFormat), rhsData.UtcTime.ToString(TableRowCountRegistryDataType.TimeFormat));
			AssertEquals(message, lhsData.Dict.Count, rhsData.Dict.Count);

			foreach (var key in lhsData.Dict.Keys)
			{
				AssertEquals(message, lhsData.Dict[key], rhsData.Dict[key]);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var data1 = new TableRowCountDataType();
			data1.Dict = new Dictionary<string, long>();
			data1.Dict.Add("table1", 10);
			data1.UtcTime = new DateTime(2015, 11, 3, 10, 13, 00, DateTimeKind.Utc);

			var data2 = new TableRowCountDataType();
			data2.Dict = new Dictionary<string, long>();
			data2.Dict.Add("table1", 10);
			data2.Dict.Add("table2", 20);
			data2.Dict.Add("table3", 30);
			data2.UtcTime = new DateTime(2016, 11, 3, 10, 13, 00, DateTimeKind.Utc);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(data1, new byte[]
				{
					50,48,49,53,45,49,49,45,48,51,32,49,48,58,49,51,58,48,48,116,97,98,108,101,49,61,49,48
				}),
				new ValidSampleAndBinaryValueInDB(data2, new byte[]
				{
					50,48,49,54,45,49,49,45,48,51,32,49,48,58,49,51,58,48,48,116,97,98,108,101,49,61,49,48,59,116,97,98,108,101,51,61,51,48,59,116,97,98,108,101,50,61,50,48
				})
			};
		}
	}
}
