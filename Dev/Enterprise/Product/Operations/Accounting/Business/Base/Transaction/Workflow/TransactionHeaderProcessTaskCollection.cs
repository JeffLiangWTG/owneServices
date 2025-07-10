using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	internal class TransactionHeaderProcessTaskCollection : ProcessTaskCollection
	{
		public TransactionHeaderProcessTaskCollection(TransactionHeader invoice)
			: base(invoice)
		{
		}

		public new TransactionHeaderProcessTask this[int index]
		{
			get { return (TransactionHeaderProcessTask)Elements[index]; }
		}

		public new TransactionHeaderProcessTask AddNew()
		{
			return (TransactionHeaderProcessTask)base.AddNew();
		}
	}
}
