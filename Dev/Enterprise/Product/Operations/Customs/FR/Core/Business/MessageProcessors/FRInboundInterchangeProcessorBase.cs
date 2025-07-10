using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class FRInboundInterchangeProcessorBase : InboundInterchangeProcessor
	{
		protected FRInboundInterchangeProcessorBase(LoggingInformation logger)
			: base(logger)
		{
		}

		protected abstract string GenericMessageInterchangeType { get; }
		protected override Type TypeOfInterchangeToCreate() => typeof(EDIInterchange);
		protected override string[] ApplicationCodes => new string[] { ApplicationCodeList.Codes.GenericMessageDelivery };
		protected override void AddTypeFilter(ZQuery query)
		{
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, new[] { GenericMessageInterchangeType });
			base.AddTypeFilter(query);
		}
	}
}
