using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLinePreviousDocumentTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new ETLinePreviousDocument(null));
			AssertNoExceptionThrown(() => new ETLinePreviousDocument(new Mock<IPreviousDocument>().Object));
		});
	}
}
