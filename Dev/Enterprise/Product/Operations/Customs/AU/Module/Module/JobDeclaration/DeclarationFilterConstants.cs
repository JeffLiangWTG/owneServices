namespace Enterprise.Customs.AU.Module
{
	public class DeclarationFilterConstants : Customs.Module.DeclarationFilterConstants
	{
		public const string COLSLodgementReferenceNumber = "COLS Lodgement Reference Number";
		public const string COLSLodgementReferenceNumberStatus = "COLS Lodgement Reference Number Status";
		public const string COLSEntryStatus = "COLS Entry Status";
		public const string COLSMessageStatus = "COLS Message Status";
		public const string CustomsMessageStatus = "Customs Message Status";
		public const string CustomsEntryStatus = "Customs Entry Status";
		public const string PaymentStatusText = "Payment Status";
		public const string NatureType = "Nature Type";
		public const string CustomsConsolidatedCargoStatus = "Consolidated Cargo Status";
		public const string ConsignmentRefNumber = "Consignment Reference #";
		public const string RFPNumber = "RFP Number";
		public const string RFPStatus = "RFP Status";
		public const string ExportPermitNumber = "Export Permit Number";
		public const string QuarantineProduceType = "Quarantine Produce Type";
		public const string DrawbackDate = "Drawback Date";

		public static class PaymentStatus
		{
			public const string Paid = "PAY";
			public const string NotPaid = "NOP";
		}

		public static class NatureTypes
		{
			public const string Nature10 = "N10";
			public const string Nature20 = "N20";
			public const string Nature30 = "N30";
		}

		public static class ConsolidatedCargoStatus
		{
			public const string NotClearForFilter = "NCL";
		}

		public static class COLSExtraMessageStatus
		{
			public const string Failed = "Failed";
			public const string Success = "Success";
		}
	}
}
