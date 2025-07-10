using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class PreviousDocumentProviderTest : DataProviderTestCase<PreviousDocumentProvider>
	{
		public void TestPreviousDocumentType()
		{
			AssertEquals("PreviousDocumentType=>CSI_Code", "P1", Provider.PreviousDocumentType);
		}

		public void TestPreviousDocumentIdentifier()
		{
			AssertEquals("PreviousDocumentIdentifier=>CSI_ReferenceNumber", "PRE001", Provider.PreviousDocumentIdentifier);
		}

		public void TestPreviousDocumentLineId()
		{
			AssertEquals("PreviousDocumentLineId=>CSI_LineNo", "1", Provider.PreviousDocumentLineId);
		}

		protected override void SetUp() => SetUpTestDataIfNeeded();

		protected override PreviousDocumentProvider GetProvider() => PreviousDocumentProvider.New(previousDocument);

		void SetUpTestDataIfNeeded()
		{
			if (entryInstruction == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				previousDocument = entryInstruction.PreviousDocuments.AddNew();

				previousDocument.CSI_Code = "P1";
				previousDocument.CSI_ReferenceNumber = "PRE001";
				previousDocument.CSI_LineNo = 1;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		PreviousDocument previousDocument;
	}
}
