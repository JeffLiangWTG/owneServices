using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	[TestedType(typeof(GvmsInspectionAtLocationCusCodeData))]
	sealed class GvmsInspectionAtLocationCusCodeDataTest : Customs.Business.Testing.CusCodeDataTest<GvmsInspectionAtLocationCusCodeData>
	{
		protected override void SetUp()
		{
			base.SetUp();
			GVMSMessageTestHelper.SetupInspectionLocationsRefCusCodeList(Factory);
		}

		public void TestDisplayPropertiesForGrid()
		{
			var location = Factory.NewWithValidTestData<GvmsInspectionAtLocationCusCodeData>();
			location.CY_Code = "1";
			location.CY_Data = "L0029A";

			CombineAssertions(() =>
			{
				AssertEquals("GVI", location.CY_Type);
				AssertEquals("1 - CUSTOMS", location.InspectionType);
				AssertEquals("L0029A - Sevington", location.InspectionLocation);
			});
		}

		public void TestLookups()
		{
			var location = Factory.NewWithValidTestData<GvmsInspectionAtLocationCusCodeData>();
			CombineAssertions(() =>
			{
				AssertGreaterThan("TypeList Count", location.Lookups.TypeList.Count, 0);
				AssertGreaterThan("InspectionLocationList Count", location.Lookups.InspectionLocationList.Count, 0);
			});
		}

		public void TestValidationType()
		{
			var location = Factory.NewWithValidTestData<GvmsInspectionAtLocationCusCodeData>();
			AssertType<GvmsInspectionAtLocationCusCodeDataValidation>(location.Validation);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = (factory ?? Factory).NewWithValidTestData<AsycudaManifestHeader>();
			var location = manifestHeader.InspectionLocations.AddNew();
			location.CY_Code = "1";
			location.CY_Data = "L0029A";
			return location;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(null);

		#endregion
	}
}
