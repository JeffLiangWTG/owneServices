using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestHeaderCollection))]
	sealed class ExportCustomsManifestHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new ExportCustomsManifestHeaderCollection(Factory);
	}
}
