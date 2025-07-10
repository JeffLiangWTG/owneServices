using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BatchTransaction : AccTransactionHeader
	{
		public BatchTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void Delete()
		{
			if (IsInDatabase)
			{
				throw new NotSupportedException("Delete is not supported for the BatchTransaction");
			}
			else
			{
				base.Delete();
			}
		}

		#endregion

		#region Debit

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal Debit
		{
			get
			{
				if (AH_TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.Receipt && AH_OSTotal < 0m)
				{
					return -AH_OSTotal;
				}
				else if (AH_TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.DirectReceipt && AH_OSTotal >= 0m)
				{
					return AH_OSTotal;
				}
				else
				{
					return 0m;
				}
			}
		}

		#endregion

		#region Credit

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal Credit
		{
			get
			{
				if (AH_TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.Receipt && AH_OSTotal >= 0m)
				{
					return AH_OSTotal;
				}
				else if (AH_TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.DirectReceipt && AH_OSTotal < 0m)
				{
					return -AH_OSTotal;
				}
				else
				{
					return 0m;
				}
			}
		}

		#endregion
	}
}
