using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public abstract class CusReconEntryLineBuilder<TImportDecLine, TMonthlyClosingDecLine>
		where TImportDecLine : IImportDecLine
		where TMonthlyClosingDecLine : IMonthlyClosingDecLine
	{
		public CusReconEntryLineBuilder(TImportDecLine lineProvider, IImportDecHeader headerProvider, CusReconEntry cusReconEntry)
		{
			this.lineProvider = Argument.NotNull(lineProvider, nameof(lineProvider));
			this.headerProvider = Argument.NotNull(headerProvider, nameof(headerProvider));
			this.cusReconEntry = Argument.NotNull(cusReconEntry, nameof(cusReconEntry));
		}

		public void CreateLodgedEntryLineAndSnapshot()
		{
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine.CRL_Description = lineProvider.GoodsDescription;
			cusReconEntryLine.CRL_OriginalEntryLineNumber = (ZShort)lineProvider.SequenceNumber;

			var cusReconEntryLineSnapshot = cusReconEntryLine.CusReconSnapshots.AddNew();
			cusReconEntryLineSnapshot.CRS_Type = CusReconConstants.Lodged;
			cusReconEntryLineSnapshot.CRS_SnapshotXml = new CusReconEntryLineSnapshotBuilder(GetEntryLineSnapshotProvider(lineProvider, headerProvider)).GetXMLMessage();
		}

		public void CreateCurrentEntryLineAndSnapshot()
		{
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.Cast<CusReconEntryLine>().FirstOrDefault(x => x.CRL_OriginalEntryLineNumber == (ZShort)lineProvider.SequenceNumber);
			var existingLodgedEntryLineSnapshot = cusReconEntryLine?.LodgedSnapshot;

			if (existingLodgedEntryLineSnapshot != null)
			{
				var existingLodgedEntryLineSnapshotObject = CusReconEntryLineSnapshotBuilder.Deserialize(existingLodgedEntryLineSnapshot.CRS_SnapshotXml);
				var snapshotBuilder = new CurrentEntryLineSnapshotBuilder(GetEntryLineSnapshotProvider(lineProvider, headerProvider), existingLodgedEntryLineSnapshotObject);

				var updatedCurrentEntryLineSnapshotXMLMessage = snapshotBuilder.GenerateMessage();

				if (!updatedCurrentEntryLineSnapshotXMLMessage.IsEmpty || HasChangesNotInSnapshot(cusReconEntryLine))
				{
					cusReconEntryLine.CurrentSnapshot?.Delete();
					var cusReconEntryLineSnapshot = cusReconEntryLine.CusReconSnapshots.AddNew();
					cusReconEntryLineSnapshot.CRS_Type = CusReconConstants.Current;
					cusReconEntryLineSnapshot.CRS_SnapshotXml = CurrentEntryLineSnapshotBuilder.Serialize(updatedCurrentEntryLineSnapshotXMLMessage);
				}
			}
		}

		protected abstract IMonthlyClosingEntryLineSnapshot GetEntryLineSnapshotProvider(TImportDecLine lineProvider, IImportDecHeader headerProvider);

		protected abstract TMonthlyClosingDecLine GetMonthlyClosingDecLineProvider(CusReconEntryLine reconEntryLine);

		protected abstract IEqualityComparer<TMonthlyClosingDecLine> LineEqualityComparer { get; }

		bool HasChangesNotInSnapshot(CusReconEntryLine reconEntryLine)
		{
			var lineProvider = GetMonthlyClosingDecLineProvider(reconEntryLine);
			var persistedLineProvider = GetMonthlyClosingDecLineProvider(ReadOnlyFactory.Load<CusReconEntryLine>(reconEntryLine.PK));
			return !LineEqualityComparer.Equals(lineProvider, persistedLineProvider);
		}

		ReadOnlyBusinessObjectFactory ReadOnlyFactory => readOnlyFactoryCached ?? (readOnlyFactoryCached = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory readOnlyFactoryCached;

		readonly TImportDecLine lineProvider;
		readonly IImportDecHeader headerProvider;
		readonly CusReconEntry cusReconEntry;
	}
}
