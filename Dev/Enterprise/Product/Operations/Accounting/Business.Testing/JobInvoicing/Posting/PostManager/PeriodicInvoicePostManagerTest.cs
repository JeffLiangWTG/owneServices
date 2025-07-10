using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class PeriodicInvoicePostManagerTest : TransactionCreatorBaseTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			OrgInvoiceType invoiceType = LocalClient.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_RS_NKServiceLevel = "STD";
			invoiceType.DeferredCharges.AddNew().PO_AC = CC1.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC2.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC3.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC4.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC5.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC6.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = TestObjectCreator.RevenueChargeCode.PK;

			invoiceType = Agent.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_RS_NKServiceLevel = "STD";
			invoiceType.DeferredCharges.AddNew().PO_AC = CC1.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC2.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC3.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC4.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC5.PK;
			invoiceType.DeferredCharges.AddNew().PO_AC = CC6.PK;
		}

		#region RoundingForJapan

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestRoundChargeAmountsForJapan()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYen);

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKDestination = "JPAAM";
				shipment.JS_RL_NKOrigin = "USLAX";

				Job job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

				CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

				RefCurrency jPY = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");

				CreateCharge(job, CC1, "Charge Code 1", jPY, 100M, Creditor1, jPY, 150M, LocalClient);
				CreateCharge(job, CC1, "Charge Code 1", jPY, 100M, Creditor1, jPY, 150M, LocalClient);

				job.Charges[0].JR_InvoiceType = job.Charges[1].JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				job.Charges[0].JR_LocalSellAmt = 50;
				job.Charges[1].JR_LocalSellAmt = 17;

				Factory.Save();

				ZString expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
				ZByte expectedInvoiceTermDays = 3;
				ZDateTime expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
				ZDateTime expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
				ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-1);

				PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.DebtorPK = LocalClient.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				periodicInvoice.InvoiceDate = expectedInvoiceDate;
				periodicInvoice.DueDate = expectedDueDate;
				periodicInvoice.PostDate = expectedPostDate;
				periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
				periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;
				periodicInvoice.Jobs.Add(job);
				periodicInvoice.LoadJobs();

				PeriodicInvoicePostManager postManager = new PeriodicInvoicePostManager(periodicInvoice);
				TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

				AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

				AssertEquals(53m, job.Charges[0].JR_LocalSellAmt);
				AssertEquals(17m, job.Charges[1].JR_LocalSellAmt);

				InvoicingBase[] invoices = postManager.Poster.GetInvoices(jPY, LocalClient);
				AssertEquals(1, invoices.Length);

				ARInvoice invoice = (ARInvoice)invoices[0];
				AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
				AssertEquals(53m, invoice.Lines[0].AL_LocalExTaxAmount);
				AssertEquals(17m, invoice.Lines[1].AL_LocalExTaxAmount);
			}
		}

		#endregion

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostInvoiceUpdateSellReference()
		{
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			var job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			var charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			var charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, Agent);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			charge1.JR_SellReference = "reference 1";
			charge2.JR_SellReference = ZString.Empty;

			Factory.Save();

			var expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			var expectedInvoiceTermDays = new ZByte(3);
			var expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			var expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			var expectedPostDate = ZDateTime.Now.AddDays(-1);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = AUD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;
			var testSellReference = "Set Sell Reference";
			periodicInvoice.SellReference = testSellReference;

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			var transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);

			var agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(2, agentAUDInvoices.Length);
			Assert(periodicInvoice.Charges.Any(x => x.JR_SellReference == testSellReference));
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostInvoiceWithTaxBranchAndDueDate()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testBranch1 = TestObjectCreator.CreateBranch("001", GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			var job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			var charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			var charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, Agent);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			charge1.JR_GB_SellTaxBranch = testBranch1.PK;
			charge2.JR_GB_SellTaxBranch = testBranch1.PK;

			Factory.Save();

			var expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			var expectedInvoiceTermDays = new ZByte(3);
			var expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			var expectedDueDate = ZDateTime.Now.AddDays(10);
			var expectedPostDate = ZDateTime.Now.AddDays(-1);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = AUD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.TaxBranch = testBranch1.PK;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;
			periodicInvoice.DueDate = expectedDueDate;

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);

			var agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			AssertEquals(testBranch1.PK, agentAUDInvoices[0].AH_GB_TaxBranch);
			AssertEquals(expectedDueDate, agentAUDInvoices[0].AH_DueDate);
			AssertEquals(testBranch1.PK, agentAUDInvoices[0].Lines[0].AL_GB_TaxBranch);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostInvoiceTaxBranchRelyOnJobChargeNotPeriodicInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var testBranch1 = TestObjectCreator.CreateBranch("001", GlbCompany.CurrentCompany);
			var testBranch2 = TestObjectCreator.CreateBranch("002", GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			var job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			var charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			var charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, Agent);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			charge1.JR_GB_SellTaxBranch = testBranch1.PK;
			charge2.JR_GB_SellTaxBranch = testBranch1.PK;

			Factory.Save();

			var expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			var expectedInvoiceTermDays = new ZByte(3);
			var expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			var expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			var expectedPostDate = ZDateTime.Now.AddDays(-1);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = AUD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.TaxBranch = testBranch1.PK;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			var newFactory = new BusinessObjectFactory();
			var charge11 = newFactory.Load<Charge>(charge1.PK);
			var charge22 = newFactory.Load<Charge>(charge2.PK);
			charge11.JR_GB_SellTaxBranch = testBranch2.PK;
			charge22.JR_GB_SellTaxBranch = testBranch2.PK;
			newFactory.Save();

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);

			var agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);

			AssertEquals(testBranch2.PK, agentAUDInvoices[0].AH_GB_TaxBranch);
			AssertEquals(testBranch2.PK, agentAUDInvoices[0].Lines[0].AL_GB_TaxBranch);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostInvoiceUpdateSIVProcessTasksForJobs()
		{
			Guid processTaskTemplatePK = new Guid("365923A7-9B8B-481D-A5E5-33BD2232C20D");
			Guid processTaskPK = new Guid("C1964E5B-BF2B-4A6F-8544-DC263268747E");

			string sQL = string.Format(@"INSERT INTO dbo.ProcessTaskTemplate
(
	P0_PK,
	P0_DischargePortCountry,
	P0_IsActive,
	P0_IsSystem,
	P0_LoadPortCountry,
	P0_OrgAssessmentOrder,
	P0_ProcessType,
	P0_SubType1,
	P0_SubType2,
	P0_SubType3,
	P0_SubType4,
	P0_Subtype5,
	P0_Name,
	P0_SystemCreateTimeUtc,
	P0_SystemCreateUser,
	P0_SystemLastEditTimeUtc,
	P0_SystemLastEditUser
)
VALUES
(
	'{0}',
	'',
	1,
	0,
	'',
	'',
	'SHP',
	'',
	'',
	'',
	'',
	'',
	'Really?',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)", processTaskTemplatePK.ToString());
			DbCommand cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();

			sQL = string.Format(@"INSERT INTO dbo.ProcessTasks
(
	P9_PK,
	P9_AndOr,
	P9_Condition1,
	P9_Condition2,
	P9_Condition2Value,
	P9_Description,
	P9_EstimatedDefaultedFrom,
	P9_EstimatedDefaultFromPredecessor,
	P9_GS_NKAssignedStaffMember,
	P9_IsCalendarItem,
	P9_IsPublished,
	P9_ParentID,
	P9_ParentTableCode,
	P9_ReferencedTableCode,
	P9_RN_NKDestinationCountry,
	P9_RN_NKOriginCountry,
	P9_SE_NKExceptionEvent,
	P9_SE_NKMilestoneEvent,
	P9_SE_NKTaskCompletionEvent,
	P9_Sequence,
	P9_Status,
	P9_TaskCannotBeDeleted,
	P9_TaskID,
	P9_TriggerField,
	P9_Type,
	P9_SystemCreateTimeUtc,
	P9_SystemCreateUser,
	P9_SystemLastEditTimeUtc,
	P9_SystemLastEditUser
)
VALUES
(
	'{0}',
	'',
	'',
	'',
	'',
	'try send siv',
	'',
	0,
	'',
	0,
	1,
	'{1}',
	'P0',
	'',
	'',
	'',
	'',
	'SIV',
	'',
	1,
	'OPN',
	0,
	'T00001001',
	'',
	'TRG',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)", processTaskPK.ToString(), processTaskTemplatePK.ToString());
			cmd = Db.Connection.Command(sQL);
			cmd.ExecuteNonQuery();

			var shipment1 = TestObjectCreator.CreateShipment("Z00001000");
			Job job1 = TestObjectCreator.CreateJob(shipment1, LocalClient, 5M, Agent, 10M);

			ZGuid job1PK = job1.PlugInData.PK;
			Charge charge1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			Charge charge2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, Agent);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var shipment2 = TestObjectCreator.CreateShipment("Z00001001");
			Job job2 = TestObjectCreator.CreateJob(shipment2, LocalClient, 5M, Agent, 10M);

			ZGuid job2PK = job2.PlugInData.PK;
			Charge charge3 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 100M, Creditor3, AUD, 150M, Agent);
			Charge charge4 = CreateCharge(job2, CC4, "Charge Code 4", AUD, 200M, Creditor3, AUD, 200M, Agent);
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			ZQuery query = new ZQuery(ProcessTasksSchema.P9_ParentID, job1PK);
			query.AddToFilter(ProcessTasksSchema.P9_Type, "TRG");
			ProcessTask[] tasks = Factory.Load<ProcessTask>(query);
			AssertEquals("length should be 1", 1, tasks.Length);
			AssertEquals("Precondition - P9_ActualDate is empty", ZDateTime.Empty, tasks[0].P9_ActualDate.ToZDateTime());
			AssertEquals("Precondition - P9_Status is OPEN", "OPN", tasks[0].P9_Status);

			query = new ZQuery(ProcessTasksSchema.P9_ParentID, job2PK);
			query.AddToFilter(ProcessTasksSchema.P9_Type, "TRG");
			tasks = Factory.Load<ProcessTask>(query);
			AssertEquals("length should be 1", 1, tasks.Length);
			AssertEquals("Precondition - P9_ActualDate is empty", ZDateTime.Empty, tasks[0].P9_ActualDate.ToZDateTime());
			AssertEquals("Precondition - P9_Status is OPEN", "OPN", tasks[0].P9_Status);

			ZString expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			ZByte expectedInvoiceTermDays = 3;
			ZDateTime expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			ZDateTime expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-1);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;
			periodicInvoice.Jobs.Add(job1);
			periodicInvoice.Jobs.Add(job2);
			periodicInvoice.LoadJobs();

			PeriodicInvoicePostManager creator = new PeriodicInvoicePostManager(periodicInvoice);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();
			query = new ZQuery(ProcessTasksSchema.P9_ParentID, job1PK);
			query.AddToFilter(ProcessTasksSchema.P9_Type, "TRG");
			tasks = Factory.Load<ProcessTask>(query);

			AssertEquals("length should be 1", 1, tasks.Length);
			AssertEquals("P9_ActualDate should be updated", expectedDueDate, tasks[0].P9_ActualDate.ToZDateTime());
			AssertEquals("P9_Status should be CLOSED", "CLS", tasks[0].P9_Status);

			query = new ZQuery(ProcessTasksSchema.P9_ParentID, job2PK);
			query.AddToFilter(ProcessTasksSchema.P9_Type, "TRG");
			tasks = Factory.Load<ProcessTask>(query);

			AssertEquals("length should be 1", 1, tasks.Length);
			AssertEquals("P9_ActualDate should be updated", expectedDueDate, tasks[0].P9_ActualDate.ToZDateTime());
			AssertEquals("P9_Status should be CLOSED", "CLS", tasks[0].P9_Status);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestPostAndReverseInvoiceAddSIVEventLog()
		{
			var shipment1 = TestObjectCreator.CreateShipment("Z00001000");
			var job1 = TestObjectCreator.CreateJob(shipment1, LocalClient, 5M, Agent, 10M);
			var charge1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, Agent);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var shipment2 = TestObjectCreator.CreateShipment("Z00001001");
			var job2 = TestObjectCreator.CreateJob(shipment2, LocalClient, 5M, Agent, 10M);
			var charge2 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 100M, Creditor3, AUD, 150M, Agent);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.Jobs.Add(job1);
			periodicInvoice.Jobs.Add(job2);
			periodicInvoice.LoadJobs();

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			var transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Precondition: Should post 1 invoice", 1, creator.Poster.PostedInvoices.Count);
			var postedInvoice = creator.Poster.PostedInvoices[0];
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, job1.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			StmALog[] retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("1 log found for job Z00001000", 1, retrievedLogs.Count(x => x.SL_Reference == "Z00001000"));

			filter = new ZQuery(StmALogSchema.SL_Parent, job2.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("1 log found for job Z00001001 ", 1, retrievedLogs.Count(x => x.SL_Reference == "Z00001001"));

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(postedInvoice);
			reversing.Reverse();
			Factory.Save();

			filter = new ZQuery(StmALogSchema.SL_Parent, job1.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("2 log found for job Z00001000", 2, retrievedLogs.Count(x => x.SL_Reference == "Z00001000"));

			filter = new ZQuery(StmALogSchema.SL_Parent, job2.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code);
			retrievedLogs = Factory.Load<StmALog>(filter);
			AssertEquals("2 log found for job Z00001001 ", 2, retrievedLogs.Count(x => x.SL_Reference == "Z00001001"));
		}

		public void TestReversingPeriodicInvoiceWithMultipleJobsShouldCopyTheCorrectTransactionCategory()
		{
			var job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.PlugInData = TestObjectCreator.CreateShipment(job1.JH_JobNum);
			var charge1 = CreateCharge(job1, CC1, "charge2", USD, 200M, Creditor2, USD, 200M, Agent);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;

			var job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.PlugInData = TestObjectCreator.CreateShipment(job2.JH_JobNum);
			var charge2 = CreateCharge(job2, CC1, "charge4", USD, 200M, Creditor2, USD, 400M, Agent);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = USD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			periodicInvoice.Jobs.AddRange(new[] { job1, job2 });
			periodicInvoice.LoadJobs();
			periodicInvoice.Jobs.AddRange(new[] { job1, job2 });

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();

			var invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(1, invoices.Length);
			AssertEquals("USD", invoices[0].AH_RX_NKTransactionCurrency);
			AssertEquals(InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, invoices[0].AH_TransactionCategory);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoices[0]);
			reversing.Reverse();
			var reversedTransaction = reversing.ReverseTransaction as InvoicingBase;
			AssertNotNull(reversedTransaction);
			AssertEquals(InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching, reversedTransaction.AH_TransactionCategory);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesAllRevenueChargesBillInLocalCurrency()
		{
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			Job job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			ExchangeRate rate1 = CreateExchangeRate(job, USD, 0.63M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, 0.38M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);

			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			ZString expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			ZByte expectedInvoiceTermDays = 3;
			ZDateTime expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			ZDateTime expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-1);

			#region AUD Agent Invoice

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			PeriodicInvoicePostManager creator = new PeriodicInvoicePostManager(periodicInvoice);
			//AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			ARInvoice agentAUDInvoice = (ARInvoice)agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				1716.84M, 48.50M, 41.75M, 1765.34M, AUD, 1, expectedPostDate, ZBool.False, Agent, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = agentAUDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 881.83M, GSTFREE1, 0M, WHTFREE1, 0M, 881.83M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentAUDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 485.01M, GST1, 48.50M, WHT1, 24.25M, 533.51M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			//AssertInvoicesContainConsolidatedInvoiceRef(Creator.Poster.PostedInvoices, "Z00001000");

			#endregion

			#region AUD Local Client Invoice

			periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = LocalClient.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Line Count", 3, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				696.26M, 69.63M, 10.00M, 765.89M, AUD, 1M, expectedPostDate, ZBool.False, LocalClient, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150M, GST1, 15M, WHTFREE1, 0M, 165M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200M, GST1, 20M, WHT1, 10M, 220M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC5Line = localClientAUDInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 346.26M, GST1, 34.63M, WHTFREE1, 0M, 380.89M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesAllRevenueChargesBillInForeignCurrency()
		{
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			Job job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, GBP, .4M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge3 = CreateCharge(job, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge4 = CreateCharge(job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge5 = CreateCharge(job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge6 = CreateCharge(job, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);

			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			charge5.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			charge6.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;

			Factory.Save();

			AccTransactionLines charge1WIP = charge1.WIP;
			AccTransactionLines charge2WIP = charge2.WIP;
			AccTransactionLines charge3WIP = charge3.WIP;
			AccTransactionLines charge4WIP = charge4.WIP;
			AccTransactionLines charge5WIP = charge5.WIP;
			AccTransactionLines charge6WIP = charge6.WIP;

			AccTransactionLines charge1Accrual = charge1.Accrual;
			AccTransactionLines charge2Accrual = charge2.Accrual;
			AccTransactionLines charge3Accrual = charge3.Accrual;
			AccTransactionLines charge5Accrual = charge5.Accrual;
			AccTransactionLines charge6Accrual = charge6.Accrual;

			ZString expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			ZByte expectedInvoiceTermDays = 3;
			ZDateTime expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			ZDateTime expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-1);

			#region AUD Agent Invoice

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = AUD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			PeriodicInvoicePostManager creator = new PeriodicInvoicePostManager(periodicInvoice);
			TransactionCreatorHashtable transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Line Count", 1, agentAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				350.00M, 0M, 17.50M, 350.00M, AUD, 1M, expectedPostDate, ZBool.False, Agent, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			TransactionLine cC3Line = agentAUDInvoice.FindTransactionLine("REV", CC3, job.PK);
			AssertTransactionLineValues(cC3Line, "REV", 3, "Charge Code 3", 350M, GSTFREE1, 0M, WHT1, 17.50M, 350.00M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, job, CC3, CC3.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC3Line);

			//AssertInvoicesContainConsolidatedInvoiceRef(Creator.Poster.PostedInvoices, "Z00001000");

			#endregion

			#region USD Agent Invoice

			periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = USD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			creator = new PeriodicInvoicePostManager(periodicInvoice);
			transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			InvoicingBase[] agentUSDInvoices = creator.Poster.GetInvoices(USD, Agent);
			AssertEquals(1, agentUSDInvoices.Length);
			InvoicingBase agentUSDInvoice = agentUSDInvoices[0];
			AssertEquals("Invoice Line Count", 2, agentUSDInvoice.Lines.Count);

			AssertTransactionHeaderValues(agentUSDInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				1107.15M, 39.29M, 19.64M, 802.50M, USD, 0.699993M, expectedPostDate, ZBool.False, Agent, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentUSDInvoice);

			TransactionLine cC4Line = agentUSDInvoice.FindTransactionLine("REV", CC4, job.PK);
			AssertTransactionLineValues(cC4Line, "REV", 4, "Charge Code 4", 714.29M, GSTFREE1, 0M, WHTFREE1, 0M, 500.00M, USD, 0.699993M, expectedPostDate,
				ZBool.False, agentUSDInvoice, job, CC4, CC4.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC4Line);

			TransactionLine cC6Line = agentUSDInvoice.FindTransactionLine("REV", CC6, job.PK);
			AssertTransactionLineValues(cC6Line, "REV", 6, "Charge Code 6", 392.86M, GST1, 39.29M, WHT1, 19.64M, 302.50M, USD, 0.699993M, expectedPostDate,
				ZBool.False, agentUSDInvoice, job, CC6, CC6.RevenueAccount, Agent);
			AssertTransactionLineDefaults(cC6Line);

			//AssertInvoicesContainConsolidatedInvoiceRef(Creator.Poster.PostedInvoices, "Z00001000/A");

			#endregion

			#region Local Client AUD Invoice

			periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = LocalClient.PK;
			periodicInvoice.CurrencyNK = AUD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 2, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				350.00M, 35.00M, 10.00M, 385.00M, AUD, 1, expectedPostDate, ZBool.False, LocalClient, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150.00M, GST1, 15.00M, WHTFREE1, 0M, 165.00M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200.00M, GST1, 20.00M, WHT1, 10.00M, 220.00M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			//AssertInvoicesContainConsolidatedInvoiceRef(Creator.Poster.PostedInvoices, "Z00001000/B");

			#endregion

			#region Local Client GBP Invoice

			periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = LocalClient.PK;
			periodicInvoice.CurrencyNK = GBP.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			creator = new PeriodicInvoicePostManager(periodicInvoice);
			transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Payables Transaction Count", 0, transactions.APTransactionsCount);

			InvoicingBase[] localClientGBPInvoices = creator.Poster.GetInvoices(GBP, LocalClient);
			AssertEquals(1, localClientGBPInvoices.Length);
			InvoicingBase localClientGBPInvoice = localClientGBPInvoices[0];
			AssertEquals("Invoice Lines Count", 1, localClientGBPInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientGBPInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				312.50M, 31.25M, 0M, 137.50M, GBP, .4M, expectedPostDate, ZBool.False, LocalClient, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientGBPInvoice);

			TransactionLine cC5Line = localClientGBPInvoice.FindTransactionLine("REV", CC5, job.PK);
			AssertTransactionLineValues(cC5Line, "REV", 5, "Charge Code 5", 312.50M, GST1, 31.25M, WHTFREE1, 0M, 137.50M, GBP, .4M, expectedPostDate,
				ZBool.False, localClientGBPInvoice, job, CC5, CC5.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC5Line);

			//AssertInvoicesContainConsolidatedInvoiceRef(Creator.Poster.PostedInvoices, "Z00001000/C");

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", true, charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", true, charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", true, charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", true, charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", true, charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", true, charge6WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1 Accrual Reversed", false, charge1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 Accrual Reversed", false, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 Accrual Reversed", false, charge3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", false, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 Accrual Reversed", false, charge6Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		public void TestCancelPosting()
		{
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			Job job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = LocalClient.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();

			PeriodicInvoicePostManager creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.SetCancelPostingForTestOnly(true);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Shouldn't be any transactions posted", 0, creator.Poster.PostedInvoices.Count);

			creator.SetCancelPostingForTestOnly(false);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Assert("Shouldn't be any transactions posted", creator.Poster.PostedInvoices.Count > 0);
		}

		public void TestCannotPostARInvoicesFromJobWithEmptyProfitLossReason()
		{
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			Job job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);
			job.JH_ProfitLossReasonCode = "";

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);
			var value = AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (CodeDescriptionBool item in value)
			{
				AssertEquals("Is status changed to Invoiced on posting first AR Invoice when Job Status: " + item.Code, false, item.Bool);
			}

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = LocalClient.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();

			PeriodicInvoicePostManager creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.OnCriticalPostError += new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);
			IsOnCriticalPostErrorEventRaised = false;
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Shouldn't be any transactions posted", 0, creator.Poster.PostedInvoices.Count);
			Assert("Posting should be cancelled", creator.CancelPosting);
			Assert("Critical Post Error Event should be raised", IsOnCriticalPostErrorEventRaised);
			creator.OnCriticalPostError -= new EventHandler<CriticalPostingErrorEventArgs>(TestPostManager_OnCriticalPostError);
		}

		bool IsOnCriticalPostErrorEventRaised;
		void TestPostManager_OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			IsOnCriticalPostErrorEventRaised = true;
			if (e is CriticalJobPostingErrorEventArgs)
			{
				AssertHasRowError("RowError should be as expected", ((CriticalJobPostingErrorEventArgs)e).Jobs[0], string.Format("Job {0} status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices.", ((CriticalJobPostingErrorEventArgs)e).Jobs[0].JH_JobNum));
			}
			else
			{
				Fail(string.Format("Expected CriticalJobPostingErrorEventArgs argument but was {0}", e.GetType()));
			}
		}

		public void TestMiscInvoicesPostingWithOtherTaxes()
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			var chargeCode = TestObjectCreator.RevenueChargeCode;
			var org = LocalClient;
			taxTestHelper.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsReceivable);

			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			var job = TestObjectCreator.CreateJob(shipment, org, 5M, Agent, 10M);

			var charge = CreateCharge(job, chargeCode, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, org);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", AUD, 1M);
			invoice.AH_OH = org.PK;
			invoice.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice, AUD, 1M, 50M, 0M, 0M, chargeCode.PK);
			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory)
			{
				DebtorPK = org.PK,
				InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching
			};
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.LoadMiscInvoices();

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);
			var numberOfLinesInInvoiceForPassedTaxProcessing = 0;
			taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("").Callback((ITaxRecordParent taxParent) => numberOfLinesInInvoiceForPassedTaxProcessing = taxParent.GetLines().Count);

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals(1, creator.Poster.PostedInvoices.Count);
			AssertEquals("Invoice Lines Count", 2, creator.Poster.PostedInvoices[0].Lines.Count);
			AssertEquals("Invoice Lines Count for tax processing", 2, numberOfLinesInInvoiceForPassedTaxProcessing);
		}

		public void TestMiscInvoicesPosting()
		{
			var shipment = TestObjectCreator.CreateShipment("Z00001000");
			Job job = TestObjectCreator.CreateJob(shipment, LocalClient, 5M, Agent, 10M);

			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			InvoicingBase invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", AUD, 1M);
			invoice1.AH_OH = LocalClient.PK;
			invoice1.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice1, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice1, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			InvoicingBase invoice2 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV1", AUD, 1M);
			invoice2.AH_OH = Agent.PK;
			invoice2.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice2, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice2, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			InvoicingBase invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", AUD, 1M);
			invoice3.AH_OH = Agent.PK;
			invoice3.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice3, AUD, 1M, 80M, 0M, 0M, TestObjectCreator.GLHeader2.PK);

			Factory.Save();

			ZString expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			ZByte expectedInvoiceTermDays = 3;
			ZDateTime expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			ZDateTime expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-1);

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = LocalClient.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.LoadMiscInvoices();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			PeriodicInvoicePostManager creator = new PeriodicInvoicePostManager(periodicInvoice);
			creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals(1, creator.Poster.PostedInvoices.Count);

			InvoicingBase[] localClientAUDInvoices = creator.Poster.GetInvoices(AUD, LocalClient);
			AssertEquals(1, localClientAUDInvoices.Length);
			InvoicingBase localClientAUDInvoice = localClientAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 4, localClientAUDInvoice.Lines.Count);

			AssertTransactionHeaderValues(localClientAUDInvoice, "AR", "INV", null, "AR PERIODIC INVOICE", expectedInvoiceDate, expectedDueDate,
				450.00M, 35.00M, 10.00M, 485.00M, AUD, 1, expectedPostDate, ZBool.False, LocalClient, job,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(localClientAUDInvoice);

			TransactionLine cC1Line = localClientAUDInvoice.FindTransactionLine("REV", CC1, job.PK);
			AssertTransactionLineValues(cC1Line, "REV", 1, "Charge Code 1", 150.00M, GST1, 15.00M, WHTFREE1, 0M, 165.00M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC1, CC1.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC2Line = localClientAUDInvoice.FindTransactionLine("REV", CC2, job.PK);
			AssertTransactionLineValues(cC2Line, "REV", 2, "Charge Code 2", 200.00M, GST1, 20.00M, WHT1, 10.00M, 220.00M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, job, CC2, CC2.RevenueAccount, LocalClient);
			AssertTransactionLineDefaults(cC2Line);

			TransactionLine cC3Line = localClientAUDInvoice.Lines[2];
			AssertTransactionLineValues(cC3Line, "REV", 3, "", 50.00M, null, 0.00M, null, 0.00M, 50.00M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, null, null, TestObjectCreator.GLHeader1, LocalClient);
			AssertTransactionLineDefaults(cC3Line);

			TransactionLine cC4Line = localClientAUDInvoice.Lines[3];
			AssertTransactionLineValues(cC4Line, "REV", 4, "", 50.00M, null, 0.00M, null, 0.00M, 50.00M, AUD, 1, expectedPostDate,
				ZBool.False, localClientAUDInvoice, null, null, TestObjectCreator.GLHeader1, LocalClient);
			AssertTransactionLineDefaults(cC4Line);

			//AssertInvoicesContainConsolidatedInvoiceRef(Creator.Poster.PostedInvoices, "Z00001000");
			Assert("The Invoice must be reversed.", invoice1.AH_IsCancelled);

			periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.Add(job);
			periodicInvoice.LoadJobs();
			periodicInvoice.LoadMiscInvoices();
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;

			creator = new PeriodicInvoicePostManager(periodicInvoice);
			var postedTransactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals(1, creator.Poster.PostedInvoices.Count);
			AssertContainsExactElementsInAnyOrder(postedTransactions.GetAllARTransactions(), creator.Poster.PostedInvoices);

			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 3, agentAUDInvoice.Lines.Count);
			AssertEquals("Performance: GetSumOfLines call count", 5, agentAUDInvoice.GetSumOfLinesCallAmount_ForTestOnly);

			AssertTransactionHeaderValues(agentAUDInvoice, "AR", "CRD", null, "AR CREDIT NOTE", expectedInvoiceDate, expectedDueDate,
				-70.00M, 0.00M, 0.00M, -70.00M, AUD, 1, expectedPostDate, ZBool.False, Agent, null,
				expectedInvoiceTerm, expectedInvoiceTermDays, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(agentAUDInvoice);

			cC1Line = agentAUDInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AG == TestObjectCreator.GLHeader1.PK);
			var expectedSequence = cC1Line.AL_Sequence;
			AssertTransactionLineValues(cC1Line, "REV", expectedSequence, "", -75.00M, null, 0.00M, null, 0.00M, -75.00M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, null, null, TestObjectCreator.GLHeader1, Agent);
			AssertTransactionLineDefaults(cC1Line);

			cC2Line = agentAUDInvoice.Lines.Cast<InvoicingLineBase>().Where(x => x.AL_AG == TestObjectCreator.GLHeader1.PK).Skip(1).First();
			AssertTransactionLineValues(cC2Line, "REV", expectedSequence + 1, "", -75.00M, null, 0.00M, null, 0.00M, -75.00M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, null, null, TestObjectCreator.GLHeader1, Agent);
			AssertTransactionLineDefaults(cC2Line);

			cC3Line = agentAUDInvoice.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AG == TestObjectCreator.GLHeader2.PK);
			expectedSequence = cC3Line.AL_Sequence < cC1Line.AL_Sequence ? cC1Line.AL_Sequence - 1 : cC2Line.AL_Sequence + 1;
			AssertTransactionLineValues(cC3Line, "REV", expectedSequence, "", 80.00M, null, 0.00M, null, 0.00M, 80.00M, AUD, 1, expectedPostDate,
				ZBool.False, agentAUDInvoice, null, null, TestObjectCreator.GLHeader2, Agent);

			AssertTransactionLineDefaults(cC3Line);

			Assert("The Invoice must be reversed.", invoice2.AH_IsCancelled);
			Assert("The Invoice must be reversed.", invoice3.AH_IsCancelled);
		}

		public void TestMiscInvoicesPostingPerformance()
		{
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", AUD, 1M);
			invoice1.AH_OH = Agent.PK;
			invoice1.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice1, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice1, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV2", AUD, 1M);
			invoice2.AH_OH = Agent.PK;
			invoice2.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice2, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice2, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice2, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV3", AUD, 1M);
			invoice3.AH_OH = Agent.PK;
			invoice3.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice3, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice3, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice3, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice3, AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			var invoice4 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV4", AUD, 1M);
			invoice4.AH_OH = Agent.PK;
			invoice4.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(invoice4, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice4, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice4, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice4, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateInvoiceLine(invoice4, AUD, 1M, 75M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			Factory.Save();

			ZString expectedInvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			ZByte expectedInvoiceTermDays = 3;
			ZDateTime expectedInvoiceDate = ZDateTime.Now.AddDays(-3);
			ZDateTime expectedDueDate = expectedInvoiceDate.AddDays(expectedInvoiceTermDays);
			ZDateTime expectedPostDate = ZDateTime.Now.AddDays(-1);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.InvoiceTerm = expectedInvoiceTerm;
			periodicInvoice.InvoiceTermDays = expectedInvoiceTermDays;
			periodicInvoice.LoadMiscInvoices();

			var creator = new PeriodicInvoicePostManager(periodicInvoice);
			var postedTransactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals(1, creator.Poster.PostedInvoices.Count);
			InvoicingBase[] agentAUDInvoices = creator.Poster.GetInvoices(AUD, Agent);
			AssertEquals(1, agentAUDInvoices.Length);
			InvoicingBase agentAUDInvoice = agentAUDInvoices[0];
			AssertEquals("Invoice Lines Count", 14, agentAUDInvoice.Lines.Count);
			AssertEquals("Performance: GetSumOfLines call count", 5, agentAUDInvoice.GetSumOfLinesCallAmount_ForTestOnly);
		}

		[TestDate(2015, 5, 10)]
		public void TestPeriodicInvoicePostManagerPostWithInvoicePostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");

			TestObjectCreator.CreateUSDBuyRate(5.01m, new DateTime(2015, 5, 1));
			TestObjectCreator.CreateUSDBuyRate(5.02m, new DateTime(2015, 5, 2));
			TestObjectCreator.CreateUSDBuyRate(5.03m, new DateTime(2015, 5, 3));
			TestObjectCreator.CreateUSDBuyRate(5.10m, new DateTime(2015, 5, 10));

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var job1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			job1.PlugInData = TestObjectCreator.CreateShipment(job1.JH_JobNum);
			var charge1 = CreateCharge(job1, CC1, "charge2", USD, 200M, Creditor2, USD, 200M, Agent);

			var job2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			job2.PlugInData = TestObjectCreator.CreateShipment(job2.JH_JobNum);
			var charge2 = CreateCharge(job2, CC1, "charge4", USD, 200M, Creditor2, USD, 400M, Agent);

			foreach (Charge charge in job1.Charges.Union(job2.Charges))
			{
				charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			}

			Factory.Save();

			foreach (Charge charge in job1.Charges.Union(job2.Charges))
			{
				AssertEquals("USD", charge.JR_RX_NKSellCurrency);
				AssertEquals(5.10m, charge.JR_OSSellExRate);  //today's default rate
			}

			ZDateTime expectedInvoiceDate = new DateTime(2015, 5, 1);
			ZDateTime expectedDueDate = new DateTime(2015, 5, 2);
			ZDateTime expectedPostDate = new DateTime(2015, 5, 3);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = Agent.PK;
			periodicInvoice.CurrencyNK = USD.RX_Code;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			periodicInvoice.InvoiceDate = expectedInvoiceDate;
			periodicInvoice.DueDate = expectedDueDate;
			periodicInvoice.PostDate = expectedPostDate;
			periodicInvoice.Jobs.AddRange(new[] { job1, job2 });
			periodicInvoice.LoadJobs();
			periodicInvoice.Jobs.AddRange(new[] { job1, job2 });

			var creator = new PeriodicInvoicePostManager(periodicInvoice);

			var invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(0, invoices.Length);

			var transactions = creator.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();

			invoices = Factory.Load<InvoicingBase>(new ZQuery());
			AssertEquals(1, invoices.Length);

			AssertEquals(expectedInvoiceDate, invoices[0].AH_InvoiceDate);
			AssertEquals(expectedPostDate, invoices[0].AH_PostDate);
			AssertEquals(5.01m, invoices[0].AH_ExchangeRate);  //invoice date's rate
			AssertEquals("USD", invoices[0].AH_RX_NKTransactionCurrency);  //invoice date's rate

			foreach (Charge charge in job1.Charges.Union(job2.Charges))
			{
				AssertEquals("USD", charge.JR_RX_NKSellCurrency);
				AssertEquals(5.01m, charge.JR_OSSellExRate);  //invoice date's rate

				var line = invoices[0].Lines.Cast<InvoicingLineBase>().FirstOrDefault(x => x.PK == charge.JR_AL_ARLine);
				AssertEquals(charge.JR_RX_NKSellCurrency, line.AL_RX_NKTransactionCurrency);
				AssertEquals(charge.JR_OSSellExRate, line.AL_ExchangeRate.Round(2));
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}
	}
}
