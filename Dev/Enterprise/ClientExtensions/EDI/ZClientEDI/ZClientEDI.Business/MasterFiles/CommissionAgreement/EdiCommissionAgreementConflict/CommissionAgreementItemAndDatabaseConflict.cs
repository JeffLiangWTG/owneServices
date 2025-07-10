using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class CommissionAgreementItemAndDatabaseConflict : EdiCommissionAgreementConflict
	{
		public CommissionAgreementItemAndDatabaseConflict(OrgCommissionAgreementItem agreementItem, LicenceDatabase database, OrgCommissionAgreement loserAgreement, OrgCommissionAgreementItem displayAgreementItem = null)
			: base(agreementItem, loserAgreement, displayAgreementItem)
		{
			this.licenceDatabase = database;
		}

		readonly LicenceDatabase licenceDatabase;

		public LicenceDatabase LicenceDatabase
		{
			get { return licenceDatabase; }
		}

		ZGuid LicenceDatabasePk
		{
			get { return licenceDatabase != null ? licenceDatabase.PK : ZGuid.Empty; }
		}

		public override ZBool IsDeleted
		{
			get { return base.IsDeleted || (licenceDatabase != null && licenceDatabase.IsDeleted); }
		}

		#region Equals

		public override bool Equals(object obj)
		{
			var otherConflict = obj as CommissionAgreementItemAndDatabaseConflict;
			if (otherConflict == null)
			{
				return false;
			}

			return
				LicenceDatabasePk == otherConflict.LicenceDatabasePk &&
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

