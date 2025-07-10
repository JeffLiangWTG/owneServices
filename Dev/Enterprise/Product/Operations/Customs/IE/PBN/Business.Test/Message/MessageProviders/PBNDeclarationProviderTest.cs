using CargoWise.Customs.IE.MessageContracts.Interfaces.PBN;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	sealed class PBNDeclarationProviderTest : DataProviderTestCase<PBNDeclarationProvider>
	{
		public void TestIPBNDeclaration()
		{
			Assert("Should implement IPBNDeclaration", Provider is IPBNDeclaration);
		}

		public void TestConstructor()
		{
			AssertNull(PBNDeclarationProvider.New(null));
		}

		public void TestDeclarationId()
		{
			AssertEquals("25IEROS124782356", Provider.DeclarationId);
		}

		public void TestDeclarationType()
		{
			AssertNull(Provider.DeclarationType);
		}

		protected override PBNDeclarationProvider GetProvider() => PBNDeclarationProvider.New(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			var pbn = Factory.New<AsycudaManifestHeader>();
			declaration = pbn.CustomsReferenceCollection.AddNew();
			declaration.CSI_ReferenceNumber = "25IEROS124782356";
			declaration.CSI_Code = "AES";
		}
		PBNCustomsDeclarationItem declaration;
	}
}
