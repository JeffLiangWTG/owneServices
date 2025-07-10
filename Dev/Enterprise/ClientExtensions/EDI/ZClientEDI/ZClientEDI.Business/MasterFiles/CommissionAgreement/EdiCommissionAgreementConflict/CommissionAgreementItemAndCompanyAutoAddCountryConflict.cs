using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class CommissionAgreementItemAndCompanyAutoAddCountryConflict : EdiCommissionAgreementConflict
	{
		public CommissionAgreementItemAndCompanyAutoAddCountryConflict(OrgCommissionAgreementItem commissionAgreementItem, LicenceDatabase licenceDatabase, ZString countryCode, OrgCommissionAgreement loserAgreement, OrgCommissionAgreementItem displayAgreementItem = null)
			: base(commissionAgreementItem, loserAgreement, displayAgreementItem)
		{
			this.LicenceDatabase = licenceDatabase;
			this.CountryCode = countryCode;
		}

		public readonly LicenceDatabase LicenceDatabase;

		ZGuid LicenceDatabasePk
		{
			get { return LicenceDatabase != null ? LicenceDatabase.PK : ZGuid.Empty; }
		}

		public readonly ZString CountryCode;

		public override ZBool IsDeleted
		{
			get { return base.IsDeleted || (LicenceDatabase != null && LicenceDatabase.IsDeleted); }
		}

		#region Equals

		public override bool Equals(object obj)
		{
			var otherConflict = obj as CommissionAgreementItemAndCompanyAutoAddCountryConflict;
			if (otherConflict == null)
			{
				return false;
			}

			return
				LicenceDatabasePk == otherConflict.LicenceDatabasePk &&
				CountryCode == otherConflict.CountryCode &&
				GetMainVersionPk(LoserCommissionAgreement) == GetMainVersionPk(otherConflict.LoserCommissionAgreement) &&
				GetMainVersionPk(WinnerCommissionAgreement) == GetMainVersionPk(otherConflict.WinnerCommissionAgreement) &&
				AgreementItem.GetItemPath().SequenceEqual(otherConflict.AgreementItem.GetItemPath());
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 13;

				hash = (hash * 7) + LicenceDatabasePk.GetHashCode();
				hash = (hash * 7) + CountryCode.GetHashCode();
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

