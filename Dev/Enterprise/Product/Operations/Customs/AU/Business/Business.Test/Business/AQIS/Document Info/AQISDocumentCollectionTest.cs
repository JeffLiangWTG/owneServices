using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISDocumentCollection))]
	sealed class AQISDocumentCollectionTest : AQISCollectionTest<AQISDocumentCollection, AQISDocument>
	{
		public void TestLoadingOneAQISDocument()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISDocuments_Hidden = "TT/Num^suffix";

			AQISDocumentCollection collection = invoiceLine.AQISDocuments;
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISDocuments_Hidden);
			AssertEquals("Collection count", 1, collection.Count);
			AssertEquals("Type", "TT", collection[0].Type);
			AssertEquals("Number", "Num/suffix", collection[0].Number);

			collection[0].Number = "AAA/BBB";
			AssertEquals("AAA/BBB", collection[0].Number);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobComInvoiceLine invoiceLineLoaded = factory2.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals("AAA/BBB", invoiceLineLoaded.AQISDocuments[0].Number);
		}

		public void TestLoadingMultipleAQISDocuments()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_AQISDocuments_Hidden = "TT/Num,TT2/Num2";
			AQISDocumentCollection collection = new AQISDocumentCollection(Factory, invoiceLine.AddInfo);
			collection.SplitAndAddAQISElements(invoiceLine.AddInfo.ZA_AQISDocuments_Hidden);
			AssertEquals("Collection count", 2, collection.Count);
			AssertEquals("Document Type 1 not empty", false, collection[0].Type.IsEmpty);
			AssertEquals("Document Number 1 type not empty", false, collection[0].Number.IsEmpty);
			AssertEquals("Document Type 2 not empty", false, collection[1].Type.IsEmpty);
			AssertEquals("Document Number 2 type not empty", false, collection[1].Number.IsEmpty);
		}

		public void TestGetNewAQISDocumentString()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AQISDocumentCollection collection = new AQISDocumentCollection(Factory, invoiceLine.AddInfo);

			AQISDocument document1 = new AQISDocument(Factory);
			document1.Type = "TT";
			document1.Number = "Num";
			collection.Add(document1);
			collection.ReBuildAndSaveAQISElements();
			AssertEquals("ReBuildAndSaveAQISDocuments", "TT/Num", invoiceLine.AddInfo.ZA_AQISDocuments_Hidden);

			AQISDocument document2 = new AQISDocument(Factory);
			document2.Type = "TT2";
			document2.Number = "Num2/suffix";
			collection.Add(document2);
			collection.ReBuildAndSaveAQISElements();
			AssertEquals("GetNewAQISPackageString", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT/Num"));
			AssertEquals("GetNewAQISPackageString", true, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.Contains("TT2/Num2^suffix"));
			AssertEquals("GetNewAQISPackageString", false, invoiceLine.AddInfo.ZA_AQISDocuments_Hidden.EndsWith(","));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISDocument(Factory);

		protected override AQISDocumentCollection GetCollectionToTest()
		{
			var declaration = JobDeclaration.New(Factory);
			return new AQISDocumentCollection(Factory, declaration.AddInfo);
		}
	}
}
