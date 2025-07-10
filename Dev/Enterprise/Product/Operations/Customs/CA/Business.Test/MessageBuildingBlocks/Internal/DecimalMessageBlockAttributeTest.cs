using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DecimalMessageBlockAttributeTest : TestCase
	{
		public void TestDeserialiseEmptyDecimal()
		{
			AssertEquals(ZDecimal.Zero, new MessageBlockDecimalAttribute(1, 5).DeSerialise("     "));
		}

		public void TestDeserialiseDecimal()
		{
			MessageBlockDecimalAttribute attribute = new MessageBlockDecimalAttribute(1, 6);
			AssertEquals(3.1415m, attribute.DeSerialise("3.14159"));
			AssertEquals(23.14m, attribute.DeSerialise("23.14000"));
		}

		public void TestSerialise()
		{
			MessageBlockDecimalAttribute attribute = new MessageBlockDecimalAttribute(1, 4);
			AssertEquals("120 ", attribute.Serialise(new ZDecimal(120.1)));
			AssertEquals("12.1", attribute.Serialise(new ZDecimal(12.1)));
			AssertEquals("12.2", attribute.Serialise(new ZDecimal(12.15)));
			AssertEquals("12.1", attribute.Serialise(new ZDecimal(12.14)));
			AssertEquals("0.2 ", attribute.Serialise(new ZDecimal(0.200)));
		}

		public void TestSerialiseOutOfRangeDecimalPlaces()
		{
			MessageBlockDecimalAttribute attribute = new MessageBlockDecimalAttribute(1, 6);
			AssertEquals("1.2131", attribute.Serialise(new ZDecimal(1.21312321)));
			AssertEquals("10002 ", attribute.Serialise(new ZDecimal(10001.50)));
			AssertEquals("******", attribute.Serialise(new ZDecimal(1210001.50)));
			AssertEquals("******", attribute.Serialise(new ZDecimal(-10)));
		}
	}
}
