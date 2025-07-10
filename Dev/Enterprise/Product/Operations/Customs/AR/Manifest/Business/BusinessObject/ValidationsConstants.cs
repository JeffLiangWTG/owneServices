
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public static class ValidationsConstants
	{
		public static string MustBeLoggedInUnderARToSendARMessages
		{
			get { return ResString.GetMultilingualString("96BECABF-D350-4727-B894-85CA820295EF", "To create a message for Argentina you must be logged-in under a Argentinian company."); }
		}
	}
}
