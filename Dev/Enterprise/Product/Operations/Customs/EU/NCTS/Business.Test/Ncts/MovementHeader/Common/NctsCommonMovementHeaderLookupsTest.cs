using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsCommonMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportNationalityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
			Factory.Save();

			var list = lookups.TransportNationalityList;
			list.Load();
			AssertContainsExactElementsInAnyOrder("Values", new ZString[] { "AU", "DE", "FR" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestDeclarationTypeList()
		{
			var declarationTypeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NCTSDeclarationType;
			var eurpoeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(declarationTypeCode, "NCTS Declaration Type (Box 1)");
			var t1 = helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, declarationTypeCode, "T1", "Goods moving under external community transit procedure", startDate, endDate);
			var t2 = helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, declarationTypeCode, "T2", "Goods moving under internal community transit procedure", startDate, endDate);

			helper.CreateOrGetLanguage("LT", "Latvian");
			helper.CreateNewOrGetExistingCusCodeListLanguage(t1, "LT", "T1-Waren im externen Versandverfahren");
			helper.CreateNewOrGetExistingCusCodeListLanguage(t2, "LT", "T2-Waren im internen Versandverfahren");

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "LTV";
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var list = lookups.DeclarationTypeList;
				CombineAssertions(() =>
				{
					AssertSame("Cached", list, lookups.DeclarationTypeList);
					AssertEquals("All the EU Codes", "T1, T2", list.CodesAsString);
					AssertEquals("T1 in Latvian", "T1-Waren im externen Versandverfahren", list.GetDescriptionFromCode("T1"));
					AssertEquals("T2 in Latvian", "T2-Waren im internen Versandverfahren", list.GetDescriptionFromCode("T2"));
				});
			}
		}

		public void TestNctsMoveHeaderTypeList()
		{
			var list = lookups.NctsMoveHeaderTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "A, D, U", list.CodesAsString);
				AssertSame("Cached", list, lookups.NctsMoveHeaderTypeList);
			});
		}

		public void TestCountryOfDestinationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDestinationList = lookups.CountryOfDestinationList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfDestinationList.CodesAsString);
				AssertSame("Cached", countryOfDestinationList, lookups.CountryOfDestinationList);
			});
		}

		public void TestNctsTransitStatusList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.NctsTransitStatusList;
				AssertContainsExactElementsInAnyOrder(new NctsTransitStatusList().GetAllCodes(), list.GetAllCodes());
				AssertSame(commonMovement.Header.Lookups.NctsTransitStatusList, list);
				AssertSame("Cached", list, lookups.NctsTransitStatusList);
			});
		}

		public void TestDischargeTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.DischargeTypeList;
				AssertEquals("Values", "FD, PD", list.CodesAsString);
				AssertSame("Cached", list, lookups.DischargeTypeList);
			});
		}

		public void TestCarnetTotalPagesList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.CarnetTotalPagesList;
				AssertEquals("Values", "2, 4, 6, 8, 10, 12, 14, 16, 18, 20", list.CodesAsString);
				AssertSame("Cached", list, lookups.CarnetTotalPagesList);
			});
		}

		public void TestCountryOfDispatchList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDestinationList = lookups.CountryOfDestinationList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfDestinationList.CodesAsString);
				AssertSame("Cached", countryOfDestinationList, lookups.CountryOfDestinationList);
			});
		}

		public void TestNctsMovementHeaderTransactionStatusList()
		{
			var list = lookups.NctsMovementHeaderTransactionStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "007, 013, 014, 015, 043, 044, 141, 170, AAC, ACK, ARR, AWO, CAC, CNT, CRF, DAC, DAJ, DAR, DCC, DCI, DGN, DIS, DMA, DNR, DPJ, DRI, DRJ, DRL, DTJ, FIN, FRC, MAM, MAS, MDS, MNS, MPN, MRI, MRR, MUS, NCK, NRL, PRC, R4A, RYP, STU, URJ", list.CodesAsString);
				AssertSame("Cached", list, lookups.NctsMovementHeaderTransactionStatusList);
			});
		}

		public void TestWeightUnitList()
		{
			var list = lookups.WeightUnitList;
			CombineAssertions(() =>
			{
				AssertEquals(OLookUpEditType.Weight, list.LookupEditType);
				AssertEquals("Codes", "DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN", list.CodesAsString);
				AssertSame("Cached", list, lookups.WeightUnitList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			commonMovement = nctsHeader.MovementHeader;
			lookups = commonMovement.Lookups;
		}
		NctsCommonMovementHeader commonMovement;
		NctsCommonMovementHeaderLookups lookups;
	}
}
