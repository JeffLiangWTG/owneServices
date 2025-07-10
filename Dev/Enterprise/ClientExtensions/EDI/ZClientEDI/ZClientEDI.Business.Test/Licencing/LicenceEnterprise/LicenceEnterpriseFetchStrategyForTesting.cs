using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class LicenceEnterpriseFetchStrategyForTesting : LicenceEnterpriseFetchStrategy
	{
		public LicenceEnterpriseFetchStrategyForTesting(LicenceEnterprise licEnterprise)
			: base(licEnterprise)
		{
		}

		protected override void AddHint(SchemaColumn column, ZGuid key)
		{
			Hints.Add(column.Name);
		}

		public List<string> Hints = new List<string>();
	}

	public class LicenceEnterpriseFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewCore()
		{
			LicenceEnterprise testEnt = Factory.New<LicenceEnterprise>();
			LicenceEnterpriseFetchStrategyForTesting strategy = new LicenceEnterpriseFetchStrategyForTesting(testEnt);

			AssertEquals("No hints should have been created", 0, strategy.Hints.Count);
			TableColumn[] columns = new TableColumn[] { new TableColumn("", "Organisation+OH_Code"),
														new TableColumn("", "OrganisationName"),
														new TableColumn("", "Organisation+OH_RL_NKClosestPort"),
														new TableColumn("", "Organisation+MainAddress+OA_Address1"),
														new TableColumn("", LicenceEnterpriseSchema.Constants.LE_EnterpriseCode),
														new TableColumn("", "NumOfDatabasesRegistered") };
			strategy.FetchForView(columns);

			AssertEquals("Three hints should have been created", 3, strategy.Hints.Count);

			AssertCollectionContains("Hint for OrgHeaderSchema.PK should have been created", OrgHeaderSchema.Constants.PK, strategy.Hints);
			AssertCollectionContains("Hint for LicenceDatabaseSchema.LD_LE should have been created", LicenceDatabaseSchema.Constants.LD_LE, strategy.Hints);
			AssertCollectionContains("Hint for OrgAddressSchema.OA_OH should have been created", OrgAddressSchema.Constants.OA_OH, strategy.Hints);
		}
	}
}