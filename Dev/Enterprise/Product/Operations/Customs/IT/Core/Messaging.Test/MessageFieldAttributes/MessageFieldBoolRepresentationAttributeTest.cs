using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldBoolRepresentationAttributeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var attribute = new MessageFieldBoolRepresentationAttribute();
			AssertEquals("IsFixedLength should be correctly set via constructor.", true, attribute.IsFixedLength);
			AssertEquals("Length should be 1", 1, attribute.Length);
		});
	}

	public void TestSerialize()
	{
		var attribute = new MessageFieldBoolRepresentationAttribute();
		CombineAssertions("SerializeValue", () =>
		{
			AssertExceptionThrown<ArgumentNullException>("Input value null is not allowed", () => attribute.SerializeValue(null));
			AssertExceptionThrown<InvalidCastException>("Input value must be ZBool", () => attribute.SerializeValue(new ZString("aaa")));
			AssertEquals("When input is 'false' the return value", "0", attribute.SerializeValue(ZBool.False));
			AssertEquals("When input is 'true' the return value", "1", attribute.SerializeValue(ZBool.True));
		});
	}
}
