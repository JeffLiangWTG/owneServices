using System.Globalization;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class CommissionAgreementItemAndCompanyDuplication : CommissionAgreementItemDuplication
	{
		public CommissionAgreementItemAndCompanyDuplication(OrgCommissionAgreementItem commissionAgreementItem, ClientCompany clientCompany)
			: base(commissionAgreementItem)
		{
			this.ClientCompany = clientCompany;
		}

		public readonly ClientCompany ClientCompany;

		public override string ToDisplayText()
		{
			var commissionAgreement = CommissionAgreementItem.CommissionAgreement;
			if (commissionAgreement == null)
			{
				return string.Empty;
			}
			else if (ClientCompany == null)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} (All Companies)", commissionAgreement.HumanReadableName);
			}
			else
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} ({1} Company)", commissionAgreement.HumanReadableName, ClientCompany.LCC_Code);
			}
		}
	}
}

