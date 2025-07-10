using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocExportCustomsManifestLineCollection))]
	sealed class DocExportCustomsManifestLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocExportCustomsManifestLineCollection>
	{
		#region Implementation

		protected override DocExportCustomsManifestLineCollection GetCollectionToTest()
		{
			return new DocExportCustomsManifestLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ExportCustomsManifestLines line = Factory.New<ExportCustomsManifestLines>();
			return DocExportCustomsManifestLine.New(line, Factory);
		}

		#endregion
	}
}
