using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE170ConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE170ConsignmentProvider>
	{
		public void TestContainerIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals(0, nctsHeader.DepartureHeaderContainers.Count);
				AssertEquals("No containers on header", false, Provider.ContainerIndicator);

				var container = nctsHeader.DepartureHeaderContainers.AddNew();
				AssertEquals("Container number not set", false, Provider.ContainerIndicator);

				container.BC_ContainerNum = "CONT1234T";
				AssertEquals("Container number set", true, Provider.ContainerIndicator);
			});
		}

		public void TestInlandModeOfTransport()
		{
			Provider.MovementHeader.BM_InlandTransportMode = "3";
			AssertEquals("InlandModeOfTransport", "3", Provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			Provider.MovementHeader.BM_ExportTransportMode = "1";
			AssertEquals("ModeOfTransportAtTheBorder", "1", Provider.ModeOfTransportAtTheBorder);
		}

		public void TestTransportEquipment()
		{
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "EIRU1909897";
			container1.Seal1 = "1111";
			container1.Seal2 = "2222";
			var addSeal = container1.AdditionalSeals.AddNew();
			addSeal.BK_SealNumber = "3333";

			var bill1 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_Description = "Item Description";
			item1.BY_LineNo = 25;
			item1.Packages.AddNew().ContainersPivot.AddPivotFor(container1);

			AssertEquals(1, Provider.TransportEquipment.Count);
			CombineAssertions(() =>
			{
				var transportEquipment1 = Provider.TransportEquipment.First();
				AssertEquals(3, transportEquipment1.Seals.Count);
				AssertEquals("EIRU1909897", transportEquipment1.ContainerIdentificationNumber);
				AssertCollectionContains("1111", transportEquipment1.Seals);
				AssertCollectionContains("2222", transportEquipment1.Seals);
				AssertCollectionContains("3333", transportEquipment1.Seals);

				AssertEquals(1, transportEquipment1.GoodsReferences.Count);
				AssertCollectionContains("25", transportEquipment1.GoodsReferences);
			});
		}

		public void TestTransportEquipment_Multiple()
		{
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "EIRU1909897";
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "EIRU1909886";
			AssertEquals(2, Provider.TransportEquipment.Count);
		}

		public void TestLocationOfGoods()
		{
			AssertType<LocationOfGoodsProvider>("LocationOfGoods", Provider.LocationOfGoods);
		}

		public void TestDepartureTransportMeans()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			movementHeader.BM_TransportAtDeparture = "777";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = "IE";
			movementHeader.BM_TransportAtDepartureTrailer1RegNo = "AS1111";
			movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "XI";
			movementHeader.BM_TransportAtDepartureTrailer2RegNo = "AS2222";
			movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = "XI";
			AssertType<ITransportMeans[]>("Departure Transport Means", Provider.DepartureTransportMeans);
			AssertEquals(3, Provider.DepartureTransportMeans.Count);
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertType<ActiveTransportMeansProvider>("ActiveBorderTransportMeans", Provider.ActiveBorderTransportMeans.FirstOrDefault());
		}

		public void TestPlaceOfLoading()
		{
			AssertType<PortProvider>("PlaceOfLoading", Provider.PlaceOfLoading);
		}

		public void TestPlaceOfLoading_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				movementHeader.BM_PortOfPresentationCode = "IEGWY";
				movementHeader.BM_PlaceOfLoading = "Galway";
				movementHeader.BM_TypeOfSecurity = "NON";
				AssertEquals("Removed the logic that returns null PlaceOfLoading when BM_TypeOfSecurity=NON", "IEGWY", Provider.PlaceOfLoading.UNLocode);
				AssertNull("Country", Provider.PlaceOfLoading.Country);
				AssertNull("Location", Provider.PlaceOfLoading.Location);
			}
		}

		public void TestPlaceOfLoading_UNLocode_Empty()
		{
			movementHeader.BM_PortOfPresentationCode = "GB";
			movementHeader.BM_PlaceOfLoading = "Pembroke";
			CombineAssertions(() =>
			{
				AssertNull("UNLocode", Provider.PlaceOfLoading.UNLocode);
				AssertEquals("Country", "GB", Provider.PlaceOfLoading.Country);
				AssertEquals("Location", "Pembroke", Provider.PlaceOfLoading.Location);
			});
		}

		public void TestPlaceOfLoading_UNLocode_Set()
		{
			movementHeader.BM_PortOfPresentationCode = "GBPEM";
			movementHeader.BM_PlaceOfLoading = "Pembroke";
			CombineAssertions(() =>
			{
				AssertEquals("UNLocode", "GBPEM", Provider.PlaceOfLoading.UNLocode);
				AssertNull("Country", Provider.PlaceOfLoading.Country);
				AssertNull("Location", Provider.PlaceOfLoading.Location);
			});
		}

		public void TestHouseConsignment()
		{
			AssertType<IE170HouseConsignmentProvider>("HouseConsignment", Provider.HouseConsignment.FirstOrDefault());
		}

		protected override IE170ConsignmentProvider GetProvider() => new IE170ConsignmentProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}

		NctsHeader nctsHeader;
		NctsDepartureMovementHeader movementHeader;
	}
}
