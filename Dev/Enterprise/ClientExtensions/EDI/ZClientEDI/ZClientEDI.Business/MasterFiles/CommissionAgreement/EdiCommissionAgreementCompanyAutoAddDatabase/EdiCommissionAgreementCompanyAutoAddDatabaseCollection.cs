using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyAutoAddDatabaseCollection : ActiveBusinessObjectCollection<EdiCommissionAgreementCompanyAutoAddDatabase>
	{
		public EdiCommissionAgreementCompanyAutoAddDatabaseCollection(EdiCommissionAgreementCustomization commissionAgreementCustomization)
			: base(commissionAgreementCustomization)
		{
		}

		public EdiCommissionAgreementCompanyAutoAddDatabase AddNew(LicenceDatabase licenceDatabase)
		{
			return AddNew(licenceDatabase != null ? licenceDatabase.PK : ZGuid.Empty);
		}

		public EdiCommissionAgreementCompanyAutoAddDatabase AddNew(ZGuid databasePk)
		{
			var result = AddNew();
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.EPD_LD = databasePk;
			}

			return result;
		}
	}
}

