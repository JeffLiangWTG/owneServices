using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	public static class PeriodicInvoiceErrorHandleHelper
	{
		public static void OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			string errorMessage = string.Empty;
			if (e is CriticalChargePostingErrorEventArgs)
			{
				var criticalChargePostingErrorEventArgs = e as CriticalChargePostingErrorEventArgs;
				if (criticalChargePostingErrorEventArgs.Charges != null && criticalChargePostingErrorEventArgs.Charges.Length > 0)
				{
					var jobNumbers = criticalChargePostingErrorEventArgs.Charges.Select(x => x.Job.JH_JobNum).Distinct().OrderBy(x => x);
					errorMessage = Res.GetString("eb7e54ee-b305-4aeb-81ce-c1bd373974b2", "Error occurred for the following Job(s): {0}", new ZStringBuilder(jobNumbers).ToStringWithDelimiterBetweenAppends(", "));
				}
			}
			if (!string.IsNullOrEmpty(errorMessage))
			{
				errorMessage += System.Environment.NewLine + System.Environment.NewLine + Res.GetString("5fcd2626-cd88-488c-a5dc-3d768880e0e6", "Summary of the error(s):") + System.Environment.NewLine + e.ErrorMessage;
			}
			else
			{
				errorMessage = e.ErrorMessage;
			}

			Globals.Message.ShowError(errorMessage);
		}
	}
}
