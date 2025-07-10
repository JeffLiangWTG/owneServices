using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Client.EDI.Mail.ServiceTasks.CustomerServiceEmailProcessorServiceTask.Code,
	"Customer Service Email Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.Mail.ServiceTasks.CustomerServiceEmailProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = EmailProcessorServiceProvider.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

[assembly: HostedService(Enterprise.Client.EDI.Mail.ServiceTasks.ImplementationEmailProcessorServiceTask.Code,
	"Implementation Email Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.Mail.ServiceTasks.ImplementationEmailProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = EmailProcessorServiceProvider.MinimumPeriod,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.Mail.ServiceTasks
{
	#region Customer Service Email Processor Service Task

	class CustomerServiceEmailProcessorServiceTask : OperationEmailProcessorServiceProvider<SupportIncident>
	{
		public const string Code = "CSE";

		public CustomerServiceEmailProcessorServiceTask() : base(new EmailReaderFactory())
		{
		}

		public CustomerServiceEmailProcessorServiceTask(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		// Overriding RunTask so exception stack trace shows which service task
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overriding to show in stack trace")]
		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				base.RunTask(token);
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected override BusinessObjectEmailProcessor<SupportIncident> GetEmailProcessor() { return new CustomerServiceEmailProcessor(); }
		protected override MailboxSettings MailboxSettings { get { return EDIDataRegistry.Instance.CustomerServiceMailBox; } }
	}

	#endregion

	#region Implementation Email Processor Service Task

	class ImplementationEmailProcessorServiceTask : OperationEmailProcessorServiceProvider<EDIProject>
	{
		public const string Code = "IEP";

		public ImplementationEmailProcessorServiceTask() : base(new EmailReaderFactory())
		{
		}

		public ImplementationEmailProcessorServiceTask(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		// Overriding RunTask so exception stack trace shows which service task
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Overriding to show in stack trace")]
		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				base.RunTask(token);
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		protected override BusinessObjectEmailProcessor<EDIProject> GetEmailProcessor() { return new ImplementationEmailProcessor(); }
		protected override MailboxSettings MailboxSettings { get { return EDIDataRegistry.Instance.ImplementationMailBox; } }
	}

	#endregion

	abstract class OperationEmailProcessorServiceProvider<T> : EdiEmailProcessorServiceProvider where T : BusinessObject, IAllowAttachEmailsToEDocs
	{
		protected OperationEmailProcessorServiceProvider(IEmailReaderFactory emailReaderFactory) : base(emailReaderFactory)
		{
		}

		protected override bool ProcessEmailCore(Email email)
		{
			return GetEmailProcessor().CreateAndProcessMailItem(email, this);
		}

		protected abstract BusinessObjectEmailProcessor<T> GetEmailProcessor();
	}
}
