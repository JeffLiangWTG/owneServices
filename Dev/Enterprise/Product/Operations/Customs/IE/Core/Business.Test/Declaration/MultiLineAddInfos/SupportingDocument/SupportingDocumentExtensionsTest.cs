using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class SupportingDocumentExtensionsTest : TestCaseWithFactory
	{
		public void TestHasN018SupportingDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var supDoc = cusEntryInstruction.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supDoc.CSI_Code = "YYYY";
				AssertEquals(false, SupportingDocumentCollectionExtensions.HasN018SupportingDocument(cusEntryInstruction.SupportingDocuments));

				supDoc.CSI_Code = "N018";
				AssertEquals(true, SupportingDocumentCollectionExtensions.HasN018SupportingDocument(cusEntryInstruction.SupportingDocuments));
			});
		}

		public void TestHasRepetitiveSupportingDocument()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var supportDocuments = invoiceHeader.SupportingDocuments;
			AssertEquals("empty collection", false, SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(supportDocuments));

			var supportingDoc = invoiceHeader.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "666";
			AssertEquals("without target supportingDocument", false, SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(supportDocuments));

			supportingDoc.CSI_Code = "U164";
			var supportingDoc2 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "U165";
			AssertEquals(true, SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(supportDocuments));

			supportingDoc.CSI_Code = "U167";
			AssertEquals("Specific case", false, SupportingDocumentCollectionExtensions.HasMutuallyExclusiveSupportingDocument(supportDocuments));
		}

		public void TestIsValidForBR20312()
		{
			Assert("Only U164, U165, U166, U167 should be valid", SupportingDocumentCollectionExtensions.IsValidForBR20312("U164"));
			Assert("Only U164, U165, U166, U167 should be valid", SupportingDocumentCollectionExtensions.IsValidForBR20312("U165"));
			Assert("Only U164, U165, U166, U167 should be valid", SupportingDocumentCollectionExtensions.IsValidForBR20312("U166"));
			Assert("Only U164, U165, U166, U167 should be valid", SupportingDocumentCollectionExtensions.IsValidForBR20312("U167"));
			Assert("Only U164, U165, U166, U167 should be valid", !SupportingDocumentCollectionExtensions.IsValidForBR20312("U168"));
		}
	}
}
