using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class AutomaticSignatureInterchangeMessageEnricher
	: IEDIInterchangeEnricher
{
	public AutomaticSignatureInterchangeMessageEnricher(IAutomaticSignatureExternalPassword automaticSignature, SignatureOperation signatureOperation)
	{
		this.automaticSignature = Argument.NotNull(automaticSignature, nameof(automaticSignature));
		this.signatureOperation = signatureOperation;
	}

	EDIInterchange IEDIInterchangeEnricher.Enrich(EDIInterchange ediInterchange)
	{
		Argument.NotNull(ediInterchange, nameof(ediInterchange));

		var headerText = new AutomaticSignatureMetadata()
		{
			DelegatedUser = automaticSignature.DelegateName,
			SignatureUser = automaticSignature.GP_UserID,
			Operation = signatureOperation
		};

		var settings = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

		ediInterchange.EI_HeaderText = JsonSerializer.Serialize(headerText, settings);
		return ediInterchange;
	}

	readonly IAutomaticSignatureExternalPassword automaticSignature;
	readonly SignatureOperation signatureOperation;
}
