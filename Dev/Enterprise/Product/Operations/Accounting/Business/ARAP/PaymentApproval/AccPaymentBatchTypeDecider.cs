using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace EEnterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class AccPaymentBatchTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return typeof(APPaymentBatchPoster);
		}

		public override Type GetTypeForNew()
		{
			return typeof(APPaymentBatchPoster);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(APPaymentBatchPoster);
		}
	}
}
