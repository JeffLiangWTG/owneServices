using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CalcExportManifestLineCollection))]
	sealed class CalcExportManifestLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CalcExportManifestLineCollection>
	{
		protected override CalcExportManifestLineCollection GetCollectionToTest()
		{
			var header = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest);
			return new CalcExportManifestLineCollection(header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest);
			return new CalcExportManifestLine(header);
		}
	}
}
