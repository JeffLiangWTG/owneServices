using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AdditionalSupplyChainActorProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalSupplyChainActorProvider>
	{
		public void TestRole()
		{
			supplyChainActorReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			supplyChainActorReference.CFR_Reference = "REF001";
			AssertEquals("AdditionalSupplyChainActorProvider Role", FiscalReferenceCodeList.Codes.FR1_Importer, provider.Role);
		}

		public void TestID()
		{
			supplyChainActorReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
			supplyChainActorReference.CFR_Reference = "REF001";
			AssertEquals("AdditionalSupplyChainActorProvider ID", "REF001", provider.ID);
		}

		protected override AdditionalSupplyChainActorProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			supplyChainActorReference = instruction.CusSupplyChainActorReferences.AddNew();

			orgHeader = Factory.New<OrgHeader>();
			orgAddress = orgHeader.MainAddress;
			jobDocAddress = declaration.DocAddresses.AddNew();
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			provider = new AdditionalSupplyChainActorProvider(supplyChainActorReference);
		}

		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		protected CusEntryInstruction instruction;
		protected EU.Business.Declaration.CusSupplyChainActorReference supplyChainActorReference;
		protected OrgHeader orgHeader;
		protected OrgAddress orgAddress;
		protected JobDocAddress jobDocAddress;

		AdditionalSupplyChainActorProvider provider;
	}
}
