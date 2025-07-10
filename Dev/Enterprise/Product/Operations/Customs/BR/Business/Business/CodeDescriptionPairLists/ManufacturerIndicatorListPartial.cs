using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ManufacturerIndicatorList
	{
		public static string MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes._1:
					return "EXPORTADOR_IGUAL_FABRICANTE";
				case Codes._2:
				case Codes._3:
					return "EXPORTADOR_DIFERENTE_FABRICANTE";
				default:
					return string.Empty;
			}
		}
	}
}
