using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public class CommissionAgreementApproveProgressController
	{
		public void Show(Action<Progress> action, ZForm modalForm)
		{
			using (var progressForm = new CommissionAgreementApproveProgressForm())
			{
				progressForm.Status = Res.GetString("bda2157d-a861-446a-acb7-2fb0adac2d22", "Creating commissions...");
				progressForm.ShowCancelButton = false;
				ZFormModaliser.Show(progressForm, modalForm);

				Progress onAgreementApproverProgress = (string status, int percentage) =>
				{
					progressForm.Status = status;
					progressForm.PercentComplete = percentage;
				};

				action(onAgreementApproverProgress);
			}
		}
	}
}
