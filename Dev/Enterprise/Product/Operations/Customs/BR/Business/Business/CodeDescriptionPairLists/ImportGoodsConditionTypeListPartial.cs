using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportGoodsConditionTypeList
	{
		public static string MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes.New:
					return "NOVA";
				case Codes.Used:
					return "USADA";
				default:
					return string.Empty;
			}
		}
	}
}
