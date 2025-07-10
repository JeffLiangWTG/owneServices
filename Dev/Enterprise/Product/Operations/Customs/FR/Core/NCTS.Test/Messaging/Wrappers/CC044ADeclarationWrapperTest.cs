using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC0044AWrapper))]
	sealed class CC044ADeclarationWrapperTest : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC0044AWrapper>
	{
		public void TestDestinationTrader()
		{
			var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
			destinationTrader.OH_FullName = "destinationTrader NAME";
			destinationTrader.MainAddress.Address1 = "10234-119 BOULEVARD FELIX FAURE STREET";
			destinationTrader.MainAddress.City = "SAINT-REMY-EN-BOUZEMONT-SAINT-GENEST-ET-ISSON";
			destinationTrader.MainAddress.Postcode = "00937-0616";
			destinationTrader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			destinationTrader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			destinationTrader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);

			header.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

			AssertEquals("destinationTrader NAME", wrapper.DestinationTrader.Name);
			AssertEquals("SAINT-REMY-EN-BOUZEMONT-SAINT-GENES", wrapper.DestinationTrader.City);
			AssertEquals(Core.Constants.CountryCodes.France, wrapper.DestinationTrader.CountryCode);
			AssertEquals("00937-061", wrapper.DestinationTrader.PostalCode);
			AssertEquals("EN", wrapper.DestinationTrader.NameAndAddressLanguage);
			AssertEquals("10234-119 BOULEVARD FELIX FAURE STR", wrapper.DestinationTrader.StreetAndNumber);
			AssertEquals("FR12345678900001", wrapper.DestinationTrader.TIN);
		}

		public void TestAgreementNumber_IsArrival()
		{
			var org = DeclarationWrapperHelperTest.CreateOrgHeaderWithDTA(Factory, "AC0003");
			header.Declarant.E2_OA_Address = ZGuid.Empty;
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("AC0003", wrapper.AgreementNumber);
		}

		public void TestUnloadedGoodsItems()
		{
			header.UnloadingMovementHeader.GoodsItems.AddNew();
			header.UnloadingMovementHeader.GoodsItems.AddNew();
			AssertEquals(2, wrapper.UnloadedGoodsItems.Count);
		}

		public void TestUnloadedGoodsItemsOrder()
		{
			var item1 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			item1.BY_LineNo = 3;
			var item2 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			item2.BY_LineNo = 2;
			var item3 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			item3.BY_LineNo = 1;
			var item4 = header.UnloadingMovementHeader.GoodsItems.AddNew();
			item4.BY_LineNo = 10;

			AssertEquals(4, wrapper.UnloadedGoodsItems.Count);

			var listOfGoodItem = wrapper.UnloadedGoodsItems;
			AssertEquals(1, listOfGoodItem.ElementAt(0).ItemNumber);
			AssertEquals(2, listOfGoodItem.ElementAt(1).ItemNumber);
			AssertEquals(3, listOfGoodItem.ElementAt(2).ItemNumber);
			AssertEquals(10, listOfGoodItem.ElementAt(3).ItemNumber);
		}

		public void TestExpectedGoodsItems()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var wrapper = new CC0044AWrapper(header);
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			header.ArrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(2, wrapper.ExpectedGoodsItems.Count);
		}

		public void TestExpectedGoodsItemsOrder()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var wrapper = new CC0044AWrapper(header);
			var item1 = header.ArrivalMovementHeader.GoodsItems.AddNew();
			item1.BY_LineNo = 3;
			var item2 = header.ArrivalMovementHeader.GoodsItems.AddNew();
			item2.BY_LineNo = 2;
			var item3 = header.ArrivalMovementHeader.GoodsItems.AddNew();
			item3.BY_LineNo = 1;

			AssertEquals(3, wrapper.ExpectedGoodsItems.Count);

			var listOfGoodItem = wrapper.ExpectedGoodsItems;
			AssertEquals(1, listOfGoodItem.ElementAt(0).ItemNumber);
			AssertEquals(2, listOfGoodItem.ElementAt(1).ItemNumber);
			AssertEquals(3, listOfGoodItem.ElementAt(2).ItemNumber);
		}

		public void TestListOfDifferenceInHeader()
		{
			var arrivalMovementHeader = header.ArrivalMovementHeader;
			var unloadingMovementHeader = header.UnloadingMovementHeader;
			header.UnloadingRemark.G9_Conform = Customs.Business.YesNoList.Codes.No;
			header.UnloadedMeansOfTransportAtDepartureIdentity = "1";
			arrivalMovementHeader.BM_TransportAtDeparture = "2";

			arrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "FR";
			header.UnloadedMeansOfTransportAtDepartureNationality = "GB";

			var goodsItem1 = unloadingMovementHeader.GoodsItems.AddNew();
			var package1 = goodsItem1.Packages.AddNew();
			package1.B5_UnitCount = 20;
			var goodsItem2 = arrivalMovementHeader.GoodsItems.AddNew();
			var package3 = goodsItem2.Packages.AddNew();
			package3.B5_UnitCount = 50;

			unloadingMovementHeader.BM_GrossWeight = 5m;
			var item1 = unloadingMovementHeader.GoodsItems.AddNew();
			item1.BY_GrossWeight = 2.5;
			item1.BY_GrossWeightUnit = "KG";
			var item2 = unloadingMovementHeader.GoodsItems.AddNew();
			item2.BY_GrossWeight = 2.5;
			item2.BY_GrossWeightUnit = "KG";

			arrivalMovementHeader.BM_GrossWeight = 3m;
			var item3 = arrivalMovementHeader.GoodsItems.AddNew();
			item3.BY_GrossWeight = 3;
			item3.BY_GrossWeightUnit = "KG";

			var expectedValues = new Tuple<string, string>[]
			{
				new Tuple<string, string>("18", "1"),
				new Tuple<string, string>("18#1", "GB"),
				new Tuple<string, string>("5", "3"),
				new Tuple<string, string>("6", "20"),
				new Tuple<string, string>("35", "5"),
			};

			AssertContainsExactElementsInAnyOrder(expectedValues, wrapper.ListOfDifferenceInHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			wrapper = new CC0044AWrapper(header);
		}
		ICC044ADeclaration wrapper;
		NctsHeader header;

		protected override CC0044AWrapper GetProvider() => (CC0044AWrapper)wrapper;
	}
}
