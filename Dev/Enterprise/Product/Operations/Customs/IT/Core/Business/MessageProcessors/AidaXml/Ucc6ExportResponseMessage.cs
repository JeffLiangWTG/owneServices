using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import;
using CargoWise.Types;
using IAidaXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ExportResponseMessage : ResponseMessage<recuperaEsitoResponse, CargoWise.Customs.IT.MessageDefinitions.Declaration.Export.data.Data>, IResponseMessageWithWrapper
{
	public Ucc6ExportResponseMessage(ZString message) : base(message)
	{
	}

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory() => null;

	protected override string GetMessageStatus() => ResponseBody?.recuperaEsitoReturn?.esito?.codice;

	IAidaXmlResponseMessage IResponseMessageWithWrapper.GetResponseMessageContents() => Data;
}
