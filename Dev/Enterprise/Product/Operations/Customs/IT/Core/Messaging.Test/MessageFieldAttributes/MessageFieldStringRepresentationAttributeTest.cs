using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldStringRepresentationAttributeTest : TestCase
{
	public void TestConstructor()
	{
		var attribute1 = new MessageFieldStringRepresentationAttribute(CharType.Alphanumeric, 10, true);

		AssertExceptionThrown<ArgumentOutOfRangeException>("It should throw an exception if the input charType is invalid.", () => new MessageFieldStringRepresentationAttribute((CharType)100000, 10, true));
		AssertEquals("CharType should be correctly set via constructor.", CharType.Alphanumeric, attribute1.CharType);
		AssertEquals("Length should be correctly set via constructor.", 10, attribute1.Length);
		Assert("IsFixedLength should be correctly set via constructor.", attribute1.IsFixedLength);
	}

	public void TestSerialize()
	{
		var notFixedLengthAttribute = new MessageFieldStringRepresentationAttribute(CharType.Alphanumeric, 7, false);
		var fixedLengthAttribute = new MessageFieldStringRepresentationAttribute(CharType.Alphanumeric, 7, true);

		CombineAssertions("SerializeValue", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => notFixedLengthAttribute.SerializeValue(null));
			AssertExceptionThrown<InvalidCastException>("Input value must be ZDecimal", () => notFixedLengthAttribute.SerializeValue(new ZInt(0)));
			AssertEquals("Return value", "hello", notFixedLengthAttribute.SerializeValue(new ZString("hello")));
			AssertEquals("Return value", "hello".PadRight(7, ' '), fixedLengthAttribute.SerializeValue(new ZString("hello")));
		});
	}
}
