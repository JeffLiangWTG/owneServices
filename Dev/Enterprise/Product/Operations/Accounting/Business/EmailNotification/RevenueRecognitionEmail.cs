using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class RevenueRecognitionEmail : AccountingEmailDef
	{
		public RevenueRecognitionEmail(Job job, string additionalErrors)
			: base()
		{
			if (job == null)
			{
				throw new ArgumentNullException(nameof(job));
			}

			if (additionalErrors == null)
			{
				throw new ArgumentNullException(nameof(additionalErrors));
			}

			ContentType = EmailContentTypes.HTML;
			this.additionalErrors = additionalErrors.Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);

			subject = GetSubjectCore(job);
			body = GetBodyCore(job);
		}

		readonly string additionalErrors;
		readonly string subject;
		readonly string body;

		protected override GuidRegistryItem Recipient
		{
			get { return AccountingConfigurationRegistry.Instance.RevenueRecognitionNotificationGroup; }
		}

		string GetBodyCore(Job job)
		{
			string link = "";

			var genericJob = job.GenericJobView;
			if (genericJob != null)
			{
				ControllerID consumerControllerID = genericJob.GetConsumerController();
				if (consumerControllerID != null)
				{
					link = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(consumerControllerID, job.JH_ParentID.ToGuid());
				}
			}

			var allErrors = new List<string>();
			foreach (string error in job.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
			{
				allErrors.Add(error);
			}
			allErrors.AddRange(additionalErrors.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

			return string.Format(
@"<html>
<body>
<p>{0}</p>
<p>{1}</p>
<ul>{2}</ul>
<p>{3}</p>
<p>{4} <a href=""{5}"">{6}</a></p>
</body>
</html>",
				Res.GetString("1f7ebda8-11d2-48d4-8f2c-4db06a86609f", "Revenue recognition was run as workflow action for the job. There were errors during the operation. It might be done successfully for valid dates and skipped for invalid dates."),
				Res.GetString("f17d2b23-1cb4-4f5a-9287-e2cc4459c6a8", "Errors:"),
				string.Concat((from error in allErrors select string.Format((NoResString)"<li>{0}</li>", error)).ToArray()),
				Res.GetString("e7946735-e4e5-438d-9066-6a221da1f191", "Job revenue recognition dates after this operation: {0}.", job.RevenueRecognitionDates),
				Res.GetString("423ea912-b77a-4c7d-bc9f-a4687d951273", "Revenue recognition can be done manually from Job Invoicing menu."),
				link,
				job.JH_JobNum
				);
		}

		string GetSubjectCore(Job job)
		{
			return Res.GetString("1E2F1E2F-FE4C-4E0B-A754-CD99711E0DC4", "Revenue recognition errors for Job {0}", job.JH_JobNum);
		}

		protected sealed override string GetBody()
		{
			return body;
		}

		protected sealed override string GetSubject()
		{
			return subject;
		}

#if DEBUG
		public string GetBody_ForTestOnly()
		{
			return GetBody();
		}

		public string GetSubject_ForTestOnly()
		{
			return GetSubject();
		}
#endif

	}
}
