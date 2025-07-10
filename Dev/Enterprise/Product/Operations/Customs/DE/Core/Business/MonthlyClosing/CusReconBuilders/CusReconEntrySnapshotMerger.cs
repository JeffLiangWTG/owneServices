using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	static class CusReconEntrySnapshotMerger
	{
		public static DEMonthlyClosingEntrySnapshot DoMerge(DEMonthlyClosingEntrySnapshot lodged, DEMonthlyClosingEntrySnapshot current)
		{
			var result = lodged;
			MergeDocument();
			UpdateLastUpdateTimeUtc();
			return result;

			void MergeDocument()
			{
				if (current.Document != null)
				{
					result.Document = current.Document;
				}
			}

			void UpdateLastUpdateTimeUtc()
			{
				result.LastUpdateTimeUtc = ZDateTime.UtcNow.ToDateTime();
				result.LastUpdateTimeUtcSpecified = true;
			}
		}
	}
}
