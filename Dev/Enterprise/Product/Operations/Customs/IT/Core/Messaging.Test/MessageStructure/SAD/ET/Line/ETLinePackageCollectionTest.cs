using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLinePackageCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLinePackageCollection(null));
			AssertNoExceptionThrown(() => new ETLinePackageCollection(new Mock<IEnumerable<IPackage>>().Object));
		});
	}
}
