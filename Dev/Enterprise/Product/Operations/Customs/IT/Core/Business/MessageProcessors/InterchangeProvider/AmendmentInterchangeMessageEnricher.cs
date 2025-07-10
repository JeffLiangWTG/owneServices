using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class AmendmentInterchangeMessageEnricher : IEDIInterchangeEnricher
{
	EDIInterchange IEDIInterchangeEnricher.Enrich(EDIInterchange ediInterchange)
	{
		Argument.NotNull(ediInterchange, nameof(ediInterchange));

		ediInterchange.EI_HeaderText = JsonSerializer.Serialize(new AmendmentInterchangeHeaderText());
		return ediInterchange;
	}

	#region AmendmentInterchangeHeaderText

	sealed class AmendmentInterchangeHeaderText
	{
		[JsonPropertyName("custom.MessageSubType")]
		public string MessageSubType => EDIMessageTypeList.Codes.Amendment;
	}

	#endregion
}
