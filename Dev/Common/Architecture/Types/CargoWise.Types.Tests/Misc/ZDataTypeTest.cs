using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	public class ZDataTypeTest : TestCase
	{
		public void TestDefaultConstructor()
		{
			AssertEquals("IsNumeric", false, new ZDataType().IsNumeric);
			AssertEquals("IsInteger", false, new ZDataType().IsInteger);
		}

		public void TestNonNumeric()
		{
			AssertEquals("IsNumeric", false, ZDataType.NonNumeric.IsNumeric);
			AssertEquals("IsInteger", false, ZDataType.NonNumeric.IsInteger);
		}

		public void TestNumeric()
		{
			AssertEquals("IsNumeric", true, ZDataType.Numeric.IsNumeric);
			AssertEquals("IsInteger", false, ZDataType.Numeric.IsInteger);
		}

		public void TestInteger()
		{
			AssertEquals("IsNumeric", true, ZDataType.Integer.IsNumeric);
			AssertEquals("IsInteger", true, ZDataType.Integer.IsInteger);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestObjectToZTypeException()
		{
			ZDataType.ObjectToZType(typeof(ZDataTypeTest), new ZDataTypeTest());
		}

		[ExpectException(typeof(FormatException))]
		public void TestObjectToTypeFormatException()
		{
			ZDataType.ObjectToZType(typeof(ZGuid), "bad guid");
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestObjectToTypeZTypeNotSupportedException()
		{
			ZDataType.ObjectToZType(typeof(ZGuid), DateTime.MinValue);
		}

		public void TestObjectToZGuid()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZGuid), Guid.NewGuid()) is ZGuid);
		}

		public void TestObjectToZDate()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZDate), DateTime.MinValue) is ZDate);
		}

		public void TestObjectToZDateTime()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZDateTime), DateTime.MinValue) is ZDateTime);
		}

		public void TestObjectToZDateTimeOffset()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZDateTimeOffset), DateTimeOffset.MinValue) is ZDateTimeOffset);
		}

		public void TestObjectToZTime()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZTime), DateTime.MinValue) is ZTime);
		}

		public void TestObjectToZGeography()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZGeography), ZGeography.EmptySqlGeography) is ZGeography);
		}

		public void TestObjectToZDecimal()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZDecimal), decimal.MinValue) is ZDecimal);
		}

		public void TestObjectToZInt()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZInt), int.MinValue) is ZInt);
		}

		public void TestObjectToZShort()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZShort), short.MinValue) is ZShort);
		}

		public void TestObjectToZLong()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZLong), long.MinValue) is ZLong);
		}

		public void TestObjectToZByte()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZByte), byte.MinValue) is ZByte);
		}

		public void TestObjectToZBlob()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZBlob), Array.Empty<byte>()) is ZBlob);
		}

		public void TestObjectToZString()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZString), string.Empty) is ZString);
		}

		public void TestObjectToZBool()
		{
			Assert(ZDataType.ObjectToZType(typeof(ZBool), false) is ZBool);
		}

		public void TestObjectToZTypeReturnsSameInstance()
		{
			object anyZType = ZString.Empty;
			AssertEquals(anyZType, ZDataType.ObjectToZType(anyZType.GetType(), anyZType));
		}

		public void TestZTypeToBaseType()
		{
			AssertNull(ZDataType.ZTypeToBaseType(typeof(OperatingSystem)));

			Dictionary<Type, Type> expectedResult = new Dictionary<Type, Type>();
			expectedResult.Add(typeof(ZString), typeof(string));
			expectedResult.Add(typeof(ZGuid), typeof(Guid));
			expectedResult.Add(typeof(ZInt), typeof(int));
			expectedResult.Add(typeof(ZShort), typeof(short));
			expectedResult.Add(typeof(ZLong), typeof(long));
			expectedResult.Add(typeof(ZBlob), typeof(byte[]));
			expectedResult.Add(typeof(ZByte), typeof(byte));
			expectedResult.Add(typeof(ZDecimal), typeof(decimal));
			expectedResult.Add(typeof(ZDateTime), typeof(DateTime));
			expectedResult.Add(typeof(ZDateTimeOffset), typeof(DateTimeOffset));
			expectedResult.Add(typeof(ZTime), typeof(TimeSpan));
			expectedResult.Add(typeof(ZGeography), typeof(SqlGeography));
			expectedResult.Add(typeof(ZDate), typeof(DateTime));
			expectedResult.Add(typeof(ZBool), typeof(bool));

			foreach (Type type in expectedResult.Keys)
			{
				AssertEquals(type.Name, expectedResult[type], ZDataType.ZTypeToBaseType(type));
			}
		}

		public void TestZTypeToEmptyValue()
		{
			Dictionary<Type, object> expectedResult = new Dictionary<Type, object>();
			expectedResult.Add(typeof(ZString), ZString.Empty);
			expectedResult.Add(typeof(ZGuid), ZGuid.Empty);
			expectedResult.Add(typeof(ZInt), ZInt.Zero);
			expectedResult.Add(typeof(ZShort), ZShort.Zero);
			expectedResult.Add(typeof(ZLong), ZLong.Zero);
			expectedResult.Add(typeof(ZBlob), ZBlob.Empty);
			expectedResult.Add(typeof(ZByte), ZByte.Zero);
			expectedResult.Add(typeof(ZDecimal), ZDecimal.Zero);
			expectedResult.Add(typeof(ZDateTime), ZDateTime.Empty);
			expectedResult.Add(typeof(ZTime), ZTime.Empty);
			expectedResult.Add(typeof(ZDateTimeOffset), ZDateTimeOffset.Empty);
			expectedResult.Add(typeof(ZGeography), ZGeography.Empty);
			expectedResult.Add(typeof(ZDate), ZDateTime.Empty);
			expectedResult.Add(typeof(ZBool), ZBool.False);

			foreach (Type type in typeof(ZString).Assembly.GetTypes())
			{
				if (!type.IsInterface &&
					Array.IndexOf(type.GetInterfaces(), typeof(IZType)) >= 0 &&
					!type.Name.EndsWith("Contract")
					)
				{
					Assert(type.Name, expectedResult.ContainsKey(type));
					AssertEquals(type.Name, expectedResult[type], ZDataType.ZTypeToEmptyValue(type));
				}
			}
		}

		public void TestIsConvertibleToZType()
		{
			var values = new object[]
			{
				Guid.NewGuid(),
				DateTime.Now,
				DateTimeOffset.Now,
				12.34m,
				23,
				(short)12,
				(byte)2,
				new byte[] { 1, 2, 3 },
				"aaa",
				true
			};

			foreach (var value in values)
			{
				AssertEquals(string.Format("Expected {0} to be convertible to ZType", value),
					true, ZDataType.IsConvertibleToZType(value));

				AssertNoExceptionThrown("No exception thrown for conversion", () => ZDataType.ObjectToZType(value));
			}

			values = new object[]
			{
				new object(),
				null,
				new ZDataTypeTest(),
			};

			foreach (var value in values)
			{
				AssertEquals(string.Format("Expected {0} not to be convertible to ZType", value),
					false, ZDataType.IsConvertibleToZType(value));
			}
		}
	}
}
