using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815HeaderEadTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815HeaderEad(null));
			AssertNoExceptionThrown(() => new IE815HeaderEad(new Mock<IHeaderEad>().Object));
		});
	}
}
