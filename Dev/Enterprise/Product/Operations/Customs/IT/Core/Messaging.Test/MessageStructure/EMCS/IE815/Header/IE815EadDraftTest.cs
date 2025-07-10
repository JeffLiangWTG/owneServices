using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815EadDraftTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815EadDraft(null));
			AssertNoExceptionThrown(() => new IE815EadDraft(new Mock<IEadDraft>().Object));
		});
	}
}
