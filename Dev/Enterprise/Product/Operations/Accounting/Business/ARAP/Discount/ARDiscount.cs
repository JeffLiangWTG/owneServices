using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP
{
	/// <summary>
	/// Summary description for ARDiscount.
	/// </summary>
	public class ARDiscount : Discount
	{
		public ARDiscount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d4fcacac-53dd-499d-9fd7-9b5bda18a611", "Accounts Receivable Discount"); }
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
				return AccountingNumberFountainWrapperFactory.Instance.ARDiscountNo;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_AG = AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value;
		}
	}
}