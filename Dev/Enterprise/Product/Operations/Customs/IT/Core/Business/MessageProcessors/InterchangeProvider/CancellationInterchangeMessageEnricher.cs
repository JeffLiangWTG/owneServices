using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class CancellationInterchangeMessageEnricher : IEDIInterchangeEnricher
{
	EDIInterchange IEDIInterchangeEnricher.Enrich(EDIInterchange ediInterchange)
	{
		Argument.NotNull(ediInterchange, nameof(ediInterchange));

		ediInterchange.EI_HeaderText = JsonSerializer.Serialize(new CancellationInterchangeHeaderText());
		return ediInterchange;
	}

	#region CancellationInterchangeHeaderText

	class CancellationInterchangeHeaderText
	{
		[JsonPropertyName("custom.MessageSubType")]
		public string MessageSubType => EDIMessageTypeList.Codes.Cancellation;
	}

	#endregion
}
