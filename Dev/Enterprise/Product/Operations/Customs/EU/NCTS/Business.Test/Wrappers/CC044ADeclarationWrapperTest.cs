using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CC044ADeclarationWrapper))]
	sealed class CC044ADeclarationWrapperTest : DeclarationWrapperAbstractTest<CC044ADeclarationWrapper>
	{
		public void TestExpectedGoodsItems()
		{
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(2, wrapper.ExpectedGoodsItems.Count);
		}

		public void TestUnloadingRemark()
		{
			header.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			AssertEquals(YesNoList.Codes.Yes, wrapper.UnloadingRemark.StateOfSealsOk);
		}

		public void TestHeaderUnloadingNotes()
		{
			header.HeaderUnloadingNotes = "noteTest";
			AssertEquals("noteTest", wrapper.HeaderUnloadingNotes);
		}

		public void TestHeaderUnloadingNotesLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.HeaderUnloadingNotesLanguage);
		}

		public void TestControlResultList()
		{
			var resultsOfControlCollection = header.ResultsOfControlCollection;
			var resultsOfControl1 = resultsOfControlCollection.AddNew();
			resultsOfControl1.Data.G9_Description = "DESCRIPTION 1";
			var resultsOfControl2 = resultsOfControlCollection.AddNew();
			resultsOfControl2.Data.G9_Description = "DESCRIPTION 2";
			AssertContainsExactElementsInAnyOrder(new[] { "DESCRIPTION 1", "DESCRIPTION 2" }, wrapper.ControlResultList.Select(x => x.Description));
		}

		public void TestIsProduction()
		{
			AssertEquals(false, wrapper.IsProduction);
		}

		public void TestEnRouteEvents()
		{
			header.EnRouteTransshipments.AddNew();
			header.EnRouteTransshipments.AddNew();
			header.EnRouteTransshipments.AddNew();
			header.EnRouteIncidents.AddNew();
			header.EnRouteIncidents.AddNew();
			header.EnRouteSeals.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Total Events", 3, wrapper.EnRouteEvents.Count);
				AssertEquals("Total Transhipment Events", 3, wrapper.EnRouteEvents.Where(x => x.EnRouteTranshipment != null).Count());
				AssertEquals("Total Incident Events", 2, wrapper.EnRouteEvents.Where(x => x.EnRouteIncident != null).Count());
				AssertEquals("Total Seal Events", 1, wrapper.EnRouteEvents.Where(x => x.EnRouteEventSeal != null).Count());
			});
		}

		public void TestIdentityOfMeansOfTransportAtDeparture()
		{
			header.UnloadedMeansOfTransportAtDepartureIdentity = "REG DEP1";
			AssertEquals("REG DEP1", wrapper.IdentityOfMeansOfTransportAtDeparture);
		}

		public void TestIdentityOfMeansOfTransportAtDepartureLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.IdentityOfMeansOfTransportAtDepartureLanguage);
		}

		public void TestNationalityOfMeansOfTransportAtDeparture()
		{
			header.UnloadedMeansOfTransportAtDepartureNationality = Core.Constants.CountryCodes.Denmark;
			AssertEquals(Core.Constants.CountryCodes.Denmark, wrapper.NationalityOfMeansOfTransportAtDeparture);
		}

		public void TestTotalNumberOfItems()
		{
			header.UnloadingMovementHeader.GoodsItems.AddNew();
			header.UnloadingMovementHeader.GoodsItems.AddNew();
			AssertEquals(2, wrapper.TotalNumberOfItems);
		}

		public void TestTotalNumberOfPackages()
		{
			var goodsItem1 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var package2 = goodsItem1.Packages.AddNew();
			package2.B5_UnitCount = 30;
			var goodsItem2 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;
			AssertEquals(100, wrapper.TotalNumberOfPackages);
		}

		public void TestTotalGrossMass()
		{
			header.UnloadingMovementHeader.BM_GrossWeight = 123.45m;
			var item1 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			item1.BY_GrossWeight = 2.5;
			item1.BY_GrossWeightUnit = "KG";
			var item2 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			item2.BY_GrossWeight = 2.5;
			item2.BY_GrossWeightUnit = "KG";
			AssertEquals(123.45m, wrapper.TotalGrossMass);
		}

		public void TestDestinationTrader()
		{
			var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
			destinationTrader.OH_FullName = "destinationTrader NAME";
			destinationTrader.MainAddress.Address1 = "10234-119 BOULEVARD FELIX FAURE STREET";
			destinationTrader.MainAddress.City = "SAINT-REMY-EN-BOUZEMONT-SAINT-GENEST-ET-ISSON";
			destinationTrader.MainAddress.Postcode = "00937-0616";
			destinationTrader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var eori = destinationTrader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "32582075100080999";
			header.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

			AssertEquals("destinationTrader NAME", wrapper.DestinationTrader.Name);
			AssertEquals("SAINT-REMY-EN-BOUZEMONT-SAINT-GENES", wrapper.DestinationTrader.City);
			AssertEquals(Core.Constants.CountryCodes.France, wrapper.DestinationTrader.CountryCode);
			AssertEquals("00937-061", wrapper.DestinationTrader.PostalCode);
			AssertEquals("EN", wrapper.DestinationTrader.NameAndAddressLanguage);
			AssertEquals("10234-119 BOULEVARD FELIX FAURE STR", wrapper.DestinationTrader.StreetAndNumber);
			AssertEquals("FR325820751000809", wrapper.DestinationTrader.TIN);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			wrapper = new CC044ADeclarationWrapper(header);
		}
		CC044ADeclarationWrapper wrapper;
		NctsHeader header;

		protected override CC044ADeclarationWrapper GetProvider()
		{
			header.UnloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			return new CC044ADeclarationWrapper(header);
		}
	}
}
