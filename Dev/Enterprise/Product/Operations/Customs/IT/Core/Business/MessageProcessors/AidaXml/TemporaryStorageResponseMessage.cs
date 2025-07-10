using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import;
using CargoWise.Types;
using IAidaXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.ITemporaryStorageResponseMessage;

namespace Enterprise.Customs.IT.Business;

public sealed class TemporaryStorageResponseMessage : ResponseMessage<recuperaEsitoResponse, CargoWise.Customs.IT.MessageDefinitions.TemporaryStorage.TEMPORARY_STORAGE_OUTPUT.TcEsitoG4>, ITemporaryStorageResponseMessageWithWrapper
{
	public TemporaryStorageResponseMessage(ZString message) : base(message)
	{
	}

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory() => null;

	protected override string GetMessageStatus() => ResponseBody?.recuperaEsitoReturn?.esito?.codice;

	public IAidaXmlResponseMessage GetResponseMessageContents() => Data;
}
