namespace Enterprise.Customs.AU.Declaration.Business
{
	public partial class EXDOCComplianceStatusCodesForCusEntryNumber
	{
		public static string GetCodeFromEXDOCComplianceStatusCode(string complianceStatusCode)
		{
			string result;
			switch (complianceStatusCode)
			{
				case EXDOCComplianceStatusCodes.Codes.Cancelled:
					result = Codes.CancCancelled;
					break;
				case EXDOCComplianceStatusCodes.Codes.Completed:
					result = Codes.CompCompleted;
					break;
				case EXDOCComplianceStatusCodes.Codes.CertificateReady:
					result = Codes.CtrdCertificateReady;
					break;
				case EXDOCComplianceStatusCodes.Codes.RexEmergencyHealthCertificate:
					result = Codes.EmctEmergencyHealthCertificate;
					break;
				case EXDOCComplianceStatusCodes.Codes.RfpEmergencyHealthCertificate:
					result = Codes.EmhcEmergencyHealthCertificate;
					break;
				case EXDOCComplianceStatusCodes.Codes.Final:
					result = Codes.FinlFinal;
					break;
				case EXDOCComplianceStatusCodes.Codes.HealthCertificateReady:
					result = Codes.HcrdHealthCertificateReady;
					break;
				case EXDOCComplianceStatusCodes.Codes.Initial:
					result = Codes.InitInitial;
					break;
				case EXDOCComplianceStatusCodes.Codes.Inspected:
					result = Codes.InspInspected;
					break;
				case EXDOCComplianceStatusCodes.Codes.Order:
					result = Codes.OrdrOrder;
					break;
				case EXDOCComplianceStatusCodes.Codes.Review:
					result = Codes.ReviewInReview;
					break;
				case EXDOCComplianceStatusCodes.Codes.Suspended:
					result = Codes.SuspSuspended;
					break;
				case EXDOCComplianceStatusCodes.Codes.Withdrawn:
					result = Codes.WtdrnWithdrawn;
					break;
				default:
					result = string.Empty;
					break;
			}
			return result;
		}
	}
}
