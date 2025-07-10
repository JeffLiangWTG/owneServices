using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815CompetentAuthorityDispatchOfficeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815CompetentAuthorityDispatchOffice(null));
			AssertNoExceptionThrown(() => new IE815CompetentAuthorityDispatchOffice(new Mock<IOffice>().Object));
		});
	}
}
