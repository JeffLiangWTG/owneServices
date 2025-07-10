using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class PartyContactCommunicationProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyContactCommunicationProvider>
{
	[ExpectNoExceptions]
	public void TestCommunicationCode() => NUnit.Framework.Assert.That(GetProvider().CommunicationCode, NUnit.Framework.Is.EqualTo(ExpectedCode));

	[ExpectNoExceptions]
	public void TestCommunicationIdentifier() => NUnit.Framework.Assert.That(GetProvider().CommunicationIdentifier, NUnit.Framework.Is.EqualTo(ExpectedIdentifier));

	protected override PartyContactCommunicationProvider GetProvider() => new(ExpectedCode, ExpectedIdentifier);

	const string ExpectedCode = "CODE";
	const string ExpectedIdentifier = "IDENTIFIER";
}
