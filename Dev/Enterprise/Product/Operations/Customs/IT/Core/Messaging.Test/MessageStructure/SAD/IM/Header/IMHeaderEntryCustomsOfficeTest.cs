using System;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMHeaderEntryCustomsOfficeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderEntryCustomsOffice(null));
			AssertNoExceptionThrown(() => new IMHeaderEntryCustomsOffice(new Mock<IIMHeaderEntryCustomsOffice>().Object));
		});
	}
}
