using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public abstract class EdiCommissionAgreementConflict : ICommissionAgreementItemConflict
	{
		protected EdiCommissionAgreementConflict(OrgCommissionAgreementItem agreementItem, OrgCommissionAgreement loserAgreement, OrgCommissionAgreementItem displayAgreementItem = null)
		{
			Argument.NotNull(agreementItem, "agreementItem");
			Argument.NotNull(loserAgreement, "loserAgreement");

			this.agreementItemWrapper = new CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreementItem>(agreementItem);
			this.loserAgreementWrapper = new CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreement>(loserAgreement);

			this.displayAgreementItem = displayAgreementItem;
		}

		readonly CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreementItem> agreementItemWrapper;
		readonly CommissionAgreementRelatedLastestVersionWrapper<OrgCommissionAgreement> loserAgreementWrapper;
		readonly OrgCommissionAgreementItem displayAgreementItem;

		public OrgCommissionAgreementItem AgreementItem
		{
			get { return displayAgreementItem ?? agreementItemWrapper.LastestVersion; }
		}

		public OrgCommissionAgreement WinnerCommissionAgreement
		{
			get { return AgreementItem.CommissionAgreement; }
		}

		public OrgCommissionAgreement LoserCommissionAgreement
		{
			get { return loserAgreementWrapper.LastestVersion; }
		}

		public OrgCommissionAgreementItem WinnerAgreementItem
		{
			get { return displayAgreementItem; }
		}

		public OrgCommissionAgreementItem LoserAgreementItem
		{
			get { return agreementItemWrapper.LastestVersion; }
		}

		public virtual ZBool IsDeleted
		{
			get { return AgreementItem.IsDeleted || LoserCommissionAgreement.IsDeleted; }
		}

		protected ZGuid GetMainVersionPk(OrgCommissionAgreement commissionAgreement)
		{
			return commissionAgreement != null ? commissionAgreement.MainVersion.PK : ZGuid.Empty;
		}
	}
}

