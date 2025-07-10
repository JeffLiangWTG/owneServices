using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldSignedIntegerRepresentationAttributeTest : TestCase
{
	public void TestConstructor()
	{
		var attribute1 = new MessageFieldSignedIntegerRepresentationAttribute(10, true);
		Assert("IsFixedLength should be correctly set via constructor.", attribute1.IsFixedLength);
		AssertEquals("Length should be correctly set as 10 via constructor, because the Sign takes up a station.", 10, attribute1.Length);
	}

	public void TestSerialize()
	{
		var notFixedLengthAttribute = new MessageFieldSignedIntegerRepresentationAttribute(5, false);
		var fixedLengthAttribute = new MessageFieldSignedIntegerRepresentationAttribute(5, true);

		CombineAssertions("SerializeValue method", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => notFixedLengthAttribute.SerializeValue(null));
			AssertExceptionThrown<InvalidCastException>("Input value must be ZInt", () => notFixedLengthAttribute.SerializeValue(new ZString("a")));
			AssertEquals("Return value", "5", notFixedLengthAttribute.SerializeValue(new ZInt(5)));
			AssertEquals("Return value", "-5", notFixedLengthAttribute.SerializeValue(new ZInt(-5)));
			AssertEquals("Return value", "00005", fixedLengthAttribute.SerializeValue(new ZInt(5)));
			AssertEquals("Return value", "-0005", fixedLengthAttribute.SerializeValue(new ZInt(-5)));
		});
	}
}
