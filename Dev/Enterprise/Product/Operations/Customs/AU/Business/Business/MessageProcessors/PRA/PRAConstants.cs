namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class PRAConstants
	{
		public const int ConsignorNameMaxLength = 35;

		public const string Cancellation = "Cancellation ";
		public const string Submission = "Submission ";

		public const string MessageAcknowledged = "ACK";
		public const string MessageRejected = "REJ";

		public const string VictoriaInternationalContainerTerminal = "VICTM";

		public static class WATCommodityNotAllowed
		{
			public const string OutOfGauge = "OOG";
		}

		public static class ContainerGrossWeightVerificationTypes
		{
			public static class Codes
			{
				public const string Method1Container = "SM1";
				public const string Method2Packages = "SM2";
				public const string WeightAtTerminal = "WAT";
			}
		}
	}
}
