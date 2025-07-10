using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.CH.NCTS.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.NCTS.Module.NctsMovementFilterStripBusinessObject;
using static Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.ZString>;
using EUNCTS5DeparturePhaseList = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DeparturePhaseList;
using NctsHeaderCollection = Enterprise.Customs.EU.NCTS.Business.NctsHeaderCollection;

namespace Enterprise.Customs.CH.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
sealed class NctsMovementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestNctsDeclarationTypeList()
	{
		new RefDataTestHelper(Factory).CreateNctsDeclarationTypeList();

		var declarationTypeList = FilterStrip.DeclarationTypeList;
		var expectedList = new ZString[] { "T", "T1", "T2", "T2F", "T-CH" };

		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Elements", expectedList, declarationTypeList.GetAllCodesZString());
			AssertSame("Cached", declarationTypeList, FilterStrip.DeclarationTypeList);
		});
	}

	public void TestDeparturePhaseStatusFilterList()
	{
		var expectedCodes = new EUNCTS5DeparturePhaseList();
		expectedCodes.AddPair("123");
		expectedCodes.AddPair("016");

		var filterStrip = GetNewFilterStripBusinessObject();

		var applicationCodeFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
		applicationCodeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
		applicationCodeFilter.Property = CusInBondApplicationCodeList.Codes.NCTS5;

		var departurePhaseStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.DeparturePhaseStatus];
		AssertContainsExactElementsInAnyOrder(expectedCodes.GetAllCodes(), ((CodeDescriptionPairList)departurePhaseStatusFilter.List).GetAllCodes());
	}

	public void TestActivationDeadline()
	{
		var departure1 = GetNewDeparture();
		departure1.MovementReferenceNumberSetter("mrn1", expiryDate: new ZDateTime(2022, 1, 1, 10, 0, 0));

		var departure2 = GetNewDeparture();
		departure2.MovementReferenceNumberSetter("mrn2", expiryDate: new ZDateTime(2022, 1, 1, 10, 0, 0));

		var departure3 = GetNewDeparture();
		departure3.MovementReferenceNumberSetter("mrn2", expiryDate: new ZDateTime(2021, 2, 16, 15, 30, 0));

		Factory.Save();

		var nctsCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		nctsCollection.Load(FilterStrip.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 3, nctsCollection.Count);

			LoadNctsHeaderCollectionDateFilter("Activation Deadline", new ZDateTime(2022, 1, 1, 10, 0, 0), nctsCollection);

			AssertEquals("Total NCTS Declarations match the filter", 2, nctsCollection.Count);
			AssertEquals("Departure 1 is in the filter cause its Arrival Limit Date is 01/01/2022 10:00:00", true, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is in the filter cause its Arrival Limit Date is 01/01/2022 10:00:00", true, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is not in the filter cause its Arrival Limit Date is not 01/01/2022 10:00:00", false, nctsCollection.Contains(departure3));

			LoadNctsHeaderCollectionDateFilter("Activation Deadline", new ZDateTime(2021, 2, 16, 15, 30, 0), nctsCollection);

			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause its Arrival Limit Date is not 16/02/2021 15:30:00", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause its Arrival Limit Date is not 16/02/2021 15:30:00", false, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is in the filter cause its Arrival Limit Date is 16/02/2021 15:30:00", true, nctsCollection.Contains(departure3));

			LoadNctsHeaderCollectionDateFilter("Activation Deadline", new ZDateTime(2020, 5, 26, 18, 15, 0), nctsCollection);
			AssertEquals("Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause its Arrival Limit Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause its Arrival Limit Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is not in the filter cause its Arrival Limit Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure3));
		});
	}

	public void TestDepartureStatusList()
	{
		var departureStatus = (ModuleTextFilter)FilterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
		var departureStatusList = (CodeDescriptionPairList)departureStatus.List;

		var requiredCodes = new NCTS5DepartureCustomsStatusList().GetAllCodes();
		var missingCodes = requiredCodes.Except(departureStatusList.GetAllCodes());
		AssertContainsExactElementsInAnyOrder("Missing CH codes", Array.Empty<string>(), missingCodes);
	}

	public void TestArrivalStatusList()
	{
		var arrivalStatus = (ModuleTextFilter)FilterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
		var arrivalStatusList = (CodeDescriptionPairList)arrivalStatus.List;

		var requiredCodes = new NCTS5ArrivalCustomsStatusList().GetAllCodes();
		var missingCodes = requiredCodes.Except(arrivalStatusList.GetAllCodes());
		AssertContainsExactElementsInAnyOrder("Missing CH codes", Array.Empty<string>(), missingCodes);
	}

	public void TestArrivalMRNFilter() => CombineAssertions(() =>
	{
		var arrivalFilter = (ModuleTextFilter)FilterStrip[CHFilterConstants.ArrivalMRNATOReference];
		AssertEquals("MultilingualDescription", "Arrival MRN/Additional Transit Ref.#", arrivalFilter.MultilingualDescription);
		AssertEquals("Category", FilterCategories.NumbersAndReferences, arrivalFilter.Category);

		var arrival1 = GetNewArrival();
		var arrival2 = GetNewArrival();
		arrival2.ArrivalMovementHeader.MovementReferenceNumbers.AddNew().CSI_ReferenceNumber = "REF2";
		var arrival3 = GetNewArrival();
		arrival3.ArrivalMovementHeader.AdditionalTransitOperations.AddNew().CSI_ReferenceNumber = "REF3";
		Factory.Save();

		arrivalFilter.IsActive = true;

		AssertFilterMatch("Neither MRN nor ATO", false, arrival1, "REF2");
		AssertFilterMatch("Neither MRN nor ATO", false, arrival1, "REF3");

		AssertFilterMatch("Has MRN with Reference=REF2", false, arrival2, "REF1");
		AssertFilterMatch("Has MRN with Reference=REF2", true, arrival2, "REF2");
		AssertFilterMatch("Has MRN with Reference=MEF2", false, arrival2, "REF3");

		AssertFilterMatch("Has ATO with Reference=REF3", false, arrival3, "REF1");
		AssertFilterMatch("Has ATO with Reference=REF3", false, arrival3, "REF2");
		AssertFilterMatch("Has ATO with Reference=REF3", true, arrival3, "REF3");

		void AssertFilterMatch(string assertionMessage, bool expectedMatch, NctsHeader arrival, string property)
		{
			arrivalFilter.Property = property;
			AssertEquals($"{assertionMessage} Property={property}", expectedMatch, arrival.MatchesFilter(FilterStrip.Filter));
		}
	});

	NctsHeader GetNewDeparture()
	{
		var departure = Factory.New<NctsHeader>();
		departure.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		return departure;
	}

	NctsHeader GetNewArrival()
	{
		var departure = Factory.New<NctsHeader>();
		departure.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		return departure;
	}

	void LoadNctsHeaderCollectionDateFilter(ZString filterField, ZDateTime filterValue, NctsHeaderCollection nctsHeaderCollection)
	{
		var filter = (ModuleDateFilter)FilterStrip[filterField];
		filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		filter.Property1 = filterValue;
		filter.Property2 = filterValue;
		filter.IsActive = true;
		nctsHeaderCollection.Load(FilterStrip.Filter);
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NctsMovementFilterStripBusinessObject();

	NctsMovementFilterStripBusinessObject FilterStrip => filterStrip ??= CreateFilterStrip();
	NctsMovementFilterStripBusinessObject filterStrip;

	NctsMovementFilterStripBusinessObject CreateFilterStrip()
	{
		var filterStrip = new NctsMovementFilterStripBusinessObject();
		var applicationCode = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
		applicationCode.ComparisonOperator = ComparisonConstants.Exact;
		applicationCode.Property = CusInBondApplicationCodeList.Codes.NCTS5;
		return filterStrip;
	}
}
