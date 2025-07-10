using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryLineCustomsValuationTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("freightAdjustments required", () => new EntryLineCustomsValuation(null, Money.Empty));
		AssertExceptionThrown<ArgumentNullException>("lineValue required", () => new EntryLineCustomsValuation(Money.Empty, null));
		AssertNoExceptionThrown(() => new EntryLineCustomsValuation(Money.Empty, Money.Empty));
	}
}
