using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class IncludeChargeInProfitShareErrorEmail : AccountingEmailDef
	{
		public IncludeChargeInProfitShareErrorEmail(IJobInvoicingPlugIn plugIn, NotificationCollection notifications)
		{
			Argument.NotNull(plugIn, "PlugIn");
			Argument.NotNull(notifications, "Notifications");
			ContentType = EmailContentTypes.HTML;
			this.additionalErrors = notifications.ToMessageListString().Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
			subject = GetSubjectCore(plugIn);
			body = GetBodyCore(plugIn);
		}

		readonly string additionalErrors;
		readonly string subject;
		readonly string body;

		protected override string GetBody()
		{
			return body;
		}

		string GetBodyCore(IJobInvoicingPlugIn job)
		{
			var allErrors = new List<string>();
			allErrors.AddRange(additionalErrors.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

			return string.Format(
@"<html>
<body>
<p>{0}</p>
<ul>{1}</ul>
</body>
</html>",
				Res.GetString("919a2454-2ab0-450a-b3e6-b440580baaf2", "Include Charge in Profit share was run as a workflow actions and an attempt to run for Job {0} failed because of the following error(s):", job.JobNumber),
				string.Concat((from error in allErrors select string.Format((NoResString)"<li>{0}</li>", error)).ToArray())
				);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(IJobInvoicingPlugIn job)
		{
			return Res.GetString("185e71ec-82b5-4882-b409-38612318bf9a", "Include Charge in Profit Share failed for Job {0}", job.JobNumber);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}

