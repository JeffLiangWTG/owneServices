using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SupplierPartOwnerFilterProviderTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			OrgHeader org = OrgHeader.New(Factory);

			LookupField master = new LookupField(Factory);
			LookupField detail = new LookupField(Factory);
			master.Value = org.PK.ToGuid();

			SupplierPartOwnerFilterProvider provider = new SupplierPartOwnerFilterProvider(master, detail);
			OrgHeader[] orgs = (OrgHeader[])Factory.Load(typeof(OrgHeader), provider.Filter);

			AssertEquals(1, orgs.Length);
			AssertEquals(org, orgs[0]);
		}
	}
}
