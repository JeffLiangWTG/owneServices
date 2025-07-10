namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public static class IndicatorHelper
	{
		public static bool IsSpecificCircumstanceIndicatorApplicableForReEntryIndicator(string code)
		{
			switch (code)
			{
				case EUICS2SpecificCircumstanceList.Codes.F10:
				case EUICS2SpecificCircumstanceList.Codes.F11:
				case EUICS2SpecificCircumstanceList.Codes.F12:
				case EUICS2SpecificCircumstanceList.Codes.F13:
				case EUICS2SpecificCircumstanceList.Codes.F20:
				case EUICS2SpecificCircumstanceList.Codes.F21:
				case EUICS2SpecificCircumstanceList.Codes.F27:
				case EUICS2SpecificCircumstanceList.Codes.F28:
				case EUICS2SpecificCircumstanceList.Codes.F29:
				case EUICS2SpecificCircumstanceList.Codes.F30:
				case EUICS2SpecificCircumstanceList.Codes.F42:
				case EUICS2SpecificCircumstanceList.Codes.F50:
				case EUICS2SpecificCircumstanceList.Codes.F51:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSpecificCircumstanceIndicatorApplicableForPreviousMRN(string code)
		{
			switch (code)
			{
				case EUICS2SpecificCircumstanceList.Codes.F10:
				case EUICS2SpecificCircumstanceList.Codes.F11:
				case EUICS2SpecificCircumstanceList.Codes.F12:
				case EUICS2SpecificCircumstanceList.Codes.F13:
				case EUICS2SpecificCircumstanceList.Codes.F25:
				case EUICS2SpecificCircumstanceList.Codes.F30:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSpecificCircumstanceIndicatorApplicableForManifestHeaderSupportingDocuments(string code)
		{
			switch (code)
			{
				case EUICS2SpecificCircumstanceList.Codes.F10:
				case EUICS2SpecificCircumstanceList.Codes.F11:
				case EUICS2SpecificCircumstanceList.Codes.F12:
				case EUICS2SpecificCircumstanceList.Codes.F13:
				case EUICS2SpecificCircumstanceList.Codes.F14:
				case EUICS2SpecificCircumstanceList.Codes.F20:
				case EUICS2SpecificCircumstanceList.Codes.F21:
				case EUICS2SpecificCircumstanceList.Codes.F27:
				case EUICS2SpecificCircumstanceList.Codes.F28:
				case EUICS2SpecificCircumstanceList.Codes.F29:
				case EUICS2SpecificCircumstanceList.Codes.F42:
				case EUICS2SpecificCircumstanceList.Codes.F50:
				case EUICS2SpecificCircumstanceList.Codes.F51:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSpecificCircumstanceIndicatorApplicableForBillSupportingDocuments(string code)
		{
			switch (code)
			{
				case EUICS2SpecificCircumstanceList.Codes.F10:
				case EUICS2SpecificCircumstanceList.Codes.F11:
				case EUICS2SpecificCircumstanceList.Codes.F12:
				case EUICS2SpecificCircumstanceList.Codes.F13:
				case EUICS2SpecificCircumstanceList.Codes.F14:
				case EUICS2SpecificCircumstanceList.Codes.F15:
				case EUICS2SpecificCircumstanceList.Codes.F20:
				case EUICS2SpecificCircumstanceList.Codes.F22:
				case EUICS2SpecificCircumstanceList.Codes.F26:
				case EUICS2SpecificCircumstanceList.Codes.F27:
				case EUICS2SpecificCircumstanceList.Codes.F30:
				case EUICS2SpecificCircumstanceList.Codes.F32:
				case EUICS2SpecificCircumstanceList.Codes.F43:
				case EUICS2SpecificCircumstanceList.Codes.F50:
				case EUICS2SpecificCircumstanceList.Codes.F51:
					return true;
				default:
					return false;
			}
		}

		public static bool IsSpecificCircumstanceIndicatorApplicableForPacksSupportingDocuments(string code)
		{
			switch (code)
			{
				case EUICS2SpecificCircumstanceList.Codes.F10:
				case EUICS2SpecificCircumstanceList.Codes.F11:
				case EUICS2SpecificCircumstanceList.Codes.F12:
				case EUICS2SpecificCircumstanceList.Codes.F13:
				case EUICS2SpecificCircumstanceList.Codes.F14:
				case EUICS2SpecificCircumstanceList.Codes.F15:
				case EUICS2SpecificCircumstanceList.Codes.F20:
				case EUICS2SpecificCircumstanceList.Codes.F22:
				case EUICS2SpecificCircumstanceList.Codes.F26:
				case EUICS2SpecificCircumstanceList.Codes.F27:
				case EUICS2SpecificCircumstanceList.Codes.F30:
				case EUICS2SpecificCircumstanceList.Codes.F32:
				case EUICS2SpecificCircumstanceList.Codes.F50:
				case EUICS2SpecificCircumstanceList.Codes.F51:
					return true;
				default:
					return false;
			}
		}
	}
}
