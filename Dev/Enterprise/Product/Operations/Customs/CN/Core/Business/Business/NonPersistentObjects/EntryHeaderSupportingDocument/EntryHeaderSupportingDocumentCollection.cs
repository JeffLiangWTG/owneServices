using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class EntryHeaderSupportingDocumentCollection : NonPersistentBusinessObjectCollection<EntryHeaderSupportingDocument>
	{
		public EntryHeaderSupportingDocumentCollection(CusEntryHeader cusEntryHeader) : base(cusEntryHeader.Factory)
		{
			this.cusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		}

		readonly CusEntryHeader cusEntryHeader;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			RemoveAndDeleteAll();

			foreach (var groupedDocument in cusEntryHeader.CusSupportingDocuments.Where(doc => !doc.IsLicense).GroupBy(doc => new { doc.CSI_Code, doc.CSI_ReferenceNumber }))
			{
				Add(new EntryHeaderSupportingDocument(groupedDocument.ToList()));
			}
		}
	}
}
