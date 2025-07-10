using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineSpecialMentionsUnloadingDataTest : TestCase
{
	public void TestConstrutor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineSpecialMentionInfoUnloadingData(null));
			AssertNoExceptionThrown(() => new IMLineSpecialMentionInfoUnloadingData(new Mock<ISpecialMentionUnloadingDataInfo>().Object));
		});
	}
}
