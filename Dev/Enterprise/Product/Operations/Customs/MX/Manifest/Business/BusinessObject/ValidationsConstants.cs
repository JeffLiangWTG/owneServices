
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public static class ValidationsConstants
	{
		public static string MustBeLoggedInUnderMXToSendMXMessages
		{
			get { return ResString.GetMultilingualString("EA1450EF-753C-4867-B721-566104EE4A7E", "To create a message for Mexico you must be logged-in under a Mexican company."); }
		}
	}
}
