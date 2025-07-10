using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	public static class JobStatusUpdateRestrictionRuleHelper
	{
		public static string ValidateUpdateJobStatus(Job job)
		{
			var errorMessage = string.Empty;
			var oldJobStatus = (ZString)job.JH_StatusInfo.OriginalValue;
			var newJobStatus = job.JH_Status;

			if (!oldJobStatus.IsEmpty && !newJobStatus.IsEmpty && oldJobStatus != newJobStatus && job.IsInDatabase)
			{
				var rule = AccountingConfigurationRegistry.Instance.JobStatusUpdateRestrictionRule.Value.Cast<JobStatusUpdateRestrictionRule>().FirstOrDefault(x => x.JobStatus == oldJobStatus);
				if (rule != null)
				{
					bool isRestricted = newJobStatus == JobHeaderStatus.Closed.Code || rule.IsRestricted(newJobStatus);
					if (isRestricted && !(rule.RelatedSecurityRight?.IsAllowed ?? false))
					{
						var revertOldValueErrorMessage = string.Format(CultureInfo.InvariantCulture, JobHeaderValidation.DisallowOverrideofFieldSecurityErrorMessage, ((CodeDescriptionPair)job.JobStatusList[oldJobStatus]).CodeAndDescription);
						errorMessage = FormattableString.Invariant($"{rule.RelatedSecurityRight.ErrorMessageForNotAllowed}{System.Environment.NewLine}{System.Environment.NewLine}{revertOldValueErrorMessage}");
					}
				}
			}

			return errorMessage;
		}
	}
}
