using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	sealed class CusReconEntrySnapshotBuilder : SnapshotBuilder<DEMonthlyClosingEntrySnapshot>
	{
		public CusReconEntrySnapshotBuilder(IMonthlyClosingEntrySnapshot provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
		}

		public override DEMonthlyClosingEntrySnapshot GenerateMessage()
		{
			var snapshotObj = new DEMonthlyClosingEntrySnapshot()
			{
				Document = PopulateDocuments(),
			};
			PopulateLastUpdateTimeUtc(snapshotObj);
			return snapshotObj;
		}

		public DEMonthlyClosingEntrySnapshot GenerateEmptyMessage()
		{
			var snapshotObj = new DEMonthlyClosingEntrySnapshot();
			PopulateLastUpdateTimeUtc(snapshotObj);
			return snapshotObj;
		}

		void PopulateLastUpdateTimeUtc(DEMonthlyClosingEntrySnapshot snapshotObj)
		{
			snapshotObj.LastUpdateTimeUtc = ZDateTime.UtcNow.ToDateTime();
			snapshotObj.LastUpdateTimeUtcSpecified = true;
		}

		DEMonthlyClosingEntrySnapshotDocument[] PopulateDocuments() => provider.Documents.Select(d => new DEMonthlyClosingEntrySnapshotDocument
		{
			Type = d.Type,
			ReferenceNumber = d.ReferenceNumber,
			IssuingDate = d.IssuingDate.GetValueOrDefault(),
		}).ToArray();

		readonly IMonthlyClosingEntrySnapshot provider;
	}
}
