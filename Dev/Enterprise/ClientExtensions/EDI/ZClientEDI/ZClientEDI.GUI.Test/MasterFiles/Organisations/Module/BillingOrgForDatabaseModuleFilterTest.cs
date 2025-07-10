using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BillingOrgForDatabaseModuleFilter))]
	class BillingOrgForDatabaseModuleFilterTest : ModuleFilterTestCase<BillingOrgForDatabaseModuleFilter>
	{
		public void TestQuery()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "111", "SD1");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "222", "SD2");
			var licence3 = BillingTestHelper.CreateLicence(Factory, "DDD", "333", "SD3");
			var licence4 = BillingTestHelper.CreateLicence(Factory, "DDD", "444", "SD4");

			var org1 = licence1.Company.Header;
			var org2 = licence2.Company.Header;
			var org3 = licence3.Company.Header;
			var org4 = licence4.Company.Header;

			var invoiceDelivery1 = licence1.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_IsBilled = true;

			var invoiceDelivery2 = licence2.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_IsBilled = true;

			var invoiceDelivery3 = licence3.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery3.L9_IsBilled = true;
			invoiceDelivery3.L9_OH_InvoiceTo = org2.PK;

			var invoiceDelivery4 = licence4.Company.InvoiceDeliveries.AddNew();
			invoiceDelivery4.L9_IsBilled = true;
			invoiceDelivery4.L9_OH_InvoiceTo = org3.PK;

			Factory.Save();

			var filter = new BillingOrgForDatabaseModuleFilter("moo", new LicenceDatabaseNonDependentCollection(Factory));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(3, result.Length);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextADOFormatted, new[] { org1.OH_Code, org2.OH_Code, org3.OH_Code }, result.Select(x => x.OH_Code));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			AssertEquals(false, filter.Query.IsEmpty);
			result = Factory.Load<EDIOrgHeader>(filter.Query.AddToFilter(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "DDD" ));
			AssertEquals(1, result.Length);
			AssertContainsExactElementsInAnyOrder("None match: " + filter.Query.LiteralTextADOFormatted, new[] { org4.OH_Code }, result.Select(x => x.OH_Code));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override BillingOrgForDatabaseModuleFilter GetNewModuleFilter()
		{
			return new BillingOrgForDatabaseModuleFilter("moo", new LicenceDatabaseNonDependentCollection(Factory));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}
	}
}
