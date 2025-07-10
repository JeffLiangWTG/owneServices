using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class IMLineNationalProcedureCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => new IMLineNationalProcedureCollection(null));
			AssertNoExceptionThrown(() => new IMLineNationalProcedureCollection(new Mock<IEnumerable<ZString>>().Object));
		});
	}
}
