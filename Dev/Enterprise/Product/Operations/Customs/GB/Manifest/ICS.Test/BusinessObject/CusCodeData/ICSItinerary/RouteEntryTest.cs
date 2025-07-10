using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	[TestedType(typeof(RouteEntry))]
	public class RouteEntryTest : Customs.Business.Testing.CusCodeDataTest<RouteEntry>
	{
		public void TestDefaultValues()
		{
			var entry = Factory.New<RouteEntry>();
			AssertEquals(CusCodeDataTypeList.Codes.IcsRouteEntry, entry.CY_Type);
		}

		public void TestLookups()
		{
			var entry = Factory.New<RouteEntry>();
			AssertEquals("Lookups", typeof(RouteEntryLookups), entry.Lookups.GetType());
		}

		public void TestValidation()
		{
			var entry = Factory.New<RouteEntry>();
			AssertEquals("Validation", typeof(RouteEntryValidation), entry.Validation.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override IEnumerable<RouteEntry> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (RouteEntry)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Itinerary.AddNew();
		}

		#endregion
	}
}
