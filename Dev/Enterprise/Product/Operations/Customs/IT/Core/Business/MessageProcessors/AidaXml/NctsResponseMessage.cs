using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import;
using CargoWise.Types;
using IAidaXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

public sealed class NctsResponseMessage : ResponseMessage<recuperaEsitoResponse, CargoWise.Customs.IT.MessageDefinitions.NCTS.Departure.data.Data>, IResponseMessageWithWrapper
{
	public NctsResponseMessage(ZString message) : base(message)
	{
	}

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory() => null;

	protected override string GetMessageStatus() => ResponseBody?.recuperaEsitoReturn?.esito?.codice;

	IAidaXmlResponseMessage IResponseMessageWithWrapper.GetResponseMessageContents() => Data;
}
