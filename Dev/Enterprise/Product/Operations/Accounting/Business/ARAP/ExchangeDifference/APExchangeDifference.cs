using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP
{
	/// <summary>
	/// Summary description for APExchangeDifference.
	/// </summary>
	public class APExchangeDifference : ExchangeDifference
	{
		public APExchangeDifference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("fd83c73b-4d07-4e9e-9b6c-0fa76b8f858c", "Accounts Payable Exchange Difference"); }
		}

		protected override ZString Ledger
		{
			get
			{
				return ZArchitecture.Core.LedgerTypes.AccountsPayable;
			}
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get
			{
				return AccountingNumberFountainWrapperFactory.Instance.APExchangeDifferenceNo;
			}
		}
	}
}
