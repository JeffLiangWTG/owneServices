using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using WTG.Statistics;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StatisticsCacheSerializerTest : TestCase
	{
		void AssertAreEqual(SqlHistogram histogram1, SqlHistogram histogram2)
		{
			AssertEquals(histogram1.SchemaName, histogram2.SchemaName);
			AssertEquals(histogram1.TableName, histogram2.TableName);
			AssertEquals(histogram1.ColumnName, histogram2.ColumnName);

			foreach (var field in typeof(SqlHistogram).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.FieldType == typeof(string[]))
				{
					AssertArrayEqualsByElements((string[])field.GetValue(histogram1), (string[])field.GetValue(histogram2));
				}
				else if (field.FieldType == typeof(object[]))
				{
					var arr1 = (object[])field.GetValue(histogram1);
					var arr2 = (object[])field.GetValue(histogram2);
					AssertEquals(arr1.Length, arr2.Length);
					if (arr1.Length > 0)
					{
						if (arr1[0].GetType().IsArray)
						{
							for (int i = 0; i < arr1.Length; i++)
							{
								AssertArrayEqualsByElements(
									((IEnumerable)arr1[i]).OfType<object>().ToArray(),
									((IEnumerable)arr2[i]).OfType<object>().ToArray()
									);
							}
						}
						else
						{
							AssertArrayEqualsByElements((object[])field.GetValue(histogram1), (object[])field.GetValue(histogram2));
						}
					}
				}
				else
				{
					AssertEquals(field.GetValue(histogram1), field.GetValue(histogram2));
				}
			}
		}

		public void TestSerializeDeserialize_EmptyArray()
		{
			var serializer = new StatisticsCacheSerializer();
			var histogram = new SqlHistogram("Schema", "Table", "Column", Array.Empty<SqlHistogramStep>());
			var histograms = new SqlHistogram[] { histogram };

			var serialized = serializer.SerializeHistograms(histograms);

			var histograms2 = serializer.DeserializeHistograms(serialized);

			AssertAreEqual(histograms[0], histograms2[0]);
		}

		public void TestSerializeDeserialize_GuidKey()
		{
			var serializer = new StatisticsCacheSerializer();
			var step1 = new SqlHistogramStep(Guid.Parse("AAAAAAAA-0000-0000-0000-000000000000"), 100, 10000);
			var step2 = new SqlHistogramStep(Guid.Parse("BBBBBBBB-0000-0000-0000-000000000000"), 100, 10000);
			var step3 = new SqlHistogramStep(Guid.Parse("CCCCCCCC-0000-0000-0000-000000000000"), 100, 10000);
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { step1, step2, step3 });
			var histograms = new SqlHistogram[] { histogram };

			var serialized = serializer.SerializeHistograms(histograms);

			var histograms2 = serializer.DeserializeHistograms(serialized);

			AssertAreEqual(histograms[0], histograms2[0]);
		}

		public void TestSerializeDeserialize_DecimalKey()
		{
			var serializer = new StatisticsCacheSerializer();

			var values = new List<decimal>() {
				decimal.MinValue,
				decimal.MinusOne,
				decimal.One,
				decimal.MaxValue,
				(decimal)Math.PI,
				(decimal)Math.Sqrt(2)
			};

			var epsilon = decimal.MaxValue;
			var halfsilon = epsilon / 2M;
			while (halfsilon != 0M)
			{
				epsilon = halfsilon;
				values.Add(epsilon);
				halfsilon = halfsilon / 2M;
			}

			var histogram = new SqlHistogram("Schema", "Table", "Column",
				values.Select(value => new SqlHistogramStep(value, 100, 10000)).ToArray());

			var histograms = new SqlHistogram[] { histogram };

			var serialized = serializer.SerializeHistograms(histograms);
			string json = Encoding.Unicode.GetString(serialized);
			var histograms2 = serializer.DeserializeHistograms(serialized);

			AssertAreEqual(histograms[0], histograms2[0]);
		}

		public void TestSerializeDeserialize_DoubleKey()
		{
			var serializer = new StatisticsCacheSerializer();

			var values = new List<double>() {
				double.MinValue,
				double.MaxValue,
				Math.PI,
				(double)Math.Sqrt(2)
			};

			var epsilon = double.MaxValue;
			var halfsilon = epsilon / 2D;
			while (halfsilon != 0D)
			{
				epsilon = halfsilon;
				values.Add(epsilon);
				halfsilon = halfsilon / 2D;
			}

			var histogram = new SqlHistogram("Schema", "Table", "Column",
				values.Select(value => new SqlHistogramStep(value, 100, 10000)).ToArray());

			var histograms = new SqlHistogram[] { histogram };

			var serialized = serializer.SerializeHistograms(histograms);
			string json = Encoding.Unicode.GetString(serialized);
			var histograms2 = serializer.DeserializeHistograms(serialized);

			AssertAreEqual(histograms[0], histograms2[0]);
		}

		public void TestSerializeDeserialize_SqlGuidKey()
		{
			var serializer = new StatisticsCacheSerializer();
			var step1 = new SqlHistogramStep(SqlGuid.Parse("AAAAAAAA-0000-0000-0000-000000000000"), 100, 10000);
			var step2 = new SqlHistogramStep(SqlGuid.Parse("BBBBBBBB-0000-0000-0000-000000000000"), 100, 10000);
			var step3 = new SqlHistogramStep(SqlGuid.Null, 100, 10000);
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { step1, step2, step3 });
			var histograms = new SqlHistogram[] { histogram };

			var serialized = serializer.SerializeHistograms(histograms);

			var histograms2 = serializer.DeserializeHistograms(serialized);

			AssertAreEqual(histograms[0], histograms2[0]);
		}

		public void TestSerializeDeserialize_UnsupportedKeyType_ThrowException()
		{
			var serializer = new StatisticsCacheSerializer();
			var step1 = new SqlHistogramStep(new Tuple<int, int>(1, 10), 100, 10000);
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { step1 });
			var histograms = new SqlHistogram[] { histogram };

			var serialized = serializer.SerializeHistograms(histograms);

			AssertExceptionThrown<ArgumentException>("Type 'Tuple`2' is not allowed to deserialize by StatisticsCacheSerializer", () =>
			{
				var histograms2 = serializer.DeserializeHistograms(serialized);
			});
		}

		public void TestSerializeDeserialize_SupportedKeyTypes()
		{
			var supportedKeySamples = new object[][] {
			new object[] { Guid.Parse("AAAAAAAA-0000-0000-0000-000000000000") , Guid.NewGuid() , Guid.Empty }, //Guid
			new object[] { SqlGuid.Parse("AAAAAAAA-0000-0000-0000-000000000000") , SqlGuid.Null }, //SqlGuid
			new object[] { "AAA" , "\n\t\r !@#$%^&*()/.,';][\\=-`~", String.Empty }, //string
			new object[] { byte.MinValue , byte.MaxValue , default(byte) }, //byte
			new object[] { new byte[] { 1 , 2 , 3 }, Array.Empty<byte>() }, //byte[]
			new object[] { short.MinValue, short.MaxValue , default(short) }, //short
			new object[] { int.MinValue, int.MaxValue , default(int) }, //int
			new object[] { long.MinValue, long.MaxValue , default(long) }, //long
			new object[] { 1M, decimal.MinValue, decimal.MaxValue , default(decimal) }, //decimal
			new object[] { 1D, double.MinValue , double.MaxValue , default(double) }, //double
			new object[] { true , false }, //bool
			};

			foreach (object[] keys in supportedKeySamples)
			{
				var serializer = new StatisticsCacheSerializer();
				var histogram = new SqlHistogram("Schema", "Table", "Column",
					keys.Select(key => new SqlHistogramStep(key, 100, 10000)).ToArray()
					);
				var histograms = new SqlHistogram[] { histogram };

				var serialized = serializer.SerializeHistograms(histograms);

				var histograms2 = serializer.DeserializeHistograms(serialized);

				AssertAreEqual(histograms[0], histograms2[0]);
			}
		}

		public void TestSerializeDeserialize_DifferentTypesInObjectArray_ThrowException()
		{
			var serializer = new StatisticsCacheSerializer();
			var step1 = new SqlHistogramStep(new Tuple<int, int>(1, 10), 100, 10000);
			var step2 = new SqlHistogramStep(30.1d, 100, 10000);
			var histogram = new SqlHistogram("Schema", "Table", "Column", new[] { step1, step2 });
			var histograms = new SqlHistogram[] { histogram };

			AssertExceptionThrown<ArgumentException>("Should Throw Exception", "All items in the object array should be of the same type as the first item. First item type :System.Tuple`2[System.Int32,System.Int32], Other item type : System.Double", () =>
			{
				var serialized = serializer.SerializeHistograms(histograms);
			});
		}
	}
}


