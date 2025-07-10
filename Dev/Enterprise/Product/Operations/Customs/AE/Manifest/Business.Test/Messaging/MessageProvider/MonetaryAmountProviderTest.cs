using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

internal class MonetaryAmountProviderTest : Customs.Business.Testing.DataProviderTestCase<MonetaryAmountProvider>
{
	[ExpectNoExceptions]
	public void TestAmountType() => NUnit.Framework.Assert.That(GetProvider().AmountType, NUnit.Framework.Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestAmount() => NUnit.Framework.Assert.That(GetProvider().Amount, NUnit.Framework.Is.EqualTo(123.45m));

	[ExpectNoExceptions]
	public void TestCurrency() => NUnit.Framework.Assert.That(GetProvider().Currency, NUnit.Framework.Is.EqualTo("XYZ"));

	protected override MonetaryAmountProvider GetProvider() => new MonetaryAmountProvider("ABC", 123.45m, "XYZ");
}
