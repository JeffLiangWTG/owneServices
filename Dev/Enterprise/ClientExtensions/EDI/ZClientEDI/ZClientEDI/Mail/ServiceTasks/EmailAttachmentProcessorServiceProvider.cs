using System;
using System.Threading;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.LicenceKeyBuilder.BatchProcessor;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.VersionReporting.BatchProcessor;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Client.EDI.Mail.ServiceTasks.SupportRequestProcessorServiceTask.Code,
	"Support Request Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.Mail.ServiceTasks.SupportRequestProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = EmailProcessorServiceProvider.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

[assembly: HostedService(
	Enterprise.Client.EDI.Mail.ServiceTasks.CurrentVersionReportProcessorServiceTask.Code,
	"Current Version Report Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.Mail.ServiceTasks.CurrentVersionReportProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = EmailProcessorServiceProvider.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

[assembly: HostedService(
	Enterprise.Client.EDI.Mail.ServiceTasks.DeliveredVersionReportProcessorServiceTask.Code,
	"Delivered Version Report Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.Mail.ServiceTasks.DeliveredVersionReportProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = EmailProcessorServiceProvider.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.Mail.ServiceTasks
{
	#region Support Request

	class SupportRequestProcessorServiceTask : EmailAttachmentProcessorServiceProvider
	{
		public const string Code = "SRT";

		public SupportRequestProcessorServiceTask() : base(new EmailReaderFactory())
		{
		}

		public SupportRequestProcessorServiceTask(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		// Overriding RunTask so exception stack trace shows which service task
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overridden so service task appears in stack trace")]
		public override void RunTask(CancellationToken token) { base.RunTask(token); }
		protected override IEmailAttachmentProcessor GetAttachmentProcessor(Email email) { return new SupportRequestProcessor(ServiceLogger); }
		protected override MailboxSettings MailboxSettings { get { return EDIDataRegistry.Instance.SupportRequestMailBox; } }
	}

	#endregion

	#region Current Version Report

	class CurrentVersionReportProcessorServiceTask : EdiEmailProcessorServiceProvider
	{
		public const string Code = "CVR";

		public CurrentVersionReportProcessorServiceTask() : base(new EmailReaderFactory())
		{
		}

		public CurrentVersionReportProcessorServiceTask(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		// Overriding RunTask so exception stack trace shows which service task
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overriding to show in stack trace")]
		public override void RunTask(CancellationToken token) { base.RunTask(token); }

		internal const string LicenceUsagePassword = "eV7pXwikS3k"; // see LicenceUsageReportBuilder

		protected override bool ProcessEmailCore(Email email)
		{
			string attachmentText = email.GetFirstAttachmentOrVisualText(LicenceUsagePassword);
			bool isUsage = false;
			bool isVersion = false;
			if (email.Subject.Contains("Enterprise Report") || email.Subject.Contains("Licence Usage Data"))
			{
				isUsage = true;
			}
			else if (email.Subject.Contains("Version Report"))
			{
				isVersion = true;
			}

			if (!string.IsNullOrEmpty(attachmentText)
				&& attachmentText.StartsWith("<?xml"))
			{
				IEmailAttachmentProcessor processor = attachmentText.IndexOf("LicenceConsumptionLog", 0, Math.Min(attachmentText.Length, 200)) >= 0
					? new LicenceUsageProcessor(ServiceLogger)
					: CreateCurrentVersionReportProcessor();

				processor.Process(attachmentText);
			}
			else if (isUsage)
			{
				ForwardUnprocessedEmail(email, null, "invalid usage from: " + email.SenderNameAddress);
				ReportProcessorHelper.SaveReport(ServiceLogger, "LicenceUsageBad", email.SenderAddress + ".eml", email.GetEml());
			}
			else if (isVersion)
			{
				ForwardUnprocessedEmail(email, null, "invalid version from: " + email.SenderNameAddress);
			}
			else
			{
				// invalid subject is almost always spam - ignore it
			}

			return true;
		}

		internal CurrentVersionReportProcessor CreateCurrentVersionReportProcessor()
		{
			return new CurrentVersionReportProcessor(ServiceLogger, false);
		}

		protected override MailboxSettings MailboxSettings { get { return EDIDataRegistry.Instance.CurrentVersionReportMailBox; } }
	}

	#endregion

	#region Delivered Version Report

	class DeliveredVersionReportProcessorServiceTask : EmailAttachmentProcessorServiceProvider
	{
		public const string Code = "DVR";

		public DeliveredVersionReportProcessorServiceTask() : base(new EmailReaderFactory())
		{
		}

		public DeliveredVersionReportProcessorServiceTask(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		// Overriding RunTask so exception stack trace shows which service task
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overriding to show in stack trace")]
		public override void RunTask(CancellationToken token) { base.RunTask(token); }
		protected override IEmailAttachmentProcessor GetAttachmentProcessor(Email email) { return new DeliveredVersionReportProcessor(email.From, ServiceLogger); }
		protected override MailboxSettings MailboxSettings { get { return EDIDataRegistry.Instance.DeliveredVersionReportMailBox; } }
	}

	#endregion

	abstract class EmailAttachmentProcessorServiceProvider : EdiEmailProcessorServiceProvider
	{
		protected EmailAttachmentProcessorServiceProvider(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		protected override bool ProcessEmailCore(Email email)
		{
			string attachmentText = email.GetFirstAttachmentOrVisualText();
			if (!string.IsNullOrEmpty(attachmentText))
			{
				GetAttachmentProcessor(email).Process(attachmentText);
			}
			return true;
		}

		protected abstract IEmailAttachmentProcessor GetAttachmentProcessor(Email email);
	}
}
