using System.Globalization;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class CommissionAgreementItemAndCompanyAutoAddCountryDuplication : CommissionAgreementItemDuplication
	{
		public CommissionAgreementItemAndCompanyAutoAddCountryDuplication(OrgCommissionAgreementItem commissionAgreementItem, LicenceDatabase licenceDatabase, ZString countryCode)
			: base(commissionAgreementItem)
		{
			this.LicenceDatabase = licenceDatabase;
			this.CountryCode = countryCode;
		}

		public readonly LicenceDatabase LicenceDatabase;
		public readonly ZString CountryCode;

		ZString CountryCodeWithFallback
		{
			get { return CountryCode.IsEmpty ? (ZString)"Unknown" : CountryCode; }
		}

		public override string ToDisplayText()
		{
			var commissionAgreement = CommissionAgreementItem.CommissionAgreement;
			if (commissionAgreement == null)
			{
				return string.Empty;
			}
			else if (LicenceDatabase == null)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} (Auto-add new {1} companies for new Databases)", commissionAgreement.HumanReadableName, CountryCodeWithFallback);
			}
			else
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} (Auto-add new {1} companies for {2} Database)", commissionAgreement.HumanReadableName, CountryCodeWithFallback, LicenceDatabase.LD_ServerCode);
			}
		}
	}
}

