using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public static class EInvoicingKoreaSouthConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public const string KRElectronicInvoice = "KR Electronic Invoice";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public const string KRElectronicExemptInvoice = "KR Electronic Exempt Invoice";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public const string ElectronicInvoiceMenuPath = "Electronic Invoice";

		public static string[] AllowedAmendmentStatusCodesWhenAmendWithInvoice => new string[] { "01", "02", "05" };

		public static string[] SuggestedAmendmentStatusCodesWhenReverseInvoice => new string[] { "03", "04", "06" };

		public static CodeDescriptionPairList AmendStatusList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("01", (NoResString)"기재사항 착오.정정");
				result.AddPair("02", (NoResString)"공급금액 변동");
				result.AddPair("03", (NoResString)"제화의 환입");
				result.AddPair("04", (NoResString)"계약의 해제");
				result.AddPair("05", (NoResString)"내국신용장등 사후개설");
				result.AddPair("06", (NoResString)"착오에 의한 이중발급");
				return result;
			}
		}

		public static class InvoiceTypeList
		{
			public static string OriginalInvoice => (NoResString)"Original Invoice";
			public static string Amendment => (NoResString)"Amendment";
		}

		public static class DataElementList
		{
			public static string InvoiceDocumentHeaderDescriptionLine1 => (NoResString)"Invoice Document Header Description Line 1";
			public static string InvoiceDocumentHeaderDescriptionLine2 => (NoResString)"Invoice Document Header Description Line 2";
			public static string InvoiceDocumentHeaderDescriptionLine3 => (NoResString)"Invoice Document Header Description Line 3";
		}

		public static class DataContext
		{
			public const string KoreaStatusCode = "EINV_KoreaStatusCode";
			public const string KoreaValidationDocumentStatus = "EINV_KoreaDocumentStatus";
			public const string KoreaDocTaxInvoice = "EINV_DocTaxInvoice";
			public const string KoreaDocTaxInvoiceCount = "EINV_DocTaxInvoiceCount";
			public const string KoreaReceiptID = "EINV_ReceiptID";
		}

		public static class StatusCodes
		{
			public const string SubmitFail = "SubmitFail";
			public const string SubmitSuccess = "SubmitSuccess";
			public const string QueryFail = "QueryFail";
			public const string QueryProcessing = "QueryProcessing";
			public const string QuerySuccess = "QuerySuccess";
		}
	}
}
