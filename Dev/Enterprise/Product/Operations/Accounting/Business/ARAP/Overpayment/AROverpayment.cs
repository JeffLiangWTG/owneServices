using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Overpayment
{
	/// <summary>
	/// Summary description for AROverpayment.
	/// </summary>
	public class AROverpayment : Overpayment
	{
		public AROverpayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString Ledger
		{
			get
			{
				return LedgerTypes.AccountsReceivable;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("392a8864-cffe-47a4-9d91-e7047996c928", "Accounts Receivable Overpayment"); }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				return AccountingNumberFountainWrapperFactory.Instance.AROverpaymentsNo;
			}
		}

		protected override SecurityCheckpoint CheckpointToUnmatchCore => Env.Security.ReceivablesUnMatchTransactionOverpaymentType;
	}
}
