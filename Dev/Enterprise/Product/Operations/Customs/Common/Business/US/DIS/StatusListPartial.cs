namespace Enterprise.Customs.Common.US.DIS
{
	partial class StatusList
	{
		public static bool HasBeenLodgedAtCustoms(string code)
		{
			return code != Codes.AOS
				&& code != Codes.CWS
				&& code != Codes.EOS
				&& !string.IsNullOrEmpty(code);
		}

		public static bool IsWithdrawn(string code)
		{
			return code == Codes.CWS;
		}

		public static bool IsDocumentIDSentToCustoms(string code)
		{
			return IsWaitingForResponse(code)
				|| HasBeenLodgedAtCustoms(code)
				|| IsWithdrawn(code);
		}

		public static bool IsWaitingForResponse(string code)
		{
			return code == Codes.AOS
				|| code == Codes.ARS
				|| code == Codes.AWS;
		}

		public static string GetCodeByDocumentReviewStatus(string documentReviewStatus)
		{
			return documentReviewStatus switch
			{
				"ACCEPTED" => Codes.ERA,
				"REJECTED" => Codes.ERV,
				"UNDER_REVIEW" => Codes.ERR,
				_ => ""
			};
		}
	}
}
