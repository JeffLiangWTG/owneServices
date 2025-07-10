using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;

namespace Enterprise.DocumentWrappers
{
	public class DocCollectionOrderLine : DocBaseWrapper
	{
		protected DocCollectionOrderLine(AccCollectionOrderLine line, BusinessObjectFactory factory)
			: base(line, factory)
		{
		}

		public static DocCollectionOrderLine New(AccCollectionOrderLine line, BusinessObjectFactory factory)
		{
			if (line == null)
			{
				return null;
			}

			return new DocCollectionOrderLine(line, factory);
		}

		public AccCollectionOrderLine AccCollectionOrderLine
		{
			get { return (AccCollectionOrderLine)WrappedObject; }
		}

		public ZString TransactionType => AccCollectionOrderLine.TransactionType;

		public ZString TransactionNumber => AccCollectionOrderLine.TransactionNumber;

		public ZDateTime TransactionDueDate => AccCollectionOrderLine.DueDate;

		public ZString TransactionDescription => AccCollectionOrderLine.Description;

		public ZString OSCurrencyCode => AccCollectionOrderLine.OSCurrency;

		public ZString LocalCurrencyCode => AccCollectionOrderLine.CollectionCurrency;

		public ZDecimal OSOutstandingAmount => AccCollectionOrderLine.OSOutstandingAmount;

		public ZDecimal LocalOutstandingAmount => AccCollectionOrderLine.CollectionAmount;
	}
}
