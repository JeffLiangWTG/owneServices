using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.BorderWise.Test
{
	public class BorderWiseUsageSetTest : TestCaseWithFactory
	{
		public void TestGetUsages()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var licStlNoCW1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");
			var licOdplNoCW1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR", LicenceAdvStdOthList.Codes.OnDemand);
			var licCompany3 = BillingTestHelper.CreateLicenceCompany(Factory, "EN3", "CO3");
			var licStlWithCW1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN4", "CO4", "BOR");
			var licOdplWithCW1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN5", "CO5", "BOR", LicenceAdvStdOthList.Codes.OnDemand);

			var licCompanyStlNoCW1 = licStlNoCW1.Company;
			var licCompanyOdplNoCW = licOdplNoCW1.Company;

			var licCwStl = BillingTestHelper.CreateAnotherDatabase(licStlWithCW1, "CW1", false);
			licCwStl.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licCwStl.LA_AgreedLiveDate = periodStart;

			var licCwOdpl = BillingTestHelper.CreateAnotherDatabase(licOdplWithCW1, "CW1", false);
			licCwOdpl.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			licCwOdpl.LA_AgreedLiveDate = periodStart;

			var usage1aStlNoCW1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, licCompanyStlNoCW1.PK, 10);
			var usage1bStlNoCW1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.ExtraMachinePriceCode, periodStart, licCompanyStlNoCW1.PK, 5);
			var usage2OdplNoCW1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, licCompanyOdplNoCW.PK, 3);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, licCompany3.PK, 9);
			var usage4StlWithCW1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, licStlWithCW1.LA_LC, 13);
			var usage4OdplWithCW1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, licOdplWithCW1.LA_LC, 17);

			// Out of range
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart.AddMonths(1), licCompanyStlNoCW1.PK, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart.AddMonths(-1), licCompanyStlNoCW1.PK, 10);

			Factory.Save();

			var contextStl = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1));
			contextStl.IncludeOdpl = false;
			var contextStlWithOrg = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1), licCompanyStlNoCW1.LC_OH);
			contextStlWithOrg.IncludeOdpl = false;
			var contextStlWithEnt = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1), ZGuid.Empty, "EN3");
			contextStlWithEnt.IncludeOdpl = false;
			var contextOdpl = new BillingRunContext(Factory, periodStart.AddMonths(1), periodStart.AddMonths(1).AddDays(-1));

			var mainStlDatabaseMap = new Dictionary<Guid, IBilledDatabase>();
			mainStlDatabaseMap.Add(licCwStl.LA_LD.ToGuid(), licCwStl.Database);
			var mainOdplDatabaseMap = new Dictionary<Guid, IBilledDatabase>();
			mainOdplDatabaseMap.Add(licCwOdpl.LA_LD.ToGuid(), licCwOdpl.Database);

			var companyPkToDatabase1 = new Dictionary<Guid, BilledDatabase>();
			var companyPkToDatabase2 = new Dictionary<Guid, BilledDatabase>();
			var companyPkToDatabase3 = new Dictionary<Guid, BilledDatabase>();
			var companyPkToDatabase4 = new Dictionary<Guid, BilledDatabase>();
			var companyPkToDatabase5 = new Dictionary<Guid, BilledDatabase>();
			var usagesStl = BorderWiseUsageSet.GetUsagesAndLinkedCW1(contextStl, companyPkToDatabase1, mainStlDatabaseMap);
			var usagesStlWithOrg = BorderWiseUsageSet.GetUsagesAndLinkedCW1(contextStlWithOrg, companyPkToDatabase2, mainStlDatabaseMap);
			var usagesStlWithEnt = BorderWiseUsageSet.GetUsagesAndLinkedCW1(contextStlWithEnt, companyPkToDatabase3, mainStlDatabaseMap);
			var usagesOdpl = BorderWiseUsageSet.GetUsagesAndLinkedCW1(contextOdpl, companyPkToDatabase4, mainOdplDatabaseMap);
			var usagesStlNoMain = BorderWiseUsageSet.GetUsagesAndLinkedCW1(contextStl, companyPkToDatabase5, new Dictionary<Guid, IBilledDatabase>());
			AssertEquals(4, usagesStl.Length);
			AssertEquals(4, usagesStlNoMain.Length);
			AssertEquals(2, usagesStlWithOrg.Length);
			AssertEquals(1, usagesStlWithEnt.Length);
			AssertEquals(2, usagesOdpl.Length);

			AssertNotNull(usagesStl.Single(x => x.PK == usage1aStlNoCW1.PK));
			AssertNotNull(usagesStl.Single(x => x.PK == usage1bStlNoCW1.PK));
			AssertNotNull(usagesStl.Single(x => x.PK == usage3.PK));
			AssertNotNull(usagesStl.Single(x => x.PK == usage4StlWithCW1.PK));

			AssertNotNull(usagesStlWithOrg.Single(x => x.PK == usage1aStlNoCW1.PK));
			AssertNotNull(usagesStlWithOrg.Single(x => x.PK == usage1bStlNoCW1.PK));

			AssertNotNull(usagesStlWithEnt.Single(x => x.PK == usage3.PK));

			AssertNotNull(usagesOdpl.Single(x => x.PK == usage2OdplNoCW1.PK));
			AssertNotNull(usagesOdpl.Single(x => x.PK == usage4OdplWithCW1.PK));

			AssertEquals(2, companyPkToDatabase1.Count);
			AssertEquals("Company is matched to BorderWise database", licStlNoCW1.LA_LD, companyPkToDatabase1[licStlNoCW1.LA_LC.ToGuid()].PK);
			AssertEquals("Company is matched to BorderWise database", licStlWithCW1.LA_LD, companyPkToDatabase1[licStlWithCW1.LA_LC.ToGuid()].PK);
			AssertEquals("Parent is CW1 database", ZGuid.Empty, companyPkToDatabase1[licStlNoCW1.LA_LC.ToGuid()].LD_LD_ParentDatabase);
			AssertEquals("Parent is CW1 database", licCwStl.LA_LD, companyPkToDatabase1[licStlWithCW1.LA_LC.ToGuid()].LD_LD_ParentDatabase);

			AssertEquals(2, companyPkToDatabase4.Count);
			AssertEquals("Company is matched to BorderWise database", licOdplNoCW1.LA_LD, companyPkToDatabase4[licOdplNoCW1.LA_LC.ToGuid()].PK);
			AssertEquals("Company is matched to BorderWise database", licOdplWithCW1.LA_LD, companyPkToDatabase4[licOdplWithCW1.LA_LC.ToGuid()].PK);
			AssertEquals("Parent is CW1 database", ZGuid.Empty, companyPkToDatabase4[licOdplNoCW1.LA_LC.ToGuid()].LD_LD_ParentDatabase);
			AssertEquals("Parent is CW1 database", licCwOdpl.LA_LD, companyPkToDatabase4[licOdplWithCW1.LA_LC.ToGuid()].LD_LD_ParentDatabase);

			AssertEquals(2, companyPkToDatabase5.Count);
			AssertEquals("Parent is empty since no CW1 database", ZGuid.Empty, companyPkToDatabase5[licStlNoCW1.LA_LC.ToGuid()].LD_LD_ParentDatabase);
			AssertEquals("Parent is empty since no main DBs were given", ZGuid.Empty, companyPkToDatabase5[licStlWithCW1.LA_LC.ToGuid()].LD_LD_ParentDatabase);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestLoadAllChargeableUsage_SharedBorderwiseSeparateCW1()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var sharedBorderWiseLic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "BOR", false);
			var sharedBorderWiseLic2 = BillingTestHelper.CreateAnotherLicence(sharedBorderWiseLic1, "CO2");
			sharedBorderWiseLic1.Database.LD_Product = ProductTypes.Codes.BorderWise;
			sharedBorderWiseLic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			sharedBorderWiseLic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var separateCWLic1 = BillingTestHelper.CreateAnotherDatabase(sharedBorderWiseLic1, "SYD");
			var separateCWLic2 = BillingTestHelper.CreateAnotherDatabase(sharedBorderWiseLic2, "AKL");

			separateCWLic1.LA_AgreedLiveDate = periodStart;
			separateCWLic2.LA_AgreedLiveDate = periodStart;

			separateCWLic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			separateCWLic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			// Each org pays for itself
			BillingTestHelper.SetInvoicing(sharedBorderWiseLic1.Company.Header, Env.CurrentBranchPK);
			BillingTestHelper.SetInvoicing(sharedBorderWiseLic2.Company.Header, Env.CurrentBranchPK);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, sharedBorderWiseLic1.LA_LC, 10);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, sharedBorderWiseLic2.LA_LC, 20);

			Factory.Save();

			using (var dataSet = new DataSet())
			{
				BorderWiseUsageSet.LoadAllChargeableUsage(dataSet, periodStart, isStl: true, orgPk: ZGuid.Empty, enterpriseCode: null);

				var usageRows = dataSet.Tables[0].Rows;
				AssertEquals(2, usageRows.Count);

				var dbRows = dataSet.Tables[1].Rows;
				AssertEquals(2, dbRows.Count);
				AssertEquals("usage for same BW database must have same parent", separateCWLic1.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[0]["ParentPk"]));
				AssertEquals("usage for same BW database must have same parent", separateCWLic1.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[1]["ParentPk"]));
			}

			using (var dataSet = new DataSet())
			{
				BorderWiseUsageSet.LoadAllChargeableUsage(dataSet, periodStart, isStl: true, orgPk: sharedBorderWiseLic1.Company.LC_OH, enterpriseCode: "ENT");

				var usageRows = dataSet.Tables[0].Rows;
				AssertEquals(2, usageRows.Count);

				var dbRows = dataSet.Tables[1].Rows;
				AssertEquals(2, dbRows.Count);
				AssertEquals("usage for same BW database must have same parent", separateCWLic1.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[0]["ParentPk"]));
				AssertEquals("usage for same BW database must have same parent", separateCWLic1.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[1]["ParentPk"]));
			}
		}

		public void TestAppendTo()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, lic1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.ExtraMachinePriceCode, periodStart, lic1.LA_LC, 3);

			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.IncludeOdpl = false;
			var usageDatabasePkMap = new Dictionary<Guid, IBilledDatabase>();
			var mainDatabasePkMap = new Dictionary<Guid, IBilledDatabase>();
			var chargeableUsages = new List<ClientChargeableUsage>();
			var usageSet = new BorderWiseUsageSet(context);
			usageSet.AppendTo(usageDatabasePkMap, mainDatabasePkMap, chargeableUsages);

			AssertEquals("HasUsageWithDatabase", true, usageSet.HasUsageWithDatabase);
			AssertEquals("mainDatabasePkMap.Count", 1, mainDatabasePkMap.Count);
			AssertEquals("", true, mainDatabasePkMap.ContainsKey(lic1.LA_LD.ToGuid()));
			AssertEquals("usageDatabasePkMap.Count", 1, usageDatabasePkMap.Count);
			AssertEquals("", true, usageDatabasePkMap.ContainsKey(lic1.LA_LD.ToGuid()));
			AssertEquals("chargeableUsages.Count", 2, chargeableUsages.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestLoadAllChargeableUsage_SharedBorderwiseWithOwnerSeparateCW1()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var sharedBorderWiseLic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "BOR", false);
			var sharedBorderWiseLic2 = BillingTestHelper.CreateAnotherLicence(sharedBorderWiseLic1, "CO2");
			sharedBorderWiseLic1.Database.LD_Product = ProductTypes.Codes.BorderWise;
			sharedBorderWiseLic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			sharedBorderWiseLic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			sharedBorderWiseLic1.Database.LD_OH_BillingParty = sharedBorderWiseLic1.Company.LC_OH;

			var separateCWLic1 = BillingTestHelper.CreateAnotherDatabase(sharedBorderWiseLic1, "SYD");
			var separateCWLic2 = BillingTestHelper.CreateAnotherDatabase(sharedBorderWiseLic2, "AKL");

			separateCWLic1.LA_AgreedLiveDate = periodStart;
			separateCWLic2.LA_AgreedLiveDate = periodStart.AddMonths(-1);

			separateCWLic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			separateCWLic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			// Each org pays for itself
			BillingTestHelper.SetInvoicing(sharedBorderWiseLic1.Company.Header, Env.CurrentBranchPK);
			BillingTestHelper.SetInvoicing(sharedBorderWiseLic2.Company.Header, Env.CurrentBranchPK);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, sharedBorderWiseLic1.LA_LC, 10);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, sharedBorderWiseLic2.LA_LC, 20);

			Factory.Save();

			using (var dataSet = new DataSet())
			{
				BorderWiseUsageSet.LoadAllChargeableUsage(dataSet, periodStart, isStl: true, orgPk: ZGuid.Empty, enterpriseCode: null);

				var usageRows = dataSet.Tables[0].Rows;
				AssertEquals(2, usageRows.Count);

				var dbRows = dataSet.Tables[1].Rows;
				AssertEquals(2, dbRows.Count);
				AssertEquals("usage owner company has priority for linked CW1", separateCWLic1.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[0]["ParentPk"]));
				AssertEquals("usage owner company has priority for linked CW1", separateCWLic1.LA_LD, DatabaseUsageSet.AsZGuid(dbRows[1]["ParentPk"]));
			}
		}
	}
}
