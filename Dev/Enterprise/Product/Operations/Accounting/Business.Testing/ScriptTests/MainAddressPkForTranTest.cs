using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class MainAddressPkForTranTest : ScriptTest
	{
		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithoutCallRankingAddresses_APM()
		{
			PrepareTestDataForTranWithoutCallRankingAddresses();

			var result = RunScript(LedgerTypes.AccountsReceivable, arInvoice.PK, addressAPM.PK, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressAPM.PK}", addressAPM.PK, new ZGuid(result.Rows[0][0]));
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithoutCallRankingAddresses_ARM()
		{
			PrepareTestDataForTranWithoutCallRankingAddresses();

			job.JH_OA_AgentCollectAddr = addressARM.PK;
			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsReceivable, arInvoice.PK, null, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressARM.PK}", addressARM.PK, new ZGuid(result.Rows[0][0]));
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithoutCallRankingAddresses_PST()
		{
			PrepareTestDataForTranWithoutCallRankingAddresses();

			job.JH_OA_AgentCollectAddr = addressARM.PK;
			job.JH_OA_LocalChargesAddr = addressPST.PK;
			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsReceivable, arInvoice.PK, null, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressPST.PK}", addressPST.PK, new ZGuid(result.Rows[0][0]));
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_AR()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			var result = RunScript(LedgerTypes.AccountsReceivable, arInvoice.PK, null, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressARM.PK}", addressARM.PK, new ZGuid(result.Rows[0][0]));
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_AP()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			var result = RunScript(LedgerTypes.AccountsPayable, apInvoice.PK, null, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressAPM.PK}", addressAPM.PK, new ZGuid(result.Rows[0][0]));
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_GL()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			var result = RunScript(LedgerTypes.General, journal.PK, null, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressPST.PK}", addressPST.PK, new ZGuid(result.Rows[0][0]));
		}

		[TestDate(2021, 9, 20)]
		public void TestMainAddressPkForTranWithCallRankingAddresses_NonMainPostalAddress()
		{
			PrepareTestDataForTranWithCallRankingAddresses();

			addressPST.AddressCapability.SetIsNotMainAddress(OrgAddressType.Postal.Code);
			Factory.Save();

			var result = RunScript(LedgerTypes.General, journal.PK, null, Core.Constants.CountryCodes.Australia, company.PK, branch.PK);
			AssertEquals("The result should be 1 row", 1, result.Rows.Count);
			AssertEquals($"The main address pk should be {addressOFC.PK}", addressOFC.PK, new ZGuid(result.Rows[0][0]));
		}

		#region Implementation

		DataTable RunScript(string ledger, ZGuid transactionPK, ZGuid? invoiceAddressPK, string countryCode, ZGuid companyPK, ZGuid branchPK)
		{
			var sql = $@"SELECT * FROM MainAddressPkForTran('{ledger}', '{transactionPK}', {(invoiceAddressPK == null ? "NULL" : $"'{invoiceAddressPK}'")}, '{countryCode}', '{companyPK}', '{branchPK}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void PrepareBaseTestData()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			Factory.Save();

			company = TestObjectCreator.CreateCompanyAndBranch("AUSYD");
			branch = company.Branches[0];

			orgHeader = company.OrgProxy;
			addressARM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, true);
			addressARM.OA_RL_NKRelatedPortCode = "AUSYD";
			var addressARM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Receivables, false);
			addressARM1.OA_RL_NKRelatedPortCode = "AUSYD";

			addressAPM = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, true);
			addressAPM.OA_RL_NKRelatedPortCode = "AUAAA";
			var addressAPM1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Payables, false);
			addressAPM1.OA_RL_NKRelatedPortCode = "AUAAA";

			addressPST = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, true);
			addressPST.OA_RL_NKRelatedPortCode = "AUBBB";
			var addressPST1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Postal, false);
			addressPST1.OA_RL_NKRelatedPortCode = "AUBBB";

			addressOFC = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, true);
			addressOFC.OA_RL_NKRelatedPortCode = "AUSYD";
			var addressOFC1 = TestObjectCreator.CreateAddress(orgHeader, OrgAddressType.Office, false);
			addressOFC1.OA_RL_NKRelatedPortCode = "AUSYD";

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("s00001234");
			job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();
		}

		void PrepareTestDataForTranWithoutCallRankingAddresses()
		{
			PrepareBaseTestData();

			arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1234", TestObjectCreator.AUD, 2m, orgHeader);
			arInvoice.AH_JH = job.PK;

			Factory.Save();
		}

		void PrepareTestDataForTranWithCallRankingAddresses()
		{
			PrepareBaseTestData();

			arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1234", TestObjectCreator.AUD, 2m, orgHeader);
			arInvoice.AH_JH = job.PK;

			apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, orgHeader);
			var line = TestObjectCreator.CreateAPInvoiceLine(apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 100.00m);
			TestObjectCreator.CreateCharge(line);
			Factory.Save();
			apInvoice.AH_JH = job.PK;

			journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			journal.AH_JH = job.PK;
			journal.AH_OH = orgHeader.PK;
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);

			Factory.Save();
		}

		ARInvoice arInvoice;
		APInvoice apInvoice;
		GLJournal journal;
		OrgAddress addressPST, addressARM, addressAPM, addressOFC;
		GlbCompany company;
		GlbBranch branch;
		OrgHeader orgHeader;
		Job job;

		#endregion
	}
}
