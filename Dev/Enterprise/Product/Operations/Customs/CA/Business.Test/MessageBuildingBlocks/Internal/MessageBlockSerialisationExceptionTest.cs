using System;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks.Testing
{
	sealed class MessageBlockSerialisationExceptionTest : TestCase
	{
		public void TestType()
		{
			Assert("MessageBlockSerialisationException should inherit from Exception", typeof(MessageBlockSerialisationException).IsSubclassOf(typeof(Exception)));
		}

		public void TestProperties()
		{
			MessageBlockSerialisationException exception = new MessageBlockSerialisationException("Dummy Test Message", "Hello World");
			AssertEquals("Message", "Dummy Test Message", exception.Message);
			AssertEquals("NewInvalidFormat", "Hello World", exception.NewInvalidFormat);
		}
	}
}
