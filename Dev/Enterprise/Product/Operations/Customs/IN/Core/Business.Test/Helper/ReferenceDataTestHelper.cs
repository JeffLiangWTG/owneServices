using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IN.Business.Testing;

public sealed class ReferenceDataTestHelper : TestCaseWithFactory
{
	public static void AssertCustomsOfficeCollection(
		Func<ZZRefCusCodeListCombinedCollection> getCollectionForNonAir = null,
		Func<ZZRefCusCodeListCombinedCollection> getCollectionForAir = null)
	{
		var factory = new BusinessObjectFactory();
		RefDataSetupTestHelper.SetupCustomsOfficeData(factory);

		var collection = getCollectionForNonAir?.Invoke();
		if (collection != null)
		{
			collection.Load();
			CombineAssertions("Collection for Non-Air", () =>
			{
				AssertContainsExactElementsInAnyOrder("", new[] { "ABC123" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
				AssertFilterBusinessObjectDefaults(ModuleTextFilter.ComparisonConstants.NotEqual);
			});
		}

		collection = getCollectionForAir?.Invoke();
		if (collection != null)
		{
			collection.Load();
			CombineAssertions("Collection for Air", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "DEF123", "GHI123" }, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
				AssertFilterBusinessObjectDefaults(ModuleTextFilter.ComparisonConstants.Exact);
			});
		}

		void AssertFilterBusinessObjectDefaults(string comparisonOperator)
		{
			var transportModeFiltertDefaults = collection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Where(x => x.FilterName == Universal.Constants.ZZRefCusCodeListFilters.TransportMode).ToList();
			AssertEquals(2, transportModeFiltertDefaults.Count);
			AssertEquals("Transport Mode", RefTransportModeList.Codes.AIR, transportModeFiltertDefaults.Single(x => x.PropertyName == "Property").Value);
			AssertEquals("Comparison Operator", comparisonOperator, transportModeFiltertDefaults.Single(x => x.PropertyName == "ComparisonOperator").Value);
		}
	}

	public static void AssertSupportDocumentTypeCollection(Func<ZZRefCusCodeListCombinedCollection> getCollection)
	{
		var factory = new BusinessObjectFactory();
		RefDataSetupTestHelper.SetupSupportDocumentTypeData(factory);

		var collection = getCollection?.Invoke();
		if (collection != null)
		{
			collection.Load();

			var actualCodes = collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).ToList();

			CombineAssertions("Collection for Supporting Document Types", () =>
			{
				AssertContainsExactElementsInAnyOrder(
					new[] { "DOC001", "DOC002", "DOC003" },
					actualCodes);
			});
		}
	}
}
