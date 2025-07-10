using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsInspectionAtLocationCusCodeDataCollection))]
	sealed class GvmsInspectionAtLocationCusCodeDataCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestReadOnly()
		{
			var collection = (GvmsInspectionAtLocationCusCodeDataCollection)GetCollectionToTest();
			AssertEquals(true, collection.ReadOnly);
		}

		public void TestLoadInspectionLocationsCollectionFromCusCodeData()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			AssertEquals(0, header.InspectionLocations.Count);

			var location1 = Factory.New<GvmsInspectionAtLocationCusCodeData>();
			location1.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			location1.CY_ParentID = header.PK;
			location1.CY_Code = "1";
			location1.CY_Data = "L0029A";

			var location2 = Factory.New<GvmsInspectionAtLocationCusCodeData>();
			location2.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			location2.CY_ParentID = header.PK;
			location2.CY_Code = "2";
			location2.CY_Data = "L0030A";

			header.InspectionLocations.Load();

			CombineAssertions(() =>
			{
				AssertEquals(2, header.InspectionLocations.Count);
				AssertEquals(1, header.InspectionLocations.Where(location => location.CY_Code == "1" && location.CY_Data == "L0029A").Count());
				AssertEquals(1, header.InspectionLocations.Where(location => location.CY_Code == "2" && location.CY_Data == "L0030A").Count());
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new GvmsInspectionAtLocationCusCodeDataCollection(ManifestHeader);

		AsycudaManifestHeader ManifestHeader => Factory.NewWithValidTestData<AsycudaManifestHeader>();
	}
}
