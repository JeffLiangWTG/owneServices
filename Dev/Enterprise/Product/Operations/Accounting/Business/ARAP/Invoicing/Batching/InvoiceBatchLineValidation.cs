using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class InvoiceBatchLineValidation : AccTransactionHeaderValidation
	{
		public InvoiceBatchLineValidation(InvoicingBase parent)
			: base(parent)
		{
			Argument.NotNull(parent, "Parent");
			this.Parent = parent;
		}
		readonly new InvoicingBase Parent;

		public void CheckNotAlreadyInBatchWhenBatching()
		{
			if (Parent.IncludeInTheBatch && !Parent.AH_AH_InvoiceStatement.IsEmpty && Parent.AH_AH_InvoiceStatement.IsValid)
			{
				InvoiceBatchHeader batch = Parent.Factory.Load<InvoiceBatchHeader>(Parent.AH_AH_InvoiceStatement);
				if (batch != null)
				{
					Parent.AddRowError(Res.GetString("05980ea2-81df-471f-bb26-fb43a40ff986", "This invoice is already part of batch {0}.\r\nRedo search for invoices to batch or exclude this invoice from the batch.", batch.AH_TransactionNum));
				}
			}
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckNotAlreadyInBatchWhenBatching();
		}
	}
}
