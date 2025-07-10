using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementDatabasePivotCollection : ActiveBusinessObjectCollection<EdiCommissionAgreementDatabasePivot>
	{
		public EdiCommissionAgreementDatabasePivotCollection(EdiCommissionAgreementCustomization commissionAgreementCustomization)
			: base(commissionAgreementCustomization)
		{
		}

		public EdiCommissionAgreementDatabasePivot AddNew(LicenceDatabase licenceDatabase)
		{
			return AddNew(licenceDatabase != null ? licenceDatabase.PK : ZGuid.Empty);
		}

		public EdiCommissionAgreementDatabasePivot AddNew(ZGuid licenceDatabasePk)
		{
			var result = AddNew();
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.EZD_LD = licenceDatabasePk;
			}

			return result;
		}
	}
}

