using System.ComponentModel;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.ServiceTask;
using Enterprise.ServiceManager.Tasks.MailProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MailProcessingTask.Code,
	"Mail Processing",
	"MAI",
	typeof(MailProcessingTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: HostedServiceBusinessObjectBinding(MailProcessingTask.Code, MailDBItemsSchema.Constants.TableName, new[] {
	MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.MailProcessingTask,
	MailDBItemsSchema.Constants.MI_Direction + "=" + MailDirection.Receive,
	MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
}, "Mail Processing Task")]

//We don't need MailSubscriber/MailFilter attributes because MailFilterLocator.cs explicitly appends a new MailFilterProvider, which is the kind of filter that this uses.
namespace Enterprise.ServiceManager.Tasks.MailProcessor
{
	class MailProcessingTask : MessageProcessorTaskImpl
	{
		public override void RunTask(CancellationToken token)
		{
			base.RunTask(token);

			var processor = ProcessorFactory.GetProcessor<MailItem>();
			var ctx = ProcessorFactory.GetContext(ServiceLogger);

			var filter = new MessageFilterMailFilter(Code, ctx, processor);
			var queue = new DbOnlyBusinessObjectQueue<MailItem>(filter.Query);

			queue.ProcessBatch((mailItems, e) => DoProcess(processor, ctx, mailItems, e), 10, token);
		}

		#region Implementation

		void DoProcess(IMessageProcessor<MailItem> processor, IMessageProcessorContext ctx, MailItem[] mailItems, CancelEventArgs e)
		{
			foreach (var mailItem in mailItems)
			{
				if (processor.Process(ctx, mailItem))
				{
					mailItem.MI_Status = MailStatus.Processed;
				}
				else
				{
					mailItem.MI_Status = MailStatus.Failed;
				}
			}
			ZExceptionReporting.ProcessWithConcurrencyHandling(() => { mailItems[0].Factory.Save(); }, null);
		}

		#endregion

		public const string Code = MailFilterCodes.MailProcessingTask;
	}
}
