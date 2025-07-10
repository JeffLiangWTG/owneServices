using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.DataTransfer
{
	public static class Constants
	{
		public static class AddInfoKeys
		{
			public static class InvoiceLine
			{
				public const string ComplementaryDescriptionExport = "ComplementaryDescriptionExport"; // No need to be translated
			}

			public static class AddInfoGroupTypeCodes
			{
				public const string ComplementaryDescriptionExportType = "CDE"; // No need to be translated
				public const string SuspensionDrawback = CusSupportingInfoTypeList.Codes.SuspensionDrawback;
				public const string SuspensionDrawbackInvoice = CusSupportingInfoTypeList.Codes.SuspensionDrawbackInvoice;
				public const string SuspensionDrawbackImportEntryDocument = CusSupportingInfoTypeList.Codes.SuspensionDrawbackImportEntryDocument;
			}

			public static class EntryInstruction
			{
				public const string UCRNumber = "UCRNumber";
				public const string IsUCROverridden = "IsUCROverridden";
			}

			public static class SuspensionDrawback
			{
				public const string CNPJBeneficiary = "CNPJBeneficiary";
				public const string CANumber = "CANumber";
				public const string CALineItemNumber = "CALineItemNumber";
				public const string QuantityUsed = "QuantityUsed";
				public const string CATypeOfConcessionAct = "CATypeOfConcessionAct";
				public const string TariffOfTheCAImportItem = "TariffOfTheCAImportItem";
				public const string ForeignExchangeHedgedVMLE = "ForeignExchangeHedgedVMLE";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
			public static class SuspensionDrawbackInvoice
			{
				public const string InvoiceNumber = "InvoiceNumber";
				public const string Quantity = "Quantity";
				public const string TradingCurrencyValue = "TradingCurrencyValue";
				public const string Date = "Date";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
			public static class SuspensionDrawbackImportEntryDocument
			{
				public const string ImportEntry = "ImportEntry";
				public const string Quantity = "Quantity";
				public const string Value = "Value";
				public const string Category = "Category";
				public const string EntryLine = "EntryLine";
			}
		}

		public static class JobComInvLineRefsType
		{
			public static class Codes
			{
				public const string LPCO = "LPC";
				public const string TariffDetach = "TRD";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
			public static class Descriptions
			{
				public const string LPCO = "LPCO";
				public const string TariffDetach = "Tariff Detach";
			}
		}
	}
}
