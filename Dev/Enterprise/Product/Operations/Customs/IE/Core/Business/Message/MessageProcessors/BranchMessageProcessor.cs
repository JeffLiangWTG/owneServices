using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class BranchMessageProcessor : BranchCustomsMessageProcessor
	{
		public BranchMessageProcessor() : base(new ZString[]
		{
			EDIMessage.ApplicationCodes.IECustomsExport,
			EDIMessage.ApplicationCodes.IECustomsImport,
			EDIMessage.ApplicationCodes.IECustomsUCC5Import,
			EDIMessage.ApplicationCodes.IECustomsEMCS,
			EDIMessage.ApplicationCodes.IECustomsNCTS,
			EDIMessage.ApplicationCodes.IECustomsAndExcise,
			EDIMessage.ApplicationCodes.IECustomsPBN
		}, null) { }

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(Enterprise.Messaging.Business.EDIMessage message)
		{
			cachedProcessors = cachedProcessors ?? new Dictionary<Enterprise.Messaging.Business.EDIMessage, ApplicationTypeMessageProcessor>();

			if (!cachedProcessors.TryGetValue(message, out var processor))
			{
				var responseDetail = ResponseMessageDetails.GetResponseDetail(message.EM_ApplicationCode, message.EM_MessageType, message.EM_MessageSubType, message.EM_MessageText);
				var processorType = responseDetail?.ProcessorType;
				if (processorType != null)
				{
					if (processorType.IsSubclassOfRawGeneric(typeof(MessageProcessor<,>)) ||
						processorType.IsSubclassOfRawGeneric(typeof(CustomsAndExciseReportInboundMessageProcessor<>)))
					{
						processor = (ApplicationTypeMessageProcessor)Activator.CreateInstance(processorType, Logger, responseDetail.XmlObjectType);
					}
					else
					{
						processor = (ApplicationTypeMessageProcessor)Activator.CreateInstance(processorType, Logger);
					}
				}
				cachedProcessors.Add(message, processor);
			}

			return processor;
		}
		Dictionary<Enterprise.Messaging.Business.EDIMessage, ApplicationTypeMessageProcessor> cachedProcessors;

		protected override EDIMessageComparer EDIMessageComparer => new IEEDIMessageComparer(ListSortDirection.Ascending);

		protected override int MessagesPerSaveCore => 1;
	}
}
