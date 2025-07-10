using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalWizardLookups : ZLookups
	{
		public CommissionAgreementApprovalWizardLookups(CommissionAgreementApprovalWizard parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList FromTypes
		{
			get { return new CommissionAgreementApprovalWizardFromTypeList(); }
		}
	}
}
