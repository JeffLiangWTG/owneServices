using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportGoodsApplicationTypeList
	{
		public static string MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes.Consumption:
					return "CONSUMO";
				case Codes.IncorporationAssets:
					return "INCORPORACAO_ATIVO_FIXO";
				case Codes.Industrialization:
					return "INDUSTRIALIZACAO";
				case Codes.Resale:
					return "REVENDA";
				case Codes.Other:
					return "OUTRA";
				default:
					return string.Empty;
			}
		}
	}
}
