using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiesta_lista_documenti_dichiarazione;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class ElectronicFolderResponseMessage : ResponseMessage<Risposta, RichiestaDocumentiDichiarazione>
{
	public ElectronicFolderResponseMessage(ZString message) : base(message)
	{
	}

	public ElectronicFolderResponseMessageWrapper GetResponseWrapper() => new(Data);

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory()
		=> new RispostaOutputXmlOverridesCreationFactory();

	protected override string GetMessageStatus() => ResponseBody?.Esito?.Codice;
}
