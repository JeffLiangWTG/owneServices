using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Integration;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager
{
	public class MailFilterProvider : IMailFilterProvider
	{
		readonly ICollection<IMailFilter> filters;

		public MailFilterProvider()
			: this(AssemblyMetaDataReader.GetAttributes<MessageFilterAttribute>())
		{
		}

		internal MailFilterProvider(IEnumerable<IMessageFilterConfig> messageFilterConfigs)
		{
			filters = messageFilterConfigs
				.Where(a => a.TableName == MailDBItemsSchema.Constants.TableName)
				.GroupBy(a => a.ServiceTaskCode)
				.Select(kvp => CreateFilter(kvp.Key, kvp))
				.ToList<IMailFilter>();
		}

		static MessageFilterMailFilter CreateFilter(string code, IEnumerable<IMessageFilterConfig> configs)
		{
			var processorFactory = ObjectFactory.Get<IMessageProcessorFactory>(nameof(IMessageProcessorFactory), code, configs.ToArray());
			return new MessageFilterMailFilter(code, processorFactory.GetContext(null), processorFactory.GetProcessor<MailItem>(MailDBItemsSchema.Constants.TableName));
		}

		public IEnumerable<IMailFilter> GetFilters()
			=> filters;

		public bool TryGetFilter(string code, out IMailFilter filter)
		{
			filter = filters.FirstOrDefault(f => f.Code == code);
			return filter != null;
		}
	}

	public class MessageFilterMailFilter : IMailFilter
	{
		readonly IMessageProcessorContext context;
		readonly IMessageProcessor<MailItem> processor;

		public string Code { get; }

		public bool IsEnabled { get; set; }

		public MessageFilterMailFilter(string code, IMessageProcessorContext context, IMessageProcessor<MailItem> processor)
		{
			Code = code;
			this.context = context;
			this.processor = processor;
			IsEnabled = true;
		}

		public bool CanProcess(IMailItem item)
			=> processor.IsMatch(context, (MailItem)item);

		public ZQuery Query => CreateQuery(null);

		public ZQuery LoadQuery(int limit = -1)
		{
			return CreateQuery(limit > 0 ? limit : null);
		}

		ZQuery CreateQuery(int? limit)
			=> new ZQuery { OrderBy = MailDBItemsSchema.MI_ReceivedDateTime.Name, MaximumRows = limit }
				.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive)
				.AddToFilter(MailDBItemsSchema.MI_Application, MailFilterCodes.MailProcessingTask)
				.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued);

		public IMailItem[] Load(BusinessObjectFactory factory, int limit) => factory.Load<MailItem>(CreateQuery(limit > 0 ? limit : null));
	}
}
