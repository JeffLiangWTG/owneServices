using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public class AttachmentInvoiceLineLinkCollection : NonPersistentBusinessObjectCollection<AttachmentInvoiceLineLink>
	{
		public AttachmentInvoiceLineLinkCollection(JobComInvoiceLine invoiceLine)
		{
			InvoiceLine = invoiceLine;
		}
		JobComInvoiceLine InvoiceLine { get; }

		public override void Load()
		{
			var obsoletedLinks = this.Cast<AttachmentInvoiceLineLink>().ToList();
			RemoveAll();

			if (InvoiceLine.CanLinkToAttachment && InvoiceLine.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				foreach (var attachment in entryInstruction.Attachments.Cast<EntryInstructionAttachment>()
					.Where(x => x.CusStorageDocPivot is CusStorageDocPivot pivot && pivot.CanLinkToInvoiceLine))
				{
					var link = obsoletedLinks.FirstOrDefault(x => x.Attachment == attachment);
					if (link == null)
					{
						link = new AttachmentInvoiceLineLink(attachment, InvoiceLine);
					}
					else
					{
						obsoletedLinks.Remove(link);
					}
					Add(link);
				}
			}

			obsoletedLinks.DeleteAll();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException();
	}
}
