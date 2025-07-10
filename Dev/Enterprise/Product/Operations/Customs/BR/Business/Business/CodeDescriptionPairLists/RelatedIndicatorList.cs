using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class RelatedIndicatorList : MasterFiles.Business.Customs.BR.RelatedIndicatorList
	{
		public static string MapToCustomsCodeISW(ZString code)
		{
			switch (code)
			{
				case Codes.NoBuyerSellerRelation:
					return "1";
				case Codes.BuyerSellerRelationNoInfluence:
					return "2";
				case Codes.BuyerSellerRelationWithInfluence:
					return "3";
				default:
					return string.Empty;
			}
		}

		public static string MapToCustomsCodeDuimp(string type)
		{
			switch (type)
			{
				case Codes.NoBuyerSellerRelation:
					return "NAO_HA_VINCULACAO";
				case Codes.BuyerSellerRelationNoInfluence:
					return "VINCULACAO_SEM_INFLUENCIA_PRECO";
				case Codes.BuyerSellerRelationWithInfluence:
					return "VINCULACAO_COM_INFLUENCIA_PRECO";
				default:
					return null;
			}
		}
	}
}
