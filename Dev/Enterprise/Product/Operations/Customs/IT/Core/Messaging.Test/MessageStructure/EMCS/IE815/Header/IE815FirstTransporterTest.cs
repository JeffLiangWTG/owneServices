using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815FirstTransporterTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815FirstTransporter(null));
			AssertNoExceptionThrown(() => new IE815FirstTransporter(new Mock<ITransportTrader>().Object));
		});
	}
}
