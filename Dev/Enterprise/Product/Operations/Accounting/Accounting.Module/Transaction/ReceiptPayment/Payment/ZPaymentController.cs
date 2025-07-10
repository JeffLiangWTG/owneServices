using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public abstract partial class ZPaymentController : AccountingTransactionController
	{
		protected ZPaymentController()
		{
		}

		public override IZForm ShowNewForm()
		{
			if (CheckPointForNew.IsAllowed && ShowPaymentApprovalBusinessObjectForNew)
			{
				PaymentApprovalBase approval = GetNewPaymentApproval();
				return ShowFormForNewEntity(approval);
			}

			return base.ShowNewForm();
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			if (businessEntity is PaymentApprovalBase)
			{
				return new PaymentApprovalForm((PaymentApprovalBase)businessEntity);
			}
			else if (businessEntity is APPayment)
			{
				SetArgsForNewForm(new[] { ModuleID?.ToString() });
				return new APPaymentForm((APPayment)businessEntity);
			}
			else
			{
				SetArgsForNewForm(new[] { ModuleID?.ToString() });
				return new PaymentForm((ARPayment)businessEntity);
			}
		}

		protected abstract bool ShowPaymentApprovalBusinessObjectForNew
		{
			get;
		}

		protected abstract PaymentApprovalBase GetNewPaymentApproval();
	}
}
