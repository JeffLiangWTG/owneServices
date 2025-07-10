using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestedType(typeof(CATCPLoadFlatFileDataExporter))]
	sealed class CATCPLoadFlatFileDataExporterTest : FlatFileDataExporterTestCase
	{
		public override FlatFileDataExporter GetDataExporter() => new CATCPLoadFlatFileDataExporter();

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			var collection = new OrgHeaderCollectionForTest(Factory);
			collection.Add(Factory.NewWithValidTestData<OrgHeader>());
			return collection;
		}

		sealed class OrgHeaderCollectionForTest : BusinessObjectCollection<OrgHeader>
		{
			public OrgHeaderCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}
		}
	}
}
