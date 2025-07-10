using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineAdditionalCodeCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETLineAdditionalCodeCollection(null));
			AssertNoExceptionThrown(() => new ETLineAdditionalCodeCollection(new Mock<IEnumerable<ZString>>().Object));
		});
	}
}
