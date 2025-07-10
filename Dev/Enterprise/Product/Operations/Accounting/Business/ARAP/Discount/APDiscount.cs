using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP
{
	/// <summary>
	/// Summary description for APDiscount.
	/// </summary>
	public class APDiscount : Discount
	{
		public APDiscount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("eabb1cab-ede7-4f0e-b23c-19d102a08ba6", "Accounts Payable Discount"); }
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
				return AccountingNumberFountainWrapperFactory.Instance.APDiscountNo;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_AG = AccountingConfigurationRegistry.Instance.APDiscountAccount.Value == Guid.Empty ?
					AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value : AccountingConfigurationRegistry.Instance.APDiscountAccount.Value;
		}
	}
}
