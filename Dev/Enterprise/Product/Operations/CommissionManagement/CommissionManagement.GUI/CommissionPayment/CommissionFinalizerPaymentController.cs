using System.Linq;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionFinalizerPaymentController : CommissionPaymentController
	{
		public CommissionFinalizerPaymentController(ZForm parentForm, CommissionFinalizer commissionFinalizer)
			: base(parentForm, commissionFinalizer.GetNewCommissionPayment())
		{
		}

		protected override bool RunPreProcessPaymentValidation()
		{
			if (OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.Value > 0)
			{
				if (CommissionPayable.CommissionLinesForPayment.Any(x => !x.IsApproved))
				{
					Globals.Message.ShowError(Res.GetString("2b1efbf6-811c-4b92-9383-8c7c7e2712f9", "At least one selected entity commission has not been approved yet."), UnableToProcessPaymentCaption);
					return false;
				}
			}

			return base.RunPreProcessPaymentValidation();
		}
	}
}
