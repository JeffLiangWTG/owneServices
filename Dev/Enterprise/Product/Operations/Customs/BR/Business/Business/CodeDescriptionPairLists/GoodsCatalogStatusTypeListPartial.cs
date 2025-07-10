using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class GoodsCatalogStatusTypeList
	{
		public static ZString MapToCWCode(ZString code)
		{
			switch (code.ToUpper())
			{
				case "ATIVADO":
					return GoodsCatalogStatusTypeList.Codes.Active;
				case "DESATIVADO":
					return GoodsCatalogStatusTypeList.Codes.Inactive;
				case "RASCUNHO":
					return GoodsCatalogStatusTypeList.Codes.Draft;
				default:
					return string.Empty;
			}
		}
	}
}
