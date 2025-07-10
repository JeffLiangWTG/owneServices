using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;
using Enterprise.Customs.IE.PBN.Business;

namespace Enterprise.Customs.IE.PBN.Messaging;

public class ROSErrorProvider
{
	public ROSErrorProvider(ROSErrorDefinition jsonObject)
	{
		this.jsonObject = jsonObject;
	}

	readonly ROSErrorDefinition jsonObject;
	public IReadOnlyCollection<PBNValidationErrorsProvider> ValidationErrors => validationErrors ??= jsonObject.ValidationErrors.Select(e => new PBNValidationErrorsProvider(e)).ToArray();
	IReadOnlyCollection<PBNValidationErrorsProvider> validationErrors;

	public static bool TryConvert(string messageText, out ROSErrorDefinition rosError, bool isRetry = false)
	{
		rosError = null;

		if ((messageText != null)
			&& messageText.Contains(ROSErrorDefinition.JsonPropertyValidationErrors))
		{
			try
			{
				if (JsonSerializer.Deserialize<ROSErrorDefinition>(messageText) is ROSErrorDefinition rOSErrorDefinition && rOSErrorDefinition.ValidationErrors?.Count == 1)
				{
					rosError = rOSErrorDefinition;
				}
			}
			catch
			{
				return !isRetry && TryConvert(UniversalInterchangeXMLHelper.TryGetReasonNodeFromUniversalInterchangeTypeXML(messageText), out rosError, true);
			}
		}
		return rosError != null;
	}
}
