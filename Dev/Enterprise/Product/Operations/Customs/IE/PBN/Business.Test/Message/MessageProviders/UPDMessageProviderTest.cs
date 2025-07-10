using System.Linq;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(UPDMessageProvider))]
	sealed class UPDMessageProviderTest : DataProviderTestCase<UPDMessageProvider>
	{
		public void TestDeclarations()
		{
			var dec1 = pbn.CustomsReferenceCollection.AddNew();
			dec1.CSI_ReferenceNumber = "MRN12345";
			dec1.CSI_Status = "HBA"; // has been added

			var dec2 = pbn.CustomsReferenceCollection.AddNew();
			dec2.CSI_ReferenceNumber = "MRN2BDEL";
			dec2.CSI_Status = "TBD"; // to be deleted

			var dec3 = pbn.CustomsReferenceCollection.AddNew();
			dec3.CSI_ReferenceNumber = "MRN2BADD";
			dec3.CSI_Status = "TBA"; // to be added

			var declarations = Provider.Declarations;
			AssertType<PBNDeclarationProvider[]>(declarations);
			AssertEquals("Should include declarations with CSI_Status = TBA only", 1, declarations.Count);

			var firstDeclaration = declarations.First();
			AssertEquals("MRN2BADD", firstDeclaration.DeclarationId);
		}

		public void TestDeclarationsToDelete()
		{
			var dec1 = pbn.CustomsReferenceCollection.AddNew();
			dec1.CSI_ReferenceNumber = "MRN12345";
			dec1.CSI_Status = "HBA"; // has been added

			var dec2 = pbn.CustomsReferenceCollection.AddNew();
			dec2.CSI_ReferenceNumber = "MRN2BDEL";
			dec2.CSI_Status = "TBD"; // to be deleted

			var declarationsToDelete = Provider.DeclarationsToDelete;
			AssertType<PBNDeclarationProvider[]>(declarationsToDelete);
			AssertEquals("Should include declarations with CSI_Status = TBD only", 1, declarationsToDelete.Count);

			var firstDeclaration = declarationsToDelete.First();
			AssertEquals("MRN2BDEL", firstDeclaration.DeclarationId);
		}

		protected override UPDMessageProvider GetProvider() => new UPDMessageProvider(new PBNMessageSendingObject(pbn));

		protected override void SetUp()
		{
			base.SetUp();
			pbn = Factory.New<AsycudaManifestHeader>();
		}
		AsycudaManifestHeader pbn;
	}
}
