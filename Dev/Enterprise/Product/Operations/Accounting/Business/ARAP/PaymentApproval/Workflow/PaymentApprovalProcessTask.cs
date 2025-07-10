using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalProcessTask : ProcessTask
	{
		public PaymentApprovalProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void CancelIfParentIsCancelled()
		{
		}

		protected override Type ParentType
		{
			get { return typeof(PaymentApprovalBase); }
		}

		public override ControllerID ParentControllerID
		{
			get
			{
				var paymentApproval = (PaymentApprovalBase)Parent;
				if (paymentApproval == null)
				{
					return null;
				}
				else if (paymentApproval.AV_Ledger == LedgerTypes.AccountsPayable)
				{
					return ControllerIDs.APPaymentProcessing;
				}
				else
				{
					return ControllerIDs.ARPaymentProcessing;
				}
			}
		}
	}
}
