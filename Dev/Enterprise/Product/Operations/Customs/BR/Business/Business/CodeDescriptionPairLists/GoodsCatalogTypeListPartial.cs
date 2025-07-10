using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogTypeList : Customs.Business.GoodsCatalogTypeList
	{
		public static ZString MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case GoodsCatalogTypeList.Codes.Import:
					return "IMPORTACAO";
				case GoodsCatalogTypeList.Codes.Export:
					return "EXPORTACAO";
				default:
					return ZString.Empty;
			}
		}

		public static ZString MapToCWCode(ZString code)
		{
			switch (code.ToUpper())
			{
				case "EXPORTACAO":
					return GoodsCatalogTypeList.Codes.Export;
				case "IMPORTACAO":
					return GoodsCatalogTypeList.Codes.Import;
				default:
					return ZString.Empty;
			}
		}
	}
}
