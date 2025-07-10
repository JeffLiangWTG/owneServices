using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DateMessageBlockAttributeTest : TestCase
	{
		public void TestLength()
		{
			AssertEquals(6, new MessageBlockDateAttribute(1, "yyddMM").Length);
			AssertEquals(8, new MessageBlockDateAttribute(1).Length);
		}

		public void TestDeserialiseEmptyDate()
		{
			AssertEquals(ZDate.Empty, new MessageBlockDateAttribute(1).DeSerialise("        "));
			AssertEquals(ZDate.Empty, new MessageBlockDateAttribute(1).DeSerialise("00000000"));
		}

		public void TestSerialiseZDateTime()
		{
			AssertEquals("19710918", new MessageBlockDateAttribute(1).Serialise(ZDate.BrettsBirthday));
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "'SA999ASD' is not a valid yyyyMMdd format\r\nParameter name: value")]
		public void TestDeSerialiseInvalidDate()
		{
			new MessageBlockDateAttribute(1).DeSerialise("SA999ASDFDS");
		}

		public void TestDeSerialiseMaxDate()
		{
			AssertEquals(new ZDateTime(2099, 12, 31), new MessageBlockDateAttribute(1).DeSerialise("99999999"));
		}

		public void TestDeserialiseDate()
		{
			AssertEquals(ZDate.BrettsBirthday, new MessageBlockDateAttribute(1).DeSerialise("19710918"));
		}

		public void TestDeserialiseAndSerialiseDateFormat()
		{
			MessageBlockDateAttribute attribute = new MessageBlockDateAttribute(1, "yyddMM");
			AssertEquals(ZDate.BrettsBirthday, attribute.DeSerialise("711809"));
			AssertEquals("711809", attribute.Serialise(ZDate.BrettsBirthday));
		}
	}
}
