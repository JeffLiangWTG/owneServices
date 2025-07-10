using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

interface IInboundInterchangeProcessingStrategy
{
	void ProcessInterchange(EDIInterchange interchange);
}
