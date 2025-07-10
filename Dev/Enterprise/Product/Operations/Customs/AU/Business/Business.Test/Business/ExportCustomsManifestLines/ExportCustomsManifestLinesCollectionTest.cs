using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestLinesCollection))]
	sealed class ExportCustomsManifestLinesCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultCountryOfDestinationFromHeader()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_RN_NKCountryOfDestination = "AQ";
			AssertEquals("AQ", header.Lines.AddNew().EL_RN_NKCountryOfDestination);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			return new ExportCustomsManifestLinesCollection(header, Factory);
		}
	}
}
