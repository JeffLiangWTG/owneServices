using System;
using Enterprise.Customs.IT.Messaging.EMCS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IE815PackageTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var packageMock = new Mock<IPackage>().Object;
			AssertExceptionThrown<ArgumentNullException>(() => new IE815Package(null, 1));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815Package(packageMock, 0));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new IE815Package(packageMock, 100));
			AssertNoExceptionThrown(() => new IE815Package(packageMock, 1));
		});
	}
}
