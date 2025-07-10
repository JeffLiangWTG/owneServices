using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineSpecialMentionGroupTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineSpecialMentionGroup(null));
			AssertNoExceptionThrown(() => new IMLineSpecialMentionGroup(new Mock<IIMLineSpecialMentionGroup>().Object));
		});
	}
}
