using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public sealed class AcknowledgementResponseMessage : ResponseMessage<Risposta>
{
	public AcknowledgementResponseMessage(ZString message) : base(message)
	{
	}

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory()
		=> new RispostaOutputXmlOverridesCreationFactory();

	protected override string GetMessageStatus() => ResponseBody?.Esito?.Codice;
}
