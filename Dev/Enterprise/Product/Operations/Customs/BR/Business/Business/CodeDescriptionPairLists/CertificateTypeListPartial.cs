using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class CertificateTypeList
	{
		public static string MapToCustomsCode(ZString eventId)
		{
			switch (eventId)
			{
				case Codes.CCPTC:
					return "2";
				case Codes.CCROM:
					return "3";
				default:
					return string.Empty;
			}
		}
	}
}
