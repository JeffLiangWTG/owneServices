using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ProcessRelatedTypeList
	{
		public static string MapToCustomsCode(ZString code)
		{
			switch (code)
			{
				case Codes.ADM:
					return "1";
				case Codes.JUD:
					return "2";
				case Codes.PRE:
					return "3";
				case Codes.EJD:
					return "4";
				default:
					return string.Empty;
			}
		}
	}
}
