using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using OrgBalance = Enterprise.Accounting.Business.OrganisationBalance.OrganisationBalance;

namespace Enterprise.Accounting.Business.ARAP
{
	public class APTransfer : Transfer
	{
		protected APTransfer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type TransferFromType
		{
			get { return typeof(APTransferFromRow); }
		}

		public override Type TransferToType
		{
			get { return typeof(APTransferToRow); }
		}

		public override OrgBalance OutstandingBalanceFrom
		{
			get { return new OrgBalance(AH_FromAccount, ZArchitecture.Core.LedgerTypes.AccountsPayable); }
		}

		internal override AccountingNumberFountainWrapper TransferNumberFountain
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.APTransferNo; }
		}

		public override OrgBalance OutstandingBalanceTo
		{
			get { return new OrgBalance(AH_ToAccount, ZArchitecture.Core.LedgerTypes.AccountsPayable); }
		}

		#region UserAllowedToBackPost

		public override bool UserAllowedToBackPost
		{
			get { return Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed; }
		}

		#endregion
	}
}
