namespace CargoWise.Services.Common
{
	public static class TransactionTypes
	{
		public static class DistanceCalculation
		{
			public const string Code = "DIS";

			public static class SubTypes
			{
				public const string GoogleProvider = "GOO";
				public const string PCMiler = "PCM";
			}
		}

		public static class BarcodeReader
		{
			public const string Code = "BRD";
		}
	}
}
