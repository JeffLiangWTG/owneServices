using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceForBulkPosterCollection : InvoicingBaseCollection
	{
		public APInvoiceForBulkPosterCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public APInvoiceForBulkPosterCollection(BusinessObjectFactory factory, APBulkInvoicePoster masterAPBulkInvoicePoster)
			: this(factory)
		{
			this.MasterAPBulkInvoicePoster = masterAPBulkInvoicePoster;
		}

		public readonly APBulkInvoicePoster MasterAPBulkInvoicePoster;

		public new APInvoiceForBulkPoster this[int index]
		{
			get { return (APInvoiceForBulkPoster)Elements[index]; }
		}

		public virtual new APInvoiceForBulkPoster AddNew()
		{
			return (APInvoiceForBulkPoster)base.AddNew();
		}

		public new APInvoiceForBulkPoster AddNew(Type bizObjType)
		{
			return (APInvoiceForBulkPoster)base.AddNew(bizObjType);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (MasterAPBulkInvoicePoster != null)
			{
				MasterAPBulkInvoicePoster.UpdateTotalOSExTaxAmount();
				MasterAPBulkInvoicePoster.UpdateTotalOSTaxAmount();
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (MasterAPBulkInvoicePoster != null)
			{
				MasterAPBulkInvoicePoster.UpdateTotalOSExTaxAmount();
				MasterAPBulkInvoicePoster.UpdateTotalOSTaxAmount();
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((APInvoiceForBulkPoster)child).SubmittedFromInvoicingForm = true;
			((APInvoiceForBulkPoster)child).IsInvoiceReceiptPayment = true;
		}
	}
}
