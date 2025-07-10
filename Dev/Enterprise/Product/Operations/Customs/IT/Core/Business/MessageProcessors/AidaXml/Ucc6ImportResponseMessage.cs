using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import;
using CargoWise.Types;
using IAidaXmlResponseMessage = CargoWise.Customs.IT.MessageDefinitions.IResponseMessage;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ImportResponseMessage : ResponseMessage<recuperaEsitoResponse, CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.data.Data>, IResponseMessageWithWrapper
{
	public Ucc6ImportResponseMessage(ZString message) : base(message)
	{
	}

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory() => null;

	protected override string GetMessageStatus() => ResponseBody?.recuperaEsitoReturn?.esito?.codice;

	protected override string ProcessDataStringBeforeSerialization(string dataString)
	{
		return SanitizeXmlContentDueToDateTimeTagsXsdViolation(dataString);
	}

	static string SanitizeXmlContentDueToDateTimeTagsXsdViolation(string dataString)
	{
		dataString = dataString
			.Replace("<DataOraIniElab></DataOraIniElab>", string.Empty)
			.Replace("<DataOraIniElab />", string.Empty)
			.Replace("<DataOraIniElab/>", string.Empty);

		dataString = dataString
			.Replace("<DataOraFinElab></DataOraFinElab>", string.Empty)
			.Replace("<DataOraFinElab />", string.Empty)
			.Replace("<DataOraFinElab/>", string.Empty);
		return dataString;
	}

	IAidaXmlResponseMessage IResponseMessageWithWrapper.GetResponseMessageContents() => Data;
}
