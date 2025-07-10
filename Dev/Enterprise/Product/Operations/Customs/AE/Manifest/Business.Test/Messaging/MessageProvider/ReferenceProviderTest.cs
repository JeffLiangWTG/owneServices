using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class ReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<ReferenceProvider>
{
	[ExpectNoExceptions]
	public void TestReferenceCode() => NUnit.Framework.Assert.That(GetProvider().ReferenceCode, Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestReferenceIdentifier() => NUnit.Framework.Assert.That(GetProvider().ReferenceIdentifier, Is.EqualTo("XYZ"));

	protected override ReferenceProvider GetProvider() => new ReferenceProvider("ABC", "XYZ");
}
