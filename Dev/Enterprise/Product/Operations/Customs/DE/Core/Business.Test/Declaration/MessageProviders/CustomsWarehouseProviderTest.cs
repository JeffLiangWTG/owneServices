using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CustomsWarehouseProviderTest : DataProviderTestCase<CustomsWarehouseProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", CustomsWarehouseProvider.NewOrNull(null));
				AssertNotNull("Has PreviousDocuments", CustomsWarehouseProvider.NewOrNull(previousDocumentMaster));

				entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
				AssertNull("No PreviousDocuments", CustomsWarehouseProvider.NewOrNull(previousDocumentMaster));
			});
		}

		public void TestGoodsItemQuantity()
		{
			entryInstruction.PreviousDocuments.AddNew();
			AssertEquals(2, Provider.GoodsItemQuantity);
		}

		public void TestWarehouseOwnerIdentifier()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.WarehouseOwnerIdentifier);

				previousDocumentMaster.AuthorizationNumber = "AUTH12345";
				AssertEquals("Not empty", "AUTH12345", Provider.WarehouseOwnerIdentifier);
			});
		}

		public void TestLocalReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.LocalReferenceNumber);

				previousDocumentMaster.CSI_ReferenceNumber2 = "REF12345";
				AssertEquals("Not empty", "REF12345", Provider.LocalReferenceNumber);
			});
		}

		public void TestGoodsItems()
		{
			entryInstruction.PreviousDocuments.AddNew();
			AssertEquals(2, Provider.GoodsItems.Count);
		}

		protected override CustomsWarehouseProvider GetProvider() => CustomsWarehouseProvider.NewOrNull(previousDocumentMaster);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.PreviousDocuments.AddNew();
		}
		CusEntryInstruction entryInstruction;
		PreviousDocumentMaster previousDocumentMaster => entryInstruction.PreviousDocumentMaster;
	}
}
