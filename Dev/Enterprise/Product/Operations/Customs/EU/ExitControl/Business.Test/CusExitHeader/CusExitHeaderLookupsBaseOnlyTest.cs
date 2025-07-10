using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitHeaderLookupsBaseOnlyTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizationsFindBoxList()
		{
			AssertType(typeof(OrganisationsFindBoxCollection), lookups.OrganizationsFindBoxList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var exitHeader = Factory.New<CusExitHeader>();
			lookups = exitHeader.Lookups;
		}
		CusExitHeaderLookups lookups;
	}
}
