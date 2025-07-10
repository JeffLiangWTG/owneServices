using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalItem : AutoCommissionAgreementApprovalItem
	{
		public CommissionAgreementApprovalItem(CommissionAgreementApprovalWizard wizard, OrgCommissionAgreement commissionAgreement)
			: base(wizard.Factory)
		{
			this.Wizard = wizard;
			this.commissionAgreement = commissionAgreement;
			this.commissionAgreement.ReadOnly = true;
			this.commissionAgreement.MainVersion.ReadOnly = true;
		}

		public readonly CommissionAgreementApprovalWizard Wizard;

		internal CommissionAgreementApprovalItem()
		{
			commissionAgreement = null;
		}

		#region Properties

		public OrgCommissionAgreement CommissionAgreement
		{
			get { return commissionAgreement; }
		}
		readonly OrgCommissionAgreement commissionAgreement;

		#endregion

		#region Fetch Strategy

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new CommissionAgreementApprovalItemFetchStrategy(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return CommissionAgreement.HumanReadableName; }
		}

		#endregion
	}
}
