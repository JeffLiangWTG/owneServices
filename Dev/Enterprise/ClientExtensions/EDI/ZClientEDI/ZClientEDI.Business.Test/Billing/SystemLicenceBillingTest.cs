using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class SystemLicenceBillingTest : TestCaseWithFactory
	{
		public void TestCreateHostedProductionServices()
		{
			var today = ZDateTime.UtcToday;
			var thisMonth = today.AddDays(1 - today.Day);

			var prodHostedLic1 = BillingTestHelper.CreateLicence(Factory, "PR1");
			var prodNonHostedLic2 = BillingTestHelper.CreateLicence(Factory, "PR2");

			prodHostedLic1.LA_InstallationCompleteDate = today.AddMonths(-10);
			prodHostedLic1.LA_AgreedLiveDate = today.AddMonths(-24);
			prodHostedLic1.Database.LD_HostedLocation = "SYD";

			prodNonHostedLic2.LA_InstallationCompleteDate = today.AddMonths(-8);
			prodNonHostedLic2.LA_AgreedLiveDate = today.AddMonths(-4);
			prodNonHostedLic2.Database.LD_HostedLocation = Core.Constants.LicenceConstants.NotHostedWithCargoWise;

			var testLic1 = BillingTestHelper.CreateLicence(Factory, "TS1");
			testLic1.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			Factory.Save();

			var billing = new SystemLicenceBilling();

			var services = billing.CreateHostedProductionServices(Factory,
				new IBilledDatabase[] { prodHostedLic1.Database, prodNonHostedLic2.Database, testLic1.Database },
				thisMonth
				).OrderBy(x => x.Database.LD_ServerCode).ToArray();
			AssertEquals(1, services.Length);
			AssertEquals("PR1", services[0].Database.LD_ServerCode);
			AssertEquals(true, services[0].IsSystemLicenceFee);
			AssertEquals(1, (int)services[0].CPS_Units);
			AssertEquals("Server PR1", services[0].CPS_ClientRef);
			AssertEquals(thisMonth, services[0].CPS_StartDate);
		}

		public void TestCreateNonProductionServices()
		{
			var today = ZDateTime.UtcToday;
			var thisMonth = today.AddDays(1 - today.Day);

			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "PR1");
			var prodLic2 = BillingTestHelper.CreateLicence(Factory, "PR2");

			prodLic1.LA_InstallationCompleteDate = today.AddMonths(-10);
			prodLic1.LA_AgreedLiveDate = today.AddMonths(-24);

			prodLic2.LA_InstallationCompleteDate = today.AddMonths(-8);
			prodLic2.LA_AgreedLiveDate = today.AddMonths(-4);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(prodLic1.Company, "#HF", "#MF");
			stlPrices1.L6_TestDbPriceCode = "#MF";
			stlPrices1.L6_LiveMonthsUntilTestDbBilling = 3;
			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_ValidFrom = today.AddMonths(-1);
			priceLink.PHL_L6 = stlPrices1.PK;

			Factory.Save();

			var tstLic1a = CreateTestDb(prodLic1, "TS1");
			var tstLic1b = CreateTestDb(prodLic1, "TS2");
			var tstLic2a = CreateTestDb(prodLic2, "TS3");
			var tstLic2b = CreateTestDb(prodLic2, "TS4");
			var tstLic2c = CreateTestDb(prodLic2, "TS5");
			tstLic2c.LA_IsActive = false;
			var tstLic2d = CreateTestDb(prodLic2, "TS6");
			tstLic2d.Database.LD_Billable = "N";

			Factory.Save();

			var billing = new SystemLicenceBilling();
			var dbPkToPriceHeader = new Dictionary<Guid, ClientLicencePriceHeader>();
			dbPkToPriceHeader.Add(prodLic1.LA_LD.ToGuid(), stlPrices1);
			dbPkToPriceHeader.Add(prodLic2.LA_LD.ToGuid(), stlPrices1);

			var services = billing.CreateNonProductionServices(Factory,
				new Guid[] { prodLic1.LA_LD.ToGuid(), prodLic2.LA_LD.ToGuid() },
				dbPkToPriceHeader,
				ZDateTime.Empty
				).OrderBy(x => x.Database.LD_ServerCode).ToArray();
			AssertEquals(4, services.Length);
			AssertEquals("TS1", services[0].Database.LD_ServerCode);
			AssertEquals("TS2", services[1].Database.LD_ServerCode);
			AssertEquals("TS3", services[2].Database.LD_ServerCode);
			AssertEquals("TS4", services[3].Database.LD_ServerCode);

			AssertEquals("#MF", services[0].CPS_Type);
			AssertEquals("#MF", services[1].CPS_Type);
			AssertEquals("#MF", services[2].CPS_Type);
			AssertEquals("#MF", services[3].CPS_Type);

			AssertEquals(true, services[0].IsSystemLicenceFee);
			AssertEquals(true, services[1].IsSystemLicenceFee);
			AssertEquals(true, services[2].IsSystemLicenceFee);
			AssertEquals(true, services[3].IsSystemLicenceFee);

			AssertEquals(1, (int)services[0].CPS_Units);
			AssertEquals("Server TS1", services[0].CPS_ClientRef);

			AssertEquals(priceLink.PHL_ValidFrom.AddMonths(3), services[0].CPS_StartDate);
			AssertEquals(priceLink.PHL_ValidFrom.AddMonths(3), services[1].CPS_StartDate);
			AssertEquals(prodLic2.LA_AgreedLiveDate.AddMonths(3), services[2].CPS_StartDate);
			AssertEquals(prodLic2.LA_AgreedLiveDate.AddMonths(3), services[3].CPS_StartDate);
		}

		[TestDate(2017, 8, 1)]
		public void CreateNonProductionServices_TestSystemOnly()
		{
			var today = ZDateTime.UtcToday;
			var thisMonth = today.AddDays(1 - today.Day);

			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "TS1");
			var prodLic2 = BillingTestHelper.CreateLicence(Factory, "TS2");
			var prodLic3 = BillingTestHelper.CreateLicence(Factory, "TS3");

			prodLic1.LA_InstallationCompleteDate = today.AddMonths(-10);
			prodLic1.LA_AgreedLiveDate = today.AddMonths(-24);

			prodLic2.LA_InstallationCompleteDate = today.AddMonths(-8);
			prodLic2.LA_AgreedLiveDate = today.AddMonths(-4);

			prodLic3.LA_InstallationCompleteDate = today.AddMonths(-8);
			prodLic3.LA_AgreedLiveDate = today.AddMonths(-4);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(prodLic1.Company, "#HF", "#MF");
			stlPrices1.L6_TestDbPriceCode = "#MF";
			stlPrices1.L6_LiveMonthsUntilTestDbBilling = 3;
			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_ValidFrom = today.AddMonths(-1);
			priceLink.PHL_L6 = stlPrices1.PK;

			prodLic1.Database.LD_LicenceType = "TST";
			prodLic2.Database.LD_LicenceType = "TST";
			prodLic3.Database.LD_LicenceType = "TST";
			prodLic3.Database.LD_Billable = "N";

			Factory.Save();

			var billing = new SystemLicenceBilling();
			var dbPkToPriceHeader = new Dictionary<Guid, ClientLicencePriceHeader>();
			dbPkToPriceHeader.Add(prodLic1.LA_LD.ToGuid(), stlPrices1);
			dbPkToPriceHeader.Add(prodLic2.LA_LD.ToGuid(), stlPrices1);
			dbPkToPriceHeader.Add(prodLic3.LA_LD.ToGuid(), stlPrices1);

			var services = billing.CreateNonProductionServices(Factory,
				new Guid[] { prodLic1.LA_LD.ToGuid(), prodLic2.LA_LD.ToGuid(), prodLic3.LA_LD.ToGuid() },
				dbPkToPriceHeader,
				ZDateTime.Empty
				).OrderBy(x => x.Database.LD_ServerCode).ToArray();
			AssertEquals(2, services.Length);
			AssertEquals("TS1", services[0].Database.LD_ServerCode);
			AssertEquals("TS2", services[1].Database.LD_ServerCode);

			AssertEquals("#MF", services[0].CPS_Type);
			AssertEquals("#MF", services[1].CPS_Type);

			AssertEquals(true, services[0].IsSystemLicenceFee);
			AssertEquals(true, services[1].IsSystemLicenceFee);

			AssertEquals(1, (int)services[0].CPS_Units);
			AssertEquals("Server TS1", services[0].CPS_ClientRef);

			AssertEquals(priceLink.PHL_ValidFrom.AddMonths(3), services[0].CPS_StartDate);
			AssertEquals(priceLink.PHL_ValidFrom, services[1].CPS_StartDate);
		}

		[TestDate(2017, 8, 1)]
		public void CreateNonProductionServices_Partner()
		{
			var today = ZDateTime.UtcToday;
			var thisMonth = today.AddDays(1 - today.Day);
			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "TS1");
			prodLic1.LA_InstallationCompleteDate = today.AddMonths(-1);
			prodLic1.LA_AgreedLiveDate = today.AddMonths(-2);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(prodLic1.Company, "#HF", "#MF");
			stlPrices1.L6_TestDbPriceCode = "#MF";
			stlPrices1.L6_LiveMonthsUntilTestDbBilling = 20;
			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_ValidFrom = today.AddMonths(-1);
			priceLink.PHL_L6 = stlPrices1.PK;

			prodLic1.Database.LD_LicenceType = "TST";

			Factory.Save();

			var dbPkToPriceHeader = new Dictionary<Guid, ClientLicencePriceHeader>();
			dbPkToPriceHeader.Add(prodLic1.LA_LD.ToGuid(), stlPrices1);

			var services = new SystemLicenceBilling().CreateNonProductionServices(Factory,
				new Guid[] { prodLic1.LA_LD.ToGuid() },
				dbPkToPriceHeader,
				thisMonth
				).OrderBy(x => x.Database.LD_ServerCode).ToArray();
			AssertEquals(false, services.Any());

			prodLic1.Database.LD_Billable = "P";
			Factory.Save();

			services = new SystemLicenceBilling().CreateNonProductionServices(Factory,
				new Guid[] { prodLic1.LA_LD.ToGuid() },
				dbPkToPriceHeader,
				thisMonth
				).OrderBy(x => x.Database.LD_ServerCode).ToArray();

			var service = services.Single();
			AssertEquals("TS1", service.Database.LD_ServerCode);
			AssertEquals("#MF", service.CPS_Type);
			AssertEquals(true, service.IsSystemLicenceFee);
			AssertEquals(1, (int)service.CPS_Units);
			AssertEquals("Server TS1", service.CPS_ClientRef);
			AssertEquals(priceLink.PHL_ValidFrom, service.CPS_StartDate);
		}

		public void TestCalculateMinimumFees()
		{
			var today = ZDateTime.UtcToday;
			var thisMonth = today.AddDays(1 - today.Day);

			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "PR1");

			prodLic1.LA_InstallationCompleteDate = today.AddMonths(-10);
			prodLic1.LA_AgreedLiveDate = today.AddMonths(-24);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(prodLic1.Company, "#HF", "#MF");
			stlPrices1.L6_TestDbPriceCode = "#MF";
			stlPrices1.L6_LiveMonthsUntilTestDbBilling = 3;
			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_ValidFrom = today.AddMonths(-1);
			priceLink.PHL_L6 = stlPrices1.PK;

			Factory.Save();

			var tstLic1a = CreateTestDb(prodLic1, "TS1");
			var tstLic1b = CreateTestDb(prodLic1, "TS2");
			tstLic1b.Database.LD_Billable = "N";

			Factory.Save();

			var billing = new SystemLicenceBilling();
			var pks = billing.CalculateMinimumFees(Factory, prodLic1.Database, stlPrices1, ZDateTime.Empty).ToArray();
			AssertEquals(1, pks.Length);
			Assert(pks.First() == tstLic1a.Database.PK);
		}

		public void TestCalculateMinimumFees_Partner()
		{
			var today = ZDateTime.UtcToday;
			var thisMonth = today.AddDays(1 - today.Day);

			var prodLic1 = BillingTestHelper.CreateLicence(Factory, "PR1");

			prodLic1.LA_InstallationCompleteDate = today.AddMonths(-2);
			prodLic1.LA_AgreedLiveDate = today.AddMonths(-3);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(prodLic1.Company, "#HF", "#MF");
			stlPrices1.L6_TestDbPriceCode = "#MF";
			stlPrices1.L6_LiveMonthsUntilTestDbBilling = 30;
			var priceLink = prodLic1.Database.PriceHeaderLinks.AddNew();
			priceLink.PHL_ValidFrom = today.AddMonths(-5);
			priceLink.PHL_L6 = stlPrices1.PK;

			Factory.Save();

			var tstLic1a = CreateTestDb(prodLic1, "TS1");
			var tstLic1b = CreateTestDb(prodLic1, "TS2");
			tstLic1b.Database.LD_Billable = "N";

			Factory.Save();

			var pks = new SystemLicenceBilling().CalculateMinimumFees(Factory, prodLic1.Database, stlPrices1, thisMonth).ToArray();
			AssertEquals(0, pks.Length);

			tstLic1a.Database.LD_Billable = "P";
			Factory.Save();
			pks = new SystemLicenceBilling().CalculateMinimumFees(Factory, prodLic1.Database, stlPrices1, thisMonth).ToArray();
			AssertEquals(1, pks.Length);
			Assert(pks.First() == tstLic1a.Database.PK);
		}

		LicenceHeader CreateTestDb(LicenceHeader productionLicence, string serverCode)
		{
			var lic = BillingTestHelper.CreateAnotherDatabase(productionLicence, serverCode);
			lic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			lic.Database.LD_LD_ParentDatabase = productionLicence.LA_LD;
			return lic;
		}
	}
}
