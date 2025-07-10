using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItemTransportEquipmentSeal>
	{
		public void TestId()
		{
			AssertEquals("ID must have expected value", "Seal1", Provider.Id.Value);
		}

		public void TestConditionCode()
		{
			CreateSealUnloadingStateMap();

			AssertConditionCode(1, "DEC");
			AssertConditionCode(1, "NEW");
			AssertConditionCode(2, "DAM");
			AssertConditionCode(3, "MIS");
			AssertConditionCode(4, "DIF");
		}

		public void TestTypeCode()
		{
			AssertTypeCode(6, "E", "CAR");
			AssertTypeCode(6, "E", "CRD");
			AssertTypeCode(6, "E", "QRT");
			AssertTypeCode(6, "E", "CTO");
			AssertTypeCode(2, "E", "CUS");
			AssertTypeCode(5, "M", "CAR");
			AssertTypeCode(5, "M", "CRD");
			AssertTypeCode(5, "M", "QRT");
			AssertTypeCode(5, "M", "CTO");
			AssertTypeCode(1, "M", "CUS");
			AssertTypeCode(7, "R", "CAR");
			AssertTypeCode(7, "R", "CRD");
			AssertTypeCode(7, "R", "QRT");
			AssertTypeCode(7, "R", "CTO");
			AssertTypeCode(3, "R", "CUS");
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(null, null, "NEW", null, null));
			AssertNull(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(Factory, null, "NEW", null, null));
			AssertNull(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(null, "Seal", "NEW", null, null));
			AssertNotNull(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(Factory, "Seal", null, null, null));
			AssertNotNull(DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(Factory, "Seal", "NEW", null, null));
		}

		protected override IDeclarationConsignmentConsignmentItemTransportEquipmentSeal GetProvider() => GetProvider(null, null, null);

		IDeclarationConsignmentConsignmentItemTransportEquipmentSeal GetProvider(string unloadingState, string sealType, string sealPartyType)
			=> DeclarationConsignmentConsignmentItemTransportEquipmentSealWrapper.NewOrNull(Factory, "Seal1", unloadingState, sealType, sealPartyType);

		void AssertConditionCode(int expected, string unloadingState)
		{
			var provider = GetProvider(unloadingState, null, null);
			AssertEquals("Condition Code should be mapped correctly", expected, provider.ConditionCode);
		}

		void AssertTypeCode(int expected, string sealType, string sealPartyType)
		{
			var provider = GetProvider(null, sealType, sealPartyType);
			AssertEquals("Type Code should be mapped correctly", expected, provider.TypeCode);
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
