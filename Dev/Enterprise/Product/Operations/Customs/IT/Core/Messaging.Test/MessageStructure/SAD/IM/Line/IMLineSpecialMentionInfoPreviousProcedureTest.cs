using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineSpecialMentionsPreviousProcedureTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineSpecialMentionInfoPreviousProcedure(null));
			AssertNoExceptionThrown(() => new IMLineSpecialMentionInfoPreviousProcedure(new Mock<IPreviousAdministrativeReference>().Object));
		});
	}
}
