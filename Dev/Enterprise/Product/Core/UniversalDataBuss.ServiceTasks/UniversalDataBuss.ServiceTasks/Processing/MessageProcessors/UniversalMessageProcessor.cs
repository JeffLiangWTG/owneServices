using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public class UniversalMessageProcessor : ApplicationTypeMessageProcessor
	{
		readonly IFactoryService factoryService;

		public UniversalMessageProcessor(LoggingInformation logger, IEnumerable<string> excludedSubTypes, IFactoryService factoryService)
			: base(logger)
		{
			this.excludedSubTypes = Argument.NotNull(excludedSubTypes, "messageSubTypes").ToArray();
			Argument.NotNull(factoryService, nameof(factoryService));

			this.factoryService = factoryService;
		}

		protected IFactoryService FactoryService => factoryService;

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.UniversalDataMessaging; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return (NoResString)"XML Universal Data Message"; }
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			this.factoryService.GetFactory<Func<LoggingInformation, UniversalMessageProcessingManager>>().Invoke(Logger).Process(message);
		}

		protected sealed override ZQuery MessageFilterCore => AddSubTypesToQuery(base.MessageFilterCore);

		protected virtual ZQuery AddSubTypesToQuery(ZQuery query)
		{
			var result = query.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			if (excludedSubTypes.Length > 0)
			{
				result.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, excludedSubTypes);
			}

			return result;
		}

		readonly string[] excludedSubTypes;
	}
}
