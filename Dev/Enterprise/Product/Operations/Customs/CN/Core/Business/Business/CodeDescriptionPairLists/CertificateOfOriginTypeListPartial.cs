using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public partial class CertificateOfOriginTypeList
	{
		public static bool IsSmallAmountGoods(ZString cooType)
		{
			return cooType == Codes.SmallAmountGoods;
		}
	}
}
