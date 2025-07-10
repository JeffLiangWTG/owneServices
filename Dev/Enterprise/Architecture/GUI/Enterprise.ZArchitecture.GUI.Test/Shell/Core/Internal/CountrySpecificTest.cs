using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class CountrySpecificTest : TestCaseWithFactory
	{
		/// <summary>
		/// If testing a country specific module (eg, AUCustoms), return
		/// the country code for the module (eg, "AU").
		/// </summary>
		protected virtual string CountryCode
		{
			get
			{
				return null; // not country specific
			}
		}

		protected string OriginalCountryCode;

		protected override void SetUp()
		{
			base.SetUp();

			if (!string.IsNullOrEmpty(CountryCode))
			{
				var company = StaticCurrentFetcher.Instance.CurrentCompany;
				if (CountryCode != company.Country.RN_Code)
				{
					OriginalCountryCode = company.Country.RN_Code;
					company.SetCountry(CountryCode);
				}
			}
		}

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(OriginalCountryCode))
			{
				var company = StaticCurrentFetcher.Instance.CurrentCompany;
				if (OriginalCountryCode != company.Country.RN_Code)
				{
					company.SetCountry(OriginalCountryCode);
				}
			}

			base.TearDown();
		}
	}
}
