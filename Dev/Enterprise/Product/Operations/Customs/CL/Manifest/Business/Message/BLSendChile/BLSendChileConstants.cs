namespace Enterprise.Customs.CL.Manifest.Business
{
	public static class BLSendChileConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Status Container")]
		public static class StatusContainer
		{
			internal const string Empty = "Empty";
			internal const string LclLcl = "LclLcl";
			public const string FclFcl = "FclFcl";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Document")]
		public static class ServiceDocument
		{
			internal const string Tramp = "Tramp";
			public const string Liner = "Liner";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Type Document")]
		public static class ServiceTypeDocument
		{
			public const string Empty = "Empty";
			public const string RoRo = "RoRo";
			public const string Bb = "Bb";
			public const string LclLcl = "LclLcl";
			public const string FclFcl = "FclFcl";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Payment Type")]
		public static class PaymentType
		{
			internal const string Collect = "Collect";
			public const string Prepaid = "Prepaid";
		}
	}

	class ContainerSealParties
	{
		internal class CarrierShippingLine
		{
			internal const string Code = "CA";
			internal const string Name = "COMPANIA";
		}

		internal class ConsignorShipper
		{
			internal const string Code = "SH";
			internal const string Name = "CLIENTE-EMBARCADOR";
		}

		internal class Customs
		{
			internal const string Code = "AD";
			internal const string Name = "ADUANA";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is not a database field")]
		internal class Terminal
		{
			internal const string Code = "TO";
			internal const string Name = "TERMINAL OPERADOR";
		}
	}
}
