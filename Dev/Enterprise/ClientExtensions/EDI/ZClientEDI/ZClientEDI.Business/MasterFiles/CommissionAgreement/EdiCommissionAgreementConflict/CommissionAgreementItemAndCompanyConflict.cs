using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class CommissionAgreementItemAndCompanyConflict : EdiCommissionAgreementConflict
	{
		public CommissionAgreementItemAndCompanyConflict(OrgCommissionAgreementItem agreementItem, ClientCompany company, OrgCommissionAgreement loserAgreement, OrgCommissionAgreementItem displayAgreementItem = null)
			: base(agreementItem, loserAgreement, displayAgreementItem)
		{
			this.clientCompany = company;
		}

		readonly ClientCompany clientCompany;

		public ClientCompany ClientCompany
		{
			get { return clientCompany; }
		}

		ZGuid ClientCompanyPk
		{
			get { return clientCompany != null ? clientCompany.PK : ZGuid.Empty; }
		}

		public override ZBool IsDeleted
		{
			get { return base.IsDeleted || (clientCompany != null && clientCompany.IsDeleted); }
		}

		#region Equals

		public override bool Equals(object obj)
		{
			var otherConflict = obj as CommissionAgreementItemAndCompanyConflict;
			if (otherConflict == null)
			{
				return false;
			}

			return
				ClientCompanyPk == otherConflict.ClientCompanyPk &&
				GetMainVersionPk(LoserCommissionAgreement) == GetMainVersionPk(otherConflict.LoserCommissionAgreement) &&
				GetMainVersionPk(WinnerCommissionAgreement) == GetMainVersionPk(otherConflict.WinnerCommissionAgreement) &&
				AgreementItem.GetItemPath().SequenceEqual(otherConflict.AgreementItem.GetItemPath());
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 13;

				hash = (hash * 7) + ClientCompanyPk.GetHashCode();
				hash = (hash * 7) + GetMainVersionPk(LoserCommissionAgreement).GetHashCode();
				hash = (hash * 7) + GetMainVersionPk(WinnerCommissionAgreement).GetHashCode();

				foreach (var item in AgreementItem.GetItemPath())
				{
					hash = (hash * 7) + item.GetHashCode();
				}

				return hash;
			}
		}

		#endregion
	}
}

