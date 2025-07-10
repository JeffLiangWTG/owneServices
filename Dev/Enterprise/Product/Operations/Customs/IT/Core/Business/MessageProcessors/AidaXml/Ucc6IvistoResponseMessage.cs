using System.Text.RegularExpressions;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6IvistoResponseMessage : ResponseMessage<Risposta, CargoWise.Customs.IT.MessageDefinitions.Declaration.Export.Ivisto.CC599C.Cc599CType>
{
	public Ucc6IvistoResponseMessage(ZString message) : base(message)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant tag")]
	public bool HasIvistoNotAvailableTag()
	{
		const string IvistoNotAvailableTagPattern = @"Ivisto\s+non\s+disponibile";
		var data = (ResponseBody as IResponseDataProvider)?.DataString;

		return data != null
			&& Regex.IsMatch(data, IvistoNotAvailableTagPattern, RegexOptions.IgnoreCase);
	}

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory() => null;

	protected override string GetMessageStatus() => ResponseBody?.Esito?.Codice;

	protected override string ProcessDataStringBeforeSerialization(string dataString)
	{
		var result = base.ProcessDataStringBeforeSerialization(dataString);
		return SanitizeXmlContentDueToMismatchBetweenXsdAndProductionFiles(result);
	}

	static string SanitizeXmlContentDueToMismatchBetweenXsdAndProductionFiles(string result)
	{
		return result.Replace("ie:CC599C", "ie:CC599");
	}
}
