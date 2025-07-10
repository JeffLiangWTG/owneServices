using System;
using System.Text.RegularExpressions;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public sealed class IrildesResponseMessage : ResponseMessage<Risposta, CargoWise.Customs.IT.MessageDefinitions.NCTS.Departure.Irildes.CC045C.Cc045CType>
{
	public IrildesResponseMessage(ZString message) : base(message)
	{
		hasIrildesNotAvailableTagLazy = new Lazy<bool>(GetHasIrildesNotAvailableTag);
	}

	internal bool HasIrildesNotAvailableTag => hasIrildesNotAvailableTagLazy.Value;

	protected override string GetMessageStatus()
		=> ResponseBody?.Esito?.Codice;

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory()
		=> null;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant tag pattern")]
	bool GetHasIrildesNotAvailableTag()
	{
		const string IrildesNotAvailableTagPattern = @"Irildes\s+non\s+disponibile";
		var data = (ResponseBody as IResponseDataProvider)?.DataString;

		return data is not null
			&& Regex.IsMatch(data, IrildesNotAvailableTagPattern, RegexOptions.IgnoreCase);
	}

	readonly Lazy<bool> hasIrildesNotAvailableTagLazy;
}
