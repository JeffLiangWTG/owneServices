using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815MovementImportSadTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815ImportSadContainer(null));
			AssertNoExceptionThrown(() => new IE815ImportSadContainer(new Mock<IImportSadContainer>().Object));
		});
	}
}
