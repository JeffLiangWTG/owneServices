using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CusStorageDocPivot : BaseCusStorageDocPivot
	{
		public CusStorageDocPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		public CusEntryInstruction EntryInstruction => Parent as CusEntryInstruction;

		[ChildEditable(true)]
		public AttachmentInvoiceLineGenPivotCollection InvoiceLineLinks
		{
			get
			{
				if (invoiceLineLinks == null)
				{
					invoiceLineLinks = new AttachmentInvoiceLineGenPivotCollection(this);
					invoiceLineLinks.Load();
					RegisterEditableChildObject(invoiceLineLinks);
				}
				return invoiceLineLinks;
			}
		}
		AttachmentInvoiceLineGenPivotCollection invoiceLineLinks;

		public bool CanLinkToInvoiceLine => CSDDocTypeList.CanLinkToInvoiceLine(CSD_DocType);

		public override ZString CSD_DocType
		{
			get => base.CSD_DocType;
			set
			{
				var oldCanLinkToInvoiceLine = CanLinkToInvoiceLine;
				base.CSD_DocType = value;
				var newCanLinkToInvoiceLine = CanLinkToInvoiceLine;

				if (!IsCopying && oldCanLinkToInvoiceLine != newCanLinkToInvoiceLine)
				{
					if (oldCanLinkToInvoiceLine)
					{
						InvoiceLineLinks.RemoveAndDeleteAll();
					}
					EntryInstruction?.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.ReloadAttachmentLinksIfLoaded());
				}
			}
		}

		public override void Delete()
		{
			InvoiceLineLinks.RemoveAndDeleteAll();
			base.Delete();
		}
	}
}
