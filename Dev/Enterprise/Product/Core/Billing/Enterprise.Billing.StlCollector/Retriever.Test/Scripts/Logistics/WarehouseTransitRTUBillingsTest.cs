using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Logistics
{
	[TestedType(typeof(WarehouseTransitRTUBillings))]
	sealed class WarehouseTransitRTUBillingsTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			var company = GlbCompany.ShallowLoadFromDB(TestConnection, c => c.GC_Code == "DEM").First();
			var branch = GlbBranch.ShallowLoadFromDB(TestConnection, b => b.GB_GC == company.PK).First();
			var department = GlbDepartment.ShallowLoadFromDB(TestConnection).First();
			var orgAddress = OrgAddress.ShallowLoadFromDB(TestConnection).First();
			var orgHeader = OrgHeader.ShallowLoadFromDB(TestConnection).First();
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];

			var whs1 = new WhsWarehouse("WH1", branch.PK, orgAddress.PK) { WW_WarehouseName = "Warehouse 1", WW_IsVirtualWarehouse = true }.WithDockDoor(TestConnection);
			var area1 = new WhsArea(whs1.PK, "SV1").InsertAndReturnObject(TestConnection);
			var row1 = new WhsRow(whs1, "DDL").InsertAndReturnObject(TestConnection);
			var location1 = new WhsLocation(row1.PK, area1.PK, area1.PK, ddlLocationType.PK).InsertAndReturnObject(TestConnection);

			var rtu1 = new WhsItemReceiveTransportationUnit(whs1, "RTU1", location1, "VEH1").InsertAndReturnObject(TestConnection);
			var rtu2 = new WhsItemReceiveTransportationUnit(whs1, "RTU2", location1, "VEH2").InsertAndReturnObject(TestConnection);
			var rtu3 = new WhsItemReceiveTransportationUnit(whs1, "RTU3", location1, "VEH3").InsertAndReturnObject(TestConnection);
			var rtu4 = new WhsItemReceiveTransportationUnit(whs1, "RTU4", location1, "VEH4").InsertAndReturnObject(TestConnection);

			var jobHeader1 = new JobHeader("WRK", branch.PK, department.PK, company.PK, rtu1.PK, "WRH") { JH_JobNum = "rtu1", JH_SystemCreateTimeUtc = TestDateTimeRange.StartDateTimeInclusive }
				.InsertAndReturnObject(TestConnection);
			var jobHeader2 = new JobHeader("WRK", branch.PK, department.PK, company.PK, rtu2.PK, "WRH") { JH_JobNum = "rtu2", JH_SystemCreateTimeUtc = TestDateTimeRange.StartDateTimeInclusive.AddDays(15) }
				.InsertAndReturnObject(TestConnection);
			var jobHeader3 = new JobHeader("WRK", branch.PK, department.PK, company.PK, rtu3.PK, "WRH") { JH_JobNum = "rtu3", JH_SystemCreateTimeUtc = TestDateTimeRange.StartDateTimeInclusive.AddDays(27) }
				.InsertAndReturnObject(TestConnection);
			var jobHeaderOld = new JobHeader("WRK", branch.PK, department.PK, company.PK, rtu4.PK, "WRH") { JH_JobNum = "rtu4", JH_SystemCreateTimeUtc = TestDateTimeRange.StartDateTimeInclusive.AddMonths(-1) }
				.InsertAndReturnObject(TestConnection);

			var accChargeCode = new AccChargeCode("TWH", "FRT", "QUP").InsertAndReturnObject(TestConnection);

			var accTransactionHeader = new AccTransactionHeader("AP", "JRJ", branch.PK, department.PK, company.PK).InsertAndReturnObject(TestConnection);
			var accTransactionHeader2 = new AccTransactionHeader("AR", "JRJ", branch.PK, department.PK, company.PK).InsertAndReturnObject(TestConnection);

			var accTransactionLine1 = new AccTransactionLines("CST", 100, department.PK, branch.PK, company.PK) { AL_JH = jobHeader1.PK }.InsertAndReturnObject(TestConnection);
			var accTransactionLine2 = new AccTransactionLines("CST", 100, department.PK, branch.PK, company.PK) { AL_JH = jobHeader2.PK }.InsertAndReturnObject(TestConnection);

			var jobCharge1 = new JobCharge(jobHeader1.PK, accChargeCode.PK, branch.PK, company.PK, department.PK, "description - 1") { JR_AL_APLine = accTransactionLine1.PK }.InsertAndReturnObject(TestConnection);
			var jobCharge2 = new JobCharge(jobHeader2.PK, accChargeCode.PK, branch.PK, company.PK, department.PK, "description - 2") { JR_AL_ARLine = accTransactionLine2.PK }.InsertAndReturnObject(TestConnection);
			var jobCharge3 = new JobCharge(jobHeader3.PK, accChargeCode.PK, branch.PK, company.PK, department.PK, "description - 3").InsertAndReturnObject(TestConnection);
			var jobChargeOld = new JobCharge(jobHeaderOld.PK, accChargeCode.PK, branch.PK, company.PK, department.PK, "description - 4").InsertAndReturnObject(TestConnection);

			jobCharge1Pk = jobCharge1.PK.ToString();
			jobCharge2Pk = jobCharge2.PK.ToString();
			jobCharge3Pk = jobCharge3.PK.ToString();

			transaction1Date = (DateTime)jobHeader1.JH_SystemCreateTimeUtc;
			transaction2Date = (DateTime)jobHeader2.JH_SystemCreateTimeUtc;
			transaction3Date = (DateTime)jobHeader3.JH_SystemCreateTimeUtc;
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 11);

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var additionalRefs1 = "{" + "\"ChargeCode\":\"FRT\",\"ChargeDesc\":\"description - 1\",\"ChargeGroup\":\"TWH\",\"ChargeSubGroup\":\"QUP\"" + "}";
			AssertRow(transactions, 0, "DEM", "DEM", "GDS", 1, "WH1", "RTU1", "rtu1", "1", jobCharge1Pk, transaction1Date, additionalRefs1);

			var additionalRefs2 = "{" + "\"ChargeCode\":\"FRT\",\"ChargeDesc\":\"description - 2\",\"ChargeGroup\":\"TWH\",\"ChargeSubGroup\":\"QUP\"" + "}";
			AssertRow(transactions, 1, "DEM", "DEM", "GDS", 1, "WH1", "RTU2", "rtu2", "1", jobCharge2Pk, transaction2Date, additionalRefs2);

			var additionalRefs3 = "{" + "\"ChargeCode\":\"FRT\",\"ChargeDesc\":\"description - 3\",\"ChargeGroup\":\"TWH\",\"ChargeSubGroup\":\"QUP\"" + "}";
			AssertRow(transactions, 2, "DEM", "DEM", "GDS", 1, "WH1", "RTU3", "rtu3", "0", jobCharge3Pk, transaction3Date, additionalRefs3);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, int line, string companyCode, string branchCode, string userCode, int itemCount, string reference01, string reference02, string reference03, string reference04, string reference05, DateTime transactionDateUtc, string additionalRefs)
		{
			var transaction = transactions.Single(t => t.Reference5 == reference05.ToUpper());
			AssertEquals("[T" + line + "] CompanyCode", companyCode, transaction.GetCompanyCode());
			AssertEquals("[T" + line + "] BranchCode", branchCode, transaction.GetBranchCode());
			AssertEquals("[T" + line + "] UserCode", userCode, transaction.ClientStaffCode);
			AssertEquals("[T" + line + "] ItemCount", itemCount, transaction.BillableCount);

			AssertEquals("[T" + line + "] TransactionReference01", reference01, transaction.Reference1);
			AssertEquals("[T" + line + "] TransactionReference02", reference02, transaction.Reference2);
			AssertEquals("[T" + line + "] TransactionReference03", reference03, transaction.Reference3);
			AssertEquals("[T" + line + "] TransactionReference04", reference04, transaction.Reference4);

			AssertEquals("[T" + line + "] TransactionDateUtc", transactionDateUtc.Date, transaction.ServiceOccuredUTC.Date);
			AssertEquals("[T" + line + "] AdditionalRefs ", additionalRefs, transaction.AdditionalRefs);
		}

		string jobCharge1Pk;
		string jobCharge2Pk;
		string jobCharge3Pk;

		DateTime transaction1Date;
		DateTime transaction2Date;
		DateTime transaction3Date;
	}
}
