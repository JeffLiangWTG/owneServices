using CargoWise.ComponentModel;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionApprovalRequestPaymentController : CommissionPaymentController
	{
		public CommissionApprovalRequestPaymentController(ZForm parentForm, AccCommissionApprovalRequest approvalRequest)
			: base(parentForm, approvalRequest)
		{
		}

		AccCommissionApprovalRequest ApprovalRequest
		{
			get { return (AccCommissionApprovalRequest)base.CommissionPayable; }
		}

		protected override bool RunPreProcessPaymentValidation()
		{
			if (!ApprovalRequest.IsApproved)
			{
				Globals.Message.ShowError(Res.GetString("f5a6ccbc-6a75-4d87-9b75-baaa91e3b554", "Cannot process payment until all authorization staff have approved this request."), UnableToProcessPaymentCaption);
				return false;
			}

			ApprovalRequest.CommissionApprovalRequestItemGroupingCollection.AddFullPaymentNotifications(NotificationType.Warning, true);
			return base.RunPreProcessPaymentValidation();
		}
	}
}
