using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISACLiabilityQuestionProvider
	{
		ZDecimal GoodsValueInLocalCurrency { get; }
		ZString GoodsDescription { get; }
		ZPropertyInfo SACFlagInfo { get; }
	}
}
