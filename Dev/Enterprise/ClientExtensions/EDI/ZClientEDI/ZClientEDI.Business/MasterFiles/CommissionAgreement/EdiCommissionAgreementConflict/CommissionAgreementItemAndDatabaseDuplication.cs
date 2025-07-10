using System.Globalization;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class CommissionAgreementItemAndDatabaseDuplication : CommissionAgreementItemDuplication
	{
		public CommissionAgreementItemAndDatabaseDuplication(OrgCommissionAgreementItem commissionAgreementItem, LicenceDatabase licenceDatabase)
			: base(commissionAgreementItem)
		{
			this.LicenceDatabase = licenceDatabase;
		}

		public readonly LicenceDatabase LicenceDatabase;

		public override string ToDisplayText()
		{
			var commissionAgreement = CommissionAgreementItem.CommissionAgreement;
			if (commissionAgreement == null)
			{
				return string.Empty;
			}
			else if (LicenceDatabase == null)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} (All Databases)", commissionAgreement.HumanReadableName);
			}
			else
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} ({1} Database)", commissionAgreement.HumanReadableName, LicenceDatabase.LD_ServerCode);
			}
		}
	}
}

