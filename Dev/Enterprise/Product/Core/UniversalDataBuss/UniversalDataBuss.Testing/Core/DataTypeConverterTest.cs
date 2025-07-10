using System;
using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	[TestedType(typeof(DataTypeConverter))]
	class DataTypeConverterTest : EnumConverterTestCase<DataTypeConverter, DataType, Type>
	{
		public void TestNullEnumValueThrowsException()
		{
			var converter = new DataTypeConverter();
			AssertExceptionThrown(typeof(InvalidOperationException)
				, "Cannot pass null into GetCode(). Supported DataType values are: Boolean, Byte, DateTime, Decimal, Integer, Short, String, DateTimeOffset, Geography, Time."
				, delegate { converter.FromEnumValue(null); });
		}

		protected override IEnumerable<Type> GetAllPossibleEnterpriseValues()
		{
			return null; //Not required to map against Enterprise value
		}
	}
}
