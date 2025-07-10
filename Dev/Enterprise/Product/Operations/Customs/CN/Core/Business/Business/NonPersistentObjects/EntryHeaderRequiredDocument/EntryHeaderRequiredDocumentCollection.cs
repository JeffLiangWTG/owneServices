using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class EntryHeaderRequiredDocumentCollection : NonPersistentBusinessObjectCollection<EntryHeaderRequiredDocument>
	{
		public EntryHeaderRequiredDocumentCollection(CusEntryHeader entryHeader) : base(entryHeader.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
		readonly CusEntryHeader entryHeader;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			RemoveAndDeleteAll();

			if (entryHeader.EntryInstruction != null)
			{
				foreach (CIQRequiredDocument document in entryHeader.EntryInstruction.CIQRequiredDocuments)
				{
					Add(new EntryHeaderRequiredDocument(document));
				}
			}
		}
	}
}
