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
	class JobInvoiceHeaderCreationErrorEmail : AccountingEmailDef
	{
		public JobInvoiceHeaderCreationErrorEmail(IJobInvoicingPlugIn job, NotificationCollection notifications)
			: base()
		{
			Argument.NotNull(job, "Job");
			Argument.NotNull(notifications, "Notifications");
			ContentType = EmailContentTypes.HTML;
			this.additionalErrors = notifications.ToMessageListString().Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
			subject = GetSubjectCore(job);
			body = GetBodyCore(job);
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
<p>{1}</p>
<ul>{2}</ul>
</body>
</html>",
				Res.GetString("55b702f3-1426-4033-87ed-cab8ba2700b6", "The invoice header {0} creation was ran as a workflow action. There were errors during the operation.", job.JobNumber),
				Res.GetString("bdb3aef9-52f0-41f8-8dde-7adc57ed0b7c", "Errors:"),
				string.Concat((from error in allErrors select string.Format((NoResString)"<li>{0}</li>", error)).ToArray())
				);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(IJobInvoicingPlugIn job)
		{
			return Res.GetString("18967445-3709-4437-8126-d0fd05ec6a6a", "The invoice header creation errors for Job {0}", job.JobNumber);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}


