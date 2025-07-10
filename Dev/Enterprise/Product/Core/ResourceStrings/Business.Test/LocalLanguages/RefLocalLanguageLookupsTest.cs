using CargoWise.EntityFramework.Testing;

namespace Enterprise.ResourceStrings.Business.Testing
{
	public class RefLocalLanguageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			AssertNotNull(Bizo.Lookups.LocalLanguagesList);
		}

		protected virtual RefLocalLanguage Bizo
		{
			get { return Factory.New<RefLocalLanguage>(); }
		}
	}
}
