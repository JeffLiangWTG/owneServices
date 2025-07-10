using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RouteEntry))]
	sealed class RouteEntryTest : Customs.Business.Testing.CusCodeDataTest<RouteEntry>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Route Entry", Factory.New<RouteEntry>().HumanReadableName);
		}

		public void TestDefaultValues()
		{
			var entry = Factory.New<RouteEntry>();
			AssertEquals(CusCodeDataTypeList.Codes.EUICS2RouteEntry, entry.CY_Type);
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

		public void TestCountryName()
		{
			var entry = Factory.New<RouteEntry>();
			Assert("Country name is empty when country code is empty", entry.CountryName.IsEmpty);

			entry.CY_Code = "AUSYD";
			AssertEquals("Country name is Australia when country code is AU", "Australia", entry.CountryName);
		}

		public void TestCY_Data()
		{
			var entry = Factory.New<RouteEntry>();
			Assert(entry.CY_DataInfo.ReadOnly);
		}

		public void TestIsInEU()
		{
			var entry = Factory.New<RouteEntry>();
			Assert("Default to false", !entry.IsInEU);

			entry.CY_Code = "AUSYD";
			Assert(!entry.IsInEU);

			entry.CY_Code = "IEADA";
			Assert(entry.IsInEU);
		}

		public void TestISynchroniserReadOnlyMembersProvider()
		{
			ISynchroniserReadOnlyMembersProvider entry = Factory.New<RouteEntry>();
			AssertArrayEqualsByElements(new [] { RouteEntry.Schema.CY_Order, RouteEntry.Schema.CY_Code, RouteEntry.Schema.CY_Data }, entry.SynchroniserReadOnlyMembers.ToArray());
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
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			return header.Itinerary.AddNew();
		}

		#endregion
	}
}
