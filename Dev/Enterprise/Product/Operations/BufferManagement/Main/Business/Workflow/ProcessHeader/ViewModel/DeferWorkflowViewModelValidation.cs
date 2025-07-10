using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class DeferWorkflowViewModelValidation : ZValidation
	{
		public DeferWorkflowViewModelValidation(DeferWorkflowViewModel parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly DeferWorkflowViewModel parent;

		public override Type AutoValidationType
		{
			get { return typeof(ProcessHeader); }
		}

		public override void ValidateAll()
		{
			ValidateDoNotStartBeforeDate();
			ValidateWorkflowsHint();
			ValidateDeferralReason();
		}

		public void ValidateDoNotStartBeforeDate()
		{
			ValidateCalculatedProperty(parent.DoNotStartBeforeDateInfo);
		}

		protected void CheckDoNotStartBeforeDate()
		{
			MandatoryValidation.CheckEntered(parent.DoNotStartBeforeDateInfo);

			if (parent.DoNotStartBeforeDate < ZDateTime.Now)
			{
				parent.DoNotStartBeforeDateInfo.AddError(Res.GetString("8edf325d-ff2d-4721-89f1-26919937f70e", "Defer date must be in the future."));
			}
		}

		public void ValidateWorkflowsHint()
		{
			ValidateCalculatedProperty(parent.WorkflowsHintInfo);
		}

		protected void CheckWorkflowsHint()
		{
			if (!parent.Workflow.IsWorkflow)
			{
				if (!parent.WorkflowsToDefer.Cast<WorkflowToDeferBusinessObject>().Any(x => x.ActionToBeTaken == WorkflowDeferalActionList.Codes.Defer))
				{
					parent.WorkflowsHintInfo.AddError(Res.GetString("fddb053c-1f62-4193-a661-80a0ddfdf981", "Please select at least one workflow to defer."));
				}
			}
		}

		public void ValidateDeferralReason()
		{
			ValidateCalculatedProperty(parent.DeferralReasonInfo);
		}

		protected void CheckDeferralReason()
		{
			if (parent.DeferralReasonsList.CodesAsString.Any())
			{
				MandatoryValidation.CheckEntered(parent.DeferralReasonInfo);
				ListValidation.ErrorIfInvalidCode(parent.DeferralReasonInfo);
			}
		}
	}
}
