using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public interface IResponseDataProvider
	{
		ZBool IsImport { get; }
		ZBool HasErrors { get; }
		ZString TransactionID { get; }
		ZString Refdec { get; }
		ZString Etat { get; }
		ZString EtatECS { get; }
		ZString EtatDate { get; }
		ZString EtatHeure { get; }
		ZString Refdos { get; }
		ZBool HasLiquidation { get; }
		ZLong? Montantcautionnable { get; }

		IEnumerable<IResponseLiquidationWrapper> ResponseLiquidations { get; }

		ZBool HasArticleDAU { get; }
		IEnumerable<IResponseArticleDAUWrapper> ResponseArticleDAUs { get; }

		ZBool IsGvmsAdvice { get; }

		ZString Evenement { get; }
	}
}
