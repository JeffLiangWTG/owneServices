using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETMessageLineTest : TestCase
{
	public void TestConstructor()
	{
		var etLine = new Mock<IETLine>().Object;
		var etHeader = new Mock<IETHeader>().Object;
		var sadMessageSendingObject = new Mock<ISadMessageSendingObject>().Object;

		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessageLine(null, null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessageLine(etLine, null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new ETMessageLine(etLine, etHeader, null));
			AssertNoExceptionThrown(() => new ETMessageLine(etLine, etHeader, sadMessageSendingObject));
		});
	}
}
