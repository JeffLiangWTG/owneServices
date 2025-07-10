using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocGenericTransactionHeaderCollection : DocumentWrapperCollection
	{
		public DocGenericTransactionHeaderCollection(BusinessObjectFactory factory)
			   : base(factory)
		{
		}

		public static DocGenericTransactionHeaderCollection New(BusinessObjectFactory factory)
		{
			DocGenericTransactionHeaderCollection result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(factory);
			}
			else
			{
				result = new DocGenericTransactionHeaderCollection(factory);
			}
			return result;
		}

		protected delegate DocGenericTransactionHeaderCollection NewDelegate(BusinessObjectFactory factory);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public new DocGenericTransactionHeader this[int index]
		{
			get
			{
				DocGenericTransactionHeader result = (DocGenericTransactionHeader)base[index];

				if (result != null && result.TransactionType == ZArchitecture.Core.TransactionTypes.InvoiceBatch)
				{
					InvoiceBatchHeader batchInvoice = Factory.Load<InvoiceBatchHeader>(result.BusinessObject.PK);
					result = DocGenericTransactionHeader.New(batchInvoice, Factory);
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
				DocGenericTransactionHeader docTransactionHeaderA = (DocGenericTransactionHeader)x;
				DocGenericTransactionHeader docTransactionHeaderB = (DocGenericTransactionHeader)y;

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
