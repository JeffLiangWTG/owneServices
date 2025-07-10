using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class MessageSendingInvoiceLineCollection : NonPersistentBusinessObjectCollection<MessageSendingInvoiceLine>
	{
		public MessageSendingInvoiceLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new MessageSendingInvoiceLine(Factory);
		protected override bool AllowNewCore => false;

		public void PopulateElements(ExtendReExportDateMessageSendingObject header, ZDateTime currentDate)
		{
			var entryLineObjects = header.D72EntryLines.Cast<MessageSendingEntryLineObject>().OrderBy(x => x.EntryLineNo);
			foreach (MessageSendingEntryLineObject entryLineObject in entryLineObjects)
			{
				var entryLine = header.Header.MergedLines.FirstOrDefault(x => x.CL_LineNumber == entryLineObject.EntryLineNo);
				if (entryLine != null)
				{
					foreach (var invoiceLine in entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.JI_ScheduledReExportDate == currentDate).OrderBy(x => x.JI_SequenceNumber))
					{
						Add(new MessageSendingInvoiceLine(invoiceLine));
					}
				}
			}
		}

		public void PopulateElementsFTA(CusEntryHeader header)
		{
			var invoiceLines = header.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => !x.CusEntryLine.CL_FTASequenceNumber.IsEmpty).OrderBy(x => x.CusEntryLine.CL_FTASequenceNumber).ThenBy(x => x.JI_SequenceNumber);
			foreach (var invoiceLine in invoiceLines)
			{
				Add(new MessageSendingInvoiceLine(invoiceLine));
			}
		}
	}
}
