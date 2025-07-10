using CargoWise.Customs.CH.MessageContracts.MessageProviders;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public class EbdInboundMessageCreator : BaseInboundMessageCreator
{
	public EbdInboundMessageCreator(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString GetMessageSubTypeFromResponse(string responseText) => MessageSchemaDecider.GetSpecificMessageAnalyzer<IDocumentImportResponseAnalyzer>(responseText)?.GetMessageSubType();

	public override bool RemoveSoapEnvelope => true;
}
