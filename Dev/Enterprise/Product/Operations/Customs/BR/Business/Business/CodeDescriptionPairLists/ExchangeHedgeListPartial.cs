using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ExchangeHedgeList
	{
		public static string MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes._1:
					return "ATE_180_DIAS";
				case Codes._2:
					return "DE_181_ATE_360";
				case Codes._3:
					return "ACIMA_360";
				case Codes._4:
					return "SEM_COBERTURA";
				default:
					return string.Empty;
			}
		}
	}
}
