using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebRefVesselFilterBusinessObject))]
	sealed class WebRefVesselFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		#region TestFilterDoesNotReturnInactiveCodes

		public void TestFilterDoesNotReturnInactiveVessels()
		{
			TestCaseHelper.ClearTable(RefVesselSchema.Constants.TableName);

			testVessel.RV_IsActive = false;
			RefVessel vessel1 = CreateVessel(false);
			RefVessel vessel2 = CreateVessel(false);
			RefVessel vessel3 = CreateVessel(false);
			RefVessel vessel4 = CreateVessel(true);
			RefVessel vessel5 = CreateVessel(true);
			RefVessel vessel6 = CreateVessel(true);

			RefVesselCollection collection = new RefVesselCollection(Factory);
			collection.AdditionalFilter = RefVesselFilterBO.Filter;

			AssertEquals("Collection should contain 3 vessels", 3, collection.Count);
			Assert("Collection should contain Vessel2", collection.Contains(vessel4));
			Assert("Collection should contain Vessel4", collection.Contains(vessel5));
			Assert("Collection should contain Vessel6", collection.Contains(vessel6));
		}

		RefVessel CreateVessel(bool isActive)
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_IsActive = isActive;
			return vessel;
		}

		#endregion

		WebRefVesselFilterBusinessObject RefVesselFilterBO;
		RefVessel testVessel;

		protected override void SetUp()
		{
			base.SetUp();
			testVessel = Factory.New<RefVessel>();
			testVessel.RV_Code = "TST";
			Factory.Save();
			RefVesselFilterBO = (WebRefVesselFilterBusinessObject)GetNewBusinessObject();
		}

		public void TestCommodityTypeFiltering()
		{
			RefVesselFilterBO.RV_Code = "ABC";
			ZQuery filter = RefVesselFilterBO.Filter;
			filter.FetchOnlyFromLocalCache = true;

			RefVesselCollection testVessels = new RefVesselCollection(Factory, filter);
			AssertEquals("Vessels count", 0, testVessels.Count);

			RefVesselFilterBO.RV_Code = "TST";
			ZQuery filter1 = RefVesselFilterBO.Filter;
			filter1.FetchOnlyFromLocalCache = true;

			testVessels = new RefVesselCollection(Factory, filter1);
			AssertEquals("Vessels count", 1, testVessels.Count);
		}
	}
}
