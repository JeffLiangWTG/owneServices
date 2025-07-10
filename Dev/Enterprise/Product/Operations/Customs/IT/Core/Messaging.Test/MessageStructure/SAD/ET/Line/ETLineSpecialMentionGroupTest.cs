using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineSpecialMentionGroupTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineSpecialMentionGroup(null));
			AssertNoExceptionThrown(() => new ETLineSpecialMentionGroup(new Mock<IETLineSpecialMentionGroup>().Object));
		});
	}
}
