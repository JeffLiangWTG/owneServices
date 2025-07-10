using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class CurrentCompanyCountryCultureChangerTest : TestCaseWithFactory
	{
		public void TestCurrentCompanyCountryCultureChanger()
		{
			string originalCultureName = Culture.CurrentCompanyCountryCulture.Name;
			using (new CurrentCompanyCountryCultureChanger(Core.Constants.CountryCodes.VietNam))
			{
				AssertEquals("vi-VN", Culture.CurrentCompanyCountryCulture.Name);
			}
			AssertEquals(originalCultureName, Culture.CurrentCompanyCountryCulture.Name);
		}
	}
}
