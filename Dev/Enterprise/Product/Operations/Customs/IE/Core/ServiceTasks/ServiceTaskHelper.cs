using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.ServiceTasks
{
	public static class ServiceTaskHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error string")]
		public static string GetCertificateMessageError() =>
			CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Ireland, PasswordTypesList.Codes.IER, PasswordStatusList.Codes.Valid)
			|| ExistsIECompanyWithValidEMCSCertificate
			? string.Empty : "There is no Certificate configured in Ireland.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error string")]
		public static string GetEMCSCertificateMessageError() => ExistsIECompanyWithValidEMCSCertificate ? string.Empty : "There is no valid EMCS Certificate configured in Ireland.";

		static bool ExistsIECompanyWithValidEMCSCertificate => CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Ireland, PasswordTypesList.Codes.IEM, PasswordStatusList.Codes.Valid);
	}
}
