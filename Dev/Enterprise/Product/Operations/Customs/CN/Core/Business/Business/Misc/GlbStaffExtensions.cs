using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class GlbStaffExtensions
	{
		static GenRegCertAccredMaintList GetValidCertificate(this GlbStaff glbStaff, string type, ZDateTime checkDate)
		{
			var certificate = glbStaff?.Certificates?.Find(
				cert =>
					cert.XZ_RN_NKCountryOfIssuance == Core.Constants.CountryCodes.China
					&& cert.XZ_Type == type
					&& (cert.XZ_ExpiryOrDueDate >= checkDate || cert.XZ_ExpiryOrDueDate.IsEmpty)
			)
			.OrderBy(x => x.XZ_IssueDate)
			.FirstOrDefault();

			return certificate;
		}

		public static ZString GetNameOnCertificate(this GlbStaff glbStaff, string type, ZDateTime checkDate, string fallback = "")
		{
			var result = glbStaff.GetValidCertificate(type, checkDate)?.XZ_Comment;
			if (!result.HasValue || result.Value.IsEmpty)
			{
				result = fallback;
			}

			return result.Value;
		}

		public static ZString GetCertificationNumber(this GlbStaff glbStaff, string type, ZDateTime checkDate)
		{
			return glbStaff.GetValidCertificate(type, checkDate)?.XZ_RefNumber ?? ZString.Empty;
		}
	}
}
