using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using OrgBalance = Enterprise.Accounting.Business.OrganisationBalance.OrganisationBalance;

namespace Enterprise.Accounting.Business.ARAP
{
	public class ARTransfer : Transfer
	{
		protected ARTransfer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override Type TransferFromType
		{
			get { return typeof(ARTransferFromRow); }
		}

		public override Type TransferToType
		{
			get { return typeof(ARTransferToRow); }
		}

		internal override AccountingNumberFountainWrapper TransferNumberFountain
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.ARTransferNo; }
		}

		public override OrgBalance OutstandingBalanceFrom
		{
			get { return new OrgBalance(AH_FromAccount, ZArchitecture.Core.LedgerTypes.AccountsReceivable); }
		}

		public override OrgBalance OutstandingBalanceTo
		{
			get { return new OrgBalance(AH_ToAccount, ZArchitecture.Core.LedgerTypes.AccountsReceivable); }
		}

		#region UserAllowedToBackPost

		public override bool UserAllowedToBackPost
		{
			get { return Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed; }
		}

		#endregion
	}
}
