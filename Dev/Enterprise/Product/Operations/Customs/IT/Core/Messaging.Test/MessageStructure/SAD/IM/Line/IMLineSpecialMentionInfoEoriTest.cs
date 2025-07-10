using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineSpecialMentionsEoriTest : TestCase
{
	public void TestConstrutor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineSpecialMentionInfoEori(null));
			AssertNoExceptionThrown(() => new IMLineSpecialMentionInfoEori(new Mock<ISpecialMentionEoriInfo>().Object));
		});
	}
}
