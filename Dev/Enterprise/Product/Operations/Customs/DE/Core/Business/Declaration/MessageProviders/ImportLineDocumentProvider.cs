using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportLineDocumentProvider : IImportLineDocument
	{
		public ImportLineDocumentProvider(SupportingDocument document, decimal quantity)
		{
			this.document = Argument.NotNull(document, nameof(document));
			this.quantity = quantity;
		}

		readonly SupportingDocument document;
		readonly decimal quantity;

		public string Division => document.Division;

		public string DocumentType => document.CSI_Code;

		public string ReferenceNumber => document.CSI_ReferenceNumber;

		public DateTime? IssuingDate => document.CSI_DateOfIssue.ToNullableDateTime()?.Date;

		public string AtHandFlag => document.CSI_Status;

		public IAmount WriteOff => CachedValueHelper.GetValue(ref writeOff, () => quantity > 0 ? new AmountProvider(quantity, document.CSI_UnitOfQuantity) : null);
		CachedValue<IAmount> writeOff;
	}
}
