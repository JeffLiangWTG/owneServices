using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentWrappers
{
	[CodeAlive("Used in Document")]
	public class DocCollectionOrder : DocBaseWrapper
	{
		protected DocCollectionOrder(AccCollectionOrder accCollectionOrder, BusinessObjectFactory factory)
		: base(accCollectionOrder, factory)
		{
		}

		public static DocCollectionOrder New(AccCollectionOrder accCollectionOrder, BusinessObjectFactory factory)
		{
			if (accCollectionOrder == null)
			{
				return null;
			}
			return new DocCollectionOrder(accCollectionOrder, factory);
		}

		protected AccCollectionOrder CollectionOrder
		{
			get { return (AccCollectionOrder)WrappedObject; }
		}

		protected AccCollectionBatch CollectionBatch
		{
			get
			{
				if (fCollectionBatch == null)
				{
					fCollectionBatch = CollectionOrder.CollectionBatch;
				}
				return fCollectionBatch;
			}
		}
		AccCollectionBatch fCollectionBatch;

		#region Properties

		public ZString AccountCode => CollectionOrder.Debtor != null ? CollectionOrder.Debtor.OH_Code : ZString.Empty;

		public ZDate CollectionDate => CollectionOrder.ACO_CollectionDate;

		public ZString CollectionBatchNumber => CollectionBatch.ACB_BatchNumber;

		public ZString CollectionOrderNumber => CollectionOrder.ACO_OrderNumber;

		public ZString CollectionBankCode => CollectionBatch.BankAccount.AB_Code;

		public ZString CollectionBatchBankCurrency => CollectionOrder.ACO_RX_NKCurrency;

		public ZDecimal OrderTotalAmount => CollectionOrder.ACO_Amount;

		public ZString DebtorBankName => CollectionOrder.CollectionRequestBankName;

		public ZString DebtorBankAccountName => CollectionOrder.CollectionRequestAccountName;

		public ZString DebtorBankAndBranchCode => CollectionOrder.CollectionRequestBankBsb;

		public ZString DebtorBankSwift => CollectionOrder.CollectionRequestBankSwift;

		public ZString DebtorBankCountry => CollectionOrder.CollectionRequestBankCountry;

		public ZString DebtorBankAccountCurrency => CollectionOrder.CollectionRequestAccountCurrency;

		public ZString DebtorBankAccountNumber => CollectionOrder.CollectionRequestAccountNumber;

		public ZString DebtorBankAccountIBAN => CollectionOrder.CollectionRequestIBANNumber;

		#endregion

		public DocCollectionOrderLineCollection OrderLines
		{
			get
			{
				return orderLines ?? (orderLines = GetOrderLines());
			}
		}

		DocCollectionOrderLineCollection orderLines;

		DocCollectionOrderLineCollection GetOrderLines()
		{
			var result = new DocCollectionOrderLineCollection(Factory);
			if (CollectionOrder != null)
			{
				foreach (AccCollectionOrderLine line in CollectionOrder.CollectionOrderLines)
				{
					var docLine = DocCollectionOrderLine.New(line, Factory);
					result.Add(docLine);
				}
			}

			return result;
		}
	}
}
