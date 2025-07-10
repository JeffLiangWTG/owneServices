using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class HelpDataStringLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLanguagesAvailable()
		{
			HelpDataString data = new HelpDataString();
			AssertEquals(new CodeDescriptionPairList(OLookUpEditType.Language).Count, data.Lookups.Languages.Count);
		}
	}
}
