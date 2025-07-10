using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	[TestedType(typeof(OrgFlatFileExporter))]
	public class OrgFlatFileExporterTest : FlatFileDataExporterTestCase
	{
		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			OrgHeaderCollection organisations = new OrgHeaderCollection(Factory);
			organisations.Add(NavisionTestHelper.OrgForTesting(Factory));
			return organisations;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetDataExporter();
		}

		public override FlatFileDataExporter GetDataExporter()
		{
			return new OrgFlatFileExporter(Factory, null, "");
		}

		public void TestEnglishDesription()
		{
			OrgFlatFileExporter exporter = new OrgFlatFileExporter(Factory, null, "");
			AssertEquals("English Description should be 'Navision Organisations'", "Navision Organisations", exporter.EnglishDescription);
		}
	}
}
