using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ConsolAPInvoiceCreatorTest : TransactionCreatorBaseTest
	{
		#region Approval Request Authorization

		public void TestRequestToCompareAndCreditNoteCharge()
		{
			AsserRequestToCompareAndCreditNoteCharge(false);
		}

		public void TestRequestToCompareAndCreditNoteCharge_Preview()
		{
			AsserRequestToCompareAndCreditNoteCharge(true);
		}

		void AsserRequestToCompareAndCreditNoteCharge(bool isForPreviewOnly)
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
			consolCost.E6_InvoiceNum = "INV1";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals("Precondition: apportionment charges count", 2, consolCost.ApportionmentCharges.Count);
			AssertEquals("Precondition: job1 charges count", 1, job1.Charges.Count);
			AssertEquals("Precondition: job2 charges count", 1, job2.Charges.Count);
			var postGUIProviderMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			postGUIProviderMock.Setup(m => m.IsForPreviewOnly).Returns(isForPreviewOnly);
			postGUIProviderMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(consol.PK, (ZString)consol.TablePrefix));
			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			var invoiceCharges = new APInvoiceCharges(consolCost.Creditor.OH_Code, consolCost.E6_InvoiceNum, ZGuid.Empty, "", postGUIProviderMock.Object);
			invoiceCharges.Charges.Add(job1.Charges[0]);
			invoiceCharges.Charges.Add(job2.Charges[0]);
			request.InitializeJobRelated(invoiceCharges, consol.PK, consol.TablePrefix);

			postGUIProviderMock.Setup(m => m.RequestToCompare).Returns(request);
			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var creator = new ConsolAPInvoiceCreator(Factory, new[] { job1, job2 }, false, consol, apportionmentListing.CostsCollection, apInvoicePostGUIProvider: postGUIProviderMock.Object);
			var transactions = new TransactionCreatorHashtable();

			consolCost.E6_OSCostAmount *= -1;
			Factory.Save();
			var result = creator.CreateTransactions(transactions);
			Assert(!result);
			AssertEquals("No transactions are created.", 0, transactions.GetAllAPTransactions().Length);

			postGUIProviderMock.Setup(m => m.ShowLoginFormForTest).Returns(false);
			postGUIProviderMock.Setup(m => m.SecurityItemForTest).Returns(Env.Instance.Security.APInvoiceApproval.Code);
			consolCost.E6_OSCostAmount *= -1;
			Factory.Save();
			result = creator.CreateTransactions(transactions);
			Assert(result);
			AssertEquals("A transaction is created.", 1, transactions.GetAllAPTransactions().Length);
		}

		#endregion

		#region TEST: Create All Cost Invoices From Consol

		public void TestPostAPInvoiceWhenConsolCostOnlyIsFalse()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			ApportionmentListing consol1Apps = new ApportionmentListing(Factory, consol1);
			JobConsolCost consolCost1 = consol1Apps.CostsCollection.TryAddNew();
			consolCost1.E6_AC_ChargeCode = CC8.PK;

			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge1_1.JR_E6 = consolCost1.PK;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "2", Now.AddDays(10), Now.AddDays(20));

			var jobs = new[] { job1 };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol1, consol1Apps.CostsCollection);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			AssertEquals("Should have 2 AP invoice", 2, transactions.APTransactionsCount);
			AssertNotNull(transactions.RetrieveAPInvoice(Creditor1, "1"));
			AssertNotNull(transactions.RetrieveAPInvoice(Creditor2, "2"));
		}

		public void TestPostAPInvoiceWhenConsolCostOnlyIsTrue()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			ApportionmentListing consol1Apps = new ApportionmentListing(Factory, consol1);
			JobConsolCost consolCost1 = consol1Apps.CostsCollection.TryAddNew();
			consolCost1.E6_AC_ChargeCode = CC8.PK;

			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			charge1_1.JR_E6 = consolCost1.PK;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "2", Now.AddDays(10), Now.AddDays(20));

			var jobs = new[] { job1 };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol1, consol1Apps.CostsCollection, true);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			AssertEquals("Should be only 1 AP invoice", 1, transactions.APTransactionsCount);
			AssertNotNull(transactions.RetrieveAPInvoice(Creditor1, "1"));
		}

		public void TestDontPostInvoiceForApportionmentOnDifferentConsol()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			ApportionmentListing consol1Apps = new ApportionmentListing(Factory, consol1);
			JobConsolCost consol1Cost = consol1Apps.CostsCollection.TryAddNew();
			consol1Cost.E6_AC_ChargeCode = CC8.PK;

			ApportionmentListing consol2Apps = new ApportionmentListing(Factory, consol2);
			JobConsolCost consol2Cost = consol2Apps.CostsCollection.TryAddNew();
			consol2Cost.E6_AC_ChargeCode = CC8.PK;
			ZGuid consolID2 = consol2.PK;

			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);
			ZGuid apportionSplitCharge1 = consol1Cost.PK;
			ZGuid apportionSplitCharge2 = consol2Cost.PK;

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor1, AUD, 200M, LocalClient);
			charge1_1.JR_E6 = apportionSplitCharge1;
			charge1_2.JR_E6 = apportionSplitCharge2;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));

			var jobs = new[] { job1 };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol1, consol1Apps.CostsCollection);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);

			AssertEquals("Should be only 1 invoice", 1, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			APInvoice invoice = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Should only be one line on invoice", 1, invoice.Lines.Count);
			AssertEquals("Should have correct AP FK", invoice.Lines[0].PK, charge1_1.JR_AL_APLine);
		}

		public void TestCreateConsolInvoicesForSelfBilledCreditors()
		{
			Creditor1.CompanyData.OB_APCostsSelfBilled = true;
			Creditor1.Factory.Save();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ApportionmentListing consolApps = new ApportionmentListing(Factory, consol);

			JobConsolCost cost1 = consolApps.CostsCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = CC8.PK;
			cost1.E6_OSCostAmount = 100m;
			cost1.E6_ApportionmentMethod = "SHP";
			cost1.E6_OH_Creditor = Creditor1.PK;

			JobConsolCost cost2 = consolApps.CostsCollection.TryAddNew();
			cost2.E6_AC_ChargeCode = CC9.PK;
			cost2.E6_RX_NKCurrency = USD.RX_Code;
			cost2.E6_ExchangeRate = 0.9511m;
			cost2.E6_OSCostAmount = 280m;
			cost2.E6_ApportionmentMethod = "SHP";
			cost2.E6_OH_Creditor = Creditor1.PK;

			Factory.Save();

			var jobs = new[] { (Job)shipment1.Job, (Job)shipment2.Job };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol, consolApps.CostsCollection);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			Factory.Save();
			AssertEquals("Should have created 2 invoices", 1, transactions.GetAllAPInvoicesAndCreditNotes().Length);
			InvoicingBase[] invoices = transactions.GetAllAPInvoicesAndCreditNotes();
			InvoicingBase aUDInvoice;

			aUDInvoice = invoices[0];

			AssertEquals(aUDInvoice.AH_TransactionNum, cost1.E6_InvoiceNum);
			AssertEquals(aUDInvoice.AH_InvoiceDate, cost1.E6_InvoiceDate);
			AssertEquals(aUDInvoice.AH_DueDate, cost1.E6_PaymentDate);

			AssertEquals(aUDInvoice.AH_TransactionNum, cost2.E6_InvoiceNum);
			AssertEquals(aUDInvoice.AH_InvoiceDate, cost2.E6_InvoiceDate);
			AssertEquals(aUDInvoice.AH_DueDate, cost2.E6_PaymentDate);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllCostInvoicesFromConsol()
		{
			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);
			var consol = (ForwardingConsol)Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>());
			var cost = Factory.New<JobConsolCost>();
			using (cost.ReportSettingParentSuspender.GetSuspender())
			{
				cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));

			Job job2 = CreateJob("S00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, AUD, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol, null);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			AssertEquals("Invoice Count", 7, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1_1Line = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1_1Line, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1_1Line);

			TransactionLine cC1_7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC1_7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1_7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "S00001001", Now.AddDays(10), Now.AddDays(20),
				-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			TransactionLine cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "S00001001", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, -12.50M, -275M, AUD, 1, Now, ZBool.False, Creditor2, job2,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv2);

			TransactionLine cC2Line2 = creditor2Inv2.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Charge Code 2", -250M, GST1, -25M, WHT1, -12.50M, -275M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv2, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "S00001001", Now.AddDays(15), Now.AddDays(25),
				-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);

			TransactionLine cC2_3Line = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC2_3Line, "CST", 1, "Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC2_3Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", false, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", false, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", false, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", false, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", false, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", false, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		public void TestPostAPInvoicesToLoginBranchFromExportConsol()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			Consol.JK_OA_SendingForwarderAddress = TestBranch2.OrgProxy.MainAddress.PK;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("System will use branch of sending agent proxy for export consol", TestBranch2.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromExportConsol_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("AUSYD", "KRSEL");

			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			Consol.JK_OA_SendingForwarderAddress = TestBranch2.OrgProxy.MainAddress.PK;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");

			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromImportConsol()
		{
			CreateConsolTestData("KRSEL", "AUSYD");
			Consol.JK_OA_ReceivingForwarderAddress = TestBranch2.OrgProxy.MainAddress.PK;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("System will use branch of receiving agent proxy for import consol", TestBranch2.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromImportConsol_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("KRSEL", "AUSYD");

			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			Consol.JK_OA_ReceivingForwarderAddress = TestBranch2.OrgProxy.MainAddress.PK;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");

			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromDomesticConsol()
		{
			CreateConsolTestData("AUPER", "AUMEL");
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("System will use branch of job header for domestic consol", TestBranch.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromDomesticConsol_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("AUPER", "AUMEL");
			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");

			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromForeignConsol()
		{
			CreateConsolTestData("KRSEL", "KRPUS");
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("System will use branch of job header for foreign consol", TestBranch.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromForeignConsol_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("KRSEL", "KRPUS");
			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");

			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsolSameJobHeaders()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("System will use branch of job headers when they are the same", TestBranch.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsolSameJobHeaders_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");

			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsolDifferentJobHeaders()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			Job2.JH_GB = TestBranch3.PK;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Fall back to Login Branch when job headers are different", GlbBranch.CurrentBranch.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsolDifferentJobHeaders_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			Job2.JH_GB = TestBranch3.PK;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");

			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsolNoJobHeaders()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			Job.JH_ParentID = ZGuid.Empty;
			Job.JH_ParentTableCode = string.Empty;
			Job2.JH_ParentID = ZGuid.Empty;
			Job2.JH_ParentTableCode = string.Empty;
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Fall back to Login Branch when there are no job headers", GlbBranch.CurrentBranch.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsolNoJobHeaders_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("AUSYD", "KRSEL");
			Job.JH_ParentID = ZGuid.Empty;
			Job.JH_ParentTableCode = string.Empty;
			Job2.JH_ParentID = ZGuid.Empty;
			Job2.JH_ParentTableCode = string.Empty;
			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsol()
		{
			CreateConsolTestData("AUPER", "AUMEL");
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("System will use login branch", GlbBranch.CurrentBranch.PK, invoice.AH_GB);
		}

		public void TestPostAPInvoicesToLoginBranchFromConsol_BranchLevelPostingEnabled()
		{
			CreateConsolTestData("AUPER", "AUMEL");
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);

			AssertEquals("Precondition: both the charges have same branch", Charge1.JR_GB, Charge2.JR_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			APInvoice invoice = PostCostsFromConsol().RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice branch is selected from charge branch", Charge1.JR_GB, invoice.AH_GB);
		}

		void CreateConsolTestData(string loadPort, string dischargePort)
		{
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, false);

			TestBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			TestBranch2.GB_OH_OrgProxy = TestObjectCreator.AALSHI.PK;

			Consol = TestObjectCreator.CreateConsol(loadPort, dischargePort, "C00001234");
			Shipment1 = TestObjectCreator.CreateShipment("S00000001");
			Shipment2 = TestObjectCreator.CreateShipment("S00000002");
			Consol.Shipments.Add(Shipment1);
			Consol.Shipments.Add(Shipment2);
			Job = TestObjectCreator.CreateJob(Shipment1, false);
			Job2 = TestObjectCreator.CreateJob(Shipment2, false);
			CreateExchangeRate(Job, USD, .7M);
			CreateExchangeRate(Job2, USD, .7M);
			Job.JH_GB = TestBranch.PK;
			Job.JH_GE = TestDepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job2.JH_GB = TestBranch.PK;
			Job2.JH_GE = TestDepartment.PK;
			Job2.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", null, 0M, Creditor1, USD, 350M, LocalClient);
			Charge2 = CreateCharge(Job2, CC1, "Charge Code 1", null, 0M, Creditor1, USD, 350M, LocalClient);
			Charge1.JR_GB = TestBranch4.PK;
			Charge1.JR_GE = TestDepartment.PK;
			Charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Charge2.JR_GB = TestBranch4.PK;
			Charge2.JR_GE = TestDepartment.PK;
			Charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			SetAPInvoiceInfo(Charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge2, "2", Now.AddDays(10), Now.AddDays(20));

			Jobs = new[] { Job, Job2 };
		}

		TransactionCreatorHashtable PostCostsFromConsol()
		{
			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, Jobs, false, Consol, null);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			return transactions;
		}

		#endregion

		#region TEST: Create All Cost Invoices From Consol With Apportionment

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestChargesNotProcessedMultipleTimesForApplyExRateOptionIfVisibleFromMoreThanOneJob()
		{
			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);
			Job job2 = CreateJob("S00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job2, USD, .67M);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			Factory.Save();

			Charge charge1 = CreateCharge(job1, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			Charge charge2 = CreateCharge(job2, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			charge1.JR_E6 = cost.PK;
			charge2.JR_E6 = cost.PK;
			job1.Charges.Add(charge2);

			SetAPInvoiceInfo(charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2, "1", Now.AddDays(10), Now.AddDays(20));

			var jobs = new[] { job1, job2 };
			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol, apps.CostsCollection);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			using (PostingExRateRegistryAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, InvoicePostingExchangeRateOption.TodayExchangeRate.Code))
			{
				AssertNoExceptionThrown(() => creator.CreateTransactions(transactions));
			}
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllCostInvoicesFromConsolWithApportionment()
		{
			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC8.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			Factory.Save();

			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = cost.PK;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			Job job2 = CreateJob("S00001001", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, AUD, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = cost.PK;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));
			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol, apps.CostsCollection);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			AssertEquals("Invoice Count", 8, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 1 Invoice 3

			APInvoice creditor1Inv3 = transactions.RetrieveAPInvoice(Creditor1, "3");
			AssertEquals("Invoice Line Count", 1, creditor1Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv3, "AP", "INV", "3", "S00001001", Now.AddDays(10), Now.AddDays(20),
				-150M, -15M, 0M, -165M, AUD, 1, Now, ZBool.False, Creditor1, job2,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv3);

			TransactionLine cC1Line2 = creditor1Inv3.FindTransactionLine("CST", CC1, job2.PK);
			AssertTransactionLineValues(cC1Line2, "CST", 1, "Charge Code 1", -150M, GST1, -15M, WHTFREE1, 0M, -165M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv3, job2, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line2);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 2 Invoice 2

			APInvoice creditor2Inv2 = transactions.RetrieveAPInvoice(Creditor2, "2");
			AssertEquals("Invoice Line Count", 1, creditor2Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv2, "AP", "INV", "2", "S00001001", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, -12.50M, -275M, AUD, 1, Now, ZBool.False, Creditor2, job2,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv2);

			TransactionLine cC2Line2 = creditor2Inv2.FindTransactionLine("CST", CC2, job2.PK);
			AssertTransactionLineValues(cC2Line2, "CST", 1, "Charge Code 2", -250M, GST1, -25M, WHT1, -12.50M, -275M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv2, job2, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line2);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 3 Invoice 2

			APInvoice creditor3Inv2 = transactions.RetrieveAPInvoice(Creditor3, "2");
			AssertEquals("Invoice Line Count", 1, creditor3Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv2, "AP", "INV", "2", "S00001001", Now.AddDays(15), Now.AddDays(25),
				-350, 0M, -17.50M, -350M, AUD, 1, Now, ZBool.False, Creditor3, job2,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv2);

			TransactionLine cC3Line2 = creditor3Inv2.FindTransactionLine("CST", CC3, job2.PK);
			AssertTransactionLineValues(cC3Line2, "CST", 1, "Charge Code 3", -350M, GSTFREE1, 0M, WHT1, -17.50M, -350M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv2, job2, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line2);

			#endregion

			#region Creditor 3 Invoice 3

			APInvoice creditor3Inv3 = transactions.RetrieveAPInvoice(Creditor3, "3");
			AssertEquals("Invoice Line Count", 2, creditor3Inv3.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv3, "AP", "INV", "3", "Multiple Jobs", Now.AddDays(10), Now.AddDays(25),
				-635M, -63.50M, 0M, -698.50M, AUD, 1, Now, ZBool.False, Creditor3, null,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor3Inv3);

			TransactionLine cC8Line1 = creditor3Inv3.FindTransactionLine("CST", CC8, job1.PK);
			AssertTransactionLineValues(cC8Line1, "CST", 1, "Charge Code 8", -300M, GST1, -30M, WHTFREE1, 0M, -330M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv3, job1, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line1);

			TransactionLine cC8Line2 = creditor3Inv3.FindTransactionLine("CST", CC8, job2.PK);
			AssertTransactionLineValues(cC8Line2, "CST", 2, "Charge Code 8", -335M, GST1, -33.50M, WHTFREE1, 0M, -368.50M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv3, job2, CC8, CC8.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC8Line2);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", false, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", false, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", false, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", false, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", false, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", false, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", true, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", true, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", true, charge2_3Accrual.AL_ReverseDate.IsValid);

			#endregion		}
		}

		#endregion

		public void TestSetTransactionHeaderBranch_WhenGetBranchFromConsolAgentsAndJobHeadersReturnNull()
		{
			var consol = TestObjectCreator.CreateConsol("AUPER", "AUMEL", "C00001111");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, TestBranch2.PK);
			SetAPInvoiceInfo(charge, "INV1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var jobCostingPlugInHelpersMock = new Mock<IJobCostingPlugInHelpers>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			mockIAccountingDependencyFactory.Setup(x => x.GetJobCostingPlugInHelpers()).Returns(jobCostingPlugInHelpersMock.Object);
			mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

			jobCostingPlugInHelpersMock.Setup(mock => mock.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory)).Returns((GlbBranch)null);

			var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
			var linesCountInPassedTransaction = 0;
			branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<APInvoice>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
				.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
					(transaction, invoiceLevel, charges) =>
					{
						initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						linesCountInPassedTransaction = transaction.Lines.Count;
					}
				);

			var transactions = new TransactionCreatorHashtable();
			var consolAPInvoiceCreator = new ConsolAPInvoiceCreator(Factory, new[] { job }, false, consol, null);
			consolAPInvoiceCreator.CreateTransactions(transactions);
			var apInvoice = transactions.RetrieveAPInvoice(Creditor1, "INV1");

			AssertGreaterThan("linesCountInPassedTransaction", linesCountInPassedTransaction, 0);
			AssertEquals("initialBranchPKValueInPassedTransaction: ", GlbBranch.CurrentBranch.PK, initialBranchPKValueInPassedTransaction);

			jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(It.IsAny<IJobCostingPlugIn>(), It.IsAny<ZGuid>(), It.IsAny<BusinessObjectFactory>()), Times.Once);
			jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory));

			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<APInvoice>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Exactly(2));
			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(apInvoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, null));
		}

		public void TestSetTransactionHeaderBranch_Consol_WhenGetBranchFromJobHeaders()
		{
			var expectedBranch = TestBranch;
			var consol = TestObjectCreator.CreateConsol("AUPER", "AUMEL", "C00001111");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = CreateCharge(job, TestObjectCreator.CC1, "Charge Code 1", AUD, 100M, TestObjectCreator.Creditor1, AUD, 150M, LocalClient, InvoiceTypesList.Codes.FinalInvoice, TestBranch2.PK);
			SetAPInvoiceInfo(charge, "INV1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var jobCostingPlugInHelpersMock = new Mock<IJobCostingPlugInHelpers>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();
			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);

			mockIAccountingDependencyFactory.Setup(x => x.GetJobCostingPlugInHelpers()).Returns(jobCostingPlugInHelpersMock.Object);
			mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

			jobCostingPlugInHelpersMock.Setup(mock => mock.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory)).Returns(TestBranch);

			var initialBranchPKValueInPassedTransaction = ZGuid.Empty;
			var linesCountInPassedTransaction = 0;
			branchLevelPostingHelperMock.Setup(x => x.SetTransactionHeaderBranch(It.IsAny<APInvoice>(), InvoiceProcessingLevelIsAllowingToResetBranch.Creation, It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()))
				.Callback<TransactionHeaderWithLines, InvoiceProcessingLevelIsAllowingToResetBranch, ITransactionBranchCalculationDataProviderFromJobCharge>(
					(transaction, invoiceLevel, charges) =>
					{
						initialBranchPKValueInPassedTransaction = transaction.AH_GB;
						linesCountInPassedTransaction = transaction.Lines.Count;
					}
				);

			var transactions = new TransactionCreatorHashtable();
			var consolAPInvoiceCreator = new ConsolAPInvoiceCreator(Factory, new[] { job }, false, consol, null);
			consolAPInvoiceCreator.CreateTransactions(transactions);
			var apInvoice = transactions.RetrieveAPInvoice(Creditor1, "INV1");

			AssertGreaterThan("linesCountInPassedTransaction", linesCountInPassedTransaction, 0);
			AssertEquals("initialBranchPKValueInPassedTransaction: ", expectedBranch.PK, initialBranchPKValueInPassedTransaction);

			jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(It.IsAny<IJobCostingPlugIn>(), It.IsAny<ZGuid>(), It.IsAny<BusinessObjectFactory>()), Times.Once);
			jobCostingPlugInHelpersMock.Verify(x => x.FindBranchFromConsolAgentsAndJobHeaders(consol, GlbCompany.CurrentCompany.PK, Factory));

			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(It.IsAny<APInvoice>(), It.IsAny<InvoiceProcessingLevelIsAllowingToResetBranch>(), It.IsAny<ITransactionBranchCalculationDataProviderFromJobCharge>()), Times.Exactly(2));
			branchLevelPostingHelperMock.Verify(x => x.SetTransactionHeaderBranch(apInvoice, InvoiceProcessingLevelIsAllowingToResetBranch.Creation, null));
		}

		#region TEST: Create All Cost Invoices From Consol With Apportionment And Job with Work On Hold

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAllCostInvoicesFromConsolWithApportiomnmentAndJobWithWorkOnHold()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC8.PK;
			cost.E6_OH_Creditor = Creditor3.PK;
			cost.E6_InvoiceNum = "3";
			cost.E6_InvoiceDate = Now.AddDays(10);
			cost.E6_PaymentDate = Now.AddDays(25);
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			ZGuid costSplitGroup1 = cost.PK;

			Job job1 = CreateJob("S00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1_1 = CreateExchangeRate(job1, USD, .7M);
			ExchangeRate rate1_2 = CreateExchangeRate(job1, GBP, .4M);
			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge charge1_3 = CreateCharge(job1, CC3, "Charge Code 3", AUD, 300M, Creditor3, AUD, 350M, Agent);
			Charge charge1_4 = CreateCharge(job1, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge charge1_5 = CreateCharge(job1, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge charge1_6 = CreateCharge(job1, CC6, "Charge Code 6", USD, 200M, Creditor2, USD, 275M, Agent);
			Charge charge1_7 = CreateCharge(job1, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
			Charge charge1_8 = CreateCharge(job1, CC8, "Charge Code 8", AUD, 300M, Creditor3, AUD, 300M, LocalClient);
			charge1_8.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge1_1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_7, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge1_8, "3", Now.AddDays(10), Now.AddDays(25));

			Job job2 = CreateJob("S00001001", LocalClient, 5M, Agent, 10M);
			job2.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			ExchangeRate rate2_1 = CreateExchangeRate(job2, USD, .7M);
			ExchangeRate rate2_2 = CreateExchangeRate(job2, AUD, .4M);

			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 200M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 250M, Creditor2, AUD, 250M, LocalClient);
			Charge charge2_3 = CreateCharge(job2, CC3, "Charge Code 3", AUD, 350M, Creditor3, AUD, 400M, LocalClient);
			Charge charge2_4 = CreateCharge(job2, CC8, "Charge Code 8", AUD, 335M, Creditor3, AUD, 500M, Agent);
			charge2_4.JR_E6 = costSplitGroup1;

			SetAPInvoiceInfo(charge2_1, "3", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_2, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(charge2_3, "2", Now.AddDays(15), Now.AddDays(25));
			SetAPInvoiceInfo(charge2_4, "3", Now.AddDays(10), Now.AddDays(25));
			Factory.Save();

			#region WIPs and Accruals

			AccTransactionLines charge1_1WIP = charge1_1.WIP;
			AccTransactionLines charge1_2WIP = charge1_2.WIP;
			AccTransactionLines charge1_3WIP = charge1_3.WIP;
			AccTransactionLines charge1_4WIP = charge1_4.WIP;
			AccTransactionLines charge1_5WIP = charge1_5.WIP;
			AccTransactionLines charge1_6WIP = charge1_6.WIP;
			AccTransactionLines charge1_7WIP = charge1_7.WIP;
			AccTransactionLines charge1_8WIP = charge1_8.WIP;
			AccTransactionLines charge2_1WIP = charge2_1.WIP;
			AccTransactionLines charge2_2WIP = charge2_2.WIP;
			AccTransactionLines charge2_3WIP = charge2_3.WIP;
			AccTransactionLines charge2_4WIP = charge2_4.WIP;

			AccTransactionLines charge1_1Accrual = charge1_1.Accrual;
			AccTransactionLines charge1_2Accrual = charge1_2.Accrual;
			AccTransactionLines charge1_3Accrual = charge1_3.Accrual;
			AccTransactionLines charge1_5Accrual = charge1_5.Accrual;
			AccTransactionLines charge1_6Accrual = charge1_6.Accrual;
			AccTransactionLines charge1_7Accrual = charge1_7.Accrual;
			AccTransactionLines charge1_8Accrual = charge1_8.Accrual;
			AccTransactionLines charge2_1Accrual = charge2_1.Accrual;
			AccTransactionLines charge2_2Accrual = charge2_2.Accrual;
			AccTransactionLines charge2_3Accrual = charge2_3.Accrual;
			AccTransactionLines charge2_4Accrual = charge2_4.Accrual;

			#endregion

			var jobs = new[] { job1, job2 };

			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, true, consol, apps.CostsCollection);

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			AssertEquals("Invoice Count", 4, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, -10M, 0M, -310M, AUD, 1, Now, ZBool.False, Creditor1, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line1 = creditor1Inv1.FindTransactionLine("CST", CC1, job1.PK);
			AssertTransactionLineValues(cC1Line1, "CST", 1, "Charge Code 1", -100M, GST1, -10M, WHTFREE1, 0M, -110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job1, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line1);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, job1.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, job1, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, job1.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, job1, CC5, CC5.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line1 = creditor2Inv1.FindTransactionLine("CST", CC2, job1.PK);
			AssertTransactionLineValues(cC2Line1, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, job1, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line1);

			#endregion

			#region Creditor 3 Invoice 1

			APInvoice creditor3Inv1 = transactions.RetrieveAPInvoice(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "INV", "1", "S00001000", Now.AddDays(10), Now.AddDays(20),
				-300M, 0M, -15M, -300M, AUD, 1, Now, ZBool.False, Creditor3, job1,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, job1.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", -300M, GSTFREE1, 0M, WHT1, -15M, -300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, job1, CC3, CC3.CostAccount, Creditor3);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1_1 WIP Reversed", false, charge1_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 WIP Reversed", false, charge1_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 WIP Reversed", false, charge1_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_4 WIP Reversed", false, charge1_4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 WIP Reversed", false, charge1_5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 WIP Reversed", false, charge1_6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 WIP Reversed", false, charge1_7WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 WIP Reversed", false, charge2_1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 WIP Reversed", false, charge2_2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 WIP Reversed", false, charge2_3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 WIP Reversed", false, charge2_4WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 1_1 Accrual Reversed", true, charge1_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_2 Accrual Reversed", true, charge1_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_3 Accrual Reversed", true, charge1_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_5 Accrual Reversed", true, charge1_5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_6 Accrual Reversed", false, charge1_6Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 1_7 Accrual Reversed", true, charge1_7Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_1 Accrual Reversed", false, charge2_1Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_2 Accrual Reversed", false, charge2_2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_3 Accrual Reversed", false, charge2_3Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2_4 Accrual Reversed", false, charge2_4Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#region TestPostInvoicesAsUAInvoices

		public void TestPostInvoicesAsUAInvoices()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTestPostInvoicesAsUAInvoices(false);
		}

		public void TestPostInvoicesAsRequests_WhenChargeApprovalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTestPostInvoicesAsUAInvoices(true);
		}

		void AssertTestPostInvoicesAsUAInvoices(bool isChargeApprovalActivated)
		{
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = APInvoiceCreatorTest.GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 250, AuthorisationCodes.NoApprovalRequired);
			var above = APInvoiceCreatorTest.GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 250, AuthorisationCodes.FirstApprovalRequiredOnly);
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			if (isChargeApprovalActivated)
			{
				Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
			}
			else
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
			}

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var shipment2 = TestObjectCreator.CreateShipment("S002", consol);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			Factory.Save();

			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, CC1, 270m, Creditor1, AllocationMethod.Manual, apportionmentListing);
			consolCost1.ApportionmentCharges[0].JR_OSCostAmt = 150m;
			consolCost1.ApportionmentCharges[1].JR_OSCostAmt = 120m;
			TestObjectCreator.SetAPInvoiceInfo(consolCost1, "1", ZDateTime.Today);
			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, CC1, 220m, Creditor1, AllocationMethod.Manual, apportionmentListing);
			consolCost2.ApportionmentCharges[0].JR_OSCostAmt = 270m;
			consolCost2.ApportionmentCharges[1].JR_OSCostAmt = -50m;
			TestObjectCreator.SetAPInvoiceInfo(consolCost2, "2", ZDateTime.Today);
			Factory.Save();

			var jobs = new[] { job1, job2 };
			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(consol.PK, (ZString)consol.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns((ZDialogResult)DialogResult.OK);
			}
			ConsolAPInvoiceCreator creator = new ConsolAPInvoiceCreator(Factory, jobs, false, consol, apportionmentListing.CostsCollection, apInvoicePostGUIProvider: guiWrapperMock.Object);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			AssertEquals("Posted transactions", isChargeApprovalActivated ? 1 : 2, transactions.Count);
			AssertEquals("AP Transactions Count", 1, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 1 : 0, requests.Length);
			APInvoice invoice;
			TransactionLine line;
			if (isChargeApprovalActivated)
			{
				var request = requests.FirstOrDefault(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "1");
				AssertEquals("Request lines", 2, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == job1.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
				charge = charges.FirstOrDefault(x => x.ChargeCode == CC1.AC_Code && x.JobNumber == job2.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				invoice = transactions.RetrieveAPInvoice(Creditor1, "1");
				AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
				AssertEquals("Should be correct type of invoice", LedgerTypes.UnapprovedPayableTransactions, invoice.AH_Ledger);
				AssertEquals("Should be correct type of line", TransactionTypes.UAInvoice, invoice.AH_TransactionType);
				line = invoice.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, job1.PK);
				AssertNotNull("Should be correct type of line", line);
				line = invoice.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC1, job2.PK);
				AssertNotNull("Should be correct type of line", line);
			}

			invoice = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 2, invoice.Lines.Count);
			Assert("Should be APInvoice", invoice.AH_Ledger == LedgerTypes.AccountsPayable);
			Assert("Should be APInvoice type", invoice.AH_TransactionType == TransactionTypes.Invoice);
			line = invoice.FindTransactionLine(TransactionLineTypes.Cost, CC1, job1.PK);
			AssertNotNull("Should be APInvoiceLine", line);
			line = invoice.FindTransactionLine(TransactionLineTypes.Cost, CC1, job2.PK);
			AssertNotNull("Should be APInvoiceLine", line);
		}

		#endregion

		[TestDate(2015, 5, 10)]
		public void TestPerformAPInvoiceBackDatingAndUpdateExRate()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			//AP backdating
			var regBackDateAPConfig = new BackDateAPInvoicesConfiguration();
			var regBackDateAPConfig1 = regBackDateAPConfig.PostDateConfigurationCollection[0];
			regBackDateAPConfig1.JobType = "ALL";
			regBackDateAPConfig1.DirectionCode = "";
			regBackDateAPConfig1.Mode = "";
			regBackDateAPConfig1.BrokerCode = "";
			regBackDateAPConfig1.SignificantDateCode = "ADD";
			regBackDateAPConfig1.PriorClosedPeriod = "";
			regBackDateAPConfig1.PriorOpenPeriod = "";
			regBackDateAPConfig1.CurrentPeriod = "EPM";
			regBackDateAPConfig1.FuturePeriod = "";
			regBackDateAPConfig1.ReversalRule = "STD";
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regBackDateAPConfig);

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10)); //today's rate
			TestObjectCreator.CreateUSDBuyRate(1.5m, new DateTime(2015, 4, 30)); //backdating rate

			CreateConsolTestData("AUSYD", "KRSEL");

			Charge1.JR_RX_NKCostCurrency = Charge1.JR_RX_NKSellCurrency = "USD";
			Charge1.JR_OSCostExRate = 0.5m;
			Charge1.JR_OSCostAmt = 100m;
			Charge1.JR_OSSellAmt = 150m;

			Charge2.JR_RX_NKCostCurrency = Charge2.JR_RX_NKSellCurrency = "USD";
			Charge2.JR_OSCostExRate = 0.5m;
			Charge2.JR_OSCostAmt = 200m;
			Charge2.JR_OSSellAmt = 200m;

			Factory.Save();

			AssertEquals("USD", Charge1.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, Charge1.JR_OSCostExRate);
			AssertEquals(200m, Charge1.JR_LocalCostAmt);
			AssertEquals(100m, Charge1.JR_OSCostAmt);
			AssertEquals(0.7m, Charge1.JR_OSSellExRate);
			AssertEquals(214.29m, Charge1.JR_LocalSellAmt);
			AssertEquals(150m, Charge1.JR_OSSellAmt);
			AssertEquals(true, !Charge1.IsRevenuePosted && !Charge1.IsCostPosted);

			AssertEquals("USD", Charge2.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, Charge2.JR_OSCostExRate);
			AssertEquals(400m, Charge2.JR_LocalCostAmt);
			AssertEquals(200m, Charge2.JR_OSCostAmt);
			AssertEquals(0.7m, Charge2.JR_OSSellExRate);
			AssertEquals(285.71m, Charge2.JR_LocalSellAmt);
			AssertEquals(200m, Charge2.JR_OSSellAmt);
			AssertEquals(true, !Charge2.IsRevenuePosted && !Charge2.IsCostPosted);

			var transactions = PostCostsFromConsol();
			Factory.Save();

			AssertEquals("Payables Transaction Count", 2, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);

			var invoice1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			var invoice2 = transactions.RetrieveAPInvoice(Creditor1, "2");

			AssertEquals("USD", Charge1.JR_RX_NKCostCurrency);
			AssertEquals(1.5m, Charge1.JR_OSCostExRate); //backdating rate
			AssertEquals(66.67m, Charge1.JR_LocalCostAmt);
			AssertEquals(100m, Charge1.JR_OSCostAmt);
			AssertEquals(0.7m, Charge1.JR_OSSellExRate);
			AssertEquals(214.29m, Charge1.JR_LocalSellAmt);
			AssertEquals(150m, Charge1.JR_OSSellAmt);
			AssertEquals(true, !Charge1.IsRevenuePosted && Charge1.IsCostPosted);

			AssertEquals("USD", Charge2.JR_RX_NKCostCurrency);
			AssertEquals(1.5m, Charge2.JR_OSCostExRate); //backdating rate
			AssertEquals(133.33m, Charge2.JR_LocalCostAmt);
			AssertEquals(200m, Charge2.JR_OSCostAmt);
			AssertEquals(0.7m, Charge2.JR_OSSellExRate);
			AssertEquals(285.71m, Charge2.JR_LocalSellAmt);
			AssertEquals(200m, Charge2.JR_OSSellAmt);
			AssertEquals(true, !Charge2.IsRevenuePosted && Charge2.IsCostPosted);

			AssertEquals("USD", invoice1.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_PostedToEFT", true, invoice1.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 110 / 73.34.", 1.499864m, invoice1.AH_ExchangeRate);
			AssertEquals(new DateTime(2015, 4, 30), invoice1.AH_PostDate);
			AssertEquals(new DateTime(2015, 5, 20), invoice1.AH_InvoiceDate);

			AssertEquals("USD", invoice2.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_PostedToEFT", true, invoice2.AH_PostedToEFT);
			AssertEquals("Exchange Rate is recaculated becasue of enabled AH_PostedToEFT(UseJobExchangeRate), 220 / 146.66.", 1.500068m, invoice2.AH_ExchangeRate);
			AssertEquals(new DateTime(2015, 4, 30), invoice1.AH_PostDate);
			AssertEquals(new DateTime(2015, 5, 20), invoice1.AH_InvoiceDate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		#region Implementation

		protected GlbCompany TestCompany;
		protected GlbBranch TestBranch;
		protected GlbBranch TestBranch2;
		protected GlbBranch TestBranch3;
		protected GlbBranch TestBranch4;
		protected GlbDepartment TestDepartment;
		protected ForwardingConsol Consol;
		protected ForwardingShipment Shipment1;
		protected ForwardingShipment Shipment2;
		protected Job Job;
		protected Job Job2;
		protected Charge Charge1;
		protected Charge Charge2;
		protected IEnumerable<Job> Jobs;

		protected override void SetUp()
		{
			base.SetUp();

			TestCompany = GlbCompany.CurrentCompany;
			TestBranch = TestObjectCreator.CreateBranch("AAA", "AAA NAME", TestCompany);
			TestBranch2 = TestObjectCreator.CreateBranch("BBB", "BBB NAME", TestCompany);
			TestBranch3 = TestObjectCreator.CreateBranch("CCC", "CCC NAME", TestCompany);
			TestBranch4 = TestObjectCreator.CreateBranch("DDD", "DDD NAME", TestCompany);
			TestDepartment = TestObjectCreator.NonCurrentDepartment;
		}

		#endregion
	}
}
