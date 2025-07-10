using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CalcExportManifestLine))]
	sealed class CalcExportManifestLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = new CalcExportManifestHeader(Factory, ManifestTypeList.Codes.ExportMainManifest);
			return new CalcExportManifestLine(header);
		}
	}
}
