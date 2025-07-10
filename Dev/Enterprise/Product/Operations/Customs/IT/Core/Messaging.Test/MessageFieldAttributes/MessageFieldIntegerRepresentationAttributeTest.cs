using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldIntegerRepresentationAttributeTest : TestCase
{
	public void TestConstructor()
	{
		var attribute1 = new MessageFieldIntegerRepresentationAttribute(10, true);
		AssertEquals("Length should be correctly set via constructor.", 10, attribute1.Length);
		Assert("IsFixedLength should be correctly set via constructor.", attribute1.IsFixedLength);
	}

	public void TestSerialize()
	{
		var notFixedLengthAttribute = new MessageFieldIntegerRepresentationAttribute(5, false);
		var fixedLengthAttribute = new MessageFieldIntegerRepresentationAttribute(5, true);

		CombineAssertions("SerializeValue", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => notFixedLengthAttribute.SerializeValue(null));
			AssertExceptionThrown<InvalidCastException>("Input value must be ZInt", () => notFixedLengthAttribute.SerializeValue(new ZString("aaa")));
			AssertEquals("Return value", "5", notFixedLengthAttribute.SerializeValue(new ZInt(5)));
			AssertEquals("Return value", "00005", fixedLengthAttribute.SerializeValue(new ZInt(5)));
		});
	}
}
