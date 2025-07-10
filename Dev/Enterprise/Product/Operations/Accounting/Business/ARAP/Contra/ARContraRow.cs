using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP
{
	public class ARContraRow : ContraRow
	{
		public ARContraRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Transaction Header Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("6ddb7628-c16e-4398-993e-c11447d88318", "Accounts Receivable Contra Row"); }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		public override IMatching GetTopLevelTransaction
		{
			get
			{
				IMatching transactionToReturn = null;
				if (RelatedTransactions.Count == 1 && RelatedTransactions[0] is ContraRow)
				{
					transactionToReturn = Contra.Load(Factory, this, RelatedTransactions[0] as APContraRow, Ledger);
				}

				return transactionToReturn;
			}
		}

		#endregion
	}
}
