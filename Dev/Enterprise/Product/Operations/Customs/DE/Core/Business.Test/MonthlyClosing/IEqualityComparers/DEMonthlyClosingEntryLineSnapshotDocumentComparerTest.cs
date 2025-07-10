using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotDocumentComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalDocument));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalDocument, CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), "1", new Amount() { Qualifier = "AAA" }, "T1")));
		}

		public void TestEquals_NullString()
		{
			var document = CreateDocument(null, DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), null, new Amount() { Qualifier = "AAA" }, "T1");
			AssertNoExceptionThrown(() => comparer.Equals(document, originalDocument));
		}

		public void TestEquals_ReferenceNumber()
		{
			var document = CreateDocument("BBB", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), "1", new Amount() { Qualifier = "AAA" }, "T1");
			AssertEquals(false, comparer.Equals(originalDocument, document));
		}

		public void TestEquals_Division()
		{
			var document = CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item2, new DateTime(2021, 12, 31), "1", new Amount() { Qualifier = "AAA" }, "T1");
			AssertEquals(false, comparer.Equals(originalDocument, document));
		}

		public void TestEquals_IssuingDate()
		{
			var document = CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2022, 01, 01), "1", new Amount() { Qualifier = "AAA" }, "T1");
			AssertEquals(false, comparer.Equals(originalDocument, document));
		}

		public void TestEquals_AtHandFlag()
		{
			var document = CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), "0", new Amount() { Qualifier = "AAA" }, "T1");
			AssertEquals(false, comparer.Equals(originalDocument, document));
		}

		public void TestEquals_WriteOff()
		{
			var document = CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), "1", new Amount() { Qualifier = "BBB" }, "T1");
			AssertEquals(false, comparer.Equals(originalDocument, document));
		}

		public void TestEquals_Type()
		{
			var document = CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), "1", new Amount() { Qualifier = "AAA" }, "T2");
			AssertEquals(false, comparer.Equals(originalDocument, document));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalDocument = CreateDocument("AAA", DEMonthlyClosingEntryLineSnapshotDocumentDivision.Item1, new DateTime(2021, 12, 31), "1", new Amount() { Qualifier = "AAA" }, "T1");
			comparer = new DEMonthlyClosingEntryLineSnapshotDocumentComparer();
		}
		DEMonthlyClosingEntryLineSnapshotDocument originalDocument;
		DEMonthlyClosingEntryLineSnapshotDocumentComparer comparer;

		DEMonthlyClosingEntryLineSnapshotDocument CreateDocument(string referenceNumber, DEMonthlyClosingEntryLineSnapshotDocumentDivision division, DateTime issuingDate, string atHandFlag, Amount writeOff, string type)
		{
			return new DEMonthlyClosingEntryLineSnapshotDocument
			{
				ReferenceNumber = referenceNumber,
				Division = division,
				IssuingDate = issuingDate,
				AtHandFlag = atHandFlag,
				WriteOff = writeOff,
				Type = type
			};
		}
	}
}
