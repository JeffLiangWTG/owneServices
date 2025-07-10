using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingUtilsTest : TestCaseWithFactory
	{
		public void TestGetTopOneActiveBranchOfCompany()
		{
			var company1 = TestObjectCreator.CreateNewCompany("TS1");
			company1.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch1 = TestObjectCreator.CreateNewBranch(company1, "TB1");
			branch1.GB_IsActive = true;

			var company2 = TestObjectCreator.CreateNewCompany("TS2");
			company2.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch2 = TestObjectCreator.CreateNewBranch(company2, "TB2");
			branch2.GB_IsActive = false;

			var company3 = TestObjectCreator.CreateNewCompany("TS3");
			company3.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			AssertEquals(branch1.PK, AccountingUtils.GetTopOneActiveBranchOfCompany(company1.PK, Factory).PK);
			AssertNull(AccountingUtils.GetTopOneActiveBranchOfCompany(company2.PK, Factory));
			AssertNull(AccountingUtils.GetTopOneActiveBranchOfCompany(company3.PK, Factory));
		}

		public void TestIsTransactionNumUsedInJobInvoicingSqlQuery()
		{
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538
			using (TestConnection.TrackExecutedCommands())
			{
				AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UAInvoice, "11111", TestObjectCreator.AALSHI.PK, ZGuid.Empty, out ZString jobNumbers);
				var commandThatWasExecuted = TestConnection.ExecutedCommands.First(c => c.Contains("SELECT DISTINCT TOP 10\r\n\t\tJH_JobNum"));
				var expectedQuery = @"
	SELECT DISTINCT TOP 10
		JH_JobNum
	FROM 
		dbo.JobCharge
	INNER JOIN
		dbo.JobHeader ON JH_PK = JR_JH
	LEFT JOIN
		dbo.AccTransactionLines ON AL_PK = JR_AL_APLine
	LEFT JOIN
		dbo.AccTransactionHeader ON AH_PK = AL_AH
	WHERE
		JR_GC = @CurrentCompany
		AND JR_OH_CostAccount = @Creditor
		AND JR_APInvoiceNum = @TransactionNum
		AND JR_OSCostAmt <> 0 /* Parameterised value literalised by ZNonPersistentDataQuery */
		AND JH_ParentTableCode <> 'TH'
		AND (AL_AH IS NULL OR AL_AH <> @TransactionHeaderToExcludePK)
		AND (AH_TransactionType IS NULL OR AH_TransactionType IN ('UAI' /* Parameterised value literalised by ZNonPersistentDataQuery */, 'INV' /* Parameterised value literalised by ZNonPersistentDataQuery */, 'UAI' /* Parameterised value literalised by ZNonPersistentDataQuery */))
	ORDER BY JH_JobNum";
				var message = $"Query should contain {JobChargeSchema.Constants.JR_GC}, {JobChargeSchema.Constants.JR_OH_CostAccount}, {JobChargeSchema.Constants.JR_APInvoiceNum} and {JobChargeSchema.Constants.JR_OSCostAmt} filters in order to use NR_RX__JR_GC_JR_OH_CostAccount_JR_APInvoiceNum index";
				AssertContains(message, expectedQuery, commandThatWasExecuted, true);
			}
		}

		public void TestGetExchangeRate()
		{
			AssertEquals("exRate updated to 1", 1m, AccountingUtils.GetExchangeRate(TestObjectCreator.AUD.RX_Code, ExchangeRateType.Buy, ZDateTime.Now));

			AssertEquals("exRate default is zero if there is no today's rate", 0m, AccountingUtils.GetExchangeRate(TestObjectCreator.USD.RX_Code, ExchangeRateType.Buy, ZDateTime.Now));

			TestObjectCreator.CreateUSDBuyRate(0.88m, ZDateTime.Now);
			Factory.Save();
			AssertEquals("exRate updated to Today's rate", 0.88m, AccountingUtils.GetExchangeRate(TestObjectCreator.USD.RX_Code, ExchangeRateType.Buy, ZDateTime.Now));
		}

		public void TestCopyPersistentValuesOnDataRowLevel()
		{
			Job fromJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();

			fromJob.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.MainAddress.PK;

			var newFactory = new BusinessObjectFactory();
			Job toJob = newFactory.Load<Job>(fromJob.PK);

			toJob.CopyPersistentValuesFrom(fromJob, new BusinessObjectCloneArgs(Array.Empty<string>(), true));
			Assert("CopyPersistentValuesFrom does not set HasChanges", !toJob.HasChanges);

			AccountingUtils.CopyPersistentValuesOnDataRowLevel(fromJob, toJob);
			Assert("CopyPersistentValuesOnDataRowLevel sets HasChanges", toJob.HasChanges);
		}

		public void TestGetAllActiveCompanies()
		{
			var company = TestObjectCreator.CreateNewCompany("AUC", "AU");
			company.GC_IsActive = true;
			company.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			AssertNotNull(companies.FirstOrDefault(x => x.GC_Code == "AUC"));

			company.GC_IsActive = false;
			Factory.Save();

			companies = AccountingUtils.GetAllActiveCompanies(Factory);
			AssertNull(companies.FirstOrDefault(x => x.GC_Code == "AUC"));
		}

		public void TestGetAllActiveCompaniesCountries()
		{
			var company = TestObjectCreator.CreateNewCompany("DIT", "IT");
			company.GC_IsActive = true;
			Factory.Save();

			var countries = AccountingUtils.GetAllActiveCompaniesCountries(Db.Connection);
			AssertCollectionNotContains("IT", countries);

			company.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			countries = AccountingUtils.GetAllActiveCompaniesCountries(Db.Connection);
			AssertCollectionContains("IT", countries);

			company.GC_IsActive = false;
			Factory.Save();

			countries = AccountingUtils.GetAllActiveCompaniesCountries(Db.Connection);
			AssertCollectionNotContains("IT", countries);
		}

		public void TestGetBranchesOfCompany()
		{
			var company = TestObjectCreator.CreateNewCompany("AUC", "AU");
			company.GC_IsActive = true;
			company.GC_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

			var branch1 = TestObjectCreator.CreateBranch("PE1", "Perth company", company);
			branch1.GB_IsActive = true;
			branch1.GB_RL_NKHomePort = "AUPER";

			var branch2 = TestObjectCreator.CreateBranch("SY1", "Sydney company", company);
			branch2.GB_IsActive = true;
			branch2.GB_RL_NKHomePort = "AUSYD";

			var branch3 = TestObjectCreator.CreateBranch("BN1", "Brisbane company", company);
			branch3.GB_IsActive = false;
			branch3.GB_RL_NKHomePort = "AUBNE";

			Factory.Save();

			GlbBranchCollection branches = AccountingUtils.GetBranchesOfCompany(company.PK, Factory);
			AssertEquals(2, branches.Count);

			AssertNotNull(branches.FirstOrDefault(x => ((GlbBranch)x).GB_Code == "SY1"));
			AssertNotNull(branches.FirstOrDefault(x => ((GlbBranch)x).GB_Code == "PE1"));
			AssertNull(branches.FirstOrDefault(x => ((GlbBranch)x).GB_Code == "BN1"));

			branches = AccountingUtils.GetBranchesOfCompanySortedByTimeZone(company.PK, Factory);
			AssertEquals(2, branches.Count);

			AssertEquals("Sydney branch should precede Perth branch based on time zone", branch2.PK, branches[0].PK);
			AssertEquals("Perth branch should follow Sydney branch based on time zone", branch1.PK, branches[1].PK);
		}

		public void TestGetMasterBillSubQueryForJobHeader()
		{
			var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C0001");
			consol.JK_MasterBillNum = "1111111111";
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();
			AssertGetMasterBillSubQueryForJobHeaderResult("222");
			AssertGetMasterBillSubQueryForJobHeaderResult("111", job);

			var agencyShipment = TestObjectCreator.CreateShipment("S0002");
			agencyShipment.JS_HouseBill = "2222222222";
			agencyShipment.JS_IsShipping = true;
			var agencyShipmentJob = TestObjectCreator.CreateJob(agencyShipment);
			Factory.Save();
			AssertGetMasterBillSubQueryForJobHeaderResult("333");
			AssertGetMasterBillSubQueryForJobHeaderResult("222", agencyShipmentJob);

			var declaration = TestObjectCreator.CreateDeclaration();
			var customBill = Factory.NewWithValidTestData<Bill>();
			customBill.CU_JE = declaration.PK;
			customBill.CU_BillNum = "3333333333";
			customBill.CU_BillType = BillTypeList.Codes.MasterBill;
			var declarationJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			declarationJob.JH_ParentID = declaration.PK;
			Factory.Save();
			AssertGetMasterBillSubQueryForJobHeaderResult("444");
			AssertGetMasterBillSubQueryForJobHeaderResult("333", declarationJob);

			var cfsConsol = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C0002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			cfsConsol.JK_MasterBillNum = "4444444444";
			cfsConsol.JK_IsCFS = true;
			var cfsConsolJob = TestObjectCreator.CreateJob(cfsConsol);
			Factory.Save();
			AssertGetMasterBillSubQueryForJobHeaderResult("555");
			AssertGetMasterBillSubQueryForJobHeaderResult("444", cfsConsolJob);

			var oldGatewayConsol = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C0003GW", receivingGatewayCompany: GlbCompany.CurrentCompany);
			oldGatewayConsol.JK_MasterBillNum = "5555555555";
			var oldGatewayJob = TestObjectCreator.CreateJob(oldGatewayConsol);
			oldGatewayJob.JH_JobNum = "1234567890GW";
			Factory.Save();
			AssertGetMasterBillSubQueryForJobHeaderResult("666");
			AssertGetMasterBillSubQueryForJobHeaderResult("555", oldGatewayJob);

			var newGatewayConsol = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C0004", receivingGatewayCompany: GlbCompany.CurrentCompany);
			newGatewayConsol.JK_MasterBillNum = "6666666666";
			newGatewayConsol.JK_IsForwarding = true;
			newGatewayConsol.JK_IsCFS = false;
			newGatewayConsol.JK_AgentType = "AGT";
			newGatewayConsol.JK_SendingForwarderHandlingType = "GTA";
			var newGatewayJob = TestObjectCreator.CreateJob(newGatewayConsol);
			Factory.Save();
			AssertGetMasterBillSubQueryForJobHeaderResult("777");
			AssertGetMasterBillSubQueryForJobHeaderResult("666", newGatewayJob);
		}
		void AssertGetMasterBillSubQueryForJobHeaderResult(string billNumberToSearch, Job expectedJob = null)
		{
			var query = new ZDBOnlyQuery(typeof(JobHeader));
			query.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, billNumberToSearch), JoinCondition.And);
			var result = Factory.LoadTop1<JobHeader>(query);

			if (expectedJob == null)
			{
				AssertNull(result);
			}
			else
			{
				AssertEquals(expectedJob.PK, result.PK);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetAccountingDateSubQueryForJobHeader()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Canada))
			{
				var declaration1 = TestObjectCreator.CreateDeclaration("B001");
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration1.GetType().FullName);
				var propertyInfo = declaration1.GetType().GetProperty("CA_K84AccountingDate", BindingFlags.Instance | BindingFlags.Public);
				propertyInfo.SetValue(declaration1, new ZDateTime(2011, 2, 1));
				var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job1.JH_ParentID = declaration1.PK;

				var declaration2 = TestObjectCreator.CreateDeclaration("B002");
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration2.GetType().FullName);
				propertyInfo.SetValue(declaration2, new ZDateTime(2011, 2, 3));
				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = declaration2.PK;

				// For Export.
				var declaration3 = TestObjectCreator.CreateDeclaration("B003");
				declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration3.GetType().FullName);
				propertyInfo.SetValue(declaration3, new ZDateTime(2011, 3, 3));
				var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job3.JH_ParentID = declaration3.PK;

				// With different company.
				var declaration4 = TestObjectCreator.CreateDeclaration("B004");
				declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration4.GetType().FullName);
				propertyInfo.SetValue(declaration4, new ZDateTime(2011, 2, 3));
				var job4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job4.JH_ParentID = declaration4.PK;
				job4.JH_GC = TestObjectCreator.NonCurrentCompany.PK;

				// Accounting Date is not evaluated.
				var declaration5 = TestObjectCreator.CreateDeclaration("B005");
				declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Enterprise.Customs.CA.Business.JobDeclaration", declaration5.GetType().FullName);
				AssertEquals("Accounting Date should be empty", ZDateTime.Empty, (ZDateTime)propertyInfo.GetValue(declaration5));
				var job5 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job5.JH_ParentID = declaration5.PK;

				//For LVX and LVS
				var lvx = TestObjectCreator.CreateDeclaration("B006");
				lvx.JE_MessageType = Customs.Common.CA.CAJobMessageTypeList.Codes.LVSForConsolidation;
				var job6 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job6.JH_ParentID = lvx.PK;

				var invoice = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
				invoice.JZ_InvoiceNumber = "INV001";
				invoice.JZ_JE = lvx.PK;

				var lvs = TestObjectCreator.CreateDeclaration("B007");
				lvs.JE_MessageType = Customs.Common.CA.CAJobMessageTypeList.Codes.LowValueShipments;
				var propertyInfo2 = lvs.GetType().GetProperty("CA_K84AccountingDate", BindingFlags.Instance | BindingFlags.Public);
				propertyInfo2.SetValue(lvs, new ZDateTime(2021, 2, 1));
				var job7 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job7.JH_ParentID = lvs.PK;

				var genPivot = Factory.New<GenPivot>();
				genPivot.XX_RelationType = "ZE";
				genPivot.XX_Relation1TableCode = "JZ";
				genPivot.XX_Relation2TableCode = "JE";
				genPivot.XX_Relation1ID = invoice.PK;
				genPivot.XX_Relation2ID = lvs.PK;
				Factory.Save();

				var collection = new JobCollection(Factory);

				collection.Load(AccountingUtils.GetAccountingDateQuery(DateComparisonOperator.HasDateInRange, new ZDateTime(2011, 1, 30), new ZDateTime(2011, 2, 20)));
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder("Accouting Date property is within search range.", new List<string>() { "B001", "B002" }, collection.ToList<Job>().Select(x => ((BaseJobDeclaration)x.Parent).JE_DeclarationReference));

				collection.Load(AccountingUtils.GetAccountingDateQuery(DateComparisonOperator.HasDateInRange, new ZDateTime(2011, 2, 21), new ZDateTime(2011, 3, 2)));
				AssertEquals("Accouting Date property is out of search range.", 0, collection.Count);

				collection.Load(AccountingUtils.GetAccountingDateQuery(DateComparisonOperator.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty));
				AssertEquals(5, collection.Count);
				AssertContainsExactElementsInAnyOrder("Accouting Date property is evaluated.", new List<string>() { "B001", "B002", "B003", "B006", "B007" }, collection.ToList<Job>().Select(x => ((BaseJobDeclaration)x.Parent).JE_DeclarationReference));

				collection.Load(AccountingUtils.GetAccountingDateQuery(DateComparisonOperator.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty));
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder("Accouting Date property is empty.", new List<string>() { "B005", "B006" }, collection.ToList<Job>().Select(x => ((BaseJobDeclaration)x.Parent).JE_DeclarationReference));

				collection.Load(AccountingUtils.GetAccountingDateQuery(DateComparisonOperator.HasDateInRange, new ZDateTime(2021, 1, 30), new ZDateTime(2021, 2, 20)));
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder("LVX should be found according to its parent LVS.", new List<string>() { "B006", "B007" }, collection.ToList<Job>().Select(x => ((BaseJobDeclaration)x.Parent).JE_DeclarationReference));
			}
		}

		public void TestGetOrphanWIPsOrAccrualsFilterQuery()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S001001", false);
			var job1 = TestObjectCreator.CreateJob(shipment1, false, false);

			var wip1 = TestObjectCreator.CreateWIP(job1);
			var acr1 = TestObjectCreator.CreateAccrual(job1);
			Factory.Save();

			ZQuery query = AccountingUtils.GetOrphanWIPsOrAccrualsFilterQuery();
			var orphanWIPsOrAccruals = Factory.Load<AccTransactionLines>(query);

			Assert(orphanWIPsOrAccruals.Length == 0);

			var wipCharge = wip1.LoadRelatedJobCharge();
			wipCharge.SetARLineForcedForTest(ZGuid.Empty, true);
			Factory.Save();

			orphanWIPsOrAccruals = Factory.Load<AccTransactionLines>(query);
			Assert(orphanWIPsOrAccruals.Length > 0);
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestPATransactionNumberExists_Standard()
		{
			AssertPATransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths - 1));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestPATransactionNumberExists_Calendar()
		{
			AssertPATransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year + 1, 1, 1),
				new ZDateTime(ZDateTime.Today.Year, 1, 1));
		}

		void AssertPATransactionNumberExists(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime outsideInvoiceDate, ZDateTime insideInvoiceDate)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string transactionNumber = TestObjectCreator.GetRandomString(10);
				OrgHeader organisation = TestObjectCreator.GetOrganisation();
				Assert(!(AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				AccTransactionHeader invoice = TestObjectCreator.InsertTransaction(TransactionTypes.InvoicePendingAllocation, LedgerTypes.TransactionsPendingAllocation);
				invoice.AH_OH = organisation.PK;
				invoice.AH_TransactionNum = transactionNumber;
				invoice.AH_TransactionCount = 1;
				invoice.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				Assert("Invoice", (AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("Invoice with excluding this invoice", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, invoice.PK, invoiceDate)).HasNotification);
				Assert("InvoicePendingAllocation", (AccountingUtils.PATransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("UAInvoice", (AccountingUtils.PATransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("IncompleteInvoice", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				AccTransactionHeader creditNote = TestObjectCreator.InsertTransaction(TransactionTypes.CreditNotePendingAllocation, LedgerTypes.TransactionsPendingAllocation);
				creditNote.AH_OH = organisation.PK;
				creditNote.AH_TransactionNum = transactionNumber;
				creditNote.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				Assert("CreditNote", (AccountingUtils.PATransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("CreditNote with excluding this invoice", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, creditNote.PK, invoiceDate)).HasNotification);
				Assert("CreditNotePendingAllocation", (AccountingUtils.PATransactionNumberExists(TransactionTypes.CreditNotePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("UACreditNote", (AccountingUtils.PATransactionNumberExists(TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("IncompleteCreditNote", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = true;
				AccountingUtils.PreviousSameNumberTransactionDetails previousInvoiceDetails = new AccountingUtils.PreviousSameNumberTransactionDetails();
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Empty)).HasNotification);
				AssertEquals("Not checked", AccountingUtils.DuplicateTransactionNumberCheckResult.UnableToCheck, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideOfPeriod, previousInvoiceDetails.Result);
				AssertEquals("Previous transaction count", invoice.AH_TransactionCount, previousInvoiceDetails.PreviousTransactionCount);
				AssertEquals("Previous transaction date", invoice.AH_InvoiceDate, previousInvoiceDetails.PreviousInvoiceDate);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period but no permission", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideButNoPermissions, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, insideInvoiceDate)).HasNotification);
				AssertEquals("Inside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousInvoiceDetails.Result);
			}
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestAPTransactionNumberExists_Standard()
		{
			AssertAPTransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths - 1));
		}

		[TestDate(2024, 3, 2, 00, 00, 0)]
		public void TestAPTransactionNumberExists_Calendar()
		{
			AssertAPTransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year - 1, 12, 31),
				new ZDateTime(ZDateTime.Today.Year, 12, 31));
		}

		void AssertAPTransactionNumberExists(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime outsideInvoiceDate, ZDateTime insideInvoiceDate)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string transactionNumber = TestObjectCreator.GetRandomString(10);

				OrgHeader organisation = TestObjectCreator.GetOrganisation();

				Assert(!(AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, invoiceDate)).HasNotification);

				AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
				aPInvoice.AH_OH = organisation.PK;
				aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoice.AH_TransactionNum = transactionNumber;
				aPInvoice.AH_TransactionCount = 1;
				aPInvoice.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				Assert("Invoice", (AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("UAInvoice", (AccountingUtils.APTransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("InvoicePendingAllocation", (AccountingUtils.APTransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("IncompleteInvoice", !(AccountingUtils.APTransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, invoiceDate)).HasNotification);

				AccTransactionHeader aPCreditNote = TestObjectCreator.InsertTransaction(TransactionTypes.CreditNote);
				aPCreditNote.AH_OH = organisation.PK;
				aPCreditNote.AH_Ledger = LedgerTypes.AccountsPayable;
				aPCreditNote.AH_TransactionNum = transactionNumber;
				aPCreditNote.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				Assert("CreditNote", (AccountingUtils.APTransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("UACreditNote", (AccountingUtils.APTransactionNumberExists(TransactionTypes.UACreditNote, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("CreditNotePendingAllocation", (AccountingUtils.APTransactionNumberExists(TransactionTypes.CreditNotePendingAllocation, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("IncompleteCreditNote", !(AccountingUtils.APTransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, invoiceDate)).HasNotification);
				Assert("InvoiceBatch", !(AccountingUtils.APTransactionNumberExists(TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, invoiceDate)).HasNotification);

				AccTransactionHeader aPAdjustmentNote = TestObjectCreator.InsertTransaction(TransactionTypes.AdjustmentNote);
				aPAdjustmentNote.AH_OH = organisation.PK;
				aPAdjustmentNote.AH_Ledger = LedgerTypes.AccountsPayable;
				aPAdjustmentNote.AH_TransactionNum = transactionNumber;
				aPAdjustmentNote.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				Assert("AdjustmentNote", (AccountingUtils.APTransactionNumberExists(TransactionTypes.AdjustmentNote, transactionNumber, organisation.PK, invoiceDate)).HasNotification);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = true;
				AccountingUtils.PreviousSameNumberTransactionDetails previousInvoiceDetails = new AccountingUtils.PreviousSameNumberTransactionDetails();
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZDateTime.Empty)).HasNotification);
				AssertEquals("Unable to check", AccountingUtils.DuplicateTransactionNumberCheckResult.UnableToCheck, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideOfPeriod, previousInvoiceDetails.Result);
				AssertEquals("Previous transaction count", aPInvoice.AH_TransactionCount, previousInvoiceDetails.PreviousTransactionCount);
				AssertEquals("Previous transaction date", aPInvoice.AH_InvoiceDate, previousInvoiceDetails.PreviousInvoiceDate);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period but no permission", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideButNoPermissions, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, insideInvoiceDate)).HasNotification);
				AssertEquals("Inside of period (forbidden)", AccountingUtils.DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousInvoiceDetails.Result);
			}
		}

		public void TestAPTransactionNumberExistsWithMoreThanOneCompany()
		{
			string transactionNumber = TestObjectCreator.GetRandomString(10);
			string transactionType = TransactionTypes.Invoice;

			OrgHeader organisation = TestObjectCreator.GetOrganisation();
			GlbBranch nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			Assert("Precondition - transaction number shouldn't exist yet", !(AccountingUtils.APTransactionNumberExists(transactionType, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);

			using (TestObjectCreator.SwitchEnvToBranch(nonCurrentCompanyBranch))
			{
				CreateAPInvoice();
				Factory.Save();
			}
			Assert("Should not exist because transaction number doesn't exist for logged in company", !(AccountingUtils.APTransactionNumberExists(transactionType, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);

			CreateAPInvoice();
			Factory.Save();
			Assert("Should exist because transaction number exists for logged in company", (AccountingUtils.APTransactionNumberExists(transactionType, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);

			AccTransactionHeader CreateAPInvoice()
			{
				AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(transactionType);
				aPInvoice.AH_OH = organisation.PK;
				aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoice.AH_TransactionNum = transactionNumber;
				return aPInvoice;
			}
		}

		public void TestAPTransactionNumberExistsWithEnforceUniqueTransactionNumberRegistry()
		{
			string transactionNumber = TestObjectCreator.GetRandomString(10);

			var organisation = TestObjectCreator.GetOrganisation();

			Assert(!(AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);

			AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			aPInvoice.AH_OH = organisation.PK;
			aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			aPInvoice.AH_TransactionNum = transactionNumber;
			aPInvoice.AH_TransactionCount = 1;
			aPInvoice.AH_InvoiceDate = ZDateTime.Today;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.PayableEnforceUniqueTransactionNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			Assert("Invoice number should be unique", (AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);
			Assert("CreditNote number can be same with invoice number", !(AccountingUtils.APTransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);

			AccountingConfigurationRegistry.Instance.PayableEnforceUniqueTransactionNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Assert("AP transaction number should be unique", (AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);
			Assert("AP transaction number should be unique", (AccountingUtils.APTransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZDateTime.Today)).HasNotification);
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestUATransactionNumberExists_Standard()
		{
			AssertUATransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths - 1));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestUATransactionNumberExists_Calendar()
		{
			AssertUATransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year - 1, 12, 31),
				new ZDateTime(ZDateTime.Today.Year, 12, 31));
		}

		void AssertUATransactionNumberExists(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime outsideInvoiceDate, ZDateTime insideInvoiceDate)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				string transactionNumber = TestObjectCreator.GetRandomString(10);

				OrgHeader organisation = TestObjectCreator.GetOrganisation();

				Assert(!(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				AccTransactionHeader uAInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.UAInvoice);
				uAInvoice.AH_OH = organisation.PK;
				uAInvoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
				uAInvoice.AH_TransactionNum = transactionNumber;
				uAInvoice.AH_TransactionCount = 1;
				uAInvoice.AH_InvoiceDate = invoiceDate;
				Factory.Save();

				Assert("Invoice", (AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("Invoice with excluding this invoice", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, uAInvoice.PK, invoiceDate)).HasNotification);
				Assert("InvoicePendingAllocation", (AccountingUtils.UATransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("UAInvoice", (AccountingUtils.UATransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("IncompleteInvoice", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				AccTransactionHeader uACreditNote = TestObjectCreator.InsertTransaction(TransactionTypes.UACreditNote);
				uACreditNote.AH_OH = organisation.PK;
				uACreditNote.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
				uACreditNote.AH_TransactionNum = transactionNumber;
				Factory.Save();

				Assert("CreditNote", (AccountingUtils.UATransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("CreditNotePendingAllocation", (AccountingUtils.UATransactionNumberExists(TransactionTypes.CreditNotePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("UACreditNote", (AccountingUtils.UATransactionNumberExists(TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("InvoiceBatch", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("IncompleteCreditNote", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = true;
				AccountingUtils.PreviousSameNumberTransactionDetails previousInvoiceDetails = new AccountingUtils.PreviousSameNumberTransactionDetails();
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Empty)).HasNotification);
				AssertEquals("Unable to check", AccountingUtils.DuplicateTransactionNumberCheckResult.UnableToCheck, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideOfPeriod, previousInvoiceDetails.Result);
				AssertEquals("Previous transaction count", uAInvoice.AH_TransactionCount, previousInvoiceDetails.PreviousTransactionCount);
				AssertEquals("Previous transaction date", uAInvoice.AH_InvoiceDate, previousInvoiceDetails.PreviousInvoiceDate);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period but no permission", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideButNoPermissions, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, insideInvoiceDate)).HasNotification);
				AssertEquals("Inside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousInvoiceDetails.Result);
			}
		}

		public void TestUATransactionNumberExistsChecksApprovalRequestsLinkedInvoices()
		{
			var transactionNumber = TestObjectCreator.GetRandomString(10);

			var organisation = TestObjectCreator.GetOrganisation();

			Assert(!(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(organisation, 100, transactionNumber);
			var request = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
			var statusFields = typeof(GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var statusField in statusFields)
			{
				var status = (string)statusField.GetValue(null);
				var shouldTransactionExist = status != Constants.GenApprovalRequestApprovalStatus.Rejected && status != Constants.GenApprovalRequestApprovalStatus.Cancelled;
				request.XP_ApprovalStatus = status;
				if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
				{
					request.SetContext(BusinessContext.CancelApprovalRequestByUser);
				}
				request.Factory.Save();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Assert("Invoice without registry on", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals("Invoice", shouldTransactionExist, (AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				Assert("Invoice with excluding this invoice", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, invoice.PK, ZDateTime.Today)).HasNotification);
				AssertEquals("InvoicePendingAllocation", shouldTransactionExist, (AccountingUtils.UATransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				AssertEquals("UAInvoice", shouldTransactionExist, (AccountingUtils.UATransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				Assert("IncompleteInvoice", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			}

			var creditNote = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APCreditNote>(organisation, 100, transactionNumber);
			request = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(creditNote);
			foreach (var statusField in statusFields)
			{
				var status = (string)statusField.GetValue(null);
				var shouldTransactionExist = status != Constants.GenApprovalRequestApprovalStatus.Rejected && status != Constants.GenApprovalRequestApprovalStatus.Cancelled;
				request.XP_ApprovalStatus = status;
				if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
				{
					request.SetContext(BusinessContext.CancelApprovalRequestByUser);
				}
				request.Factory.Save();

				AssertEquals("CreditNote", shouldTransactionExist, (AccountingUtils.UATransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				AssertEquals("CreditNotePendingAllocation", shouldTransactionExist, (AccountingUtils.UATransactionNumberExists(TransactionTypes.CreditNotePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				AssertEquals("UACreditNote", shouldTransactionExist, (AccountingUtils.UATransactionNumberExists(TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				Assert("InvoiceBatch", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				Assert("IncompleteCreditNote", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			}
		}

		public void TestPATransactionNumberExistsChecksApprovalRequestsLinkedInvoices()
		{
			var transactionNumber = TestObjectCreator.GetRandomString(10);
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			Assert(!(AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			var transaction = TestObjectCreator.CreateTransactionPendingAllocation(transactionNumber, organisation, 100);
			Factory.Save();

			var request = new BusinessObjectFactory().Load<TransactionPendingAllocationApprovalRequest>(transaction.AllocationApprovalRequest.PK);

			var allStatusFields = typeof(GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Public | BindingFlags.Static);
			var statusFieldsThatAllowDuplicateTransactionNumber = new string[]
			{
				GenApprovalRequestApprovalStatus.Cancelled,
				GenApprovalRequestApprovalStatus.Rejected
			};

			foreach (var statusField in allStatusFields)
			{
				var status = (string)statusField.GetValue(null);
				request.XP_ApprovalStatus = status;
				if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled)
				{
					request.SetContext(BusinessContext.CancelApprovalRequestByUser);
				}
				request.Factory.Save();

				var shouldTransactionExist = !statusFieldsThatAllowDuplicateTransactionNumber.Contains(status);

				AssertEquals(shouldTransactionExist, (AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				Assert("Invoice with excluding this invoice", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, transaction.PK, ZDateTime.Today)).HasNotification);
				AssertEquals("InvoicePendingAllocation", shouldTransactionExist, (AccountingUtils.PATransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				AssertEquals("UAInvoice", shouldTransactionExist, (AccountingUtils.PATransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
				Assert("IncompleteInvoice", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			}

			Assert("empty transaction number returns false", !(AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, string.Empty, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
		}

		public void TestInvalidInvoiceDateShouldReturnUnableCheck()
		{
			var transactionNumber = TestObjectCreator.GetRandomString(10);

			var organisation = TestObjectCreator.GetOrganisation();

			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(organisation, 100);
			invoice.AH_TransactionNum = transactionNumber;
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var previousSameNumberTransactionDetails = AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Invalid);
			AssertEquals("Invalid Invoice date should return UnableCheck", AccountingUtils.DuplicateTransactionNumberCheckResult.UnableToCheck, previousSameNumberTransactionDetails.Result);
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestINTransactionNumberExists_Standard()
		{
			AssertINTransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				ZDateTime.Today,
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths),
				ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths - 1));
		}

		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestINTransactionNumberExists_Calendar()
		{
			AssertINTransactionNumberExists(AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL, ZDateTime.Today,
				new ZDateTime(ZDateTime.Today.Year - 1, 6, 1),
				new ZDateTime(ZDateTime.Today.Year, 6, 1));
		}

		[TestDate(2024, 11, 1, 00, 00, 0)]
		public void TestAPTransactionNumberExistsInPreviousYear_Calendar()
		{
			AccountingConfigurationRegistry.Instance.PayableEnforceUniqueTransactionNumber.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL))
			{
				string transactionNumber = TestObjectCreator.GetRandomString(10);

				var organisation = TestObjectCreator.GetOrganisation();

				AccTransactionHeader aPInvoiceFromPreviousYear = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
				aPInvoiceFromPreviousYear.AH_OH = organisation.PK;
				aPInvoiceFromPreviousYear.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoiceFromPreviousYear.AH_TransactionNum = transactionNumber;
				aPInvoiceFromPreviousYear.AH_TransactionCount = 1;
				aPInvoiceFromPreviousYear.AH_InvoiceDate = new ZDateTime(ZDateTime.Today.Year - 1, 2, 1);
				aPInvoiceFromPreviousYear.AH_PostDate = ZDateTime.Today;
				Factory.Save();

				Assert(!(AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZDateTime.Today)).HasError);

				AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
				aPInvoice.AH_OH = organisation.PK;
				aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoice.AH_TransactionNum = transactionNumber;
				aPInvoice.AH_TransactionCount = 2;
				aPInvoice.AH_InvoiceDate = ZDateTime.Today;
				aPInvoice.AH_PostDate = ZDateTime.Today;
				Factory.Save();

				var previousSameNumberTransactionDetailsFromPreviousYear = AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber,
					organisation.PK, new ZDateTime(ZDateTime.Today.Year - 1, 4, 1));
				AssertEquals("The transaction number is already in use. Last posted transaction’s invoice date is 01-Feb-23 which is in the same calendar year. This transaction number cannot be used. Please enter another one.", AccountingUtils.DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousSameNumberTransactionDetailsFromPreviousYear.Result);

				Assert(!(AccountingUtils.APTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZDateTime.Today.AddMonths(13))).HasError);
			}
		}

		void AssertINTransactionNumberExists(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime outsideInvoiceDate, ZDateTime insideInvoiceDate)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var transactionNumber = TestObjectCreator.GetRandomString(10);

				var organisation = TestObjectCreator.GetOrganisation();

				Assert(!(AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>(transactionNumber, TestObjectCreator.AUD, 1, 100, 0, 0, 100, 0, 0, organisation);
				invoice.AH_TransactionCount = 1;
				invoice.AH_InvoiceDate = invoiceDate;
				invoice.SaveAsIncomplete();

				Assert("Invoice", (AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("Invoice with excluding this invoice", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, invoice.PK, invoiceDate)).HasNotification);
				Assert("InvoicePendingAllocation", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("UAInvoice", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("IncompleteInvoice", (AccountingUtils.INTransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var creditNote = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APCreditNote>(organisation, 100);
				creditNote.AH_TransactionNum = transactionNumber;
				Factory.Save();

				Assert("CreditNote", (AccountingUtils.INTransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("CreditNotePendingAllocation", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.CreditNotePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("UACreditNote", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("InvoiceBatch", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);
				Assert("IncompleteCreditNote", (AccountingUtils.INTransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, ZGuid.Empty, invoiceDate)).HasNotification);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = true;
				AccountingUtils.PreviousSameNumberTransactionDetails previousInvoiceDetails = new AccountingUtils.PreviousSameNumberTransactionDetails();
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Empty)).HasNotification);
				AssertEquals("Unable to check", AccountingUtils.DuplicateTransactionNumberCheckResult.UnableToCheck, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideOfPeriod, previousInvoiceDetails.Result);
				AssertEquals("Previous transaction count", invoice.AH_TransactionCount, previousInvoiceDetails.PreviousTransactionCount);
				AssertEquals("Previous transaction date", invoice.AH_InvoiceDate, previousInvoiceDetails.PreviousInvoiceDate);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, outsideInvoiceDate)).HasNotification);
				AssertEquals("Outside of period but no permission", AccountingUtils.DuplicateTransactionNumberCheckResult.OutsideButNoPermissions, previousInvoiceDetails.Result);

				Assert("HasNotification", (previousInvoiceDetails = AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, insideInvoiceDate)).HasNotification);
				AssertEquals("Inside of period", AccountingUtils.DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousInvoiceDetails.Result);
			}
		}

		public void TestINTransactionNumberExistsChecksApprovalRequestsLinkedInvoices()
		{
			var transactionNumber = TestObjectCreator.GetRandomString(10);

			var organisation = TestObjectCreator.GetOrganisation();

			Assert(!(AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APInvoice>(organisation, 100);
			invoice.AH_TransactionNum = transactionNumber;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("Invoice without registry on", (AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("IncompleteInvoice without registry on", (AccountingUtils.INTransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Invoice", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("Invoice with excluding this invoice", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, invoice.PK, ZDateTime.Today)).HasNotification);
			Assert("InvoicePendingAllocation", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.InvoicePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("UAInvoice", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("IncompleteInvoice", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.IncompleteInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			var creditNote = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<APCreditNote>(organisation, 100);
			creditNote.AH_TransactionNum = transactionNumber;
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("CreditNote without registry on", (AccountingUtils.INTransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("IncompleteCreditNote without registry on", (AccountingUtils.INTransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("CreditNote", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("CreditNotePendingAllocation", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.CreditNotePendingAllocation, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("UACreditNote", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("InvoiceBatch", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("IncompleteCreditNote", !(AccountingUtils.INTransactionNumberExists(TransactionTypes.IncompleteCreditNote, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
		}

		public void TestUATransactionNumberExistsWithMoreThanOneCompany()
		{
			string transactionNumber = TestObjectCreator.GetRandomString(10);

			OrgHeader organisation = TestObjectCreator.GetOrganisation();
			GlbBranch nonCurrentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			Assert("Precondition - transaction number shouldn't exist yet", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			using (TestObjectCreator.SwitchEnvToBranch(nonCurrentCompanyBranch))
			{
				CreateUAInvoice();
				Factory.Save();
			}
			Assert("Should not exist because transaction number doesn't exist for logged in company", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("Should not exist because transaction number doesn't exist for logged in company", !(AccountingUtils.UATransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			CreateUAInvoice();
			Factory.Save();
			Assert("Should exist because transaction number exists for logged in company", (AccountingUtils.UATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);
			Assert("Should exist because transaction number exists for logged in company", (AccountingUtils.UATransactionNumberExists(TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty, ZDateTime.Today)).HasNotification);

			AccTransactionHeader CreateUAInvoice()
			{
				AccTransactionHeader uAInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.UAInvoice);
				uAInvoice.AH_OH = organisation.PK;
				uAInvoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
				uAInvoice.AH_TransactionNum = transactionNumber;
				return uAInvoice;
			}
		}

		[SuspendCriticalValidation]
		public void TestIsTransactionNumUsedInJobInvoicing()
		{
			var jobNumbers = ZString.Empty;

			OrgHeader organisation = TestObjectCreator.GetOrganisation();
			AssertEquals(false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UAInvoice, "11111", organisation.PK, ZGuid.Empty, out jobNumbers));

			var uAInvoice = CreateUAInvoiceWithJobRelatedOneLine(organisation, "11111");
			Factory.Save();
			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(uAInvoice.AH_TransactionType, "11111", organisation.PK, ZGuid.Empty, out jobNumbers));
			AssertEquals("Z00001000", jobNumbers);
			AssertEquals(false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(uAInvoice.AH_TransactionType, "11111", organisation.PK, uAInvoice.PK, out jobNumbers));

			var uAInvoice2 = CreateUAInvoiceWithJobRelatedOneLine(organisation, "11111");
			AssertEquals(false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(uAInvoice.AH_TransactionType, "11111", organisation.PK, uAInvoice.PK, out jobNumbers));
			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(uAInvoice2.AH_TransactionType, "11111", organisation.PK, uAInvoice2.PK, out jobNumbers));
			AssertEquals("Z00001000", jobNumbers);
		}

		[SuspendCriticalValidation]
		public void TestIsTransactionNumUsedInJobInvoicingLetsInvoiceAndCreditNoteWithSameNumber()
		{
			var jobNumbers = ZString.Empty;

			OrgHeader organisation = TestObjectCreator.GetOrganisation();
			AssertEquals("No Invoices with same number", false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UAInvoice, "12345", organisation.PK, ZGuid.Empty, out jobNumbers));
			Assert("JobNumbers must be empty", jobNumbers.IsEmpty);

			var invoice = CreateTransactionWithJobRelatedOneLine(TransactionTypes.UAInvoice, LedgerTypes.UnapprovedPayableTransactions, organisation, "12345", ZDateTime.Empty, "JOB001");
			AssertEquals(false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(invoice.AH_TransactionType, "12345", organisation.PK, invoice.PK, out jobNumbers));
			Assert("JobNumbers must be empty", jobNumbers.IsEmpty);

			Factory.Save();

			AssertEquals(false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(invoice.AH_TransactionType, "12345", organisation.PK, invoice.PK, out jobNumbers));
			Assert("JobNumbers must be empty", jobNumbers.IsEmpty);
			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(invoice.AH_TransactionType, "12345", organisation.PK, ZGuid.Empty, out jobNumbers));
			AssertEquals("JobNumbers", "JOB001", jobNumbers);

			var creditNote = CreateTransactionWithJobRelatedOneLine(TransactionTypes.UACreditNote, LedgerTypes.UnapprovedPayableTransactions, organisation, "12345", ZDateTime.Empty, "JOB100");
			AssertEquals(false, AccountingUtils.IsTransactionNumUsedInJobInvoicing(creditNote.AH_TransactionType, "12345", organisation.PK, creditNote.PK, out jobNumbers));
			Assert("JobNumbers must be empty", jobNumbers.IsEmpty);

			for (int i = 2; i <= 10; i++)
			{
				var jobNum = "JOB0" + i.ToString("D2");
				CreateJobCharge(organisation, "12345", jobNum);
			}
			Factory.Save();

			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UAInvoice, "12345", organisation.PK, ZGuid.Empty, out jobNumbers));
			AssertEquals("JobNumbers", "JOB001, JOB002, JOB003, JOB004, JOB005, JOB006, JOB007, JOB008, JOB009, JOB010", jobNumbers);
			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.Invoice, "12345", organisation.PK, ZGuid.Empty, out jobNumbers));
			AssertEquals("JobNumbers", "JOB001, JOB002, JOB003, JOB004, JOB005, JOB006, JOB007, JOB008, JOB009, JOB010", jobNumbers);

			CreateJobCharge(organisation, "12345", "JOB011");
			Factory.Save();

			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UAInvoice, "12345", organisation.PK, ZGuid.Empty, out jobNumbers));
			AssertEquals("Only top 10 JobNumbers", "JOB001, JOB002, JOB003, JOB004, JOB005, JOB006, JOB007, JOB008, JOB009, JOB010", jobNumbers);
			AssertEquals(true, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.Invoice, "12345", organisation.PK, ZGuid.Empty, out jobNumbers));
			AssertEquals("Only top 10 JobNumbers", "JOB001, JOB002, JOB003, JOB004, JOB005, JOB006, JOB007, JOB008, JOB009, JOB010", jobNumbers);
		}

		[SuspendCriticalValidation]
		public void TestIsTransactionNumUsedInJobInvoicing_BookingWithQuote()
		{
			var transactionNum = "1234";
			var creditor = TestObjectCreator.CreateOrgHeader("CRE", true, false);
			var jobNumbers = ZString.Empty;

			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			var job = TestObjectCreator.CreateJob(quotedBooking, false);
			var charge = TestObjectCreator.CreateCharge(job);
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_APInvoiceNum = transactionNum;
			Factory.Save();

			AssertPossibleTransactionTypes(false);

			CreateTransactionWithJobRelatedOneLine(TransactionTypes.UAInvoice, LedgerTypes.UnapprovedPayableTransactions, creditor, transactionNum, ZDateTime.Empty, "JOB001");
			CreateTransactionWithJobRelatedOneLine(TransactionTypes.UACreditNote, LedgerTypes.UnapprovedPayableTransactions, creditor, transactionNum, ZDateTime.Empty, "JOB002");
			CreateTransactionWithJobRelatedOneLine(TransactionTypes.AdjustmentNote, LedgerTypes.UnapprovedPayableTransactions, creditor, transactionNum, ZDateTime.Empty, "JOB003");
			Factory.Save();

			AssertPossibleTransactionTypes(true);

			transactionNum = "4568";

			AssertPossibleTransactionTypes(false);

			void AssertPossibleTransactionTypes(bool expectedValue)
			{
				CombineAssertions(() =>
				{
					AssertEquals("TransactionTypes.Invoice", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.Invoice, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
					AssertEquals("TransactionTypes.CreditNote", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.CreditNote, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
					AssertEquals("TransactionTypes.AdjustmentNote", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.AdjustmentNote, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
					AssertEquals("TransactionTypes.UAInvoice", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UAInvoice, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
					AssertEquals("TransactionTypes.UACreditNote", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.UACreditNote, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
					AssertEquals("TransactionTypes.InvoicePendingAllocation", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.InvoicePendingAllocation, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
					AssertEquals("TransactionTypes.CreditNotePendingAllocation", expectedValue, AccountingUtils.IsTransactionNumUsedInJobInvoicing(TransactionTypes.CreditNotePendingAllocation, transactionNum, creditor.PK, ZGuid.Empty, out jobNumbers));
				});
			}
		}

		AccTransactionHeader CreateUAInvoiceWithJobRelatedOneLine(OrgHeader organisation, ZString transactionNum)
		{
			return CreateTransactionWithJobRelatedOneLine(TransactionTypes.UAInvoice, LedgerTypes.UnapprovedPayableTransactions, organisation, transactionNum, ZDateTime.Empty);
		}

		AccTransactionHeader CreateTransactionWithJobRelatedOneLine(string transactionType, string ledger, OrgHeader organisation, ZString transactionNum, ZDateTime invoiceDate, string jobNumber = null)
		{
			AccTransactionHeader result = TestObjectCreator.InsertTransaction(transactionType, ledger);
			result.AH_OH = organisation.PK;
			result.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			result.AH_TransactionNum = transactionNum;

			var line = Factory.New<UAInvoiceLine>();
			line.AL_AC = TestObjectCreator.CC1.PK;

			if (!string.IsNullOrEmpty(jobNumber))
			{
				var job = TestObjectCreator.CreateJob(jobNumber, TestObjectCreator.LocalClient, 1M, TestObjectCreator.Agent, 2M);
				line.AL_JH = job.PK;
			}
			else
			{
				line.AL_JH = TestObjectCreator.Job1.PK;
			}
			line.AL_AH = result.PK;

			Charge charge = TestObjectCreator.CreateCharge(line);
			charge.JR_OH_CostAccount = organisation.PK;
			charge.JR_APInvoiceNum = transactionNum;
			charge.JR_AL_APLine = line.PK;
			charge.JR_JH = line.AL_JH;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_APInvoiceDate = invoiceDate;
			charge.JR_OSCostAmt = 10m;
			return result;
		}

		Charge CreateJobCharge(OrgHeader organisation, ZString transactionNum, string jobNumber)
		{
			var job = TestObjectCreator.CreateJob(jobNumber, TestObjectCreator.LocalClient, 1M, TestObjectCreator.Agent, 2M);

			var charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = organisation.PK;
			charge.JR_APInvoiceNum = transactionNum;
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 10m;
			return charge;
		}

		public void TestAccountingDocumentTitles()
		{
			AssertEquals("Receipt Matching", AccountingUtils.AccountingDocumentTitles.ReceiptMatching);
			AssertEquals("Receipt Journal", AccountingUtils.AccountingDocumentTitles.ReceiptJournal);
		}

		public void TestCompareTransactionsBySettlementGroupAndOrganisation()
		{
			AccountingUtils utils = new AccountingUtils();
			TransactionHeaderCollection transactions = MakeCollectionToBeUnsorted(GetTransactionsForComparingTest());
			Assert("Collection should be unsorted", !CollectionIsSorted(transactions));
			transactions.Sort<TransactionHeader>(utils.CompareTransactionsBySettlementGroupAndOrganisation);
			Assert("Collection should be sorted by SettlementGroup", CollectionIsSorted(transactions));
		}

		[ExpectNoExceptions]
		public void TestGetNextChequeNumberFromActiveChequeBookWithLock_LastNo()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_IsActive = ZBool.True;
			chequeBook.AK_LastNo = 10;
			chequeBook.AK_CurrentNo = 10;
			Factory.Save();

			AccountingUtils utils = new AccountingUtils();
			ZDecimal result = 0;
			try
			{
				Db.Connection.BeginTransaction();
				result = utils.GetNextChequeNumberFromActiveChequeBookWithLock(chequeBook, Factory);
				Db.Connection.CommitTransaction();
			}
			catch
			{
				Db.Connection.RollbackTransaction();
			}
			AssertEquals("Should be correct result", 10m, result);

			AccChequeBook reloadedChequeBook = new BusinessObjectFactory().Load<AccChequeBook>(chequeBook.PK);
			Assert("ChequeBook should become to be Inactive", !reloadedChequeBook.AK_IsActive);
			AssertEquals("CurrentNo should be set to 11", 11m, reloadedChequeBook.AK_CurrentNo);
		}

		[ExpectNoExceptions]
		public void TestGetNextChequeNumberFromActiveChequeBookWithLock_NotLastNo()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_IsActive = ZBool.True;
			chequeBook.AK_LastNo = 10;
			chequeBook.AK_CurrentNo = 9;
			Factory.Save();

			AccountingUtils utils = new AccountingUtils();
			ZDecimal result = ZDecimal.Zero;
			try
			{
				Db.Connection.BeginTransaction();
				result = utils.GetNextChequeNumberFromActiveChequeBookWithLock(chequeBook, Factory);
				Db.Connection.CommitTransaction();
			}
			catch
			{
				Db.Connection.RollbackTransaction();
			}
			AssertEquals("Should be correct result", 9m, result);

			AccChequeBook reloadedChequeBook = new BusinessObjectFactory().Load<AccChequeBook>(chequeBook.PK);
			Assert("ChequeBook should remain Active", reloadedChequeBook.AK_IsActive);
			AssertEquals("CurrentNo should be set to 10", 10m, reloadedChequeBook.AK_CurrentNo);
		}

		public void TestGetNextChequeNumberFromActiveChequeBookWithLock_InActive()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_IsActive = ZBool.False;
			chequeBook.AK_LastNo = 10;
			chequeBook.AK_CurrentNo = 9;
			Factory.Save();

			AccountingUtils utils = new AccountingUtils();
			try
			{
				Db.Connection.BeginTransaction();
				ZDecimal result = utils.GetNextChequeNumberFromActiveChequeBookWithLock(chequeBook, Factory);
				Db.Connection.CommitTransaction();
				Assert("Test Failed. The exception should have been raised.", false);
			}
			catch (Exception ex)
			{
				Db.Connection.RollbackTransaction();
				AssertEquals("Should be of correct exception type", typeof(AllocationChequeBookException), ex.GetType());
				AssertEquals("The exception message should be correct", AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsFullExceptionMessage, ((AllocationChequeBookException)ex).UserFriendlyMessage);
			}
		}

		[ExpectNoExceptions]
		public void TestGetNextChequeNumberFromActiveChequeBookWithLock_NullChequeBook()
		{
			TestGetNextChequeNumberFromActiveChequeBookWithLock_NullParameters(true, false);
		}

		[ExpectNoExceptions]
		public void TestGetNextChequeNumberFromActiveChequeBookWithLock_NullFactory()
		{
			TestGetNextChequeNumberFromActiveChequeBookWithLock_NullParameters(false, true);
		}

		[ExpectNoExceptions]
		void TestGetNextChequeNumberFromActiveChequeBookWithLock_NullParameters(bool nullChequeBook, bool nullFactory)
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_IsActive = ZBool.True;
			chequeBook.AK_LastNo = 10;
			chequeBook.AK_CurrentNo = 10;
			Factory.Save();

			AccountingUtils utils = new AccountingUtils();
			ZDecimal result = ZDecimal.Zero;
			try
			{
				Db.Connection.BeginTransaction();
				if (nullChequeBook)
				{
					result = utils.GetNextChequeNumberFromActiveChequeBookWithLock(null, Factory);
				}
				if (nullFactory)
				{
					result = utils.GetNextChequeNumberFromActiveChequeBookWithLock(chequeBook, null);
				}
				Db.Connection.CommitTransaction();
			}
			catch
			{
				Db.Connection.RollbackTransaction();
			}
			AssertEquals("Should be correct result", 0m, result);

			AccChequeBook reloadedChequeBook = new BusinessObjectFactory().Load<AccChequeBook>(chequeBook.PK);
			AssertEquals("CurrentNo should be set to", 10m, reloadedChequeBook.AK_CurrentNo);
		}

		public void TestConvertPostingOptionToHumanReadableName()
		{
			string[] humanReadableNames =
			{
				"Post All Charges and Costs",
				"Post Local Client Charges",
				"Post Gateway Agent Charges",
				"Post Overseas Agent Charges",
				"Post All Revenue Charges",
				"Post All Costs",
				"Post Disbursement Charges only",
				"Post Customs Disbursement Costs only",
				"Post Customs Disbursement Charges only",
				"Post Consol Costs Only",
				"Post Charges for All Group Companies",
				"Post Charges for Group Companies in My Login Country/Region",
			};

			string postAllNameForConsol = "Post Whole Consol";
			AssertEquals("All options must be tested here.", humanReadableNames.Length, Enum.GetNames(typeof(JobInvoicingPostingOption)).Length);
			Array optionValues = Enum.GetValues(typeof(JobInvoicingPostingOption));
			string message = "Postion option: '{0}', isThisConsolPosting: '{1}'";
			for (int i = 0; i < optionValues.Length; i++)
			{
				JobInvoicingPostingOption optionValue = (JobInvoicingPostingOption)optionValues.GetValue(i);
				string expectedName = humanReadableNames[i];
				AssertEquals(string.Format(message, optionValue, "False"), expectedName, AccountingUtils.ConvertPostingOptionToHumanReadableName(optionValue, false));
				if (optionValue == JobInvoicingPostingOption.All)
				{
					expectedName = postAllNameForConsol;
				}
				AssertEquals(string.Format(message, optionValue, "True"), expectedName, AccountingUtils.ConvertPostingOptionToHumanReadableName(optionValue, true));
			}
		}

		public void TestConvertApprovingOptionToHumanReadableName()
		{
			AssertEquals("Single Level 1", AccountingUtils.ConvertApprovingOptionToHumanReadableName(1, ApprovalCredentialOption.SingleLogin));
			AssertEquals("Two Level 2", AccountingUtils.ConvertApprovingOptionToHumanReadableName(2, ApprovalCredentialOption.DoubleLogin));
			AssertEquals("Seq 1,2,3,4,5,6", AccountingUtils.ConvertApprovingOptionToHumanReadableName(6, ApprovalCredentialOption.SequentialLogin));
		}

		[ExpectNoExceptions]
		public void TestDeleteFileSafe()
		{
			AssertEquals("Should not throw exception when null is passed", false, AccountingUtils.DeleteFileSafe(null));
			AssertEquals("Empty string", false, AccountingUtils.DeleteFileSafe(string.Empty));
			AssertEquals("Non-existing path", false, AccountingUtils.DeleteFileSafe(Env.TempPath + "abc###zzz.###"));
		}

		[ExpectNoExceptions]
		public void TestDeleteDirectorySafe()
		{
			AssertEquals("Should not throw exception when null is passed", false, AccountingUtils.DeleteDirectorySafe(null));
			AssertEquals("Empty string", false, AccountingUtils.DeleteDirectorySafe(string.Empty));
			AssertEquals("Non-existing path", false, AccountingUtils.DeleteDirectorySafe(Env.TempPath + "abc###zzz.###"));
		}

		public void TestChunksOf()
		{
			for (int batchSize = 1; batchSize < 5; batchSize++)
			{
				var batchList = new[] { 9, 1, 7 };
				var chunks = new StringBuilder();
				foreach (var batch in AccountingUtils.ChunksOf(batchList, batchSize))
				{
					chunks.Append(batch.Count);
				}

				switch (batchSize)
				{
					case 1:
						AssertEquals("Chunks 1 1 1", "111", chunks.ToString());
						break;
					case 2:
						AssertEquals("Chunks 2 1", "21", chunks.ToString());
						break;
					case 3:
						AssertEquals("Chunks 3", "3", chunks.ToString());
						break;
					case 4:
						AssertEquals("Chunks 3", "3", chunks.ToString());
						break;
				}
			}
		}

		public void TestGetDuplicateNumbersTransactionList()
		{
			string transactionNumber = TestObjectCreator.GetRandomString(10);

			OrgHeader organisation = TestObjectCreator.GetOrganisation();

			var results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.Invoice, transactionNumber, organisation.PK);
			AssertEquals("Haven't inserted any data, must return zero list", results.Length, 0);

			//AP Invoice
			AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.Invoice);
			aPInvoice.AH_OH = organisation.PK;
			aPInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPInvoice.AH_TransactionNum = transactionNumber;
			Factory.Save();

			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.Invoice, transactionNumber, organisation.PK);
			AssertEquals("Inserted 1 AP Invoice", results.Length, 1);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.UAInvoice, transactionNumber, organisation.PK);
			AssertEquals("Inserted 1 AP Invoice", results.Length, 1);

			AccTransactionHeader aPCreditNote = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.CreditNote);
			aPCreditNote.AH_OH = organisation.PK;
			aPCreditNote.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			aPCreditNote.AH_TransactionNum = transactionNumber;
			Factory.Save();

			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.CreditNote, transactionNumber, organisation.PK);
			AssertEquals("Inserted 1 CreditNotes", results.Length, 1);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.UACreditNote, transactionNumber, organisation.PK);
			AssertEquals("Inserted 1 CreditNotes", results.Length, 1);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK);
			AssertEquals("Inserted 1 CreditNotes, must return 0 InvoiceBatch", results.Length, 0);

			//UA Invoice + AP Invoice inserted b4
			AccTransactionHeader uAInvoice = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.UAInvoice);
			uAInvoice.AH_OH = organisation.PK;
			uAInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions;
			uAInvoice.AH_TransactionNum = transactionNumber;
			Factory.Save();

			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 AP Invoice + 1 UA Invoice", 2, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 AP Invoice + 1 UA Invoice", 2, results.Length);

			AccTransactionHeader uACreditNote = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.UACreditNote);
			uACreditNote.AH_OH = organisation.PK;
			uACreditNote.AH_Ledger = ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions;
			uACreditNote.AH_TransactionNum = transactionNumber;
			Factory.Save();

			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 UAUACreditNote + 1 CreditNotes", 2, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 UAUACreditNote + 1 CreditNotes", 2, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 UAUACreditNote + 1 CreditNotes, must return 0 InvoiceBatch", results.Length, 0);

			// Only UA Invoice
			aPInvoice.Delete();
			aPCreditNote.Delete();
			Factory.Save();
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.Invoice, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 UA Invoice", 1, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.UAInvoice, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 UA Invoice", 1, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.CreditNote, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 CreditNotes", 1, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.UACreditNote, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 CreditNotes", 1, results.Length);
			results = AccountingUtils.GetDuplicateNumbersTransactionList(ZArchitecture.Core.TransactionTypes.InvoiceBatch, transactionNumber, organisation.PK, ZGuid.Empty);
			AssertEquals("Inserted 1 CreditNotes, must return 0 InvoiceBatch", results.Length, 0);
		}

		public void TestGetCoLoadMasterBillSubQueryForJobHeader()
		{
			var consol = TestObjectCreator.CreateConsol("KRSEL", "AUSYD", "C0001");
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadMasterBill = "1111111111";
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			shipment.JS_IsShipping = false;
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			AssertGetCoLoadMBLSubQueryForJobHeaderResult("222");
			AssertGetCoLoadMBLSubQueryForJobHeaderResult("111", job);

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();
			AssertGetCoLoadMBLSubQueryForJobHeaderResult("111", job);

			shipment.JS_IsShipping = true;
			Factory.Save();
			AssertGetCoLoadMBLSubQueryForJobHeaderResult("111");

			consol.JK_AgentType = Constants.AgentType.Direct;
			Factory.Save();
			AssertGetCoLoadMBLSubQueryForJobHeaderResult("111");
		}
		void AssertGetCoLoadMBLSubQueryForJobHeaderResult(string coLoadMBLToSearch, Job expectedJob = null)
		{
			var query = new ZDBOnlyQuery(typeof(JobHeader));
			query.AddSubQuery(AccountingUtils.GetCoLoadMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, coLoadMBLToSearch), JoinCondition.And);
			var result = Factory.LoadTop1<JobHeader>(query);

			if (expectedJob == null)
			{
				AssertNull(result);
			}
			else
			{
				AssertEquals(expectedJob.PK, result.PK);
			}
		}

		public void TestGetCoLoadMBLSubQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			var maximumMaxLength = JobConsolSchema.JK_CoLoadMasterBill.MaxLength;
			AssertEquals("is no result query should be FALSE when search value is an acceptable size.", false, AccountingUtils.GetCoLoadMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(maximumMaxLength)).IsNoResultQuery);
			AssertEquals("is no result query should be TRUE when search value is too long.", true, AccountingUtils.GetCoLoadMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(maximumMaxLength + 1)).IsNoResultQuery);

			var query1 = new ZDBOnlyQuery(typeof(Job));
			query1.AddSubQuery(AccountingUtils.GetCoLoadMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobConsolSchema.JK_CoLoadMasterBill.MaxLength)), JoinCondition.And);
			AssertContains(JobConsolSchema.JK_CoLoadMasterBill.Name, query1.GetAsWhereClause(false));

			var query2 = new ZDBOnlyQuery(typeof(Job));
			query2.AddSubQuery(AccountingUtils.GetCoLoadMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobConsolSchema.JK_CoLoadMasterBill.MaxLength + 1)), JoinCondition.And);
			AssertNotContains(JobConsolSchema.JK_CoLoadMasterBill.Name, query2.GetAsWhereClause(false));
		}

		public void TestGetMasterBillSubQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			var maximumMaxLength = Math.Max(JobConsolSchema.JK_MasterBillNum.MaxLength, Math.Max(JobShipmentSchema.JS_HouseBill.MaxLength, CusDecHouseBillSchema.CU_BillNum.MaxLength));
			AssertEquals("is no result query should be FALSE when search value is an acceptable size.", false, AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(maximumMaxLength)).IsNoResultQuery);
			AssertEquals("is no result query should be TRUE when search value is too long.", true, AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(maximumMaxLength + 1)).IsNoResultQuery);

			var query1 = new ZDBOnlyQuery(typeof(Job));
			query1.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobConsolSchema.JK_MasterBillNum.MaxLength)), JoinCondition.And);
			AssertContains(JobConsolSchema.JK_MasterBillNum.Name, query1.GetAsWhereClause(false));

			var query2 = new ZDBOnlyQuery(typeof(Job));
			query2.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobConsolSchema.JK_MasterBillNum.MaxLength + 1)), JoinCondition.And);
			AssertNotContains(JobConsolSchema.JK_MasterBillNum.Name, query2.GetAsWhereClause(false));

			var query3 = new ZDBOnlyQuery(typeof(Job));
			query3.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobShipmentSchema.JS_HouseBill.MaxLength)), JoinCondition.And);
			AssertContains(JobShipmentSchema.JS_HouseBill.Name, query3.GetAsWhereClause(false));

			var query4 = new ZDBOnlyQuery(typeof(Job));
			query4.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobShipmentSchema.JS_HouseBill.MaxLength + 1)), JoinCondition.And);
			AssertNotContains(JobShipmentSchema.JS_HouseBill.Name, query4.GetAsWhereClause(false));

			var query5 = new ZDBOnlyQuery(typeof(Job));
			query5.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(CusDecHouseBillSchema.CU_BillNum.MaxLength)), JoinCondition.And);
			AssertContains(CusDecHouseBillSchema.CU_BillNum.Name, query5.GetAsWhereClause(false));

			var query6 = new ZDBOnlyQuery(typeof(Job));
			query6.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(CusDecHouseBillSchema.CU_BillNum.MaxLength + 1)), JoinCondition.And);
			AssertNotContains(CusDecHouseBillSchema.CU_BillNum.Name, query6.GetAsWhereClause(false));
		}

		public void TestGetHouseBillQueryForJobHeader_WhenSearchLengthGreaterThanMaxLength()
		{
			var maximumMaxLength = Math.Max(JobShipmentSchema.JS_HouseBill.MaxLength, JobDeclarationSchema.JE_HouseBill.MaxLength);
			var minimumMaxLength = Math.Min(JobShipmentSchema.JS_HouseBill.MaxLength, JobDeclarationSchema.JE_HouseBill.MaxLength);

			AssertEquals("is-no-result query should be FALSE when search value is an acceptable size.", false, AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(maximumMaxLength)).IsNoResultQuery);
			AssertEquals("is-no-result query should be TRUE when search value is too long.", true, AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(maximumMaxLength + 1)).IsNoResultQuery);

			var query1 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobShipmentSchema.JS_HouseBill.MaxLength));
			AssertContains(JobShipmentSchema.JS_HouseBill.Name, query1.GetAsWhereClause(false));

			if (minimumMaxLength == maximumMaxLength || minimumMaxLength == JobShipmentSchema.JS_HouseBill.MaxLength)
			{
				AssertContains(JobDeclarationSchema.JE_HouseBill.Name, query1.GetAsWhereClause(false));
			}
			else
			{
				AssertNotContains(JobDeclarationSchema.JE_HouseBill.Name, query1.GetAsWhereClause(false));
			}

			var query2 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobShipmentSchema.JS_HouseBill.MaxLength + 1));
			AssertNotContains(JobShipmentSchema.JS_HouseBill.Name, query2.GetAsWhereClause(false));

			var query3 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobDeclarationSchema.JE_HouseBill.MaxLength));
			AssertContains(JobDeclarationSchema.JE_HouseBill.Name, query3.GetAsWhereClause(false));

			if (minimumMaxLength == maximumMaxLength || minimumMaxLength == JobDeclarationSchema.JE_HouseBill.MaxLength)
			{
				AssertContains(JobShipmentSchema.JS_HouseBill.Name, query3.GetAsWhereClause(false));
			}
			else
			{
				AssertNotContains(JobShipmentSchema.JS_HouseBill.Name, query3.GetAsWhereClause(false));
			}

			var query4 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(JobDeclarationSchema.JE_HouseBill.MaxLength + 1));
			AssertNotContains(JobDeclarationSchema.JE_HouseBill.Name, query4.GetAsWhereClause(false));
		}

		public void TestGetHouseBillQueryForJobHeader_QueryData()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_HouseBill = "A12345";
			shipment2.JS_HouseBill = "B98765";

			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			declaration1.JE_HouseBill = "A56789";
			declaration2.JE_HouseBill = "B12345";

			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();

			job1.JH_ParentID = shipment1.PK;
			job2.JH_ParentID = shipment2.PK;
			job3.JH_ParentID = declaration1.PK;
			job4.JH_ParentID = declaration2.PK;

			Factory.Save();

			//For performance reason please include JH_GC filter in the query that queries JobHeader.
			var query1 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, "A").AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			var query2 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.Equal, "B98765").AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			var query3 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.Equal, "B12345").AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			var query4 = AccountingUtils.GetHouseBillQueryForJobHeader(SQLComparisonOperator.StartsWith, "C").AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);

			var result1 = Factory.Load<Job>(query1).Select(u => u.PK).ToList();
			var result2 = Factory.Load<Job>(query2).Select(u => u.PK).ToList();
			var result3 = Factory.Load<Job>(query3).Select(u => u.PK).ToList();
			var result4 = Factory.Load<Job>(query4).Select(u => u.PK).ToList();

			AssertEquals(2, result1.Count);
			Assert("Collection should contain job1", result1.Contains(job1.PK));
			Assert("Collection should contain job3", result1.Contains(job3.PK));

			AssertEquals(1, result2.Count);
			Assert("Collection should contain job2", result2.Contains(job2.PK));

			AssertEquals(1, result3.Count);
			Assert("Collection should contain job4", result3.Contains(job4.PK));

			AssertEquals(0, result4.Count);
		}

		public void TestGetGatewayJobsQueryForJobHeader_QueryData()
		{
			DataRegistry.Instance.MultiSearchSeparator = ",";

			var consol1 = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consol2 = TestObjectCreator.CreateGatewayConsol("KRSEL", "AUSYD", "C0002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol1);
			var shipment2 = TestObjectCreator.CreateShipment("S0002", consol2);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var job3 = TestObjectCreator.CreateJob(consol1);
			var job4 = TestObjectCreator.CreateJob(consol2);

			Factory.Save();
			var query1 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.StartsWith, "S0001");
			var query2 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.Equal, "S0002");
			var query3 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.StartsWith, "S");
			var query4 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.StartsWith, "C");
			var query5 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.Equal, "S0002,S0001");
			var query6 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.Equal, "A,'B',asdfgasdfgasfdgasdfgasfdgasfdgasfdgasdfg");
			var query7 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.StartsWith, "S0002,S0001");
			var query8 = AccountingUtils.GetGatewayJobsQueryForJobHeader(SQLComparisonOperator.StartsWith, "A,'B',asdfgasdfgasfdgasdfgasfdgasfdgasfdgasdfg");

			var result1 = Factory.Load<Job>(query1).Select(u => u.PK).ToList();
			var result2 = Factory.Load<Job>(query2).Select(u => u.PK).ToList();
			var result3 = Factory.Load<Job>(query3).Select(u => u.PK).ToList();
			var result4 = Factory.Load<Job>(query4).Select(u => u.PK).ToList();
			var result5 = Factory.Load<Job>(query5).Select(u => u.PK).ToList();
			var result6 = Factory.Load<Job>(query6).Select(u => u.PK).ToList();
			var result7 = Factory.Load<Job>(query7).Select(u => u.PK).ToList();
			var result8 = Factory.Load<Job>(query8).Select(u => u.PK).ToList();

			AssertEquals(2, result1.Count);
			Assert("Collection should contain job for shipment1", result1.Contains(job1.PK));
			Assert("Collection should contain job for consol1", result1.Contains(job3.PK));

			AssertEquals(2, result2.Count);
			Assert("Collection should contain job for shipment2", result2.Contains(job2.PK));
			Assert("Collection should contain job for consol2", result2.Contains(job4.PK));

			AssertEquals(4, result3.Count);
			Assert("Collection should contain job for shipment1", result3.Contains(job1.PK));
			Assert("Collection should contain job for shipment2", result3.Contains(job2.PK));
			Assert("Collection should contain job for consol1", result3.Contains(job3.PK));
			Assert("Collection should contain job for consol2", result3.Contains(job4.PK));

			AssertEquals(0, result4.Count);

			AssertEquals(4, result5.Count);
			Assert("Collection should contain job for shipment1", result5.Contains(job1.PK));
			Assert("Collection should contain job for shipment2", result5.Contains(job2.PK));
			Assert("Collection should contain job for consol1", result5.Contains(job3.PK));
			Assert("Collection should contain job for consol2", result5.Contains(job4.PK));

			AssertEquals(0, result6.Count);

			AssertEquals(4, result7.Count);
			Assert("Collection should contain job for shipment1", result7.Contains(job1.PK));
			Assert("Collection should contain job for shipment2", result7.Contains(job2.PK));
			Assert("Collection should contain job for consol1", result7.Contains(job3.PK));
			Assert("Collection should contain job for consol2", result7.Contains(job4.PK));

			AssertEquals(0, result8.Count);
		}

		[ExpectNoExceptions]
		public void TestAnyNumberNotExceedingMaxLengthNotThrowExceptionWhenPassInMultiSearchSeparator()
		{
			ZString multiSearchSeparator = EnvProxy.Instance.Registry.MultiSearchSeparator;
			Assert(!AccountingUtils.AnyNumberNotExceedingMaxLength(ref multiSearchSeparator, JobConsolSchema.JK_MasterBillNum.MaxLength));
		}

		public void TestGetGatewayJobsQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			AssertQueryContentsForMaxLengthCheck(AccountingUtils.GetGatewayJobsQueryForJobHeader, JobShipmentSchema.JS_UniqueConsignRef);
		}

		public void TestGetConsolNumberQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			AssertQueryContentsForMaxLengthCheck(AccountingUtils.GetConsolNumberQueryForJobHeader, JobConsolSchema.JK_UniqueConsignRef);
		}

		public void TestGetCustomsEntryNumberQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			AssertQueryContentsForMaxLengthCheck(AccountingUtils.GetCustomsEntryNumberQueryForJobHeader, CusEntryNumSchema.CE_EntryNum);
		}

		public void TestGetOrderNumberQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			AssertQueryContentsForMaxLengthCheck(AccountingUtils.GetOrderNumberQueryForJobHeader, JobOrderHeaderSchema.JD_OrderNumber);
		}

		public void TestGetConsignmentRunsheetQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			AssertQueryContentsForMaxLengthCheck(AccountingUtils.GetConsignmentRunsheetQueryForJobHeader, DtbConsignmentRunSheetSchema.KG_RunSheetNumber);
		}

		public void TestGetTransportBookingReferenceQueryForJobHeader_WhenSearchValueIsGreaterThanMaxLength()
		{
			AssertQueryContentsForMaxLengthCheck(AccountingUtils.GetTransportBookingReferenceQueryForJobHeader, DtbBookingSchema.KM_TransportReference);
		}

		public void TestIsAllowFuturePostingRegistryEnabled()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingUtils.IsAllowFuturePostingRegistryEnabled);
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingUtils.IsAllowFuturePostingRegistryEnabled);
		}

		public void TestDoesUserHaveFuturePostingSecurity()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			AssertEquals(false, AccountingUtils.DoesUserHaveFuturePostingSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertEquals(true, AccountingUtils.DoesUserHaveFuturePostingSecurity);
		}

		[SuspendCriticalValidation]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetFirstARTransactionFromJob()
		{
			var otherCompanyPK = new ZGuid("22c79b3e-cd3e-4ca1-8fc9-6da7ab1bd061");
			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_GC = otherCompanyPK;
			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			var arInvoiceIn2011 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2011", TestObjectCreator.AUD, 1m, 1, 1, 1, 0);
			var arInvoiceIn2012 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV2012", TestObjectCreator.AUD, 1m);
			var arInvoiceIn2013 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2013", TestObjectCreator.AUD, 1m, 1, 1, 1, 0);
			arInvoiceIn2013.AH_GC = otherCompanyPK;
			var arInvoiceIn2014 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2014", TestObjectCreator.AUD, 1m, 1, 1, 1, 0);

			arInvoiceIn2011.AH_PostDate = ZDateTime.Empty;
			arInvoiceIn2012.AH_PostDate = new ZDateTime(2012, 1, 1);
			arInvoiceIn2013.AH_PostDate = new ZDateTime(2013, 1, 1);
			arInvoiceIn2014.AH_PostDate = new ZDateTime(2014, 1, 1);

			arInvoiceIn2011.AH_JH = job1.PK;
			arInvoiceIn2012.AH_JH = job2.PK;
			arInvoiceIn2013.AH_JH = ZGuid.Empty;
			arInvoiceIn2013.Lines[0].AL_JH = job1.PK;
			arInvoiceIn2014.AH_JH = job2.PK;

			Factory.Save();
			AssertEquals(ZGuid.Empty, arInvoiceIn2013.AH_JH);
			AssertEquals(job1.PK, arInvoiceIn2013.Lines[0].AL_JH);

			using (Db.Connection.TrackExecutedCommands())
			{
				AssertEquals(arInvoiceIn2013.PK, AccountingUtils.GetFirstARTransactionFromJob(job1).PK);
				AssertEquals(arInvoiceIn2012.PK, AccountingUtils.GetFirstARTransactionFromJob(job2).PK);
				AssertEquals(null, AccountingUtils.GetFirstARTransactionFromJob(job3));
				AssertEquals(null, AccountingUtils.GetFirstARTransactionFromJob(null));

				var dbQueriesOnAccTransactionHeader = Db.Connection.ExecutedCommandsAndQueryPlans.Where(x => x.Item1.Contains("FROM dbo.AccTransactionHeader"));
				AssertEquals("we expect 3 queries", 3, dbQueriesOnAccTransactionHeader.Count());
				Assert("Index hint is added to avoid poor SQL indexes when calling GetFirstARTransactionFromJob() method",
					dbQueriesOnAccTransactionHeader.All(x => x.Item1.Contains("FROM dbo.AccTransactionHeader WITH (FORCESEEK, INDEX(FK_RX__AH_JH, PK_UC__AH_PK))")));
			}
		}

		public void TestSplitTransactionBatchOnCollectionByChequeBookParameter()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var accountingUtils = new AccountingUtils();
			AssertEquals(0, accountingUtils.SplitTransactionBatchOnCollectionByChequeBookParameter(new DummyBusinessObjectCollection(Factory), Factory).Count);

			var chequeBook1 = TestObjectCreator.GetAutoPrintChequeBook(Factory, 1, 200, 1);
			var chequeBook2 = TestObjectCreator.GetAutoPrintChequeBook(Factory, 200, 400, 200);
			var paymentApproval1 = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook1.BankAccount, chequeBook1);
			paymentApproval1.AV_OH = TestObjectCreator.AALSHI.PK;
			paymentApproval1.AV_Amount = 1000m;
			paymentApproval1.ChequeOrReference = "2";
			paymentApproval1.IsPostWithoutMatching = true;
			var paymentApproval2 = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook1.BankAccount, chequeBook1);
			paymentApproval2.AV_OH = TestObjectCreator.AALSHI.PK;
			paymentApproval2.AV_Amount = 1000m;
			paymentApproval2.ChequeOrReference = "3";
			paymentApproval2.IsPostWithoutMatching = true;
			var paymentApproval3 = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), ReceiptTypes.Cheque, chequeBook2.BankAccount, chequeBook2);
			paymentApproval3.AV_OH = TestObjectCreator.AALSHI.PK;
			paymentApproval3.AV_Amount = 1000m;
			paymentApproval3.ChequeOrReference = "201";
			paymentApproval3.IsPostWithoutMatching = true;

			paymentApproval1.CreateNewPaymentCore_ForTestOnly();
			paymentApproval2.CreateNewPaymentCore_ForTestOnly();
			paymentApproval3.CreateNewPaymentCore_ForTestOnly();

			AssertNotNull("Percondition", paymentApproval1.NewPayment);
			AssertNotNull("Percondition", paymentApproval2.NewPayment);
			AssertNotNull("Percondition", paymentApproval3.NewPayment);

			AssertEquals("Percondition", true, ((IChequeNumberAutoAllocation)paymentApproval1.NewPayment).IsAutoAllocationEnabled);
			AssertEquals("Percondition", true, ((IChequeNumberAutoAllocation)paymentApproval2.NewPayment).IsAutoAllocationEnabled);
			AssertEquals("Percondition", true, ((IChequeNumberAutoAllocation)paymentApproval3.NewPayment).IsAutoAllocationEnabled);

			var transactionCreatorHashtable = new TransactionCreatorHashtable();
			transactionCreatorHashtable.AddAPPaymentApproval(paymentApproval1, "CLIENT", paymentApproval1.BankAccount.AB_Code.ToString(), ReceiptTypes.Cheque, paymentApproval1.ChequeOrReference, "JOB");
			transactionCreatorHashtable.AddAPPaymentApproval(paymentApproval2, "CLIENT", paymentApproval2.BankAccount.AB_Code.ToString(), ReceiptTypes.Cheque, paymentApproval2.ChequeOrReference, "JOB");
			transactionCreatorHashtable.AddAPPaymentApproval(paymentApproval3, "CLIENT", paymentApproval3.BankAccount.AB_Code.ToString(), ReceiptTypes.Cheque, paymentApproval3.ChequeOrReference, "JOB");
			var result = accountingUtils.SplitTransactionBatchOnCollectionByChequeBookParameter(transactionCreatorHashtable, Factory);
			AssertEquals(2, result.Count);
			AssertContainsExactElementsInAnyOrder(new [] { paymentApproval3.NewPayment }, result.Single(x => x.Count == 1));
			AssertContainsExactElementsInAnyOrder(new [] { paymentApproval1.NewPayment, paymentApproval2.NewPayment }, result.Single(x => x.Count == 2));

			var paymentApprovalCollection = new APPaymentApprovalWithoutAuthorisationCollection(Factory);
			paymentApprovalCollection.Add(paymentApproval1);
			paymentApprovalCollection.Add(paymentApproval2);
			paymentApprovalCollection.Add(paymentApproval3);
			result = accountingUtils.SplitTransactionBatchOnCollectionByChequeBookParameter(paymentApprovalCollection, Factory);
			AssertEquals(2, result.Count);
			AssertContainsExactElementsInAnyOrder(new [] { paymentApproval3.NewPayment }, result.Single(x => x.Count == 1));
			AssertContainsExactElementsInAnyOrder(new [] { paymentApproval1.NewPayment, paymentApproval2.NewPayment }, result.Single(x => x.Count == 2));

			var transactionHeaderCollection = new TransactionHeaderCollection(Factory);
			transactionHeaderCollection.Add(paymentApproval1.NewPayment);
			transactionHeaderCollection.Add(paymentApproval2.NewPayment);
			transactionHeaderCollection.Add(paymentApproval3.NewPayment);
			result = accountingUtils.SplitTransactionBatchOnCollectionByChequeBookParameter(transactionHeaderCollection, Factory);
			AssertEquals(2, result.Count);
			AssertContainsExactElementsInAnyOrder(new [] { paymentApproval3.NewPayment }, result.Single(x => x.Count == 1));
			AssertContainsExactElementsInAnyOrder(new [] { paymentApproval1.NewPayment, paymentApproval2.NewPayment }, result.Single(x => x.Count == 2));
		}

		void AssertQueryContentsForMaxLengthCheck(Func<SQLComparisonOperator, ZString, ZDBOnlyQuery> methodToTest, SchemaStringColumn schemaColumn)
		{
			var query = methodToTest(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(schemaColumn.MaxLength));
			AssertEquals("is no result query", false, query.IsNoResultQuery);
			AssertContains(schemaColumn.Name, query.GetAsWhereClause(false));

			query = methodToTest(SQLComparisonOperator.StartsWith, TestObjectCreator.GetRandomString(schemaColumn.MaxLength + 1));
			AssertEquals("is no result query", true, query.IsNoResultQuery);
			AssertNotContains(schemaColumn.Name, query.GetAsWhereClause(false));
		}

		public void TestParseHex()
		{
			var result = AccountingUtils.ParseHex(null);
			AssertEquals(Array.Empty<byte>(), result);

			result = AccountingUtils.ParseHex("");
			AssertEquals(Array.Empty<byte>(), result);

			result = AccountingUtils.ParseHex("0123456789abcdef");
			AssertEquals(new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef }, result);

			var expectedBytes = Enumerable.Range(0, 255).Select(x => (byte)x).ToArray();
			var hexStringWithAllPossibleCombinations = string.Join("", Enumerable.Range(0, 255).Select(x => x.ToString("X2")));
			result = AccountingUtils.ParseHex(hexStringWithAllPossibleCombinations);
			AssertEquals(expectedBytes, result);

			var hexStringWithAllPossibleCombinationsLowerCase = hexStringWithAllPossibleCombinations.ToLowerInvariant();
			result = AccountingUtils.ParseHex(hexStringWithAllPossibleCombinations);
			AssertEquals(expectedBytes, result);
		}

		public void TestParseHex_Exceptions()
		{
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => AccountingUtils.ParseHex("0"));
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => AccountingUtils.ParseHex("yp"));
		}

		public void TestCollectRegistryInfoForOrganizationCreditLimitCheck()
		{
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(".Use Web Service for Credit Limit: No\r\n.Use Web Service for Outstanding Balance: No\r\n.Use Web Service for Transaction Payment Status: No\r\n.Use Web Service for Unposted Revenue: No\r\n.Enable Background Validation on Billing Tab: No", AccountingUtils.CollectRegistryInfoForOrganizationCreditLimitCheck());

			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(".Use Web Service for Credit Limit: Yes\r\n.Use Web Service for Outstanding Balance: Yes\r\n.Use Web Service for Transaction Payment Status: Yes\r\n.Use Web Service for Unposted Revenue: Yes\r\n.Enable Background Validation on Billing Tab: Yes", AccountingUtils.CollectRegistryInfoForOrganizationCreditLimitCheck());
		}

		public void TestShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting()
		{
			AssertEquals(false, AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals(true, AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice));
			}
		}

		[TestDate(2023, 03, 29)]
		public void TestGetGLJournalExchangeRateForPeriod()
		{
			var periodManager = new PeriodManager(Factory);
			var period = periodManager.CreateOnePeriod(202303, new ZDateTime(2023, 3, 1), new ZDateTime(2023, 3, 31), Factory);
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);

			AssertEquals("exRate updated to 1", 1m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.AUD.RX_Code, 202303));
			AssertEquals("exRate default is zero if there is no rate for the period.", 0m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.USD.RX_Code, 202303));

			TestObjectCreator.CreateUSDBuyRate(0.88m, period.AM_EndDate);
			Factory.Save();
			AssertEquals("exRate updated to Today's rate", 0.88m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.USD.RX_Code, 202303));
		}

		[TestDate(2023, 03, 29)]
		public void TestGetGLJournalExchangeRateForPostDate()
		{
			var gLJournalExchangeRateType = new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = ExchangeRateTypes.Code.BuyRate, ProfitAndLossAccountTypeExchangeRateType = ExchangeRateTypes.Code.SellRate };
			AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLJournalExchangeRateType);

			new TestObjectCreator(Factory).CreateTestPeriodsForEntireYear(2023);
			AssertEquals("exRate updated to 1", 1m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.AUD.RX_Code, 202303, ZDateTime.Empty));
			AssertEquals("exRate updated to 1", 1m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.AUD.RX_Code, 202303, ZDateTime.Now));
			AssertEquals("exRate default is zero if there is no rate for the period.", 0m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.USD.RX_Code, 202303, ZDateTime.Now));

			TestObjectCreator.CreateUSDBuyRate(0.88m, ZDateTime.Now);
			Factory.Save();
			AssertEquals("exRate updated to Today's rate", 0.88m, AccountingUtils.GetGLJournalExchangeRate(Factory, AccountType.BalanceSheetAccount, TestObjectCreator.USD.RX_Code, 202303, ZDateTime.Now));
		}

		public void TestAreDatesInTheSameCalendarMonth()
		{
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(null));
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(Array.Empty<ZDateTime>()));

			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(ZDateTime.Now));
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(ZDateTime.Empty));
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(ZDateTime.Invalid));
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1)));

			AssertEquals(false, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1), ZDateTime.Invalid));
			AssertEquals(false, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1), ZDateTime.Empty));
			AssertEquals(false, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1), new ZDateTime(2023, 4, 1)));
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1), new ZDateTime(2023, 3, 2)));

			AssertEquals(false, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1), new ZDateTime(2023, 3, 2), new ZDateTime(2023, 4, 1)));
			AssertEquals(true, AccountingUtils.AreDatesInTheSameCalendarMonth(new ZDateTime(2023, 3, 1), new ZDateTime(2023, 3, 2), new ZDateTime(2023, 3, 3)));
		}

		public void TestGetTransactionTypeList()
		{
			Action<CodeDescriptionPairList> assertDefaultElement = list =>
			{
				AssertEquals("All Transaction Types Code", "", list[0].Code);
				AssertEquals("All Transaction Types description", "All Transaction Types", list[0].Description);
			};

			Action<IList, string[]> assertTransactionTypeList = (list, codes) =>
			{
				assertDefaultElement((CodeDescriptionPairList)list);
				var listCodes = from CodeDescriptionPair pair in list
								where !string.IsNullOrEmpty(pair.Code)
								select pair.Code;
				AssertArrayEqualsByElements(codes, listCodes.ToArray());
			};

			string[] accountsReceivablePayableLedgerCodes =
			{
				TransactionTypes.AdjustmentNote,
				TransactionTypes.Contra,
				TransactionTypes.CreditNote,
				TransactionTypes.Discount,
				TransactionTypes.ExchangeDifference,
				TransactionTypes.Invoice,
				TransactionTypes.Journal,
				TransactionTypes.Overpayment,
				TransactionTypes.Payment,
				TransactionTypes.Receipt,
				TransactionTypes.Transfer
			};
			var typeList = AccountingUtils.GetTransactionTypeList(LedgerTypes.AccountsReceivable);
			assertTransactionTypeList(typeList, accountsReceivablePayableLedgerCodes);

			typeList = AccountingUtils.GetTransactionTypeList(LedgerTypes.AccountsPayable);
			assertTransactionTypeList(typeList, accountsReceivablePayableLedgerCodes);

			string[] cashBookLedgerCodes =
			{
				TransactionTypes.Transfer,
				TransactionTypes.ExchangeDifference,
				TransactionTypes.DirectPayment,
				TransactionTypes.DirectReceipt
			};
			typeList = AccountingUtils.GetTransactionTypeList(LedgerTypes.CashBook);
			assertTransactionTypeList(typeList, cashBookLedgerCodes);

			string[] jobCostingLedgerCodes =
			{
				TransactionTypes.Journal,
				TransactionLineTypes.WIP,
				TransactionLineTypes.Accrual,
				TransactionTypes.JobRevenueJournal
			};
			typeList = AccountingUtils.GetTransactionTypeList(LedgerTypes.JobCosting);
			assertTransactionTypeList(typeList, jobCostingLedgerCodes);

			string[] generalLedgerCodes =
			{
				TransactionTypes.GLStandardJournal,
				TransactionTypes.GLAutoJournal,
				TransactionTypes.GLReversingJournal,
				TransactionTypes.GLNoteJournal
			};
			typeList = AccountingUtils.GetTransactionTypeList(LedgerTypes.General);
			assertTransactionTypeList(typeList, generalLedgerCodes);

			var allDistinctCodes = accountsReceivablePayableLedgerCodes
				.Union(cashBookLedgerCodes)
				.Union(jobCostingLedgerCodes)
				.Union(generalLedgerCodes);
			typeList = AccountingUtils.GetTransactionTypeList("");
			assertTransactionTypeList(typeList, allDistinctCodes.ToArray());
		}

		public void TestEnableElectronicProcessingCharge()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");

			AssertEquals(false, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			AssertEquals(0, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Count);
			AssertEquals(false, AccountingUtils.EnableElectronicProcessingCharge(shipment));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			AssertEquals(0, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Count);
			AssertEquals(false, AccountingUtils.EnableElectronicProcessingCharge(shipment));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			var config = collection.AddNew();
			config.JobType = "SHP";
			config.StartDate = ZDate.Today.AddDays(-1);
			config.EndDate = ZDate.Today.AddDays(1);
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(false, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			Assert(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Cast<ElectronicProcessingChargeConfiguration>().Any(x => x.JobType == shipment.InvoicingSupporter.ConsumerType.Code && ZDateTime.Today >= x.StartDate && (x.EndDate.IsEmpty || ZDateTime.Today < x.EndDate.AddDays(1))));
			AssertEquals(false, AccountingUtils.EnableElectronicProcessingCharge(shipment));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(true, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			Assert(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Cast<ElectronicProcessingChargeConfiguration>().Any(x => x.JobType == shipment.InvoicingSupporter.ConsumerType.Code && ZDateTime.Today >= x.StartDate && (x.EndDate.IsEmpty || ZDateTime.Today < x.EndDate.AddDays(1))));
			AssertEquals(true, AccountingUtils.EnableElectronicProcessingCharge(shipment));
		}

		public void TestShouldShowRelatedDisbursementTransactions()
		{
			TestCase(CountryCodes.Australia, true, false);
			TestCase(CountryCodes.Australia, false, false);

			TestCase(CountryCodes.KoreaSouth, true, true);
			TestCase(CountryCodes.KoreaSouth, false, false);

			void TestCase(string countryCode, bool enableEInvoicingFunctionality, bool expectedResult)
			{
				using (TestObjectCreator.SetUpForTestingEInvoicingAndAutomaticllyAppendingDisbursementFeesSummary(countryCode, enableEInvoicingFunctionality))
				{
					AssertEquals($"Country: {countryCode}, EnableEInvoicingFunctionality: {enableEInvoicingFunctionality}, ExpectedResult should be {expectedResult}", expectedResult, AccountingUtils.ShouldShowRelatedDisbursementTransactions());
				}
			}
		}

		public void TestIsEDWEnabled()
		{
			Assert(AccountingUtils.IsEDWEnabled());
			using(AccountingUtils.TemporarilySetDataWarehouseServerToNull())
			{
				Assert(!AccountingUtils.IsEDWEnabled());
			}
		}

		[TestDate(2025, 4, 18, 00, 00, 0)]
		public void TestAddTransactionNumInfoNotification()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, -200);
			transaction.AH_InvoiceDate = new ZDateTime(2023, 12, 1);
			transaction.Validation.ValidateAH_TransactionNum();
			transaction.AH_TransactionType = "IPA";
			AssertNoErrors(transaction.AH_TransactionNumInfo);
			Factory.Save();

			var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("DEF", org, 500);
			transaction2.AH_InvoiceDate = new ZDateTime(2024, 12, 1);
			transaction2.AH_TransactionType = "IPA";
			transaction2.Validation.ValidateAH_TransactionNum();
			AssertNoErrors(transaction2.AH_TransactionNumInfo);
			Factory.Save();

			var transaction3 = TestObjectCreator.CreateTransactionPendingAllocation("GHI", org, 300);
			transaction3.AH_InvoiceDate = new ZDateTime(2025, 1, 10);
			transaction3.AH_TransactionType = "IPA";
			transaction3.Validation.ValidateAH_TransactionNum();
			AssertNoErrors(transaction3.AH_TransactionNumInfo);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AllowDuplicateInvoiceNumberRule.STD))
			{
				AssertTransactionNumInfo("ABC", false);
				AssertTransactionNumInfo("DEF", true);
				AssertTransactionNumInfo("GHI", true);
			}
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AllowDuplicateInvoiceNumberRule.CAL))
			{
				AssertTransactionNumInfo("ABC", false);
				AssertTransactionNumInfo("DEF", false);
				AssertTransactionNumInfo("GHI", true);
			}
			CargoWise.Common.ErrorReporter.Clear();

			void AssertTransactionNumInfo(string transactionNumber, Boolean hasError)
			{
				var transactionTest = TestObjectCreator.CreateTransactionPendingAllocation(transactionNumber, org, 400);
				transactionTest.AH_InvoiceDate = ZDateTime.Today;
				transactionTest.AH_TransactionType = "IPA";
				transactionTest.Validation.ValidateAH_TransactionNum();

				var previousInvoiceDetails = AccountingUtils.PATransactionNumberExists(TransactionTypes.Invoice, transactionNumber, org.PK, ZGuid.Empty, ZDateTime.Today);
				Assert("HasNotification", previousInvoiceDetails.HasNotification);
				AccountingUtils.AddTransactionNumInfoNotification(previousInvoiceDetails, "The transaction number is already in use. Please select another one.", transactionTest);
				if (hasError)
				{
					AssertHasErrors("Error should be reported", transactionTest.AH_TransactionNumInfo);
				}
				else
				{
					Assert("Warning should be reported", transactionTest.AH_TransactionNumInfo.GetWarnings().Count() > 0);
					Assert("No errors should be reported", !transactionTest.AH_TransactionNumInfo.HasErrors());
				}
			}
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		TransactionHeaderCollection GetTransactionsForComparingTest()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 6;
			AccChequeBook testCheques = Factory.NewWithValidTestData<AccChequeBook>();
			testCheques.AK_StartNo = 1;
			testCheques.AK_LastNo = 100;
			testCheques.AK_CurrentNo = 1;
			testCheques.AK_AB = testBank.PK;

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			testAPInv.AH_LocalExTaxAmount = 94M;
			testAPInv.AH_OSExTaxAmount = 94M;
			testAPInv.AH_Desc = "For Payment Approval 1";

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			testOrg2.CompanyData.OB_IsCreditor = true;
			testOrg2.APSettlementGroupPK = testOrg2.PK;

			OrgHeader testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			testOrg3.CompanyData.OB_IsCreditor = true;
			testOrg3.APSettlementGroupPK = ZGuid.Empty;

			APInvoice testAPInv2 = Factory.NewWithValidTestData<APInvoice>();
			testAPInv2.AH_OH = testOrg2.PK;
			testAPInv2.AH_LocalExTaxAmount = 94M;
			testAPInv2.AH_OSExTaxAmount = 94M;
			testAPInv2.AH_Desc = "For Payment Approval 2";

			APReceipt testAPRec = Factory.NewWithValidTestData<APReceipt>();
			testAPRec.AH_OH = testOrg3.PK;
			testAPRec.AH_LocalExTaxAmount = 94M;
			testAPRec.AH_OSExTaxAmount = 94M;
			testAPRec.AH_Desc = "For Payment Approval 3";

			Factory.Save();

			TransactionHeaderCollection transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(testAPInv);
			transactions.Add(testAPInv2);
			transactions.Add(testAPRec);
			return transactions;
		}

		ZBool CollectionIsSorted(TransactionHeaderCollection transactions)
		{
			TransactionHeader previousTransaction = null;
			foreach (TransactionHeader transaction in transactions)
			{
				if (previousTransaction != null)
				{
					if (previousTransaction.OH_APSettlementGroup != ZGuid.Empty && transaction.OH_APSettlementGroup != ZGuid.Empty && previousTransaction.OH_APSettlementGroup != transaction.OH_APSettlementGroup)
					{
						if (previousTransaction.OH_APSettlementGroup > transaction.OH_APSettlementGroup)
						{
							return ZBool.False;
						}
					}
					else if (previousTransaction.OH_APSettlementGroup == ZGuid.Empty && transaction.OH_APSettlementGroup == ZGuid.Empty && previousTransaction.AH_OH != transaction.AH_OH)
					{
						if (previousTransaction.AH_OH > transaction.AH_OH)
						{
							return ZBool.False;
						}
					}
				}
				previousTransaction = transaction;
			}
			return ZBool.True;
		}

		TransactionHeaderCollection MakeCollectionToBeUnsorted(TransactionHeaderCollection transactions)
		{
			if (CollectionIsSorted(transactions))
			{
				TransactionHeaderCollection newCollection = new TransactionHeaderCollection(Factory);
				for (int i = transactions.Count - 1; i > -1; i--)
				{
					newCollection.Add(transactions[i]);
				}
				return newCollection;
			}
			return transactions;
		}

		#endregion
	}
}
