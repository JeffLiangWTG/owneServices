using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class FreeTextProviderTest : Customs.Business.Testing.DataProviderTestCase<FreeTextProvider>
{
	[ExpectNoExceptions]
	public void TestSubjectCode() => NUnit.Framework.Assert.That(GetProvider().SubjectCode, NUnit.Framework.Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestText() => NUnit.Framework.Assert.That(GetProvider().Text, NUnit.Framework.Is.EqualTo("XYZ"));

	protected override FreeTextProvider GetProvider() => new FreeTextProvider("ABC", "XYZ");
}
