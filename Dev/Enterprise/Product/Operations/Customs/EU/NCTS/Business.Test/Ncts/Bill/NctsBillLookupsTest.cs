using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfExportList = lookups.CountryList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "AU, DE, FR", countryOfExportList.CodesAsString);
				AssertSame("Cached", countryOfExportList, lookups.CountryList);
			});
		}

		public void TestTransportPaymentMethodList()
		{
			var transportPaymentMethodList = lookups.TransportPaymentMethodList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "A, B, C, D, H, Y, Z", transportPaymentMethodList.CodesAsString);
				AssertSame("Cached", transportPaymentMethodList, lookups.TransportPaymentMethodList);
			});
		}

		public void TestUnitOfQuantityList()
		{
			var unitOfQuantityList = lookups.UnitOfQuantityList;
			AssertEquals("CodesAsString", string.Empty, unitOfQuantityList.CodesAsString);
		}

		public void TestWeightUnits()
		{
			var unitOfQuantityList = lookups.WeightUnitList;
			AssertEquals("Weight Units CodesAsString", "DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN", unitOfQuantityList.CodesAsString);
		}

		public void TestVessels()
		{
			CombineAssertions(() =>
			{
				nctsBill.TransportTypeAtDeparture = "10";
				Assert(lookups.Vessels.UseLloyds);
				AssertType<RefVesselCollection>(lookups.Vessels);

				nctsBill.TransportTypeAtDeparture = "11";
				AssertType<RefVesselCollection>(lookups.Vessels);
			});
		}

		public void TestTransportNationalityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);
			Factory.Save();

			var list = lookups.TransportNationalityList;
			list.Load();
			AssertContainsExactElementsInAnyOrder("Values", new ZString[] { "AU", "DE", "FR" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestModeOfTransportList()
		{
			AssertType<ModeOfTransportList>(lookups.ModeOfTransportList);
		}

		public void TestTransportAtDepartureTypeOfIdList()
		{
			var lookup = nctsBill.Lookups.TransportAtDepartureTypeOfIdList;

			AssertEquals("Values", "10, 11, 21, 30, 40, 41, 80, 81, 99", lookup.CodesAsString);
			AssertSame("Cached", lookup, nctsBill.Lookups.TransportAtDepartureTypeOfIdList);
		}

		public void TestTransportAtDepartureTypeOfIdList_MOT1()
		{
			moveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			var lookup = nctsBill.Lookups.TransportAtDepartureTypeOfIdList;

			AssertEquals("Values", "10, 11", lookup.CodesAsString);
			AssertSame("Cached", lookup, nctsBill.Lookups.TransportAtDepartureTypeOfIdList);
		}

		public void TestTransportAtDepartureTypeOfIdList_MOT2()
		{
			moveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			var lookup = nctsBill.Lookups.TransportAtDepartureTypeOfIdList;

			AssertEquals("Values", "20, 21", lookup.CodesAsString);
			AssertSame("Cached", lookup, nctsBill.Lookups.TransportAtDepartureTypeOfIdList);
		}

		public void TestTransportAtDepartureTypeOfIdList_MOT4()
		{
			moveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			var lookup = nctsBill.Lookups.TransportAtDepartureTypeOfIdList;

			AssertEquals("Values", "40, 41", lookup.CodesAsString);
			AssertSame("Cached", lookup, nctsBill.Lookups.TransportAtDepartureTypeOfIdList);
		}

		public void TestTransportAtDepartureTypeOfIdList_MOT8()
		{
			moveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			var lookup = nctsBill.Lookups.TransportAtDepartureTypeOfIdList;

			AssertEquals("Values", "80, 81", lookup.CodesAsString);
			AssertSame("Cached", lookup, nctsBill.Lookups.TransportAtDepartureTypeOfIdList);
		}

		public void TestTransportAtDepartureTypeOfIdList_MOT3()
		{
			moveHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			var lookup = nctsBill.Lookups.TransportAtDepartureTypeOfIdList;

			AssertEquals("Values", "30", lookup.CodesAsString);
			AssertSame("Cached", lookup, nctsBill.Lookups.TransportAtDepartureTypeOfIdList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			moveHeader = nctsHeader.MovementHeader;
			nctsBill = nctsHeader.Bills.AddNew();
			lookups = new NctsBillLookups(nctsBill);
		}
		NctsBillLookups lookups;
		NctsBill nctsBill;
		NctsDepartureMovementHeader moveHeader;
	}
}
