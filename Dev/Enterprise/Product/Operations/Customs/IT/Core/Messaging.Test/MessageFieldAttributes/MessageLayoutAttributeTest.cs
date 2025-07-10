using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.MessageFieldAttributes.Testing;

sealed class MessageLayoutAttributeTest : TestCase
{
	public void TestPosition()
	{
		var attribute1 = new MessageLayoutAttribute();
		AssertExceptionThrown<ArgumentOutOfRangeException>("It should throw an exception when set Position as zero.", () => { attribute1.Position = 0; });
		AssertExceptionThrown<ArgumentOutOfRangeException>("It should throw an exception when set Position as negative.", () => { attribute1.Position = -10; });
	}
}
