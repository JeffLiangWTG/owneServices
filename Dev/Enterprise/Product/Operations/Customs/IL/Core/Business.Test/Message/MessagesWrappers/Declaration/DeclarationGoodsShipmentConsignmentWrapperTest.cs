using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business.Message.MessagesWrappers.Declaration;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeclarationGoodsShipmentConsignmentWrapperTest : DataProviderTestCase<DeclarationGoodsShipmentConsignmentWrapper>
	{
		public void TestNewOrNull()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			AssertNotNull("Provider", Provider);
			AssertNull("entryInstruction is must", DeclarationGoodsShipmentConsignmentWrapper.NewOrNull(null, declaration));
			AssertNull("declaration is must", DeclarationGoodsShipmentConsignmentWrapper.NewOrNull(cusEntryInstruction, null));
		}
		public void TestLoadingLocation()
		{
			AssertNotNull("LoadingLocation", Provider.LoadingLocation);
			AssertType<DeclarationGoodsShipmentConsignmentLoadingLocationWrapper>(Provider.LoadingLocation);
		}

		public void TestDmExtensions()
		{
			AssertNotNull("DMExtensions", Provider.DmExtensions);
			AssertType<DeclarationGoodsShipmentConsignmentDmExtensionsWrapper>(Provider.DmExtensions);
		}

		public void TestTransportContractDocument()
		{
			AssertNotNull("TransportContractDocument", Provider.TransportContractDocument);
			AssertType<DeclarationGoodsShipmentConsignmentTransportContractDocumentWrapper>(Provider.TransportContractDocument);
		}

		public void TestUnloadingLocation()
		{
			AssertNotNull("UnloadingLocation", Provider.UnloadingLocation);
			AssertType<DeclarationGoodsShipmentConsignmentUnloadingLocationWrapper>(Provider.UnloadingLocation);
		}

		protected override DeclarationGoodsShipmentConsignmentWrapper GetProvider()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IL123";
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "CE123";
			var cusEntryInstruction = factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = cusEntryInstruction.PK;
			declaration.JE_RL_NKPortOfLoading = "LEON";

			return DeclarationGoodsShipmentConsignmentWrapper.NewOrNull(cusEntryInstruction, declaration);
		}
	}
}
