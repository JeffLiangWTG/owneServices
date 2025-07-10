namespace Enterprise.Customs.BR.Manifest.Business
{
	public static class MercanteConstants
	{
		internal const string NotApplicable = "NA";
		internal const string Prepaid = "P";
		internal const string Collect = "C";
		internal const string CargoClassTransitChar = "P";
		internal const string CargoClassImportChar = "I";

		public static class LoadType
		{
			internal const int Containerised = 1;
			internal const int BreakBulk = 2;
			internal const int Bulk = 3;
		}
	}
}
