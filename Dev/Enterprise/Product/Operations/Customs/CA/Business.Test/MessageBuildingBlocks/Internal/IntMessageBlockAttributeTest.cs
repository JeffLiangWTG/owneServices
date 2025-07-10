using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class IntMessageBlockAttributeTest : TestCase
	{
		public void TestDeSerialiseEmptyInt()
		{
			AssertEquals(ZInt.Zero, new MessageBlockIntAttribute(1, 5).DeSerialise("     "));
			AssertEquals(ZInt.Zero, new MessageBlockIntAttribute(1, 5).DeSerialise("*****"));
		}

		public void TestSerialiseEmptyZInt()
		{
			AssertEquals("0    ", new MessageBlockIntAttribute(1, 5).Serialise(ZInt.Zero));
		}

		public void TestSerialiseZInt()
		{
			AssertEquals("123  ", new MessageBlockIntAttribute(1, 5).Serialise((ZInt)123));
		}

		public void TestSerialiseZIntHavingMoreDigitsThanAllowed()
		{
			AssertEquals("**", new MessageBlockIntAttribute(1, 2).Serialise((ZInt)123));
			AssertEquals("**", new MessageBlockIntAttribute(1, 2).Serialise(new ZInt(-1)));
		}
	}
}
