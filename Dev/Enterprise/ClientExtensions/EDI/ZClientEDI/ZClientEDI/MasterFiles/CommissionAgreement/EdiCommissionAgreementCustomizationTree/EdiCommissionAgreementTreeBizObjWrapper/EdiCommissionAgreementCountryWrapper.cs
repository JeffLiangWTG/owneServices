using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCountryWrapper : EdiCommissionAgreementTreeBizObjWrapper
	{
		public EdiCommissionAgreementCountryWrapper(EdiCommissionAgreementCustomization customization, LicenceDatabase licenceDatabase, ZString countryCode, IEnumerable<EdiCommissionAgreementCompanyWrapper> children)
			: base(customization, children)
		{
			this.LicenceDatabase = licenceDatabase;
			this.CountryCode = countryCode;
		}

		public readonly LicenceDatabase LicenceDatabase;

		ZGuid licenceDatabasePk
		{
			get { return LicenceDatabase != null ? LicenceDatabase.PK : ZGuid.Empty; }
		}
		#region CountryCode

		internal readonly ZString CountryCode;

		RefCountry Country
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCode); }
		}

		#endregion

		#region Code

		public override ZString Code
		{
			get
			{
				var country = Country;
				if (country == null)
				{
					return "Unknown";
				}
				else
				{
					return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", Country.RN_DescMultilingual, CountryCode);
				}
			}
		}

		#endregion

		#region Description

		public override ZString Description
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ShouldAutoAdd

		public override ZBool ShouldAutoAdd
		{
			get { return GetMatchingCompanyAutoAddCountries().Any(); }
			set
			{
				if (value)
				{
					if (!GetMatchingCompanyAutoAddCountries().Any())
					{
						customization.CompanyAutoAddCountries.AddNew(licenceDatabasePk, CountryCode);
					}
				}
				else
				{
					foreach (var companyCountries in GetMatchingCompanyAutoAddCountries().ToArray())
					{
						companyCountries.Delete();
					}
				}
			}
		}

		public override ZBool ShouldAutoAdd_Enabled
		{
			get { return true; }
		}

		#endregion

		IEnumerable<EdiCommissionAgreementCompanyAutoAddCountry> GetMatchingCompanyAutoAddCountries()
		{
			return customization.CompanyAutoAddCountries
				.Where(x => x.EPC_LD == licenceDatabasePk && x.EPC_RN_NKCountry.EqualsIgnoringCase(CountryCode));
		}
	}
}
