using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.EmailNotification
{
	class ProfitShareChargeWorkflowProcessorErrorEmail : AccountingEmailDef
	{
		public ProfitShareChargeWorkflowProcessorErrorEmail(IJobInvoicingPlugIn plugIn, IJobCostingPlugIn consol, NotificationCollection notifications)
			: base()
		{
			Argument.NotNull(notifications, "Notifications");
			if (plugIn == null && consol == null)
			{
				throw new ArgumentException("Both PlugIn and Consol cannot be null.");
			}
			ContentType = EmailContentTypes.HTML;
			errors = notifications.ToMessageListString().Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
			subject = GetSubjectCore(plugIn, consol);
			body = GetBodyCore(plugIn, consol);
		}

		readonly string errors;
		readonly string subject;
		readonly string body;

		protected override string GetBody()
		{
			return body;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String is html markup")]
		string GetBodyCore(IJobInvoicingPlugIn job, IJobCostingPlugIn consol)
		{
			var allErrors = new List<string>();
			allErrors.AddRange(errors.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

			return string.Format(
@"<html>
<body>
<p>{0}</p>
<p>{1}</p>
<ul>{2}</ul>
</body>
</html>",
			Res.GetString("5cbf5fa8-105e-4c72-ac7c-20195f610889", "Profit Share Charge creation was ran as a workflow action for {0}. There were errors during the operation."
				, consol == null ? "Job " + job.JobNumber : "Consol " + consol.JK_UniqueConsignRef),
			Res.GetString("3d0d9e91-b58a-49e6-9b53-911190dbc8de", "Errors:"),
				string.Concat((from error in allErrors select string.Format("<li>{0}</li>", error)).ToArray())
			);
		}

		protected override string GetSubject()
		{
			return subject;
		}

		string GetSubjectCore(IJobInvoicingPlugIn job, IJobCostingPlugIn consol)
		{
			return Res.GetString("85399f9c-b977-4c99-a716-3fe29123e84f", "Profit Share Charge Creation errors for {0}", consol == null ? (NoResString)"Job " + job.JobNumber : (NoResString)"Consol " + consol.JK_UniqueConsignRef);
		}

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup; }
		}
	}
}
