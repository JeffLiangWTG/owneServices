using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
class NctsMovementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestNctsTransitStatusList()
	{
		var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
		mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
		mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);
		using (ObjectFactory.Substitute(mockSettings.Object))
		{
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var departureStatusFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
			var departureStatusFilterCodeList = (CodeDescriptionPairList)departureStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("DepartureStatus", new NctsTransitStatusList().GetAllCodes(), departureStatusFilterCodeList.GetAllCodes());
			var arrivalStatusFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			var arrivalStatusFilterCodeList = (CodeDescriptionPairList)arrivalStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("ArrivalStatus", new NctsTransitStatusList().GetAllCodes(), arrivalStatusFilterCodeList.GetAllCodes());
		}
	}

	public void TestNctsStatusList_Phase5() => CombineAssertions(() =>
	{
		var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
		mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(true);
		using (ObjectFactory.Substitute(mockSettings.Object))
		{
			var filterStrip = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterApplicationCode = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterApplicationCode.Property = ZString.Empty;

			var departureStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
			var departureStatusFilterCodeList = (CodeDescriptionPairList)departureStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Departure when application code is empty", AddNcst4Codes(new ESNCTS5DepartureCustomsStatusList()).GetAllCodes(), departureStatusFilterCodeList.GetAllCodes());

			var arrivalStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			var arrivalStatusFilterCodeList = (CodeDescriptionPairList)arrivalStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Arrival when application code is empty", AddNcst4Codes(new ESNCTS5ArrivalCustomsStatusList()).GetAllCodes(), arrivalStatusFilterCodeList.GetAllCodes());

			textFilterApplicationCode.Property = CusInBondApplicationCodeList.Codes.NCTS5;

			departureStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
			departureStatusFilterCodeList = (CodeDescriptionPairList)departureStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Departure when application code is NCTS5", new ESNCTS5DepartureCustomsStatusList().GetAllCodes(), departureStatusFilterCodeList.GetAllCodes());

			arrivalStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			arrivalStatusFilterCodeList = (CodeDescriptionPairList)arrivalStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Arrival when application code is NCTS5", new ESNCTS5ArrivalCustomsStatusList().GetAllCodes(), arrivalStatusFilterCodeList.GetAllCodes());
		}

		static CodeDescriptionPairList AddNcst4Codes(CodeDescriptionPairList codeList)
		{
			codeList.AddRange(new NctsTransitStatusList());
			return codeList;
		}
	});

	public void TestNctsDeparturePhaseStatusList_Phase5()
	{
		var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
		var expectedCodeList = new EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DeparturePhaseList().GetAllCodes()
			.Concat(new []
			{
				ESNctsMovementHeaderTransactionStatusList.Codes.Annexes,
				ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData,
			});
		using (ObjectFactory.Substitute(mockSettings.Object))
		{
			var filterStrip = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterApplicationCode = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterApplicationCode.Property = CusInBondApplicationCodeList.Codes.NCTS5;

			var departurePhaseStatusFilter = (ModuleTextFilter)filterStrip[NctsMovementFilterStripBusinessObject.FilterConstants.DeparturePhaseStatus];
			var departurePhaseStatusFilterCodeList = (CodeDescriptionPairList)departurePhaseStatusFilter.List;
			AssertContainsExactElementsInAnyOrder("Phase status list contains ES phase statuses", expectedCodeList, departurePhaseStatusFilterCodeList.GetAllCodes());
		}
	}

	public void TestBrokerFilter()
	{
		var departure1 = GetNewDeparture();
		departure1.MovementHeader.BM_GS_NKCusAgent = "AG1";

		var departure2 = GetNewDeparture();
		departure2.MovementHeader.BM_GS_NKCusAgent = "AG2";

		var arrival = GetNewArrival();
		arrival.ArrivalMovementHeader.BM_GS_NKCusAgent = "AG1";

		Factory.Save();

		var nctsCollection = new EU.NCTS.Business.NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		nctsCollection.Load(stripBO.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 3, nctsCollection.Count);

			LoadNCTSHeaderCollectionNkFilter("Customs Broker", "AG1", nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 2, nctsCollection.Count);
			AssertEquals("Departure 1 is in the filter cause has AG1 as Customs Broker", true, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause has not AG1 as Customs Broker", false, nctsCollection.Contains(departure2));
			AssertEquals("Arrival is in the filter cause has AG1 as Customs Broker", true, nctsCollection.Contains(arrival));

			LoadNCTSHeaderCollectionNkFilter("Customs Broker", "AG2", nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause has not AG2 as Customs Broker", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is in the filter cause has AG2 as Customs Broker", true, nctsCollection.Contains(departure2));
			AssertEquals("Arrival is not in the filter cause has not AG2 as Customs Broker", false, nctsCollection.Contains(arrival));

			LoadNCTSHeaderCollectionNkFilter("Customs Broker", "AG3", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause has not AG3 as Customs Broker", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause has not AG3 as Customs Broker", false, nctsCollection.Contains(departure2));
			AssertEquals("Arrival is not in the filter cause has not AG3 as Customs Broker", false, nctsCollection.Contains(arrival));
		});
	}

	public void TestClearanceDateFilter()
	{
		var departure1 = GetNewDeparture();
		departure1.ClearanceEntryNumber.CE_IssueDate = new ZDateTime(2022, 1, 1, 10, 0, 0);

		var departure2 = GetNewDeparture();
		departure2.ClearanceEntryNumber.CE_IssueDate = new ZDateTime(2022, 1, 1, 10, 0, 0);

		var departure3 = GetNewDeparture();
		departure3.ClearanceEntryNumber.CE_IssueDate = new ZDateTime(2021, 2, 16, 15, 30, 0);

		Factory.Save();

		var nctsCollection = new EU.NCTS.Business.NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		nctsCollection.Load(stripBO.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 3, nctsCollection.Count);

			LoadNctsHeaderCollectionDateFilter("Dep. Clearance Date", new ZDateTime(2022, 1, 1, 10, 0, 0), nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 2, nctsCollection.Count);
			AssertEquals("Departure 1 is in the filter cause its Clearance Date is 01/01/2022 10:00:00", true, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is in the filter cause its Clearance Date is 01/01/2022 10:00:00", true, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is not in the filter cause its Clearance Date is not 01/01/2022 10:00:00", false, nctsCollection.Contains(departure3));

			LoadNctsHeaderCollectionDateFilter("Dep. Clearance Date", new ZDateTime(2021, 2, 16, 15, 30, 0), nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause its Clearance Date is not 16/02/2021 15:30:00", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause its Clearance Date is not 16/02/2021 15:30:00", false, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is in the filter cause its Clearance Date is 16/02/2021 15:30:00", true, nctsCollection.Contains(departure3));

			LoadNctsHeaderCollectionDateFilter("Dep. Clearance Date", new ZDateTime(2020, 5, 26, 18, 15, 0), nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause its Clearance Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause its Clearance Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is not in the filter cause its Clearance Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure3));
		});
	}

	public void TestArrivalLimitDateFilter()
	{
		var departure1 = GetNewDeparture();
		departure1.ClearanceEntryNumber.CE_ExpiryDate = new ZDateTime(2022, 1, 1, 10, 0, 0);

		var departure2 = GetNewDeparture();
		departure2.ClearanceEntryNumber.CE_ExpiryDate = new ZDateTime(2022, 1, 1, 10, 0, 0);

		var departure3 = GetNewDeparture();
		departure3.ClearanceEntryNumber.CE_ExpiryDate = new ZDateTime(2021, 2, 16, 15, 30, 0);

		Factory.Save();

		var nctsCollection = new EU.NCTS.Business.NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		nctsCollection.Load(stripBO.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 3, nctsCollection.Count);

			LoadNctsHeaderCollectionDateFilter("Arrival Limit Date", new ZDateTime(2022, 1, 1, 10, 0, 0), nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 2, nctsCollection.Count);
			AssertEquals("Departure 1 is in the filter cause its Arrival Limit Date is 01/01/2022 10:00:00", true, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is in the filter cause its Arrival Limit Date is 01/01/2022 10:00:00", true, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is not in the filter cause its Arrival Limit Date is not 01/01/2022 10:00:00", false, nctsCollection.Contains(departure3));

			LoadNctsHeaderCollectionDateFilter("Arrival Limit Date", new ZDateTime(2021, 2, 16, 15, 30, 0), nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause its Arrival Limit Date is not 16/02/2021 15:30:00", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause its Arrival Limit Date is not 16/02/2021 15:30:00", false, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is in the filter cause its Arrival Limit Date is 16/02/2021 15:30:00", true, nctsCollection.Contains(departure3));

			LoadNctsHeaderCollectionDateFilter("Arrival Limit Date", new ZDateTime(2020, 5, 26, 18, 15, 0), nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("Departure 1 is not in the filter cause its Arrival Limit Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure1));
			AssertEquals("Departure 2 is not in the filter cause its Arrival Limit Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure2));
			AssertEquals("Departure 3 is not in the filter cause its Arrival Limit Date is not 26/05/2020 18:15:00", false, nctsCollection.Contains(departure3));
		});
	}

	public void TestArrivalSummaryDeclarationNumberFilter()
	{
		var arrival1 = GetNewArrival();
		SetSummaryDeclaration(arrival1, "TestSummary1");

		var arrival2 = GetNewArrival();
		SetSummaryDeclaration(arrival2, "TestSummary1");

		var arrival3 = GetNewArrival();
		SetSummaryDeclaration(arrival3, "TestSummary2");

		Factory.Save();

		var nctsCollection = new EU.NCTS.Business.NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		nctsCollection.Load(stripBO.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 3, nctsCollection.Count);

			LoadNCTSHeaderCollectionTextFilter("Arrival Summary Declaration Number", "TestSummary1", nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 2, nctsCollection.Count);
			AssertEquals("Arrival 1 is in the filter cause its Summary Declaration Number is TestSummary1", true, nctsCollection.Contains(arrival1));
			AssertEquals("Arrival 2 is in the filter cause its Summary Declaration Number is TestSummary1", true, nctsCollection.Contains(arrival2));
			AssertEquals("Arrival 3 is not in the filter cause its Summary Declaration Number is not TestSummary1", false, nctsCollection.Contains(arrival3));

			LoadNCTSHeaderCollectionTextFilter("Arrival Summary Declaration Number", "TestSummary2", nctsCollection, stripBO);

			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("Arrival 1 is not in the filter cause its Summary Declaration Number is not TestSummary2", false, nctsCollection.Contains(arrival1));
			AssertEquals("Arrival 2 is not in the filter cause its Summary Declaration Number is not TestSummary2", false, nctsCollection.Contains(arrival2));
			AssertEquals("Arrival 3 is in the filter cause its Summary Declaration Number is TestSummary2", true, nctsCollection.Contains(arrival3));

			LoadNCTSHeaderCollectionTextFilter("Arrival Summary Declaration Number", "TestSummary3", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("Arrival 1 is not in the filter cause its Summary Declaration Number is not TestSummary3", false, nctsCollection.Contains(arrival1));
			AssertEquals("Arrival 2 is not in the filter cause its Summary Declaration Number is not TestSummary3", false, nctsCollection.Contains(arrival2));
			AssertEquals("Arrival 3 is not in the filter cause its Summary Declaration Number is not TestSummary3", false, nctsCollection.Contains(arrival3));
		});

		void SetSummaryDeclaration(NctsHeader declaration, ZString summaryEntryNumber)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = summaryEntryNumber;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}
	}

	public void TestDepartureGoodsLocationFilter()
	{
		var departure1 = GetNewDeparture();
		departure1.MovementHeader.BM_LocationOfGoodsCode = "Location1";
		departure1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		var departure2 = GetNewDeparture();
		departure2.MovementHeader.BM_LocationOfGoodsCode = "Location2";
		departure2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		var departure3 = GetNewDeparture();
		departure3.CusGoodsLocation.CGL_AdditionalIdentifier = "Location1";
		departure3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var departure4 = GetNewDeparture();
		departure4.CusGoodsLocation.CGL_AdditionalIdentifier = "Location3";
		departure4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var arrival1 = GetNewArrival();
		arrival1.ArrivalMovementHeader.BM_LocationOfGoodsCode = "Location1";
		arrival1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		var arrival2 = GetNewArrival();
		arrival2.CusGoodsLocation.CGL_AdditionalIdentifier = "Location1";

		Factory.Save();

		var nctsCollection = new EU.NCTS.Business.NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		nctsCollection.Load(stripBO.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 6, nctsCollection.Count);

			LoadNCTSHeaderApplicationCodeFilter(stripBO, true);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.DepartureGoodsLocation, "Location1", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Departure 1 is in the filter cause its BM_LocationOfGoodsCode is Location1", true, nctsCollection.Contains(departure1));
			AssertEquals("NCTS4: Departure 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(departure2));
			AssertEquals("NCTS5: Departure 3 is not in the filter cause its CusGoodsLocation.CGL_AdditionalIdentifier is Location1", false, nctsCollection.Contains(departure3));
			AssertEquals("NCTS5: Departure 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(departure4));
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS5: Arrival 2 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, false);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.DepartureGoodsLocation, "Location1", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Departure 1 is mot in the filter cause its BM_LocationOfGoodsCode is Location1", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS4: Departure 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(departure2));
			AssertEquals("NCTS5: Departure 3 is in the filter cause its CusGoodsLocation.CGL_AdditionalIdentifier is Location1", true, nctsCollection.Contains(departure3));
			AssertEquals("NCTS5: Departure 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(departure4));
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS5: Arrival 2 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, true);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.DepartureGoodsLocation, "Location2", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location2", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS4: Departure 2 is in the filter cause its BM_LocationOfGoodsCode is Location2", true, nctsCollection.Contains(departure2));
			AssertEquals("NCTS5: Departure 3 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location2", false, nctsCollection.Contains(departure3));
			AssertEquals("NCTS5: Departure 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location2", false, nctsCollection.Contains(departure4));
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS5: Arrival 2 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, false);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.DepartureGoodsLocation, "Location3", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location3", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS4: Departure 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location3", false, nctsCollection.Contains(departure2));
			AssertEquals("NCTS5: Departure 3 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location3", false, nctsCollection.Contains(departure3));
			AssertEquals("NCTS5: Departure 4 is in the filter cause its CusGoodsLocation.CGL_AdditionalIdentifier is Location3", true, nctsCollection.Contains(departure4));
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS5: Arrival 2 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, true);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.DepartureGoodsLocation, "Location4", nctsCollection, stripBO);
			AssertEquals("NCTS4: Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS4: Departure 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(departure2));
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival1));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, false);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.DepartureGoodsLocation, "Location4", nctsCollection, stripBO);
			AssertEquals("NCTS5: Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("NCTS5: Departure 3 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(departure3));
			AssertEquals("NCTS5: Departure 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(departure4));
			AssertEquals("NCTS5: Arrival 2 is not in the filter cause its an Arrival", false, nctsCollection.Contains(arrival2));
		});
	}

	public void TestArrivalGoodLocationFilter()
	{
		var arrival1 = GetNewArrival();
		arrival1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		arrival1.ArrivalMovementHeader.BM_LocationOfGoodsCode = "Location1";

		var arrival2 = GetNewArrival();
		arrival2.ArrivalMovementHeader.BM_LocationOfGoodsCode = "Location2";
		arrival2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		var arrival3 = GetNewArrival();
		arrival3.CusGoodsLocation.CGL_AdditionalIdentifier = "Location1";
		arrival3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var arrival4 = GetNewArrival();
		arrival4.CusGoodsLocation.CGL_AdditionalIdentifier = "Location3";
		arrival4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var departure1 = GetNewDeparture();
		departure1.MovementHeader.BM_LocationOfGoodsCode = "Location1";
		departure1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		var departure2 = GetNewDeparture();
		departure2.CusGoodsLocation.CGL_AdditionalIdentifier = "Location1";
		departure2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		Factory.Save();

		var nctsCollection = new EU.NCTS.Business.NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		nctsCollection.Load(stripBO.Filter);

		CombineAssertions(() =>
		{
			AssertEquals("[PRECONDITION] Total NCTS Declarations", 6, nctsCollection.Count);

			LoadNCTSHeaderApplicationCodeFilter(stripBO, true);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalGoodsLocation, "Location1", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Arrival 1 is in the filter cause its BM_LocationOfGoodsCode is Location1", true, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS4: Arrival 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(arrival2));
			AssertEquals("NCTS5: Arrival 3 is not in the filter cause its CusGoodsLocation.CGL_AdditionalIdentifier is Location1", false, nctsCollection.Contains(arrival3));
			AssertEquals("NCTS5: Arrival 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(arrival4));
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS5: Departure 2 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, false);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalGoodsLocation, "Location1", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its BM_LocationOfGoodsCode is Location1", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS4: Arrival 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(arrival2));
			AssertEquals("NCTS5: Arrival 3 is in the filter cause its CusGoodsLocation.CGL_AdditionalIdentifier is Location1", true, nctsCollection.Contains(arrival3));
			AssertEquals("NCTS5: Arrival 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location1", false, nctsCollection.Contains(arrival4));
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS5: Departure 2 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, true);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalGoodsLocation, "Location2", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location2", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS4: Arrival 2 is in the filter cause its BM_LocationOfGoodsCode is Location2 and NCTS4", true, nctsCollection.Contains(arrival2));
			AssertEquals("NCTS5: Arrival 3 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location2", false, nctsCollection.Contains(arrival3));
			AssertEquals("NCTS5: Arrival 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location2", false, nctsCollection.Contains(arrival4));
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS5: Departure 2 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, false);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalGoodsLocation, "Location3", nctsCollection, stripBO);
			AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location3", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS4: Arrival 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location3", false, nctsCollection.Contains(arrival2));
			AssertEquals("NCTS5: Arrival 3 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location3", false, nctsCollection.Contains(arrival3));
			AssertEquals("NCTS5: Arrival 4 is in the filter cause its CusGoodsLocation.CGL_AdditionalIdentifier is Location3 and NCTS5", true, nctsCollection.Contains(arrival4));
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure1));
			AssertEquals("NCTS5: Departure 2 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure2));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, true);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalGoodsLocation, "Location4", nctsCollection, stripBO);
			AssertEquals("NCTS4: Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("NCTS4: Arrival 1 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(arrival1));
			AssertEquals("NCTS4: Arrival 2 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(arrival2));
			AssertEquals("NCTS4: Departure 1 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure1));

			LoadNCTSHeaderApplicationCodeFilter(stripBO, false);
			LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalGoodsLocation, "Location4", nctsCollection, stripBO);
			AssertEquals("NCTS5: Total NCTS Declarations match the filter", 0, nctsCollection.Count);
			AssertEquals("NCTS5: Arrival 3 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(arrival3));
			AssertEquals("NCTS5: Arrival 4 is not in the filter cause its BM_LocationOfGoodsCode and CusGoodsLocation.CGL_AdditionalIdentifier are not Location4", false, nctsCollection.Contains(arrival4));
			AssertEquals("NCTS5: Departure 2 is not in the filter cause its a Departure", false, nctsCollection.Contains(departure2));
		});
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NctsMovementFilterStripBusinessObject();

	NctsHeader GetNewDeparture()
	{
		var departure = Factory.New<NctsHeader>();
		departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return departure;
	}

	NctsHeader GetNewArrival()
	{
		var arrival = Factory.New<NctsHeader>();
		arrival.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		return arrival;
	}

	void LoadNCTSHeaderCollectionNkFilter(ZString filterField, ZString filterValue, EU.NCTS.Business.NctsHeaderCollection nctsHeaderCollection, NctsMovementFilterStripBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
	{
		var filter = (ModuleNkFilter)stripBO[filterField];
		filter.Property = filterValue;
		filter.ComparisonOperator = comparisonOperator;
		filter.IsActive = true;
		nctsHeaderCollection.Load(stripBO.Filter);
	}

	void LoadNctsHeaderCollectionDateFilter(ZString filterField, ZDateTime filterValue, EU.NCTS.Business.NctsHeaderCollection nctsHeaderCollection, NctsMovementFilterStripBusinessObject stripBO)
	{
		var filter = (ModuleDateFilter)stripBO[filterField];
		filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		filter.Property1 = filterValue;
		filter.Property2 = filterValue;
		filter.IsActive = true;
		nctsHeaderCollection.Load(stripBO.Filter);
	}

	void LoadNCTSHeaderCollectionTextFilter(ZString filterField, ZString filterValue, EU.NCTS.Business.NctsHeaderCollection nctsHeaderCollection, NctsMovementFilterStripBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact)
	{
		var textFilterStatus = (ModuleTextFilter)stripBO[filterField];
		textFilterStatus.ComparisonOperator = comparisonOperator;
		textFilterStatus.Property = filterValue;
		textFilterStatus.IsActive = true;
		nctsHeaderCollection.Load(stripBO.Filter);
	}

	void LoadNCTSHeaderApplicationCodeFilter(NctsMovementFilterStripBusinessObject stripBO, bool isNcts4)
	{
		var applicationCodeFilter = (ModuleTextFilter)stripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
		applicationCodeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
		applicationCodeFilter.IsActive = true;
		applicationCodeFilter.Property = isNcts4 ? CusInBondApplicationCodeList.Codes.NCTS4 : CusInBondApplicationCodeList.Codes.NCTS5;
	}
}
