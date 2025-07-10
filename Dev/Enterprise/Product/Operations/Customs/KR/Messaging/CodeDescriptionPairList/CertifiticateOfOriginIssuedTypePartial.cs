using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class CertifiticateOfOriginIssuedTypeList
	{
		public static string GetCertifiticateOfOriginIssuingAgencyType(string certifiticateOfOriginIssuingAgencyType)
		{
			var result = ZString.Empty;
			switch (certifiticateOfOriginIssuingAgencyType)
			{
				case Codes._0:
					result = "1";
					break;
				case Codes._1:
					result = "2";
					break;
				case Codes._2:
				case Codes._3:
				case Codes._4:
					result = "9";
					break;
			}
			return result;
		}

		public static string GetCertifiticateOfOriginIssuerType(string certifiticateOfOriginIssuerType)
		{
			var result = ZString.Empty;
			switch (certifiticateOfOriginIssuerType)
			{
				case Codes._0:
				case Codes._1:
					result = "1";
					break;
				case Codes._2:
					result = "2";
					break;
				case Codes._3:
					result = "3";
					break;
				case Codes._4:
					result = "4";
					break;
			}
			return result;
		}
	}
}
