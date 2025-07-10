using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETHeaderGuaranteeCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderGuaranteeCollection(null));
			AssertNoExceptionThrown(() => new ETHeaderGuaranteeCollection(new Mock<IEnumerable<IETHeaderGuarantee>>().Object));
		});
	}
}
