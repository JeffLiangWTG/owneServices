using System.Reflection;

namespace Enterprise.Registry.Business.Customs.Testing
{
	static class GlbCompanyTestHelper
	{
		public static void TemporarilySetCountry(string countryCode)
		{
			var assembly = Assembly.Load("Enterprise.MasterFiles.Business");
			var glbCompanyType = assembly.GetType("Enterprise.MasterFiles.Business.GlbCompany");
			var currentCompanyProp = glbCompanyType.GetProperty("CurrentCompany");
			var currentCompany = currentCompanyProp.GetValue(null);
			var temporarilySetCountry = glbCompanyType.GetMethod("TemporarilySetCountry");
			temporarilySetCountry.Invoke(currentCompany, new object[] { countryCode });
		}
	}
}
