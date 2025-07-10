using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineSpecialMentionInfoUnloadingDataTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineSpecialMentionInfoUnloadingData(null));
			AssertNoExceptionThrown(() => new ETLineSpecialMentionInfoUnloadingData(new Mock<ISpecialMentionUnloadingDataInfo>().Object));
		});
	}
}
