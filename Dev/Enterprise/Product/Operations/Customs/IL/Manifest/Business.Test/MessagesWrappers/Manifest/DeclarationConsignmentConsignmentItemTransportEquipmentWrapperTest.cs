using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemTransportEquipmentWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemTransportEquipment>
	{
		public void TestId()
		{
			AssertEquals("ID must be of expected value", "Cont1", Provider.Id.Value);
		}

		public void TestCharacteristicCode()
		{
			AssertEquals("CharacteristicCode must be of expected value", "4ISO", Provider.CharacteristicCode.Value);
		}

		public void TestFullnessCode_WhenEmpty()
		{
			AssertEquals("FullnessCode must be of expected value", "4", Provider.FullnessCode.Value);
		}

		public void TestFullnessCode_WhenNotEmpty()
		{
			var asycudaContainer = Factory.New<AsycudaContainer>();
			asycudaContainer.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
			var provider = DeclarationConsignmentConsignmentItemTransportEquipmentWrapper.NewOrNull(asycudaContainer);

			AssertEquals("FullnessCode must be of expected value", "5", provider.FullnessCode.Value);
		}

		public void TestSeal()
		{
			CreateSealUnloadingStateMap();

			AssertSeals([("Seal1", "DEC", "E", "CAR")], [("Seal1", 1, 6)]);

			AssertSeals(
				[
					("Seal1", "NEW", "E", "CRD"),
					("Seal2", "DAM", "E", "QRT")
				],
				[
					("Seal1", 1, 6),
					("Seal2", 2, 6)
				]);

			AssertSeals(
				[
					("Seal1", "MIS", "E", "CTO"),
					("Seal2", "DIF", "E", "CUS"),
					("Seal3", "NEW", "M", "CAR")
				],
				[
					("Seal1", 3, 6),
					("Seal2", 4, 2),
					("Seal3", 1, 5)
				]);

			AssertSeals(
				[
					("Seal1", "NEW", "M", "CRD"),
					("Seal2", "NEW", "M", "QRT"),
					("Seal3", "NEW", "M", "CTO"),
					("Seal4", "NEW", "M", "CUS")
				],
				[
					("Seal1", 1, 5),
					("Seal2", 1, 5),
					("Seal3", 1, 5),
					("Seal4", 1, 1)
				]);
		}

		public void TestEventStatusCode()
		{
			AssertNull("EventStatusCode", Provider.EventStatusCode);
		}

		public void TestLegalStatusIndicator()
		{
			AssertNull("LegalStatusIndicator", Provider.LegalStatusIndicator);
		}

		public void TestLicensePlateIssuingCountryCode()
		{
			AssertNull("LicensePlateIssuingCountryCode", Provider.LicensePlateIssuingCountryCode);
		}

		public void TestSealId()
		{
			AssertNull("SealID", Provider.SealId);
		}

		public void TestSupplierPartyTypeCode()
		{
			AssertNull("SupplierPartyTypeCode", Provider.SupplierPartyTypeCode);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemTransportEquipmentWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemTransportEquipmentWrapper.NewOrNull(Factory.New<AsycudaContainer>()));
		}

		protected override IDeclarationConsignmentConsignmentItemTransportEquipment GetProvider()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ContainerType = "RFG";
			refContainer.RC_Code = "45R1";
			refContainer.RC_ISOType = "4ISO";

			var asycudaContainer = Factory.New<AsycudaContainer>();
			asycudaContainer.ACN_RC_ContainerType = refContainer.PK;
			asycudaContainer.ACN_ContainerNumber = "Cont1";
			asycudaContainer.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.EmptyContainer;
			asycudaContainer.ACN_Seal1 = "Seal1";

			return DeclarationConsignmentConsignmentItemTransportEquipmentWrapper.NewOrNull(asycudaContainer);
		}

		void AssertSeals(
			(string seal, string unloadingState, string sealType, string sealPartyType)[] seals,
			(string sealId, int? conditionCode, int? typeCode)[] expectedValues)
		{
			int expectedSealsNumber = 0;
			var asycudaContainer = Factory.New<AsycudaContainer>();

			if (seals.Length > 0)
			{
				asycudaContainer.ACN_Seal1 = seals[0].seal;
				asycudaContainer.ACN_Seal1UnloadingState = seals[0].unloadingState;
				asycudaContainer.ACN_SealType1 = seals[0].sealType;
				asycudaContainer.ACN_SealingPartyType = seals[0].sealPartyType;
				expectedSealsNumber++;
			}

			if (seals.Length > 1)
			{
				asycudaContainer.ACN_Seal2 = seals[1].seal;
				asycudaContainer.ACN_Seal2UnloadingState = seals[1].unloadingState;
				asycudaContainer.ACN_SealType2 = seals[1].sealType;
				asycudaContainer.ACN_SealingPartyType2 = seals[1].sealPartyType;
				expectedSealsNumber++;
			}

			if (seals.Length > 2)
			{
				asycudaContainer.ACN_Seal3 = seals[2].seal;
				asycudaContainer.ACN_Seal3UnloadingState = seals[2].unloadingState;
				asycudaContainer.ACN_SealType3 = seals[2].sealType;
				asycudaContainer.ACN_SealingPartyType3 = seals[2].sealPartyType;
				expectedSealsNumber++;
			}

			for (int i = 3; i < seals.Length; i++)
			{
				var cusSeal = asycudaContainer.AdditionalSeals.AddNew();
				cusSeal.BK_SealNumber = seals[i].seal;
				cusSeal.BK_UnloadingState = seals[i].unloadingState;
				cusSeal.BK_SealType = seals[i].sealType;
				cusSeal.BK_SealingPartyType = seals[i].sealPartyType;
				expectedSealsNumber++;
			}

			var provider = DeclarationConsignmentConsignmentItemTransportEquipmentWrapper.NewOrNull(asycudaContainer);
			AssertNotNull("Seals", provider.Seal);
			AssertEquals($"Seals collection must be of {expectedSealsNumber} element", expectedSealsNumber, provider.Seal.Count);
			AssertContainsExactElementsInAnyOrder(
				"Seals must contain expected elements",
				expectedValues.Select(s => $"{s.sealId},{s.conditionCode},{s.typeCode}"),
				provider.Seal.Select(s => $"{s.Id.Value},{s.ConditionCode},{s.TypeCode}"));
		}

		void CreateSealUnloadingStateMap()
		{
			var factory = Factory;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusMapType("SULST", "BTH", "Seal Unloading State Mapping", true);
			helper.CreateCusMap("SULST", "DEC", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("SULST", "NEW", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("SULST", "DAM", "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("SULST", "MIS", "3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("SULST", "DIF", "4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			factory.Save();
		}
	}
}
