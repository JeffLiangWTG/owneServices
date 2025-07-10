using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public sealed class ImportLineDocumentFromSnapshotProvider : IImportLineDocument
	{
		public ImportLineDocumentFromSnapshotProvider(DEMonthlyClosingEntryLineSnapshotDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}
		readonly DEMonthlyClosingEntryLineSnapshotDocument document;

		public string Division => document.Division.XmlEnumToString();

		public string DocumentType => document.Type;

		public string ReferenceNumber => document.ReferenceNumber;

		public DateTime? IssuingDate => document.IssuingDate.NullIfNotSpecified(document.IssuingDateSpecified);

		public string AtHandFlag => document.AtHandFlag;

		public IAmount WriteOff => CachedValueHelper.GetValue(ref writeOff, () => AmountFromSnapshotProvider.NewOrNull(document.WriteOff));
		CachedValue<IAmount> writeOff;
	}
}
