using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntrySnapshotDocumentEqualityComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalEntrySnapshotDocument));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalEntrySnapshotDocument, CreateDEMonthlyClosingEntrySnapshotDocument("AAA", new DateTime(2021, 12, 31), "ReferenceNumber")));
		}

		public void TestEquals_NullString()
		{
			var entrySnapshotDocument = CreateDEMonthlyClosingEntrySnapshotDocument(null, new DateTime(2021, 12, 31), null);
			AssertNoExceptionThrown(() => comparer.Equals(entrySnapshotDocument, originalEntrySnapshotDocument));
		}

		public void TestEquals_Type()
		{
			var entrySnapshotDocument = CreateDEMonthlyClosingEntrySnapshotDocument("BBB", new DateTime(2021, 12, 31), "ReferenceNumber");
			AssertEquals(false, comparer.Equals(originalEntrySnapshotDocument, entrySnapshotDocument));
		}

		public void TestEquals_IssuingDate()
		{
			var entrySnapshotDocument = CreateDEMonthlyClosingEntrySnapshotDocument("AAA", new DateTime(2022, 01, 01), "ReferenceNumber");
			AssertEquals(false, comparer.Equals(originalEntrySnapshotDocument, entrySnapshotDocument));
		}

		public void TestEquals_ReferenceNumber()
		{
			var entrySnapshotDocument = CreateDEMonthlyClosingEntrySnapshotDocument("AAA", new DateTime(2021, 12, 31), "ReferenceNumber2");
			AssertEquals(false, comparer.Equals(originalEntrySnapshotDocument, entrySnapshotDocument));
		}

		public void TestEquals_Division()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Length", 1, Enum.GetValues(typeof(DEMonthlyClosingEntrySnapshotDocumentDivision)).Length);
				AssertEquals("Item4 string", "Item4", nameof(DEMonthlyClosingEntrySnapshotDocumentDivision.Item4));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalEntrySnapshotDocument = CreateDEMonthlyClosingEntrySnapshotDocument("AAA", new DateTime(2021, 12, 31), "ReferenceNumber");
			comparer = new DEMonthlyClosingEntrySnapshotDocumentEqualityComparer();
		}
		DEMonthlyClosingEntrySnapshotDocument originalEntrySnapshotDocument;
		DEMonthlyClosingEntrySnapshotDocumentEqualityComparer comparer;

		DEMonthlyClosingEntrySnapshotDocument CreateDEMonthlyClosingEntrySnapshotDocument(string type, DateTime issuingDate, string referenceNumber)
		{
			return new DEMonthlyClosingEntrySnapshotDocument
			{
				Type = type,
				Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4,
				IssuingDate = issuingDate,
				ReferenceNumber = referenceNumber
			};
		}
	}
}
