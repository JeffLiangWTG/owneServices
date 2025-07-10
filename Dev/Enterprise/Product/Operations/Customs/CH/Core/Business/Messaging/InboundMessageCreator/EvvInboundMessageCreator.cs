using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public class EvvInboundMessageCreator : BaseInboundMessageCreator
{
	public EvvInboundMessageCreator(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetMessageSubTypeFromResponse(string xmlResponse) => MessageSchemaDecider.GetSpecificMessageAnalyzer<IEvvResponseAnalyzer>(xmlResponse)?.GetMessageSubType();
}
