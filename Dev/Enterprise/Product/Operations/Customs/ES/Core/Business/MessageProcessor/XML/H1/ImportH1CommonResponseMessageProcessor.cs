using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business;

public abstract class ImportH1CommonResponseMessageProcessor<TResponse> : XMLResponseMessageProcessor<TResponse, IMessagePrettyFormatter>
	where TResponse : class, ICommonServiceSegment, IResponseCode, ICommonErrors, IMRNField
{
	public ImportH1CommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	protected override ZString AcceptedResponseCode => ZString.Empty;

	protected override ZBool IsAcceptedDeclaration(TResponse response) => (response.FunctionalErrors == null || !response.FunctionalErrors.Any())
		&& (response.XMLErrors == null || !response.XMLErrors.Any());
}
