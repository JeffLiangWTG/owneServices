using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public static class GlbExternalPasswordHelper
	{
		#region Staff has Certificate

		public static string CheckAnyStaffHasCCTCertificate()
		{
			var staffHasCertificate = CertificateRequirementChecker.ExistsStaffWithCertificate(PasswordTypesList.Codes.CCT, PasswordStatusList.Codes.Valid);
			return staffHasCertificate ? string.Empty : (NoResString)"There is no Certificate configured in Brazil.";
		}

		#endregion
	}
}
