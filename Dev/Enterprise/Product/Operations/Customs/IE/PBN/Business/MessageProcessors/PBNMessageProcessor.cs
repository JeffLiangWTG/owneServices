using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;
using EDIMessage = Enterprise.Customs.IE.Business.EDIMessage;

namespace Enterprise.Customs.IE.PBN.Business;

public abstract class PBNMessageProcessor<TDataProvider> : MessageAttacheeMessageProcessor<PBNInboundEDIMessage, TDataProvider>
{
	protected PBNMessageProcessor(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType) { }

	protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsPBN;

	protected override EDIMessage FindOriginalOutgoingMessageCore(BusinessObjectFactory factory, EDIMessage incomingMessage)
	{
		var interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
		interchangeQuery.AddToFilter(incomingMessage.Interchange.BuildOutgoingInterchangeQueryMatchingSessionGUID());
		var messageQuery = new ZDBOnlyQuery(typeof(BaseEDIMessage));
		messageQuery.AddSubQuery(EDIMessageSchema.EM_EI, interchangeQuery, JoinCondition.And);
		messageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Ascending;
		return factory.LoadTop1<BaseEDIMessage>(messageQuery) as EDIMessage;
	}
}
