using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ApportionedLineRemovedEventArgs : EventArgs
	{
		public ApportionedLineRemovedEventArgs(InvoicingLineBase removedLine)
		{
			this.RemovedLine = removedLine;
		}

		public readonly InvoicingLineBase RemovedLine;
	}

	public delegate void ApportionedLineRemovedEventHander(InvoicingLineBaseCollection sender, ApportionedLineRemovedEventArgs e);
	public delegate void ApportionedInvoiceLineModifiedEventHandler(InvoicingLineBase sender, EventArgs e);

	public class APInvoiceLineCollection : InvoicingLineBaseCollection
	{
		public APInvoiceLineCollection(BusinessObject parent)
			: base(parent)
		{
		}

		public new APInvoiceLine this[int i]
		{
			get { return (APInvoiceLine)Elements[i]; }
		}

		public new APInvoiceLine AddNew()
		{
			return (APInvoiceLine)base.AddNew();
		}

		protected override void SetDefaultsForNewChildCore(BusinessObject child)
		{
			APInvoiceLine newLine = child as APInvoiceLine;
			if (newLine != null)
			{
				newLine.ShowJobChargesForImportEvent += OnShowJobChargesForImportEvent;
			}
			base.SetDefaultsForNewChildCore(child);
		}
	}
}
