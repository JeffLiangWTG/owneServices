using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry
{
	public static class DocumentSigningRegistryConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static class DocumentAllowedForSigningType
		{
			public const string Yes = "Yes";
			public const string No = "No";
			public const string All = "All";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static class DocumentAllowedForSigningName
		{
			public const string DocBuilderInvoice = "DocBuilder Invoice";
			public const string PeriodicInvoice = "Periodic Invoice";
			public const string PeriodicInvoiceAndInvoiceDetail = "Periodic Invoice && Invoice Detail";
		}

		public static class SignatureImagePositioningAnchors
		{
			public const string BottomLeft = "BL";
			public const string TopLeft = "TL";
			public const string BottomRight = "BR";
			public const string TopRight = "TR";
		}

		public static class PdfSigningOptionCodes
		{
			public const string EmudhraV1 = "EMD";
			public const string DigitalSign = "DGS";
			public const string Placeholder = "PHD";
		}

		public class PdfSigningOptionList : CodeDescriptionPairList
		{
			public PdfSigningOptionList()
			{
				Add(EmudhraV1);
				Add(DigitalSign);
			}

			public static CodeDescriptionPair EmudhraV1 =>
				new CodeDescriptionPair(PdfSigningOptionCodes.EmudhraV1, (NoResString)"Emudhra V1"); // no need to be localizable
			public static CodeDescriptionPair DigitalSign =>
				new CodeDescriptionPair(PdfSigningOptionCodes.DigitalSign, (NoResString)"DigitalSign"); // no need to be localizable
		}
	}
}
