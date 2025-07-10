using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.MailManager.ServiceTask
{
	public abstract class MessageProcessorTaskImpl : ServiceProviderImpl, IMessageFiltersUser
	{
		public override void RunTask(CancellationToken cancellationToken)
		{
			var messageFiltersUser = this as IMessageFiltersUser;
			if (messageFiltersUser.NeedFactory())
			{
				messageFiltersUser.SetFactory(new MessageProcessorFactory(ServiceCode, messageFilters.Value));
			}
		}

		protected IMessageProcessorFactory ProcessorFactory
		{
			get
			{
				return processorFactories.TryGetValue(GetType(), out var result) ? result : null;
			}
			private set
			{
				processorFactories[GetType()] = value;
			}
		}

		static readonly ConcurrentDictionary<Type, IMessageProcessorFactory> processorFactories = new ();

		bool IMessageFiltersUser.NeedFactory()
		{
			return ProcessorFactory == null;
		}

		void IMessageFiltersUser.SetFactory(IMessageProcessorFactory factory)
		{
			ProcessorFactory = factory;
		}

		readonly Lazy<IMessageFilterConfig[]> messageFilters = new (() =>
			AssemblyMetaDataReader
				.GetAttributes<MessageFilterAttribute>()
				.Where(a => a.ClientSpecificCode == Clients.None)
				.ToArray());
	}
}
