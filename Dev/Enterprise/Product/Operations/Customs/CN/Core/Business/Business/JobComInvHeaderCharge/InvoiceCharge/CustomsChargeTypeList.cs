using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed")]
	public class CustomsChargeTypeList : Common.CustomsChargeTypeList
	{
		public CustomsChargeTypeList() : base()
		{
			AddPair(Codes.Royalty, Descriptions.Royalty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Codes : Common.CustomsChargeTypeList.Codes
		{
			public const string Royalty = "RYT";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Descriptions : Common.CustomsChargeTypeList.Descriptions
		{
			public static MultilingualString Royalty => ResString.GetMultilingualString("CustomsChargeTypeList|Royalty", "Royalty");
		}
	}
}
