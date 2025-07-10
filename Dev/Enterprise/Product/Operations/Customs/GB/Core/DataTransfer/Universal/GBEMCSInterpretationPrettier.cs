using System;
using System.Linq;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Messaging.MessageProcessors;
using static System.FormattableString;
using static Enterprise.Customs.GB.DataTransfer.Universal.GBEMCSEventParentFinder;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class GBEMCSInterpretationPrettier
	{
		public ZString CreatePrettyRejectionInterpretationFromJson(EMCSJsonResponse jsonResponse)
		{
			var htmlInterpretation = new ZStringBuilder(MessagePrettierCss.CSS);
			HtmlTableCreator tableCreator;

			if (!string.IsNullOrEmpty(jsonResponse.Message))
			{
				tableCreator = InterpretationHelper.GetHtmlTableCreator();
				tableCreator.WriteRow("Message");
				tableCreator.WriteRow(jsonResponse.Message);
				_ = htmlInterpretation.Append($"{tableCreator.ToHtml()}</BR>");
			}

			if (!string.IsNullOrEmpty(jsonResponse.DebugMessage))
			{
				tableCreator = InterpretationHelper.GetHtmlTableCreator();
				tableCreator.WriteRow("Debug Message");
				tableCreator.WriteRow(jsonResponse.DebugMessage);
				_ = htmlInterpretation.Append($"{tableCreator.ToHtml()}</BR>");
			}

			if (!string.IsNullOrEmpty(jsonResponse.DateTime))
			{
				tableCreator = InterpretationHelper.GetHtmlTableCreator();
				tableCreator.WriteRow("Date-Time");
				tableCreator.WriteRow(jsonResponse.DateTime);
				_ = htmlInterpretation.Append($"{tableCreator.ToHtml()}</BR>");
			}

			var correlationId = !string.IsNullOrEmpty(jsonResponse.CorrelationId) ? jsonResponse.CorrelationId : jsonResponse.EmcsCorrelationId;

			if (!string.IsNullOrEmpty(correlationId))
			{
				tableCreator = InterpretationHelper.GetHtmlTableCreator();
				tableCreator.WriteRow("Correlation ID");
				tableCreator.WriteRow(correlationId);
				_ = htmlInterpretation.Append($"{tableCreator.ToHtml()}</BR>");
			}

			if (jsonResponse.Errors != null)
			{
				foreach (var error in jsonResponse.Errors)
				{
					tableCreator = InterpretationHelper.GetHtmlTableCreator();
					tableCreator.WriteRow("Error Code", error.ErrorCode);
					tableCreator.WriteRow("Error Message", error.ErrorMessage);
					tableCreator.WriteRow("Location", error.Location);
					tableCreator.WriteRow("Value", error.Value);
					_ = htmlInterpretation.Append($"{tableCreator.ToHtml()}</BR>");
				}
			}

			if (jsonResponse.ValidatorResults != null)
			{
				foreach (var result in jsonResponse.ValidatorResults)
				{
					tableCreator = InterpretationHelper.GetHtmlTableCreator();
					tableCreator.WriteRow("Error Code", (!result.ErrorCategory.IsNullOrEmpty() ? $"{result.ErrorCategory} " : string.Empty) + result.ErrorType);
					tableCreator.WriteRow("Error Message", result.ErrorReason);
					tableCreator.WriteRow("Location", result.ErrorLocation);
					tableCreator.WriteRow("Value", result.OriginalAttributeValue);
					_ = htmlInterpretation.Append($"{tableCreator.ToHtml()}</BR>");
				}
			}

			return htmlInterpretation.ToString();
		}

		public ZString CreatePrettyRejectionInterpretationFromXml(EMCSXmlResponse xmlResponse)
		{
			var htmlFriendlyError = new ZStringBuilder(MessagePrettierCss.CSS);
			var errors = xmlResponse.Document.SelectNodes(ErrorResponseErrorsXPath);

			if (errors.Count > 0)
			{
				_ = htmlFriendlyError.Append(ToH3IfNotEmpty($"EMCS Response Errors"));
				_ = htmlFriendlyError.Append(ToH4IfNotEmpty($"Summary: {xmlResponse.ErrorSummary}"));

				InterpretationHelper.CreateTableOfErrorsFromNodeList(htmlFriendlyError, errors, new[]
				{
					("Raised By", RaisedByXPath),
					("Error Number", NumberXPath),
					("Error Type", TypeXPath),
					("Error Text", TextXPath),
					("Location", LocationXPath),
					("Message", DeveloperMessageXPath),
				});
			}

			return htmlFriendlyError.ToString();
		}

		public ZString CreatePrettyPVTResponseInterpretationFromJson(string jsonRequestText, string jsonResponseText)
		{
			var request = JsonSerializer.Deserialize<PreValidateTraderRequest>(jsonRequestText, jsonOptions);
			var requestBody = request?.ExciseTraderValidationRequest?.ExciseTraderRequest;
			var traderId = requestBody?.ExciseRegistrationNumber ?? string.Empty;
			var traderType = requestBody?.EntityGroup ?? string.Empty;
			var exciseProductCodes = requestBody?.ValidateProductAuthorisationRequest?.Select(r => r.Product.ExciseProductCode) ?? Array.Empty<string>();

			var response = JsonSerializer.Deserialize<PreValidateTraderResponse>(jsonResponseText, jsonOptions);
			var responseBody = (response?.ExciseTraderValidationResponse?.ExciseTraderResponse ?? response.ExciseTraderResponse)?.FirstOrDefault();
			var responseId = responseBody?.ExciseRegistrationNumber;
			var validTrader = responseBody?.ValidTrader;
			var validProduct = responseBody?.ValidateProductAuthorisationResponse?.Valid ?? false;

			var htmlInterpretation = new ZStringBuilder(MessagePrettierCss.CSS);
			htmlInterpretation.Append(ToH3IfNotEmpty("Response to Pre-Validate Trader Query"));
			htmlInterpretation.Append(ToH4IfNotEmpty("Query:"));

			var tableCreator = InterpretationHelper.GetHtmlTableCreator();
			tableCreator.WriteRow("Trader ID", traderId);
			tableCreator.WriteRow("Type", traderType);
			tableCreator.WriteRow("Excise codes", string.Join(",", exciseProductCodes));
			htmlInterpretation.Append(ToUlIfNotEmpty(tableCreator.ToHtml()));

			htmlInterpretation.Append(ToH4IfNotEmpty("Response:"));

			if (responseId != traderId || validTrader == null)
			{
				htmlInterpretation.Append(ToUlIfNotEmpty($"<i>Unexpected response string, check message text for details</i>"));
			}
			else
			{
				var isValid = validTrader.Value && validProduct;
				htmlInterpretation.Append(ToUlIfNotEmpty($"Trader is <span style=\"color:{(isValid ? "green" : "red")}\">{(isValid ? "" : "in")}valid</span> for these products"));
			}

			return htmlInterpretation.ToString();
		}

		static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

		const string ErrorResponseErrorsXPath = "//ErrorResponse/Error";
		const string RaisedByXPath = "RaisedBy";
		const string NumberXPath = "Number";
		const string TypeXPath = "Type";
		const string TextXPath = "Text";
		const string LocationXPath = "Location";
		const string DeveloperMessageXPath = "Application/Messages/DeveloperMessage";

		ZString ToH3IfNotEmpty(ZString h3) => ToElementIfNotEmpty("h3", h3);

		ZString ToH4IfNotEmpty(ZString h4) => ToElementIfNotEmpty("h4", h4);

		ZString ToUlIfNotEmpty(ZString ul) => ToElementIfNotEmpty("ul", ul);

		ZString ToElementIfNotEmpty(ZString element, ZString value)
		{
			return !value.IsEmpty ? (ZString)Invariant($"<{element}>{value}</{element}>") : ZString.Empty;
		}
	}
}
