using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineAdditionalCodeCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineAdditionalCodeCollection(null));
			AssertNoExceptionThrown(() => new IMLineAdditionalCodeCollection(new Mock<IEnumerable<ZString>>().Object));
		});
	}
}
