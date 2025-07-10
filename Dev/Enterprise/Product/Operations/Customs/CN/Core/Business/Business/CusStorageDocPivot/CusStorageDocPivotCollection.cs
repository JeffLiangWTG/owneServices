using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class CusStorageDocPivotCollection : Customs.Business.CusStorageDocPivotCollection<CusStorageDocPivot, CusEntryInstruction>
	{
		public CusStorageDocPivotCollection(CusEntryInstruction master) : base(master)
		{
		}

		public IEnumerable<CusStorageDocPivot> FindByInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			return this.Cast<CusStorageDocPivot>().Where(x => x.CanLinkToInvoiceLine && x.InvoiceLineLinks.GetRelatedPivot(invoiceLine) != null);
		}

		public void UnlinkInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			FindByInvoiceLine(invoiceLine).ForEach(x => x.InvoiceLineLinks.DeletePivotFor(invoiceLine));
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (bizO is CusStorageDocPivot storageDoc)
			{
				Master.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.RemoveAttachmentLinkIfLoaded(storageDoc));
			}
		}
	}
}
