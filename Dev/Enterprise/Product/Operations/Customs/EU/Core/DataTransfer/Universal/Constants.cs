namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class Constants
	{
		public const string UnknownTaxType = "UNK";
		public static class OldEUAddInfo
		{
			public static class Fields
			{
				public const string OtherDeferNumber = "OtherDeferNumber";
				public const string OSAirTransportLoad = "OSAirTransportLoad";
			}
		}

		public static class OldCusAddInfoTypes
		{
			public static class Tax
			{
				public const string TaxAddInfoTypeCodeGTX = "GTX";
				public static class Fields
				{
					public const string Amount = "Amount"; // An explanation that must be at least 15 characters long - off piss
					public const string BaseAmount = "BaseAmount";
					public const string BaseQuantity = "BaseQuantity";
					public const string MethodOfPayment = "MethodOfPayment";
					public const string Tty = "Type";  // Tty is not a typo. An explanation that must be at least 15 characters long - piss off
					public const string RateOverride = "RateOverride";
					public const string RateSuspension = "RateSuspension";
					public const string RateDuty = "RateDuty";
				}
			}

			public static class SupportingDocument
			{
				public const string Type = "GSD";
				public static class Fields
				{
					public const string Actions = "Actions";
					public const string Availability = "Availability";
					public const string Part = "Part";
					public const string Qty = "Qty";
					public const string Reason = "Reason";
					public const string Reference = "Reference";
					public const string TypeCode = "TypeCode";
				}
			}

			public static class AdditionalInfo
			{
				public const string Type = "GAI";
				public static class Fields
				{
					public const string Description = "Description";
					public const string NctsExportFromCountry = "NctsExportFromCountry";
					public const string NctsExportFromEC = "NctsExportFromEC";
					public const string TypeCode = "TypeCode";
				}
			}

			public static class PreviousDocument
			{
				public const string Type = "GPD";
				public static class Fields
				{
					public const string Class = "Class";
					public const string DateOfIssue = "DateOfIssue";
					public const string MoreInfo = "MoreInfo";
					public const string Reference = "Reference";
					public const string TypeCode = "TypeCode";
				}
			}
		}

		public static class AddInfoKeys
		{
			public static class InvoiceLine
			{
				public const string EstimatedDutyBreakdown = "EstimatedDutyBreakdown";
				public const string EstimatedVATBreakdown = "EstimatedVATBreakdown";
				public const string EstimatedOtherTaxesBreakdown = "EstimatedOtherTaxesBreakdown";
			}

			public static class EntryInstruction
			{
				public const string TotalInnerPackages = "TotalInnerPackages";
				public const string CustomsValueInformationLink = "CustomsValueInformationLink";
			}
		}

		public static class CustomsValueTypes
		{
			public const string Relationship = "Relationship";
			public const string PriceInfluence = "PriceInfluence";
			public const string RelationDetails = "RelationDetails";
			public const string Restrictions = "Restrictions";
			public const string DecisionNumber = "DecisionNumber";
			public const string Consideration = "Consideration";
			public const string RoyaltiesLicence = "RoyaltiesLicence";
			public const string Resale = "Resale";
			public const string RestrictionConsiderationDetails = "RestrictionConsiderationDetails";
		}
	}
}
