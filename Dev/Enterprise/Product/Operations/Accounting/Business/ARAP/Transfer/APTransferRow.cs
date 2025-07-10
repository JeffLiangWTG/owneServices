using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP
{
	public abstract class APTransferRow : TransferRow
	{
		public APTransferRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		protected override Type TypeOfTransferWrapper
		{
			get { return typeof(APTransfer); }
		}

		#endregion
	}
}
