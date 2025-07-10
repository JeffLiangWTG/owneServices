namespace Enterprise.Customs.KR.Messaging
{
	public partial class PackageKindCodeList
	{
		public static bool IsBulk(string code)
		{
			const string StartCharOfBulkPackages = "V";
			return code.StartsWith(StartCharOfBulkPackages);
		}
	}
}
