using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyAutoAddCountryCollection : ActiveBusinessObjectCollection<EdiCommissionAgreementCompanyAutoAddCountry>
	{
		public EdiCommissionAgreementCompanyAutoAddCountryCollection(EdiCommissionAgreementCustomization commissionAgreementCustomization)
			: base(commissionAgreementCustomization)
		{
		}

		public EdiCommissionAgreementCompanyAutoAddCountry AddNew(LicenceDatabase licenceDatabase, ZString countryCode)
		{
			return AddNew(licenceDatabase != null ? licenceDatabase.PK : ZGuid.Empty, countryCode);
		}

		public EdiCommissionAgreementCompanyAutoAddCountry AddNew(ZGuid databasePk, ZString countryCode)
		{
			var result = AddNew();
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.EPC_LD = databasePk;
				result.EPC_RN_NKCountry = countryCode;
			}

			return result;
		}
	}
}

