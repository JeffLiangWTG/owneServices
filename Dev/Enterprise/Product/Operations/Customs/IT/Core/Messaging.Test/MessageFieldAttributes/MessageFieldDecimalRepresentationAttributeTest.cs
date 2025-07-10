using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldDecimalRepresentationAttributeTest : TestCase
{
	public void TestConstructor()
	{
		var attribute = new MessageFieldDecimalRepresentationAttribute(12, 3, true);
		AssertEquals("IntegerPartLength should be correctly set via constructor.", 12, attribute.IntegerPartLength);
		AssertEquals("DecimalPartLength should be correctly via constructor.", 3, attribute.DecimalPartLength);
		Assert("IsFixedLength should be correctly set via constructor.", attribute.IsFixedLength);
		AssertEquals("Length should be correctly calculated. Length = IntegerPartLength + Dot + DecimalPartLength.", 16, attribute.Length);

		attribute = new MessageFieldDecimalRepresentationAttribute(12, 3, false);
		Assert("When IsFixedLength is false, IsDecimalPartFixedLength should be set to false", !attribute.IsDecimalPartFixedLength);

		attribute = new MessageFieldDecimalRepresentationAttribute(12, 3, true);
		Assert("When IsFixedLength is true, IsDecimalPartFixedLength should be set to true", attribute.IsDecimalPartFixedLength);

		attribute = new MessageFieldDecimalRepresentationAttribute(12, 3, false, true);
		Assert("IsDecimalPartFixedLength should be set to true.", attribute.IsDecimalPartFixedLength);

		AssertExceptionThrown<ArgumentOutOfRangeException>("It should thrown an exception if the integerPartLength is zero.", () => new MessageFieldDecimalRepresentationAttribute(0, 2, true));
		AssertExceptionThrown<ArgumentOutOfRangeException>("It should thrown an exception if the integerPartLength is negative.", () => new MessageFieldDecimalRepresentationAttribute(-10, 2, true));

		AssertExceptionThrown<ArgumentOutOfRangeException>("It should thrown an exception if the decimalPartLength is zero.", () => new MessageFieldDecimalRepresentationAttribute(10, 0, true));
		AssertExceptionThrown<ArgumentOutOfRangeException>("It should thrown an exception if the decimalPartLength is negative.", () => new MessageFieldDecimalRepresentationAttribute(10, -2, true));
	}

	public void TestSerialize()
	{
		var notFixedLengthAttribute = new MessageFieldDecimalRepresentationAttribute(5, 5, false, false);
		var fixedLengthAttribute = new MessageFieldDecimalRepresentationAttribute(5, 5, true);
		var decimalPartFixedLengthAttribute = new MessageFieldDecimalRepresentationAttribute(5, 5, false, true);

		CombineAssertions("SerializeValue", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => notFixedLengthAttribute.SerializeValue(null));
			AssertExceptionThrown<InvalidCastException>("Input value must be ZDecimal", () => notFixedLengthAttribute.SerializeValue(new ZString("aaa")));
			AssertEquals("Return value", "5.12", notFixedLengthAttribute.SerializeValue(new ZDecimal(5.12m)));
			AssertEquals("Return value", "00005.12000", fixedLengthAttribute.SerializeValue(new ZDecimal(5.12m)));
			AssertEquals("Return value", "5.12000", decimalPartFixedLengthAttribute.SerializeValue(new ZDecimal(5.12m)));
			AssertEquals("Return value", "5.12346", decimalPartFixedLengthAttribute.SerializeValue(new ZDecimal(5.1234558)));
		});
	}
}
