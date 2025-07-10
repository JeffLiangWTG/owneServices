using System.Collections.Generic;

namespace Enterprise.xTMessaging.Shared
{
	public static class XtEvents
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static string GetXtEventsReaderErrorDescription(int errorCode)
		{
			var errorDictionary = new Dictionary<int, string>
			{
				{ 1, "Object has invalid state" },
				{ 2, "Object not found" },
				{ 3, "Database conflict" },
				{ 4, "Object is still referenced by other objects" },
				{ 5, "Referenced object(s) does not exist" },
				{ 6, "Internal unexpected error" },
				{ 7, "The operation is not supported by this type" },
				{ 8, "Generic system error" },
				{ 9, "Generic contract error" },
				{ 10, "Application error" },
				{ 11, "Destination error" },
				{ 12, "The sequence is locked or contains errors" },
				{ 13, "Msg has wrong state" },
				{ 14, "The processing lock is set" },
				{ 15, "The message is already added" },
				{ 16, "Sequence error is unlocked" },
				{ 18, "The message is assigned to a sequence" },
				{ 19, "The message is not last in the sequence" },
				{ 22, "Invalid file parameter" },
				{ 23, "Bad parameter in call" },
				{ 24, "Permission denied" },
				{ 25, "Error parsing message" },
				{ 26, "Error routing message" },
				{ 27, "Party/Organization error" },
				{ 28, "Configuration error" },
				{ 29, "Recursion error" },
				{ 30, "Header error" },
				{ 31, "External post parsing error" },
				{ 32, "External pre parsing error" },
				{ 34, "Unknown header" },
				{ 35, "Timeout" },
				{ 36, "Bad or unknown Message type" },
				{ 50, "Not implemented" }
			};

			return errorDictionary.ContainsKey(errorCode) ? errorDictionary[errorCode] : "Unknown error";
		}
	}
}
