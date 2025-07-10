using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Test
{
	internal class EDIAllWebSecurityRightsTest : TestCaseWithFactory
	{
		public void TestOnlyIncludesMyAccountStuff()
		{
			var webSecurity = AllWebSecurityRights.New(Factory);
			var webSecurityRightsList = WebSecurityRightsList.New();
			var reportsWebSecurityRights = ReportsWebSecurityRights.New(Factory);

			AssertEquals(webSecurityRightsList.Count + reportsWebSecurityRights.Count, webSecurity.Count);

			var iter1 = webSecurityRightsList.GetEnumerator();
			while (iter1.MoveNext())
			{
				AssertCollectionContains(iter1.Current, webSecurity);
			}

			var iter2 = reportsWebSecurityRights.GetEnumerator();
			while (iter2.MoveNext())
			{
				AssertCollectionContains(iter2.Current, webSecurity);
			}
		}
	}
}