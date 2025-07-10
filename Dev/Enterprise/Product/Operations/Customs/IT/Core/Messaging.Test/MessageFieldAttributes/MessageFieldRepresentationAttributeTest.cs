using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageFieldRepresentationAttributeTest : TestCase
{
	public void TestConstructor()
	{
		var attribute1 = new ConcreteMessageFieldRepresentationAttribute(10, true);
		AssertEquals("Length should be correctly set via constructor.", 10, attribute1.Length);
		Assert("IsFixedLength should be correctly set via constructor.", attribute1.IsFixedLength);

		AssertExceptionThrown<ArgumentOutOfRangeException>("It should throw an exception if the input length is zero.", () => new ConcreteMessageFieldRepresentationAttribute(0, true));
		AssertExceptionThrown<ArgumentOutOfRangeException>("It should throw an exception if the input length is negative.", () => new ConcreteMessageFieldRepresentationAttribute(-10, true));
	}

	public void TestSerializeValue()
	{
		var attribute = new ConcreteMessageFieldRepresentationAttribute(10, true);
		AssertEquals("SerializeValue method return value", "Test", attribute.SerializeValue(null));
	}
}

public sealed class ConcreteMessageFieldRepresentationAttribute : MessageFieldRepresentationAttribute
{
	public ConcreteMessageFieldRepresentationAttribute(int length, bool isFixedLength) : base(length, isFixedLength)
	{
	}

	public override ZString SerializeValue(IZType value) => "Test";
}
