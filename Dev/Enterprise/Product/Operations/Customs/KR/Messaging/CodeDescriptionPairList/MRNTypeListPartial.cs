namespace Enterprise.Customs.KR.Messaging
{
	public partial class MRNTypeList
	{
		public static bool IsSpecialType(string type)
		{
			return type == Codes.NewVessel
				|| type == Codes.ChangeOfQualification
				|| type == Codes.ScheduledToArrive;
		}
	}
}
