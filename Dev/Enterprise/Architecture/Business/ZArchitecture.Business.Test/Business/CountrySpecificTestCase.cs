using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class CountrySpecificTestCase : TestCaseWithFactory
	{
		protected ZString InitialCountryCode;
		protected override void SetUp()
		{
			base.SetUp();
			InitialCountryCode = EnvProxy.Instance.CurrentCompany.Country.Code;
		}

		protected override void TearDown()
		{
			SetCountryCode(InitialCountryCode);
			base.TearDown();
		}

		/// <summary>
		/// Sets the static GlbCompany.CurrentCompany.Country to the specified code.
		/// When the test is completed, the Country is returned to its initial value
		/// </summary>
		/// <param name="countryCode">The 2 character code of the country to be set</param>
		protected void SetCountryCode(ZString countryCode)
		{
			StaticCurrentFetcher.Instance.CurrentCompany.SetCountry(countryCode);
		}
	}
}
