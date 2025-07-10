using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP
{
	public class APContraRow : ContraRow
	{
		public APContraRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Transaction Header Overrides

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("6e69303d-1b36-4876-b6ba-79a0c2b13fac", "Accounts Payable Contra"); }
		}

		#region GetTopLevelTransaction

		public override IMatching GetTopLevelTransaction
		{
			get
			{
				IMatching transactionToReturn = null;
				if (RelatedTransactions.Count == 1 && RelatedTransactions[0] is ContraRow)
				{
					transactionToReturn = Contra.Load(Factory, RelatedTransactions[0] as ARContraRow, this, Ledger);
				}

				return transactionToReturn;
			}
		}

		#endregion

		#endregion
	}
}
