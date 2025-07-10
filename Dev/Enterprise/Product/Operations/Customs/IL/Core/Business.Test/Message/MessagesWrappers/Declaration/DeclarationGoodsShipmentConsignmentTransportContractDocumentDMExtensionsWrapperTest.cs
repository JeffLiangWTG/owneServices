using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper>
	{
		public void TestSecondCargoId()
		{
			AssertNull("SecondCargoId", Provider.SecondCargoId);

			jobDeclaration.JE_TransportMode = TransportModes.Air;
			jobDeclaration.JE_MasterBill = "12345678";
			AssertEquals("SecondCargoId should be equal to JE_MasterBill", "12345678", Provider.SecondCargoId.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Sea;
			AssertNull("SecondCargoId", Provider.SecondCargoId);

			var cusEntryNumber = CusEntryNumber.New(jobDeclaration, IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber, CountryCodes.Israel);
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_EntryNum = "9876543210";
			AssertEquals("SecondCargoId should be equal to cusEntryNumber.CE_EntryNum of type FDN", "9876543210", Provider.SecondCargoId.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Road;
			AssertNull("SecondCargoId", Provider.SecondCargoId);
		}

		public void TestThirdCargoId()
		{
			AssertNull("ThirdCargoId", Provider.ThirdCargoId);

			jobDeclaration.JE_TransportMode = TransportModes.Air;
			jobDeclaration.JE_HouseBill = "AAA 1234-56/78";
			AssertEquals("ThirdCargoId should be equal to only digits from JE_HouseBill", "12345678", Provider.ThirdCargoId.Value);

			jobDeclaration.JE_TransportMode = TransportModes.Sea;
			AssertNull("ThirdCargoId", Provider.ThirdCargoId);

			jobDeclaration.JE_TransportMode = TransportModes.Road;
			AssertNull("ThirdCargoId", Provider.ThirdCargoId);
		}

		public void TestNewOrNull()
		{
			AssertNull("Provider", DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper.NewOrNull(null));
			AssertNotNull("Provider", DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper.NewOrNull(Factory.New<JobDeclaration>()));
		}

		protected override DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper GetProvider()
		{
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_GoodsOrigin = "IL";

			return DeclarationGoodsShipmentConsignmentTransportContractDocumentDMExtensionsWrapper.NewOrNull(jobDeclaration);
		}

		JobDeclaration jobDeclaration;
	}
}
