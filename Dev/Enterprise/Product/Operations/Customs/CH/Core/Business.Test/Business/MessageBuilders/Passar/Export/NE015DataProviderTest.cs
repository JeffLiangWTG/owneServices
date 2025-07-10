using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE015DataProvider))]
sealed class NE015DataProviderTest : BasePassarExportDeclarationDataProviderTest<NE015DataProvider>
{
	protected override NE015DataProvider CreateDataProvider() => new NE015DataProvider(SendingObject);
}
