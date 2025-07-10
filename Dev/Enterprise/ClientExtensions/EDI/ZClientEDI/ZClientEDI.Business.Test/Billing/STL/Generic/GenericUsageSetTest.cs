using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class GenericUsageSetTest : TestCaseWithFactory
	{
		public void TestGetUsages()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "ABC");
			var licCompany1 = lic1.Company;
			lic1.Database.LD_Product = "ABC";
			lic1.LA_LicenceAdvStdOth = "STL";

			var usage1aStlNoCW1 = BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart, lic1, 10);
			var usage1bStlNoCW1 = BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", periodStart, lic1, 5);

			// Out of range
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart.AddMonths(1), lic1, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", periodStart.AddMonths(-1), lic1, 10);

			Factory.Save();

			var contextStl = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1));
			contextStl.IncludeOdpl = false;
			var contextStlWithOrg = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1), licCompany1.LC_OH);
			contextStlWithOrg.IncludeOdpl = false;
			var contextStlWithEnt = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1), ZGuid.Empty, "EN3");
			contextStlWithEnt.IncludeOdpl = false;

			ClientChargeableUsage[] getUsages(BillingRunContext context)
			{
				var usages = new List<ClientChargeableUsage>();
				var usageSet = new GenericUsageSet("ABC", "SAT", "DEF", context, new Dictionary<Guid, IBilledDatabase>());
				usageSet.AppendTo(new Dictionary<Guid, IBilledDatabase>(), new Dictionary<Guid, IBilledDatabase>(), usages);
				return usages.ToArray();
			}

			var usagesStl = getUsages(contextStl);
			var usagesStlWithOrg = getUsages(contextStlWithOrg);
			var usagesStlWithEnt = getUsages(contextStlWithEnt);
			var usagesStlNoMain = getUsages(contextStl);
			AssertEquals(2, usagesStl.Length);
			AssertEquals(2, usagesStlNoMain.Length);
			AssertEquals(2, usagesStlWithOrg.Length);
			AssertEquals(0, usagesStlWithEnt.Length);

			AssertNotNull(usagesStl.Single(x => x.PK == usage1aStlNoCW1.PK));
			AssertNotNull(usagesStl.Single(x => x.PK == usage1bStlNoCW1.PK));

			AssertNotNull(usagesStlWithOrg.Single(x => x.PK == usage1aStlNoCW1.PK));
			AssertNotNull(usagesStlWithOrg.Single(x => x.PK == usage1bStlNoCW1.PK));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestLoadAllChargeableUsage_SharedDatabaseSeparateCW1()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var sharedDatabaseLic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "ABC", false);
			var sharedLic2 = BillingTestHelper.CreateAnotherLicence(sharedDatabaseLic1, "CO2");
			sharedDatabaseLic1.Database.LD_Product = "ABC";
			sharedDatabaseLic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			sharedLic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var separateCWLic1 = BillingTestHelper.CreateAnotherDatabase(sharedDatabaseLic1, "SYD");
			var separateCWLic2 = BillingTestHelper.CreateAnotherDatabase(sharedLic2, "AKL");

			separateCWLic1.LA_AgreedLiveDate = periodStart;
			separateCWLic2.LA_AgreedLiveDate = periodStart;

			separateCWLic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			separateCWLic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			// Each org pays for itself
			BillingTestHelper.SetInvoicing(sharedDatabaseLic1.Company.Header, Env.CurrentBranchPK);
			BillingTestHelper.SetInvoicing(sharedLic2.Company.Header, Env.CurrentBranchPK);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ABC", "P01", periodStart, sharedDatabaseLic1, 10);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ABC", "P02", periodStart, sharedLic2, 20);

			Factory.Save();

			using (var dataSet = new DataSet())
			{
				GenericUsageSet.LoadAllChargeableUsage(dataSet, periodStart, orgPk: ZGuid.Empty, enterpriseCode: null, productCode: "ABC", "ABC");

				var usageRows = dataSet.Tables[0].Rows;
				AssertEquals(2, usageRows.Count);

				var dbRows = dataSet.Tables[1].Rows;
				AssertEquals(1, dbRows.Count);
			}

			using (var dataSet = new DataSet())
			{
				GenericUsageSet.LoadAllChargeableUsage(dataSet, periodStart, orgPk: sharedDatabaseLic1.Company.LC_OH, enterpriseCode: "ENT", productCode: "ABC", "ABC");

				var usageRows = dataSet.Tables[0].Rows;
				AssertEquals(2, usageRows.Count);

				var dbRows = dataSet.Tables[1].Rows;
				AssertEquals(1, dbRows.Count);
			}
		}

		public void TestAppendTo()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "ABC");
			lic1.Database.LD_Product = "ABC";
			lic1.LA_LicenceAdvStdOth = "STL";

			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart, lic1, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", periodStart, lic1, 3);

			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var usageDatabasePkMap = new Dictionary<Guid, IBilledDatabase>();
			var mainDatabasePkMap = new Dictionary<Guid, IBilledDatabase>();
			var chargeableUsages = new List<ClientChargeableUsage>();
			var usageSet = new GenericUsageSet("ABC", "SAT", "DEF", context);
			usageSet.AppendTo(usageDatabasePkMap, mainDatabasePkMap, chargeableUsages);

			AssertEquals("mainDatabasePkMap.Count", 1, mainDatabasePkMap.Count);
			AssertEquals("", true, mainDatabasePkMap.ContainsKey(lic1.LA_LD.ToGuid()));
			AssertEquals("usageDatabasePkMap.Count", 1, usageDatabasePkMap.Count);
			AssertEquals("", true, usageDatabasePkMap.ContainsKey(lic1.LA_LD.ToGuid()));
			AssertEquals("chargeableUsages.Count", 2, chargeableUsages.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}
	}
}
