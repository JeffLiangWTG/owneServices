using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineSpecialMentionInfoEoriTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineSpecialMentionInfoEori(null));
			AssertNoExceptionThrown(() => new ETLineSpecialMentionInfoEori(new Mock<ISpecialMentionEoriInfo>().Object));
		});
	}
}
