using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;
sealed class EdecBordereauInboundMessageCreator : BaseInboundMessageCreator
{
	public EdecBordereauInboundMessageCreator(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetMessageSubTypeFromResponse(string xmlResponse) => MessageSchemaDecider.GetSpecificMessageAnalyzer<IEdecBordereauResponseAnalyzer>(xmlResponse)?.GetMessageSubType();

	public override bool RemoveSoapEnvelope => true;
}
