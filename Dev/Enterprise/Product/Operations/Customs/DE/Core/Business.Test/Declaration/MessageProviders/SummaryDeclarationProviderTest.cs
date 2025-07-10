using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class SummaryDeclarationProviderTest : DataProviderTestCase<SummaryDeclarationProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertNull("NULL", SummaryDeclarationProvider.NewOrNull(null));
				AssertNotNull("Has PreviousDocuments", SummaryDeclarationProvider.NewOrNull(previousDocumentMaster));

				entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
				AssertNull("No PreviousDocuments", SummaryDeclarationProvider.NewOrNull(previousDocumentMaster));
			});
		}

		public void TestIdentificationIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, Provider.IdentificationIndicator);

				previousDocument.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("Not empty", "REG", Provider.IdentificationIndicator);
			});
		}

		public void TestGoodsItems()
		{
			var previousDocument2 = entryInstruction.PreviousDocuments.AddNew();
			AssertEquals(2, Provider.GoodsItems.Count);
		}

		protected override SummaryDeclarationProvider GetProvider() => SummaryDeclarationProvider.NewOrNull(previousDocumentMaster);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			previousDocument = entryInstruction.PreviousDocuments.AddNew();
		}
		CusEntryInstruction entryInstruction;
		PreviousDocument previousDocument;
		PreviousDocumentMaster previousDocumentMaster => entryInstruction.PreviousDocumentMaster;
	}
}
