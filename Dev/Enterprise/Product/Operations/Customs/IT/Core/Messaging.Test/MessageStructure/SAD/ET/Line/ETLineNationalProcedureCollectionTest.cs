using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ETLineNationalProcedureCollectionTest : TestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>(() => new ETLineNationalProcedureCollection(null));
			AssertNoExceptionThrown(() => new ETLineNationalProcedureCollection(new Mock<IEnumerable<ZString>>().Object));
		});
	}
}
