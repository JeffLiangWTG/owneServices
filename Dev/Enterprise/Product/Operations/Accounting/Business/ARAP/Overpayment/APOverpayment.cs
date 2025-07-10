using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Overpayment
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class APOverpayment : Overpayment
	{
		public APOverpayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("8baa3138-9dbf-4b4a-8648-a7e11ca41252", "Accounts Payable Overpayment"); }
		}

		protected override ZString Ledger
		{
			get
			{
				return LedgerTypes.AccountsPayable;
			}
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				return AccountingNumberFountainWrapperFactory.Instance.APOverpaymentsNo;
			}
		}

		protected override SecurityCheckpoint CheckpointToUnmatchCore => Env.Security.PayablesUnMatchTransactionsOverpaymentType;
	}
}
