using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.Esterometro
{
	/// <summary>
	/// Helper class for common functionality in EsterometroXmlWriter.
	/// </summary>
	public static class EsterometroDataHelper
	{
		/// <summary>
		/// Chooses the best OrgProxy from current branch and company.
		/// </summary>
		internal static OrgHeader SelectBestOrgProxyOrNull(GlbBranch currentBranch, GlbCompany currentCompany) =>
				currentBranch.OrgProxy ?? currentCompany.OrgProxy;

		internal static ZString IvaNumberForItaly(this OrgHeader orgProxy)
		{
			var result = ZString.Empty;
			if (orgProxy != null)
			{
				var ivaCustomCodeOrNull = orgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(occ => occ.OK_CodeType == OrgCusCode.CodeTypes.IVA && occ.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Italy);
				if (ivaCustomCodeOrNull != null)
				{
					result = ivaCustomCodeOrNull.OK_CustomsRegNo;
				}
			}
			return result;
		}
	}
}
