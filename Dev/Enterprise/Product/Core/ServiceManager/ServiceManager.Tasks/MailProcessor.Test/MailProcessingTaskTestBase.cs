using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.Test;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing
{
	abstract class MailProcessingTaskTestBase<T> : ServiceTaskTestCase<T>
		where T : ServiceProviderImpl
	{
		protected void RunTaskSchedule(T serviceTask, IMessageProcessorFactory processorFactory, CancellationToken token)
		{
			var messageFiltersUser = serviceTask as IMessageFiltersUser;
			if (messageFiltersUser != null && messageFiltersUser.NeedFactory())
			{
				messageFiltersUser.SetFactory(processorFactory);
			}
			RunTaskSchedule(serviceTask, token);
		}

		protected void RunTaskScheduleWithProcessorFactory<TFilter, TBizObj>(T serviceTask, CancellationToken token) where TBizObj : BusinessObject
		{
			var helper = new MessageFilterTestHelper<TFilter, TBizObj>();
			helper.Log = (TestServiceLogger)serviceTask.ServiceLogger;
			RunTaskSchedule(serviceTask, helper.ProcessorFactory, token);
		}

		protected void RunTaskScheduleWithProcessorFactory<TFilter, TBizObj>(T serviceTask) where TBizObj : BusinessObject
		{
			RunTaskScheduleWithProcessorFactory<TFilter, TBizObj>(serviceTask, CancellationToken.None);
		}
	}
}
