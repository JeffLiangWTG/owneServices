using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class EntryDocumentFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<EntryDocumentFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new EntryDocumentFromSnapshotProvider(null));
		}

		public void TestType()
		{
			AssertEquals("N381", dataProvider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("reference2", dataProvider.ReferenceNumber);
		}

		[TestDate(2022, 1, 6)]
		public void TestIssuingDate()
		{
			AssertEquals(ZDate.Today, dataProvider.IssuingDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			document = new DEMonthlyClosingEntrySnapshotDocument() { Type = "N381", ReferenceNumber = "reference2", IssuingDate = new DateTime(2022, 1, 6) };
			dataProvider = new EntryDocumentFromSnapshotProvider(document);
		}

		IImportDocument dataProvider;
		DEMonthlyClosingEntrySnapshotDocument document;

		protected override EntryDocumentFromSnapshotProvider GetProvider() => (EntryDocumentFromSnapshotProvider)dataProvider;
	}
}
