using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceBulkBatch : InvoiceBatchHeader
	{
		public InvoiceBulkBatch(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		#region NonPersistent Overrides

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		public override bool IsInDatabase
		{
			get { return false; }
		}

		#endregion

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				base.AH_OHCore = ZGuid.Empty;
				AH_OHInfo.RefreshBinding();
			}
		}

		#endregion

		#region InvoiceBatchHeaders

		public void UpdateBatchTotals(InvoicingBase line)
		{
			if (line == null)
			{
				return;
			}

			ZDecimal multiplier = line.IncludeInTheBatch ? 1.0m : -1.0m;
			AH_OSTotal += line.AH_OSTotal * multiplier;
			AH_GSTAmount += line.AH_GSTAmount * multiplier;
			AH_InvoiceAmount += line.AH_InvoiceAmount * multiplier;
		}

		[ChildEditable(true)]
		public InvoiceBatchHeaderCollection InvoiceBatchHeaders
		{
			get
			{
				if (fInvoiceBatchHeaders == null)
				{
					fInvoiceBatchHeaders = new InvoiceBatchHeaderCollection(Factory);
					fInvoiceBatchHeaders.CountChanged += new CollectionCountChangedEventHandler(fInvoiceBatchHeaders_CountChanged);
					RegisterEditableChildObject(fInvoiceBatchHeaders);
				}
				return fInvoiceBatchHeaders;
			}
		}
		InvoiceBatchHeaderCollection fInvoiceBatchHeaders;

		void fInvoiceBatchHeaders_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			InvoiceBatchHeader batch = e.BizObject as InvoiceBatchHeader;
			if (e.ItemRemoved && batch != null && !batch.IsDeleted)
			{
				batch.ClearLines();
			}
		}

		public void CreateBatches(InvoicingBase[] invoices)
		{
			InvoiceBatchHeader currentBatch = null;
			InvoicingBase currentInvoice;
			for (int i = 0; i < invoices.Length; i++)
			{
				currentInvoice = invoices[i];
				if (currentBatch == null || currentBatch.AH_OH != currentInvoice.AH_OH)
				{
					currentBatch = Factory.New<InvoiceBatchHeader>();
					currentBatch.UpdateInvoiceBulkBatchTotals = UpdateBatchTotals;
					currentBatch.Initialization(this, currentInvoice.AH_OH);
					currentBatch.Validation.ValidateAH_OH();
					InvoiceBatchHeaders.Add(currentBatch);
				}
				currentBatch.Line.Add(currentInvoice);
			}
			InvoiceBatchHeaders.Sort(Schema.AH_OH, System.ComponentModel.ListSortDirection.Ascending);
			InvoiceBatchHeaders.RefreshBinding();
		}

		public void ClearBatches()
		{
			InvoiceBatchHeaders.RemoveAndDeleteAll();
		}

		#endregion

		#region Filter Business Object

		public override InvoiceBatchHeaderFilterBusinessObject Filter
		{
			get
			{
				if (fFilter == null)
				{
					fFilter = new InvoiceBulkBatchFilterBusinessObject();
					fFilter.SetParent(this);
				}
				return fFilter;
			}
		}

		#endregion

		#region Validation Object

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new InvoiceBulkBatchValidation(this);
		}

		#endregion
	}
}
