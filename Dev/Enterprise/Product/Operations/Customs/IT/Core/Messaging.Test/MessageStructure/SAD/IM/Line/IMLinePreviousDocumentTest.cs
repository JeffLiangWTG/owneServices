using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLinePreviousDocumentTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLinePreviousDocument(null));
			AssertNoExceptionThrown(() => new IMLinePreviousDocument(new Mock<IPreviousDocument>().Object));
		});
	}
}
