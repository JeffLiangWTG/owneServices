using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class AddEdiCommissionAgreementCompanyAutoAddCountryItem : AutoAddEdiCommissionAgreementCompanyAutoAddCountryItem
	{
		public AddEdiCommissionAgreementCompanyAutoAddCountryItem(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region CountryCode

		[List("Countries")]
		public override ZString CountryCode
		{
			get { return base.CountryCode; }
			set { base.CountryCode = value; }
		}

		public RefCountry Country
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode); }
		}

		#endregion

		#region Lists

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region Validation

		public override void ValidateCountryCode()
		{
			base.ValidateCountryCode();
			ListValidation.ErrorIfInvalidCode(CountryCodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CountryCodeInfo, true);
		}

		#endregion
	}
}

