using System;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportLineDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportLineDocumentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ImportLineDocumentProvider(null, 0m));
		}

		public void TestDivision()
		{
			AssertEquals(string.Empty, dataProvider.Division);
		}

		public void TestDocumentType()
		{
			AssertEquals("C626", dataProvider.DocumentType);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("DE 15950/14-1", dataProvider.ReferenceNumber);
		}

		public void TestIssuingDate()
		{
			AssertEquals(docDate, dataProvider.IssuingDate);
		}

		public void TestIssuingDate_Empty()
		{
			suppDoc.CSI_DateOfIssue = ZDate.Empty;
			AssertNull(dataProvider.IssuingDate);
		}

		public void TestAtHandFlag()
		{
			AssertEquals("J", dataProvider.AtHandFlag);
		}

		public void TestWriteOff_Cached()
		{
			var writeOff = dataProvider.WriteOff;
			AssertEquals("Cached", writeOff, dataProvider.WriteOff);
		}

		public void TestWriteOff_Empty()
		{
			dataProvider = new ImportLineDocumentProvider(suppDoc, 0);
			AssertNull(dataProvider.WriteOff);
		}

		public void TestWriteOff()
		{
			dataProvider = new ImportLineDocumentProvider(suppDoc, 123.45m);
			CombineAssertions(() =>
			{
				AssertEquals("Overriden Writeoff quantity", 123.45m, dataProvider.WriteOff.Quantity);
				AssertEquals("Overriden Writeoff unit", "NAR", dataProvider.WriteOff.MeasurementUnit);
			});
		}

		protected override void SetUp()
		{
			docDate = new ZDate(2020, 11, 10);

			suppDoc = Factory.New<SupportingDocument>();
			suppDoc.CSI_Code = "C626";
			suppDoc.CSI_ReferenceNumber = "DE 15950/14-1";
			suppDoc.CSI_DateOfIssue = docDate;
			suppDoc.CSI_Status = "J";
			suppDoc.CSI_Quantity = 100;
			suppDoc.CSI_UnitOfQuantity = "NAR";

			dataProvider = new ImportLineDocumentProvider(suppDoc, 100);
		}
		ImportLineDocumentProvider dataProvider;
		SupportingDocument suppDoc;
		ZDate docDate;

		protected override ImportLineDocumentProvider GetProvider() => dataProvider;
	}
}
