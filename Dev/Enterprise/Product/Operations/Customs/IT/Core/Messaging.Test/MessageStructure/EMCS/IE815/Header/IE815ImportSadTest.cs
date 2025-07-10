using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815ImportSadTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var importSadMock = new Mock<IImportSad>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815ImportSad(null, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815ImportSad(importSadMock, -1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815ImportSad(importSadMock, 10));
			AssertNoExceptionThrown(() => new IE815ImportSad(importSadMock, 1));
		});
	}
}
