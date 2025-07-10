namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class InvoiceBulkBatchValidation : InvoiceBatchHeaderValidation
	{
		public InvoiceBulkBatchValidation(InvoiceBulkBatch parent)
			: base(parent)
		{
		}

		protected new InvoiceBulkBatch Parent
		{
			get { return (InvoiceBulkBatch)base.Parent; }
		}

		#region AH_OH

		protected override void CheckAH_OH()
		{
			if (!Parent.AH_OH.IsEmpty)
			{
				base.CheckAH_OH();
			}
		}

		#endregion

		#region Empty Validation For Not Used Property

		protected override void CheckAH_OSTotal()
		{
		}

		protected override void CheckAH_GSTAmount()
		{
		}

		protected override void CheckAH_InvoiceAmount()
		{
		}

		#endregion
	}
}
