using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class PartyProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyProvider>
{
	[ExpectNoExceptions]
	public void TestPartyFunctionCode() => NUnit.Framework.Assert.That(GetProvider().PartyFunctionCode, NUnit.Framework.Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestPartyIdentifier() => NUnit.Framework.Assert.That(GetProvider().PartyIdentifier, NUnit.Framework.Is.EqualTo("XYZ"));

	protected override PartyProvider GetProvider() => new PartyProvider("ABC", "XYZ");
}
