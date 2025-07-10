using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Common
{
	public static class SimplifiedFrontierHelper
	{
		public static bool IsSimplifiedFrontierSubstyle(ZString subStyle)
		{
			return subStyle == GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsArrived
				|| subStyle == GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSfdGoodsNotArrived
				|| subStyle == GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.TransitSfdGoodsArrived
				|| subStyle == GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived;
		}
	}
}
