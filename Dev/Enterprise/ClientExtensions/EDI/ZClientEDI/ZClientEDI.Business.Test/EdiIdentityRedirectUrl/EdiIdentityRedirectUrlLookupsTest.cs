using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IdentityRedirectUrl.Business.Testing
{
	internal class EdiIdentityRedirectUrlLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRedirectTypes()
		{
			AssertContainsExactElementsInAnyOrder(new EdiIdentityRedirectType(), new EdiIdentityRedirectUrlLookups(null).RedirectTypes);
		}
	}
}
