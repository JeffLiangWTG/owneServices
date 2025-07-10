using System;

namespace Enterprise.ZArchitecture.GUI
{
	public class MultistepProgressForm : ProgressForm
	{
		readonly int stepNumber;
		int currentStep;
		int lastPercentComplete;

		public MultistepProgressForm(int stepNumber)
		{
			if (stepNumber == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(stepNumber), stepNumber, "StepNumber cannot be zero.");
			}

			this.stepNumber = stepNumber;
		}

		protected override void ModifyStatusAndPercentComplete(ref string status, ref int percentComplete)
		{
			if (lastPercentComplete > percentComplete)
			{
				currentStep++;
			}
			lastPercentComplete = percentComplete;

			var multiStepStatus = Res.GetString("0eb5e6fb-27a1-4875-9e90-0f11659a096b", "Step {0} of {1}: {2}", currentStep + 1, stepNumber, status);
			var multiStepPercentageComplete = (100 * currentStep + percentComplete) / stepNumber;

			status = multiStepStatus;
			percentComplete = multiStepPercentageComplete;
		}
	}
}
