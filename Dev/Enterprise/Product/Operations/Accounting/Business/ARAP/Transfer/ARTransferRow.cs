using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP
{
	public abstract class ARTransferRow : TransferRow
	{
		public ARTransferRow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsReceivable; }
		}

		protected override Type TypeOfTransferWrapper
		{
			get { return typeof(ARTransfer); }
		}

		#endregion
	}
}
