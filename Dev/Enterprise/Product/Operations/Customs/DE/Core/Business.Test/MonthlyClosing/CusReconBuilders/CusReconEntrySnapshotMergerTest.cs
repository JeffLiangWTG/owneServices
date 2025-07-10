using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class CusReconEntrySnapshotMergerTest : TestCase
	{
		public void TestDocument_Current()
		{
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntrySnapshotDocumentEqualityComparer(), current.Document, result.Document);
		}

		public void TestDocument_NoCurrent()
		{
			current.Document = null;
			AssertContainsExactElementsInAnyOrder(new DEMonthlyClosingEntrySnapshotDocumentEqualityComparer(), lodged.Document, result.Document);
		}

		[TestDate(2022, 01, 25, 15, 41, 19)]
		public void TestLastUpdateTimeUtc()
		{
			var mergedSnapshot = result;
			CombineAssertions(() =>
			{
				AssertEquals("LastUpdateTimeUtc", new DateTime(2022, 01, 25, 15, 41, 19), mergedSnapshot.LastUpdateTimeUtc);
				AssertEquals("LastUpdateTimeUtcSpecified", true, mergedSnapshot.LastUpdateTimeUtcSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			current = CreateCurrentEntrySnapshot();
			lodged = CreateLodgedEntrySnapshot();
		}
		DEMonthlyClosingEntrySnapshot lodged;
		DEMonthlyClosingEntrySnapshot current;

		DEMonthlyClosingEntrySnapshot result => CusReconEntrySnapshotMerger.DoMerge(lodged, current);

		DEMonthlyClosingEntrySnapshot CreateCurrentEntrySnapshot()
		{
			return new DEMonthlyClosingEntrySnapshot
			{
				Document = new DEMonthlyClosingEntrySnapshotDocument[]
				{
					new DEMonthlyClosingEntrySnapshotDocument { Type = "N381", Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4, ReferenceNumber = "reference", IssuingDate = new DateTime(2022, 2, 26) },
					new DEMonthlyClosingEntrySnapshotDocument { Type = "N991", Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4, ReferenceNumber = "reference2", IssuingDate = new DateTime(2021, 2, 26) }
				},
			};
		}

		DEMonthlyClosingEntrySnapshot CreateLodgedEntrySnapshot()
		{
			return new DEMonthlyClosingEntrySnapshot
			{
				Document = new DEMonthlyClosingEntrySnapshotDocument[]
				{
					new DEMonthlyClosingEntrySnapshotDocument { Type = "N380", Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4, ReferenceNumber = "reference", IssuingDate = new DateTime(2022, 1, 26) },
					new DEMonthlyClosingEntrySnapshotDocument { Type = "N990", Division = DEMonthlyClosingEntrySnapshotDocumentDivision.Item4, ReferenceNumber = "reference2", IssuingDate = new DateTime(2021, 1, 26) }
				},
			};
		}
	}
}
