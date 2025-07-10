using ResString = Enterprise.Customs.CL.Manifest.Business.ResString;

namespace Enterprise.Customs.CL.Manifest
{
	public static class ValidationsConstants
	{
		public static string MustBeLoggedInUnderCLToSendCLMessages
		{
			get { return ResString.GetMultilingualString("F5CF83B7-5572-444F-B6D5-1B93691BB712", "To create a message for Chile you must be logged-in under a Chilean company."); }
		}
	}
}
