using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class LocationProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationProvider>
{
	[ExpectNoExceptions]
	public void TestLocationFunctionCode() => NUnit.Framework.Assert.That(GetProvider().LocationFunctionCode, NUnit.Framework.Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestLocationIdentifier() => NUnit.Framework.Assert.That(GetProvider().LocationIdentifier, NUnit.Framework.Is.EqualTo("XYZ"));

	protected override LocationProvider GetProvider() => new LocationProvider("ABC", "XYZ");
}
