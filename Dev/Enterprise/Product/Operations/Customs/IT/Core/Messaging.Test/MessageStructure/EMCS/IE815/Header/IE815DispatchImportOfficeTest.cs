using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815DispatchImportOfficeTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IE815DispatchImportOffice(null));
			AssertNoExceptionThrown(() => new IE815DispatchImportOffice(new Mock<IOffice>().Object));
		});
	}
}
