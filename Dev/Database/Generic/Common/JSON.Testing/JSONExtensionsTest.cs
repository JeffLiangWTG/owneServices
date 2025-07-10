using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CargoWise.Common.JSON.Extensions;
using NUnit.Framework;

namespace CargoWise.Common.JSON.Testing
{
	public class JSONExtensionsTest : TestCase
	{
		readonly TestDTO dto = new TestDTO
		{
			Boolean = false,
			Number = 123,
			StringMember = "aBCDe!~中国|АБВГД|ایران",
			FloatingPoint = 1.023d,
			DateAndTime = new DateTime(2015, 4, 8, 12, 00, 00),
			GUID = new Guid("{455063B9-0E6F-4380-8961-023C9C073405}"),
			Collection = new Dictionary<int, string>() { { 1, "a" }, { 2, "b" }, { 3, "c" } },
			EnumerationValue = SomeEnum.Ten
		};

		readonly string json = @"{""Boolean"":false,""Collection"":{""1"":""a"",""2"":""b"",""3"":""c""},""DateAndTime"":""\/Date(1428458400000+1000)\/"",""EnumerationValue"":10,""FloatingPoint"":1.023,""GUID"":""455063b9-0e6f-4380-8961-023c9c073405"",""Number"":123,""StringMember"":""aBCDe!~中国|АБВГД|ایران""}";

		public void TestToJSON()
		{
			var result = dto.ToJSON();
			AssertEquals(json, result);
		}

		public void TestFromJSON()
		{
			var result = json.FromJSON<TestDTO>();
			AssertEquals(dto.Boolean, result.Boolean);
			AssertEquals(dto.Number, result.Number);
			AssertEquals(dto.StringMember, result.StringMember);
			AssertEquals(dto.FloatingPoint, result.FloatingPoint);
			AssertEquals(dto.DateAndTime, result.DateAndTime);
			AssertEquals(dto.GUID, result.GUID);
			Assert(dto.Collection.SequenceEqual(result.Collection));
			AssertEquals(dto.EnumerationValue, result.EnumerationValue);
		}
	}

	[DataContract]
	class TestDTO
	{
		[DataMember]
		public int Number { get; set; }

		[DataMember]
		public string StringMember { get; set; }

		[DataMember]
		public double FloatingPoint { get; set; }

		[DataMember]
		public bool Boolean { get; set; }

		[DataMember]
		public DateTime DateAndTime { get; set; }

		[DataMember]
		public Guid GUID { get; set; }

		[DataMember]
		public Dictionary<int, string> Collection { get; set; }

		[DataMember]
		public SomeEnum EnumerationValue { get; set; }
	}

	enum SomeEnum
	{
		One,
		Two,
		Three,
		Ten = 10
	}
}
