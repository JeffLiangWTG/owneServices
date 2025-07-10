using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineSpecialMentionInfoAdditionalInformationTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineSpecialMentionInfoAdditionalInformation(null));
			AssertNoExceptionThrown(() => new ETLineSpecialMentionInfoAdditionalInformation(new Mock<IETLineSpecialMentionInfoAdditionalInformation>().Object));
		});
	}
}
