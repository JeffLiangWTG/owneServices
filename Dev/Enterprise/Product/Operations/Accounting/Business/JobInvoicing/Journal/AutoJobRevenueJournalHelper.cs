using System.Linq;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AutoJobRevenueJournalHelper : IAutoJobRevenueJournalHelper
	{
		public bool IsExcludedFromAutoJRJ(GlbBranch branch, OrgHeader org, string countryCode)
		{
			if (!AutoJRJRegistryStatusHelper.IsAutoJRJWithTaxRegistrationNumberEnabled())
			{
				return false;
			}

			if (branch == null || org == null || string.IsNullOrEmpty(countryCode))
			{
				return true;
			}

			return GetOrgTaxRegistrationNumber(branch.OrgProxy, countryCode) != GetOrgTaxRegistrationNumber(org, countryCode);
		}

		public bool IsAutoJRJEnabled => AutoJRJRegistryStatusHelper.IsAutoJRJEnabled();

		#region Implementation

		string GetOrgTaxRegistrationNumber(OrgHeader orgHeader, string countryCode)
		{
			if (orgHeader == null)
			{
				return string.Empty;
			}

			var codeType = Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
			var orgCusCode = orgHeader.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == codeType && x.OK_RN_NKCodeCountry == countryCode);

			return orgCusCode?.OK_CustomsRegNo ?? string.Empty;
		}

		#endregion
	}
}
