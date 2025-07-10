using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(NavisionFlatFileExporterTestClass))]
	public class NavisionFlatFileExporterTest : FlatFileDataExporterTestCase
	{
		public override FlatFileDataExporter GetDataExporter()
		{
			return new NavisionFlatFileExporterTestClass();
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			OrgHeaderCollection organisations = new OrgHeaderCollection(Factory);
			organisations.Add(NavisionTestHelper.OrgForTesting(Factory));
			return organisations;
		}

		public void TestFlatFileFormat()
		{
			NavisionFlatFileExporterTestClass exporter = new NavisionFlatFileExporterTestClass();
			IFlatFileFormat format = exporter.FlatFileFormat;
			AssertSame("Flat File Format was not lazy loaded", format, exporter.FlatFileFormat);
			AssertEquals("Format should be CsvFlatFileFormat type", typeof(CsvFlatFileFormat), format.GetType());
		}

		public void TestAppendToFile()
		{
			NavisionFlatFileExporterTestClass exporter = new NavisionFlatFileExporterTestClass();
			AssertEquals("Append to file should be true", true, exporter.AppendToFile);
		}

		public void TestEnglishDescription()
		{
			NavisionFlatFileExporterTestClass exporter = new NavisionFlatFileExporterTestClass();
			AssertEquals("English Description", "Navision Export", exporter.EnglishDescription);
		}
	}
}
