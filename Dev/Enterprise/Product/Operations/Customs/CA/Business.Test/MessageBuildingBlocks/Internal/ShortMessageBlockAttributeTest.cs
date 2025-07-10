using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class ShortMessageBlockAttributeTest : TestCase
	{
		public void TestDeSerialiseEmptyShort()
		{
			AssertEquals(ZShort.Zero, new MessageBlockShortAttribute(1, 5).DeSerialise("     "));
			AssertEquals(ZShort.Zero, new MessageBlockShortAttribute(1, 5).DeSerialise("*****"));
		}

		public void TestSerialiseEmptyZShort()
		{
			AssertEquals("0    ", new MessageBlockShortAttribute(1, 5).Serialise(ZShort.Zero));
		}

		public void TestSerialiseZShort()
		{
			AssertEquals("123  ", new MessageBlockShortAttribute(1, 5).Serialise((ZShort)123));
		}

		public void TestSerialiseZShortHavingMoreDigits()
		{
			AssertEquals("**", new MessageBlockShortAttribute(1, 2).Serialise((ZShort)123));
		}
	}
}
