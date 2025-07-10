using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module Controller for Payment Processing.
	/// </summary>
	public abstract class PaymentProcessingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected PaymentProcessingController()
		{
		}

		public static string CannotDeletePostedTransactionMessage
		{
			get { return Res.GetString("8d9d242a-1f90-4b09-ad05-93162687c064", "You cannot Delete this Payment Approval because it has been posted"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			PaymentApprovalWithAuthorisation bizObj = businessEntity as PaymentApprovalWithAuthorisation;
			PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(bizObj);
			return form;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (sourceEntity != null)
			{
				PaymentApprovalWithAuthorisation approval = sourceEntity.Factory.Load<PaymentApprovalWithAuthorisation>(sourceEntity.PK);

				if (approval.AV_Status == ZArchitecture.Core.PaymentApprovalStatus.Posted)
				{
					return ShowViewForm(sourceEntity);
				}
			}

			return base.ShowEditForm(sourceEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (sourceEntity != null)
			{
				PaymentApprovalWithAuthorisation approvalToDelete = sourceEntity.Factory.Load<PaymentApprovalWithAuthorisation>(sourceEntity.PK);

				if (!approvalToDelete.CanDelete)
				{
					throw new ControllerShowDeleteFormNotSupportedException(CannotDeletePostedTransactionMessage);
				}
			}

			return base.ShowDeleteForm(sourceEntity);
		}
	}
}
