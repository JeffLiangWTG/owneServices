using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class GoodsContainerDetailsProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsContainerDetailsProvider>
{
	[ExpectNoExceptions]
	public void TestContainerIdentifier() => NUnit.Framework.Assert.That(GetProvider().ContainerIdentifier, NUnit.Framework.Is.EqualTo("ABC"));

	[ExpectNoExceptions]
	public void TestPackageQuantity() => NUnit.Framework.Assert.That(GetProvider().PackageQuantity, NUnit.Framework.Is.EqualTo(123));

	protected override GoodsContainerDetailsProvider GetProvider() => new GoodsContainerDetailsProvider("ABC", 123);
}
