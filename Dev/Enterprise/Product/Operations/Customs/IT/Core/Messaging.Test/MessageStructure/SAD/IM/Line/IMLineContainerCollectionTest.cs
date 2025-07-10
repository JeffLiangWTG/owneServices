using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineContainerCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineContainerCollection(null));
			AssertNoExceptionThrown(() => new IMLineContainerCollection(new Mock<IEnumerable<ZString>>().Object));
		});
	}
}
