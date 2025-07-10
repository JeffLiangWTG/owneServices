using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public static class MessageHostedServiceRequirement
	{
		public static string CheckARCompanyHasCertificate()
		{
			var companyHasCertificate = CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Argentina, PasswordTypesList.Codes.ARB, PasswordStatusList.Codes.Valid);
			return companyHasCertificate ? string.Empty : (NoResString)"There is no Valid Certificate in Argentina.";
		}
	}
}
