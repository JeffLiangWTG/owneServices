using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(EntryHeaderRequiredDocument))]
	class EntryHeaderRequiredDocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new EntryHeaderRequiredDocument(Factory.New<CIQRequiredDocument>());
		}

		public void TestICIQRequiredDocument()
		{
			var document = Factory.New<CIQRequiredDocument>();
			document.XC_DocumentType = "13";
			document.XC_NumberOfOriginals = 2;
			document.XC_NumberOfCopies = 1;
			var wrapper = new EntryHeaderRequiredDocument(document);
			AssertEquals("DocumentType", "13", wrapper.DocumentType);
			AssertEquals("NumberOfOriginals", (ZInt)2, wrapper.NumberOfOriginals);
			AssertEquals("NumberOfCopies", (ZInt)1, wrapper.NumberOfCopies);
		}

		public void TestToString()
		{
			var document = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().CIQRequiredDocuments.AddNew();
			document.XC_NumberOfOriginals = 1;
			document.XC_NumberOfCopies = 2;
			document.XC_DocumentType = "20";
			var testItem = new EntryHeaderRequiredDocument(document);
			AssertEquals("20:1/2", testItem.ToString());
		}
	}
}
