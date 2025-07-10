using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public interface IResponseArticleDAUWrapper
	{
		ZShort numart { get; }
		ZDecimal? Valstat { get; }
		ZDecimal? Valdou { get; }
		ZDecimal? Asstva { get; }
	}
}
