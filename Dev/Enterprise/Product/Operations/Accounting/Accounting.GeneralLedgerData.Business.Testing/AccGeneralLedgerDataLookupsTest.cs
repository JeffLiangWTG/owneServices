using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GeneralLedgerData.Business.Testing
{
	public class AccGeneralLedgerDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var schemas = typeof(AccGeneralLedgerDataLookups).GetProperties();
			foreach (var shcema in schemas)
			{
				AssertNoExceptionThrown(() => shcema.GetValue(Lookups));
			}
		}

		public void TestJobs()
		{
			AssertNotNull(Lookups.Jobs);
			AssertEquals(typeof(JobHeaderCollection), Lookups.Jobs.GetType());
		}

		public void TestChargeCodes()
		{
			AssertNotNull(Lookups.ChargeCodes);
			AssertEquals(typeof(AccChargeCodeCollection), Lookups.ChargeCodes.GetType());
		}

		AccGeneralLedgerData accGeneralLedgerData;

		public void TestOrganizations()
		{
			var except = new OrgHeaderCollection(Factory);
			AssertContainsExactElementsInAnyOrder(except, Lookups.Organizations);
		}

		AccGeneralLedgerDataLookups Lookups
		{
			get { return accGeneralLedgerData.Lookups; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			accGeneralLedgerData = Factory.New<AccGeneralLedgerData>();
		}
	}
}
