using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceFilter))]
	public class MaintenanceFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadLicenceHeaders()
		{
			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			LicenceHeader licHeader1b = BillingTestHelper.CreateAnotherDatabase(licHeader1a, "BBB");
			LicenceCompany company1 = licHeader1a.Company;

			LicenceHeader licHeader2a = BillingTestHelper.CreateLicence(Factory, "YYY");
			LicenceCompany company2 = licHeader2a.Company;

			LicenceHeader licHeaderMissingBilling = BillingTestHelper.CreateLicence(Factory, "BAD");
			LicenceCompany companyMissingBilling = licHeaderMissingBilling.Company;

			BillingTestHelper.ConfigureMaintenanceLicence(licHeader1a);
			BillingTestHelper.ConfigureMaintenanceLicence(licHeader1b);
			BillingTestHelper.ConfigureMaintenanceLicence(licHeader2a);
			BillingTestHelper.ConfigureMaintenanceLicence(licHeaderMissingBilling);
			companyMissingBilling.InvoiceDeliveries.DeleteAll();

			Factory.Save();

			var filter = new MaintenanceFilter(Factory);
			filter.DueDateFrom = ZDateTime.Empty;
			filter.DueDateTo = ZDateTime.Empty;
			var headers = filter.LoadLicenceHeaders(Factory);

			AssertEquals("count", 4, headers.Length);
			Assert(headers.Any(s => s.PK == licHeader1a.PK));
			Assert(headers.Any(s => s.PK == licHeader1b.PK));
			Assert(headers.Any(s => s.PK == licHeader2a.PK));
			Assert(headers.Any(s => s.PK == licHeaderMissingBilling.PK));
		}

		public void TestIsDateInRange()
		{
			var filter = new MaintenanceFilter(Factory);
			filter.DueDateFrom = ZDateTime.Empty;
			filter.DueDateTo = ZDateTime.Empty;
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2010, 1, 1)));

			filter.DueDateTo = new ZDateTime(2010, 1, 1);
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2009, 12, 31)));
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2010, 1, 1)));
			AssertEquals(false, filter.IsDateInRange(new ZDateTime(2010, 1, 2)));

			filter.DueDateFrom = new ZDateTime(2010, 1, 1);
			filter.DueDateTo = ZDateTime.Empty;
			AssertEquals(false, filter.IsDateInRange(new ZDateTime(2009, 12, 31)));
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2010, 1, 1)));
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2010, 1, 2)));

			filter.DueDateFrom = new ZDateTime(2010, 1, 1);
			filter.DueDateTo = new ZDateTime(2010, 6, 30);
			AssertEquals(false, filter.IsDateInRange(new ZDateTime(2009, 12, 31)));
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2010, 1, 1)));
			AssertEquals(true, filter.IsDateInRange(new ZDateTime(2010, 6, 30)));
			AssertEquals(false, filter.IsDateInRange(new ZDateTime(2010, 7, 1)));
		}
	}
}
