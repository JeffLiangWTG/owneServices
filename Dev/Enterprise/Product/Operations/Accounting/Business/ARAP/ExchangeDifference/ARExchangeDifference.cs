using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP
{
	/// <summary>
	/// Summary description for ARExchangeDifference.
	/// </summary>
	public class ARExchangeDifference : ExchangeDifference
	{
		public ARExchangeDifference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("32c3a787-9b0e-41c1-915a-805aa82e6aec", "Accounts Receivable Exchange Difference"); }
		}

		protected override ZString Ledger
		{
			get
			{
				return ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			}
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				return AccountingNumberFountainWrapperFactory.Instance.ARExchangeDifferenceNo;
			}
		}
	}
}
