using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class EventIdList
	{
		public static string GetCustomsEventValue(ZString eventId)
		{
			switch (eventId)
			{
				case Codes.DuexHistoric:
					return "duex-historico";
				case Codes.ProductCatalog:
					return "catp-prod-desativado";
				case Codes.CctReleasedCargo:
					return "cctr-carga-liberada";
				case Codes.CctBlockedCargo:
					return "cctr-carga-bloqueio";
				case Codes.CctRedChannel:
					return "cctr-canal-vermelho";
				case Codes.LpcoStatusChange:
					return "talp-altsit-lpco-anu";
				case Codes.LpcoExigencyInclusion:
					return "talp-inclusao-exig";
				case Codes.LpcoExigencyCancelation:
					return "talp-cancela-exig";
				case Codes.DuimpDiagnosis:
					return "dimp-diag-import";
				case Codes.DuimpRegister:
					return "dimp-registro-import";
				case Codes.DuimpStatus:
					return "dimp-situacao-import";
				default:
					return string.Empty;
			}
		}
	}
}
