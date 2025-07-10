using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection : NonPersistentBusinessObjectCollection<AddEdiCommissionAgreementCompanyAutoAddCountryItem>
	{
		public AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region New

		public AddEdiCommissionAgreementCompanyAutoAddCountryItem AddNew(ZString countryCode)
		{
			var result = AddNew();
			result.CountryCode = countryCode;
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AddEdiCommissionAgreementCompanyAutoAddCountryItem(Factory);
		}

		#endregion
	}
}

