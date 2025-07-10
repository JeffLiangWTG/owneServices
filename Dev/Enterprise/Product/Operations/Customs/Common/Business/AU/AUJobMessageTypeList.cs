using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU
{
	public class AUJobMessageTypeList : Shared.SharedJobMessageTypeList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Codes : Shared.SharedJobMessageTypeList.Codes
		{
			public const string Quarantine = "AQS";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Descriptions : Shared.SharedJobMessageTypeList.Descriptions
		{
			public static MultilingualString Quarantine { get { return ResString.GetMultilingualString("AUJobMessageTypeList|Quarantine", "Request for Permit"); } }
		}

		public AUJobMessageTypeList()
		{
			RemoveCode(Codes.Refund);
			AddPair(Codes.Quarantine, Descriptions.Quarantine);
			Sort();
		}
	}
}
