using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Module.Testing
{
	[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
	sealed class NctsMovementFilterStripBusinessObjectBaseOnlyTest : FilterStripBusinessObjectTestCase
	{
		public void TestApplicationCodeFilter() => CombineAssertions(() =>
		{
			var departure1 = GetNewDeparture();
			departure1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var departure2 = GetNewDeparture();
			departure2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrival1 = GetNewArrival();
			arrival1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var arrival2 = GetNewArrival();
			arrival2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter =
				(ModuleTextFilter)filterStripBO[
					NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator =
				ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilter.IsActive = true;
			AssertEquals("MultilingualDescription", "Application Code", textFilter.MultilingualDescription.ToString());
			AssertEquals("Category", FilterCategories.ModesAndTypes, textFilter.Category);
			AssertEquals("ComparisonOperator_List", "exact", textFilter.ComparisonOperator_List.CodesAsString);
			AssertEquals("Value List", "NCT, NC5", ((CodeDescriptionPairList)textFilter.List).CodesAsString);

			textFilter.Property = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals($"{textFilter.Property}: departure1 does not match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{textFilter.Property}: departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{textFilter.Property}: arrival1 does  match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{textFilter.Property}: arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals($"{textFilter.Property}: departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{textFilter.Property}: departure2 does not match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{textFilter.Property}: arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{textFilter.Property}: arrival2 does  match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));

			AssertPropertyDefault(false, CusInBondApplicationCodeList.Codes.NCTS4);
			AssertPropertyDefault(true, CusInBondApplicationCodeList.Codes.NCTS5);

			void AssertPropertyDefault(bool isUsingPhase5, string expectedDefaultValue)
			{
				using (SubstituteNctsSettings(isUsingPhase5))
				{
					var filterStripBO1 = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
					var textFilterApplicationCode = (ModuleTextFilter)filterStripBO1[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
					AssertEquals($"Initial Property when isUsingPhase5={isUsingPhase5}", expectedDefaultValue, textFilterApplicationCode.Property);
					AssertEquals($"DefaultProperty when isUsingPhase5={isUsingPhase5}", expectedDefaultValue, textFilterApplicationCode.DefaultProperty);
				}
			}
		});

		public void TestDepartureStatusFilter_Phase4()
		{
			using (SubstituteNctsSettings(false))
			{
				AssertDepartureStatusFilterCodeList(null, DepartureStatusCodesPhase4);
				AssertDepartureStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS4, DepartureStatusCodesPhase4);
				AssertDepartureStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS5, DepartureStatusCodesPhase5);
				AssertDepartureStatusFilterCodeList(ZString.Empty, DepartureStatusCodesPhase5 + ", " + DepartureStatusCodesPhase4);
				AssertDepartureStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsTransitStatusList.Codes.RequestForAmendment, NctsTransitStatusList.Codes.DeclarationAccepted);
				AssertDepartureStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
			}
		}

		public void TestDepartureStatusFilter_Phase5()
		{
			AssertDepartureStatusFilterCodeList(null, DepartureStatusCodesPhase5);
			AssertDepartureStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS4, DepartureStatusCodesPhase4);
			AssertDepartureStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS5, DepartureStatusCodesPhase5);
			AssertDepartureStatusFilterCodeList(ZString.Empty, DepartureStatusCodesPhase5 + ", " + DepartureStatusCodesPhase4);
			AssertDepartureStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsTransitStatusList.Codes.RequestForAmendment, NctsTransitStatusList.Codes.DeclarationAccepted);
			AssertDepartureStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl);
		}

		const string DepartureStatusCodesPhase4 = "ARR, DAC, DCA, DDR, EXP, DGN, INI, DMA, DRJ, DNR, DRL, ART, DCC, AWO, R4A, , AUP";
		const string DepartureStatusCodesPhase5 = "ACS, ACK, CO2, AMR, CAR, CAN, CO1, DIS, WRO, GIV, INC, CO3, MRN, NRL, PRE, RJO, REL, RFR, RFA, ENQ, URP";

		void AssertDepartureStatusFilterCodeList(string applicationCode, string expectedValueList)
		{
			var assertionInfo = $"ApplicationNode={applicationCode ?? "default"}";

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			if (applicationCode != null)
			{
				var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
				textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				textFilterApplicationCode.Property = applicationCode;
			}

			var textFilterStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
			textFilterStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterStatus.IsActive = true;

			AssertEquals($"{assertionInfo} - MultilingualDescription", "Departure Status", textFilterStatus.MultilingualDescription.ToString());
			AssertEquals($"{assertionInfo} - Category", FilterCategories.ModesAndTypes, textFilterStatus.Category);
			AssertEquals($"{assertionInfo} - ComparisonOperator_List", "exact, is blank, is not blank", textFilterStatus.ComparisonOperator_List.CodesAsString);
			AssertEquals($"{assertionInfo} - Value List", expectedValueList, ((CodeDescriptionPairList)textFilterStatus.List).CodesAsString);
		}

		void AssertDepartureStatusFilter(string applicationCode, string status1, string status2) => CombineAssertions(() =>
		{
			var assertionInfo = $"ApplicationNode={applicationCode}";

			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_CustomsStatus = status1;
			departure1.BH_ApplicationCode = applicationCode;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_CustomsStatus = status2;
			departure2.BH_ApplicationCode = applicationCode;
			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.BM_CustomsStatus = status1;
			arrival1.BH_ApplicationCode = applicationCode;
			var arrival2 = GetNewArrival();
			arrival2.ArrivalMovementHeader.BM_CustomsStatus = status2;
			arrival2.BH_ApplicationCode = applicationCode;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterApplicationCode.Property = applicationCode;

			var textFilterStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureStatus];
			textFilterStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterStatus.IsActive = true;

			textFilterStatus.Property = status1;
			AssertEquals($"{assertionInfo} - departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilterStatus.Property = status2;
			AssertEquals($"{assertionInfo} - departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
		});

		public void TestArrivalStatusFilter_Phase4() => CombineAssertions(() =>
		{
			using (SubstituteNctsSettings(false))
			{
				AssertArrivalStatusFilterCodeList(null, ArrivalStatusCodesPhase4);
				AssertArrivalStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS4, ArrivalStatusCodesPhase4);
				AssertArrivalStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS5, ArrivalStatusCodesPhase5);
				AssertArrivalStatusFilterCodeList(ZString.Empty, ArrivalStatusCodesPhase5 + ", " + ArrivalStatusCodesPhase4);
				AssertArrivalStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsTransitStatusList.Codes.UnloadingPermissionGranted, NctsTransitStatusList.Codes.ArrivalRejected);
				AssertArrivalStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease);
			}
		});

		public void TestArrivalStatusFilter_Phase5() => CombineAssertions(() =>
		{
			AssertArrivalStatusFilterCodeList(null, ArrivalStatusCodesPhase5);
			AssertArrivalStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS4, ArrivalStatusCodesPhase4);
			AssertArrivalStatusFilterCodeList(CusInBondApplicationCodeList.Codes.NCTS5, ArrivalStatusCodesPhase5);
			AssertArrivalStatusFilterCodeList(ZString.Empty, ArrivalStatusCodesPhase5 + ", " + ArrivalStatusCodesPhase4);
			AssertArrivalStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsTransitStatusList.Codes.UnloadingPermissionGranted, NctsTransitStatusList.Codes.ArrivalRejected);
			AssertArrivalStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease);
		});

		const string ArrivalStatusCodesPhase4 = "ARR, DAC, DCA, DDR, EXP, DGN, INI, DMA, DRJ, DNR, DRL, ART, DCC, AWO, R4A, , AUP";
		const string ArrivalStatusCodesPhase5 = "CAN, CL1, CL3, DIS, CD4, CD2, RFR, RFA, UCN, URP, UAP, ULR";

		void AssertArrivalStatusFilterCodeList(string applicationCode, string expectedValueList)
		{
			var assertionInfo = $"ApplicationNode={applicationCode ?? "default"}";

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			if (applicationCode != null)
			{
				var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
				textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				textFilterApplicationCode.Property = applicationCode;
			}

			var textFilterStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			textFilterStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterStatus.IsActive = true;

			AssertEquals($"{assertionInfo} - MultilingualDescription", "Arrival Status", textFilterStatus.MultilingualDescription.ToString());
			AssertEquals($"{assertionInfo} - Category", FilterCategories.ModesAndTypes, textFilterStatus.Category);
			AssertEquals($"{assertionInfo} - ComparisonOperator_List", "exact, is blank, is not blank", textFilterStatus.ComparisonOperator_List.CodesAsString);
			AssertEquals($"{assertionInfo} - Value List", expectedValueList, ((CodeDescriptionPairList)textFilterStatus.List).CodesAsString);
		}

		void AssertArrivalStatusFilter(string applicationCode, string status1, string status2)
		{
			var assertionInfo = $"ApplicationNode={applicationCode}";

			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_CustomsStatus = status1;
			departure1.BH_ApplicationCode = applicationCode;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_CustomsStatus = status2;
			departure2.BH_ApplicationCode = applicationCode;
			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.BM_CustomsStatus = status1;
			arrival1.BH_ApplicationCode = applicationCode;
			var arrival2 = GetNewArrival();
			arrival2.ArrivalMovementHeader.BM_CustomsStatus = status2;
			arrival2.BH_ApplicationCode = applicationCode;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterApplicationCode.Property = "x";
			textFilterApplicationCode.Property = applicationCode;

			var textFilterStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalStatus];
			textFilterStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterStatus.IsActive = true;

			textFilterStatus.Property = status1;
			AssertEquals($"{assertionInfo} - departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does  match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilterStatus.Property = status2;
			AssertEquals($"{assertionInfo} - departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does  match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestDeparturePhaseStatusFilter_Phase4() => CombineAssertions(() =>
		{
			using (SubstituteNctsSettings(false))
			{
				AssertDeparturePhaseStatusFilterCodesList(null, DeparturePhaseStatusCodesPhase4);
				AssertDeparturePhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS4, DeparturePhaseStatusCodesPhase4);
				AssertDeparturePhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS5, DeparturePhaseStatusCodesPhase5);
				AssertDeparturePhaseStatusFilterCodesList(ZString.Empty, DeparturePhaseStatusCodesPhase5 + ", " + DeparturePhaseStatusCodesPhase4);
				AssertDeparturePhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementHeaderTransactionStatusList.Codes.AmendmentAcceptance, NctsMovementHeaderTransactionStatusList.Codes.AmendmentRejected);
				AssertDeparturePhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5DeparturePhaseList.Codes.Amendment, NCTS5DeparturePhaseList.Codes.Presentation);
			}
		});

		public void TestDeparturePhaseStatusFilter_Phase5() => CombineAssertions(() =>
		{
			AssertDeparturePhaseStatusFilterCodesList(null, DeparturePhaseStatusCodesPhase5);
			AssertDeparturePhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS4, DeparturePhaseStatusCodesPhase4);
			AssertDeparturePhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS5, DeparturePhaseStatusCodesPhase5);
			AssertDeparturePhaseStatusFilterCodesList(ZString.Empty, DeparturePhaseStatusCodesPhase5 + ", " + DeparturePhaseStatusCodesPhase4);
			AssertDeparturePhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementHeaderTransactionStatusList.Codes.AmendmentAcceptance, NctsMovementHeaderTransactionStatusList.Codes.AmendmentRejected);
			AssertDeparturePhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5DeparturePhaseList.Codes.Amendment, NCTS5DeparturePhaseList.Codes.Presentation);
		});

		const string DeparturePhaseStatusCodesPhase4 = "AAC, ACK, AWO, CAC, CRF, DAJ, DAR, DCC, DCI, DGN, DIS, DMA, DNR, DPJ, DRI, DRJ, DRL, DTJ, FIN, MAM, MDS, MNS, MPN, MRI, MRR, NCK, R4A, RYP";
		const string DeparturePhaseStatusCodesPhase5 = "013, 014, 015, 141, 170, 054";

		void AssertDeparturePhaseStatusFilterCodesList(string applicationCode, string expectedValueList)
		{
			var assertionInfo = $"ApplicationCode={applicationCode ?? "default"}";
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			if (applicationCode != null)
			{
				var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
				textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				textFilterApplicationCode.Property = "x";
				textFilterApplicationCode.Property = applicationCode;
			}

			var textFilterPhaseStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DeparturePhaseStatus];
			AssertEquals($"{assertionInfo} - MultilingualDescription", "Departure Phase Status", textFilterPhaseStatus.MultilingualDescription.ToString());
			AssertEquals($"{assertionInfo} - Category", FilterCategories.ModesAndTypes, textFilterPhaseStatus.Category);
			AssertEquals($"{assertionInfo} - ComparisonOperator_List", "exact", textFilterPhaseStatus.ComparisonOperator_List.CodesAsString);
			AssertEquals($"{assertionInfo} - Value List", expectedValueList, ((CodeDescriptionPairList)textFilterPhaseStatus.List).CodesAsString);
		}

		void AssertDeparturePhaseStatusFilter(string applicationCode, string status1, string status2)
		{
			var assertionInfo = $"ApplicationCode={applicationCode}";

			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_Phase = status1;
			departure1.BH_ApplicationCode = applicationCode;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_Phase = status2;
			departure2.BH_ApplicationCode = applicationCode;
			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.BM_Phase = status1;
			arrival1.BH_ApplicationCode = applicationCode;
			var arrival2 = GetNewArrival();
			arrival2.ArrivalMovementHeader.BM_Phase = status2;
			arrival2.BH_ApplicationCode = applicationCode;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterPhaseStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DeparturePhaseStatus];
			textFilterPhaseStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterPhaseStatus.IsActive = true;

			textFilterPhaseStatus.Property = status1;
			AssertEquals($"{assertionInfo} - departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilterPhaseStatus.Property = status2;
			AssertEquals($"{assertionInfo} - departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestArrivalPhaseStatusFilter_Phase4() => CombineAssertions(() =>
		{
			using (SubstituteNctsSettings(false))
			{
				AssertArrivalPhaseStatusFilterCodesList(null, ArrivalPhaseStatusCodesPhase4);
				AssertArrivalPhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS4, ArrivalPhaseStatusCodesPhase4);
				AssertArrivalPhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS5, ArrivalPhaseStatusCodesPhase5);
				AssertArrivalPhaseStatusFilterCodesList(ZString.Empty, ArrivalPhaseStatusCodesPhase5 + ", " + ArrivalPhaseStatusCodesPhase4);
				AssertTestArrivalPhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent, NctsMovementHeaderTransactionStatusList.Codes.ArrivalRejected);
				AssertTestArrivalPhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5ArrivalPhaseList.Codes.Arrival, NCTS5ArrivalPhaseList.Codes.UnloadingRemarks);
			}
		});

		public void TestArrivalPhaseStatusFilter_Phase5() => CombineAssertions(() =>
		{
			AssertArrivalPhaseStatusFilterCodesList(null, ArrivalPhaseStatusCodesPhase5);
			AssertArrivalPhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS4, ArrivalPhaseStatusCodesPhase4);
			AssertArrivalPhaseStatusFilterCodesList(CusInBondApplicationCodeList.Codes.NCTS5, ArrivalPhaseStatusCodesPhase5);
			AssertArrivalPhaseStatusFilterCodesList(ZString.Empty, ArrivalPhaseStatusCodesPhase5 + ", " + ArrivalPhaseStatusCodesPhase4);
			AssertTestArrivalPhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent, NctsMovementHeaderTransactionStatusList.Codes.ArrivalRejected);
			AssertTestArrivalPhaseStatusFilter(CusInBondApplicationCodeList.Codes.NCTS5, NCTS5ArrivalPhaseList.Codes.Arrival, NCTS5ArrivalPhaseList.Codes.UnloadingRemarks);
		});

		const string ArrivalPhaseStatusCodesPhase4 = "ARR, CNT, FRC, MAS, MUS, NRL, PRC, STU, URJ";
		const string ArrivalPhaseStatusCodesPhase5 = "007, 044";

		void AssertArrivalPhaseStatusFilterCodesList(string applicationCode, string expectedValueList)
		{
			var assertionInfo = $"ApplicationCode={applicationCode ?? "default"}";
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			if (applicationCode != null)
			{
				var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
				textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				textFilterApplicationCode.Property = "x";
				textFilterApplicationCode.Property = applicationCode;
			}

			var textFilterPhaseStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalPhaseStatus];
			AssertEquals($"{assertionInfo} - MultilingualDescription", "Arrival Phase Status", textFilterPhaseStatus.MultilingualDescription.ToString());
			AssertEquals($"{assertionInfo} - Category", FilterCategories.ModesAndTypes, textFilterPhaseStatus.Category);
			AssertEquals($"{assertionInfo} - ComparisonOperator_List", "exact", textFilterPhaseStatus.ComparisonOperator_List.CodesAsString);
			AssertEquals($"{assertionInfo} - Value List", expectedValueList, ((CodeDescriptionPairList)textFilterPhaseStatus.List).CodesAsString);
		}

		public void AssertTestArrivalPhaseStatusFilter(string applicationCode, string status1, string status2)
		{
			var assertionInfo = $"ApplicationCode={applicationCode}";

			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_Phase = status1;
			departure1.BH_ApplicationCode = applicationCode;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_Phase = status2;
			departure2.BH_ApplicationCode = applicationCode;
			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.BM_Phase = status1;
			arrival1.BH_ApplicationCode = applicationCode;
			var arrival2 = GetNewArrival();
			arrival2.ArrivalMovementHeader.BM_Phase = status2;
			arrival2.BH_ApplicationCode = applicationCode;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();

			var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterApplicationCode.Property = "x";
			textFilterApplicationCode.Property = applicationCode;

			var textFilterPhaseStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalPhaseStatus];
			textFilterPhaseStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterPhaseStatus.IsActive = true;

			textFilterPhaseStatus.Property = status1;
			AssertEquals($"{assertionInfo} - departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does  match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilterPhaseStatus.Property = status2;
			AssertEquals($"{assertionInfo} - departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals($"{assertionInfo} - arrival2 does  match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestMessagingStatusFilter()
		{
			const string Ncts4Codes = "MAN, MAR, MAS, MCA, MCF, MCR, MDN, MDS, MQU, MEE, MOK, REJ, SNT, , MUR, MUS";
			const string Ncts5Codes = "ACC, ACK, ERR, FAL, INV";

			var departure1 = GetNewDeparture();
			departure1.BH_JobReference = "NCT0000110";
			departure1.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;

			var departure2 = GetNewDeparture();
			departure2.BH_JobReference = "NCT0000111";
			departure2.EffectiveMessageStatus = "";

			var departureNCTS5 = GetNewDeparture();
			departureNCTS5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureNCTS5.BH_JobReference = "NCT00NCTS5";
			departureNCTS5.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;

			var arrival1 = GetNewArrival();
			arrival1.BH_JobReference = "NCT0000112";
			arrival1.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;

			var arrival2 = GetNewArrival();
			arrival2.BH_JobReference = "NCT0000113";
			arrival2.EffectiveMessageStatus = "";

			var arrivalNCTS5 = GetNewArrival();
			arrivalNCTS5.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNCTS5.BH_JobReference = "NCT00NCTS5";
			arrivalNCTS5.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;

			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MessagingStatus];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNCTS5 does match filter", true, departureNCTS5.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalNCTS5 does not match filter", false, arrivalNCTS5.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = NctsMessageStatusList.Codes.ArrivalNotificationNotSent;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNCTS5 does not match filter", false, departureNCTS5.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalNCTS5 does match filter", true, arrivalNCTS5.MatchesFilter(filterStripBO.Filter));

			var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			AssertList(CusInBondApplicationCodeList.Codes.NCTS4, Ncts4Codes);
			AssertList(CusInBondApplicationCodeList.Codes.NCTS5, Ncts5Codes + ", " + Ncts4Codes);
			AssertList(string.Empty, Ncts5Codes + ", " + Ncts4Codes);

			void AssertList(string applicationCode, string expectedValueList)
			{
				textFilterApplicationCode.Property = applicationCode;
				textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				AssertEquals($"Value list for ApplicationCode={applicationCode}", expectedValueList, ((CodeDescriptionPairList)textFilter.List).CodesAsString);
			}
		}

		public void TestMessagingStatusFilter_List() => CombineAssertions(() =>
		{
			const string ncts4Codes = "MAN, MAR, MAS, MCA, MCF, MCR, MDN, MDS, MQU, MEE, MOK, REJ, SNT, , MUR, MUS";
			const string ncts5Codes = "ACC, ACK, ERR, FAL, INV";
			AssertMessagingStatusFilter_List(CusInBondApplicationCodeList.Codes.NCTS4, ncts4Codes);
			AssertMessagingStatusFilter_List(CusInBondApplicationCodeList.Codes.NCTS5, ncts5Codes + ", " + ncts4Codes);
			AssertMessagingStatusFilter_List(string.Empty, ncts5Codes + ", " + ncts4Codes);
		});

		void AssertMessagingStatusFilter_List(string applicationCode, string expectedValueList)
		{
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilterApplicationCode = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ApplicationCode];
			textFilterApplicationCode.Property = applicationCode;
			var textFilterStatus = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MessagingStatus];
			AssertNotNull(textFilterStatus);
			textFilterStatus.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilterStatus.IsActive = true;

			AssertEquals($"Value list for ApplicationCode={applicationCode}", expectedValueList, ((CodeDescriptionPairList)textFilterStatus.List).CodesAsString);
		}

		public void TestJobStatusFilterComparisonOperator_List()
		{
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var jobStatusFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.JobStatus];
			AssertNotNull("Job Status filter", jobStatusFilter);

			AssertContainsExactElementsInAnyOrder(new string[] { "exact", "not equal" }, jobStatusFilter.ComparisonOperator_List.GetAllCodes());
		}

		#region CustomFieldFilter
		public void TestWorkflowCustomFieldsFilters()
		{
				var filter = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				AssertNull(filter["custom text"]);
				AssertNull(filter["custom int"]);
				AssertNull(filter["custom decimal"]);
				AssertNull(filter["custom datetime"]);
				AssertNull(filter["custom shipment string"]);

				NctsMoveCustomFieldHelper.CreateNCTSPhase5WorkflowWithCustomFields(Factory);
				filter = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				AssertNotNull(filter["Custom Header string"]);
				AssertNotNull(filter["Custom DepartureHeader string"]);
				AssertNotNull(filter["Custom ArrivalHeader string"]);
				var workflowCustomFieldsFilter = (ModuleTextFilter)filter["Custom Header string"];
				workflowCustomFieldsFilter.IsActive = true;
				AssertNoExceptionThrown(() => Factory.Load<NctsHeader>(filter.Filter));
		}

		#endregion

		public void TestJobStatusFilter()
		{
			var departure1 = GetNewDeparture();
			LoadOrCreateJobHeader(departure1);
			departure1.Job.JH_Status = "WRK";

			var arrival1 = GetNewArrival();
			LoadOrCreateJobHeader(arrival1);
			arrival1.Job.JH_Status = "WRK";

			var departure2 = GetNewDeparture();
			LoadOrCreateJobHeader(departure2);
			departure2.Job.JH_Status = "IHL";

			var arrival2 = GetNewArrival();
			LoadOrCreateJobHeader(arrival2);
			arrival2.Job.JH_Status = "IHL";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var jobStatusFilter = (ModuleTextFilter)filterStripBO["Job Status"];
			jobStatusFilter.IsActive = true;
			AssertNotNull("Job Status filter", jobStatusFilter);

			CombineAssertions("with exact comparision", () =>
			{
				jobStatusFilter.ComparisonOperator = "exact";
				jobStatusFilter.Property = "WRK";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
			});

			CombineAssertions("with not equal comparision", () =>
			{
				jobStatusFilter.ComparisonOperator = "not equal";
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
			});

			CombineAssertions("for unknown status, exact comparision", () =>
			{
				jobStatusFilter.ComparisonOperator = "exact";
				jobStatusFilter.Property = "XXX";
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
			});

			CombineAssertions("for unknown status, not equal comparision", () =>
			{
				jobStatusFilter.ComparisonOperator = "not equal";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
			});

			JobHeader LoadOrCreateJobHeader(IJobHeaderParent parent) => new JobHeader.Loader(parent).TryLoadOrCreate();
		}

		public void TestPrincipalFilter()
		{
			var departure1 = GetNewDeparture();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			departure1.Principal.E2_OA_Address = org1.MainAddress.PK;
			var departure2 = GetNewDeparture();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			departure2.Principal.E2_OA_Address = org2.MainAddress.PK;
			Factory.Save();

			AssertMatchesFilter(departure1, departure2, org1.PK, org2.PK, NctsMovementFilterStripBusinessObject.FilterConstants.Principal);
		}

		public void TestConsignorFilter()
		{
			var departure1 = GetNewDeparture();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			departure1.Consignor.E2_OA_Address = org1.MainAddress.PK;
			var departure2 = GetNewDeparture();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			departure2.Consignor.E2_OA_Address = org2.MainAddress.PK;
			Factory.Save();

			AssertMatchesFilter(departure1, departure2, org1.PK, org2.PK, NctsMovementFilterStripBusinessObject.FilterConstants.Consignor);
		}

		public void TestConsigneeFilter()
		{
			var departure1 = GetNewDeparture();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			departure1.Consignee.E2_OA_Address = org1.MainAddress.PK;
			var departure2 = GetNewDeparture();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			departure2.Consignee.E2_OA_Address = org2.MainAddress.PK;
			Factory.Save();

			AssertMatchesFilter(departure1, departure2, org1.PK, org2.PK, NctsMovementFilterStripBusinessObject.FilterConstants.Consignee);
		}

		public void TestConsigneeFullNameFilter()
		{
			var departure1 = GetNewDeparture();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Consignee1 Full name";
			departure1.Consignee.E2_OA_Address = org1.MainAddress.PK;

			var departure2 = GetNewDeparture();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Consignee2 Full name";
			departure2.Consignee.E2_OA_Address = org2.MainAddress.PK;

			var departure3 = GetNewDeparture();

			Factory.Save();

			AssertMatchesFilter(departure1, departure2, "Consignee1 Full", NctsMovementFilterStripBusinessObject.FilterConstants.ConsigneeFullName);

			AssertMatchesFilterIsBlank(departure1, departure3, NctsMovementFilterStripBusinessObject.FilterConstants.ConsigneeFullName);
		}

		public void TestConsignorFullNameFilter()
		{
			var departure1 = GetNewDeparture();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Consignor1 Full name";
			departure1.Consignor.E2_OA_Address = org1.MainAddress.PK;

			var departure2 = GetNewDeparture();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Consignor2 Full name";
			departure2.Consignor.E2_OA_Address = org2.MainAddress.PK;

			var departure3 = GetNewDeparture();

			Factory.Save();

			AssertMatchesFilter(departure1, departure2, "Consignor1 Full", NctsMovementFilterStripBusinessObject.FilterConstants.ConsignorFullName);

			AssertMatchesFilterIsBlank(departure1, departure3, NctsMovementFilterStripBusinessObject.FilterConstants.ConsignorFullName);
		}

		public void TestDestinationTraderFilter()
		{
			var arrival1 = GetNewArrival();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			arrival1.DestinationTrader.E2_OA_Address = org1.MainAddress.PK;
			var arrival2 = GetNewArrival();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			arrival2.DestinationTrader.E2_OA_Address = org2.MainAddress.PK;
			Factory.Save();

			AssertMatchesFilter(arrival1, arrival2, org1.PK, org2.PK, NctsMovementFilterStripBusinessObject.FilterConstants.DestinationTrader);
		}

		public void TestRepresentativeFilter()
		{
			var departure1 = GetNewDeparture();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			departure1.MovementHeader.Representative.E2_OA_Address = org1.MainAddress.PK;

			var departure2 = GetNewDeparture();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			departure2.MovementHeader.Representative.E2_OA_Address = org2.MainAddress.PK;
			Factory.Save();

			AssertMatchesFilter(departure1, departure2, org1.PK, org2.PK, NctsMovementFilterStripBusinessObject.FilterConstants.Representative);
		}

		public void TestJobNumberFilter_Standalone()
		{
			var departure1 = GetNewDeparture();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var jobNumberFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.JobNumber];
			jobNumberFilter.IsActive = true;

			CombineAssertions("JobNumber filter on standalone NctsHeader", () =>
			{
				AssertEquals("MultilingualDescription: Job Number", "Job Number", jobNumberFilter.MultilingualDescription);
				AssertEquals("Category: NumbersAndReferences", FilterCategories.NumbersAndReferences, jobNumberFilter.Category);
				AssertEquals("SupportsBlankComparisonOperators: false", false, jobNumberFilter.SupportsBlankComparisonOperators);

				jobNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
				AssertEquals("Matches filter when BH_JobReference empty and JobNumber filter requires IsBlank.", true, departure1.MatchesFilter(filterStripBO.Filter));

				jobNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				jobNumberFilter.Property = "N00001";
				AssertEquals("Not match filter when BH_JobReference empty and JobNumber filter requires exact N00001.", false, departure1.MatchesFilter(filterStripBO.Filter));

				departure1.BH_JobReference = "N00001";
				Factory.Save();
				AssertEquals("Matches filter when BH_JobReference N00001 and JobNumber filter requires exact N00001.", true, departure1.MatchesFilter(filterStripBO.Filter));

				jobNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
				AssertEquals("Matches filter when BH_JobReference N00001 and JobNumber filter requires starting with N00001.", true, departure1.MatchesFilter(filterStripBO.Filter));

				jobNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
				jobNumberFilter.Property = "N";
				AssertEquals("Not match filter when BH_JobReference N00001 and JobNumber filter requires not starting with N.", false, departure1.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestJobNumberFilter_WithShipment()
		{
			var departure1 = GetNewDeparture();
			departure1.BH_JobReference = "N00001";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			departure1.BH_ParentTableCode = shipment.TablePrefix;
			departure1.BH_ParentID = shipment.PK;
			shipment.JS_UniqueConsignRef = "S00001";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var jobNumberFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.JobNumber];
			jobNumberFilter.IsActive = true;
			CombineAssertions("JobNumber filter on NctsHeader attached to ForwardingShipment", () =>
			{
				jobNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				jobNumberFilter.Property = "N00001";
				AssertEquals("Not match filter when attached to Shipment S00001 and JobNumber filter requires exact N00001.", false, departure1.MatchesFilter(filterStripBO.Filter));

				jobNumberFilter.Property = "S00001";
				AssertEquals("Matches filter when attached to Shipment S00001 and JobNumber filter requires exact S00001.", true, departure1.MatchesFilter(filterStripBO.Filter));

				jobNumberFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith;
				jobNumberFilter.Property = "N";
				AssertEquals("Matches filter when attached to Shipment S00001 and JobNumber filter requires not starting with N.", true, departure1.MatchesFilter(filterStripBO.Filter));

				jobNumberFilter.Property = "S";
				AssertEquals("Not match filter when attached to Shipment S00001 and JobNumber filter requires not starting with S.", false, departure1.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestLocalReferenceNumberFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.BH_JobReference = "NCT0000110";
			var departure2 = GetNewDeparture();
			departure2.BH_JobReference = "NCT0000111";
			departure2.LocalReferenceNumber = "NCT0000990";
			var arrival1 = GetNewArrival();
			arrival1.BH_JobReference = "NCT0000112";
			var arrival2 = GetNewArrival();
			arrival2.BH_JobReference = "NCT0000113";
			arrival2.LocalReferenceNumber = "NCT0000991";
			var arrival3 = GetNewArrival();
			arrival3.LocalReferenceNumber = ZString.Empty.PadLeft(NctsHeader.Schema.LocalReferenceNumberMaxLength, 'A');
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "NCT000011";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "NCT000099";
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = ZString.Empty.PadLeft(NctsHeader.Schema.LocalReferenceNumberMaxLength + 1, 'A');
			AssertEquals("arrival3 does match filter", true, arrival3.MatchesFilter(filterStripBO.Filter));
		}

		public void TestLocalReferenceNumberFilter_Phase5_Arrival()
		{
			var arrival1 = GetNewArrival();
			arrival1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrival1.BH_JobReference = "NCT0000100";
			arrival1.LocalReferenceNumber = "NCT1234";
			arrival1.ArrivalMovementHeader.BM_PaperlessInbondNum = "NCT5678";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber];
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;

			textFilter.Property = "NCT5678";
			AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
		}

		public void TestLocalReferenceNumberFilter_Phase5_Departure()
		{
			var departure1 = GetNewDeparture();
			departure1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure1.MovementHeader.BM_PaperlessInbondNum = "NCT0000110";

			var departure2 = GetNewDeparture();
			departure2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure2.BH_JobReference = "NCT0000110";

			var departure3 = GetNewDeparture();
			departure3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure3.LocalReferenceNumber = "NCT0000110";

			var departure4 = GetNewDeparture();
			departure4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure4.MovementHeader.BM_PaperlessInbondNum = ZString.Empty.PadLeft(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum.MaxLength, 'A');
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber];
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;

			textFilter.Property = "NCT000011";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does match filter", true, departure3.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = ZString.Empty.PadLeft(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum.MaxLength + 1, 'A');
			AssertEquals("departure4 does match filter", true, departure4.MatchesFilter(filterStripBO.Filter));
		}

		public void TestLocalReferenceNumberFilter_Phase5_NonDeparture()
		{
			var arrival1 = GetNewArrival();
			arrival1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrival1.ArrivalMovementHeader.BM_PaperlessInbondNum = "NCT0000110";
			var arrival2 = GetNewArrival();
			arrival1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrival2.LocalReferenceNumber = "NCT0000110";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber];
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;

			textFilter.Property = "NCT000011";
			AssertEquals("arrival1 does not match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestMovementReferenceNumberFilter() => CombineAssertions(() =>
		{
			var departure1 = GetNewDeparture();
			NCTSTestHelper.SetMrnForTest(departure1, "15GB000060100C8110");
			var departure2 = GetNewDeparture();
			NCTSTestHelper.SetMrnForTest(departure2, "15GB000060100C8990");
			var departure3 = GetNewDeparture();
			var arrival1 = GetNewArrival();
			NCTSTestHelper.SetMrnForTest(arrival1, "15GB000060100C8111");
			var arrival2 = GetNewArrival();
			NCTSTestHelper.SetMrnForTest(arrival2, "15GB000060100C8991");
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MovementReferenceNumber];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "15GB000060100C811";
			AssertEquals(AssertionMessage("departure1 does match filter"), true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("departure2 does not match filter"), false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("departure3 does not match filter"), false, departure3.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("arrival1 does match filter"), true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("arrival2 does not match filter"), false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "15GB000060100C899";
			AssertEquals(AssertionMessage("departure1 does not match filter"), false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("departure2 does match filter"), true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("departure3 does not match filter"), false, departure3.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("arrival1 does not match filter"), false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("arrival2 does match filter"), true, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			AssertEquals(AssertionMessage("departure1 does not match filter"), false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("IsBlank departure3 does match filter"), true, departure3.MatchesFilter(filterStripBO.Filter));

			string AssertionMessage(string message) => $"{textFilter.ComparisonOperator} {textFilter.Property} - {message}";
		});

		public void TestMovementTypeFilter()
		{
			var departure1 = GetNewDeparture();
			var departure2 = GetNewDeparture();
			var arrival1 = GetNewArrival();
			var arrival2 = GetNewArrival();
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MovementType];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = NctsMovementType.Codes.Departure;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = NctsMovementType.Codes.Arrival;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestDeclarationTypeFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();

			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DeclarationType];
			AssertNotNull(textFilter);
			textFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
		}

		[TestDate(2004, 10, 22)]
		public void TestDepartureDateFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_EntryDate = ZDate.Today.AddDays(-1);
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_EntryDate = ZDate.Today;
			var departure3 = GetNewDeparture();
			departure3.MovementHeader.BM_EntryDate = ZDate.Today.AddDays(+1);
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DepartureDate];
			AssertNotNull(dateFilter);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", dateFilter.MultilingualDescription);

			dateFilter.Property1 = ZDate.Today;
			dateFilter.Property2 = ZDate.Today;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));

			dateFilter.Property1 = ZDate.Today.AddMonths(1);
			dateFilter.Property2 = ZDate.Today.AddMonths(1);
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));
		}

		[TestDate(1985, 07, 13)]
		public void TestArrivalDateFilter()
		{
			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.BM_ArrivalDate = ZDate.Today.AddDays(-1);
			var arrival2 = GetNewArrival();
			arrival2.ArrivalMovementHeader.BM_ArrivalDate = ZDate.Today;
			var arrival3 = GetNewArrival();
			arrival3.ArrivalMovementHeader.BM_ArrivalDate = ZDate.Today.AddDays(1);
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ArrivalDate];
			AssertNotNull(dateFilter);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", dateFilter.MultilingualDescription);

			dateFilter.Property1 = ZDate.Today;
			dateFilter.Property2 = ZDate.Today;
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does not match filter", false, arrival3.MatchesFilter(filterStripBO.Filter));

			dateFilter.Property1 = ZDate.Today.AddMonths(1);
			dateFilter.Property2 = ZDate.Today.AddMonths(1);
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does not match filter", false, arrival3.MatchesFilter(filterStripBO.Filter));
		}

		public void TestDateLimitFilter()
		{
			var header1 = GetNewDeparture();
			header1.MovementHeader.BM_ExportDate = ZDate.Today.AddDays(-1);
			var header2 = GetNewDeparture();
			header2.MovementHeader.BM_ExportDate = ZDate.Today;
			var header3 = GetNewArrival();
			header3.ArrivalMovementHeader.BM_ExportDate = ZDate.Today;
			var header4 = GetNewArrival();
			header4.ArrivalMovementHeader.BM_ExportDate = ZDate.Today.AddDays(1);
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DateLimit];
			AssertNotNull(dateFilter);
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", dateFilter.MultilingualDescription);

			dateFilter.Property1 = ZDate.Today;
			dateFilter.Property2 = ZDate.Today;
			AssertEquals("header1 does not match filter", false, header1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("header2 does match filter", true, header2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("header3 does match filter", true, header3.MatchesFilter(filterStripBO.Filter));
			AssertEquals("header4 does not match filter", false, header4.MatchesFilter(filterStripBO.Filter));

			dateFilter.Property1 = ZDate.Today.AddMonths(1);
			dateFilter.Property2 = ZDate.Today.AddMonths(1);
			AssertEquals("header1 does not match filter", false, header1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("header2 does not match filter", false, header2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("header3 does not match filter", false, header3.MatchesFilter(filterStripBO.Filter));
			AssertEquals("header4 does not match filter", false, header4.MatchesFilter(filterStripBO.Filter));
		}

		public void TestIsSimplifiedDepartureFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.IsSimplifiedNctsProcedure = false;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.IsSimplifiedNctsProcedure = true;
			var departure3 = GetNewDeparture();
			departure3.MovementHeader.IsSimplifiedNctsProcedure = false;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var flagsFilter = (ModuleFlagsFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.IsSimplifiedDeparture];
			AssertNotNull(flagsFilter);
			flagsFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", flagsFilter.MultilingualDescription);

			flagsFilter.Property0 = true;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));

			flagsFilter.Property0 = false;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does match filter", true, departure3.MatchesFilter(filterStripBO.Filter));
		}

		public void TestIsSimplifiedArrivalFilter()
		{
			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.IsSimplifiedNctsProcedure = false;
			var arrival2 = GetNewArrival();
			arrival2.ArrivalMovementHeader.IsSimplifiedNctsProcedure = true;
			var arrival3 = GetNewArrival();
			arrival3.ArrivalMovementHeader.IsSimplifiedNctsProcedure = false;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var flagsFilter = (ModuleFlagsFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.IsSimplifiedArrival];
			AssertNotNull(flagsFilter);
			flagsFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", flagsFilter.MultilingualDescription);

			flagsFilter.Property0 = true;
			AssertEquals("arrival1 does not match filter", false, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does match filter", true, arrival2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does not match filter", false, arrival3.MatchesFilter(filterStripBO.Filter));

			flagsFilter.Property0 = false;
			AssertEquals("arrival1 does match filter", true, arrival1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match filter", false, arrival2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does match filter", true, arrival3.MatchesFilter(filterStripBO.Filter));
		}

		public void TestIsSecurityDeclarationFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.BH_FTZMove = false;
			var departure2 = GetNewDeparture();
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", departure2.SecurityConsignor);
			departure2.BH_FTZMove = true;
			var departure3 = GetNewDeparture();
			departure3.BH_FTZMove = false;
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var flagsFilter = (ModuleFlagsFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.IsSecurityDeclaration];
			AssertNotNull(flagsFilter);
			flagsFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", flagsFilter.MultilingualDescription);

			flagsFilter.Property0 = true;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));

			flagsFilter.Property0 = false;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does match filter", true, departure3.MatchesFilter(filterStripBO.Filter));
		}

		public void TestShowOnlyJobsWithMRNFilter() => CombineAssertions(() =>
		{
			var departure1 = GetNewDeparture();
			var mrn = CusEntryNumber.LoadOrCreate(departure1, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123";

			var departure2 = GetNewDeparture();
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var flagsFilter = (ModuleFlagsFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ShowOnlyJobsWithMRN];
			AssertNotNull(flagsFilter);
			flagsFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", flagsFilter.MultilingualDescription);

			flagsFilter.Property0 = true;
			AssertEquals(AssertionMessage("departure1"), true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("departure2"), false, departure2.MatchesFilter(filterStripBO.Filter));

			flagsFilter.Property0 = false;
			AssertEquals(AssertionMessage("departure1"), true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage("departure2"), true, departure2.MatchesFilter(filterStripBO.Filter));

			string AssertionMessage(string message) => $"Property0={flagsFilter.Property0} - {message}";
		});

		public void TestDispatchCountryFilter_Phase4()
		{
			var departure1 = GetNewDeparture();
			departure1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure1.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.UnitedKingdom;

			var departure2 = GetNewDeparture();
			departure2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure2.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Italy;
			var goodsItem2 = departure2.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Spain;

			var departure3 = GetNewDeparture();
			departure3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure3.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			var goodsItem3 = departure3.MovementHeader.GoodsItems.AddNew();
			goodsItem3.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.UnitedKingdom;

			Factory.Save();

			CombineAssertions(() =>
			{
				var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DispatchCountry];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.Property}: departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.UnitedKingdom;
				AssertEquals($"{textFilter.Property}: departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3 does match filter", true, departure3.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Italy;
				AssertEquals($"{textFilter.Property}: departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestDispatchCountryFilter_Phase5()
		{
			var departure1 = CreateDepartureWithDispatchCountriesPhase5(Core.Constants.CountryCodes.Italy, null, null);
			var departure2 = CreateDepartureWithDispatchCountriesPhase5(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Spain, null);
			var departure3 = CreateDepartureWithDispatchCountriesPhase5(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.Ireland);
			var departure4 = CreateDepartureWithDispatchCountriesPhase5(Core.Constants.CountryCodes.Italy, ZString.Empty, ZString.Empty);
			Factory.Save();

			CombineAssertions(() =>
			{
				var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DispatchCountry];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

				textFilter.Property = Core.Constants.CountryCodes.Italy;
				AssertEquals($"{textFilter.Property}: departure1", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3", true, departure3.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure4", true, departure4.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.Property}: departure1", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3", true, departure3.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure4", false, departure4.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Ireland;
				AssertEquals($"{textFilter.Property}: departure1", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3", true, departure3.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure4", false, departure4.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.France;
				AssertEquals($"{textFilter.Property}: departure1", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure2", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure3", false, departure3.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.Property}: departure4", false, departure4.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestDispatchCountryFilterSpecificCases_Phase4()
		{
			var departure1 = CreateDepartureWithDispatchCountriesPhase4(Core.Constants.CountryCodes.Spain, ZString.Empty);
			var departure2 = CreateDepartureWithDispatchCountriesPhase4(ZString.Empty, Core.Constants.CountryCodes.Spain);
			var departure3 = CreateDepartureWithDispatchCountriesPhase4(ZString.Empty, ZString.Empty);
			Factory.Save();

			CombineAssertions(() =>
			{
				var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DispatchCountry];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.IsBlank;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on header", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on goods item", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: all countries empty", true, departure3.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.NotEqual;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: departure1 match filter", true, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.NotContains;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: departure1 match filter", true, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.DoesNotStartWith;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: departure1 match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestDispatchCountryFilterIsBlank_Phase5()
		{
			var departure1 = CreateDepartureWithDispatchCountriesPhase5(Core.Constants.CountryCodes.Spain, ZString.Empty, ZString.Empty);
			var departure2 = CreateDepartureWithDispatchCountriesPhase5(ZString.Empty, Core.Constants.CountryCodes.Spain, ZString.Empty);
			var departure3 = CreateDepartureWithDispatchCountriesPhase5(ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Spain);
			var departure4 = CreateDepartureWithDispatchCountriesPhase5(ZString.Empty, ZString.Empty, ZString.Empty);
			Factory.Save();

			CombineAssertions(() =>
			{
				var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DispatchCountry];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.IsBlank;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"departure1: country on movement", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"departure2: country on bill", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"departure3: country on gooditem", false, departure3.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"departure4: all countries blank", true, departure4.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestDispatchCountryFilterSpecificCases_Phase5()
		{
			var departure1 = CreateDepartureWithDispatchCountriesPhase5(Core.Constants.CountryCodes.Spain, ZString.Empty, ZString.Empty);
			var departure2 = CreateDepartureWithDispatchCountriesPhase5(ZString.Empty, Core.Constants.CountryCodes.Spain, ZString.Empty);
			var departure3 = CreateDepartureWithDispatchCountriesPhase5(ZString.Empty, ZString.Empty, Core.Constants.CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DispatchCountry];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.NotEqual;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on header is equal", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on bill is equal", false, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on item is equal", false, departure3.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on header is not equal", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on bill is not equal", true, departure2.MatchesFilter(filterStripBO.Filter));
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country on item is not equal", true, departure3.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.NotContains;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country is equal", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country is not equal", true, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.DoesNotStartWith;

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country is equal", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals($"{textFilter.SqlComparisonOperator} {textFilter.Property}: country is not equal", true, departure1.MatchesFilter(filterStripBO.Filter));
			});
		}

		NctsHeader CreateDepartureWithDispatchCountriesPhase4(ZString dispatchCountryOnHeader, ZString? dispatchCountryOnGoodsItem)
		{
			var departure = GetNewDeparture();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.BH_RL_NKImportLoadPort = dispatchCountryOnHeader;
			if (dispatchCountryOnGoodsItem.HasValue)
			{
				var goodsItem = departure.MovementHeader.GoodsItems.AddNew();
				goodsItem.BY_RN_NKCountryOfDispatch = dispatchCountryOnGoodsItem.Value;
			}
			return departure;
		}

		NctsHeader CreateDepartureWithDispatchCountriesPhase5(ZString dispatchCountryOnMovement, ZString? dispatchCountryOnBill, ZString? dispatchCountryOnGoodsItem)
		{
			var departure = GetNewDeparture();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departure.MovementHeader.BM_RN_NKCountryOfDispatch = dispatchCountryOnMovement;
			if (dispatchCountryOnBill.HasValue)
			{
				var bill = departure.Bills.AddNew();
				bill.B0_RN_NKCountryOfExport = dispatchCountryOnBill.Value;
				if (dispatchCountryOnGoodsItem.HasValue)
				{
					var goodsItem = bill.GoodsItems.AddNew();
					goodsItem.BY_RN_NKCountryOfDispatch = dispatchCountryOnGoodsItem.Value;
				}
			}
			return departure;
		}

		public void TestDestinationCountryFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.UnitedKingdom;

			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
			var goodsItem2 = departure2.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Spain;

			var departure3 = GetNewDeparture();
			departure3.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.France;
			var goodsItem3 = departure3.MovementHeader.GoodsItems.AddNew();
			goodsItem3.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedKingdom;

			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DestinationCountry];
			AssertNotNull(textFilter);
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = Core.Constants.CountryCodes.Spain;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does match filter", true, departure3.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = Core.Constants.CountryCodes.Italy;
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure3 does not match filter", false, departure3.MatchesFilter(filterStripBO.Filter));
		}

		public void TestDestinationCountryFilterSpecificCases()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Spain;

			var goodsItem1 = departure1.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;

			Factory.Save();

			CombineAssertions(() =>
			{
				var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
				var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DestinationCountry];
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

				textFilter.Property = Core.Constants.CountryCodes.Spain;
				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.IsBlank;
				AssertEquals("departure1 does not match filter IsBlank", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.NotEqual;
				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals("departure1 does not match filter NotEqual", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals("departure1 match filter NotEqual", true, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.NotContains;
				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals("departure1 does not match filter NotContains", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals("departure1 match filter NotContains", true, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.SqlComparisonOperator = CargoWise.EntityFramework.SQLComparisonOperator.DoesNotStartWith;
				textFilter.Property = Core.Constants.CountryCodes.Spain;
				AssertEquals("departure1 does not match filter DoesNotStartWith", false, departure1.MatchesFilter(filterStripBO.Filter));

				textFilter.Property = Core.Constants.CountryCodes.Colombia;
				AssertEquals("departure1 match filter DoesNotStartWith", true, departure1.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestPortOfLoadingFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_RL_NKForeignDestPort = "ITROM";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.PortOfLoading];
			AssertNotNull(textFilter);
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "GBDVR";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "ITROM";
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestPortOfUnloadingFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.PlaceOfUnloadingCode = "GBDVR";
			var departure2 = GetNewDeparture();
			departure2.PlaceOfUnloadingCode = "ITROM";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DeparturePortOfUnloading];
			AssertNotNull(textFilter);
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "GBDVR";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "ITROM";
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestCommercialReferenceNumberFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_AdditionalText = "Consol1";
			var goodsItem1 = departure1.MovementHeader.GoodsItems.AddNew();
			goodsItem1.BY_CommercialReferenceNumber = "Shipment1";
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_AdditionalText = "Consol2";
			var goodsItem2 = departure2.MovementHeader.GoodsItems.AddNew();
			goodsItem2.BY_CommercialReferenceNumber = "Shipment2";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.CommercialReferenceNumber];
			AssertNotNull(textFilter);
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "Consol1";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "Consol2";
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "Shipment1";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));

			textFilter.Property = "Shipment2";
			AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestNctsDeclarationTypeList()
		{
			const string eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunId = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);
			helper.CreateOrGetLanguage("LT", "Latvian");
			Factory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "NCTS Declaration Type (Box 1)");
			var t1 = helper.CreateNewOrGetExistingCusCodeList(eunCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "T1", "Goods moving under external community transit procedure", startDate, endDate);
			var t2 = helper.CreateNewOrGetExistingCusCodeList(eunCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType, "T2", "Goods moving under internal community transit procedure", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeListLanguage(t1, "LT", "T1-Waren im externen Versandverfahren");
			helper.CreateNewOrGetExistingCusCodeListLanguage(t2, "LT", "T2-Waren Tevzemei and Brivibai");

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "LTV";
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				CombineAssertions(() =>
				{
					var list = new NctsMovementFilterStripBusinessObject().DeclarationTypeList;
					AssertEquals("T1", "T1-Waren im externen Versandverfahren", list.GetDescriptionFromCode("T1"));
					AssertEquals("T2", "T2-Waren Tevzemei and Brivibai", list.GetDescriptionFromCode("T2"));
				});
			}
		}

		public void TestContainerFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT1";
			departure1.DepartureHeaderContainers.AddNew().BC_ContainerNum = "CNT2";

			var departure2 = GetNewDeparture();
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ContainerNum];
			AssertNotNull(textFilter);
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "CNT2";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestGuaranteeReferenceFilter()
		{
			var departure1 = GetNewDeparture();
			var guarantee = departure1.Guarantees.AddNew();
			guarantee.PW_BondNumber = "GRN001";

			var departure2 = GetNewDeparture();
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.GuaranteeReference];
			AssertNotNull(textFilter);
			textFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", textFilter.MultilingualDescription);

			textFilter.Property = "GRN001";
			AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
		}

		public void TestSealsNumberFilterContainers()
		{
			var departure1 = GetNewDeparture();

			var container1 = departure1.DepartureHeaderContainers.AddNew();
			container1.Seal1 = "SEAL1";
			container1.Seal2 = "SEAL2";
			var container2 = departure1.DepartureHeaderContainers.AddNew();
			container2.Seal1 = "SEAL3";

			var departure2 = GetNewDeparture();

			var container3 = departure2.DepartureHeaderContainers.AddNew();
			container3.Seal1 = "SEAL1";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total NCTS Declarations", 2, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL1", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 2, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Seal Number with value SEAL1", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Seal Number with value SEAL1", true, departureCollection.Contains(departure2));

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL2", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Seal Number with value SEAL2", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Seal Number with value SEAL2", false, departureCollection.Contains(departure2));

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL3", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Seal Number with value SEAL3", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Seal Number with value SEAL3", false, departureCollection.Contains(departure2));

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL4", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Seal Number with value SEAL3", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Seal Number with value SEAL3", false, departureCollection.Contains(departure2));
			});
		}

		public void TestSealsNumberFilterPackages()
		{
			var departure1 = GetNewDeparture();
			var container1 = departure1.DepartureHeaderContainers.AddNew();
			container1.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "SEAL2";

			var departure2 = GetNewDeparture();
			var container2 = departure2.DepartureHeaderContainers.AddNew();
			container2.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total NCTS Declarations", 2, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL1", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 2, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Seal Number with value SEAL1", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Seal Number with value SEAL1", true, departureCollection.Contains(departure2));

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL2", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Seal Number with value SEAL2", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Seal Number with value SEAL2", false, departureCollection.Contains(departure2));

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL3", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Seal Number with value SEAL3", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Seal Number with value SEAL3", false, departureCollection.Contains(departure2));
			});
		}

		public void TestSealsNumberFilterContainersAndPackages()
		{
			var departure1 = GetNewDeparture();

			var container1 = departure1.DepartureHeaderContainers.AddNew();
			container1.Seal1 = "SEAL1";

			var departure2 = GetNewDeparture();
			var container2 = departure2.DepartureHeaderContainers.AddNew();
			container2.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 2, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL1", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 2, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Seal Number with value SEAL1", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Seal Number with value SEAL1", true, departureCollection.Contains(departure2));

				LoadNCTSHeaderCollectionTextFilter("Seal #", "SEAL2", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Seal Number with value SEAL2", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Seal Number with value SEAL2", false, departureCollection.Contains(departure2));
			});
		}

		public void TestTariffGoodItemFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.GoodsItems.AddNew().BY_HarmonisedTariff = "1234567890";
			departure1.MovementHeader.GoodsItems.AddNew().BY_HarmonisedTariff = "0987654321";

			var departure2 = GetNewDeparture();
			departure2.MovementHeader.GoodsItems.AddNew().BY_HarmonisedTariff = "1234567890";

			var arrival1 = GetNewArrival();
			arrival1.ArrivalMovementHeader.GoodsItems.AddNew().BY_HarmonisedTariff = "1234567890";

			Factory.Save();

			var collection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			collection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 3, collection.Count);

				LoadNCTSHeaderCollectionTextFilter("Tariff - Good Item", "1234567890", collection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 3, collection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Tariff with value 1234567890", true, collection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Tariff with value 1234567890", true, collection.Contains(departure2));
				AssertEquals("Arrival 1 is in the filter cause has a Tariff with value 1234567890", true, collection.Contains(arrival1));

				LoadNCTSHeaderCollectionTextFilter("Tariff - Good Item", "0987654321", collection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, collection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Tariff with value 0987654321", true, collection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Tariff with value 0987654321", false, collection.Contains(departure2));
				AssertEquals("Arrival 1 is not in the filter cause has not a Tariff with value 0987654321", false, collection.Contains(arrival1));

				LoadNCTSHeaderCollectionTextFilter("Tariff - Good Item", "6501000000", collection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, collection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Tariff with value 6501000000", false, collection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Tariff with value 6501000000", false, collection.Contains(departure2));
				AssertEquals("Arrival 1 is not in the filter cause has not a Tariff with value 6501000000", false, collection.Contains(arrival1));
			});
		}

		public void TestDepartureGoodsLocationCodeFilter()
		{
			var getNewNctsHeader = new Func<string, NctsHeader>(
				s =>
				{
					var result = GetNewDeparture();
					result.MovementHeader.BM_LocationOfGoodsCode = s;
					return result;
				}
			);

			AssertGoodsLocationCodeFilter("Departure", getNewNctsHeader);
		}

		public void TestArrivalGoodsLocationCodeFilter()
		{
			var getNewNctsHeader = new Func<string, NctsHeader>(
				s =>
				{
					var result = GetNewArrival();
					result.ArrivalMovementHeader.BM_LocationOfGoodsCode = s;
					return result;
				}
			);

			AssertGoodsLocationCodeFilter("Arrival", getNewNctsHeader);
		}

		void AssertGoodsLocationCodeFilter(string direction, Func<string, NctsHeader> getNewNctsHeader)
		{
			var header1 = getNewNctsHeader("9999000002");
			var header2 = getNewNctsHeader("9998000002");

			Factory.Save();

			var nctsCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			nctsCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 2, nctsCollection.Count);

				LoadNCTSHeaderCollectionTextFilter($"{direction} Goods Location", "9999000002", nctsCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
				AssertEquals($"{direction} 1 is in the filter cause its goods location is 9999000002", true, nctsCollection.Contains(header1));
				AssertEquals($"{direction} 2 is not in the filter cause its goods location is not 9999000002", false, nctsCollection.Contains(header2));

				LoadNCTSHeaderCollectionTextFilter($"{direction} Goods Location", "9998000002", nctsCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, nctsCollection.Count);
				AssertEquals($"{direction} 1 is not in the filter cause its goods location is not 9998000002", false, nctsCollection.Contains(header1));
				AssertEquals($"{direction} 2 is in the filter cause its goods location is 9998000002", true, nctsCollection.Contains(header2));

				LoadNCTSHeaderCollectionTextFilter($"{direction} Goods Location", "2803000000", nctsCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, nctsCollection.Count);
				AssertEquals($"{direction} 1 is not in the filter cause its goods location is not 2803000000", false, nctsCollection.Contains(header1));
				AssertEquals($"{direction} 2 is not in the filter cause its goods location is not 2803000000", false, nctsCollection.Contains(header2));
			});
		}

		public void TestSupportingDocumentsFilterType()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem = bill1.GoodsItems.AddNew();
			goodItem.SupportingDocuments.AddNew().CSI_Code = "N740";
			goodItem.SupportingDocuments.AddNew().CSI_Code = "N705";
			bill1.GoodsItems.AddNew().SupportingDocuments.AddNew().CSI_Code = "N730";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.GoodsItems.AddNew().SupportingDocuments.AddNew().CSI_Code = "N740";
			bill2.GoodsItems.AddNew().SupportingDocuments.AddNew().CSI_Code = "N705";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			bill3.SupportingDocuments.AddNew().CSI_Code = "N740";

			var departure4 = GetNewDeparture();
			departure4.MovementHeader.SupportingDocuments.AddNew().CSI_Code = "N705";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 4, departureCollection.Count);

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentType, "N740", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Document with its Code is N740", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Document with its Code is N740", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has a Document with its Code is N740", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Document with its Code is N740", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentType, "N705", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Document with its Code is N705", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Document with its Code is N705", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Document with its Code is N705", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has a Document with its Code is N705", true, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentType, "N730", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Document with its Code is N730", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Document with its Code is N730", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Document with its Code is N730", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Document with its Code is N730", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentType, "N380", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Document with its Code is N380", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Document with its Code is N380", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Document with its Code is N380", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Document with its Code is N380", false, departureCollection.Contains(departure4));
			});
		}

		public void TestSupportingDocumentsFilterReference()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem = bill1.GoodsItems.AddNew();
			goodItem.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference1";
			goodItem.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference2";
			bill1.GoodsItems.AddNew().SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference3";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.GoodsItems.AddNew().SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference1";
			bill2.GoodsItems.AddNew().SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference2";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			bill3.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference1";

			var departure4 = GetNewDeparture();
			departure4.MovementHeader.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Reference2";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 4, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentReference, "Reference1", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Document with its Reference is Reference1", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Document with its Reference is Reference1", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has a Document with its Reference is Reference1", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Document with Reference is Reference1", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentReference, "Reference2", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Document with its Reference is Reference2", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Document with its Reference is Reference2", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Document with its Reference is Reference2", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has a Document with Reference is Reference2", true, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentReference, "Reference3", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Document with its Reference is Reference3", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Document with its Reference is Reference3", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Document with its Reference is Reference3", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Document with Reference is Reference3", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.SupportingDocumentReference, "Reference4", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Document with its Reference is Reference4", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Document with its Reference is Reference4", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Document with its Reference is Reference4", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Document with Reference is Reference4", false, departureCollection.Contains(departure4));
			});
		}

		public void TestAdditionalDocumentKindFilter()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem1 = bill1.GoodsItems.AddNew();
			goodItem1.AdditionalInfos.AddNew().CSI_SubType = "INF";
			goodItem1.AdditionalInfos.AddNew().CSI_SubType = "REF";
			goodItem1.AdditionalInfos.AddNew().CSI_SubType = "TRA";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			var goodItem2 = bill2.GoodsItems.AddNew();
			goodItem2.PreviousDocuments.AddNew().CSI_SubType = "INF";
			goodItem2.PreviousDocuments.AddNew().CSI_SubType = "REF";
			goodItem2.PreviousDocuments.AddNew().CSI_SubType = "TRA";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			var goodItem3 = bill3.GoodsItems.AddNew();
			goodItem3.AdditionalInfos.AddNew().CSI_SubType = "INF";
			goodItem3.AdditionalInfos.AddNew().CSI_SubType = "B";
			goodItem3.AdditionalInfos.AddNew().CSI_SubType = "C";

			var departure4 = GetNewDeparture();
			var bill4 = departure4.Bills.AddNew();
			bill4.AdditionalDocuments.AddNew().CSI_SubType = "INF";

			var departure5 = GetNewDeparture();
			departure5.AdditionalDocuments.AddNew().CSI_SubType = "REF";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 5, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentKind, "INF", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with Kind = INF", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has an Additional Document with Kind = INF", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has an Additional Document with Kind = INF", true, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is not in the filter cause has no Additional Document with Kind = INF", false, departureCollection.Contains(departure5));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentKind, "REF", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 2, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with Kind = REF", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter as it has no Additional Documents with Kind = REF", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Additional Document with Kind = REF", false, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is in the filter cause has an Additional Document with Kind = REF", true, departureCollection.Contains(departure5));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentKind, "TRA", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with its Kind = TRA", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter as it has no Additional Documents with its Kind = TRA", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Additional Document with Kind = TRA", false, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is not in the filter cause has no Additional Document with Kind = TRA", false, departureCollection.Contains(departure5));
			});
		}

		public void TestAdditionalDocumentTypeFilter()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem1 = bill1.GoodsItems.AddNew();
			goodItem1.AdditionalInfos.AddNew().CSI_Code = "A";
			goodItem1.AdditionalInfos.AddNew().CSI_Code = "B";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			var goodItem2 = bill2.GoodsItems.AddNew();
			goodItem2.PreviousDocuments.AddNew().CSI_Code = "A";
			goodItem2.PreviousDocuments.AddNew().CSI_Code = "B";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			var goodItem3 = bill3.GoodsItems.AddNew();
			goodItem3.AdditionalInfos.AddNew().CSI_Code = "A";
			goodItem3.AdditionalInfos.AddNew().CSI_Code = "C";

			var departure4 = GetNewDeparture();
			var bill4 = departure4.Bills.AddNew();
			bill4.AdditionalDocuments.AddNew().CSI_Code = "A";

			var departure5 = GetNewDeparture();
			departure5.AdditionalDocuments.AddNew().CSI_Code = "B";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 5, departureCollection.Count);

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentType, "A", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with Type = A", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has an Additional Document with Type = A", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has an Additional Document with Type = A", true, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is not in the filter cause has no Additional Document with Type = A", false, departureCollection.Contains(departure5));

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentType, "B", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 2, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with Type = B", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter as it has no Additional Documents with Type = B", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Additional Document with Type = B", false, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is in the filter cause has an Additional Document with Type = B", true, departureCollection.Contains(departure5));

				LoadNCTSHeaderCollectionNkFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentType, "C", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter as it has no Additional Document with its Type = C", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter as it has an Additional Documents with its Type = C", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Additional Document with Type = C", false, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is not in the filter cause has no Additional Document with Type = C", false, departureCollection.Contains(departure5));
			});
		}

		public void TestAdditionalDocumentReferenceFilter()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem1 = bill1.GoodsItems.AddNew();
			goodItem1.AdditionalInfos.AddNew().CSI_ReferenceNumber = "A";
			goodItem1.AdditionalInfos.AddNew().CSI_ReferenceNumber = "B";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			var goodItem2 = bill2.GoodsItems.AddNew();
			goodItem2.PreviousDocuments.AddNew().CSI_ReferenceNumber = "A";
			goodItem2.PreviousDocuments.AddNew().CSI_ReferenceNumber = "B";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			var goodItem3 = bill3.GoodsItems.AddNew();
			goodItem3.AdditionalInfos.AddNew().CSI_ReferenceNumber = "A";
			goodItem3.AdditionalInfos.AddNew().CSI_ReferenceNumber = "C";

			var departure4 = GetNewDeparture();
			var bill4 = departure4.Bills.AddNew();
			bill4.AdditionalDocuments.AddNew().CSI_ReferenceNumber = "A";

			var departure5 = GetNewDeparture();
			departure5.AdditionalDocuments.AddNew().CSI_ReferenceNumber = "B";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 5, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentReference, "A", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with Reference = A", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has an Additional Document with Reference = A", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has an Additional Document with Reference = A", true, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is not in the filter as it has no Additional Documents with Reference = A", false, departureCollection.Contains(departure5));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentReference, "B", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 2, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has an Additional Document with Reference = B", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter as it has no Additional Documents with Reference = B", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter as it has no Additional Documents with Reference = B", false, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is in the filter cause has an Additional Document with Reference = B", true, departureCollection.Contains(departure5));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDocumentReference, "C", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter as it has no Additional Document with its Reference = C", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter as it has no Additional Documents", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter as it has an Additional Documents with its Reference = C", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter as it has no Additional Documents with Reference = C", false, departureCollection.Contains(departure4));
				AssertEquals("Departure 5 is not in the filter as it has no Additional Documents with Reference = C", false, departureCollection.Contains(departure5));
			});
		}

		public void TestPreviousDocumentsFilterClass()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem = bill1.GoodsItems.AddNew();
			goodItem.PreviousDocuments.AddNew().CSI_SubType = "A";
			goodItem.PreviousDocuments.AddNew().CSI_SubType = "B";

			bill1.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_SubType = "C";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_SubType = "A";
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_SubType = "B";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			bill3.PreviousDocuments.AddNew().CSI_SubType = "A";

			var departure4 = GetNewDeparture();
			departure4.PreviousDocuments.AddNew().CSI_SubType = "B";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 4, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentClass, "A", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Class is A", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its Class is A", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has a Previous Document with its Class is A", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has not a Previous Document with its Class is A", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentClass, "B", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Class is B", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its Class is B", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has not a Previous Document with its Class is B", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has a Previous Document with its Class is B", true, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentClass, "C", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Class is C", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its Class is C", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has not a Previous Document with its Class is C", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has not a Previous Document with its Class is C", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentClass, "D", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Previous Document with its Class is D", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its Class is D", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has not a Previous Document with its Class is D", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has not a Previous Document with its Class is D", false, departureCollection.Contains(departure4));
			});
		}

		public void TestPreviousDocumentsFilterType()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem = bill1.GoodsItems.AddNew();
			goodItem.PreviousDocuments.AddNew().CSI_Code = "DUA";
			goodItem.PreviousDocuments.AddNew().CSI_Code = "SUM";
			bill1.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_Code = "ADD";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_Code = "DUA";
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_Code = "SUM";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			bill3.PreviousDocuments.AddNew().CSI_Code = "DUA";

			var departure4 = GetNewDeparture();
			departure4.PreviousDocuments.AddNew().CSI_Code = "SUM";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 4, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentType, "DUA", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Type is DUA", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its Type is DUA", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has a Previous Document with its Type is DUA", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its Type is DUA", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentType, "SUM", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Type is SUM", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its Type is SUM", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its Type is SUM", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has a Previous Document with its Type is SUM", true, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentType, "ADD", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Type is ADD", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its Type is ADD", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its Type is ADD", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its Type is ADD", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentType, "ZZZ", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Previous Document with its Type is ZZZ", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its Type is ZZZ", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its Type is ZZZ", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its Type is ZZZ", false, departureCollection.Contains(departure4));
			});
		}

		public void TestPreviousDocumentsFilterReference()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem = bill1.GoodsItems.AddNew();
			goodItem.PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference1";
			goodItem.PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference2";
			bill1.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference3";

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference1";
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference2";

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			bill3.PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference1";

			var departure4 = GetNewDeparture();
			departure4.PreviousDocuments.AddNew().CSI_ReferenceNumber = "Reference2";

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 4, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentRef, "Reference1", departureCollection, stripBO);

				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Reference is Reference1", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its Reference is Reference1", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has a Previous Document with its Reference is Reference1", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its Reference is Reference1", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentRef, "Reference2", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Reference is Reference2", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its Reference is Reference2", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its Reference is Reference2", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has a Previous Document with its Reference is Reference2", true, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentRef, "Reference3", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its Reference is Reference3", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its Reference is Reference3", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its Reference is Reference3", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its Reference is Reference3", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentRef, "Reference4", departureCollection, stripBO);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Previous Document with its Reference is Reference4", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its Reference is Reference4", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its Reference is Reference4", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its Reference is Reference4", false, departureCollection.Contains(departure4));
			});
		}

		public void TestPreviousDocumentsFilterLineNo()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			var goodItem = bill1.GoodsItems.AddNew();
			goodItem.PreviousDocuments.AddNew().CSI_LineNo = 1;
			goodItem.PreviousDocuments.AddNew().CSI_LineNo = 2;
			bill1.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_LineNo = 3;

			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_LineNo = 1;
			bill2.GoodsItems.AddNew().PreviousDocuments.AddNew().CSI_LineNo = 2;

			var departure3 = GetNewDeparture();
			var bill3 = departure3.Bills.AddNew();
			bill3.PreviousDocuments.AddNew().CSI_LineNo = 1;

			var departure4 = GetNewDeparture();
			departure4.PreviousDocuments.AddNew().CSI_LineNo = 2;

			Factory.Save();

			var departureCollection = new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			departureCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] NCTS Total Declarations", 4, departureCollection.Count);

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentLineNo, "1", departureCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its LineNo is 1", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its LineNo is 1", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is in the filter cause has a Previous Document with its LineNo is 1", true, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its LineNo is 1", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentLineNo, "2", departureCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
				AssertEquals("Total NCTS Declarations match the filter", 3, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its LineNo is 2", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is in the filter cause has a Previous Document with its LineNo is 2", true, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its LineNo is 2", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is in the filter cause has a Previous Document with its LineNo is 2", true, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentLineNo, "3", departureCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
				AssertEquals("Total NCTS Declarations match the filter", 1, departureCollection.Count);
				AssertEquals("Departure 1 is in the filter cause has a Previous Document with its LineNo is 3", true, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its LineNo is 3", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its LineNo is 3", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its LineNo is 3", false, departureCollection.Contains(departure4));

				LoadNCTSHeaderCollectionTextFilter(NctsMovementFilterStripBusinessObject.FilterConstants.PreviousDocumentLineNo, "4", departureCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
				AssertEquals("Total NCTS Declarations match the filter", 0, departureCollection.Count);
				AssertEquals("Departure 1 is not in the filter cause has not a Previous Document with its LineNo is 4", false, departureCollection.Contains(departure1));
				AssertEquals("Departure 2 is not in the filter cause has not a Previous Document with its LineNo is 4", false, departureCollection.Contains(departure2));
				AssertEquals("Departure 3 is not in the filter cause has no Previous Document with its LineNo is 4", false, departureCollection.Contains(departure3));
				AssertEquals("Departure 4 is not in the filter cause has no Previous Document with its LineNo is 4", false, departureCollection.Contains(departure4));
			});
		}

		public void TestBillingFilters()
		{
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull("AP Invoice # Filter", stripBO["AP Invoice #"]);
				AssertNotNull("AR Transaction # Filter", stripBO["AR Transaction #"]);
				AssertNotNull("Charges with Creditor Filter", stripBO["Charges with Creditor"]);
				AssertNotNull("Charges with Debtor Filter", stripBO["Charges with Debtor"]);
				AssertNotNull("Supplier Cost Reference Filter", stripBO["Supplier Cost Reference"]);
			});
		}

		public void TestIAccountingFilterStripHolderMembers()
		{
			var stripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var accountingFilterStripHolder = stripBO as IAccountingFilterStripHolder;
			AssertNotNull("NctsMovementFilterStripBusinessObject as IAccountingFilterStripHolder", accountingFilterStripHolder);

			AssertEquals("AccountingFilterStripConfiguration Count", 0, accountingFilterStripHolder.AccountingFilterStripConfiguration.Count);
			AssertNull("AmountFiltersCategoryNameOveride", accountingFilterStripHolder.AmountFiltersCategoryNameOveride);
			AssertNull("BillingFiltersCategoryNameOveride", accountingFilterStripHolder.BillingFiltersCategoryNameOveride);
			AssertSame("Factory", stripBO.Factory, accountingFilterStripHolder.Factory);
			AssertNull("FilterNameSuffixInOtherCategories", accountingFilterStripHolder.FilterNameSuffixInOtherCategories);
			AssertEquals("IsFilterStripForParentTable", true, accountingFilterStripHolder.IsFilterStripForParentTable);
			var topLevelBusinessObjectQuery = accountingFilterStripHolder.TopLevelBusinessObjectQuery(new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID));
			AssertEquals("TopLevelBusinessObjectQuery (CusInBondHeader.BH_PK added to the original SubQuery)", "BH_PK IN (SELECT JH_ParentID FROM dbo.JobHeader)", topLevelBusinessObjectQuery.LiteralTextADO);
		}

		public void TestReleaseDateFilter() => CombineAssertions(() =>
		{
			var testDate = new ZDate(2023, 8, 15);

			var departure1 = GetNewDeparture();
			departure1.MovementReferenceEntryNumber.CE_EntryNum = "MRN1";
			departure1.MovementReferenceEntryNumber.CE_IssueDate = testDate.AddDays(-1);
			var departure2 = GetNewDeparture();
			departure2.MovementReferenceEntryNumber.CE_EntryNum = "MRN2";
			departure2.MovementReferenceEntryNumber.CE_IssueDate = testDate;
			var departure3 = GetNewDeparture();
			departure3.MovementReferenceEntryNumber.CE_EntryNum = "MRN3";
			departure3.MovementReferenceEntryNumber.CE_IssueDate = testDate.AddDays(+1);
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MRNReleaseDate];
			AssertNotNull(dateFilter);
			AssertEquals("Category", FilterCategories.Dates, dateFilter.Category);

			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			AssertNotNull("MultilingualDescription", dateFilter.MultilingualDescription);

			dateFilter.Property1 = testDate;
			dateFilter.Property2 = testDate;
			AssertEquals(AssertionMessage(departure1, "departure1 does not match filter"), false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage(departure2, "departure2 does match filter"), true, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage(departure3, "departure3 does not match filter"), false, departure3.MatchesFilter(filterStripBO.Filter));

			dateFilter.Property1 = testDate.AddMonths(1);
			dateFilter.Property2 = testDate.AddMonths(1);
			AssertEquals(AssertionMessage(departure1, "departure1 does not match filter"), false, departure1.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage(departure2, "departure2 does not match filter"), false, departure2.MatchesFilter(filterStripBO.Filter));
			AssertEquals(AssertionMessage(departure3, "departure3 does not match filter"), false, departure3.MatchesFilter(filterStripBO.Filter));

			string AssertionMessage(NctsHeader departure, string message) => $"{departure.MovementReferenceEntryNumber.CE_IssueDate}: {dateFilter.Property1} - {dateFilter.Property2}; {message}";
		});

		public void TestAdditionalIdentifierFilter()
		{
			var departure = GetNewDeparture();
			var arrival = GetNewArrival();
			departure.CusGoodsLocation.CGL_AdditionalIdentifier = "DEP";
			arrival.CusGoodsLocation.CGL_AdditionalIdentifier = "ARR";
			Factory.Save();

			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalIdentifier];

			CombineAssertions(() =>
			{
				AssertNotNull(textFilter);
				textFilter.IsActive = true;
				textFilter.Property = "DEP";
				AssertEquals("departure does match filter", true, departure.MatchesFilter(filterStripBO.Filter));
				AssertEquals("arrival does not match filter", false, arrival.MatchesFilter(filterStripBO.Filter));
				textFilter.Property = "ARR";
				AssertEquals("arrival does match filter", true, arrival.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure does not match filter", false, departure.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestAdditionalIdentifierFilterProperties()
		{
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalIdentifier];

			CombineAssertions(() =>
			{
				AssertNotNull(textFilter);
				AssertEquals("Additional Identifier filter description", NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalIdentifier, textFilter.Description);
				AssertEquals("Additional Identifier filter category", FilterCategories.TextSearch, textFilter.Category);
			});
		}

		public void TestUniqueConsignmentReferenceFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_UniqueConsignmentReference = "UCR 1";
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_UniqueConsignmentReference = "UCR 2";
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.UniqueConsignmentReference];
			AssertNotNull(filter);
			filter.IsActive = true;
			CombineAssertions("UniqueConsignmentReference filter", () =>
			{
				AssertEquals("MultilingualDescription", "Unique Consignment Reference (UCR)", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.TextSearch, filter.Category);
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "UCR";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "UCR 1";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				filter.Property = "UCR 2";
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestAdditionalDeclarationTypeFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.AdditionalDeclarationType];
			AssertNotNull(filter);
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter.IsActive = true;
			CombineAssertions("AdditionalDeclarationType filter", () =>
			{
				AssertEquals($"Value List", "A, D", ((CodeDescriptionPairList)filter.List).CodesAsString);
				AssertEquals("MultilingualDescription", "Additional Declaration Type", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.ModesAndTypes, filter.Category);
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = NctsTypeOfAdditionalDeclarationList.Codes.A;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = NctsTypeOfAdditionalDeclarationList.Codes.D;
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestAuthorisationNumberFilter()
		{
			var departure1 = GetNewDeparture();
			var auth1 = departure1.MovementHeader.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "AAA";
			auth1.AGC_Number = "AUTH 1";
			var departure2 = GetNewDeparture();
			var auth2 = departure2.MovementHeader.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "AAA";
			auth2.AGC_Number = "AUTH 2";
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.AuthorisationNumber];
			AssertNotNull(filter);
			filter.IsActive = true;
			CombineAssertions("AuthorisationNumber filter", () =>
			{
				AssertEquals("MultilingualDescription", "Authorization Number", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.TextSearch, filter.Category);
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "AUTH";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "AUTH 1";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				filter.Property = "AUTH 2";
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestTransportAtDepartureFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.TransportAtDeparture = "Transport ID 1";
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.TransportAtDeparture = "Transport ID 2";
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.TransportAtDeparture];
			AssertNotNull(filter);
			filter.IsActive = true;
			CombineAssertions("UniqueConsignmentReference filter", () =>
			{
				AssertEquals("MultilingualDescription", "Departure Transport ID", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.TextSearch, filter.Category);
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "Transport ID";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "Transport ID 1";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				filter.Property = "Transport ID 2";
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestTypeOfSecurityFilter()
		{
			var departure1 = GetNewDeparture();
			departure1.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			var departure2 = GetNewDeparture();
			departure2.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.TypeOfSecurity];
			AssertNotNull(filter);
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter.IsActive = true;
			CombineAssertions("AdditionalDeclarationType filter", () =>
			{
				AssertEquals($"Value List", "NON, ENT, EXI, BTH", ((CodeDescriptionPairList)filter.List).CodesAsString);
				AssertEquals("MultilingualDescription", "Security type (phase 5)", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.ModesAndTypes, filter.Category);
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = NctsTypeOfSecurityList.Codes.BTH;
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestConsignmentBillNumberFilter()
		{
			var departure1 = GetNewDeparture();
			var bill1 = departure1.Bills.AddNew();
			bill1.B0_ReferenceID  = "Bill 1";
			var departure2 = GetNewDeparture();
			var bill2 = departure2.Bills.AddNew();
			bill2.B0_ReferenceID = "Bill 2";
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.ConsignmentBillNumber];
			AssertNotNull(filter);
			filter.IsActive = true;
			CombineAssertions("ConsignmentBillNumber filter", () =>
			{
				AssertEquals("MultilingualDescription", "Consignment/Bill Number", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.TextSearch, filter.Category);
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "Bill";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = "Bill 1";
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
				filter.Property = "Bill 2";
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestDeclarationBranchFilter()
		{
			var departure1 = GetNewDeparture();
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "GB1";
			departure1.BH_GB = branch1.PK;
			var departure2 = GetNewDeparture();
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "GB2";
			departure2.BH_GB = branch2.PK;
			Factory.Save();
			var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.DeclarationBranch];
			AssertNotNull(filter);
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			filter.IsActive = true;
			CombineAssertions("DeclarationBranch filter", () =>
			{
				AssertEquals("MultilingualDescription", "Declaration Branch", filter.MultilingualDescription);
				AssertEquals("Category", FilterCategories.Organisations, filter.Category);
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = branch1.PK;
				AssertEquals("departure1 does match filter", true, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does not match filter", false, departure2.MatchesFilter(filterStripBO.Filter));
				filter.Property = branch2.PK;
				AssertEquals("departure1 does not match filter", false, departure1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("departure2 does match filter", true, departure2.MatchesFilter(filterStripBO.Filter));
			});
		}

		public void TestGetAdditionalDocumentCodeListTypesCore()
		{
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "TD44N", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, "AI44N", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "AR44N", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N787AA", "N787", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N705AA", "N705", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, "20100AA", "20100", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, "20200AA", "20200", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "Y903AA", "Y903", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, "Y909AA", "Y909", startDate, endDate);
			Factory.Save();

			var filterStrip = new NctsMovementFilterStripBusinessObjectForTest();

			CombineAssertions(() =>
			{
				AssertDocumentTypes(ZString.Empty, new ZString[] { "N787AA", "N705AA", "20100AA", "20200AA", "Y903AA", "Y909AA" });
				AssertDocumentTypes(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation, new ZString[] { "20100AA", "20200AA" });
				AssertDocumentTypes(EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument, new ZString[] { "N787AA", "N705AA" });
				AssertDocumentTypes(EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference, new ZString[] { "Y903AA", "Y909AA" });
			});

			void AssertDocumentTypes(ZString addDocKind, IEnumerable<ZString> expectedCodeLists)
			{
				var filteredItems = filterStrip.GetAdditionalDocumentTypes(addDocKind);
				filteredItems.Load();
				var docTypes = filteredItems.Select(y => y.ZZD_Code);

				foreach (var code in expectedCodeLists)
				{
					AssertCollectionContains(code, docTypes);
				}
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NctsMovementFilterStripBusinessObject();

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			exclusions.Add(TableFilter(JobDocAddressSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.Consignor));
			exclusions.Add(TableFilter(JobDocAddressSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.Consignee));
			exclusions.Add(TableFilter(JobDocAddressSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.DestinationTrader));

			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.Consignor));
			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.Consignee));
			exclusions.Add(TableFilter(OrgAddressSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.DestinationTrader));

			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.Consignor));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.Consignee));
			exclusions.Add(TableFilter(OrgHeaderSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.DestinationTrader));

			exclusions.Add(TableFilter(CusInBondMoveHeaderSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber));

			exclusions.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber));

			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = base.GetFiltersExcludedFromSubgroupCheck();

			exclusions.Add(TableFilter(CusInBondMoveHeaderSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber));

			exclusions.Add(TableFilter(GenAddOnColumnSchema.Constants.TableName, NctsMovementFilterStripBusinessObject.FilterConstants.LocalReferenceNumber));

			return exclusions;
		}

		NctsHeader GetNewDeparture()
		{
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(NctsMovementType.Codes.Departure);
			return departure;
		}

		NctsHeader GetNewArrival()
		{
			var arrival = Factory.New<NctsHeader>();
			arrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrival.SetMovementType(NctsMovementType.Codes.Arrival);
			return arrival;
		}

		void AssertMatchesFilter(NctsHeader header1, NctsHeader header2, ZGuid org1Pk, ZGuid org2Pk, ZString filterName)
		{
			CombineAssertions(() =>
			{
				var filterStripBO = new NctsMovementFilterStripBusinessObject();
				var guidFilter = (ModuleGuidFilter)filterStripBO[filterName];
				AssertNotNull(guidFilter);
				guidFilter.IsActive = true;
				AssertNotNull("MultilingualDescription", guidFilter.MultilingualDescription);

				guidFilter.Property = org1Pk;
				AssertEquals("header1 does match filter", true, header1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("header2 does not match filter", false, header2.MatchesFilter(filterStripBO.Filter));

				guidFilter.Property = org2Pk;
				AssertEquals("header1 does not match filter", false, header1.MatchesFilter(filterStripBO.Filter));
				AssertEquals("header2 does match filter", true, header2.MatchesFilter(filterStripBO.Filter));
			});
		}

		void AssertMatchesFilter(NctsHeader header1, NctsHeader header2, string filterValue, ZString filterName)
		{
			CombineAssertions(() =>
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStrip[filterName];

				AssertNotNull(filter);
				filter.IsActive = true;
				AssertNotNull("MultilingualDescription", filter.MultilingualDescription);

				filter.Property = filterValue;
				AssertEquals("header1 does match filter", true, header1.MatchesFilter(filterStrip.Filter));
				AssertEquals("header2 does not match filter", false, header2.MatchesFilter(filterStrip.Filter));
			});
		}

		void AssertMatchesFilterIsBlank(NctsHeader header1, NctsHeader header2, ZString filterName)
		{
			CombineAssertions(() =>
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				var filter = (ModuleTextFilter)filterStrip[filterName];

				AssertNotNull(filter);
				filter.IsActive = true;
				AssertNotNull("MultilingualDescription", filter.MultilingualDescription);

				filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
				AssertEquals("header1 does not match filter", false, header1.MatchesFilter(filterStrip.Filter));
				AssertEquals("header2 does match filter", true, header2.MatchesFilter(filterStrip.Filter));
			});
		}

		void LoadNCTSHeaderCollectionTextFilter(ZString filterField, ZString filterValue, NctsHeaderCollection nctsHeaderCollection, NctsMovementFilterStripBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
		{
			var filter = (ModuleTextFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = comparisonOperator;
			filter.IsActive = true;
			nctsHeaderCollection.Load(stripBO.Filter);
		}

		void LoadNCTSHeaderCollectionNkFilter(ZString filterField, ZString filterValue, NctsHeaderCollection nctsHeaderCollection, NctsMovementFilterStripBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
		{
			var filter = (ModuleNkFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = comparisonOperator;
			filter.IsActive = true;
			nctsHeaderCollection.Load(stripBO.Filter);
		}

		IDisposable SubstituteNctsSettings(bool isUsingPhase5)
		{
			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(isUsingPhase5);
			return (ObjectFactory.Substitute(mockSettings.Object));
		}

		class NctsMovementFilterStripBusinessObjectForTest : NctsMovementFilterStripBusinessObject
		{
			public ZZRefCusCodeListCombinedCollection GetAdditionalDocumentTypes(ZString additionalDocumentKind) => GetAdditionalDocumentTypesCore(additionalDocumentKind);
		}
	}
}
