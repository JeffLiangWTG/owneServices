using System;
using System.Data;
using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string ledger = row[PaymentApprovalBase.Schema.AV_Ledger].ToString();

			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					return typeof(APPaymentApprovalWithAuthorisation);
				case LedgerTypes.AccountsReceivable:
					return typeof(ARPaymentApprovalWithAuthorisation);
				default:
					throw new ArgumentException("Ledger must be AR or AP");
			}
		}

		public override Type GetTypeForNew()
		{
			throw new NoConcreteTypeException("New Payment Approval type cannot be determined");
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}
	}
}
