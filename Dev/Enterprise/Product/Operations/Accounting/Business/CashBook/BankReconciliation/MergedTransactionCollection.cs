using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook
{
	public class MergedTransactionCollection : BusinessObjectCollection<BusinessObject>
	{
		public MergedTransactionCollection(BusinessObjectFactory factory, BankReconciliation bankReconMaster)
			: base(factory)
		{
			this.fBankReconMaster = bankReconMaster;
		}

		public new IBankReconMergedTransaction this[int index]
		{
			get
			{
				return (IBankReconMergedTransaction)(Elements[index]);
			}
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			Type result = typeof(AccTransactionHeader);
			if (pK.IsValid)
			{
				result = FindByPK(pK).GetType();
			}
			return result;
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected BankReconciliation fBankReconMaster;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			((IBankReconMergedTransaction)bizOAdded).MasterBankRecon = fBankReconMaster;
		}

		public ZDecimal CalculateDebitTotal()
		{
			ZDecimal total = 0.0m;
			foreach (IBankReconMergedTransaction transaction in Elements)
			{
				total += transaction.Debit;
			}

			return total;
		}

		public ZDecimal CalculateCreditTotal()
		{
			ZDecimal total = 0.0m;
			foreach (IBankReconMergedTransaction transaction in Elements)
			{
				total += transaction.Credit;
			}

			return total;
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException("Loading to this collection is not supported.");
		}
	}
}
