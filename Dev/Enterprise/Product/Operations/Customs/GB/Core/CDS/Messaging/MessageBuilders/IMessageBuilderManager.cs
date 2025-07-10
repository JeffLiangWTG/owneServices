using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;

public interface IMessageBuilderManager
{
	IGbCDSMessageBuilder NewMessageBuilder(JobDeclarationMessageSendingObject objectToSend, EU.Business.ErrorCollector errorCollector, CusdecMessageFunction newAmendDelete);
}
