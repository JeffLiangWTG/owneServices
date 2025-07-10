using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public sealed class EntryDocumentFromSnapshotProvider : IImportDocument
	{
		public EntryDocumentFromSnapshotProvider(DEMonthlyClosingEntrySnapshotDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}
		readonly DEMonthlyClosingEntrySnapshotDocument document;

		public string Type => document.Type;

		public string ReferenceNumber => document.ReferenceNumber;

		public DateTime? IssuingDate => MessageTimeHelper.ToNullableDateTime(document.IssuingDate)?.Date;
	}
}
