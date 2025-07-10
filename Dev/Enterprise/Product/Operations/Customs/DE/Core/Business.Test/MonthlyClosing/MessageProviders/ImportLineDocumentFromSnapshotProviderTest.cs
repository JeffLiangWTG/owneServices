using System;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class ImportLineDocumentFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportLineDocumentFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new ImportLineDocumentFromSnapshotProvider(null));
		}

		public void TestDivision()
		{
			AssertEquals("6", dataProvider.Division);
		}

		public void TestDocumentType()
		{
			AssertEquals("ABC", dataProvider.DocumentType);
		}

		public void TestAtHandFlag()
		{
			AssertEquals("J", dataProvider.AtHandFlag);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("REFERENCE", dataProvider.ReferenceNumber);
		}

		public void TestIssuingDate()
		{
			var issuingDate = new DateTime(2022, 1, 21);
			document.IssuingDate = issuingDate;
			document.IssuingDateSpecified = true;
			AssertEquals(issuingDate, dataProvider.IssuingDate);
		}

		public void TestIssuingDate_Empty()
		{
			AssertNull(dataProvider.IssuingDate);
		}

		public void TestWriteOff()
		{
			CombineAssertions(() =>
			{
				AssertEquals(12345.78m, dataProvider.WriteOff.Quantity);
				AssertEquals("KGM", dataProvider.WriteOff.MeasurementUnit);
				AssertEquals(string.Empty, dataProvider.WriteOff.Qualifier);
			});
		}

		public void TestWriteOff_Null()
		{
			document.WriteOff = null;
			AssertNull(dataProvider.WriteOff);
		}

		protected override void SetUp()
		{
			base.SetUp();

			document = new DEMonthlyClosingEntryLineSnapshotDocument()
			{
				Division = DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item6,
				Type = "ABC",
				AtHandFlag = "J",
				ReferenceNumber = "REFERENCE",
				WriteOff = new Amount() { Quantity = 12345.78m, MeasurementUnit = "KGM" },
			};
			dataProvider = new ImportLineDocumentFromSnapshotProvider(document);
		}

		IImportLineDocument dataProvider;
		DEMonthlyClosingEntryLineSnapshotDocument document;

		protected override ImportLineDocumentFromSnapshotProvider GetProvider() => (ImportLineDocumentFromSnapshotProvider)dataProvider;
	}
}
