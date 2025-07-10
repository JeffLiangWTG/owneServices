namespace Enterprise.Customs.FR.Business
{
	public partial class DeltaGFallbackStatusList
	{
		public static bool IsStatusInFallbackAndNotRegularised(string status)
		{
			return status == Codes.PDS || status == Codes.PPS || status == Codes.PPW;
		}
	}
}
