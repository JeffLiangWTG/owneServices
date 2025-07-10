using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks.Testing
{
	sealed class StringMessageBlockAttributeTest : TestCase
	{
		public void TestSerialiseToInvalidIfMaxLengthExceeded()
		{
			try
			{
				new MessageBlockStringAttribute(1, 2, false).Serialise(new ZString("XYZ"));
				Fail("Should have thrown a MessageBlockSerialisationException");
			}
			catch (MessageBlockSerialisationException ex)
			{
				AssertEquals("Data provided exceeded allowable maximum length:\r\nMaximum Length:2\r\nActual Length:3", ex.Message);
				AssertEquals("**", ex.NewInvalidFormat);
			}
		}

		public void TestSerialiseZString()
		{
			AssertEquals("XYZ  ", new MessageBlockStringAttribute(1, 5).Serialise(new ZString("XYZ")));
			AssertEquals("XYZ12", new MessageBlockStringAttribute(1, 5).Serialise(new ZString("XYZ12")));
		}

		public void TestDeSerialise()
		{
			AssertEquals(new ZString("XYZ"), new MessageBlockStringAttribute(1, 5).DeSerialise("XYZ"));
		}
	}
}
