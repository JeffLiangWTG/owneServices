using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class LinkedDocumentProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(LinkedDocumentProvider.New(null));
			AssertType<LinkedDocumentProvider>(LinkedDocumentProvider.New(Factory.New<PreviousDocument>()));
		}

		public void TestLinkedDocuments()
		{
			var previousDocument = Factory.NewWithValidTestData<PreviousDocument>();
			previousDocument.CSI_Code = "1";
			previousDocument.CSI_ReferenceNumber = "1";
			previousDocument.CSI_ItemNumber = 1;

			var dataProvider = LinkedDocumentProvider.New(previousDocument);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be", "DE", dataProvider.Type);
				AssertEquals("Number should be", "1", dataProvider.Number);
				AssertEquals("ItemNumber should be", 1, dataProvider.ItemNumber);
			});
		}
	}
}
