namespace Enterprise.Customs.AE.Business;

static class InterpretationStrings
{
	public const string InvalidComponentPosition = "-1";

	public static class CONTRL
	{
		public static string GetOutgoingReference(string responseType, string referenceNumber)
			=> Res.GetString("AE|CONTRL|OutgoingReference", "{0} Reference Number: {1}", responseType, referenceNumber);

		public static string GetInvalidHeaderTrailerString(string responseType)
		{
			var segmentType = responseType == ResponseTypes.Interchange ? "UNB/UNZ" : "UNH/UNT";
			return Res.GetString("AE|CONTRL|InvalidHeaderTrailer", "The {0} segment is missing or has a validation error", segmentType);
		}

		public static string GetInvalidContentString(string responseType)
		{
			return responseType == ResponseTypes.Interchange
			? Res.GetString("AE|CONTRL|InvalidInterchangeContent", "No errors in the UNB/UNZ segment, but one or more validation errors in the message within the interchange")
			: Res.GetString("AE|CONTRL|InvalidMessageContent", "No errors in the UNH/UNT segment, but one or more validation errors in the segments within the message");
		}

		public static string GetSubmissionAcceptedString(string responseType)
			=> Res.GetString("AE|CONTRL|SubmissionAccepted", "{0} was successful with no errors", responseType);

		public static string GetSyntaxErrorString(string errorCode, string errorDescription)
		{
			errorDescription = string.IsNullOrEmpty(errorDescription) ? "N/A" : errorDescription;
			return Res.GetString("AE|CONTRL|SyntaxError", "Error Code: {0} - {1}", errorCode, errorDescription);
		}

		public static string GetErrorPositionString(string dataElementPosition, string componentPosition)
		{
			return string.IsNullOrEmpty(componentPosition) || componentPosition == InvalidComponentPosition
				? Res.GetString("AE|CONTRL|ErrorDataElement", "Error occurred at data element {0}", dataElementPosition)
				: Res.GetString("AE|CONTRL|ErrorDataElementWithComponent", "Error occurred at data element {0}, component position {1}", dataElementPosition, componentPosition);
		}

		public static string GetErrorSegmentPositionString(string segmentPosition)
			=> Res.GetString("AE|CONTRL|ErrorSegmentPosition", "Error occurred in segment {0}", segmentPosition);
	}

	public static class CUSRES
	{
		public static string GetDocumentIdentifierString(string documentIdentifier)
			=> Res.GetString("AE|CUSRES|DocumentIdentifier", "Document Reference Number: {0}", documentIdentifier);

		public static string GetEntryStatusString(string entryStatus, string statusDescription)
		{
			statusDescription = string.IsNullOrEmpty(statusDescription) ? "N/A" : statusDescription;
			return Res.GetString("AE|CUSRES|EntryStatus", "Entry Status: {0} - {1}", entryStatus, statusDescription);
		}

		public static string GetInformationRequestString(string requestType)
			=> Res.GetString("AE|CUSRES|InformationRequest", "{0} requested", requestType);

		public static string GetErrorSegmentString(string segmentTag)
			=> Res.GetString("AE|CUSRES|ErrorSegment", "Error in segment {0}", segmentTag);
	}

	public static class ResponseTypes
	{
		public const string Message = "Message";
		public const string Interchange = "Interchange";
	}
}
