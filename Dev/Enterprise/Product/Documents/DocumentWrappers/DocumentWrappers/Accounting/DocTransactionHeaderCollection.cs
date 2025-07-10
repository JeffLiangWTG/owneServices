using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocTransactionHeaderCollection : DocumentWrapperCollection
	{
		public DocTransactionHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocTransactionHeaderCollection New(BusinessObjectFactory factory)
		{
			DocTransactionHeaderCollection result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new DocTransactionHeaderCollection(factory);
			}
			return result;
		}

		protected delegate DocTransactionHeaderCollection NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public new DocTransactionHeader this[int index]
		{
			get
			{
				DocTransactionHeader result = (DocTransactionHeader)base[index];

				if (result != null && result.TransactionType == ZArchitecture.Core.TransactionTypes.InvoiceBatch)
				{
					InvoiceBatchHeader batchInvoice = Factory.Load<InvoiceBatchHeader>(result.TransactionPK);
					result = DocARBatchInvoice.New(batchInvoice, Factory);
				}

				return result;
			}
		}

		public void SortOnDateAndTransactionNumber()
		{
			this.Sort(new InvoiceSort());
		}

		public class InvoiceSort : System.Collections.IComparer
		{
			public int Compare(object x, object y)
			{
				DocTransactionHeader docTransactionHeaderA = (DocTransactionHeader)x;
				DocTransactionHeader docTransactionHeaderB = (DocTransactionHeader)y;

				int result = 0;
				if (!docTransactionHeaderA.InvoiceDate.IsEmpty && !docTransactionHeaderB.InvoiceDate.IsEmpty)
				{
					result = docTransactionHeaderA.InvoiceDate.Date.CompareTo(docTransactionHeaderB.InvoiceDate.Date);
				}
				if (result == 0)
				{
					result = docTransactionHeaderA.TransactionNumber.CompareTo(docTransactionHeaderB.TransactionNumber);
				}
				return result;
			}
		}
	}
}
