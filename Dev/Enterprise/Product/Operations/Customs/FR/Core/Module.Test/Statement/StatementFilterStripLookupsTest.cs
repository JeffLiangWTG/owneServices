using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Module.Statement.Testing
{
	class StatementFilterStripLookupsTest : TestCaseWithFactory
	{
		public void TestCompanies()
		{
			AssertType<GlbCompanyCollection>(lookups.Companies);
		}

		public void TestStatusList()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"COM",
				"INC",
				"PND"
			}, lookups.StatusList.GetAllCodes());
		}

		public void TestConsignees()
		{
			AssertType<ConsigneeCollection>(lookups.Consignees);
		}

		public void TestDirectionList()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"IMP",
				"EXP"
			}, lookups.DirectionList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new StatementFilterStripLookups(Factory);
		}

		StatementFilterStripLookups lookups;
	}
}
