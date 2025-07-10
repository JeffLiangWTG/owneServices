using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ChargeWithCost))]
	public abstract class ChargeWithCostTest : BaseCharge_InnerTest
	{
		#region Cash Advance

		public void TestIsEligibleForNewCashAdvanceRequest()
		{
			var newFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);

			var job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "463426";

			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge1.JR_AT_CostGSTRate = objectCreator.GST1.PK;
			charge1.JR_OSCostAmt = 200m;

			Assert(!charge1.IsCostPosted);
			Assert(charge1.JR_CAL_APLine.IsEmpty);
			Assert(charge1.IsEligibleForNewCashAdvanceRequest);

			var cashAdvanceHeader = objectCreator.CreateCashAdvanceRequestHeader(job1.PK, objectCreator.Creditor1.PK, LedgerTypes.AccountsPayable, 300m, 300m, objectCreator.AUD.RX_Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			var cashAdvanceLine1 = objectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_CAL_APLine = cashAdvanceLine1.PK;
			Assert("Not eligible due to charge links to cash advance", !charge1.IsEligibleForNewCashAdvanceRequest);

			charge1.JR_CAL_APLine = ZGuid.Empty;
			// Posted this Job
			var apInvoice = newFactory.NewWithValidTestData<APInvoice>();
			apInvoice.AH_JH = job1.PK;
			apInvoice.AH_OH = objectCreator.Creditor1.PK;
			var line = newFactory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = apInvoice.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AG = objectCreator.GLHeader1.PK;
			line.AL_OSAmount = 200m;
			line.AL_LineAmount = 200m;
			charge1.JR_AL_APLine = line.PK;
			charge1.SetAmountsFromLinkedLinesForTests();
			Assert(charge1.IsCostPosted);
			Assert("Not eligible due to charge is posted", !charge1.IsEligibleForNewCashAdvanceRequest);
		}

		[TestDate(2022, 6, 13, 23, 55, 30, 253)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAPCashAdvanceRelatedProperties()
		{
			var newFactory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(newFactory);
			var chargeCodeInCurrentCompany = objectCreator.CreateChargeCode("CC1", "CC1 Current Company", Core.Constants.ChargeType.Disbursement, 100, objectCreator.GST1, objectCreator.WHT1, GlbCompany.CurrentCompany);

			var job1 = newFactory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_JobNum = "463426";

			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = chargeCodeInCurrentCompany.PK;
			charge1.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge1.JR_AT_CostGSTRate = objectCreator.GST1.PK;
			charge1.JR_OSCostAmt = 200m;

			var charge2 = job1.Charges.AddNew();
			charge2.JR_AC = chargeCodeInCurrentCompany.PK;
			charge2.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge2.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge2.JR_AT_CostGSTRate = objectCreator.GST1.PK;
			charge2.JR_OSCostAmt = 300m;

			var charge3 = job1.Charges.AddNew();
			charge3.JR_AC = chargeCodeInCurrentCompany.PK;
			charge3.JR_OH_CostAccount = objectCreator.Creditor1.PK;
			charge3.JR_RX_NKCostCurrency = objectCreator.AUD.Code;
			charge3.JR_AT_CostGSTRate = objectCreator.GST1.PK;
			charge3.JR_OSCostAmt = 400m;

			var cashAdvanceHeader = objectCreator.CreateCashAdvanceRequestHeader(job1.PK, objectCreator.Creditor1.PK, LedgerTypes.AccountsPayable, 300m, 300m, objectCreator.AUD.RX_Code, CashAdvanceStatusCodes.RequestHeader.Requested);
			cashAdvanceHeader.CAH_RequestReferenceNumber = "00001001";
			cashAdvanceHeader.CAH_Status = CashAdvanceStatusCodes.RequestHeader.Requested;
			var cashAdvanceLine1 = objectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 100m, 100m, CashAdvanceStatusCodes.RequestLine.Requested);
			var cashAdvanceLine2 = objectCreator.CreateCashAdvanceRequestLine(cashAdvanceHeader.PK, 200m, 200m, CashAdvanceStatusCodes.RequestLine.Requested);
			charge1.JR_CAL_APLine = cashAdvanceLine1.PK;
			charge2.JR_CAL_APLine = cashAdvanceLine2.PK;
			cashAdvanceHeader.Lines.Add(cashAdvanceLine1);
			cashAdvanceHeader.Lines.Add(cashAdvanceLine2);

			newFactory.Save();

			charge1.LoadRelevantChargesForAPCashAdvance();
			AssertEquals("00001001", charge1.APCashAdvanceReferenceNumber);
			AssertEquals("Requested", charge1.APCashAdvanceStatus);
			AssertEquals("14-Jun-22", charge1.APCashAdvanceCreatedDateTime);
			AssertEquals(50m, charge1.APCashAdvanceTotalTax);
			AssertEquals(550m, charge1.APCashAdvanceTotalInvoiceAmount);

			charge3.LoadRelevantChargesForAPCashAdvance();
			AssertEquals("", charge3.APCashAdvanceReferenceNumber);
			AssertEquals("Pending", charge3.APCashAdvanceStatus);
			AssertEquals("14-Jun-22", charge3.APCashAdvanceCreatedDateTime);
			AssertEquals(40m, charge3.APCashAdvanceTotalTax);
			AssertEquals(440m, charge3.APCashAdvanceTotalInvoiceAmount);
		}

		#endregion

		public void TestChargesReloaded_NewChargeSaved()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			var chargeReloaderServiceRecorder = Factory.New<ChargeReloaderServiceRecorder>();
			Factory.Save();
			Assert("Charge is reloaded for charge !IsInDatabase", chargeReloaderServiceRecorder.IsServiceCreated);
		}

		public void TestChargeReloaded_JCLServiceTask()
		{
			var newFactory = Factory.CreateNewFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);
			newFactory.RefreshEnabled = false;
			newFactory.SetContext(CargoWise.Definitions.BusinessContext.JCLServiceTask);

			var shipment = testObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
			var job = testObjectCreator.CreateJob(shipment);
			newFactory.Save();

			var charge = testObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			var chargeReloaderServiceRecorder = newFactory.New<ChargeReloaderServiceRecorder>();
			newFactory.Save();
			Assert("Charge is reloaded for charge !IsInDatabase", chargeReloaderServiceRecorder.IsServiceCreated);
		}

		public void TestDefaultJR_APDocumentReceivedDate()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);

			AssertEquals(ZDateTime.Empty, charge.JR_APDocumentReceivedDate);

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate))
			{
				charge.JR_APInvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(ZDateTime.Now.Date, charge.JR_APDocumentReceivedDate.Date);

				charge.JR_APDocumentReceivedDate = ZDateTime.Today.AddDays(1);
				charge.JR_APInvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(ZDateTime.Today.AddDays(1), charge.JR_APDocumentReceivedDate);
			}

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate))
			{
				charge.JR_APDocumentReceivedDate = ZDateTime.Empty;
				charge.JR_APInvoiceDate = ZDateTime.Today.AddDays(-2);
				AssertEquals(charge.JR_APInvoiceDate, charge.JR_APDocumentReceivedDate);
			}
		}

		public void TestCalculateDueDateWithDocumentReceivedDate()
		{
			var currentDate = ZDateTime.Now;
			var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1);
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			AssertEquals(ZDateTime.Empty, charge.JR_PaymentDate);

			charge.JR_APInvoiceDate = currentDate.Date.AddDays(-1);
			charge.JR_APDocumentReceivedDate = currentDate.AddDays(2);
			AssertEquals("Due Date should be yestday based on Invoice Date", currentDate.Date.AddDays(-1), charge.JR_PaymentDate.Date);

			using (AccountingMasterFilesRegistry.Instance.APInvoiceDueDateCalculationRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				charge.JR_APDocumentReceivedDate = currentDate.AddDays(1);
				AssertEquals("Due Date should be tomorrow based on Document Received Date", currentDate.Date.AddDays(1), charge.JR_PaymentDate.Date);
			}
		}

		class ChargeReloaderServiceRecorder : DummyBusinessObject
		{
			public ChargeReloaderServiceRecorder(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				IsServiceCreated = Factory.ServiceContainer.GetService<ChargeReloader>() != null;
			}

			public bool IsServiceCreated { get; private set; }
		}

		public void TestChargesReloaded_ExchangeRateUpdatedInAnotherFactory()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
				var job = TestObjectCreator.CreateJob(shipment);
				var chargeInDB = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, debtor: GlbCompany.CurrentCompany.OrgProxy);
				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, debtor: GlbCompany.CurrentCompany.OrgProxy);
				charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				AssertEquals("Precondition:", 1, job.ExchangeRates.Count);

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var chargeInNewFactory = newFactory.Load<Charge>(chargeInDB.PK);
				var exchangeRate = chargeInNewFactory.InvoicingJob.ExchangeRates[0];
				exchangeRate.JF_CFXPercent = 3m;
				newFactory.Save();

				Factory.Save();
				Assert("Revenue not posted", !chargeInDB.IsRevenuePosted);
				AssertEquals("Local sell amount updated after CFX changed", new ZDecimal(103.09m), chargeInDB.JR_LocalSellAmt);
				Assert("Revenue posted", charge.IsRevenuePosted);
				AssertEquals("Local sell amount not updated after CFX changed", new ZDecimal(100.0m), charge.JR_LocalSellAmt);
			}
		}

		#region JR_EstimatedCost

		public override void TestJR_EstimatedCost_ReadOnly()
		{
			base.TestJR_EstimatedCost_ReadOnly();

			var charge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			Assert(!charge.JR_EstimatedCostInfo.ReadOnly);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 10);
			charge.JR_AL_APLine = line.PK;

			Assert("Precondition: IsCostPosted", charge.IsCostPosted);
			Assert(charge.JR_EstimatedCostInfo.ReadOnly);
		}

		public void TestChargeCodeChangeResetsJR_EstimatedCost()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (BaseCharge)GetNewBusinessObject();
			charge.JR_JH = job.PK;
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 10;
			charge.JR_EstimatedCost = 20;

			var expectedJR_EstimatedCost = 0m;
			AssertNotEquals("Precondition: JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);
			charge.JR_AC = TestObjectCreator.CC2.PK;
			AssertEquals("JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);

			charge.JR_OSCostAmt = 10;
			charge.JR_EstimatedCost = 20;
			Factory.Save();
			Assert("Precondition: IsInDatabase", charge.IsInDatabase);
			AssertNotEquals("Precondition: JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);
			charge.JR_AC = TestObjectCreator.CC1.PK;
			AssertEquals("JR_EstimatedCost", expectedJR_EstimatedCost, charge.JR_EstimatedCost);
		}

		#endregion

		public void TestBookingWithQuoteShouldNotCreateJobRevenueJournal()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var bookingWithQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
				var job = TestObjectCreator.CreateJob(bookingWithQuote);

				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100m, 200m);
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 200m);
				charge1.JR_OH_CostAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge1.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge1.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				Factory.Save();

				var jobRevenueJournals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal));
				AssertEquals("No job revenue journal should be created.", 0, jobRevenueJournals.Length);
			}
		}

		public void TestOneOffQuotesShouldNotCreateJobRevenueJournal()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var spotQuote = QuotedBooking.New(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
				var job = TestObjectCreator.CreateJob(spotQuote);

				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100m, 200m);
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 200m);
				charge1.JR_OH_CostAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge1.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge1.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				Factory.Save();

				var jobRevenueJournals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal));
				AssertEquals("No job revenue journal should be created.", 0, jobRevenueJournals.Length);
			}
		}

		public void TestQuickBookingShouldCreateJobRevenueJournal()
		{
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				var quickBooking = QuotedBooking.New(Freight.Integration.QuoteBookingType.QuickBooking, Factory);
				var job = TestObjectCreator.CreateJob(quickBooking);

				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100m, 200m);
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 200m);
				charge1.JR_OH_CostAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge1.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge1.JR_GE_InternalDept = TestObjectCreator.NonCurrentDepartment.PK;

				Factory.Save();

				var jobRevenueJournals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal));
				AssertEquals("No job revenue journal should be created.", 2, jobRevenueJournals.Length);
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesChargeWithCost()
		{
			var newCharge = Factory.NewWithValidTestData<ChargeWithCost>();

			var localList = new List<string>
				{
					nameof(newCharge.JR_CFXAmtReverseSign)
				};

			var tester = new DecimalPlacesAttributeTester(newCharge);
			tester.CheckLocalCurrency(localList, nameof(newCharge.LocalCurrencyDecimals));
		}

		[ExpectNoExceptions]
		public void TestCreatingWIPMustBeSuppressesdWhenEmptyOrInvalidDebtor()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");

			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				Factory.Save();
				var invoiceBase = (InvoicingBase)Factory.New<APInvoice>();
				invoiceBase.AH_TransactionNum = "00001000";
				var line = (InvoicingLineBase)invoiceBase.Lines.AddNew();
				line.AL_AC = TestObjectCreator.CC1.PK;
				line.AL_JH = job.PK;
				line.AL_OSAmount = 100m;
				line.AL_LocalExTaxAmount = 100m;

				new TransactionLineJobChargeTransformer(Factory).Transform(invoiceBase);
				Factory.Save();

				var charges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_JH, job.PK));
				AssertEquals("Charges Count", 1, charges.Length);
				AssertEquals("Debtor", ZGuid.Empty, charges[0].JR_OH_SellAccount);
				AssertEquals(false, charges[0].IsDebtorValidToCreateWIPWhenWIPMustHaveDebtor);
				AssertNull("WIP Shouldn't be created", charges[0].WIP);
				charges.ForEach(x => x.RunPreSaveValidation());
				AssertHasErrorContaining(charges[0].JR_OH_SellAccountInfo, "You must enter a debtor. Your system has been configured so that the 'debtor' is mandatory when entering an unposted sell of non zero value.\r\n\r\nThe registry setting that governs this rule is Accounting > Job Costing > WIP Must Have Debtor Code");
			}
		}

		public void TestChargeWithCostTypeDecider()
		{
			var newCharge = Factory.NewWithValidTestData<ChargeWithCost>();
			Assert("New ChargeWithCost is Charge", newCharge is Charge);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedCharge = newFactory.Load<ChargeWithCost>(newCharge.PK);
			Assert("Loaded ChargeWithCost is Charge", loadedCharge is ChargeWithCost);
		}

		public void TestUpdatedRevenueBasedOnCostWithDifferentCurrency()
		{
			var debtor = TestObjectCreator.AALSHI;
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = "USD";

			TestObjectCreator.CC1.AC_MarginPercentage = 100m;
			TestObjectCreator.CC3.AC_MarginPercentage = 70m;

			var job = TestObjectCreator.Job1;
			job.LocalChargesPK = debtor.PK;
			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.JF_BaseRate = 0.8m;

			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = debtor.PK;
			AssertEquals("AUD", charge1.JR_RX_NKCostCurrency);
			AssertEquals("USD", charge1.JR_RX_NKSellCurrency);
			charge1.JR_OSCostAmt = 100m;
			AssertEquals("OS Cost Amt", 100m, charge1.JR_OSCostAmt);
			AssertEquals("Local Cost Amt", 100m, charge1.JR_LocalCostAmt);
			AssertEquals("OS Sell Amt", 80m, charge1.JR_OSSellAmt);
			AssertEquals("Local Sell Amt", 100m, charge1.JR_LocalSellAmt);

			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC3.PK;
			charge2.JR_OH_SellAccount = debtor.PK;
			AssertEquals("AUD", charge2.JR_RX_NKCostCurrency);
			AssertEquals("USD", charge2.JR_RX_NKSellCurrency);
			charge2.JR_OSCostAmt = 100m;
			AssertEquals("OS Cost Amt", 100m, charge2.JR_OSCostAmt);
			AssertEquals("Local Cost Amt", 100m, charge2.JR_LocalCostAmt);
			AssertEquals("OS Sell Amt", 114.29m, charge2.JR_OSSellAmt);
			AssertEquals("Local Sell Amt", 142.86m, charge2.JR_LocalSellAmt);

			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 2m);
			var charge3 = job.Charges.AddNew();
			charge3.JR_AC = TestObjectCreator.CC1.PK;
			charge3.JR_OH_SellAccount = debtor.PK;
			AssertEquals("AUD", charge3.JR_RX_NKCostCurrency);
			AssertEquals("USD", charge3.JR_RX_NKSellCurrency);
			charge3.JR_OSCostAmt = 100m;
			AssertEquals("OS Cost Amt", 100m, charge3.JR_OSCostAmt);
			AssertEquals("Local Cost Amt", 100m, charge3.JR_LocalCostAmt);
			AssertEquals("OS Sell Amt", 80m, charge3.JR_OSSellAmt);
			AssertEquals("Local Sell Amt", 102.04m, charge3.JR_LocalSellAmt);

			var charge4 = job.Charges.AddNew();
			charge4.JR_AC = TestObjectCreator.CC3.PK;
			charge4.JR_OH_SellAccount = debtor.PK;
			AssertEquals("AUD", charge4.JR_RX_NKCostCurrency);
			AssertEquals("USD", charge4.JR_RX_NKSellCurrency);
			charge4.JR_OSCostAmt = 100m;
			AssertEquals("OS Cost Amt", 100m, charge4.JR_OSCostAmt);
			AssertEquals("Local Cost Amt", 100m, charge4.JR_LocalCostAmt);
			AssertEquals("OS Sell Amt", 114.29m, charge4.JR_OSSellAmt);
			AssertEquals("Local Sell Amt", 145.78m, charge4.JR_LocalSellAmt);
		}

		public void TestUpdateCostForMarginCodeWithDifferentCurrency()
		{
			var creditor = TestObjectCreator.ABIGAS;
			creditor.CompanyData.OB_RX_NKAPDefltCurrency = "USD";

			var debtor = TestObjectCreator.AALSHI;
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = "AUD";

			TestObjectCreator.CC1.AC_MarginPercentage = 70m;

			var job = TestObjectCreator.Job1;
			job.LocalChargesPK = debtor.PK;
			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = "USD";
			exRate.JF_BaseRate = 0.8m;

			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_CostAccount = creditor.PK;
			charge1.JR_OH_SellAccount = debtor.PK;
			AssertEquals("USD", charge1.JR_RX_NKCostCurrency);
			AssertEquals("AUD", charge1.JR_RX_NKSellCurrency);
			charge1.JR_OSSellAmt = 100m;
			AssertEquals("OS Cost Amt", 56m, charge1.JR_OSCostAmt);
			AssertEquals("Local Cost Amt", 70m, charge1.JR_LocalCostAmt);
			AssertEquals("OS Sell Amt", 100m, charge1.JR_OSSellAmt);
			AssertEquals("Local Sell Amt", 100m, charge1.JR_LocalSellAmt);

			creditor.CompanyData.OB_RX_NKAPDefltCurrency = "AUD";
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 2m);
			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_CostAccount = creditor.PK;
			charge2.JR_OH_SellAccount = debtor.PK;
			AssertEquals("AUD", charge2.JR_RX_NKCostCurrency);
			AssertEquals("USD", charge2.JR_RX_NKSellCurrency);
			charge2.JR_OSSellAmt = 100m;
			AssertEquals("OS Cost Amt", 89.29m, charge2.JR_OSCostAmt);
			AssertEquals("Local Cost Amt", 89.29m, charge2.JR_LocalCostAmt);
			AssertEquals("OS Sell Amt", 100m, charge2.JR_OSSellAmt);
			AssertEquals("Local Sell Amt", 127.55m, charge2.JR_LocalSellAmt);
		}

		public void TestDefaultCurrenciesNotOverriden()
		{
			var debtor = TestObjectCreator.AALSHI;
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
			var creditor = TestObjectCreator.ABIGAS;
			creditor.CompanyData.OB_RX_NKAPDefltCurrency = "KRW";

			var charge1 = (ChargeWithCost)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = debtor.PK;
			AssertEquals("USD", charge1.JR_RX_NKSellCurrency);
			AssertEquals(0m, charge1.JR_OSSellAmt);
			charge1.JR_OSCostAmt = 1000m;
			AssertEquals("USD", charge1.JR_RX_NKSellCurrency);

			var charge2 = (ChargeWithCost)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_CostAccount = creditor.PK;
			AssertEquals("KRW", charge2.JR_RX_NKCostCurrency);
			AssertEquals(0m, charge2.JR_OSCostAmt);
			charge2.JR_OSSellAmt = 1000m;
			AssertEquals("KRW", charge2.JR_RX_NKCostCurrency);
		}

		public void TestEmptyRevenueCalculationDescription()
		{
			var aCharge = (ChargeWithCost)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			aCharge.RevenueCalculationDescription = ZBlob.FromAscii("SomeText");
			aCharge.EmptyRevenueCalculationDescription_ForTestOnly();
			AssertEquals("RevenueCalculationDescription should be cleared", ZBlob.Empty, aCharge.RevenueCalculationDescription);
		}

		public void TestEmptyCostCalculationDescription()
		{
			var aCharge = (ChargeWithCost)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			aCharge.CostCalculationDescription = ZBlob.FromAscii("SomeText");
			aCharge.EmptyCostCalculationDescription_ForTestOnly();
			AssertEquals("CostCalculationDescription should be set to the constant value", ZBlob.Empty, aCharge.CostCalculationDescription);
		}

		public void TestTwoChargesWithSameChequeNumberOnlyUsesOneChequeNumber()
		{
			AccBankAccount bank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			bank.AB_ChequeNumDigits = (ZByte)5;
			AccChequeBook book = TestObjectCreator.InsertChequeBook(100, 100, 200, bank.PK);
			Factory.Save();
			OrgHeader costAccount = TestObjectCreator.GetOrganisation();
			costAccount.OH_IsCreditor = true;
			RefCurrency currency = TestObjectCreator.GetCurrency("USD");
			AccChargeCode marginChargeCode = TestObjectCreator.InsertMarginChargeCode(100);
			Job job1 = TestObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job1.JH_JobNum = "S001";

			Charge charge1 = job1.Charges.AddNew();
			charge1.JR_JH = job1.PK;
			charge1.JR_AC = marginChargeCode.PK;
			charge1.JR_OH_CostAccount = costAccount.PK;
			charge1.JR_RX_NKCostCurrency = currency.RX_Code;
			charge1.JR_OSCostAmt = 100M;
			charge1.JR_APInvoiceNum = "INV1";

			charge1.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge1.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			charge1.JR_PaymentType = "CHQ";
			charge1.JR_AB = bank.PK;

			AssertEquals("Cheque Book Current Number should be 100 before entering charges", 100m, book.AK_CurrentNo);
			charge1.JR_AK = book.PK;
			// Cheque Number will be automatically populated when you set the cheque book - in this case 100
			AssertEquals("Cheque Number for Charge 1 should be 00100", "00100", charge1.JR_ChequeNo);
			AssertEquals("Cheque Book Current Number should be 101 once you set the cheque book", book.AK_CurrentNo, 101m);
			job1.ExchangeRates[0].JF_BaseRate = 1m; //allow saving
			job1.ExchangeRates[1].JF_BaseRate = 1m; //allow saving
			Factory.Save();

			Charge charge2 = job1.Charges.AddNew();
			charge2.JR_JH = charge1.JR_JH;
			charge2.JR_AC = charge1.JR_AC;
			charge2.JR_OH_CostAccount = charge1.JR_OH_CostAccount;
			charge2.JR_RX_NKCostCurrency = charge1.JR_RX_NKCostCurrency;
			charge2.JR_OSCostAmt = charge1.JR_OSCostAmt;
			charge2.JR_APInvoiceNum = charge1.JR_APInvoiceNum;

			// Everything else should default from the first charge as they are both on the same invoice
			AssertEquals("Cheque Number for Charge 2 should be 00100", "00100", charge2.JR_ChequeNo);
			Assert("Cheque Number for Charge 2 should be the same as Charge 1", charge1.JR_ChequeNo == charge2.JR_ChequeNo);
			AssertEquals("Cheque Book Current Number should still be 101", book.AK_CurrentNo, 101m);
		}

		public void TestInitialReadOnly()
		{
			Assert("JR_APInvoiceNum not initially read-only.", ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("JR_APInvoiceDate not initially read-only.", ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("JR_APDocumentReceivedDate not initially read-only.", ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("JR_CostReference not initially read-only.", ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("JR_PaymentType not initially read-only.", ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("JR_PaymentDate not initially read-only.", ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("JR_AB not initially read-only.", ACharge.JR_ABInfo.ReadOnly);
			Assert("JR_AK not initially read-only.", ACharge.JR_AKInfo.ReadOnly);
			Assert("JR_ChequeNo not initially read-only.", ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("JR_AT_CostGSTRate not initially read-only.", ACharge.JR_AT_CostGSTRateInfo.ReadOnly);
			Assert("JR_AT_SellGSTRate not initially read-only.", ACharge.JR_AT_SellGSTRateInfo.ReadOnly);
			Assert("JR_AW_CostWHTRate not initially read-only.", ACharge.JR_AW_CostWHTRateInfo.ReadOnly);
			Assert("JR_AW_SellWHTRate not initially read-only.", ACharge.JR_AW_SellWHTRateInfo.ReadOnly);
		}

		public void TestJR_PreventInvoicePrintGroupingReadOnly()
		{
			AssertEquals("Read Only", false, ACharge.JR_PreventInvoicePrintGroupingInfo.ReadOnly);

			AccTransactionLines postedRevenue = Factory.New<AccTransactionLines>();
			postedRevenue.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = postedRevenue.PK;

			AssertEquals("Read Only", true, ACharge.JR_PreventInvoicePrintGroupingInfo.ReadOnly);
		}

		public void TestSetDescriptionWhenChargeCodeIsChanged()
		{
			ACharge.JR_AC = ZGuid.Empty;
			AssertEquals(ZString.Empty, ACharge.JR_Desc);

			AccChargeCode code = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Revenue);
			ACharge.JR_AC = code.PK;
			AssertEquals("Failed to fetch description", code.AC_Desc, ACharge.JR_Desc);
		}

		public void TestSetTaxIdWhenChargeCodeIsChanged()
		{
			AccChargeCode code = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Revenue);
			ZGuid dodgyValue = ZGuid.NewZGuid();
			ACharge.JR_AT_CostGSTRate = dodgyValue;
			ACharge.JR_AT_SellGSTRate = dodgyValue;

			ACharge.JR_AC = code.PK;

			Assert("Failed to fetch TaxID", ACharge.JR_AT_CostGSTRate != dodgyValue);
			Assert("Failed to fetch TaxID", ACharge.JR_AT_SellGSTRate != dodgyValue);
		}

		public void TestSetWHTidWhenChargeCodeIsChanged()
		{
			AccChargeCode code = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Revenue);
			ZGuid dodgyValue = ZGuid.NewZGuid();
			ACharge.JR_AW_CostWHTRate = dodgyValue;
			ACharge.JR_AW_SellWHTRate = dodgyValue;

			ACharge.JR_AC = code.PK;

			Assert("Failed to fetch WHTaxID", ACharge.JR_AW_CostWHTRate != dodgyValue);
			Assert("Failed to fetch WHTaxID", ACharge.JR_AW_SellWHTRate != dodgyValue);
		}

		public void TestSetGSTandWHTWhenCostAccountChanges()
		{
			OrgHeader organisation = TestObjectCreator.GetOrganisation();
			ZGuid dodgyValue = ZGuid.NewZGuid();
			ACharge.JR_AT_CostGSTRate = dodgyValue;
			ACharge.JR_AW_CostWHTRate = dodgyValue;

			ACharge.JR_OH_CostAccount = organisation.PK;

			Assert("Failed to fetch GSTaxID", ACharge.JR_AT_CostGSTRate != dodgyValue);
			Assert("Failed to fetch WHTaxID", ACharge.JR_AW_CostWHTRate != dodgyValue);
		}

		public void TestSetGSTandWHTWhenLocalAccountChanges()
		{
			OrgHeader organisation = TestObjectCreator.GetOrganisation();
			ZGuid dodgyValue = ZGuid.NewZGuid();
			ACharge.JR_AT_SellGSTRate = dodgyValue;
			ACharge.JR_AW_SellWHTRate = dodgyValue;

			ACharge.JR_OH_SellAccount = organisation.PK;

			Assert("Failed to fetch GSTaxID", ACharge.JR_AT_SellGSTRate != dodgyValue);
			Assert("Failed to fetch WHTaxID", ACharge.JR_AW_SellWHTRate != dodgyValue);
		}

		public void TestJR_APInvoiceNum()
		{
			ACharge.JR_APInvoiceNum = TestString;
			AssertEquals(TestString, ACharge.JR_APInvoiceNum);
		}

		public void TestJR_AB()
		{
			ACharge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(TestObjectCreator.AUDBankAccount.PK, ACharge.JR_AB);
		}

		public void TestJR_AT_CostGSTRate()
		{
			ACharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals(TestObjectCreator.GST1.PK, ACharge.JR_AT_CostGSTRate);
		}

		public void TestJR_AT_CostGSTRateSetsJR_OSCostAmt()
		{
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 20;
			ACharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals(22M, ACharge.JR_Calc_OSCostAmtWithGST);
		}

		public void TestJR_AW_CostWHTRate()
		{
			ACharge.JR_AW_CostWHTRate = TestObjectCreator.WHT1.PK;
			AssertEquals(TestObjectCreator.WHT1.PK, ACharge.JR_AW_CostWHTRate);
		}

		public void TestJR_AT_SellGSTRate()
		{
			ACharge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			AssertEquals(TestObjectCreator.GST1.PK, ACharge.JR_AT_SellGSTRate);
		}

		public void TestJR_AW_SellWHTRate()
		{
			ACharge.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			AssertEquals(TestObjectCreator.WHT1.PK, ACharge.JR_AW_SellWHTRate);
		}

		public void TestJR_AC()
		{
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			AssertEquals(TestObjectCreator.MRG100.PK, ACharge.JR_AC);
		}

		public void TestJR_ACSetsJR_AT_CostGSTRate()
		{
			ACharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_AC = ZGuid.NewZGuid();
			AssertEquals("Can't test for real value, because if the Company is not TestObjectCreator.GST1 registered, then it will blank it out, so testing the fact of changing value", Guid.Empty, ACharge.JR_AT_CostGSTRate);
		}

		public void TestJR_ACSetsJR_AW_CostWHTRate()
		{
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			ACharge.JR_AW_CostWHTRate = TestObjectCreator.WHT1.PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_AC = ZGuid.NewZGuid();
			AssertEquals("Can't test for real value, because if the Company is not TestObjectCreator.GST1 registered, then it will blank it out, so testing the fact of changing value", Guid.Empty, ACharge.JR_AW_CostWHTRate);
		}

		public void TestJR_ACSetsJR_AT_SellGSTRate()
		{
			ACharge.JR_AT_SellGSTRate = ZGuid.Empty;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			AssertEquals(TestObjectCreator.GST1.PK, ACharge.JR_AT_SellGSTRate);
		}

		public void TestJR_ACSetsJR_AW_SellWHTRate()
		{
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			ACharge.JR_AW_SellWHTRate = TestObjectCreator.WHT1.PK;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;

			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			Assert(ACharge.JR_AW_SellWHTRate != TestObjectCreator.WHT1.PK);
		}

		public void TestJR_OH_CostAccount()
		{
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals(TestObjectCreator.TestOrganisation.PK, ACharge.JR_OH_CostAccount);
		}

		public void TestJR_OH_CostAccountSetsJR_AT_CostGSTRate()
		{
			ACharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals("Can't test for real value, because if the Company is not TestObjectCreator.GST1 registered, then it will blank it out, so testing the fact of changing value", Guid.Empty, ACharge.JR_AT_CostGSTRate);
		}

		public void TestJR_OH_CostAccountSetsJR_AW_CostWHTRate()
		{
			ACharge.JR_AW_CostWHTRate = TestObjectCreator.WHT1.PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals("Can't test for real value, because if the Company is not TestObjectCreator.GST1 registered, then it will blank it out, so testing the fact of changing value", Guid.Empty, ACharge.JR_AW_CostWHTRate);
		}

		public void TestJR_OH_SellAccount()
		{
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals(TestObjectCreator.TestOrganisation.PK, ACharge.JR_OH_SellAccount);
		}

		public void TestJR_OH_SellAccountSetsJR_AT_SellGSTRate()
		{
			ACharge.JR_AT_SellGSTRate = Guid.NewGuid();
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals("Can't test for real value, because if the Company is not TestObjectCreator.GST1 registered, then it will blank it out, so testing the fact of changing value", Guid.Empty, ACharge.JR_AT_SellGSTRate);
		}

		public void TestJR_OH_SellAccountSetsJR_AW_SellWHTRate()
		{
			ACharge.JR_AW_SellWHTRate = Guid.NewGuid();
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals("Can't test for real value, because if the Company is not TestObjectCreator.GST1 registered, then it will blank it out, so testing the fact of changing value", Guid.Empty, ACharge.JR_AW_SellWHTRate);
		}

		public void TestJR_RX_NKCostCurrency()
		{
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
		}

		public void TestJR_RX_NKCostCurrencySetsJR_OSCostExRate()
		{
			ACharge.JR_RX_NKCostCurrency = "USD";
			ACharge.JR_OSCostExRate = -1;
			ACharge.JR_RX_NKCostCurrency = "AAA";
			Assert(ACharge.JR_OSCostExRate != -1);
		}

		public void TestJR_RX_NKCostCurrencySetsJR_OSCostExRateToOneForLocalCurrency()
		{
			ACharge.JR_RX_NKCostCurrency = "USD";
			ACharge.JR_OSCostExRate = 0.88m;
			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, ACharge.JR_OSCostExRate);
		}

		public void TestJR_RX_NKSellCurrency()
		{
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
		}

		public void TestJR_RX_NKSellCurrencySetsJR_OSSellExRate()
		{
			ACharge.JR_RX_NKSellCurrency = "USD";
			ACharge.JR_OSSellExRate = -1;
			ACharge.JR_RX_NKSellCurrency = "EUR";
			Assert(ACharge.JR_OSSellExRate != -1);
		}

		public void TestJR_OSCostExRate()
		{
			ACharge.JR_RX_NKCostCurrency = "USD";
			ACharge.JR_OSCostExRate = 0.9M;
			AssertEquals(0.9M, ACharge.JR_OSCostExRate);
		}

		public void TestJR_OSSellExRate()
		{
			ACharge.JR_RX_NKSellCurrency = "USD";
			ACharge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.9m);
			AssertEquals(0.9M, ACharge.JR_OSSellExRate);
		}

		public void TestJR_IsCostPosted()
		{
			ACharge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, ACharge.JR_IsCostPosted);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = transactionLine.PK;
			AssertEquals(true, ACharge.JR_IsCostPosted);
		}

		public void TestJR_ChargePosted()
		{
			ACharge.JR_AL_ARLine = ZGuid.Empty;
			AssertEquals(false, ACharge.JR_IsRevenuePosted);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = transactionLine.PK;
			AssertEquals(true, ACharge.JR_IsRevenuePosted);
		}

		public void TestJR_OSCostAmt()
		{
			ACharge.JR_OSCostAmt = -10m;
			AssertEquals(-10m, ACharge.JR_OSCostAmt);
		}

		public void TestJR_OSSellAmt()
		{
			bool originalIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			try
			{
				SetCurrentCountryToCode(Core.Constants.CountryCodes.Iceland);
				ACharge.JR_RX_NKSellCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, Core.Constants.CurrencyCodes.Iceland)).RX_Code;
				ACharge.JR_OSSellAmt = 22.22m;
				AssertEquals("JR_OSSellAmt should be rounded to int for icelandic company.", 22m, ACharge.JR_OSSellAmt);
				ACharge.JR_OSSellAmt = 44.50m;
				AssertEquals("JR_OSSellAmt should be rounded to int.", 45.00m, ACharge.JR_OSSellAmt);

				ACharge.JR_RX_NKSellCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, Core.Constants.CurrencyCodes.Australia)).RX_Code;
				ACharge.JR_OSSellAmt = 22.22m;
				AssertEquals("JR_OSSellAmt shouldn't be rounded to int for not icelandic countries .", 22.22m, ACharge.JR_OSSellAmt);

				ACharge.JR_RX_NKSellCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.Equal, Core.Constants.CurrencyCodes.Iceland)).RX_Code;
				SetCurrentCountryToCode(Core.Constants.CountryCodes.Australia);
				ACharge.JR_OSSellAmt = 22.22m;
				AssertEquals("JR_OSSellAmt should be rounded to int for not icelandic countries .", 22m, ACharge.JR_OSSellAmt);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalIsGSTRegistered;
				SetCurrentCountryToCode(currentCountry);
			}

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ExchangeRate rate = TestObjectCreator.CreateExchangeRate((Job)ACharge.Job, TestObjectCreator.USD, 0.9m);
			AccChargeCode chargeCode = TestObjectCreator.CreateChargeCode("My", "My Charge", "MRG", 100m, null, null);
			ACharge.JR_AC = chargeCode.PK;

			ACharge.JR_OSSellAmt = 1000m;
			AssertEquals("JR_OSCostAmount", 1000m, ACharge.JR_OSCostAmt);

			ACharge.JR_OSCostAmt = 0m;
			ACharge.JR_OSSellAmt = 500m;
			AssertEquals("JR_OSCostAmount", 500m, ACharge.JR_OSCostAmt);

			ACharge.JR_OSCostAmt = 0m;
			ACharge.JR_OSSellAmt = 500m;
			AssertEquals("Assigning the same value to JR_OSSellAmt should not re-set the JR_OSCostAmount", 0m, ACharge.JR_OSCostAmt);
		}

		public void TestValidateJR_RX_NKSellCurrencyWhenOSCostCurrencyIsForeign()
		{
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = "CCC";
			Assert(ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_AL_ARLine = ZGuid.NewZGuid();
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());
		}

		public void TestValidateJR_RX_NKSellCurrencyWhenOSCostCurrencyIsLocal()
		{
			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ACharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_AL_ARLine = ZGuid.NewZGuid();
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());
		}

		public void TestValidateJR_RX_NKSellCurrencyWhenOSCostCurrencyIsBlank()
		{
			ACharge.JR_RX_NKCostCurrency = ZString.Empty;
			ACharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_AL_ARLine = ZGuid.NewZGuid();
			Assert(!ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());
		}

		public void TestValidateJR_RX_NKSellCurrencyMandatory()
		{
			ACharge.JR_RX_NKSellCurrency = ZString.Empty;
			Assert("Currency is always mandatory", ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = "AAA";
			Assert("Currency must always be in the list of valid currencies", ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert("Currency is now valid and should not have errors", !ACharge.JR_RX_NKSellCurrencyInfo.HasErrors());
		}

		public void TestValidateJR_RX_NKCostCurrencyMandatory()
		{
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_RX_NKCostCurrency = ZString.Empty;
			Assert("Currency is mandatory unless the charge code is a Revenue Charge Code", ACharge.JR_RX_NKCostCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKCostCurrency = "EEE";
			Assert("Currency must always be in the list of valid currencies unless the charge code is a Revenue Charge Code", ACharge.JR_RX_NKCostCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert("Currency is now valid and should not have errors unless the charge code is a Revenue Charge Code", !ACharge.JR_RX_NKCostCurrencyInfo.HasErrors());

			ACharge.JR_RX_NKCostCurrency = ZString.Empty;
			ACharge.JR_AC = TestObjectCreator.RevenueChargeCode.PK;
			Assert("Should not validate mandatory COST ONLY currency when charge code is Revenue", !ACharge.JR_RX_NKCostCurrencyInfo.HasErrors());
		}

		public void TestChargeCodeClearsEverything()
		{
			ACharge.JR_RX_NKCostCurrency = "FFF";
			ACharge.JR_OSCostAmt = 10;

			ACharge.JR_RX_NKSellCurrency = "GGG";
			ACharge.JR_OSSellAmt = 10;

			ACharge.JR_AC = Guid.NewGuid();

			AssertEquals("Should be cleared", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ACharge.JR_RX_NKCostCurrency);
			AssertEquals("Should be cleared", 0m, ACharge.JR_OSCostAmt);

			AssertEquals("Should be cleared", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals("Should be cleared", 0m, ACharge.JR_OSSellAmt);
		}

		public void TestSettingOSCostAmountDoesntChangePostedRevenueAmounts()
		{
			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ACharge.JR_OSCostExRate = 1.00m;
			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			ACharge.JR_GB = GlbBranch.CurrentBranch.PK;
			ACharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			ACharge.JR_OSCostAmt = 200.00m;

			ACharge.JR_AL_ARLine = RevenueLine.PK;
			ACharge.JR_OSCostAmt = 300.00m;

			AssertEquals("Local COST amount should change if costs are not posted", 300.00m, ACharge.JR_LocalCostAmt);
			AssertEquals("OS REVENUE amount should not change if Revenue is already posted", 333.33m, ACharge.JR_OSSellAmt);
			AssertEquals("Local REVENUE amount should not change if Revenue is already posted", 333.33m, ACharge.JR_LocalSellAmt);
		}

		public void TestSettingOSRevenueAmountDoesntChangePostedCostAmounts()
		{
			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ACharge.JR_OSCostExRate = 1.00m;
			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			ACharge.JR_GB = GlbBranch.CurrentBranch.PK;
			ACharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			ACharge.JR_OSCostAmt = 200.00m;

			AssertEquals("Local Cost amount should be same as OS Cost (exchange rate 1.00)", 200.00m, ACharge.JR_LocalCostAmt);
			AssertEquals("OS Revenue amount should be same as declared amount", 333.33m, ACharge.JR_OSSellAmt);
			AssertEquals("Local Revenue amount should be same as OS amount (exchange rate 1.00)", 333.33m, ACharge.JR_LocalSellAmt);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = transactionLine.PK;
			ACharge.JR_OSSellAmt = 300.00m;

			AssertEquals("Local Cost amount should not change when cost is already posted", 200.00m, ACharge.JR_LocalCostAmt);
			AssertEquals("Overseas Cost amount should not change when cost is already posted", 200.00m, ACharge.JR_OSCostAmt);
			AssertEquals("OS Revenue Amount should change when revenue is not posted", 300.00m, ACharge.JR_LocalSellAmt);
		}

		public void TestSettingLocalCostAmountDoesntChangePostedRevenueAmounts()
		{
			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ACharge.JR_OSCostExRate = 1.00m;
			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			ACharge.JR_GB = GlbBranch.CurrentBranch.PK;
			ACharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			ACharge.JR_OSCostAmt = 200.00m;

			AssertEquals("Local Cost amount should be same as OS Cost (exchange rate 1.00)", 200.00m, ACharge.JR_LocalCostAmt);
			AssertEquals("OS Revenue amount should be same as declared amount", 333.33m, ACharge.JR_OSSellAmt);
			AssertEquals("Local Revenue amount should be same as OS amount (exchange rate 1.00)", 333.33m, ACharge.JR_LocalSellAmt);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = transactionLine.PK;
			ACharge.JR_LocalCostAmt = 300.00m;

			AssertEquals("Local cost amount should be set", 300.00m, ACharge.JR_LocalCostAmt);
			AssertEquals("OS Cost Amount should recalculate", 300.00m, ACharge.JR_OSCostAmt);
			AssertEquals("OS Revenue amount should remain unchanged", 333.33m, ACharge.JR_OSSellAmt);
			AssertEquals("Local Revenue amount should remain unchanged", 333.33m, ACharge.JR_LocalSellAmt);
		}

		public void TestSettingLocalRevenueAmountDoesntChangePostedCostAmounts()
		{
			ACharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ACharge.JR_OSCostExRate = 1.00m;
			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			ACharge.JR_GB = GlbBranch.CurrentBranch.PK;
			ACharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			ACharge.JR_OSCostAmt = 200.00m;

			AssertEquals("Local Cost amount should be same as OS Cost (exchange rate 1.00)", 200.00m, ACharge.JR_LocalCostAmt);
			AssertEquals("OS Cost amount should be the san=me", 200.00m, ACharge.JR_OSCostAmt);
			AssertEquals("OS Revenue amount should be same as declared amount", 333.33m, ACharge.JR_OSSellAmt);
			AssertEquals("Local Revenue amount should be same as OS amount (exchange rate 1.00)", 333.33m, ACharge.JR_LocalSellAmt);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = transactionLine.PK;
			ACharge.JR_LocalSellAmt = 300.00m;

			AssertEquals("Local sell amount should be set", 300.00m, ACharge.JR_LocalSellAmt);
			AssertEquals("OS Sell amount should recalculate", 300.00m, ACharge.JR_OSSellAmt);
			AssertEquals("OS Cost amount should remain unchanged", 200.00m, ACharge.JR_OSCostAmt);
			AssertEquals("Local cost amount should remain unchanged", 200.00m, ACharge.JR_LocalCostAmt);
		}

		public void TestRevenueSetsFieldsReadOnly()
		{
			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Revenue).PK;
			AssertEquals(true, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_AT_CostGSTRateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_AW_CostWHTRateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_CostReferenceInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_PaymentDateInfo.ReadOnly);
		}

		public void TestJR_AK()
		{
			ACharge.JR_AK = TestObjectCreator.USDChequeBook.PK;
			AssertEquals(TestObjectCreator.USDChequeBook.PK, ACharge.JR_AK);
		}

		public void TestJR_PaymentType()
		{
			ZGuid newGuid = ZGuid.NewZGuid();
			ACharge.JR_AK = newGuid;
			ACharge.JR_PaymentType = ReceiptTypes.Cheque;
			AssertEquals(ReceiptTypes.Cheque, ACharge.JR_PaymentType);
			AssertEquals(newGuid, ACharge.JR_AK);

			ACharge.JR_PaymentType = "abc";
			AssertEquals("abc", ACharge.JR_PaymentType);
			AssertEquals(ZGuid.Empty, ACharge.JR_AK);
		}

		public void TestJR_ChequeNoValidation()
		{
			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Disbursement).PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			ACharge.JR_ChequeNo = "a1234";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			ACharge.JR_ChequeNo = "a1234";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			ACharge.JR_ChequeNo = "a1234";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ACharge.JR_ChequeNo = "a1234";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ACharge.JR_ChequeNo = "a12,34";
			Assert("Error should be set", ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ACharge.JR_ChequeNo = "1234";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ACharge.JR_ChequeNo = "q1234";
			Assert("Error should not set", ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ACharge.JR_ChequeNo = "1 / - a 234";
			Assert("Error should not set", !ACharge.JR_ChequeNoInfo.HasErrors());
		}

		public void TestJR_ChequeNumberWithinBounds()
		{
			AccBankAccount bank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AccChequeBook book = TestObjectCreator.Factory.New<AccChequeBook>();
			book.AK_StartNo = 10;
			book.AK_LastNo = 100;
			book.AK_AB = bank.PK;
			TestObjectCreator.Factory.Save();

			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Disbursement).PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ACharge.JR_AK = book.PK;
			ACharge.JR_ChequeNo = "0";
			Assert("Error should be set", ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_ChequeNo = "10";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_ChequeNo = "100";
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_ChequeNo = "101";
			Assert("Error should be set", ACharge.JR_ChequeNoInfo.HasErrors());
		}

		public void TestJR_ChequeNoValidationIfChequePosted()
		{
			ZString testCheque = "123456";
			//Posting a cheque
			AccBankAccount account = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AccTransactionHeader payment = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.DirectPayment, LedgerTypes.CashBook);
			payment.AH_AB = account.PK;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_IsCancelled = false;
			payment.AH_ChequeOrReference = testCheque;

			AccChequeBook chequeBook = TestObjectCreator.Factory.New<AccChequeBook>();
			chequeBook.AK_StartNo = 123400;
			chequeBook.AK_LastNo = 123500;
			chequeBook.AK_AB = account.PK;
			chequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			chequeBook.AK_Code = TestObjectCreator.GetRandomString(5);
			TestObjectCreator.Factory.Save();

			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Disbursement).PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ACharge.JR_AK = chequeBook.PK;
			ACharge.JR_AB = account.PK;

			ACharge.JR_ChequeNo = testCheque;
			Assert("Error should not be set", !ACharge.JR_ChequeNoInfo.HasErrors());

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ACharge.JR_AB = account.PK;
			ACharge.Validation.ValidateJR_ChequeNo();
			Assert("Error should be set", ACharge.JR_ChequeNoInfo.HasErrors());
		}

		public void TestJR_ChequeOrReferenceLabel()
		{
			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("Reference #:", ACharge.JR_ChequeOrReferenceLabel);

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Cheque #:", ACharge.JR_ChequeOrReferenceLabel);

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("Reference #:", ACharge.JR_ChequeOrReferenceLabel);

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			AssertEquals("Reference #:", ACharge.JR_ChequeOrReferenceLabel);

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AssertEquals("Reference #:", ACharge.JR_ChequeOrReferenceLabel);
		}

		public void TestAKChangesChequeNumber()
		{
			AssertEquals("", ACharge.JR_ChequeNo);

			ACharge.JR_ChequeNo = "1241234";
			ACharge.JR_AK = Guid.NewGuid();
			Assert(ACharge.JR_ChequeNo != "1241234");
		}

		public void TestReadOnlyCostTabWithRevenueCharge()
		{
			ACharge.JR_OH_CostAccount = Guid.NewGuid();
			ACharge.JR_APInvoiceNum = "212314";
			ACharge.JR_APInvoiceDate = new ZDateTime(new DateTime(1999, 1, 1));
			ACharge.JR_PaymentType = "sal";
			ACharge.JR_PaymentDate = new ZDateTime(new DateTime(1999, 1, 1));
			ACharge.JR_AB = Guid.NewGuid();
			ACharge.JR_AK = Guid.NewGuid();
			ACharge.JR_ChequeNo = "293y";

			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Revenue).PK;

			AssertEquals(true, ACharge.JR_OH_CostAccountInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_CostReferenceInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_PaymentTypeInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_PaymentDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_ABInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_AKInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_ChequeNoInfo.ReadOnly);

			AssertEquals(ZGuid.Empty, ACharge.JR_OH_CostAccount);
			AssertEquals(ZString.Empty, ACharge.JR_APInvoiceNum);
			AssertEquals(ZDateTime.Empty, ACharge.JR_APInvoiceDate);
			AssertEquals(ZString.Empty, ACharge.JR_PaymentType);
			AssertEquals(ZDateTime.Empty, ACharge.JR_PaymentDate);
			AssertEquals(ZGuid.Empty, ACharge.JR_AB);
			AssertEquals(ZGuid.Empty, ACharge.JR_AK);
			AssertEquals(ZString.Empty, ACharge.JR_ChequeNo);
		}

		public void TestReadOnlyCostTabWithNonRevenueCharge()
		{
			AssertEquals(false, ACharge.JR_OH_CostAccountInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_CostReferenceInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_PaymentTypeInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_PaymentDateInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_ABInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_AKInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_ChequeNoInfo.ReadOnly);

			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Disbursement).PK;

			AssertEquals(false, ACharge.JR_OH_CostAccountInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_CostReferenceInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_PaymentTypeInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_PaymentDateInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_ABInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_AKInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_ChequeNoInfo.ReadOnly);

			AssertEquals(ZGuid.Empty, ACharge.JR_OH_CostAccount);
			AssertEquals(ZString.Empty, ACharge.JR_APInvoiceNum);
			AssertEquals(ZDateTime.Empty, ACharge.JR_APInvoiceDate);
			AssertEquals(ZString.Empty, ACharge.JR_PaymentType);
			AssertEquals(ZDateTime.Empty, ACharge.JR_PaymentDate);
			AssertEquals(ZGuid.Empty, ACharge.JR_AB);
			AssertEquals(ZGuid.Empty, ACharge.JR_AK);
			AssertEquals(ZString.Empty, ACharge.JR_ChequeNo);
		}

		public void TestLineCFX()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.TestOrganisation.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			TestJob.AgentCollectPK = TestObjectCreator.Debtor.PK;
			TestObjectCreator.Debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

			TestJob.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50m);
			TestJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 100m);

			ChargeWithCost aCharge = TestJob.Charges.AddNew();

			aCharge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			aCharge.JR_OSSellAmt = 1;

			aCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals(50m, aCharge.JR_LineCFX);

			aCharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals(100m, aCharge.JR_LineCFX);
		}

		public void TestCFXAmtIsRecalculatedWhenExchangeRateIsUpdated()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001000", ZECTRA, true, 10M, ABIGAS, true, 10M);
			var exchangeRate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			var charge = CreateCharge(job, MRG100, "Set CFX Values", TestObjectCreator.USD, 200M, AALSHI, TestObjectCreator.USD, 400M, ZECTRA);

			var cFXHeader = Factory.New<JCJournalHeader>();
			var cFXLine = cFXHeader.Lines.AddNew();
			cFXLine.SetCFXValues(job, charge);
			//charge.JR_AL_CFXLine = CFXLine.PK; //that's wrong for assigning of a CFX line means CFX is posted and then amount is taken from CFX line

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			cFXLine.SetCFXValues(job, charge); //recalculte explicitly. we normally can't change invoice type after posting (CFXLine is set during posting only)
			AssertEquals("CFX journal", 0M, charge.JR_LineCFX);
			AssertEquals("Line Amount", 0M, cFXLine.AL_LineAmount);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			cFXLine.SetCFXValues(job, charge); //recalculte explicitly. we normally can't change invoice type after posting (CFXLine is set during posting only)
			AssertEquals("CFX journal", 10M, charge.JR_LineCFX);
			AssertEquals("Line Amount, should be -(400/0.63 - 400/0.7) = -63.49", -63.49M, cFXLine.AL_LineAmount);

			exchangeRate.JF_BaseRate = 1.5M;
			cFXLine.SetCFXValues(job, charge);
			AssertEquals("Line Amount, should be -(400/1.35 - 400/1.5) = -29.63", -29.63M, cFXLine.AL_LineAmount);
		}

		public void TestCurrencyIsExcludedFromCFXCalculation()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var job = CreateJob("Z00001011", AALSHI, true, 10M, ABIGAS, true, 20M);
			job.PlugInData = shipment;
			job.JH_ParentTableCode = "aa";

			var rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);

			ZECTRA.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 5m);

			var otherOrgCharge = CreateCharge(job, MRG100, "Charge 1", TestObjectCreator.USD, 0m, null, TestObjectCreator.USD, 1000M, ZECTRA);
			AssertEquals("Other Org Charge has Other Org CFX adjusted ExRate = 0.7 x 0.95", 0.665m, otherOrgCharge.JR_OSSellExRate);

			TestObjectCreator.USD.RX_IsExcludedCFXCalculation = true;
			Factory.Save();
			otherOrgCharge.OnRevenueExchangeRateChanged_ForTestOnly(null, new EventArgs());
			AssertEquals("Other Org Charge has Other Org CFX adjusted ExRate = 0.7 x 1 as currency is excluded", 0.7m, otherOrgCharge.JR_OSSellExRate);
		}

		public void TestUpdateLineCFX()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			testDebtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			OrgHeader testAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			testAgent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			testAgent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 20m);

			OrgHeader otherOrg = orgFactory.NewWithValidTestData<OrgHeader>();
			otherOrg.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			otherOrg.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 70m);

			orgFactory.Save();

			using (Job job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.JH_ParentTableCode = "as";

				job.LocalChargesPK = testDebtor.PK;
				job.AgentCollectPK = testAgent.PK;

				Charge charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

				charge.JR_OH_SellAccount = testDebtor.PK;
				AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);

				charge.JR_OH_SellAccount = testAgent.PK;
				AssertEquals("Line CFX is taken from Job - same as org", 20m, charge.JR_LineCFX);

				job.ExchangeRates[1].JF_CFXPercent = 40m;

				AssertEquals("Line CFX is Job - different to org", 40m, charge.JR_LineCFX);

				charge.JR_OH_SellAccount = otherOrg.PK;
				AssertEquals("Line CFX is taken directly from org, as it's not an org on the job", 70m, charge.JR_LineCFX);
			}
		}

		public void TestUpdateLineCFXWhenSellInvoiceCurrencyIsSameAsSellCurrency()
		{
			#region Data Set Up

			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			var agent = orgFactory.NewWithValidTestData<OrgHeader>();
			agent.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			agent.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 20m);

			var otherOrg = orgFactory.NewWithValidTestData<OrgHeader>();
			otherOrg.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			otherOrg.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 70m);

			orgFactory.Save();

			#endregion

			using (var job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				job.LocalChargesPK = debtor.PK;
				job.AgentCollectPK = agent.PK;

				var usdRate = job.ExchangeRates.AddNew();
				usdRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
				usdRate.JF_BaseRate = 1.3m;

				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				charge.JR_OSSellAmt = 100m;
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.17m, charge.JR_OSSellExRate);

				charge.JR_OH_SellAccount = debtor.PK;
				AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals(1.17m, charge.JR_OSSellExRate);

				charge.JR_OH_SellAccount = agent.PK;
				AssertEquals("Line CFX is taken from Job - same as org", 20m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.04m, charge.JR_OSSellExRate);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.3m, charge.JR_OSSellExRate);

				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is not applicable when Sell Invoice Currency is same as Sell Currency", 0m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.3m, charge.JR_OSSellExRate);

				charge.JR_OH_SellAccount = agent.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is not applicable when Sell Invoice Currency is same as Sell Currency", 0m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.3m, charge.JR_OSSellExRate);

				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 30m);
				job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 40m);

				usdRate.JF_BaseRate = 1.3m; // force charge to update as changes in configuration are not picked up automatically

				AssertEquals("Line CFX is not applicable when Sell Invoice Currency is same as Sell Currency", 0m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.3m, charge.JR_OSSellExRate);

				charge.JR_OH_SellAccount = otherOrg.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
				AssertEquals("Line CFX is not applicable when Sell Invoice Currency is same as Sell Currency", 0m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Buy, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 1.3m, charge.JR_OSSellExRate);

				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				Assert(charge.BillInInvoiceCurrency);
				AssertEquals("Line CFX is taken directly from org, as it's not an org on the job", 70m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 0.39m, charge.JR_OSSellExRate); //can't be 1.3m - 70m cfx applied

				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job", 30m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 0.91m, charge.JR_OSSellExRate);

				charge.JR_OH_SellAccount = agent.PK;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.RX_Code;
				AssertEquals("Line CFX is taken from Job", 40m, charge.JR_LineCFX);
				AssertEquals("SellExchangeRateType", ExchangeRateType.Sell, charge.SellExchangeRateType);
				AssertEquals("JR_OSSellExRate", 0.78m, charge.JR_OSSellExRate);
			}
		}

		public void TestUpdateLineCFXDoesNotSetHasChangesWhenUpdatedToSameCFXPercent()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "INBOM";

			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 10m);

			orgFactory.Save();

			var job = JobInvoicing.Job.CreateWithMutex(Factory, shipment);
			job.LocalChargesPK = debtor.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			charge.JR_OH_SellAccount = debtor.PK;
			AssertEquals("Line CFX is taken from Job - same as org", 10m, charge.JR_LineCFX);

			Factory.Save();
			Assert(!charge.HasChanges);

			charge.UpdateLineCFX();
			AssertEquals("Line CFX should not be changed", 10m, charge.JR_LineCFX);
			Assert("No changes on the JR_LineCFXInfo", !charge.JR_LineCFXInfo.HasChanges);
			Assert("No changes on charge", !charge.HasChanges);
		}

		public void TestValidateJR_OSCostAmt()
		{
			RefCurrency foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "CAD"));

			ACharge.JR_OH_CostAccount = Guid.NewGuid();
			ACharge.JR_RX_NKCostCurrency = foreignCurrency.RX_Code;
			ACharge.JR_APInvoiceDate = ZDateTime.Empty;
			ACharge.JR_APInvoiceNum = "";

			ACharge.JR_OSCostAmt = 50;
			AssertEquals("Positive Cost Amount", 0, ACharge.JR_OSCostAmtInfo.GetErrors().Count());

			ACharge.JR_OSCostAmt = -50;
			ACharge.Validation.ValidateJR_OSCostAmt();
			AssertEquals("Negative Cost Amount", 1, ACharge.JR_OSCostAmtInfo.GetErrors().Count());

			ACharge.JR_APInvoiceDate = ZDateTime.Now;
			ACharge.Validation.ValidateJR_OSCostAmt();
			AssertEquals("AP Invoice Date not empty", 1, ACharge.JR_OSCostAmtInfo.GetErrors().Count());

			ACharge.JR_APInvoiceNum = "349";
			ACharge.Validation.ValidateJR_OSCostAmt();
			AssertEquals("AP Invoice Num not empty", 0, ACharge.JR_OSCostAmtInfo.GetErrors().Count());

			ACharge.JR_APInvoiceDate = ZDateTime.Empty;
			ACharge.Validation.ValidateJR_OSCostAmt();
			AssertEquals("Clear AP Invoice Date", 1, ACharge.JR_OSCostAmtInfo.GetErrors().Count());

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = transactionLine.PK;

			ACharge.Validation.ValidateJR_OSCostAmt();
			AssertEquals("don't care if posted", 0, ACharge.JR_OSCostAmtInfo.GetErrors().Count());
		}

		public virtual void TestValidateJR_OSSellAmt()
		{
			ACharge.JR_OSSellAmt = 0;
			AssertEquals(0, ACharge.JR_OSSellAmtInfo.GetErrors().Count());

			ACharge.JR_OSSellAmt = 1m;
			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Margin).PK;
			AssertEquals(1, ACharge.JR_OSSellAmtInfo.GetWarnings().GetUniqueMessageList().Length);

			ACharge.JR_OSSellAmt = 1m;
			ACharge.JR_AC = TestObjectCreator.GetChargeCode(Core.Constants.ChargeType.Disbursement).PK;
			AssertEquals(1, ACharge.JR_OSSellAmtInfo.GetWarnings().GetUniqueMessageList().Length);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = transactionLine.PK;

			ACharge.Validation.ValidateJR_OSSellAmt();
			AssertEquals("don't care for posted lines", 0, ACharge.JR_OSSellAmtInfo.GetWarnings().Count());

			ACharge.JR_OSSellAmt = 1m;
			TestObjectCreator.InsertMarginChargeCode(0);
			ACharge.JR_AC = TestObjectCreator.InsertMarginChargeCode(0).PK;
			AssertEquals(0, ACharge.JR_OSSellAmtInfo.GetWarnings().Count());
		}

		public void TestJR_SellReferenceReadOnly()
		{
			ACharge.JR_AL_ARLine = ZGuid.Empty;
			Assert("Sell Reference should not be readonly", !ACharge.JR_SellReferenceInfo.ReadOnly);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = transactionLine.PK;
			Assert("Sell Reference should be readonly", ACharge.JR_SellReferenceInfo.ReadOnly);

			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			Assert("Sell Reference should not be readonly", !ACharge.JR_SellReferenceInfo.ReadOnly);
		}

		public void TestCanDeleteIfRegistryIsSetToYes()
		{
			ACharge.JR_AL_APLine = ZGuid.Empty;
			ACharge.JR_AL_ARLine = ZGuid.Empty;

			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			ACharge.JR_CostRated = true;
			ACharge.JR_SellRated = true;
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			ACharge.JR_CostRated = true;
			ACharge.JR_SellRated = false;
			Assert(!string.IsNullOrEmpty(ACharge.ReasonForNotAbleToDelete));

			ACharge.JR_CostRated = false;
			ACharge.JR_SellRated = true;
			Assert(!string.IsNullOrEmpty(ACharge.ReasonForNotAbleToDelete));

			ACharge.JR_CostRated = true;
			ACharge.JR_SellRated = true;
			Assert(!string.IsNullOrEmpty(ACharge.ReasonForNotAbleToDelete));

			ACharge.JR_CostRated = false;
			ACharge.JR_SellRated = false;
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);
		}

		public void TestCanDeleteIfRevenuePosted()
		{
			ACharge.JR_AL_APLine = ZGuid.Empty;
			ACharge.JR_AL_ARLine = ZGuid.Empty;

			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			AccTransactionLines transactionLine2 = Factory.New<AccTransactionLines>();
			transactionLine2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = transactionLine2.PK;
			Assert(!string.IsNullOrEmpty(ACharge.ReasonForNotAbleToDelete));
		}

		public void TestCanDeleteIfCostPosted()
		{
			ACharge.JR_AL_APLine = ZGuid.Empty;
			ACharge.JR_AL_ARLine = ZGuid.Empty;

			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = transactionLine.PK;
			Assert(!string.IsNullOrEmpty(ACharge.ReasonForNotAbleToDelete));
		}

		public void TestCanDeleteIfApportioned()
		{
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			ACharge.JR_E6 = ZGuid.NewZGuid();
			Assert(!string.IsNullOrEmpty(ACharge.ReasonForNotAbleToDelete));
		}

		public void TestCanDeleteIfJobReadyToPostBothCostAndRevenue()
		{
			ACharge.FillWithValidTestData();
			Factory.Save();
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			ACharge.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
			AssertEquals("Job.IsReadyForCostPosting", true, ACharge.Job.IsReadyForCostPosting);
			AssertEquals("Job.IsReadyForRevenuePosting", true, ACharge.Job.IsReadyForRevenuePosting);
			AssertNotEquals("", ACharge.ReasonForNotAbleToDelete);
			AssertContains("because it is ready to be posted.", ACharge.ReasonForNotAbleToDelete);
		}

		public void TestCanDeleteIfJobReadyToPostCost()
		{
			ACharge.FillWithValidTestData();
			Factory.Save();
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			ACharge.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			AssertEquals("Job.IsReadyForCostPosting", true, ACharge.Job.IsReadyForCostPosting);
			AssertNotEquals("", ACharge.ReasonForNotAbleToDelete);
			AssertContains("because its cost part is ready to be posted.", ACharge.ReasonForNotAbleToDelete);
		}

		public void TestCanDeleteIfJobIsCancelled()
		{
			ACharge.FillWithValidTestData();
			Factory.Save();
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			ACharge.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			AssertEquals("Job.IsReadyForCostPosting", true, ACharge.Job.IsReadyForCostPosting);
			AssertNotEquals("", ACharge.ReasonForNotAbleToDelete);
			AssertContains("because its cost part is ready to be posted.", ACharge.ReasonForNotAbleToDelete);

			ACharge.Job.MarkAsInactive();

			AssertEquals("Charge is deleted.", true, ACharge.IsDeleted);
		}

		public void TestCanDeleteIfJobReadyToPostRevenue()
		{
			ACharge.FillWithValidTestData();
			Factory.Save();
			AssertEquals("", ACharge.ReasonForNotAbleToDelete);

			ACharge.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
			AssertEquals("Job.IsReadyForRevenuePosting", true, ACharge.Job.IsReadyForRevenuePosting);
			AssertNotEquals("", ACharge.ReasonForNotAbleToDelete);
			AssertContains("because its revenue part is ready to be posted.", ACharge.ReasonForNotAbleToDelete);
		}

		public void TestJR_AC_ReadOnly()
		{
			var cachedValueModifyRight = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;
			var orgPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = Factory.NewWithValidTestData<ChargeWithCost>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = orgPK;
			charge.CostAccount.CompanyData.SetAPTaxApplicable(false);
			charge.JR_E6 = ZGuid.Empty;
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			Assert("Pre-condition", !charge.IsRevenueCharge);
			Assert("Pre-condition", !charge.IsCostPosted);
			Assert("Pre-condition", !charge.JR_IsApportioned);
			Assert("Pre-condition", !charge.IsRevenuePostedWithManualJobRevenueJournal);
			Assert("Pre-condition", !charge.IsInDatabaseAndReadyForCostPosting);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_AC_ReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", charge.JR_AC_ReadOnly_ForTestOnly);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cachedValueModifyRight))
			{
				Assert("JR_AC_ReadOnly should be read only as the related job has Ready For Financial Closure status and user didn't has modify right", !charge.JR_AC_ReadOnly_ForTestOnly);
			}
		}

		public void TestDSBChargeExchangeRateChanges()
		{
			TestJob.JH_LocalChargesCFX = 0;
			TestJob.JH_AgentChargesCFX = 0;
			TestJob.ExchangeRates.RemoveAll();

			ExchangeRate rate = TestJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 0.6m;

			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;

			ACharge.JR_OSCostAmt = 100;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			rate.JF_BaseRate = 0.7m;
			AssertEquals(70m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			TestJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 10m);
			ACharge.JR_OH_SellAccount = TestJob.LocalChargesPK;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			AssertEquals(70m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);

			AssertEquals(70m, ACharge.JR_OSSellAmt);
			AssertEquals(111.11m, ACharge.JR_LocalSellAmt);
		}

		public void TestMRG100ChargeExchangeRateChanges()
		{
			TestJob.JH_LocalChargesCFX = 0;
			TestJob.JH_AgentChargesCFX = 0;
			TestJob.ExchangeRates.RemoveAll();

			ExchangeRate rate = TestJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 0.6m;

			ACharge.JR_AC = TestObjectCreator.MRG100.PK;

			ACharge.JR_OSCostAmt = 100;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			rate.JF_BaseRate = 0.7m;
			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(85.71m, ACharge.JR_LocalSellAmt);
		}

		public void TestMRG60ChargeExchangeRateChanges()
		{
			TestJob.JH_LocalChargesCFX = 0;
			TestJob.JH_AgentChargesCFX = 0;
			TestJob.ExchangeRates.RemoveAll();

			ExchangeRate rate = TestJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 0.6m;

			ACharge.JR_AC = TestObjectCreator.MRG60.PK;

			ACharge.JR_OSCostAmt = 100;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(166.67m, ACharge.JR_LocalSellAmt);

			rate.JF_BaseRate = 0.7m;
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(142.86m, ACharge.JR_LocalSellAmt);
		}

		public void TestNonAccrualChargeExchangeRateChanges()
		{
			TestJob.JH_LocalChargesCFX = 0;
			TestJob.JH_AgentChargesCFX = 0;
			TestJob.ExchangeRates.RemoveAll();

			ExchangeRate rate = TestJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 0.6m;

			ACharge.JR_AC = TestObjectCreator.NonAccrualChargeCode.PK;

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_LocalSellAmt = 100;

			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			rate.JF_BaseRate = 0.7m;
			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(85.71m, ACharge.JR_LocalSellAmt);
		}

		public void TestValidateJR_APInvoiceNumWhenInvoiceIsBlank()
		{
			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();
			TestObjectCreator utils = new TestObjectCreator(cleanFactory);

			OrgHeader billTo = cleanFactory.LoadTop1<OrgHeader>(CompanyDataQuery(OrgCompanyDataSchema.OB_IsDebtor));
			OrgHeader creditor = cleanFactory.LoadTop1<OrgHeader>(CompanyDataQuery(OrgCompanyDataSchema.OB_IsCreditor));

			ZQuery chargeFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Margin);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode mrgCode = cleanFactory.LoadTop1<AccChargeCode>(chargeFilter);

			Job job1 = utils.CreateJob(TestObjectCreator.GetRandomString(10), billTo, 10, null, 0);

			ChargeWithCost charge1 = job1.Charges.AddNew();
			charge1.JR_AC = mrgCode.PK;
			charge1.JR_LocalCostAmt = 10;
			charge1.JR_OH_CostAccount = creditor.PK;

			cleanFactory.Save();

			Job job2 = utils.CreateJob(TestObjectCreator.GetRandomString(10), billTo, 20, null, 0);
			ChargeWithCost charge2 = job2.Charges.AddNew();
			charge2.JR_AC = mrgCode.PK;
			charge2.JR_LocalCostAmt = 100;
			charge2.JR_OH_CostAccount = creditor.PK;
			charge2.Validation.ValidateJR_APInvoiceNum();
			AssertEquals("should be no error", 0, charge2.JR_APInvoiceNumInfo.GetErrors().Count());
		}

		public void TestValidateJR_Desc()
		{
			ACharge.JR_Desc = TestObjectCreator.GetRandomString(3);
			ACharge.Validation.ValidateJR_Desc();
			AssertEquals("JR_Desc should contain at least 3 characters", 0, ACharge.JR_DescInfo.GetErrors().Count());

			ACharge.JR_Desc = TestObjectCreator.GetRandomString(4);
			ACharge.Validation.ValidateJR_Desc();
			AssertEquals("JR_Desc should contain at least 3 characters", 0, ACharge.JR_DescInfo.GetErrors().Count());

			ACharge.JR_Desc = TestObjectCreator.GetRandomString(2);
			ACharge.Validation.ValidateJR_Desc();
			AssertEquals("JR_Desc cannot be empty", 0, ACharge.JR_DescInfo.GetErrors().Count());
		}

		public void TestJR_ACNotBeingSetTwice()
		{
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			AssertEquals("Currency shouldn't be changed. Code for JR_AC shouldn't be ran", TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
		}

		public void TestValuesAreNotBeingClearedInPostedRows()
		{
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;

			ACharge.JR_AL_APLine = CostLine.PK;
			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			AssertEquals("Currency shouldn't be changed. ", TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_AL_ARLine = RevenueLine.PK;
			ACharge.JR_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			AssertEquals("Currency shouldn't be changed. ", TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
		}

		public void TestIsChequeNoInUse()
		{
			ZGuid testBook = ZGuid.NewZGuid();
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ACharge.JR_AK = testBook;
			ACharge.JR_ChequeNo = "12345";

			ChargeWithCost charge1 = (ChargeWithCost)GetNewBusinessObject();
			charge1.JR_AC = TestObjectCreator.MRG100.PK;
			charge1.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			charge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge1.JR_AK = testBook;
			charge1.JR_ChequeNo = "12345";
			//Charge1.ValidateJR_ChequeNo();
			AssertEquals(0, charge1.JR_ChequeNoInfo.GetErrors().Count());

			ChargeWithCost charge2 = (ChargeWithCost)GetNewBusinessObject();
			charge2.JR_AC = TestObjectCreator.MRG100.PK;
			charge2.JR_OH_CostAccount = ZGuid.NewZGuid();
			charge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge2.JR_AK = testBook;
			charge2.JR_ChequeNo = "12345";
			//Charge2.ValidateJR_ChequeNo();
			AssertEquals(1, charge2.JR_ChequeNoInfo.GetErrors().Count());
		}

		#region Chain of events - a number of data entry tests

		public void TestDSBChargeAmountsWhenCFXisApplicable()
		{
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50m);
			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 60;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			AssertEquals(0.3m, ACharge.RevenueExchangeRate.SellRate);

			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(200m, ACharge.JR_LocalSellAmt);

			AssertEquals(false, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalSellAmtInfo.ReadOnly);

			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			AssertEquals(false, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalSellAmtInfo.ReadOnly);
		}

		public void TestChainOfEventsMargin100Percent()
		{
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.TestOrganisation.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50);

			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			//Testing of Margin 100%
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 60;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(0.3m, ACharge.RevenueExchangeRate.SellRate);
			AssertEquals(60m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSSellAmt = 80;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(80m, ACharge.JR_OSSellAmt);
			AssertEquals(133.33m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(133.33m, ACharge.JR_OSSellAmt);
			AssertEquals(133.33m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSSellAmt = 260;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(260m, ACharge.JR_OSSellAmt);
			AssertEquals(260m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSCostAmt = 120;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(120m, ACharge.JR_OSCostAmt);
			AssertEquals(200m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(260m, ACharge.JR_OSSellAmt);
			AssertEquals(260m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSCostAmt = 60;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(60m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(260m, ACharge.JR_OSSellAmt);
			AssertEquals(260m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(100m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(260m, ACharge.JR_OSSellAmt);
			AssertEquals(260m, ACharge.JR_LocalSellAmt);

			ACharge.JR_LocalCostAmt = 120;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(120m, ACharge.JR_OSCostAmt);
			AssertEquals(120m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(260m, ACharge.JR_OSSellAmt);
			AssertEquals(260m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSSellAmt = 150;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(120m, ACharge.JR_OSCostAmt);
			AssertEquals(120m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(150m, ACharge.JR_OSSellAmt);
			AssertEquals(150m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(72m, ACharge.JR_OSCostAmt);
			AssertEquals(120m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(150m, ACharge.JR_OSSellAmt);
			AssertEquals(150m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(72m, ACharge.JR_OSCostAmt);
			AssertEquals(120m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(45m, ACharge.JR_OSSellAmt);
			AssertEquals(75m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(120m, ACharge.JR_OSCostAmt);
			AssertEquals(120m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(45m, ACharge.JR_OSSellAmt);
			AssertEquals(75m, ACharge.JR_LocalSellAmt);
		}
		public void TestChainOfEventsMargin60Percent()
		{
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.TestOrganisation.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50);

			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			//Testing of Margin 60%
			ACharge.JR_AC = TestObjectCreator.MRG60.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_LocalCostAmt = 150;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;

			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(150m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(250m, ACharge.JR_OSSellAmt);
			AssertEquals(250m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(250m, ACharge.JR_OSSellAmt);
			AssertEquals(250m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSSellAmt = 120;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(120m, ACharge.JR_OSSellAmt);
			AssertEquals(120m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSCostAmt = 100;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(100m, ACharge.JR_OSCostAmt);
			AssertEquals(166.67m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(120m, ACharge.JR_OSSellAmt);
			AssertEquals(120m, ACharge.JR_LocalSellAmt);
		}

		public void TestChainOfEventsRevenue()
		{
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.TestOrganisation.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50);

			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			//Testing of Revenue
			ACharge.JR_AC = TestObjectCreator.RevenueChargeCode.PK;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(ZString.Empty, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(0m, ACharge.JR_OSCostAmt);
			AssertEquals(0m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(0m, ACharge.JR_OSSellAmt);
			AssertEquals(0m, ACharge.JR_LocalSellAmt);

			AssertEquals(true, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalSellAmtInfo.ReadOnly);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			ACharge.JR_OSSellAmt = 72;
			AssertEquals(ZString.Empty, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(0m, ACharge.JR_OSCostAmt);
			AssertEquals(0m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(0.3m, ACharge.RevenueExchangeRate.SellRate);
			AssertEquals(72m, ACharge.JR_OSSellAmt);
			AssertEquals(120m, ACharge.JR_LocalSellAmt);

			AssertEquals(true, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalSellAmtInfo.ReadOnly);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals(ZString.Empty, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(0m, ACharge.JR_OSCostAmt);
			AssertEquals(0m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(120m, ACharge.JR_OSSellAmt);
			AssertEquals(120m, ACharge.JR_LocalSellAmt);

			AssertEquals(true, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalSellAmtInfo.ReadOnly);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(ZString.Empty, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(0m, ACharge.JR_OSCostAmt);
			AssertEquals(0m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(120m, ACharge.JR_OSSellAmt);
			AssertEquals(120m, ACharge.JR_LocalSellAmt);

			AssertEquals(true, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalSellAmtInfo.ReadOnly);
		}

		public void TestChainOfEventsDisbursement()
		{
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50);

			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			//Testing of Disbursement
			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 90;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(150m, ACharge.JR_OSSellAmt);//300m
			AssertEquals(150m, ACharge.JR_LocalSellAmt);//300m

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(ACharge.RevenueExchangeRate.SellRate, 0.3m);
			AssertEquals(90m, ACharge.JR_OSSellAmt);
			AssertEquals(150m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSCostAmt = 100;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(100m, ACharge.JR_OSCostAmt);
			AssertEquals(166.67m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(166.67m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(166.67m, ACharge.JR_OSCostAmt);
			AssertEquals(166.67m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(166.67m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(166.67m, ACharge.JR_OSCostAmt);
			AssertEquals(166.67m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(166.67m, ACharge.JR_OSSellAmt);
			AssertEquals(166.67m, ACharge.JR_LocalSellAmt);
		}

		public void TestChainOfEventsNonAccrual()
		{
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.TestOrganisation.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50);

			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			//Testing of Non-Accrual
			ACharge.JR_AC = TestObjectCreator.NonAccrualChargeCode.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_OSCostAmt = 90;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(0.3m, ACharge.RevenueExchangeRate.SellRate);
			AssertEquals(00m, ACharge.JR_OSSellAmt);
			AssertEquals(0m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSSellAmt = 0;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(0m, ACharge.JR_OSSellAmt);
			AssertEquals(0m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OSSellAmt = 100;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(90m, ACharge.JR_OSCostAmt);
			AssertEquals(150m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(166.67m, ACharge.JR_OSSellAmt);
			AssertEquals(166.67m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			ACharge.JR_OSSellAmt = 100;
			ACharge.JR_LocalCostAmt = 100;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(100m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.AUD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(100m, ACharge.JR_OSCostAmt);
			AssertEquals(100m, ACharge.JR_LocalCostAmt);
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(30m, ACharge.JR_OSSellAmt);
			AssertEquals(50m, ACharge.JR_LocalSellAmt);
		}

		#endregion

		#region Entering Cost from Revenue

		public void TestSettingCostFromRevenueAUD2AUD()
		{
			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.InsertMarginChargeCode(80).PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_LocalCostAmt = 0;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_OSSellAmt = 100;
			AssertEquals(80m, ACharge.JR_OSCostAmt);
		}

		public void TestSettingCostFromRevenueAUD2USD()
		{
			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.InsertMarginChargeCode(80).PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_LocalCostAmt = 0;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSSellAmt = 100;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(80m, ACharge.JR_OSCostAmt);
			AssertEquals(133.33m, ACharge.JR_LocalCostAmt);
		}

		public void TestSettingCostFromRevenueUSD2USD()
		{
			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.InsertMarginChargeCode(80).PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 0;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSSellAmt = 100;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals(TestObjectCreator.USD.RX_Code, ACharge.JR_RX_NKCostCurrency);
			AssertEquals(80m, ACharge.JR_OSCostAmt);
			AssertEquals(133.33m, ACharge.JR_LocalCostAmt);
		}

		public void TestSettingCostFromRevenueUSD2AUD()
		{
			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.InsertMarginChargeCode(80).PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 0;
			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			ACharge.JR_OSSellAmt = 100;
			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals(80m, ACharge.JR_OSCostAmt);
		}

		#endregion

		public void TestSellAccountChanged()
		{
			ZGuid billTo = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.TestOrganisation.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			ZGuid agent = TestObjectCreator.Debtor.PK;
			TestObjectCreator.Debtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			TestJob.AgentCollectPK = agent;
			TestJob.LocalChargesPK = billTo;

			USDRate.JF_BaseRate = 0.5m;

			ACharge.JR_OH_SellAccount = billTo;

			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_RX_NKCostCurrency = USDRate.JF_RX_NKRateCurrency;
			ACharge.JR_OSCostAmt = 100;

			AssertEquals(USDRate.JF_RX_NKRateCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(200m, ACharge.JR_LocalSellAmt);

			var rate = ACharge.InvoicingJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = USDRate.JF_RX_NKRateCurrency;
			rate.JF_OH_Org = agent;
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_BaseRate = 0.7m;

			ACharge.JR_OH_SellAccount = agent;

			AssertEquals(USDRate.JF_RX_NKRateCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(142.86m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OH_SellAccount = billTo;
			AssertEquals(USDRate.JF_RX_NKRateCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(200m, ACharge.JR_LocalSellAmt);
		}

		public void TestSellAccountChangedWhenSellCurrencyIsLocal()
		{
			ZGuid billTo = TestObjectCreator.TestOrganisation.PK;
			ZGuid agent = TestObjectCreator.Debtor.PK;
			TestJob.AgentCollectPK = agent;
			TestJob.LocalChargesPK = billTo;

			USDRate.JF_BaseRate = 0.5m;

			ACharge.JR_OH_SellAccount = billTo;

			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_RX_NKCostCurrency = USDRate.JF_RX_NKRateCurrency;
			ACharge.JR_OSCostAmt = 100;
			ACharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(200m, ACharge.JR_OSSellAmt);
			AssertEquals(200m, ACharge.JR_LocalSellAmt);

			var rate = ACharge.InvoicingJob.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = USDRate.JF_RX_NKRateCurrency;
			rate.JF_OH_Org = agent;
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_BaseRate = 0.7m;

			ACharge.JR_OH_SellAccount = agent;

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(200m, ACharge.JR_OSSellAmt);
			AssertEquals(200m, ACharge.JR_LocalSellAmt);

			ACharge.JR_OH_SellAccount = billTo;
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ACharge.JR_RX_NKSellCurrency);
			AssertEquals(200m, ACharge.JR_OSSellAmt);
			AssertEquals(200m, ACharge.JR_LocalSellAmt);
		}

		public void TestAPInvoiceDateDoesntThrowException()
		{
			ACharge.JR_PaymentDate = ZDateTime.Empty;
			OrgHeader costAccount = Factory.New<OrgHeader>();
			costAccount.OH_IsCreditor = ZBool.True;
			costAccount.CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromShipmentDate;
			ACharge.JR_OH_CostAccount = costAccount.PK;
			ACharge.JR_APInvoiceDate = ZDateTime.Invalid;
			AssertEquals("Payment date shouldn't be changed", ZDateTime.Empty, ACharge.JR_PaymentDate);
		}

		public void TestAPInvoiceDateSetPaymentDateIfCreditorHasDefaultTerm()
		{
			ACharge.JR_PaymentDate = ZDateTime.Empty;
			OrgHeader costAccount = Factory.New<OrgHeader>();
			costAccount.OH_IsCreditor = ZBool.True;
			costAccount.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			costAccount.APSettlementGroupPK = Factory.New<OrgHeader>().PK;
			costAccount.APSettlementGroup.CompanyData.OB_APPaymentTerms = Core.Constants.InvoiceTerms.FromInvoiceDate;
			costAccount.APSettlementGroup.CompanyData.OB_APPaymentTermDays = 3;
			ACharge.JR_OH_CostAccount = costAccount.PK;
			ACharge.JR_APInvoiceDate = ZDateTime.Now;
			AssertEquals("Payment date", ACharge.JR_APInvoiceDate.AddDays(3), ACharge.JR_PaymentDate);
		}

		public void TestCostCurrencyChangeDoesntChangePostedRevenueAmount()
		{
			ACharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ACharge.JR_OSSellAmt = 100;
			ACharge.JR_LocalSellAmt = 100;

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = transactionLine.PK;

			RefCurrency currency = Factory.New<RefCurrency>();
			ACharge.JR_RX_NKCostCurrency = currency.RX_Code;
			AssertEquals(100m, ACharge.JR_OSSellAmt);
			AssertEquals(100m, ACharge.JR_LocalSellAmt);
		}

		public void TestJR_ChequeNumberSetWhenChequeBookIsSet()
		{
			AccChequeBook chequeBook1 = Factory.New<AccChequeBook>();
			chequeBook1.AK_StartNo = 1000;
			chequeBook1.AK_LastNo = 1999;
			chequeBook1.AK_CurrentNo = 1493;

			AccChequeBook chequeBook2 = Factory.New<AccChequeBook>();
			chequeBook2.AK_StartNo = 2000;
			chequeBook2.AK_LastNo = 2999;
			chequeBook2.AK_CurrentNo = 2875;

			ACharge.JR_AK = chequeBook1.PK;
			AssertEquals("Cheque Number", "1493", ACharge.JR_ChequeNo);

			ACharge.JR_AK = ZGuid.Empty;
			AssertEquals("Cheque Number", "", ACharge.JR_ChequeNo);

			ACharge.JR_AK = ZGuid.Invalid;
			AssertEquals("Cheque Number", "", ACharge.JR_ChequeNo);

			ACharge.JR_AK = chequeBook2.PK;
			AssertEquals("Cheque Number", "2875", ACharge.JR_ChequeNo);

			ACharge.JR_AK = ZGuid.Missing;
			AssertEquals("Cheque Number", "", ACharge.JR_ChequeNo);
		}

		public void TestPaymentTypeCashSetsDescription()
		{
			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("", ACharge.JR_ChequeNo);

			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("CASH", ACharge.JR_ChequeNo);
		}

		public void TestPostDSBRevenueStopsCostFromChanging()
		{
			//Preconditions
			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 60;

			AccTransactionLines revenueLine = Factory.New<AccTransactionLines>();
			revenueLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = revenueLine.PK;

			ACharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals(false, ACharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			AssertEquals(false, ACharge.JR_OH_CostAccountInfo.ReadOnly);

			AssertEquals(true, ACharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_LocalSellAmtInfo.ReadOnly);
			AssertEquals(true, ACharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
		}

		public void TestDisbursementChargeLocalCostChangeUpdatesRevenue()
		{
			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
			ACharge.JR_LocalCostAmt = 100;

			AssertEquals("OS Cost amount must be updated", 100m, ACharge.JR_OSCostAmt);
			AssertEquals("Local cost", 100m, ACharge.JR_LocalCostAmt);

			AssertEquals("OS sell amount must be updated", 100m, ACharge.JR_OSSellAmt);
			AssertEquals("Local sell amount must be updated", 100m, ACharge.JR_LocalSellAmt);

			ACharge.JR_LocalCostAmt = 200;
			AssertEquals("OS Cost amount must be updated", 200m, ACharge.JR_OSCostAmt);
			AssertEquals("Local cost amount must be updated", 200m, ACharge.JR_LocalCostAmt);

			AssertEquals("OS sell amount must be updated", 200m, ACharge.JR_OSSellAmt);
			AssertEquals("Local sell amount must be updated", 200m, ACharge.JR_LocalSellAmt);
		}

		public void TestDisbursementChargeOSCostChangeUpdatesRevenue()
		{
			// Preconditions
			TestObjectCreator.Creditor1.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			TestObjectCreator.Creditor1.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

			TestJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50m);

			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 100;

			AssertEquals(0.3m, ACharge.RevenueExchangeRate.SellRate);

			AssertEquals("OS Cost", 100m, ACharge.JR_OSCostAmt);
			AssertEquals("Local cost must be updated", 166.67m, ACharge.JR_LocalCostAmt);

			AssertEquals("OS sell amount must be updated", 100m, ACharge.JR_OSSellAmt);
			AssertEquals("Local sell amount must be updated", 333.33m, ACharge.JR_LocalSellAmt);

			ACharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("OS Cost", 100m, ACharge.JR_OSCostAmt);
			AssertEquals("Local cost must be updated", 166.67m, ACharge.JR_LocalCostAmt);

			AssertEquals("OS sell amount must be updated", 166.67m, ACharge.JR_OSSellAmt);//local currency => no cfx
			AssertEquals("Local sell amount must be updated", 166.67m, ACharge.JR_LocalSellAmt);//local currency => no cfx

			ACharge.JR_OSCostAmt = 200;
			AssertEquals("OS Cost amount must be updated", 200m, ACharge.JR_OSCostAmt);
			AssertEquals("Local cost amount must be updated", 333.33m, ACharge.JR_LocalCostAmt);

			AssertEquals("OS sell amount must be updated", 333.33m, ACharge.JR_OSSellAmt);
			AssertEquals("Local sell amount must be updated", 333.33m, ACharge.JR_LocalSellAmt);
		}

		public void TestDisbursementChargeDoesNoSetSellAmountsWhenReloadingChargesAndInialisingRevenueExchangeRate()
		{
			Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			// Preconditions
			TestObjectCreator.Creditor1.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
			TestObjectCreator.Creditor1.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;

			AssertEquals(TestObjectCreator.USD.RX_Code, USDRate.JF_RX_NKRateCurrency);
			AssertEquals(0.6m, USDRate.JF_BaseRate);

			ACharge.JR_AC = TestObjectCreator.DSBChargeCode.PK;
			ACharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			ACharge.JR_OSCostAmt = 1000m;

			AssertEquals(0.6m, ACharge.RevenueExchangeRate.SellRate);

			AssertEquals("OS Cost", 1000m, ACharge.JR_OSCostAmt);
			AssertEquals("Local Cost", 1666.67m, ACharge.JR_LocalCostAmt);

			AssertEquals("OS sell amount must be updated", 1000m, ACharge.JR_OSSellAmt);
			AssertEquals("Local sell amount must be updated", 1666.67m, ACharge.JR_LocalSellAmt);

			// Another Charge to trigger ChargeReloader later
			var anotherCharge = TestObjectCreator.CreateCharge(ACharge.InvoicingJob, TestObjectCreator.FRT, "Another Charge", TestObjectCreator.USD, 200m, TestObjectCreator.Creditor1, "I0001", TestObjectCreator.USD);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newCreator = new TestObjectCreator(newFactory);

			var invoice = newCreator.CreateInvoice(typeof(APInvoice), newCreator.USD, 0.6m, newFactory.Load<OrgHeader>(TestObjectCreator.Creditor1.PK));
			var costLine = newCreator.CreateInvoiceLine(invoice, newCreator.USD, 0.6m, 1000m);
			var charge = newFactory.Load<Charge>(ACharge.PK);
			charge.ReverseWIP(ZDateTime.Today);
			charge.ReverseAccrual(ZDateTime.Today);
			charge.JR_AL_APLine = costLine.PK;
			charge.SetAmountsToLinkedLinesForTests();

			newFactory.Save();

			ACharge.JR_OSSellAmt = 0m;
			Assert(ACharge.JR_OSSellAmt.IsEmpty);
			Assert(ACharge.JR_LocalSellAmt.IsEmpty);

			Factory.Save();

			Assert("OS Sell amounts should not be restored by ChargeReloader", ACharge.JR_OSSellAmt.IsEmpty);
			Assert("Local Sell amounts should not be restored by ChargeReloader", ACharge.JR_LocalSellAmt.IsEmpty);

			newFactory = new BusinessObjectFactory();
			newFactory.SetContext(BusinessContext.InvoicingPlugInGUI);
			var loadedCharge = newFactory.Load<Charge>(ACharge.PK);

			AssertNotNull("Assert RevenueExchangeRate is initialised", loadedCharge.RevenueExchangeRate);
			Assert("OS Sell Amount should stay cleared", loadedCharge.JR_OSSellAmt.IsEmpty);
			Assert("Local Sell Amount should stay cleared", loadedCharge.JR_OSSellAmt.IsEmpty);
			Assert("Loaded Charge should not have changes", !loadedCharge.HasChanges);
		}

		public void TestIsApportionedSetsReadonly()
		{
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			ACharge.JR_PaymentType = ReceiptTypes.Cheque;
			Assert("Is not apportioned, Invoice num should not be readonly", !ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("Is not apportioned, overseas cost currency should not be readonly", !ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Is not apportioned, Invoice date should not be readonly", !ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("Is not apportioned, Document Received date should not be readonly", !ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("Is not apportioned, Supplier Cost Reference should not be readonly", !ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("Is not apportioned, Bank account should not be readonly", !ACharge.JR_ABInfo.ReadOnly);
			Assert("Is not apportioned, Payment date should not be readonly", !ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("Is not apportioned, Cheque number should not be readonly", !ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("Is not apportioned, Cheque book should not be readonly", !ACharge.JR_AKInfo.ReadOnly);
			Assert("Is not apportioned, Payment type should not be readonly", !ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("Is not apportioned, Creditor should not be readonly", !ACharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("Is not apportioned, Overseas cost amount should not be readonly", !ACharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Is not apportioned, local cost amount should not be readonly", !ACharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Is not apportioned, Charge code should not be readonly", !ACharge.JR_ACInfo.ReadOnly);
			Assert("Is not apportioned, Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);
			Assert("Is not apportioned, department should not be readonly", !ACharge.JR_GEInfo.ReadOnly);
			Assert("Is not apportioned, branch should not be readonly", !ACharge.JR_GBInfo.ReadOnly);

			//Is apportioned
			ACharge.JR_E6 = ZGuid.NewZGuid();
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();

			Assert("Is apportioned, Invoice num should be readonly", ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("Is apportioned, overseas cost currency should not be readonly", ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Is apportioned, Invoice date should be readonly", ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("Is apportioned, Document Received date should be readonly", ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("Is apportioned, Supplier Cost Reference should be readonly", ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("Is apportioned, Bank account should be readonly", ACharge.JR_ABInfo.ReadOnly);
			Assert("Is apportioned, Payment date should be readonly", ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("Is apportioned, Cheque number should be readonly", ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("Is apportioned, Cheque book should be readonly", ACharge.JR_AKInfo.ReadOnly);
			Assert("Is apportioned, Payment type should be readonly", ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("Is apportioned, Creditor should be readonly", ACharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("Is apportioned, Overseas cost amount should be readonly", ACharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Is apportioned, local cost amount should be readonly", ACharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Is apportioned, Charge code should be readonly", ACharge.JR_ACInfo.ReadOnly);
			Assert("Is apportioned, Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);
			Assert("Is apportioned, department should be readonly", ACharge.JR_GEInfo.ReadOnly);
			Assert("Is apportioned, branch should be readonly", ACharge.JR_GBInfo.ReadOnly);
		}

		public void TestIsCostPostedSetsReadonly()
		{
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			ACharge.JR_PaymentType = ReceiptTypes.Cheque;
			Assert("Cost is not posted, Invoice num should not be readonly", !ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("Cost is not posted, overseas cost currency should not be readonly", !ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Cost is not posted, Invoice date should not be readonly", !ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("Cost is not posted, Document Received date should not be readonly", !ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("Cost is not posted, Supplier Cost Reference should not be readonly", !ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("Cost is not posted, Bank account should not be readonly", !ACharge.JR_ABInfo.ReadOnly);
			Assert("Cost is not posted, Payment date should not be readonly", !ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("Cost is not posted, Cheque number should not be readonly", !ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("Cost is not posted, Cheque book should not be readonly", !ACharge.JR_AKInfo.ReadOnly);
			Assert("Cost is not posted, Payment type should not be readonly", !ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("Cost is not posted, Creditor should not be readonly", !ACharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("Cost is not posted, Overseas cost amount should not be readonly", !ACharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Cost is not posted, local cost amount should not be readonly", !ACharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Cost is not posted, Charge code should not be readonly", !ACharge.JR_ACInfo.ReadOnly);
			Assert("Cost is not posted, Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);
			Assert("Cost is not posted, department should not be readonly", !ACharge.JR_GEInfo.ReadOnly);
			Assert("Cost is not posted, branch should not be readonly", !ACharge.JR_GBInfo.ReadOnly);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = transactionLine.PK;
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();

			Assert("Cost is posted, Invoice num should be readonly", ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("Cost is posted, overseas cost currency should not be readonly", ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("Cost is posted, Invoice date should be readonly", ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("Cost is posted, Document Received date should be readonly", ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("Cost is posted, Supplier Cost Reference should be readonly", ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("Cost is posted, Bank account should be readonly", ACharge.JR_ABInfo.ReadOnly);
			Assert("Cost is posted, Payment date should be readonly", ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("Cost is posted, Cheque number should be readonly", ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("Cost is posted, Cheque book should be readonly", ACharge.JR_AKInfo.ReadOnly);
			Assert("Cost is posted, Payment type should be readonly", ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("Cost is posted, Creditor should be readonly", ACharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("Cost is posted, Overseas cost amount should be readonly", ACharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("Cost is posted, local cost amount should be readonly", ACharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("Cost is posted, Charge code should be readonly", ACharge.JR_ACInfo.ReadOnly);
			Assert("Cost is posted, Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);
			Assert("Cost is posted, department should be readonly", ACharge.JR_GEInfo.ReadOnly);
			Assert("Cost is posted, branch should be readonly", ACharge.JR_GBInfo.ReadOnly);
		}

		public void TestPostedJobRevenueJournalSetsCostReadonly()
		{
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			ACharge.JR_PaymentType = ReceiptTypes.Cheque;
			Assert("JobRevenueJournal is not posted, Invoice num should not be readonly", !ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, overseas cost currency should not be readonly", !ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Invoice date should not be readonly", !ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Document Received date should not be readonly", !ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Supplier Cost Reference should not be readonly", !ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Bank account should not be readonly", !ACharge.JR_ABInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Payment date should not be readonly", !ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Cheque number should not be readonly", !ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Cheque book should not be readonly", !ACharge.JR_AKInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Payment type should not be readonly", !ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Creditor should not be readonly", !ACharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Overseas cost amount should not be readonly", !ACharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, local cost amount should not be readonly", !ACharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Charge code should not be readonly", !ACharge.JR_ACInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, department should not be readonly", !ACharge.JR_GEInfo.ReadOnly);
			Assert("JobRevenueJournal is not posted, branch should not be readonly", !ACharge.JR_GBInfo.ReadOnly);

			JobRevenueJournal journal = Factory.New<JobRevenueJournal>();
			ACharge.JR_AL_ARLine = journal.Lines.AddNew().PK;

			Assert("JobRevenueJournal is posted, Invoice num should be readonly", ACharge.JR_APInvoiceNumInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, overseas cost currency should not be readonly", ACharge.JR_RX_NKCostCurrencyInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Invoice date should be readonly", ACharge.JR_APInvoiceDateInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Document Received date should be readonly", ACharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Supplier Cost Reference should be readonly", ACharge.JR_CostReferenceInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Bank account should be readonly", ACharge.JR_ABInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Payment date should be readonly", ACharge.JR_PaymentDateInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Cheque number should be readonly", ACharge.JR_ChequeNoInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Cheque book should be readonly", ACharge.JR_AKInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Payment type should be readonly", ACharge.JR_PaymentTypeInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Creditor should be readonly", ACharge.JR_OH_CostAccountInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Overseas cost amount should be readonly", ACharge.JR_OSCostAmtInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, local cost amount should be readonly", ACharge.JR_LocalCostAmtInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Charge code should be readonly", ACharge.JR_ACInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, Description should not be readonly", ACharge.JR_DescInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, department should be readonly", ACharge.JR_GEInfo.ReadOnly);
			Assert("JobRevenueJournal is posted, branch should be readonly", ACharge.JR_GBInfo.ReadOnly);
		}

		public void TestDescriptionIsEditableWhenOnlyCostIsPosted()
		{
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			Assert("Cost & Revenue are not posted, not part of an apportionment, the Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);

			AccTransactionLines aPTransactionLine = Factory.New<AccTransactionLines>();
			aPTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ACharge.JR_AL_APLine = aPTransactionLine.PK;
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			Assert("Cost is posted, Description should not be readonly", !ACharge.JR_DescInfo.ReadOnly);

			ACharge.JR_E6 = ZGuid.NewZGuid();
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			Assert("Cost is apportioned, Description should be not be readonly", !ACharge.JR_DescInfo.ReadOnly);

			ACharge.JR_E6 = ZGuid.Empty;
			AccTransactionLines aRTransactionLine = Factory.New<AccTransactionLines>();
			aRTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ACharge.JR_AL_ARLine = aRTransactionLine.PK;
			ACharge.UpdateCostFieldsReadOnly_ForTestOnly();
			ACharge.UpdateCoreFieldsReadOnly_ForTestOnly();
			Assert("Cost is not posted, Revenue is Posted, Description should be readonly", ACharge.JR_DescInfo.ReadOnly);
		}

		public void TestPullingAPInvoiceDataFromExisitingCharges()
		{
			ZString invoiceNumber1 = TestObjectCreator.GetRandomString(10);
			ZString invoiceNumber2 = TestObjectCreator.GetRandomString(10);
			ZDateTime invoice1Date = ZDateTime.Today.AddDays(3);
			ZDateTime invoice2Date = ZDateTime.Today.AddDays(5);
			ZDateTime invoice1DueDate = ZDateTime.Today.AddDays(13);
			ZDateTime invoice2DueDate = ZDateTime.Today.AddDays(30);
			ZString supplierCostReference = TestObjectCreator.GetRandomString(10);

			OrgHeader creditor1 = Factory.New<OrgHeader>();
			creditor1.OH_Code = "TEST1";
			creditor1.OH_IsCreditor = true;

			OrgHeader creditor2 = Factory.New<OrgHeader>();
			creditor2.OH_Code = "TEST2";
			creditor2.OH_IsCreditor = true;

			OrgHeader debtor1 = Factory.New<OrgHeader>();
			debtor1.OH_Code = "TEST3";
			debtor1.OH_IsDebtor = true;

			OrgHeader debtor2 = Factory.New<OrgHeader>();
			debtor2.OH_Code = "TEST4";
			debtor2.OH_IsDebtor = true;

			Factory.Save();

			AccBankAccount bank1 = Factory.New<AccBankAccount>();
			AccBankAccount bank2 = Factory.New<AccBankAccount>();

			AccChequeBook book1 = Factory.New<AccChequeBook>();
			AccChequeBook book2 = Factory.New<AccChequeBook>();

			AccChargeCode mRGCode1 = Factory.New<AccChargeCode>();
			mRGCode1.AC_ChargeType = Core.Constants.ChargeType.Margin;

			AccChargeCode mRGCode2 = Factory.New<AccChargeCode>();
			mRGCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;

			AccChargeCode mRGCode3 = Factory.New<AccChargeCode>();
			mRGCode3.AC_ChargeType = Core.Constants.ChargeType.Margin;

			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");

			TestObjectCreator utils = new TestObjectCreator(Factory);

			ExchangeRate uSDRate = utils.CreateExchangeRate(TestJob, TestObjectCreator.USD, 0.6m);
			ChargeWithCost testCharge1 = utils.CreateCharge(TestJob, mRGCode1, "test1", TestObjectCreator.AUD, 100, null, TestObjectCreator.AUD, 100, debtor1);
			ChargeWithCost testCharge2 = utils.CreateCharge(TestJob, mRGCode2, "test2", TestObjectCreator.AUD, 200, null, TestObjectCreator.AUD, 200, debtor2);
			ChargeWithCost testCharge3 = utils.CreateCharge(TestJob, mRGCode3, "test3", TestObjectCreator.USD, 300, null, TestObjectCreator.AUD, 300, debtor1);

			testCharge1.JR_OH_CostAccount = creditor1.PK;
			testCharge1.JR_APInvoiceNum = invoiceNumber1;
			testCharge1.JR_APInvoiceDate = invoice1Date;
			testCharge1.JR_CostReference = supplierCostReference;
			testCharge1.JR_PaymentDate = invoice1DueDate;
			testCharge1.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			bank1.AB_ChequeNumDigits = (ZByte)5;
			testCharge1.JR_AB = bank1.PK;
			testCharge1.JR_AK = book1.PK;
			testCharge1.JR_ChequeNo = "5";

			AssertEquals(creditor1.PK, testCharge1.JR_OH_CostAccount);
			AssertEquals(invoiceNumber1, testCharge1.JR_APInvoiceNum);
			AssertEquals(invoice1Date, testCharge1.JR_APInvoiceDate);
			AssertEquals(invoice1DueDate, testCharge1.JR_PaymentDate);
			AssertEquals(supplierCostReference, testCharge1.JR_CostReference);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, testCharge1.JR_PaymentType);
			AssertEquals(bank1.PK, testCharge1.JR_AB);
			AssertEquals(book1.PK, testCharge1.JR_AK);
			AssertEquals("00005", testCharge1.JR_ChequeNo);

			testCharge2.JR_OH_CostAccount = creditor1.PK;
			testCharge2.JR_APInvoiceNum = invoiceNumber1;

			AssertEquals(creditor1.PK, testCharge2.JR_OH_CostAccount);
			AssertEquals(invoiceNumber1, testCharge2.JR_APInvoiceNum);
			AssertEquals(invoice1Date, testCharge2.JR_APInvoiceDate);
			AssertEquals(invoice1DueDate, testCharge2.JR_PaymentDate);
			AssertEquals(supplierCostReference, testCharge2.JR_CostReference);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, testCharge2.JR_PaymentType);
			AssertEquals(bank1.PK, testCharge2.JR_AB);
			AssertEquals(book1.PK, testCharge2.JR_AK);
			AssertEquals("00005", testCharge2.JR_ChequeNo);

			testCharge3.JR_OH_CostAccount = creditor1.PK;
			testCharge3.JR_APInvoiceNum = invoiceNumber1;
			testCharge3.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;

			AssertEquals(creditor1.PK, testCharge3.JR_OH_CostAccount);
			AssertEquals(invoiceNumber1, testCharge3.JR_APInvoiceNum);
			AssertEquals(invoice1Date, testCharge3.JR_APInvoiceDate);
			AssertEquals(invoice1DueDate, testCharge3.JR_PaymentDate);
			AssertEquals(supplierCostReference, testCharge3.JR_CostReference);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, testCharge3.JR_PaymentType);
			AssertEquals(bank1.PK, testCharge3.JR_AB);
			AssertEquals(book1.PK, testCharge3.JR_AK);
			AssertEquals("00005", testCharge3.JR_ChequeNo);
		}

		public void TestChequeBookMustBePresentAnyway()
		{
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_LocalCostAmt = 100;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;
			ACharge.JR_APInvoiceNum = "1234";
			ACharge.JR_APInvoiceDate = TestObjectCreator.Today;
			ACharge.JR_PaymentDate = TestObjectCreator.Tomorrow;
			ACharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ACharge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			ACharge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			ACharge.JR_ChequeNo = "100";

			ACharge.JR_AK = ZGuid.Empty;
			ACharge.Validation.ValidateJR_AK();
			AssertEquals("must be an error, cheque book must be present", 1, ACharge.JR_AKInfo.GetErrors().Count());
		}

		public void TestJR_Desc_ReadOnly()
		{
			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = true;
			var charge = CreateChargeWithCost();

			var securityCheckPoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.ModifyDefaultChargeCodeDescription);

			Assert(securityCheckPoint.IsAllowed);
			Assert(!charge.JR_Desc_ReadOnly_ForTestOnly);

			var cacheValue = charge.Job.JH_Status;
			using (new DisposableAction(() => charge.Job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code, () => charge.Job.JH_Status = cacheValue))
			{
				Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
				Assert(charge.JR_Desc_ReadOnly_ForTestOnly);

				Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true;
				Assert(!charge.JR_Desc_ReadOnly_ForTestOnly);
			}

			securityCheckPoint.IsAllowed = false;
			Assert(!securityCheckPoint.IsAllowed);
			Assert(charge.JR_Desc_ReadOnly_ForTestOnly);

			TestObjectCreator.CC1.AC_AllowDescriptionOvertype = false;

			securityCheckPoint.IsAllowed = true;
			Assert(securityCheckPoint.IsAllowed);
			Assert(charge.JR_Desc_ReadOnly_ForTestOnly);

			securityCheckPoint.IsAllowed = false;
			Assert(!securityCheckPoint.IsAllowed);
			Assert(charge.JR_Desc_ReadOnly_ForTestOnly);
		}

		#region JR_ChequeNoDigits

		public void TestJR_ChequeNoDigits()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;

			aCharge.JR_ChequeNo = "1";
			AssertEquals("Cheque Number should be 00001", "00001", aCharge.JR_ChequeNo);

			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			aCharge.JR_ChequeNo = "Cash";
			AssertEquals("Cash type", "Cash", aCharge.JR_ChequeNo);

			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			aCharge.JR_ChequeNo = "CreditCard";
			AssertEquals("CreditCard type", "CreditCard", aCharge.JR_ChequeNo);

			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			aCharge.JR_ChequeNo = "DirectCredit";
			AssertEquals("DirectCredit type", "DirectCredit", aCharge.JR_ChequeNo);

			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			aCharge.JR_ChequeNo = "DirectDebit";
			AssertEquals("DirectDebit type", "DirectDebit", aCharge.JR_ChequeNo);
		}

		#endregion

		#region JR_OH_SellAccount

		public void TestSetChargeDescription_SellAccount()
		{
			TestObjectCreator.LocalClient.OH_RL_NKClosestPort = "AUSYD";
			TestObjectCreator.LocalClient2.OH_RL_NKClosestPort = "AUSYD";

			TestObjectCreator.AALSHI.OH_RL_NKClosestPort = "USLAX";
			TestObjectCreator.ABIGAS.OH_RL_NKClosestPort = "USLAX";

			TestObjectCreator.CC1.AC_LocalLanguageDescription = "CC1 Local Description";

			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				ACharge.JR_AC = TestObjectCreator.CC1.PK;
				ACharge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				AssertEquals("GIVEN localClient THEN should show Local-Description", "CC1 Local Description", ACharge.JR_Desc);

				ACharge.JR_OH_SellAccount = TestObjectCreator.LocalClient2.PK;
				AssertEquals("GIVEN localClient THEN should show Local-Description", "CC1 Local Description", ACharge.JR_Desc);

				ACharge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
				AssertEquals("GIVEN NonLocalClient THEN should show Description", "Charge Code 1", ACharge.JR_Desc);

				ACharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				AssertEquals("GIVEN NonLocalClient THEN should show Description", "Charge Code 1", ACharge.JR_Desc);

				ACharge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				AssertEquals("GIVEN localClient THEN should show Local-Description", "CC1 Local Description", ACharge.JR_Desc);
			}
		}

		public void TestSettingJR_OH_SellAccountSetsChargeCodeLocalDescriptionValues()
		{
			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value;
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, true);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				OrgHeader testDebtorInSameCountry = Factory.NewWithValidTestData<OrgHeader>();
				testDebtorInSameCountry.OH_IsActive = true;
				testDebtorInSameCountry.OH_IsDebtor = true;
				testDebtorInSameCountry.OH_RL_NKClosestPort = "AUSYD";

				OrgHeader testDebtorInDifferentCountry = Factory.NewWithValidTestData<OrgHeader>();
				testDebtorInDifferentCountry.OH_IsActive = true;
				testDebtorInDifferentCountry.OH_IsDebtor = true;
				testDebtorInDifferentCountry.OH_RL_NKClosestPort = "USLAX";

				Factory.Save();

				Charge testCharge = Factory.New<Charge>();
				Job parentJob = Factory.NewJobWithValidTestDataForTesting<Job>();
				testCharge.JR_JH = parentJob.PK;

				AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				testChargeCode.AC_Desc = "Standard";
				testChargeCode.AC_LocalLanguageDescription = "Local";

				parentJob.AgentCollectPK = testDebtorInSameCountry.PK;
				testCharge.JR_OH_SellAccount = testDebtorInSameCountry.PK;
				testCharge.JR_AC = testChargeCode.PK;
				AssertEquals("Charge description should be the local language description of the charge code", "Local", testCharge.JR_Desc);

				parentJob.AgentCollectPK = testDebtorInDifferentCountry.PK;
				testCharge.JR_OH_SellAccount = testDebtorInDifferentCountry.PK;
				testCharge.JR_AC = testChargeCode.PK;
				AssertEquals("Charge description should be the standard description of the charge code", "Standard", testCharge.JR_Desc);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
				AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, originalRegistryValue);
			}
		}

		#endregion

		#region Exchange Rates

		public void TestRatingAuditInContextOfDisbursementCharge()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();

			var rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "GBP";
			rate.JF_BaseRate = .5;

			TestObjectCreator creator = new TestObjectCreator(Factory);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = creator.CC2.PK;
			charge1.JR_OH_SellAccount = testDebtor.PK;
			charge1.JR_RX_NKSellCurrency = "GBP";
			charge1.JR_RX_NKCostCurrency = "GBP";
			charge1.JR_OSSellAmt = 5000m;

			charge1.RevenueCalculationDescription = ZBlob.FromAscii("Dummy Autorating Revenue Log");

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = creator.CC2.PK;
			charge2.JR_OH_SellAccount = testDebtor.PK;
			charge2.JR_RX_NKSellCurrency = "GBP";
			charge2.JR_RX_NKCostCurrency = "GBP";
			charge2.JR_OSCostAmt = 10000m;

			charge2.CostCalculationDescription = ZBlob.FromAscii("Dummy Autorating Cost Log");

			AssertEquals(10000.00m, charge1.JR_LocalSellAmt);
			AssertEquals(10000.00m, charge1.JR_LocalCostAmt);
			AssertEquals(5000.00m, charge1.JR_OSSellAmt);
			AssertEquals(5000.00m, charge1.JR_OSCostAmt);

			AssertEquals(20000.00m, charge2.JR_LocalSellAmt);
			AssertEquals(20000.00m, charge2.JR_LocalCostAmt);
			AssertEquals(10000.00m, charge2.JR_OSSellAmt);
			AssertEquals(10000.00m, charge2.JR_OSCostAmt);

			AssertMultilineASCIIEquals("", "Dummy Autorating Revenue Log", ORtfTextUtil.RtfToText(charge1.RevenueCalculationDescription.ToAscii()));
			AssertMultilineASCIIEquals("", "Dummy Autorating Cost Log", ORtfTextUtil.RtfToText(charge2.CostCalculationDescription.ToAscii()));

			charge1.JR_LocalCostAmt = 6000;
			charge2.JR_LocalSellAmt = 12000;

			AssertEquals(6000.00m, charge1.JR_LocalSellAmt);
			AssertEquals(6000.00m, charge1.JR_LocalCostAmt);
			AssertEquals(3000.00m, charge1.JR_OSSellAmt);
			AssertEquals(3000.00m, charge1.JR_OSCostAmt);

			AssertEquals(12000.00m, charge2.JR_LocalSellAmt);
			AssertEquals(12000.00m, charge2.JR_LocalCostAmt);
			AssertEquals(6000.00m, charge2.JR_OSSellAmt);
			AssertEquals(6000.00m, charge2.JR_OSCostAmt);
		}

		public void TestRatingAuditInContextOfCurrencyAndExchangeRateChanges()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("Create CompanyData before saving", testDebtor.CompanyData);
			orgFactory.Save();

			Job job = Factory.NewJobForTesting<Job>();
			var rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "GBP";
			rate.JF_BaseRate = .7;

			var rate2 = job.ExchangeRates.AddNew();
			rate2.JF_RX_NKRateCurrency = "CHF";
			rate2.JF_BaseRate = .9;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = testDebtor.PK;
			charge.JR_RX_NKSellCurrency = "GBP";
			charge.JR_OSSellAmt = 5000m;
			charge.JR_OSCostAmt = 4000m;
			charge.JR_CostRated = true;
			charge.JR_SellRated = true;

			charge.RevenueCalculationDescription = ZBlob.FromAscii("Dummy Autorating Revenue Log");
			charge.CostCalculationDescription = ZBlob.FromAscii("Dummy Autorating Cost Log");

			AssertEquals(7142.86m, charge.JR_LocalSellAmt);
			AssertEquals(5714.29m, charge.JR_LocalCostAmt);
			AssertEquals(5000.00m, charge.JR_OSSellAmt);
			AssertEquals(4000.00m, charge.JR_OSCostAmt);
			AssertEquals("Dummy Autorating Revenue Log", charge.RevenueCalculationDescription.ToUTF8());
			AssertEquals("Dummy Autorating Cost Log", charge.CostCalculationDescription.ToUTF8());
			Assert(charge.JR_SellRatingOverride);
			Assert(charge.JR_CostRatingOverride);

			rate.JF_BaseRate = .5;

			AssertEquals(10000.00m, charge.JR_LocalSellAmt);
			AssertEquals(8000.00m, charge.JR_LocalCostAmt);
			AssertEquals(5000.00m, charge.JR_OSSellAmt);
			AssertEquals(4000.00m, charge.JR_OSCostAmt);
			AssertEquals("Dummy Autorating Revenue Log", charge.RevenueCalculationDescription.ToUTF8());
			AssertEquals("Dummy Autorating Cost Log", charge.CostCalculationDescription.ToUTF8());
			Assert(charge.JR_SellRatingOverride);
			Assert(charge.JR_CostRatingOverride);

			charge.JR_RX_NKCostCurrency = "CHF";
			charge.JR_RX_NKSellCurrency = "CHF";

			AssertEquals(10000.00m, charge.JR_LocalSellAmt);
			AssertEquals(8000.00m, charge.JR_LocalCostAmt);
			AssertEquals(9000.00m, charge.JR_OSSellAmt);
			AssertEquals(7200.00m, charge.JR_OSCostAmt);
			AssertEquals("Should be cleared when OS amount changes", ZString.Empty, charge.RevenueCalculationDescription.ToUTF8());
			AssertEquals("Should be cleared when OS amount changes", ZString.Empty, charge.CostCalculationDescription.ToUTF8());
			Assert(charge.JR_SellRatingOverride);
			Assert(charge.JR_CostRatingOverride);

			charge.JR_OSSellAmt = 6000;
			charge.JR_OSCostAmt = 5000;

			AssertEquals(6666.67m, charge.JR_LocalSellAmt);
			AssertEquals(5555.56m, charge.JR_LocalCostAmt);
			AssertEquals(ZString.Empty, charge.RevenueCalculationDescription.ToUTF8());
			AssertEquals(ZString.Empty, charge.CostCalculationDescription.ToUTF8());
			Assert(charge.JR_SellRatingOverride);
			Assert(charge.JR_CostRatingOverride);
		}

		public void TestExchangeRateOnLine()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			Job job = CreateJob("Z00001011", AALSHI, true, 10M, ABIGAS, true, 20M);
			job.PlugInData = shipment;
			job.JH_ParentTableCode = "aa";
			job.AgentCollectPK = ABIGAS.PK;
			job.LocalChargesPK = AALSHI.PK;
			job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 10m);
			job.AgentCollect.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 20m);

			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);

			ZECTRA.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 5m);

			ChargeWithCost localClientCharge = CreateCharge(job, MRG100, "Charge 1", TestObjectCreator.USD, 0m, null, TestObjectCreator.USD, 1000M, AALSHI);
			ChargeWithCost agentCharge = CreateCharge(job, MRG100, "Charge 1", TestObjectCreator.USD, 0m, null, TestObjectCreator.USD, 1000M, ABIGAS);
			ChargeWithCost otherOrgCharge = CreateCharge(job, MRG100, "Charge 1", TestObjectCreator.USD, 0m, null, TestObjectCreator.USD, 1000M, ZECTRA);

			AssertEquals("Local Client Charge has Local Client CFX", 10m, localClientCharge.JR_LineCFX);
			AssertEquals("Local Client Charge has Local Client CFX adjusted ExRate = 0.7 x 0.9", 0.63m, localClientCharge.JR_OSSellExRate);

			AssertEquals("Agent Charge has Agent CFX", 20m, agentCharge.JR_LineCFX);
			AssertEquals("Agent Charge has Agent CFX adjusted ExRate = 0.7 x 0.8", 0.56m, agentCharge.JR_OSSellExRate);

			AssertEquals("Other Org Charge has Other Org CFX", 5m, otherOrgCharge.JR_LineCFX);
			AssertEquals("Other Org Charge has Other Org CFX adjusted ExRate = 0.7 x 0.95", 0.665m, otherOrgCharge.JR_OSSellExRate);

			otherOrgCharge.JR_OH_SellAccount = AALSHI.PK;
			AssertEquals("Other Org Charge has Local Client CFX", 10m, otherOrgCharge.JR_LineCFX);
			AssertEquals("Other Org Charge has Local Client CFX adjusted ExRate = 0.7 x 0.9", 0.63m, otherOrgCharge.JR_OSSellExRate);

			otherOrgCharge.JR_OH_SellAccount = ZECTRA.PK;
			AssertEquals("Other Org Charge has Other Org CFX", 5m, otherOrgCharge.JR_LineCFX);
			AssertEquals("Other Org Charge has Other Org CFX adjusted ExRate = 0.7 x 0.95", 0.665m, otherOrgCharge.JR_OSSellExRate);
		}

		#endregion

		#region Transaction Line Creation

		public void TestWIPAccrualSuspendedForAgency()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			job.PlugInData = billOfLading;
			var chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", GlbCompany.CurrentCompany.LocalCurrency, 250M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 350M, AALSHI);

			billOfLading.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignPrePaid;
			Factory.Save();

			AssertNull(chargeWithCost.Accrual);
			AssertNull(chargeWithCost.WIP);

			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
			Factory.Save();

			AssertNull(chargeWithCost.Accrual);
			AssertNull(chargeWithCost.WIP);

			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			Factory.Save();

			AssertNotNull(chargeWithCost.Accrual);
			AssertNotNull(chargeWithCost.WIP);

			billOfLading.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			billOfLading.JS_RL_NKDestination = "INBOM";
			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			var oldAccrual = chargeWithCost.Accrual;
			var oldWIP = chargeWithCost.WIP;
			Factory.Save();

			AssertNull(chargeWithCost.Accrual);
			AssertNull(chargeWithCost.WIP);
			AssertEquals(true, oldAccrual.IsReversed);
			AssertEquals(true, oldWIP.IsReversed);
		}

		public void TestCostTransactionLineCreated()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			Factory.Save();

			ZGuid accrualPK = chargeWithCost.Accrual.PK;

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertNull("Cost Doesn't Exist", chargeWithCost.Cost);

			ZDateTime now = ZDateTime.Now;
			APInvoice invoice = Factory.New<APInvoice>();
			chargeWithCost.CreateCostTransactionLine(invoice, now);

			Accrual oldAccrual = Factory.Load<Accrual>(accrualPK);

			AssertNull("Accrual Doesn't Exist", chargeWithCost.Accrual);
			AssertEquals("Accrual Reversed", true, oldAccrual.IsReversed);
			AssertNotNull("Cost Exists", chargeWithCost.Cost);
			AssertEquals("Post Date", now, chargeWithCost.Cost.AL_PostDate);
		}

		public void TestCostTransactionLineCreated_EmptyRevenueRecognitionType()
		{
			var emptyRevenueRecognitionByChargeGroupCollection = new RevenueRecognitionByChargeGroupCollection();
			var emptyRevenueRecognitionCollection = new RevenueRecognitionCollection();

			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty, Guid.Empty, emptyRevenueRecognitionByChargeGroupCollection))
			using (AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty, Guid.Empty, emptyRevenueRecognitionCollection))
			{
				Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
				ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
				ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
				Factory.Save();

				ZDateTime now = ZDateTime.Now;
				APInvoice invoice = Factory.New<APInvoice>();

				AssertContains("Revenue Recognition Info", "\r\nRevenueRecognitionTypeFromJobDuringLineCreation: There is no data collected for this PK.",
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(chargeWithCost.ChargeCode.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringLineCreation));

				chargeWithCost.CreateCostTransactionLine(invoice, now);

				AssertEquals("Revenue Recognition Info", $@"
RevenueRecognitionTypeFromJobDuringLineCreation:
ChargeCode PK: {chargeWithCost.ChargeCode.PK}, Revenue Recognition Type: ",
		CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(chargeWithCost.ChargeCode.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringLineCreation));
			}
		}

		public void TestCostTransactionLineCreatedWithInvoiceLineReverseDate()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.AUD, 250M, ZECTRA, TestObjectCreator.AUD, 350M, AALSHI);
			Factory.Save();

			Accrual accrual = chargeWithCost.Accrual;

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertNull("Cost Doesn't Exist", chargeWithCost.Cost);

			ZDateTime postDate = ZDateTime.Today.AddDays(-10);
			TestObjectCreator.CreateTestPeriods(postDate);

			APInvoice invoice = Factory.New<APInvoice>();
			chargeWithCost.CreateCostTransactionLine(invoice, postDate);

			AssertNull("Accrual Doesn't Exist", chargeWithCost.Accrual);
			AssertEquals("Accrual Reversed", true, accrual.IsReversed);
			AssertNotNull("Cost Exists", chargeWithCost.Cost);
			AssertEquals("Accrual Reverse Date Should Be Always Equals to Invoice Line Reverse Date", accrual.AL_ReverseDate, chargeWithCost.Cost.AL_ReverseDate);
			AssertEquals("Accrual Reverse Date Should Be Equal to postDate", postDate, accrual.AL_ReverseDate);
		}

		public void TestAccrualCreatedForDepartmentChange()
		{
			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();

			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			chargeWithCost.JR_GE = FIA.PK;
			Factory.Save();

			ZGuid accrualPK = chargeWithCost.Accrual.PK;

			Accrual[] accruals = readOnlyFactory.Load(typeof(Accrual), new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK)) as Accrual[];
			AssertEquals("Accrual Count", 1, accruals.Length);

			chargeWithCost.JR_GE = FIS.PK;
			Factory.Save();

			AssertEquals(FIS.PK, chargeWithCost.Accrual.Department.PK);

			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.Equal, accrualPK);
			accruals = readOnlyFactory.Load(typeof(Accrual), filter) as Accrual[];
			AssertEquals("Old Accrual Count", 1, accruals.Length);
			Assert("This Accrual Should be reversed.", !accruals[0].AL_ReverseDate.IsEmpty);

			filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, accrualPK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			accruals = readOnlyFactory.Load(typeof(Accrual), filter) as Accrual[];
			AssertEquals("New Accrual Count", 1, accruals.Length);
			Assert("This Accrual Should not be reversed.", accruals[0].AL_ReverseDate.IsEmpty);
		}

		public void TestAccrualCreatedForBranchChange()
		{
			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();

			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			chargeWithCost.JR_GB = DEM.PK;
			Factory.Save();

			ZGuid accrualPK = chargeWithCost.Accrual.PK;

			Accrual[] accruals = readOnlyFactory.Load(typeof(Accrual), new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, TES.Company.PK)) as Accrual[];
			AssertEquals("Accrual Count", 1, accruals.Length);

			chargeWithCost.JR_GB = TES.PK;
			Factory.Save();

			AssertEquals(TES.PK, chargeWithCost.Accrual.Branch.PK);

			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.Equal, accrualPK);
			accruals = readOnlyFactory.Load(typeof(Accrual), filter) as Accrual[];
			AssertEquals("Old Accrual Count", 1, accruals.Length);
			Assert("This Accrual Should be reversed.", !accruals[0].AL_ReverseDate.IsEmpty);

			filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, accrualPK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, TES.Company.PK);
			accruals = readOnlyFactory.Load(typeof(Accrual), filter) as Accrual[];
			AssertEquals("New Accrual Count", 1, accruals.Length);
			Assert("This Accrual Should not be reversed.", accruals[0].AL_ReverseDate.IsEmpty);
		}

		public void TestWIPAccrualNotCreatedForDelayedJob_Revenue()
		{
			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();

			Accrual[] accruals = readOnlyFactory.Load<Accrual>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			WIP[] wIPs = readOnlyFactory.Load<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			int initialAccuralCount = accruals.Length;
			int initialWIPsCount = wIPs.Length;

			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost_1 = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, ABIGAS);
			ChargeWithCost chargeWithCost_2 = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 120M, ZECTRA, TestObjectCreator.USD, 240M, null); // don't post this line
			chargeWithCost_1.JR_GB = DEM.PK;

			Factory.Save();

			AssertNull(chargeWithCost_1.Accrual);
			AssertNull(chargeWithCost_1.WIP);

			accruals = readOnlyFactory.Load<Accrual>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Accrual Count", initialAccuralCount, accruals.Length);

			wIPs = readOnlyFactory.Load<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("WIP Count", initialWIPsCount, wIPs.Length);

			chargeWithCost_1.JR_GB = TES.PK;
			Factory.Save();

			AssertNull("Further changes still don't create an Accrual or WIP", chargeWithCost_1.Accrual);
			AssertNull("Further changes still don't create an Accrual or WIP", chargeWithCost_1.WIP);

			// Post Something
			InvoicingPostManager poster = new InvoicingPostManager(job);
			poster.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_1.Accrual);
			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_1.Revenue);

			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_2.Accrual);
			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_2.WIP);

			accruals = readOnlyFactory.Load<Accrual>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Accrual Count", initialAccuralCount + 2, accruals.Length);

			wIPs = readOnlyFactory.Load<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("WIP Count", initialWIPsCount + 1, wIPs.Length);
		}

		public void TestWIPAccrualNotCreatedForDelayedJob_Cost()
		{
			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();

			Accrual[] accruals = readOnlyFactory.Load<Accrual>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			WIP[] wIPs = readOnlyFactory.Load<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			int initialAccuralCount = accruals.Length;
			int initialWIPsCount = wIPs.Length;

			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost_1 = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, null); // don't post revenue
			ChargeWithCost chargeWithCost_2 = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 120M, ZECTRA, TestObjectCreator.USD, 240M, null); // don't post revenue
			chargeWithCost_1.JR_GB = DEM.PK;
			chargeWithCost_1.JR_OH_CostAccount = AALSHI.PK;
			chargeWithCost_1.JR_APInvoiceNum = "ABCXYZ_01";
			chargeWithCost_1.JR_APInvoiceDate = ZDateTime.Now;

			Factory.Save();

			AssertNull(chargeWithCost_1.Accrual);
			AssertNull(chargeWithCost_1.WIP);

			AssertNull(chargeWithCost_2.Accrual);
			AssertNull(chargeWithCost_2.WIP);

			accruals = readOnlyFactory.Load<Accrual>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Accrual Count", initialAccuralCount, accruals.Length);

			wIPs = readOnlyFactory.Load<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("WIP Count", initialWIPsCount, wIPs.Length);

			chargeWithCost_1.JR_GB = TES.PK;
			Factory.Save();

			AssertNull("Further changes still don't create an Accrual or WIP", chargeWithCost_1.Accrual);
			AssertNull("Further changes still don't create an Accrual or WIP", chargeWithCost_1.WIP);

			// Post a Cost
			InvoicingPostManager poster = new InvoicingPostManager(job);
			poster.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_1.Cost);
			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_1.WIP);

			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_2.Accrual);
			AssertNotNull("Once status is not delayed, WIP and Accrual are created", chargeWithCost_2.WIP);

			accruals = readOnlyFactory.Load<Accrual>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "ACR").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Accrual Count", initialAccuralCount + 1, accruals.Length);

			wIPs = readOnlyFactory.Load<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("WIP Count", initialWIPsCount + 2, wIPs.Length);
		}

		public void TestWIPCreatedForDepartmentChange()
		{
			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();

			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, TestObjectCreator.GBP, .4M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Revenue Transaction Test", TestObjectCreator.USD, 250M, AALSHI, TestObjectCreator.USD, 350M, ZECTRA);
			chargeWithCost.JR_GE = FIA.PK;
			Factory.Save();

			ZGuid wIPPK = chargeWithCost.WIP.PK;

			WIP[] wIPs = readOnlyFactory.Load(typeof(WIP), new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK)) as WIP[];
			AssertEquals("WIP Count", 1, wIPs.Length);

			chargeWithCost.JR_GE = FIS.PK;
			Factory.Save();

			AssertEquals(FIS.PK, chargeWithCost.WIP.Department.PK);

			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.Equal, wIPPK);
			wIPs = readOnlyFactory.Load(typeof(WIP), filter) as WIP[];
			AssertEquals("Old WIP Count", 1, wIPs.Length);
			Assert("This WIP Should be reversed.", !wIPs[0].AL_ReverseDate.IsEmpty);

			filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, wIPPK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
			wIPs = readOnlyFactory.Load(typeof(WIP), filter) as WIP[];
			AssertEquals("New WIP Count", 1, wIPs.Length);
			Assert("This WIP Should not be reversed.", wIPs[0].AL_ReverseDate.IsEmpty);
		}

		public void TestWIPCreatedForBranchChange()
		{
			BusinessObjectFactory readOnlyFactory = new BusinessObjectFactory();

			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, TestObjectCreator.GBP, .4M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Revenue Transaction Test", TestObjectCreator.USD, 250M, AALSHI, TestObjectCreator.USD, 350M, ZECTRA);
			chargeWithCost.JR_GB = DEM.PK;

			Factory.Save();

			ZGuid wIPPK = chargeWithCost.WIP.PK;

			WIP[] wIPs = readOnlyFactory.Load(typeof(WIP), new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP").AddToFilter(AccTransactionLinesSchema.AL_GC, TES.Company.PK)) as WIP[];
			AssertEquals("WIP Count", 1, wIPs.Length);

			chargeWithCost.JR_GB = TES.PK;
			Factory.Save();

			AssertEquals(TES.PK, chargeWithCost.WIP.Branch.PK);

			ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.Equal, wIPPK);
			wIPs = readOnlyFactory.Load(typeof(WIP), filter) as WIP[];
			AssertEquals("Old WIP Count", 1, wIPs.Length);
			Assert("This WIP Should be reversed.", !wIPs[0].AL_ReverseDate.IsEmpty);

			filter = new ZQuery(AccTransactionLinesSchema.AL_LineType, "WIP");
			filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, wIPPK);
			filter.AddToFilter(AccTransactionLinesSchema.AL_GC, TES.Company.PK);
			wIPs = readOnlyFactory.Load(typeof(WIP), filter) as WIP[];
			AssertEquals("New WIP Count", 1, wIPs.Length);
			Assert("This WIP Should not be reversed.", wIPs[0].AL_ReverseDate.IsEmpty);
		}

		public void TestCanCreateWIPAndACR()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "ALL";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Job job = CreateJob("Z00001001", ZECTRA, true, 5M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Transaction Test", TestObjectCreator.AUD, 250M, AALSHI, TestObjectCreator.AUD, 350M, ZECTRA);
			Factory.Save();
			AssertNull("WIP line canot be created if parent Job has no Revenue Recognition Date set", chargeWithCost.WIP);
			AssertNull("ACR line canot be created if parent Job has no Revenue Recognition Date set", chargeWithCost.Accrual);

			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();
			AssertNotNull("WIP line can be created if parent Job has not empty Revenue Recognition Date", chargeWithCost.WIP);
			AssertNotNull("ACR line can be created if parent Job has not empty Revenue Recognition Date", chargeWithCost.Accrual);

			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);
			chargeWithCost.JR_OSCostAmt = 10M;
			chargeWithCost.JR_OSSellAmt = 10M;
			Factory.Save();
			AssertNull("WIP line canot be created if parent Job has Revenue Recognition Date set to max value", chargeWithCost.WIP);
			AssertNull("ACR line canot be created if parent Job has Revenue Recognition Date set to max value", chargeWithCost.Accrual);
		}

		public void TestCanCreateAutoJRJ()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "ALL";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Job job = CreateJob("Z00001001", ZECTRA, true, 5M, ABIGAS, true, 10M);
			var charge1 = CreateCharge(job, MRG100, "Transaction Test", TestObjectCreator.AUD, 250M, GlbBranch.CurrentBranch.OrgProxy, TestObjectCreator.AUD, 350M, ZECTRA);
			var charge2 = CreateCharge(job, MRG100, "Transaction Test", TestObjectCreator.AUD, 250M, AALSHI, TestObjectCreator.AUD, 350M, GlbBranch.CurrentBranch.OrgProxy);

			Factory.Save();
			AssertNull("Cost JRJ line canot be created if parent Job has no Revenue Recognition Date set", charge1.APLine);
			AssertNull("Sell JRJ line canot be created if parent Job has no Revenue Recognition Date set", charge2.ARLine);

			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();
			AssertNotNull("Cost JRJ line can be created if parent Job has not empty Revenue Recognition Date", charge1.APLine);
			AssertNotNull("Sell JRJ line can be created if parent Job has not empty Revenue Recognition Date", charge2.ARLine);

			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);
			charge1.JR_OSCostAmt = 10M;
			charge2.JR_OSSellAmt = 10M;
			Factory.Save();
			AssertNull("WIP line canot be created if parent Job has Revenue Recognition Date set to max value", charge1.APLine);
			AssertNull("ACR line canot be created if parent Job has Revenue Recognition Date set to max value", charge2.ARLine);
		}

		public void TestAutoJRJIsCreatedWhenDebtorIsOrgProxy()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Today.AddMonths(-3);

			GlbBranch branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Debtor.PK;

			Factory.Save();

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.Debtor);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			Factory.Save();

			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			AssertNotNull("Sell JRJ line can be created", charge.ARLine);
			AssertNotNull("Cost JRJ will be created automatically", job.Charges[1].APLine);
			AssertNull("No Cost JRJ line can be created", charge.APLine);
		}

		public void TestAutoJRJIsCreatedWhenCreditorIsOrgProxy()
		{
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Today.AddMonths(-3);

			GlbBranch branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;

			Factory.Save();

			Job job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			Factory.Save();

			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			AssertNotNull("Sell JRJ line can be created", charge.APLine);
			AssertNotNull("Cost JRJ will be created automatically", job.Charges[1].ARLine);
			AssertNull("No Cost JRJ line can be created", charge.ARLine);
		}

		[TestDate(2015, 01, 15)]
		public void TestAutoJRJIsNotCreatedWhenRevenueRecognitionConfigIsMissing()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupInvalidRevenueRecognitionRegistry();
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			var expectedErrorMessage = "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
			AssertHasError(charge.JR_JH_InternalJobInfo, expectedErrorMessage);

			Factory.Save(); // in case someone saves the factory even though there are validation errors.
			AssertEquals("Number of Charge Lines", 1, job.Charges.Count);
			AssertNull("Cost JRJ line cannot be created", charge.APLine);

			// fix the configuration to make sure it will be created.
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			Factory.Save();
			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			var secondCharge = job.Charges.Where(x => x.PK != charge.PK).First();
			AssertNotNull("Cost JRJ line was created", charge.APLine);
			AssertNotNull("second charge sell JRJ line was created", secondCharge.ARLine);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsNotCreatedWhenRevenueRecognitionDateIsMissingForARVRecognition()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Empty;
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			AssertHasError(charge.JR_JH_InternalJobInfo, "Job Revenue Journal cannot be posted until the 'Actual/Estimated Arrival Date' for this internal job is recorded. This internal job and charge code combination requires this date for revenue recognition purposes.");
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsCreatedWhenRevenueRecognitionDateIsMissingForFARRecognition()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			AssertNoErrors(charge.JR_LocalCostAmtInfo);
			AssertNoErrors(charge.JR_LocalSellAmtInfo);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			Factory.Save();
			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			AssertNotNull("Cost JRJ line has been created", charge.APLine);
			AssertEquals("Cost JRJ line is linked to AJRJ", TransactionLineTypes.Revenue, charge.APLine.AL_LineType);
			var secondCharge = job.Charges.Where(x => x.PK != charge.PK).First();
			AssertNotNull("2nd charge Sell JRJ line has been created", secondCharge.ARLine);
			AssertEquals("2nd charge Sell JRJ line is linked to AJRJ", TransactionLineTypes.Revenue, secondCharge.ARLine.AL_LineType);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsCreatedWhenRevenueRecognitionDateIsPresentForARVRecognition()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var expectedRecognitionDate = ZDateTime.Today.AddDays(3);
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = expectedRecognitionDate;
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			AssertNoErrors(charge.JR_LocalCostAmtInfo);
			AssertNoErrors(charge.JR_LocalSellAmtInfo);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			Factory.Save();
			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			var secondCharge = job.Charges.Where(x => x.PK != charge.PK).First();
			AssertNotNull("Cost JRJ line has been created", charge.APLine);
			AssertNotNull("Second JRJ line has been created", secondCharge.ARLine);
			AssertEquals("Sell JRJ line should be recognised.", expectedRecognitionDate, charge.APLine.AL_ReverseDate);
			AssertEquals("Second JRJ line should be recognised.", expectedRecognitionDate, secondCharge.ARLine.AL_ReverseDate);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsNotCreatedWhenRevenueRecognitionDateIsOutsideOfAccountingPeriodRange()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var expectedRecognitionDate = ZDateTime.Today.AddDays(3);
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = expectedRecognitionDate;
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();

			var expectedMessage = @"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 18-Jan-15 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.";
			AssertHasError(charge.JR_LocalCostAmtInfo, expectedMessage);
			AssertHasError(charge.JR_JH_InternalJobInfo, expectedMessage);
		}

		[TestDate(2015, 01, 15)]
		public void TestAutoJRJIsNotCreatedWhenRevenueRecognitionConfigIsMissing_Deferred()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupInvalidRevenueRecognitionRegistry();
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			var expectedErrorMessage = "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
			AssertHasError(charge.JR_JH_InternalJobInfo, expectedErrorMessage);

			Factory.Save(); // in case someone saves the factory even though there are validation errors.
			AssertEquals("Number of Charge Lines", 1, job.Charges.Count);
			AssertNull("Cost JRJ line cannot be created", charge.APLine);

			// now fix the configuration to make sure it will be created.
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			Factory.Save();
			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			var secondCharge = job.Charges.Where(x => x.PK != charge.PK).First();
			AssertNotNull("Cost JRJ line was created", charge.APLine);
			AssertNotNull("second charge sell JRJ line was created", secondCharge.ARLine);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsCreatedWhenRevenueRecognitionDateIsMissingForARVRecognition_Deferred()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = ZDateTime.Empty;
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			AssertNoErrors(charge.JR_LocalCostAmtInfo);
			AssertNoErrors(charge.JR_LocalSellAmtInfo);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);
			Factory.Save();
			AssertEquals("job charges count", 2, job.Charges.Count);
			AssertEquals("job revenue journal should be created", TransactionLineTypes.Revenue, charge.APLine.AL_LineType);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsCreatedWhenRevenueRecognitionDateIsMissingForFARRecognition_Deferred()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			AssertNoErrors(charge.JR_LocalCostAmtInfo);
			AssertNoErrors(charge.JR_LocalSellAmtInfo);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			Factory.Save();
			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			AssertNotNull("Cost JRJ line has been created", charge.APLine);
			AssertEquals("Cost JRJ line is linked to AJRJ", TransactionLineTypes.Revenue, charge.APLine.AL_LineType);

			var secondCharge = job.Charges.Where(x => x.PK != charge.PK).First();
			AssertNotNull("2nd charge Sell JRJ line has been created", secondCharge.ARLine);
			AssertEquals("2nd charge Sell JRJ line is linked to AJRJ", TransactionLineTypes.Revenue, secondCharge.ARLine.AL_LineType);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsCreatedWhenRevenueRecognitionDateIsPresentForARVRecognition_Deferred()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var expectedRecognitionDate = ZDateTime.Today.AddDays(3);
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = expectedRecognitionDate;
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();
			AssertNoErrors(charge.JR_LocalCostAmtInfo);
			AssertNoErrors(charge.JR_LocalSellAmtInfo);
			AssertNoErrors(charge.JR_JH_InternalJobInfo);

			Factory.Save();
			AssertEquals("Number of Charge Lines", 2, job.Charges.Count);
			var secondCharge = job.Charges.Where(x => x.PK != charge.PK).First();
			AssertNotNull("Cost JRJ line has been created", charge.APLine);
			AssertNotNull("Second JRJ line has been created", secondCharge.ARLine);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoJRJIsNotCreatedWhenRevenueRecognitionDateIsOutsideOfAccountingPeriodRange_Deferred()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			SetupRevenueRecognitionRegistry(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var expectedRecognitionDate = ZDateTime.Today.AddDays(3);
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_E_ARV = expectedRecognitionDate;
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Creditor1.PK;
			Factory.Save();

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0M, null);
			charge.JR_GE_InternalDept = TestObjectCreator.GEADepartment.PK;
			charge.RunPreSaveValidation();

			var expectedMessage = @"Please have your Accounting Department create an appropriate Accounting Period.
The Revenue Recognition Date 18-Jan-15 cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date.";
			AssertHasError(charge.JR_LocalCostAmtInfo, expectedMessage);
			AssertHasError(charge.JR_JH_InternalJobInfo, expectedMessage);
		}

		void SetupRevenueRecognitionRegistry(string recognitionType)
		{
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = recognitionType;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}
		void SetupInvalidRevenueRecognitionRegistry()
		{
			var revenueRecognitionConfig = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
			revenueRecognitionConfig.RemoveAll();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revenueRecognitionConfig);
		}

		public void TestWIPandACRCreationDependOnRevenueRecognitionDate()
		{
			Job job = CreateJob("Z00001012", ZECTRA, true, 5M, ABIGAS, true, 10M);
			GlbBranch nonCurrentBranch = TestObjectCreator.NonCurrentBranch;
			ExchangeRate rate1 = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, TestObjectCreator.GBP, .4M);
			ChargeWithCost chargeWithCost1 = CreateCharge(job, MRG100, "WIP and ACR creation test", TestObjectCreator.USD, 250M, AALSHI, TestObjectCreator.USD, 350M, ZECTRA);
			chargeWithCost1.JR_GB = nonCurrentBranch.PK;
			ClearWIPandAccurualOnCharge(chargeWithCost1);

			Factory.Save();
			AssertNotNull("Because of Revenue Recognition Date is not empty WIP can be created, and should be done automatically on charge saving", chargeWithCost1.WIP);
			AssertNotNull("Because of Revenue Recognition Date is not empty ACR can be created, and should be done automatically on charge saving", chargeWithCost1.Accrual);

			ClearWIPandAccurualOnCharge(chargeWithCost1);

			chargeWithCost1.CreateAccrualAndWIP_ForTestOnly();
			AssertNotNull("Because of Revenue Recognition Date is not empty WIP can be created", chargeWithCost1.WIP);
			AssertNotNull("Because of Revenue Recognition Date is not empty ACR can be created", chargeWithCost1.Accrual);

			ClearWIPandAccurualOnCharge(chargeWithCost1);

			chargeWithCost1.WIPAccrualCreationDate = ZDateTime.Now;
			chargeWithCost1.CreateAccrualAndWIP_ForTestOnly();
			AssertNotNull("Because of Revenue Recognition Date is not empty WIP can be created", chargeWithCost1.WIP);
			AssertNotNull("Because of Revenue Recognition Date is not empty ACR can be created", chargeWithCost1.Accrual);

			//Revenue Recognition Date is empty
			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "ALL";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			ChargeWithCost chargeWithCost2 = CreateCharge(job, MRG100, "WIP and ACR creation test", TestObjectCreator.USD, 250M, AALSHI, TestObjectCreator.USD, 350M, ZECTRA);
			chargeWithCost2.JR_GB = nonCurrentBranch.PK;

			Factory.Save();
			AssertNull("Because of Revenue Recognition Date is empty WIP can't be created", chargeWithCost2.WIP);
			AssertNull("Because of Revenue Recognition Date is empty ACR can't be created", chargeWithCost2.Accrual);

			chargeWithCost2.CreateAccrualAndWIP_ForTestOnly();
			AssertNull("Because of Revenue Recognition Date is empty WIP can't be created", chargeWithCost2.WIP);
			AssertNull("Because of Revenue Recognition Date is empty ACR can't be created", chargeWithCost2.Accrual);

			chargeWithCost2.WIPAccrualCreationDate = ZDateTime.Now;
			chargeWithCost2.CreateAccrualAndWIP_ForTestOnly();
			AssertNull("Because of Revenue Recognition Date is empty WIP can't be created", chargeWithCost2.WIP);
			AssertNull("Because of Revenue Recognition Date is empty ACR can't be created", chargeWithCost2.Accrual);

			//Revenue Recognition Date is max
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);
			chargeWithCost2 = CreateCharge(job, MRG100, "WIP and ACR creation test", TestObjectCreator.USD, 250M, AALSHI, TestObjectCreator.USD, 350M, ZECTRA);
			chargeWithCost2.JR_GB = nonCurrentBranch.PK;

			Factory.Save();
			AssertNull("Because of Revenue Recognition Date is max WIP can't be created", chargeWithCost2.WIP);
			AssertNull("Because of Revenue Recognition Date is max ACR can't be created", chargeWithCost2.Accrual);

			chargeWithCost2.CreateAccrualAndWIP_ForTestOnly();
			AssertNull("Because of Revenue Recognition Date is max WIP can't be created", chargeWithCost2.WIP);
			AssertNull("Because of Revenue Recognition Date is max ACR can't be created", chargeWithCost2.Accrual);

			chargeWithCost2.WIPAccrualCreationDate = ZDateTime.Now;
			chargeWithCost2.CreateAccrualAndWIP_ForTestOnly();
			AssertNull("Because of Revenue Recognition Date is max WIP can be created", chargeWithCost2.WIP);
			AssertNull("Because of Revenue Recognition Date is max ACR can be created", chargeWithCost2.Accrual);
		}

		void ClearWIPandAccurualOnCharge(ChargeWithCost charge)
		{
			if (charge.WIP != null)
			{
				charge.ReverseWIP(ZDateTime.Now);
			}
			else
			{
				charge.JR_AL_ARLine = ZGuid.Empty;
			}

			if (charge.Accrual != null)
			{
				charge.ReverseAccrual(ZDateTime.Now);
			}
			else
			{
				charge.JR_AL_APLine = ZGuid.Empty;
			}

			AssertNull("WIP should be null", charge.WIP);
			AssertNull("ACR should be null", charge.Accrual);
		}

		public void TestCreateAutoJRJReferRevenueRecognitionDateOfInternalInvoicingJob()
		{
			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var branch = Factory.Load<GlbBranch>(Env.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = TestObjectCreator.Debtor.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			var shipment2 = TestObjectCreator.CreateShipment("S002", "USLAX", "AUSYD");
			var internalJob = TestObjectCreator.CreateJob(shipment2, false);

			Factory.Save();

			var job = TestObjectCreator.CreateJob(shipment1, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.ManualJobAccrualChargeCode, "Desc", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.Debtor);
			charge.JR_JH_InternalJob = internalJob.PK;

			AssertNoExceptionThrown("The revenue recognition date is empty for internal job, so we won't create auto job revenue journal", Factory.Save);

			AssertEquals("Can't apply revenue recognition date for internal job", 0, internalJob.Charges.Count);
			var journal = Factory.Load<JobRevenueJournal>(new ZQuery()).FirstOrDefault();
			AssertNull("Create journal failed since no revenue recognition date for internal job", journal);

			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			internalJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			Factory.Save();

			AssertEquals("Can apply revenue recognition date for internal job", 1, internalJob.Charges.Count);
			AssertEquals("Should be equal to the sell amount of original charge", 100M, internalJob.Charges[0].JR_LocalCostAmt);
			AssertEquals("Sell amount should keep zero since the charge type is ManualJobAccrual", 0M, internalJob.Charges[0].JR_LocalSellAmt);
			journal = Factory.Load<JobRevenueJournal>(new ZQuery()).FirstOrDefault();
			AssertNotNull("Create journal sucessful since revenue recognition date applied for internal job", journal);
		}

		#region FIA Department

		GlbDepartment fFIA;
		protected GlbDepartment FIA
		{
			get
			{
				if (fFIA == null)
				{
					fFIA = Factory.LoadFromNaturalKey(typeof(GlbDepartment), GlbDepartmentSchema.GE_Code, "FIA") as GlbDepartment;
				}
				return fFIA;
			}
		}

		#endregion

		#region FIS Department

		GlbDepartment fFIS;
		protected GlbDepartment FIS
		{
			get
			{
				if (fFIS == null)
				{
					fFIS = Factory.LoadFromNaturalKey(typeof(GlbDepartment), GlbDepartmentSchema.GE_Code, "FIS") as GlbDepartment;
				}
				return fFIS;
			}
		}

		#endregion

		#region DEM Branch

		GlbBranch fDEM;
		protected GlbBranch DEM
		{
			get
			{
				if (fDEM == null)
				{
					fDEM = Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, "DEM") as GlbBranch;
					fDEM.GB_GC = GlbCompany.CurrentCompany.PK;
				}
				return fDEM;
			}
		}

		#endregion

		#region TES Branch

		GlbBranch fTES;
		protected GlbBranch TES
		{
			get
			{
				if (fTES == null)
				{
					fTES = Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, "TES") as GlbBranch;
					fTES.GB_GC = GlbCompany.CurrentCompany.PK;
				}
				return fTES;
			}
		}

		#endregion

		public void TestWIPCreatedForSellLessCFXValue()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			var charge = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			Factory.Save();

			var accrualPK = charge.Accrual.PK;

			AssertNotNull("Accrual Exists", charge.Accrual);
			AssertNotNull("Accrual Exists", charge.WIP);
			AssertNull("Cost Doesn't Exist", charge.Cost);
			AssertNull("CFX Line should still be null", charge.CFXLine);

			AssertEquals("CFX Amount", 26.32m, charge.JR_CFXAmt);

			AssertEquals("WIP Amount should be 526.32(sell component)-26.32(CFX component) to a nett of 500", 500.00m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;

			Factory.Save();

			AssertEquals("CFX Amount should now be zero", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should now be standard local value (526.32)", 526.32m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			AssertEquals("CFX Amount", 26.32m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should be 526.32(sell component)-26.32(CFX component) to a nett of 500", 500.00m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals("CFX Amount should now be zero", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should now be standard local value (526.32)", 500.00m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertNoExceptionThrown("To make sure it passes Critical Validation", () => Factory.Save());

			AssertEquals("CFX Amount", 25m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should now be standard local value (500.00)", 500.00m, charge.WIP.AL_OSExTaxAmount);
		}

		public void TestWIPNOTCreatedForSellCFXValueCFXDisabled()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			var charge = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			Factory.Save();

			var accrualPK = charge.Accrual.PK;

			AssertNotNull("Accrual Exists", charge.Accrual);
			AssertNotNull("Accrual Exists", charge.WIP);
			AssertNull("Cost Doesn't Exist", charge.Cost);
			AssertNull("CFX Line should still be null", charge.CFXLine);

			AssertEquals("CFX Amount", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should be 526.32(sell component)", 526.32m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;

			Factory.Save();

			AssertEquals("CFX Amount should now be zero", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should now be standard local value (526.32)", 526.32m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

			Factory.Save();

			AssertEquals("CFX Amount should now be zero", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should be 526.32(sell component)", 526.32m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			AssertEquals("CFX Amount should now be zero", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount should now be standard local value (526.32)", 526.32m, charge.WIP.AL_OSExTaxAmount);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			Assert("BillInInvoiceCurrency", charge.BillInInvoiceCurrency);
			Assert("BillInInvoiceCurrencyWithLocalSellCurrency", charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertNoExceptionThrown("To make sure it passes Critical Validation", () => Factory.Save());

			AssertEquals("CFX Amount should now be zero", 0m, charge.JR_CFXAmt);
			AssertEquals("WIP Amount", 525.00m, charge.WIP.AL_OSExTaxAmount);
		}

		public void TestOnFactorySavingInCompanyContextSuspender()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);

			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ChargeWithCost.OnFactorySavingInCompanyContextSuspender>.GetSuspender(Factory))
			{
				Factory.Save();
			}

			AssertNull("Accrual Not Exists", chargeWithCost.Accrual);
			AssertNull("WIP Not Exists", chargeWithCost.WIP);

			Factory.Save();

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertNotNull("WIP Exists", chargeWithCost.WIP);
		}

		public void TestWIPAccrualCreatedWithRevenueRecognitionDate()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);

			Factory.Save();

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertEquals("Accrual Post Date is Now", ZDateTime.Today, chargeWithCost.Accrual.AL_PostDate.Date);
			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertEquals("WIP Post Date is Now", ZDateTime.Today, chargeWithCost.WIP.AL_PostDate.Date);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = "ALL";
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			JobChargeRevRecognition revRecog = TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate, ZDateTime.Now.AddDays(1));

			chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			Factory.Save();

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertEquals("Accrual Post Date is Now", ZDateTime.Today, chargeWithCost.Accrual.AL_PostDate.Date);
			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertEquals("WIP Post Date is Now", ZDateTime.Today, chargeWithCost.WIP.AL_PostDate.Date);

			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			PeriodManagement.PeriodManager periodManager = new PeriodManagement.PeriodManager(Factory);
			int thisPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
			ZDateTime startDate = new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1);
			ZDateTime endDate = startDate.AddMonths(3).AddDays(-1);
			AccPeriodManagement period = periodManager.CreateOnePeriod(thisPeriod, startDate, endDate, Factory);
			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = false;

			chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			Factory.Save();

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertEquals("Accrual Post Date is Revenue Recognition Date", job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)), chargeWithCost.Accrual.AL_PostDate);
			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertEquals("WIP Post Date is Revenue Recognition Date", job.GetRevenueRecognitionDate(job.GetRevenueRecognitionType(TestObjectCreator.CC1)), chargeWithCost.WIP.AL_PostDate);

			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;

			chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			Factory.Save();

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertEquals("Accrual Post Date is Revenue Recognition Date", ZDateTime.Today, chargeWithCost.Accrual.AL_PostDate.Date);
			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertEquals("WIP Post Date is Revenue Recognition Date", ZDateTime.Today, chargeWithCost.WIP.AL_PostDate.Date);

			chargeWithCost = CreateCharge(job, MRG100, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			revRecog.D3_RecognitionDate = AccountingConstants.RevenueRecognitionDateConstants.JobClosure;
			Factory.Save();

			AssertNull("Accrual should not exist", chargeWithCost.Accrual);
			AssertNull("WIP should not exist", chargeWithCost.WIP);
		}

		public void TestValidateJR_ACWhenCreateWIPandAccrual()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = "MRG";
			chargeCode.AC_AG_AccrualAccount = creator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = creator.GLHeader2.PK;

			ChargeWithCost chargeWithCost = CreateCharge(job, chargeCode, "Cost Transaction Test", TestObjectCreator.USD, 250M, ZECTRA, TestObjectCreator.USD, 350M, AALSHI);
			chargeWithCost.RunPreSaveValidation();
			Assert("No errors expected.", !chargeWithCost.HasRowErrors);

			chargeCode.AC_AG_WIPAccount = ZGuid.Empty;
			chargeCode.AC_AG_AccrualAccount = ZGuid.Empty;
			chargeWithCost.RunPreSaveValidation();
			Assert("Must Have Row Error.", chargeWithCost.RowErrors.Contains(ChargeWithCost.GetInvalidChargeCodeError(chargeCode.AC_Code, "Accrual")));
			Assert("Must Have Row Error.", chargeWithCost.RowErrors.Contains(ChargeWithCost.GetInvalidChargeCodeError(chargeCode.AC_Code, "WIP")));
		}

		public void TestValidateJR_ACWhenCreateWIPandAccrual_WithNegativeAccrualsBehaviour()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);
			ExchangeRate rate = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = "MRG";
			chargeCode.AC_AG_AccrualAccount = creator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = creator.GLHeader2.PK;

			ChargeWithCost chargeWithCost = CreateCharge(job, chargeCode, "Cost Transaction Test", TestObjectCreator.USD, -250M, ZECTRA, TestObjectCreator.USD, -350M, AALSHI);
			chargeWithCost.RunPreSaveValidation();
			AssertNoRowErrors("No errors expected.", chargeWithCost);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			chargeCode.AC_AG_WIPAccount = ZGuid.Empty;
			chargeCode.AC_AG_AccrualAccount = ZGuid.Empty;
			chargeWithCost.RunPreSaveValidation();
			Assert("Must Have Row Error.", chargeWithCost.RowErrors.Contains(ChargeWithCost.GetInvalidChargeCodeError(chargeCode.AC_Code, "Accrual")));
			Assert("Must Have Row Error.", chargeWithCost.RowErrors.Contains(ChargeWithCost.GetInvalidChargeCodeError(chargeCode.AC_Code, "WIP")));

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			chargeWithCost.RunPreSaveValidation();
			AssertNoRowErrors("No errors expected.", chargeWithCost);
		}

		#endregion

		#region Profit Share

		public void TestGetProfit()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0M);

			ExchangeRate uSDRate = job.ExchangeRates.AddNew();
			uSDRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			uSDRate.JF_BaseRate = 0.5m;

			ExchangeRate gBPRate = job.ExchangeRates.AddNew();
			gBPRate.JF_RX_NKRateCurrency = TestObjectCreator.GBP.RX_Code;
			gBPRate.JF_BaseRate = 0.8m;

			Charge charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = AALSHI.PK;

			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			charge.JR_OSSellAmt = 500m;
			charge.JR_AgentDeclaredSellAmt = 400m;

			charge.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;
			charge.JR_OSCostAmt = 200m;
			charge.JR_AgentDeclaredCostAmt = 300m;

			AssertEquals("Local Sell Amount correct", 1000m, charge.JR_LocalSellAmt);
			AssertEquals("Local Agent Declared Sell Amount correct", 800m, charge.JR_AgentDeclaredSellAmtLocal);

			AssertEquals("Local Cost Amount correct", 250m, charge.JR_LocalCostAmt);
			AssertEquals("Local Agent Declared Cost Amount correct", 375m, charge.JR_AgentDeclaredCostAmtLocal);
		}

		public void TestProfitShareIncludedAutoSetFromAgreement()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			TestJob.PlugInData = shipment;

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "ALL";
			profitShareAgreement.O4_SendingPortOrCountry = "AU";
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);
			profitShareAgreement.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeFreightDestination;
			profitShareFactory.Save();

			AssertEquals("Precondition: Profit share agreement found", profitShareAgreement.PK, TestJob.ProfitShareAgreement.PK);

			AccChargeCode fRTChargeCode = Factory.New<AccChargeCode>();
			fRTChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			AccChargeCode oRGChargeCode = Factory.New<AccChargeCode>();
			oRGChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;

			AccChargeCode dSTChargeCode = Factory.New<AccChargeCode>();
			dSTChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			JobCharge charge = TestJob.Charges.AddNew();

			charge.JR_AC = fRTChargeCode.PK;
			Assert("Include in Profit Share is on", charge.JR_IsIncludedInProfitShare);

			charge.JR_AC = oRGChargeCode.PK;
			Assert("Include in Profit Share is off", !charge.JR_IsIncludedInProfitShare);

			charge.JR_AC = dSTChargeCode.PK;
			Assert("Include in Profit Share is on", charge.JR_IsIncludedInProfitShare);
		}

		#endregion

		#region CFX

		public void TestCFXMinimumTakenIntoAccountWhenSettingForeignSellAmount()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "INBOM";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var orgFactory = new BusinessObjectFactory();
			var testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("To create CompanyData", testDebtor.CompanyData);
			testDebtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			orgFactory.Save();

			testDebtor = Factory.Load<OrgHeader>(testDebtor.PK);

			using (Job job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
			{
				job.PlugInData = shipment;

				job.LocalChargesPK = testDebtor.PK;
				ExchangeRate rate = job.ExchangeRates.AddNew();
				rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
				rate.JF_BaseRate = 0.7m;

				Charge charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;

				charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
				charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

				charge.JR_OSSellAmt = 50m;
				AssertEquals("Line exchange rate should be same as buy rate", 0.7m, charge.JR_OSSellExRate);
				AssertEquals("Local Sell Amount is 50 / 0.7", 71.43m, charge.JR_LocalSellAmt);

				//Job.JH_LocalChargesCFX = 4m;
				job.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 4m);
				job.ExchangeRates[0].JF_BaseRate = 0.7m; //this forces charge to refresh for there is no way to update CFX upon config changes

				AssertEquals("Line exchange rate should 4% uplift on Buy Rate", 0.672m, charge.JR_OSSellExRate);
				AssertEquals("Local Sell Amount is 50 / 0.672", 74.40m, charge.JR_LocalSellAmt);

				testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 4m, 50m);
				orgFactory.Save();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = testDebtor.PK;

				AssertEquals("Line exchange rate should compensate for the CFX Minimum", 0.411760m, charge.JR_OSSellExRate);
				AssertEquals("Local Sell Amount is 50 / 0.411760", 121.43m, charge.JR_LocalSellAmt);

				testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 4m, 0m);

				orgFactory.Save();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = testDebtor.PK;
				AssertEquals("Line exchange rate should 4% uplift on Buy Rate", 0.672m, charge.JR_OSSellExRate);
				AssertEquals("Local Sell Amount is 50 / 0.672", 74.40m, charge.JR_LocalSellAmt);

				testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air, 4m, 50m);
				orgFactory.Save();
				charge.JR_OH_SellAccount = ZGuid.Empty;
				charge.JR_OH_SellAccount = testDebtor.PK;

				charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				AssertEquals("Line exchange rate should be same as buy rate as bill in local is false", 0.7m, charge.JR_OSSellExRate);
				AssertEquals("Local Sell Amount is 50 / 0.7", 71.43m, charge.JR_LocalSellAmt);

				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				AssertEquals("Line exchange rate should compensate for the CFX Minimum", 0.411760m, charge.JR_OSSellExRate);
				AssertEquals("Local Sell Amount is 50 / 0.411760", 121.43m, charge.JR_LocalSellAmt);
			}
		}

		public void TestCFXMinimumTakenIntoAccountWhenSettingForeignSellAmountWithReciprocalExRateCompany()
		{
			ZString cnCountry = Constants.CountryCodes.China;

			GlbCompany cnCompany = Factory.NewWithValidTestData<GlbCompany>();
			cnCompany.GC_RN_NKCountryCode = cnCountry;
			cnCompany.GC_RX_NKLocalCurrency = "CNY";
			cnCompany.GC_IsReciprocal = true;
			Assert("Prerequisite: SIN company should be reciprocal", cnCompany.GC_IsReciprocal);
			GlbBranch cnBranch = Factory.NewWithValidTestData<GlbBranch>();
			cnBranch.GB_GC = cnCompany.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, cnBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				CommonShipment shipment = CommonShipment.New(Factory);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "INBOM";
				shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

				BusinessObjectFactory orgFactory = new BusinessObjectFactory();
				OrgHeader testDebtor = orgFactory.NewWithValidTestData<OrgHeader>();
				AssertNotNull("To create CompanyData", testDebtor.CompanyData);
				testDebtor.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
				orgFactory.Save();

				testDebtor = Factory.Load<OrgHeader>(testDebtor.PK);

				using (Job job = JobInvoicing.Job.CreateWithMutex(Factory, shipment))
				{
					job.PlugInData = shipment;

					job.LocalChargesPK = testDebtor.PK;
					ExchangeRate rate = job.ExchangeRates.AddNew();
					rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
					rate.JF_BaseRate = 3.4m;

					Charge charge = job.Charges.AddNew();
					charge.JR_AC = Env.Registry.FreightChargeCode;

					charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
					charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;

					charge.JR_OSSellAmt = 55m;
					AssertEquals("Line exchange rate should be same as buy rate", 3.4m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.4", 187.0m, charge.JR_LocalSellAmt);

					testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 5m, 0m);
					orgFactory.Save();
					charge.JR_OH_SellAccount = ZGuid.Empty;
					charge.JR_OH_SellAccount = testDebtor.PK;

					AssertEquals("Line exchange rate should 5% uplift on Buy Rate", 3.57m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.57", 196.35m, charge.JR_LocalSellAmt);

					testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 5m, 25m);
					orgFactory.Save();
					charge.JR_OH_SellAccount = ZGuid.Empty;
					charge.JR_OH_SellAccount = testDebtor.PK;

					AssertEquals("Line exchange rate should compensate for the CFX Minimum", 3.854545m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.4 + 25.0", 212.0m, charge.JR_LocalSellAmt);

					testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 5m, 0m);
					orgFactory.Save();
					charge.JR_OH_SellAccount = ZGuid.Empty;
					charge.JR_OH_SellAccount = testDebtor.PK;

					AssertEquals("Line exchange rate should 5% uplift on Buy Rate", 3.57m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.57", 196.35m, charge.JR_LocalSellAmt);

					testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 5m, 25m);
					orgFactory.Save();
					charge.JR_OH_SellAccount = ZGuid.Empty;
					charge.JR_OH_SellAccount = testDebtor.PK;

					charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
					AssertEquals("Line exchange rate should be same as buy rate", 3.4m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.4", 187.0m, charge.JR_LocalSellAmt);

					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					AssertEquals("Line exchange rate should compensate for the CFX Minimum", 3.854545m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.4 + 25.0", 212.0m, charge.JR_LocalSellAmt);

					testDebtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea, 15m, 25m);
					orgFactory.Save();
					charge.JR_OH_SellAccount = ZGuid.Empty;
					charge.JR_OH_SellAccount = testDebtor.PK;
					AssertEquals("Line exchange rate should 15% uplift on Buy Rate and is not compensated by CFX minimum", 3.91m, charge.JR_OSSellExRate);
					AssertEquals("Local Sell Amount is 55 * 3.91", 215.05m, charge.JR_LocalSellAmt);
				}
			}
		}

		public void TestCFXTransactionLineCreated()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Job job = CreateJob("Z00001013", ZECTRA, true, 10M, ABIGAS, true, 0M);
			ExchangeRate rate1 = CreateExchangeRate(job, TestObjectCreator.USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(job, TestObjectCreator.GBP, .4M);
			ChargeWithCost chargeWithCost = CreateCharge(job, MRG100, "CFX Transaction Line Test", TestObjectCreator.USD, 200M, AALSHI, TestObjectCreator.AUD, 300M, ZECTRA);

			AssertNull("CFX Line Exists", chargeWithCost.CFXLine);
			ZDateTime now = ZDateTime.Now;

			JCJournalHeader cFXHeader = Factory.New<JCJournalHeader>();
			chargeWithCost.CreateCFXTransactionLine(cFXHeader, now);

			AssertNotNull("CFX Line Exists", chargeWithCost.CFXLine);
			AssertEquals("Post Date", now, chargeWithCost.CFXLine.AL_PostDate);
		}

		public void TestLineCFXIsUpdatedBackToZero()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestJob.LocalChargesPK = TestObjectCreator.TestOrganisation.PK;
			TestJob.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 50m);

			ChargeWithCost newCharge = TestJob.Charges.AddNew();
			newCharge.JR_AC = MRG100.PK;
			newCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			newCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			newCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			AssertEquals("Initial value", TestObjectCreator.TestOrganisation.PK, newCharge.JR_OH_SellAccount);
			AssertEquals("Initial value", 50m, newCharge.JR_LineCFX);

			newCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			AssertEquals("new value for LineCFX", 0m, newCharge.JR_LineCFX);
		}

		#endregion

		#region TestObjectCreator.GST1 and WHT Validation

		public void TestValidateJR_AT_CostGSTRate()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_LocalCostAmt = 100;
			ACharge.JR_OH_CostAccount = TestObjectCreator.TestOrganisation.PK;

			AssertEquals("default TestObjectCreator.GST1 value", TestObjectCreator.GST1.PK, ACharge.JR_AT_CostGSTRate);
			AssertEquals(0, ACharge.JR_AT_CostGSTRateInfo.GetErrors().Count());

			ACharge.JR_AT_CostGSTRate = ZGuid.Empty;
			AssertEquals(1, ACharge.JR_AT_CostGSTRateInfo.GetErrors().Count());
		}

		public virtual void TestValidateJR_AT_SellGSTRate()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			ACharge.JR_AC = TestObjectCreator.MRG100.PK;
			ACharge.JR_LocalSellAmt = 100;
			ACharge.JR_OH_SellAccount = TestObjectCreator.TestOrganisation.PK;

			AssertEquals("default TestObjectCreator.GST1 value", TestObjectCreator.GST1.PK, ACharge.JR_AT_SellGSTRate);
			AssertEquals(0, ACharge.JR_AT_SellGSTRateInfo.GetErrors().Count());

			ACharge.JR_AT_SellGSTRate = ZGuid.Empty;
			AssertEquals(1, ACharge.JR_AT_SellGSTRateInfo.GetErrors().Count());
		}

		#endregion

		#region Cheque Number Auto Allocation

		public void TestIsChequeNumberAutoAllocated()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			aCharge.JR_AB = testBank.PK;
			Assert("ChequeBook is not set, should return False", !aCharge.IsChequeNumberAutoAllocated);

			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("PaymentType is Cash, should return False", !aCharge.IsChequeNumberAutoAllocated);

			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);

			aCharge.JR_AK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !aCharge.IsChequeNumberAutoAllocated);
		}

		public void TestCalc_ChequeNumberIsAutoAllocatedLabel()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			Assert("ChequeBook is not set, should return False", !aCharge.IsChequeNumberAutoAllocated);
			Assert("Label should be empty yet", aCharge.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);

			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			AssertEquals("Label should be set to right value", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, aCharge.Calc_ChequeNumberIsAutoAllocatedLabel);

			aCharge.JR_ChequeNo = "BLAHBLAHBLAH!";
			Assert("Label should become empty as JR_ChequeNo is not empty (means it was autoallocated already)", aCharge.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
		}

		public void TestCalc_ChequeNumberIsAutoAllocatedLabel_IsCostPosted()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			AssertEquals("Label should be set to right value", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, aCharge.Calc_ChequeNumberIsAutoAllocatedLabel);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			transactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			aCharge.JR_AL_APLine = transactionLine.PK;
			Assert("Label should become empty as the IsCostPosted is now True", aCharge.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
		}

		public void TestSettingJR_ChequeNoDoesNotChangeAK_CurrentNoWhenAutoAllocationEnabled()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			AccChequeBook testChequeBook = TestObjectCreator.CreateChequeBook(0, 1, 100, testBank);
			Factory.Save();

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			aCharge.JR_ChequeNo = "22";
			AssertEquals("AK_CurrentNo on cheque book should remain old", 0m, testBookWithAutoAllocation.AK_CurrentNo);

			aCharge.JR_AK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !aCharge.IsChequeNumberAutoAllocated);
			aCharge.JR_ChequeNo = "22";
			AssertEquals("AK_CurrentNo on cheque book should change", 23m, testChequeBook.AK_CurrentNo);
		}

		public void TestSettingJR_AKDoesNotChangeJR_ChequeNoWhenAutoAllocationEnabled()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_CurrentNo = 5;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			Assert("JR_ChequeNo should remain empty", aCharge.JR_ChequeNo.IsEmpty);

			aCharge.JR_AK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !aCharge.IsChequeNumberAutoAllocated);
			AssertEquals("JR_ChequeNo should change", "55", aCharge.JR_ChequeNo);
		}

		public void TestChangingChequeBookToAutoAllocateWillResetJR_ChequeNo()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_CurrentNo = 5;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;

			ChargeWithCost aCharge = (ChargeWithCost)Factory.New(GetExpectedBusinessObjectType());
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			aCharge.JR_AK = testChequeBook.PK;
			Assert("Cheque Book is not IsAutoPrint, should return False", !aCharge.IsChequeNumberAutoAllocated);
			AssertEquals("JR_ChequeNo should change", "55", aCharge.JR_ChequeNo);

			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			Assert("JR_ChequeNo should be reset", aCharge.JR_ChequeNo.IsEmpty);
		}

		public void TestLoadingAlreadyPostedChargeWhithAutoAllocateChequeBook()
		{
			Job job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0M);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_CurrentNo = 5;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;

			ChargeWithCost aCharge = job.Charges.AddNew();
			aCharge.JR_AC = TestObjectCreator.CC1.PK;
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			aCharge.JR_ChequeNo = "55";

			AccTransactionLines newDummyAPLine = Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			newDummyAPLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			newDummyAPLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			aCharge.JR_AL_APLine = newDummyAPLine.PK;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Charge reloadedCharge = newFactory.Load<Charge>(aCharge.PK);
			AssertEquals("ChargeNo should remain old", "55", reloadedCharge.JR_ChequeNo);
		}

		#endregion

		public void TestClearCurrencyFieldsAndAmountsSetDefaultCurrency()
		{
			var costAccount = TestObjectCreator.AALSHI;
			costAccount.CompanyData.OB_RX_NKAPDefltCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var sellAccount = TestObjectCreator.ABIGAS;
			sellAccount.CompanyData.OB_RX_NKARDDefltCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;

			ChargeWithCost chargeWithCost = CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, costAccount, TestObjectCreator.AUD, 100m, sellAccount);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, chargeWithCost.JR_RX_NKCostCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, chargeWithCost.JR_RX_NKSellCurrency);

			chargeWithCost.JR_AC = TestObjectCreator.CC2.PK;
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, chargeWithCost.JR_RX_NKCostCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, chargeWithCost.JR_RX_NKSellCurrency);

			chargeWithCost.JR_AC = TestObjectCreator.RevenueChargeCode.PK;
			AssertEquals(string.Empty, chargeWithCost.JR_RX_NKCostCurrency);
			AssertEquals(Core.Constants.CurrencyCodes.KoreaRepublicOf, chargeWithCost.JR_RX_NKSellCurrency);
		}

		public void TestAccrualsAndWIPsNotCreatedWhenClosingJob()
		{
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var job = TestObjectCreator.Job1;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			Factory.Save();

			var lines = Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK).AddToFilter(AccTransactionLinesSchema.AL_GC, job.JH_GC));
			AssertEquals(0, lines.Length);
			AssertEquals(ZGuid.Empty, charge.JR_AL_APLine);
			AssertEquals(ZGuid.Empty, charge.JR_AL_ARLine);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			job.Close(null, null);
			Factory.Save();

			lines = Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, job.PK).AddToFilter(AccTransactionLinesSchema.AL_GC, job.JH_GC));
			AssertEquals(0, lines.Length);
			AssertEquals(ZGuid.Empty, charge.JR_AL_APLine);
			AssertEquals(ZGuid.Empty, charge.JR_AL_ARLine);
		}

		public void TestAmountsRoundedWithCurrencyChange()
		{
			var charge = Factory.NewWithValidTestData<ChargeWithCost>();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			charge.JR_JH = job.PK;
			charge.JR_OH_CostAccount = charge.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge.JR_RX_NKCostCurrency = charge.JR_RX_NKSellCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 100)).Code;
			charge.JR_OSCostExRate = charge.JR_OSSellExRate = 1m;

			charge.JR_AT_CostGSTRate = charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;

			charge.JR_OSCostAmt = charge.JR_OSSellAmt = 11m;
			AssertEquals("cost amt not rounded", 11m, charge.JR_OSCostAmt);
			AssertEquals("tax is ten percent", 1.1m, charge.JR_OSCostGSTAmt_Calc);
			AssertEquals("sell amt not rounded", 11m, charge.JR_OSSellAmt);
			AssertEquals("tax is ten percent", 1.1m, charge.JR_OSSellGSTAmt_Calc);

			charge.JR_RX_NKCostCurrency = charge.JR_RX_NKSellCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 1)).Code;
			AssertEquals("0 dec pts", 0, charge.OSCostCurrencyDecimals);
			AssertEquals("0 dec pts", 0, charge.OSSellCurrencyDecimals);

			AssertEquals("cost amt rounded", 11m, charge.JR_OSCostAmt);
			AssertEquals("tax is ten percent but rounded", 1m, charge.JR_OSCostGSTAmt_Calc);
			AssertEquals("sell amt rounded", 11m, charge.JR_OSSellAmt);
			AssertEquals("tax is ten percent but rounded", 1m, charge.JR_OSSellGSTAmt_Calc);
		}

		public void TestIsSisterCompanyCharge()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var orgHeader = TestObjectCreator.CreateOrgHeader("ORGUS", false, true, "USCHI");
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("ORGNZ1", false, true, "NZAKL");
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("ORGNZ2", false, true, "NZAKL");
			var orgHeader3 = TestObjectCreator.CreateOrgHeader("ORGAU1", false, true, "AUMEL");
			var orgHeader4 = TestObjectCreator.CreateOrgHeader("ORGAU2", false, true, "AUMEL");
			var orgHeader5 = TestObjectCreator.CreateOrgHeader("ORGAU3", false, true, "AUMEL");

			var companyNZ = TestObjectCreator.CreateNewCompany("NZ1", "NZ");
			companyNZ.GC_OH_OrgProxy = orgHeader1.PK;

			var branchNZ = TestObjectCreator.CreateNewBranch(companyNZ, "AKL");
			branchNZ.GB_OH_OrgProxy = orgHeader2.PK;

			var companyAU = TestObjectCreator.CreateNewCompany("AU1", "AU");
			companyAU.GC_OH_OrgProxy = orgHeader3.PK;

			var branchAU = TestObjectCreator.CreateNewBranch(companyAU, "SY1");
			branchAU.GB_OH_OrgProxy = orgHeader4.PK;

			GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_OH_OrgProxy = orgHeader5.PK;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			var charge = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, orgHeader);
			var charge1 = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, orgHeader1);
			var charge2 = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, orgHeader2);
			var charge3 = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, orgHeader3);
			var charge4 = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, orgHeader4);
			var charge5 = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, orgHeader5);
			var charge6 = CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 120M, null);

			Factory.Save();

			Assert("Charge debtor is not a sister company - !(shouldApplyLocalCompanyFilter)", !charge.IsSisterCompanyCharge(false));
			Assert("Charge debtor is a Company proxy - !(shouldApplyLocalCompanyFilter)", charge1.IsSisterCompanyCharge(false));
			Assert("Charge debtor is a Branch proxy - !(shouldApplyLocalCompanyFilter)", charge2.IsSisterCompanyCharge(false));
			Assert("Charge debtor is a Company proxy - !(shouldApplyLocalCompanyFilter)", charge3.IsSisterCompanyCharge(false));
			Assert("Charge debtor is a Branch proxy - !(shouldApplyLocalCompanyFilter)", charge4.IsSisterCompanyCharge(false));
			Assert("Charge debtor is proxy of local login company - !(shouldApplyLocalCompanyFilter)", !charge5.IsSisterCompanyCharge(false));

			Assert("Charge debtor is not a sister company - (shouldApplyLocalCompanyFilter)", !charge.IsSisterCompanyCharge(true));
			Assert("Charge debtor is a Company proxy - (shouldApplyLocalCompanyFilter)", !charge1.IsSisterCompanyCharge(true));
			Assert("Charge debtor is a Branch proxy - (shouldApplyLocalCompanyFilter)", !charge2.IsSisterCompanyCharge(true));
			Assert("Charge debtor is a local Company proxy - (shouldApplyLocalCompanyFilter)", charge3.IsSisterCompanyCharge(true));
			Assert("Charge debtor is a local Branch proxy - (shouldApplyLocalCompanyFilter)", charge4.IsSisterCompanyCharge(true));
			Assert("Charge debtor is proxy of local login company - !(shouldApplyLocalCompanyFilter)", !charge5.IsSisterCompanyCharge(true));
			Assert("Does not throw on empty debtor", !charge6.IsSisterCompanyCharge(true));
		}

		public void TestIsAgentCharge()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
			job.PlugInData = shipment;
			ChargeWithCost chargeWithCost1 = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, job.AgentCollect);
			AssertEquals("This is an agent charge", true, chargeWithCost1.IsAgentCharge);

			ChargeWithCost chargeWithCost2 = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, consol.ReceivingForwarder);
			AssertEquals("This is an agent charge", true, chargeWithCost2.IsAgentCharge);

			ChargeWithCost chargeWithCost3 = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, consol.SendingForwarder);
			AssertEquals("This is an agent charge", true, chargeWithCost3.IsAgentCharge);
		}

		public void TestDontCreateWIPsAndAccrualsForSpotQuoteJobsWhereParentIsNull()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = ZGuid.NewZGuid();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_JobNum = "S00001001";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			AssertNotNull("WIP should be created", charge.WIP);
			AssertNotNull("Accrual should be created", charge.Accrual);

			job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = ZGuid.NewZGuid();
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			job.JH_JobNum = "00001000";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OSSellAmt = 100m;
			Factory.Save();

			AssertNull("WIP should never be created for jobs linked to rating header", charge.WIP);
			AssertNull("Accrual should never be created for jobs linked to rating header", charge.Accrual);
		}

		public void TestAPInvoiceDetails()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = ZGuid.NewZGuid();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_JobNum = "S00001001";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;

			ChargeWithCost charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSCostAmt = 100m;
			charge.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			charge.JR_APInvoiceNum = "PJW1000000";
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_PaymentDate = ZDateTime.Today;
			charge.JR_PaymentType = "CHQ";
			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_TransactionNum = "PJW1000000";
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_JH = job.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_OSExTaxAmount = 100m;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;

			Factory.Save();

			Charge charge1 = job.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_AL_APLine == line.PK);
			AssertNotNull("A charge linked to the line shoudl be found", charge1);
			AssertEquals("The charge should not have cheque book defined", ZGuid.Empty, charge1.JR_AK);
			AssertEquals("The charge should not have bank account defined", ZGuid.Empty, charge1.JR_AB);
			AssertEquals("The charge should not have payment type defined", "", charge1.JR_PaymentType);
			AssertEquals("The charge should not cheque number defined", "", charge1.JR_ChequeNo);
		}

		public void TestIsDeferredCharge()
		{
			ChargeWithCost testCharge = (ChargeWithCost)GetNewBusinessObject();

			TestObjectCreator.CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			TestObjectCreator.CC2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			OrgInvoiceType invoiceType = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			invoiceType.PI_RS_NKServiceLevel = "STD";
			OrgInvTypeDeferredCharges deferredCharges1 = invoiceType.DeferredCharges.AddNew();
			OrgInvTypeDeferredCharges deferredCharges2 = invoiceType.DeferredCharges.AddNew();
			deferredCharges1.PO_AC = TestObjectCreator.CC3.PK;
			deferredCharges2.PO_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			BusinessObject shipment = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			((CommonShipment)shipment).JS_RS_NKServiceLevel = "STD";

			using (Job job = TestObjectCreator.CreateJob(shipment as IJobInvoicingPlugIn))
			{
				TestObjectCreator.ABIGAS.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
				OrgInvoiceRollupOrGroup rollup = TestObjectCreator.ABIGAS.CompanyData.InvoiceRollupOrGroups.AddNew();
				rollup.PG_JobType = "ALL";
				rollup.PG_ServiceDirection = "ALL";
				rollup.PG_TransportMode = "ALL";
				rollup.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
				TestObjectCreator.ABIGAS.Factory.Save();

				testCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				testCharge.JR_JH = job.PK;

				testCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(testCharge.IsDeferredCharge);

				testCharge.JR_AC = TestObjectCreator.CC2.PK;
				Assert(testCharge.IsDeferredCharge);

				testCharge.JR_AC = TestObjectCreator.CC3.PK;
				Assert(testCharge.IsDeferredCharge);

				invoiceType.PI_Module = JobInvoicingConsumerTypes.Brokerage.Code;
				invoiceType.PI_RS_NKServiceLevel = "STD";
				TestObjectCreator.ABIGAS.CompanyData.ClearInvoiceTypeCache_ForTestOnly();
				testCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(!testCharge.IsDeferredCharge);
			}

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			((BaseJobDeclaration)declaration).JE_RS_NKServiceLevel = "STD";

			using (Job job = TestObjectCreator.CreateJob(declaration as IJobInvoicingPlugIn))
			{
				testCharge.JR_JH = job.PK;
				job.Factory.Save();
				testCharge.JR_AC = ZGuid.Empty;
				testCharge.JR_AC = TestObjectCreator.CC1.PK;
				Assert(testCharge.IsDeferredCharge);

				invoiceType.PI_Module = JobInvoicingConsumerTypes.CFSShipment.Code;
				invoiceType.PI_RS_NKServiceLevel = "STD";
			}

			BusinessObject consol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C001", receivingGatewayCompany: GlbCompany.CurrentCompany);

			using (Job job = TestObjectCreator.CreateJob(consol as IJobInvoicingPlugIn))
			{
				testCharge.JR_JH = job.PK;
				Assert(testCharge.IsDeferredCharge);
			}

			BusinessObject cfsShipment = (BusinessObject)Factory.New<Freight.Integration.CFS.ICFSShipment>();
			((CommonShipment)cfsShipment).JS_RS_NKServiceLevel = "STD";

			using (Job job = TestObjectCreator.CreateJob(cfsShipment as IJobInvoicingPlugIn))
			{
				testCharge.JR_JH = job.PK;
				Assert(testCharge.IsDeferredCharge);
			}

			invoiceType.PI_Module = JobInvoicingConsumerTypes.LocalCartage.Code;
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.LocalCartage);
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.OverriddenDepartmentPK).Returns(ZGuid.NewZGuid());
			mockSupporter.Setup(m => m.EditSecurityLock).Returns(false);
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(It.IsAny<ZGuid>())).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.CanCreateInvoicingJob).Returns(true);

			var mockPlugIn = new Mock<IJobInvoicingPlugIn>();
			mockPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockPlugIn.Setup(m => m.IsDeleted).Returns(false);
			mockPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockPlugIn.Setup(m => m.TableName).Returns("JobCartage");
			mockPlugIn.Setup(m => m.IsInDatabase).Returns(true);

			using (Job job = TestObjectCreator.CreateJob(mockPlugIn.Object))
			{
				testCharge.JR_JH = job.PK;
				Assert(testCharge.IsDeferredCharge);
				mockPlugIn.Verify();
			}
		}

		[ExpectNoExceptions]
		public void TestCreateAccrualAndWIPShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			Job job = CreateJob("Z00001011", AALSHI, true, 0M, ABIGAS, true, 0M);
			job.PlugInData = shipment;

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank);
			testBookWithAutoAllocation.AK_CurrentNo = 5;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 55;

			ChargeWithCost aCharge = job.Charges.AddNew();
			aCharge.JR_AC = TestObjectCreator.CC1.PK;
			aCharge.JR_APInvoiceNum = "01";
			aCharge.JR_OH_CostAccount = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			aCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aCharge.JR_AB = testBank.PK;
			aCharge.JR_AK = testBookWithAutoAllocation.PK;
			Assert("AutoAllocation should be enabled", aCharge.IsChequeNumberAutoAllocated);
			aCharge.JR_ChequeNo = "55";

			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsCFSRegistered = false;

			((IBusinessObjectInternals)shipment).MarkAsDeleted();
			aCharge.CreateAccrualAndWIP_ForTestOnly();
		}

		public void TestCostCurrencyUpdatesBankAccountsList()
		{
			AccBankAccount aUDBank = Factory.New<AccBankAccount>();
			aUDBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AccBankAccount uSDBank = Factory.New<AccBankAccount>();
			OrgHeader creditor = Factory.New<OrgHeader>();
			uSDBank.AB_RX_NKAccountCurrency = TestObjectCreator.USD.RX_Code;

			TestCharge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			TestCharge.JR_OH_CostAccount = creditor.PK;
			TestCharge.JR_APInvoiceNum = "123456";
			TestCharge.JR_APInvoiceDate = ZDateTime.Today;
			TestCharge.JR_PaymentDate = ZDateTime.Today;
			TestCharge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestCharge.JR_AB = aUDBank.PK;
			AssertNoErrors(TestCharge.JR_ABInfo);

			TestCharge.JR_AB = uSDBank.PK;
			AssertHasError(TestCharge.JR_ABInfo, "Bank Account currency is incorrect. Choose AUD currency Bank Account.");

			TestCharge.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			TestCharge.JR_AB = aUDBank.PK;
			AssertHasError(TestCharge.JR_ABInfo, "Bank Account currency is incorrect. Choose USD currency Bank Account.");

			TestCharge.JR_AB = uSDBank.PK;
			AssertNoErrors(TestCharge.JR_ABInfo);

			Job job = Factory.NewJobForTesting<Job>();
			TestCharge.JR_JH = job.PK;
			Charge testCharge2 = job.Charges.AddNew();
			testCharge2.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			testCharge2.JR_OH_CostAccount = creditor.PK;
			testCharge2.JR_APInvoiceNum = "123456";
			testCharge2.JR_APInvoiceDate = ZDateTime.Today;
			testCharge2.JR_PaymentDate = ZDateTime.Today;
			testCharge2.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testCharge2.JR_AB = aUDBank.PK;
			TestCharge.JR_AB = aUDBank.PK;
			TestCharge.Validation.ValidateJR_AB();
			testCharge2.Validation.ValidateJR_AB();
			AssertNoErrors(testCharge2.JR_ABInfo);
			AssertNoErrors(TestCharge.JR_ABInfo);

			TestCharge.JR_AB = uSDBank.PK;
			testCharge2.JR_AB = uSDBank.PK;
			TestCharge.Validation.ValidateJR_AB();
			testCharge2.Validation.ValidateJR_AB();
			AssertHasError(TestCharge.JR_ABInfo, "Bank Account currency is incorrect. Choose AUD currency Bank Account.");
			AssertHasError(testCharge2.JR_ABInfo, "Bank Account currency is incorrect. Choose AUD currency Bank Account.");
		}

		public void TestCreateWIPAndAcrualReversesPreviousOnes()
		{
			TestObjectCreator.CreateJobChargeRevRecognition(ACharge.InvoicingJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);
			ACharge.FillWithValidTestData();
			ACharge.JR_OSCostAmt = 100;
			ACharge.JR_OSSellAmt = 100;
			ACharge.CreateAccrualAndWIP_ForTestOnly();

			AssertNotNull("Precondition: WIP must exist", ACharge.WIP);
			AssertNotNull("Precondition: Accrual must exist", ACharge.Accrual);

			Accrual oldAccrual = ACharge.Accrual;
			WIP oldWIP = ACharge.WIP;

			ACharge.JR_OSCostAmt = ACharge.JR_OSCostAmt + 100;
			ACharge.JR_OSSellAmt = ACharge.JR_OSSellAmt + 100;

			ACharge.CreateAccrualAndWIP_ForTestOnly();
			AssertNotEquals("Postcondition: new Accrual must be created", ACharge.Accrual.PK, oldAccrual.PK);
			Assert("Previous Accrual must be reversed.", oldAccrual.IsReversed);

			AssertNotEquals("Postcondition: new WIP must be created", ACharge.WIP.PK, oldWIP.PK);
			Assert("Previous WIP must be reversed.", oldWIP.IsReversed);
		}

		public void TestReverseWIPAccrualsWhenNewOneCantBeCreated()
		{
			ACharge.JR_AC = TestObjectCreator.CC1.PK;
			ACharge.JR_OSCostAmt = 100M;
			ACharge.JR_OSSellAmt = 100M;
			Factory.Save();

			Accrual accrual = ACharge.Accrual;
			WIP wip = ACharge.WIP;
			AssertNotNull("Precondition: accrual should be created.", accrual);
			AssertNotNull("Precondition: wip should be created.", wip);

			RevenueRecognitionCollection revRecColection = new RevenueRecognitionCollection();
			RevenueRecognition revRecOption = revRecColection.AddNew();
			revRecOption.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			revRecOption.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revRecOption.Mode = Core.Constants.TransportModes.All;
			revRecOption.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecColection);
			ACharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertEquals("Precondition: WIP and Accrual can't be created.", ZDateTime.Empty, ACharge.InvoicingJob.GetRevenueRecognitionDate(ACharge.InvoicingJob.GetRevenueRecognitionType(ACharge.ChargeCode)));

			ACharge.JR_OSCostAmt = 200M;
			ACharge.JR_OSSellAmt = 200M;
			Factory.Save();
			AssertEquals("Previous accrual should be reversed.", true, accrual.IsReversed);
			AssertEquals("Previous wip should be reversed.", true, wip.IsReversed);
			AssertNull("Accrual shouldn't be created.", ACharge.Accrual);
			AssertNull("Wip shouldn't be created.", ACharge.WIP);
		}

		public void TestCreateWIPAndAcrual_WithNegativeAccrualsBehaviour()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestObjectCreator.CreateJobChargeRevRecognition(ACharge.InvoicingJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, ZDateTime.Empty);
			ACharge.FillWithValidTestData();
			ACharge.JR_OSCostAmt = -100;
			ACharge.JR_OSSellAmt = -100;
			Factory.Save();

			AssertNotNull("Precondition: WIP must exist", ACharge.WIP);
			AssertNotNull("Precondition: Accrual must exist", ACharge.Accrual);

			Accrual oldAccrual = ACharge.Accrual;
			WIP oldWIP = ACharge.WIP;

			ACharge.JR_OSCostAmt = ACharge.JR_OSCostAmt + 100;
			ACharge.JR_OSSellAmt = ACharge.JR_OSSellAmt + 100;

			ACharge.CreateAccrualAndWIP_ForTestOnly();
			AssertNull("Postcondition: new Accrual must be created", ACharge.Accrual);
			Assert("Previous Accrual must be reversed.", oldAccrual.IsReversed);

			AssertNull("Postcondition: new WIP must be created", ACharge.WIP);
			Assert("Previous WIP must be reversed.", oldWIP.IsReversed);

			ACharge.JR_OSCostAmt = ACharge.JR_OSCostAmt + 100;
			ACharge.JR_OSSellAmt = ACharge.JR_OSSellAmt + 100;

			ACharge.CreateAccrualAndWIP_ForTestOnly();
			AssertNotNull("Postcondition: new Accrual must be created", ACharge.Accrual);

			AssertNotNull("Postcondition: new WIP must be created", ACharge.WIP);

			oldAccrual = ACharge.Accrual;
			oldWIP = ACharge.WIP;
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			ACharge.JR_OSCostAmt = ACharge.JR_OSCostAmt - 200;
			ACharge.JR_OSSellAmt = ACharge.JR_OSSellAmt - 200;
			Factory.Save();

			AssertNull("Postcondition: new Accrual must be created", ACharge.Accrual);
			Assert("Previous Accrual must be reversed.", oldAccrual.IsReversed);

			AssertNull("Postcondition: new WIP must be created", ACharge.WIP);
			Assert("Previous WIP must be reversed.", oldWIP.IsReversed);

			ACharge.JR_OSCostAmt = 0;
			ACharge.JR_OSSellAmt = 0;
			Factory.Save();

			AssertNull("Postcondition: new Accrual must be created", ACharge.Accrual);
			AssertNull("Postcondition: new WIP must be created", ACharge.WIP);
		}

		public void TestRecognizeRevenueOnFactorySavingButAfterWipAccrualReversing()
		{
			RevenueRecognitionCollection revRecColection = new RevenueRecognitionCollection();
			RevenueRecognition revRecOption = revRecColection.AddNew();
			revRecOption.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revRecOption.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			revRecOption.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			revRecOption.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecColection);
			ACharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			ZDateTime pickupDate = ZDateTime.Today.AddDays(-20);
			ZDateTime deliveryDate = ZDateTime.Today.AddDays(-10);
			TestObjectCreator.CreateTestPeriods(pickupDate);
			TestObjectCreator.CreateJobChargeRevRecognition(ACharge.InvoicingJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, pickupDate);
			TestObjectCreator.CreateJobChargeRevRecognition(ACharge.InvoicingJob, RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, deliveryDate);
			CommonShipment shipment = Factory.New<CommonShipment>();
			ACharge.FillWithValidTestData();
			ACharge.InvoicingJob.PlugInData = shipment;
			ACharge.JR_OSCostAmt = 100;
			ACharge.JR_OSSellAmt = 100;
			Factory.Save();

			AssertNotNull("WIP must exist", ACharge.WIP);
			AssertNotNull("Accrual must exist", ACharge.Accrual);
			AssertEquals("ACharge.WIP.AL_PostDate", pickupDate, ACharge.WIP.AL_PostDate);
			AssertEquals("ACharge.Accrual.AL_PostDate", pickupDate, ACharge.Accrual.AL_PostDate);
			AssertEquals("CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, ACharge.CostRecognition);
			AssertEquals("SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, ACharge.SellRecognition);

			revRecOption.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecColection);
			ACharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			ACharge.JR_OSCostAmt = 200;
			ACharge.JR_OSSellAmt = 200;
			Factory.Save();

			AssertNotNull("WIP must exist", ACharge.WIP);
			AssertNotNull("Accrual must exist", ACharge.Accrual);
			AssertEquals("ACharge.WIP.AL_PostDate", deliveryDate, ACharge.WIP.AL_PostDate);
			AssertEquals("ACharge.Accrual.AL_PostDate", deliveryDate, ACharge.Accrual.AL_PostDate);
			AssertEquals("CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, ACharge.CostRecognition);
			AssertEquals("SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate, ACharge.SellRecognition);

			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			revRecOption.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecColection);
			ACharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			ZDateTime arrivalDate = ZDateTime.Today.AddDays(-15);
			shipment.JS_E_ARV = arrivalDate;
			ACharge.JR_OSCostAmt = 300;
			ACharge.JR_OSSellAmt = 300;
			Factory.Save();

			AssertNotNull("WIP must exist", ACharge.WIP);
			AssertNotNull("Accrual must exist", ACharge.Accrual);
			AssertEquals("ACharge.WIP.AL_PostDate", arrivalDate, ACharge.WIP.AL_PostDate);
			AssertEquals("ACharge.Accrual.AL_PostDate", arrivalDate, ACharge.Accrual.AL_PostDate);
			AssertEquals("CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ACharge.CostRecognition);
			AssertEquals("SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, ACharge.SellRecognition);

			revRecOption.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revRecColection);
			ACharge.InvoicingJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			ACharge.JR_OSCostAmt = 400;
			ACharge.JR_OSSellAmt = 400;
			Factory.Save();

			AssertNull("WIP must not exist", ACharge.WIP);
			AssertNull("Accrual must not exist", ACharge.Accrual);
			AssertEquals("CostRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, ACharge.CostRecognition);
			AssertEquals("SellRecognition", RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate, ACharge.SellRecognition);
		}

		public void TestChargeNotReloadedIfDataRefreshDisabled()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var job = testObjectCreator.CreateJob("JOB1", null, 0M, null, 0M);
			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc", testObjectCreator.AUD, 10M, null, testObjectCreator.AUD, 10M, null);
			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc", testObjectCreator.AUD, 10M, null, testObjectCreator.AUD, 10M, null);
			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc", testObjectCreator.AUD, 10M, null, testObjectCreator.AUD, 10M, null);
			factory.Save();
			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc", testObjectCreator.AUD, 10M, null, testObjectCreator.AUD, 10M, null);
			string chargeTable = ChargeWithCost.Schema.TableName;
			int dbLoadsBefore = factory.GetTableHitCount(chargeTable);
			factory.Save();
			AssertEquals("Charges shouldn't be reloaded when refresh bus disabled in a Factory", 1, factory.GetTableHitCount(chargeTable) - dbLoadsBefore);

			dbLoadsBefore = factory.GetTableHitCount(chargeTable);
			factory.RefreshEnabled = false;
			factory.Save();
			AssertEquals("Charges shouldn't be reloaded when refresh bus disabled in a Factory", 0, factory.GetTableHitCount(chargeTable) - dbLoadsBefore);
		}

		public void TestGet_IsCostGSTRateActual_NullChargeCode()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge testCharge = testJob.Charges.AddNew();
			AssertNull(testCharge.ChargeCode);
			AssertNoExceptionThrown("Expect not throw exception when get_IsCostGSTRateActual", () => { bool test = testCharge.IsCostGSTRateActual; });
		}

		public void TestJR_OSSellAmt_TooLargeOrSmall()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			var testCharge = testJob.Charges.AddNew();
			var amount = 2m;
			testCharge.JR_OSSellAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true));
			AssertNoError("Precondition", testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true));

			amount = 9999999999999999999999999999m;
			testCharge.JR_OSSellAmt = amount;
			Assert(amount == testCharge.JR_LocalSellAmt);
			AssertHasError(testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true));
			AssertHasError(testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true));

			amount = 2m;
			testCharge.JR_OSSellAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true, true));
			AssertNoError("Precondition", testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true, true));

			amount = -9999999999999999999999999999m;
			testCharge.JR_OSSellAmt = amount;
			Assert(amount == testCharge.JR_LocalSellAmt);
			AssertHasError(testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true, true));
			AssertHasError(testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true, true));
		}

		public void TestJR_LocalSellAmt_TooLargeOrSmall()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			var testCharge = testJob.Charges.AddNew();
			var amount = 2m;
			testCharge.JR_LocalSellAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true));
			AssertNoError("Precondition", testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true));

			amount = 9999999999999999999999999999m;
			testCharge.JR_LocalSellAmt = amount;
			Assert(amount == testCharge.JR_OSSellAmt);
			AssertHasError(testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true));
			AssertHasError(testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true));

			amount = 2m;
			testCharge.JR_LocalSellAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true, true));
			AssertNoError("Precondition", testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true, true));

			amount = -9999999999999999999999999999m;
			testCharge.JR_LocalSellAmt = amount;
			Assert(amount == testCharge.JR_OSSellAmt);
			AssertHasError(testCharge.JR_OSSellAmtInfo, GetErrorMessageForValidMoneyCheck(false, true, true));
			AssertHasError(testCharge.JR_LocalSellAmtInfo, GetErrorMessageForValidMoneyCheck(true, true, true));
		}

		public void TestJR_OSCostAmt_TooLargeOrSmall()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			var testCharge = testJob.Charges.AddNew();
			var amount = 2m;
			testCharge.JR_OSCostAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false));
			AssertNoError("Precondition", testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false));

			amount = 9999999999999999999999999999m;
			testCharge.JR_OSCostAmt = amount;
			Assert(amount == testCharge.JR_LocalCostAmt);
			AssertHasError(testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false));
			AssertHasError(testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false));

			amount = 2m;
			testCharge.JR_OSCostAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false, true));
			AssertNoError("Precondition", testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false, true));

			amount = -9999999999999999999999999999m;
			testCharge.JR_OSCostAmt = amount;
			Assert(amount == testCharge.JR_LocalCostAmt);
			AssertHasError(testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false, true));
			AssertHasError(testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false, true));
		}

		public void TestJR_LocalCostAmt_TooLargeOrSmall()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			var testCharge = testJob.Charges.AddNew();
			var amount = 2m;
			testCharge.JR_LocalCostAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false));
			AssertNoError("Precondition", testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false));

			amount = 9999999999999999999999999999m;
			testCharge.JR_LocalCostAmt = amount;
			Assert(amount == testCharge.JR_OSCostAmt);
			AssertHasError(testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false));
			AssertHasError(testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false));

			amount = 2m;
			testCharge.JR_LocalCostAmt = amount;
			AssertNoError("Precondition", testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false, true));
			AssertNoError("Precondition", testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false, true));

			amount = -9999999999999999999999999999m;
			testCharge.JR_LocalCostAmt = amount;
			Assert(amount == testCharge.JR_OSCostAmt);
			AssertHasError(testCharge.JR_OSCostAmtInfo, GetErrorMessageForValidMoneyCheck(false, false, true));
			AssertHasError(testCharge.JR_LocalCostAmtInfo, GetErrorMessageForValidMoneyCheck(true, false, true));
		}

		string GetErrorMessageForValidMoneyCheck(bool isLocal, bool isSell, bool isNegative = false)
		{
			return string.Format("The number {0}9,999,999,999,999,999,999,999,999,999 is too large, the value's range of " +
				(isLocal ? (isSell ? "Local Sell " : "Local Cost ") : (isSell ? "Overseas Sell " : "Overseas Cost ")) + "Amount " +
				"is between -922337203685477.5808 and 922337203685477.5807.", isNegative ? "-" : "");
		}

		public void TestJR_OSCostExRateChangedAfterChargePosted()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "DKK");

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = job.PK.ToString().Substring(1, JobHeaderSchema.JH_JobNum.MaxLength).ToUpper();

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_JH = job.PK;
			charge.JR_RX_NKCostCurrency = "DKK";
			charge.JR_OSCostAmt = 92.40M;
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(4.4084M);
			charge.JR_LocalCostAmt = 20.96M;

			foreach (ExchangeRate r in charge.InvoicingJob.ExchangeRates)
			{
				if (r.JF_BaseRate == 0)
				{
					r.JF_BaseRate = 1.0m;
				}
			}

			Factory.Save();

			var acrLine = Factory.Load<AccTransactionLines>(charge.JR_AL_APLine);
			acrLine.AL_ReverseDate = ZDateTime.Today;

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var line = TestObjectCreator.CreateInvoiceLine(invoice, currency, 4.408397M, 92.40M, 0, 0);
			line.AL_AC = charge.JR_AC;
			line.AL_JH = job.PK;
			line.AL_LineAmount = -20.96M;

			charge.JR_AL_APLine = line.PK;

			Factory.Save();

			charge.JR_OSCostExRate = 4.4090M;

			AssertContains("JR_OSCostExRate has been changed from 4.408397 to 4.4090 after the charge posted.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestJR_OSCostExRateChangedAfterChargePosted_JRJ()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
			AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				var shipment = TestObjectCreator.CreateShipment("test11111");
				var job = TestObjectCreator.CreateJob(shipment);
				TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 50m);
				Factory.Save();

				var charge = job.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_OSCostAmt != 0m);
				AssertNotNull("Precondition", charge);
				AssertEquals("Precondition", "REV", charge.APLine.AL_LineType);
				charge.JR_OSCostExRate = 100M;

				AssertContains("JR_OSCostExRate has been changed from 1 to 100 after the charge posted.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestChargeIsDeletedInDifferentFactory()
		{
			Charge charge1 = TestJob.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_CostAccount = ABIGAS.PK;
			charge1.JR_InvoiceType = "FIN";
			charge1.JR_OSCostAmt = 100m;

			Charge charge2 = TestJob.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_CostAccount = ABIGAS.PK;
			charge2.JR_InvoiceType = "FIN";
			charge2.JR_OSCostAmt = 200m;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var newCharge = newFactory.Load<Charge>(charge1.PK);
			newCharge.Delete();
			newFactory.Save();

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestCostAndRevenueExchangeRatesAreNotCreatedInContructor()
		{
			// create charge
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OH_CostAccount = ABIGAS.PK;
			charge.JR_OSCostAmt = 100m;

			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 100m;

			AssertNotNull(charge.CostExchangeRate);
			AssertNotNull(charge.RevenueExchangeRate);

			charge.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			AssertEquals(100m, charge.JR_OSSellAmt);
			AssertEquals(200m, charge.JR_LocalSellAmt);

			AssertEquals(100m, charge.JR_OSCostAmt);
			AssertEquals(200m, charge.JR_LocalCostAmt);

			Factory.Save();

			//charge is not getting wired up with exchange rates once loaded
			var factory2 = new BusinessObjectFactory();

			var loadedCharge = factory2.Load<TestingChargeWithCost>(charge.PK);
			var loadedJob = factory2.Load<Job>(TestJob.PK);
			Assert("No Exchange Rates in the factory2", !factory2.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());

			AssertNotNull("Revenue Exchange Rate wrapper should be created on the first access", loadedCharge.RevenueExchangeRate);
			AssertEquals("Both Exchange Rates are loaded in the factory2", 2, factory2.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length);

			AssertNotNull("Cost Exchange Rate wrapper should be created on the first access", loadedCharge.CostExchangeRate);
			AssertEquals("Two Exchange Rate in the factory2", 2, factory2.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length);

			var revRate = loadedJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.OrgType == ExchangeRateOrgTypeEnum.Debtor);
			revRate.JF_BaseRate = 2m;

			AssertEquals(100m, loadedCharge.JR_OSSellAmt);
			AssertEquals(50m, loadedCharge.JR_LocalSellAmt);

			var costRate = loadedJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.OrgType == ExchangeRateOrgTypeEnum.Creditor);
			costRate.JF_BaseRate = 2m;

			AssertEquals(100m, loadedCharge.JR_OSCostAmt);
			AssertEquals(50m, loadedCharge.JR_LocalCostAmt);

			//no exchange rates loaded in case of periodic invoicing - same behaviour as in standard case
			var factory3 = new BusinessObjectFactory();
			factory3.SetContext(BusinessContext.PeriodicInvoicePosting);
			try
			{
				var loadedCharge1 = factory3.Load<TestingChargeWithCost>(charge.PK);
				Assert(!factory3.Load<ExchangeRate>(new ZQuery() { FetchOnlyFromLocalCache = true }).Any());

				AssertNull(loadedCharge1.RevenueExchangeRate);
				AssertNull(loadedCharge1.CostExchangeRate);
			}
			finally
			{
				factory3.RemoveContext(BusinessContext.PeriodicInvoicePosting);
			}
		}

		public void TestChangeCostCurrencyForcesCostExchangeRateUpdate()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;
			charge.JR_RX_NKCostCurrency = "USD";

			AssertNotNull(charge.CostExchangeRate);

			Assert(charge.Calculations.Enabled_ForTestOnly);

			charge.JR_RX_NKCostCurrency = "AUD";

			AssertNull("Cost Exchange Rate is null for local currency", charge.CostExchangeRate);

			Assert(charge.Calculations.Enabled_ForTestOnly);

			charge.Calculations.SuspendCalculations();

			charge.JR_RX_NKCostCurrency = "EUR";

			AssertNotNull(charge.CostExchangeRate);
			AssertEquals("EUR", charge.CostExchangeRate.CurrencyCode);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor, charge.CostExchangeRate.OrgType);
			Assert(!charge.Calculations.Enabled_ForTestOnly);
		}

		public void TestIsCostForeignOrLocal()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;

			charge.JR_RX_NKCostCurrency = "";
			Assert(!charge.IsCostForeign);
			Assert(!charge.IsCostLocal);

			charge.JR_RX_NKCostCurrency = "AUD";
			Assert(!charge.IsCostForeign);
			Assert(charge.IsCostLocal);

			charge.JR_RX_NKCostCurrency = "USD";
			Assert(charge.IsCostForeign);
			Assert(!charge.IsCostLocal);
		}

		public void TestLocalSellAmtDiscrepancy()
		{
			// first create charge in DB
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 100m;

			AssertNotNull(charge.RevenueExchangeRate);

			var rate = TestJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.OrgType == ExchangeRateOrgTypeEnum.Debtor);
			rate.JF_BaseRate = 1m;
			rate.JF_CFXMinimum = 0.5m;
			rate.JF_CFXPercent = 5m;

			AssertEquals(0.95m, charge.JR_OSSellExRate);
			AssertEquals(100m, charge.JR_OSSellAmt);
			AssertEquals(105.26m, charge.JR_LocalSellAmt);

			Factory.Save();

			//loading charge
			var factory = new BusinessObjectFactory();
			factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			try
			{
				//load exchange rate in advance and modify
				var job = factory.Load<Job>(TestJob.PK);
				var loadedRate = factory.Load<ExchangeRate>(rate.PK);
				loadedRate.JF_CFXMinimum = 0m;
				loadedRate.JF_CFXPercent = 0m;
				loadedRate.JF_IsTransformed = true;

				var loadedCharge = factory.Load<TestingChargeWithCost>(charge.PK);
				AssertNotNull(loadedCharge.RevenueExchangeRate);

				Assert(loadedCharge.LocalSellAmtOverridenAndUserNeedToCheck);
				AssertEquals(105.26m, loadedCharge.LocalSellAmtPreviousValue);
			}
			finally
			{
				factory.RemoveContext(BusinessContext.InvoicingPlugInGUI);
			}
		}

		public void TestNoLocalSellAmtDiscrepancyFromRoundingAsNoRecelculationOnLoading_NotTransformedExRate()
		{
			AssertNoLocalSellAmtDiscrepancyFromRoundingAsNoRecelculationOnLoading(false);
		}

		public void TestNoLocalSellAmtDiscrepancyFromRoundingAsNoRecelculationOnLoading_TransformedExRate()
		{
			AssertNoLocalSellAmtDiscrepancyFromRoundingAsNoRecelculationOnLoading(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void AssertNoLocalSellAmtDiscrepancyFromRoundingAsNoRecelculationOnLoading(bool isExRateTransformed)
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			// first create charge in DB
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = "USD";

			AssertNotNull(charge.RevenueExchangeRate);
			var rate = TestJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.OrgType == ExchangeRateOrgTypeEnum.Debtor);
			rate.JF_BaseRate = 6.4083m;
			rate.JF_IsTransformed = isExRateTransformed;

			charge.JR_LocalSellAmt = 150m;

			AssertEquals(6.4083m, charge.JR_OSSellExRate);
			AssertEquals(23.41m, charge.JR_OSSellAmt);
			AssertEquals(150m, charge.JR_LocalSellAmt);

			Factory.Save();

			//loading charge
			var factory = new BusinessObjectFactory();
			factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			try
			{
				var loadedJob = factory.Load<Job>(TestJob.PK);
				Assert(loadedJob.ExchangeRates.Any());
				AssertEquals(1, loadedJob.Charges.Count);

				var loadedCharge = loadedJob.Charges[0];
				AssertNotNull(loadedCharge.RevenueExchangeRate);

				AssertEquals(6.4083m, loadedCharge.JR_OSSellExRate);
				AssertEquals(23.41m, loadedCharge.JR_OSSellAmt);
				AssertEquals(150.00m, loadedCharge.JR_LocalSellAmt);
				Assert("LocalSellAmtOverridenAndUserNeedToCheck", !loadedCharge.LocalSellAmtOverridenAndUserNeedToCheck);
			}
			finally
			{
				factory.RemoveContext(BusinessContext.InvoicingPlugInGUI);
			}
		}

		public void TestRedefaultCFXMinimum()
		{
			// set configuration with CFX minimum for debtor
			TestObjectCreator.Debtor.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", null, 10m);

			// first create charge in DB
			var charge = TestJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 100m;

			AssertNotNull(charge.RevenueExchangeRate);

			var rate = TestJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.OrgType == ExchangeRateOrgTypeEnum.Debtor);
			rate.JF_BaseRate = 1m;
			AssertEquals(10m, rate.JF_CFXMinimum);

			AssertEquals(100m, charge.JR_OSSellAmt);
			AssertEquals(110.00m, charge.JR_LocalSellAmt);
			AssertEquals(0.909091m, charge.JR_OSSellExRate);

			Factory.Save();

			//loading charge
			var factory = new BusinessObjectFactory();
			factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			try
			{
				//load exchange rate in advance and set CFX minimum to 0
				var job = factory.Load<Job>(TestJob.PK);
				var loadedRate = factory.Load<ExchangeRate>(rate.PK);
				loadedRate.JF_CFXMinimum = 0m;
				loadedRate.JF_IsTransformed = true;

				var loadedCharge = factory.Load<TestingChargeWithCost>(charge.PK);

				Assert(!loadedCharge.LocalSellAmtOverridenAndUserNeedToCheck);
				AssertEquals("Local sell amount has not been changed", 110.00m, loadedCharge.JR_LocalSellAmt);
				AssertNotNull(loadedCharge.RevenueExchangeRate);
				AssertEquals("The rate has CFX Minimum re-defaulted", 10m, loadedRate.JF_CFXMinimum);
			}
			finally
			{
				factory.RemoveContext(BusinessContext.InvoicingPlugInGUI);
			}
		}

		public void TestRevenueExchangeRateUpdatedWhenSellCurrencyChanged()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;
			charge.JR_RX_NKSellCurrency = "EUR";
			charge.JR_LocalSellAmt = 100m;

			AssertEquals(1m, charge.RevenueExchangeRate.Rate);
			AssertEquals(100m, charge.JR_OSSellAmt);

			var job = charge.InvoicingJob;

			var rate = job.ExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = "GBP";
			rate.JF_BaseRate = 0.5m;

			Assert(charge.Calculations.Enabled_ForTestOnly);

			charge.JR_RX_NKSellCurrency = "GBP";
			AssertEquals(50m, charge.JR_OSSellAmt);
			AssertEquals(100m, charge.JR_LocalSellAmt);

			Assert(charge.Calculations.Enabled_ForTestOnly);

			charge.Calculations.SuspendCalculations();

			charge.JR_RX_NKSellCurrency = "USD";
			AssertEquals(50m, charge.JR_OSSellAmt);
			AssertEquals("Should not be recalculated for calculations are disabled", 100m, charge.JR_LocalSellAmt);
			Assert(!charge.Calculations.Enabled_ForTestOnly);
		}

		public void TestOSSellExRateGetter()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;
			charge.JR_RX_NKSellCurrency = "EUR";
			AssertEquals(1m, charge.JR_OSSellExRate);
			AssertNotNull(charge.RevenueExchangeRate);
			var rate = charge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(e => e.JF_RX_NKRateCurrency == "EUR");
			rate.JF_BaseRate = 0.81m;

			//case 1 - returns base (row) value if revenue is posted
			var line = Factory.New<ARInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			charge.JR_AL_ARLine = line.PK;

			Assert(charge.IsRevenuePosted);

			AssertEquals(0.81m, charge.JR_OSSellExRate);

			charge.ClearRevenueLink();
			Assert(!charge.IsRevenuePosted);
			charge.UpdateRevenueExchangeRate();
			//case 2 Has PostingReceivableCharges context
			charge.SetContext(BusinessContext.PostingReceivableCharges);
			try
			{
				AssertEquals(0.81m, charge.JR_OSSellExRate);
			}
			finally
			{
				charge.RemoveContext(BusinessContext.PostingReceivableCharges);
			}

			// case 3: Normal case, cfx applied
			charge.JR_LocalSellAmt = 100m;
			rate.JF_CFXMinimum = 50m;
			AssertEquals(0.54m, charge.JR_OSSellExRate);

			// case 4: InvoiceTypeRequireZeroCFX
			charge.SetInvoiceTypeRequiresZeroCFX(true);
			rate.JF_BaseRate = 2m;
			AssertEquals("No CFX applied", 2m, charge.JR_OSSellExRate);
			charge.SetInvoiceTypeRequiresZeroCFX(false);
			// case 5: BIllInInvoiceCurrencySameAsSellCurrency
			charge.JR_RX_NKSellInvoiceCurrency = "EUR";
			Assert(charge.BIllInInvoiceCurrencySameAsSellCurrency);
			AssertEquals("No CFX applied", 2m, charge.JR_OSSellExRate);
			// case 6: !IsCFXPercentApplied
			charge.JR_RX_NKSellInvoiceCurrency = "";
			charge.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;
			AssertEquals("No CFX applied", 2m, charge.JR_OSSellExRate);
			// case 7: Local Currency
			charge.JR_RX_NKSellCurrency = "AUD";
			AssertEquals("Exchange rate always 1 for local currency", 1m, charge.JR_OSSellExRate);
		}

		public void TestSellExchangeRateType()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;
			charge.JR_RX_NKSellCurrency = "EUR";

			Assert(!charge.BIllInInvoiceCurrencySameAsSellCurrency);
			Assert(!charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertEquals(ExchangeRateType.Sell, charge.SellExchangeRateType);

			charge.JR_RX_NKSellInvoiceCurrency = "EUR";

			Assert(charge.BIllInInvoiceCurrencySameAsSellCurrency);
			Assert(!charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertEquals(ExchangeRateType.Buy, charge.SellExchangeRateType);

			charge.JR_RX_NKSellCurrency = "AUD";
			charge.JR_RX_NKSellInvoiceCurrency = "EUR";

			Assert(!charge.BIllInInvoiceCurrencySameAsSellCurrency);
			Assert(charge.BillInInvoiceCurrencyWithLocalSellCurrency);

			AssertEquals(ExchangeRateType.Buy, charge.SellExchangeRateType);
		}

		public void TestRevenueExchangeRateChanged()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;
			charge.JR_RX_NKSellCurrency = "EUR";

			charge.JR_OSSellAmt = 100m;

			var row = ((INeedRow)charge).Row;
			AssertEquals(1m, row[nameof(charge.JR_OSSellExRate)]);

			var job = charge.InvoicingJob;

			var rate = job.ExchangeRates.AddNew();
			rate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			rate.JF_RX_NKRateCurrency = "GBP";
			rate.JF_BaseRate = 2m;

			charge.JR_RX_NKSellCurrency = "GBP";

			AssertEquals(2m, row[nameof(charge.JR_OSSellExRate)]);
			AssertEquals(2m, charge.JR_OSSellExRate);
			AssertEquals(200m, charge.JR_OSSellAmt);
			AssertEquals(100m, charge.JR_LocalSellAmt);
			AssertEquals(2m, charge.SellRateWithoutCFX);

			rate.JF_CFXPercent = 10m;

			AssertEquals(200m, charge.JR_OSSellAmt);
			AssertEquals(111.11m, charge.JR_LocalSellAmt);
			AssertEquals(10m, charge.JR_LineCFX);
			AssertEquals(2m, charge.SellRateWithoutCFX);
		}

		public void TestSellInvoiceRateWithoutCFXIsGettingUpdatedByUpdateLineCFX()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var orgFactory = new BusinessObjectFactory();
			var debtor = orgFactory.NewWithValidTestData<OrgHeader>();
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.ABIGAS, 0, TestObjectCreator.Agent, 0);
			job.PlugInData = shipment;
			var rate = TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 2m);
			var charge = job.Charges.AddNew();
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("SellInvoiceRateWithoutCFX", 2m, charge.SellInvoiceRateWithoutCFX_ForTestOnly);

			charge.SellInvoiceExchangeRate.SetBaseRate(3m);
			AssertEquals("SellInvoiceRateWithoutCFX should be changed as 3", 3m, charge.SellInvoiceRateWithoutCFX_ForTestOnly);
		}

		public void TestAL_LineAmountAndAL_OSAmountIsChangedOnPostingWhenWithDifferentExRate()
		{
			TestObjectCreator.SetCurrentCompanyReciprocal(true);
			TestObjectCreator.ABIGAS.CompanyData.AccCFXConfigurations.SetUplifts("SHP", "ALL", "ALL", 2m, 0.2m);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			PostingExRateRegistryAR.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.ABIGAS, 0, TestObjectCreator.Agent, 0);
			job.PlugInData = shipment;
			var rate = TestObjectCreator.SetExchangeRate(job, TestObjectCreator.USD, 0.6m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Test", TestObjectCreator.AUD, 0m, null,
			TestObjectCreator.USD, 1200m, TestObjectCreator.ABIGAS);

			AssertNull(charge.CFXLine);
			var cfxHeader = Factory.New<JCJournalHeader>();
			var cfxLine = cfxHeader.Lines.AddNew();
			charge.JR_AL_CFXLine = cfxLine.PK;

			Factory.SetContext(BusinessContext.PostingReceivableCharges);
			AssertEquals("AL LineAmount should be zero: ", 0m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should be zero: ", 0m, charge.CFXLine.AL_OSAmount);
			AssertEquals("CFX Amount should be zero ", 0m, charge.JR_CFXAmt);

			charge.UpdateLineCFX();
			AssertEquals("AL LineAmount should be changed: ", -14.40m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should be changed: ", -14.40m, charge.CFXLine.AL_OSAmount);
			AssertEquals("CFX Amount should be changed ", 14.40m, charge.JR_CFXAmt);

			TestJob.UpdateBaseExchangeRate(TestObjectCreator.USD.RX_Code, ExchangeRateValidLedgerEnum.AR, 0.8m, new Charge[] { charge });
			AssertEquals("AL LineAmount should be changed ", -19.2m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should be changed ", -19.2m, charge.CFXLine.AL_OSAmount);
			AssertEquals("CFX Amount should be changed ", 19.2m, charge.JR_CFXAmt);

			charge.UpdateLineCFX();
			AssertEquals("AL LineAmount should be changed ", -19.2m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should be changed ", -19.2m, charge.CFXLine.AL_OSAmount);
			AssertEquals("CFX Amount should be changed ", 19.2m, charge.JR_CFXAmt);

			TestJob.UpdateBaseExchangeRate(TestObjectCreator.USD.RX_Code, ExchangeRateValidLedgerEnum.AR, 0.9m, new Charge[] { charge });
			AssertEquals("AL LineAmount should be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("CFX Amount should be changed ", 21.60m, charge.JR_CFXAmt);

			charge.UpdateLineCFX();
			AssertEquals("AL LineAmount should be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("CFX Amount should be changed ", 21.60m, charge.JR_CFXAmt);

			Factory.RemoveContext(BusinessContext.PostingReceivableCharges);
			TestJob.UpdateBaseExchangeRate(TestObjectCreator.USD.RX_Code, ExchangeRateValidLedgerEnum.AR, 0.8m, new Charge[] { charge });
			AssertEquals("AL LineAmount should not be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should not be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("CFX Amount should not be changed ", 21.60m, charge.JR_CFXAmt);

			charge.UpdateLineCFX();
			AssertEquals("AL LineAmount should not be changed ", -21.60m, charge.CFXLine.AL_LineAmount);
			AssertEquals("AL OSAmount should not be changed ", -21.60m, charge.CFXLine.AL_OSAmount);
			AssertEquals("CFX Amount should not be changed ", 21.60m, charge.JR_CFXAmt);
		}

		public void TestCostExchangeRateChanged()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			charge.JR_JH = TestJob.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 100m;

			var row = ((INeedRow)charge).Row;
			AssertEquals(1m, row[nameof(charge.JR_OSCostExRate)]);

			var job = charge.InvoicingJob;

			var rate = job.ExchangeRates.AddNew();
			rate.OrgType = ExchangeRateOrgTypeEnum.Creditor;
			rate.JF_RX_NKRateCurrency = "GBP";
			rate.JF_BaseRate = 2m;

			charge.JR_RX_NKCostCurrency = "GBP";

			AssertEquals(2m, row[nameof(charge.JR_OSCostExRate)]);
			AssertEquals(2m, charge.JR_OSCostExRate);
			AssertEquals(200m, charge.JR_OSCostAmt);
			AssertEquals(100m, charge.JR_LocalCostAmt);

			rate.JF_BaseRate = 3m;

			AssertEquals(3m, row[nameof(charge.JR_OSCostExRate)]);
			AssertEquals(3m, charge.JR_OSCostExRate);
			AssertEquals(200m, charge.JR_OSCostAmt);
			AssertEquals(66.67m, charge.JR_LocalCostAmt);

			charge.JR_E6 = ZGuid.NewZGuid();
			Assert(charge.JR_IsApportioned);
			rate.JF_BaseRate = 2m;

			AssertEquals("No update of cost ex rate for apportioned charge", 3m, row[nameof(charge.JR_OSCostExRate)]);
			AssertEquals(3m, charge.JR_OSCostExRate);
			AssertEquals(200m, charge.JR_OSCostAmt);
			AssertEquals(66.67m, charge.JR_LocalCostAmt);

			charge.JR_E6 = ZGuid.Empty;
			Assert(!charge.JR_IsApportioned);

			var line = Factory.New<APInvoiceLine>();
			line.AL_LineType = TransactionLineTypes.Cost;
			charge.JR_AL_APLine = line.PK;

			Assert(charge.IsCostPosted);

			rate.JF_BaseRate = 1m;

			AssertEquals("No update of cost ex rate for posted charge", 3m, row[nameof(charge.JR_OSCostExRate)]);
			AssertEquals(3m, charge.JR_OSCostExRate);
			AssertEquals(200m, charge.JR_OSCostAmt);
			AssertEquals(66.67m, charge.JR_LocalCostAmt);
		}

		public void TestInvoiceTypeRequiresZeroCFX()
		{
			var charge = Factory.New<TestingChargeWithCost>();

			var expectedZeroCFXRequires = new Dictionary<string, bool>
			{
				[InvoiceTypesList.Codes.ForeignCurrencyInvoice] = true,
				[InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching] = true,
				[InvoiceTypesList.Codes.DisbursementInForeignCurrency] = true,
				[InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching] = true,
				[InvoiceTypesList.Codes.FreightInvoice] = true,
				[InvoiceTypesList.Codes.FreightInvoice_Batching] = true,
				[InvoiceTypesList.Codes.SelfBillingInvoice] = true,
				[InvoiceTypesList.Codes.SelfBillingInvoice_Batching] = true,
				[InvoiceTypesList.Codes.SelfBillingInvoice_Batching] = true,
				[InvoiceTypesList.Codes.FinalInvoice] = false,
				[InvoiceTypesList.Codes.FinalInvoice_Batching] = false,
				[InvoiceTypesList.Codes.DestinationChargesInvoice] = false,
				[InvoiceTypesList.Codes.DestinationChargesInvoice_Batching] = false,
				[InvoiceTypesList.Codes.InvoicePerTaxCode] = false,
				[InvoiceTypesList.Codes.InvoicePerTaxCode_Batching] = false,
				[InvoiceTypesList.Codes.DisbursementInvoice] = false,
				[InvoiceTypesList.Codes.DisbursementInvoice_Batching] = false,
				[InvoiceTypesList.Codes.DoNotPost] = false,
			};

			foreach (var kvp in expectedZeroCFXRequires)
			{
				charge.JR_InvoiceType = kvp.Key;
				AssertEquals(kvp.Value, charge.BaseInvoiceTypeRequiresZeroCFX);
			}

			var invoiceTypeList = (new InvoiceTypesList()).GetAllCodes();

			AssertContainsExactElementsInAnyOrder($"All existing invoice type codes must be listed in the {nameof(expectedZeroCFXRequires)} dictionary", (new InvoiceTypesList()).GetAllCodes(), expectedZeroCFXRequires.Keys);
		}

		public void TestInvoiceTypeUpdateSuspender()
		{
			var charge = Factory.New<TestingChargeWithCost>();
			AssertNotNull(charge.InvoiceTypeUpdateSuspender);
		}

		#region Performance Testing

		public void TestJR_OH_CostAccountDoesNotUpdateCostAccountOfOtherChargesWithSameInvoiceDetailsIfNewValueIsSameAsCurrentValue()
		{
			var job = TestObjectCreator.CreateJob("S0001", TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);

			var charge1 = (ChargeWithCost)job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_APInvoiceNum = "123";

			var charge2 = (ChargeWithCost)job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			var invoiceDate = ZDateTime.Today;
			charge2.JR_APInvoiceDate = invoiceDate;
			charge2.JR_APInvoiceNum = "123";

			Factory.Save();

			AssertEquals(ZDateTime.Empty, charge1.JR_APInvoiceDate);
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			AssertEquals("JR_APInvoiceDate should be updated for charge1", invoiceDate, charge1.JR_APInvoiceDate);

			charge1.JR_APInvoiceDate = ZDateTime.Empty;
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			AssertEquals(@"JR_APInvoiceDate should not be updated for charge1 because current JR_OH_CostAccount
					is same as new value", ZDateTime.Empty, charge1.JR_APInvoiceDate);

			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
			AssertEquals(ZDateTime.Empty, charge1.JR_APInvoiceDate);

			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor2.PK;
			AssertEquals("JR_APInvoiceDate should be updated for charge1 because now we are setting a different value",
				invoiceDate, charge1.JR_APInvoiceDate);
		}

		public void TestJR_ChequeNoDoesNotUpdateChequeBookCurrentNoIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateChargeWithCost();
			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			Factory.Save();

			AssertEquals(1m, TestObjectCreator.AUDChequeBook.AK_CurrentNo);
			charge.JR_ChequeNo = "10";
			AssertEquals("AK_CurrentNo should be updated", 11m, TestObjectCreator.AUDChequeBook.AK_CurrentNo);

			TestObjectCreator.AUDChequeBook.AK_CurrentNo = 1m;
			Factory.Save();

			charge.JR_ChequeNo = "10";
			AssertEquals("AK_CurrentNo should not be updated because current JR_ChequeNo is same as new value",
				1m, TestObjectCreator.AUDChequeBook.AK_CurrentNo);

			charge.JR_ChequeNo = "11";
			AssertEquals("AK_CurrentNo should be updated because now we are setting a different value",
				12m, TestObjectCreator.AUDChequeBook.AK_CurrentNo);
		}

		public void TestJR_AKDoesNotUpdateJR_ChequeNoIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateChargeWithCost();

			AssertEquals(ZString.Empty, charge.JR_ChequeNo);
			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			AssertEquals("JR_ChequeNo should be updated", "1", charge.JR_ChequeNo);

			charge.JR_ChequeNo = ZString.Empty;
			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			AssertEquals("JR_ChequeNo should not be updated because current JR_AK is same as new value",
				ZString.Empty, charge.JR_ChequeNo);

			charge.JR_AK = TestObjectCreator.AUDChequeBook2.PK;
			AssertEquals("JR_ChequeNo should be updated because now we are setting a different value",
				"1", charge.JR_ChequeNo);
		}

		public void TestJR_PaymentTypeDoesNotSetJR_AKToEmptyIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateChargeWithCost();

			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("JR_AK should be empty", ZGuid.Empty, charge.JR_AK);

			charge.JR_AK = TestObjectCreator.AUDChequeBook.PK;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("JR_AK should not be empty because current JR_PaymentType is same as new value",
				TestObjectCreator.AUDChequeBook.PK, charge.JR_AK);

			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("JR_AK should be empty because now we are setting a different value",
				ZGuid.Empty, charge.JR_AK);
		}

		public void TestJR_APInvoiceDateDoesNotUpdateJR_PaymentDateIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateChargeWithCost();
			charge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;

			var invoiceDate = ZDateTime.Today;

			AssertEquals(ZDateTime.Empty, charge.JR_PaymentDate);
			charge.JR_APInvoiceDate = invoiceDate;
			AssertEquals("JR_PaymentDate should be updated", invoiceDate, charge.JR_PaymentDate);

			charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_APInvoiceDate = invoiceDate;
			AssertEquals("JR_PaymentDate should not be updated because current JR_APInvoiceDate is same as new value",
				ZDateTime.Empty, charge.JR_PaymentDate);

			invoiceDate = ZDateTime.Today.AddDays(1);

			charge.JR_APInvoiceDate = invoiceDate;
			AssertEquals("JR_PaymentDate should be updated because now we are setting a different value",
				invoiceDate, charge.JR_PaymentDate);
		}

		public void TestJR_APInvoiceNumDoesNotupdateJR_PaymentDateIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateChargeWithCost();
			charge.JR_ChargeType = Core.Constants.ChargeType.Revenue;

			var paymentDate = ZDateTime.Today;

			charge.JR_PaymentDate = paymentDate;
			charge.JR_APInvoiceNum = "123";
			AssertEquals("JR_PaymentDate should be updated", ZDateTime.Empty, charge.JR_PaymentDate);

			charge.JR_PaymentDate = paymentDate;
			charge.JR_APInvoiceNum = "123";
			AssertEquals("JR_PaymentDate should not be updated because current JR_APInvoiceNum is same as new value",
				paymentDate, charge.JR_PaymentDate);

			charge.JR_APInvoiceNum = "125";
			AssertEquals("JR_PaymentDate should be updated because now we are setting a different value",
				ZDateTime.Empty, charge.JR_PaymentDate);
		}

		public void TestJR_ABDoesNotupdateJR_PaymentDateIfNewValueIsSameAsCurrentValue()
		{
			var charge = CreateChargeWithCost();
			charge.JR_ChargeType = Core.Constants.ChargeType.Revenue;

			var paymentDate = ZDateTime.Today;

			charge.JR_PaymentDate = paymentDate;
			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals("JR_PaymentDate should be updated", ZDateTime.Empty, charge.JR_PaymentDate);

			charge.JR_PaymentDate = paymentDate;
			charge.JR_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals("JR_PaymentDate should not be updated because current JR_AB is same as new value",
				paymentDate, charge.JR_PaymentDate);

			charge.JR_AB = TestObjectCreator.AUDBankAccount2.PK;
			AssertEquals("JR_PaymentDate should be updated because now we are setting a different value",
				ZDateTime.Empty, charge.JR_PaymentDate);
		}

		public void TestCustomsChargeCodePKsIsCachedOnFactory()
		{
			var shipment = TestObjectCreator.CreateShipment("S1234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge1 = (ChargeWithCost)job.Charges.AddNew();
			var charge2 = (ChargeWithCost)job.Charges.AddNew();
			var charge3 = (ChargeWithCost)job.Charges.AddNew();

			charge3.JR_GB = Factory.NewWithValidTestData<GlbBranch>().PK;

			using (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				Guid.NewGuid()))
			{
				AssertSame(charge1.CustomsChargeCodePKs, charge1.CustomsChargeCodePKs);
				AssertSame(charge1.CustomsChargeCodePKs, charge2.CustomsChargeCodePKs);
				AssertNotSame(charge1.CustomsChargeCodePKs, charge3.CustomsChargeCodePKs);
			}
		}

		ChargeWithCost CreateChargeWithCost()
		{
			var shipment = TestObjectCreator.CreateShipment("S1234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = (ChargeWithCost)job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();
			return charge;
		}

		#endregion

		#region Implementation

		void SetCurrentCountryToCode(string code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.StartsWith, code)).Code;
		}

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("Work Item W00024746", true);
		}

		#region Test objects

		#region Exchange rate
		protected ExchangeRate fUSDRate;
		protected ExchangeRate USDRate
		{
			get
			{
				if (fUSDRate == null)
				{
					fUSDRate = TestObjectCreator.CreateExchangeRate(TestJob, TestObjectCreator.USD, 0.6m);
				}

				return fUSDRate;
			}
		}

		#endregion

		#region ChargeWithCost

		protected ChargeWithCost ACharge
		{
			get { return (ChargeWithCost)ABaseCharge; }
		}

		#endregion

		#region Transaction Lines

		APInvoiceLine fCostLine;
		protected APInvoiceLine CostLine
		{
			get
			{
				if (fCostLine == null)
				{
					fCostLine = TestObjectCreator.CreateAPInvoiceLine(null, TestJob, TestObjectCreator.MRG100, TestObjectCreator.USD, 0.6m, "test", 100m);
				}

				return fCostLine;
			}
		}

		ARInvoiceLine fRevenueLine;
		protected ARInvoiceLine RevenueLine
		{
			get
			{
				if (fRevenueLine == null)
				{
					fRevenueLine = TestObjectCreator.CreateARInvoiceLine(null, TestJob, TestObjectCreator.MRG100, TestObjectCreator.USD, 0.6m, "test", 100m);
				}

				return fRevenueLine;
			}
		}

		#endregion

		#endregion

		#region Set up and overrides

		protected string TestString = "test";

		protected bool EventFired;
		protected override void SetUp()
		{
			base.SetUp();
			EventFired = false;

			TestObjectCreator.TestOrganisation.CompanyData.OB_RX_NKARDDefltCurrency = ZString.Empty;
			TestObjectCreator.TestOrganisation.CompanyData.OB_RX_NKAPDefltCurrency = ZString.Empty;
		}

		protected override void AssertReadOnlyOnCostRelatedProperties(bool expectedReadOnly)
		{
			base.AssertReadOnlyOnCostRelatedProperties(expectedReadOnly);

			AssertEquals("JR_AC", expectedReadOnly, TestCharge.JR_ACInfo.ReadOnly);
			AssertEquals("JR_Desc", expectedReadOnly, TestCharge.JR_DescInfo.ReadOnly);
			AssertEquals("JR_GB", expectedReadOnly, TestCharge.JR_GBInfo.ReadOnly);
			AssertEquals("JR_GE", expectedReadOnly, TestCharge.JR_GEInfo.ReadOnly);
			AssertEquals("JR_APInvoiceDate", expectedReadOnly, TestCharge.JR_APInvoiceDateInfo.ReadOnly);
			AssertEquals("JR_APDocumentReceivedDate", expectedReadOnly, TestCharge.JR_APDocumentReceivedDateInfo.ReadOnly);
			AssertEquals("JR_APInvoiceNum", expectedReadOnly, TestCharge.JR_APInvoiceNumInfo.ReadOnly);
			AssertEquals("JR_CostReference", expectedReadOnly, TestCharge.JR_CostReferenceInfo.ReadOnly);
			AssertEquals("JR_LocalCostAmt", expectedReadOnly, TestCharge.JR_LocalCostAmtInfo.ReadOnly);
			AssertEquals("JR_OH_CostAccount", expectedReadOnly, TestCharge.JR_OH_CostAccountInfo.ReadOnly);
			AssertEquals("JR_OSCostAmt", expectedReadOnly, TestCharge.JR_OSCostAmtInfo.ReadOnly);
			AssertEquals("JR_RX_NKCostCurrency", expectedReadOnly, TestCharge.JR_RX_NKCostCurrencyInfo.ReadOnly);

			AssertEquals("JR_PaymentDate", expectedReadOnly, TestCharge.JR_PaymentDateInfo.ReadOnly);
			AssertEquals("JR_PaymentType", expectedReadOnly, TestCharge.JR_PaymentTypeInfo.ReadOnly);
			AssertEquals("JR_AB", expectedReadOnly, TestCharge.JR_ABInfo.ReadOnly);
			AssertEquals("JR_AK", expectedReadOnly, TestCharge.JR_AKInfo.ReadOnly);
			AssertEquals("JR_ChequeNo", expectedReadOnly, TestCharge.JR_ChequeNoInfo.ReadOnly);
		}

		protected override void AssertReadOnlyOnRevenueRelatedProperties(bool expectedReadOnly)
		{
			base.AssertReadOnlyOnRevenueRelatedProperties(expectedReadOnly);

			AssertEquals("JR_AC", expectedReadOnly, TestCharge.JR_ACInfo.ReadOnly);
			AssertEquals("JR_Desc", expectedReadOnly, TestCharge.JR_DescInfo.ReadOnly);
			AssertEquals("JR_GB", expectedReadOnly, TestCharge.JR_GBInfo.ReadOnly);
			AssertEquals("JR_GE", expectedReadOnly, TestCharge.JR_GEInfo.ReadOnly);
			AssertEquals("JR_AT_SellGSTRate", expectedReadOnly, TestCharge.JR_AT_SellGSTRateInfo.ReadOnly);
			AssertEquals("JR_AW_SellWHTRate", expectedReadOnly, TestCharge.JR_AW_SellWHTRateInfo.ReadOnly);
			AssertEquals("JR_OSSellAmt", expectedReadOnly, TestCharge.JR_OSSellAmtInfo.ReadOnly);
			AssertEquals("JR_LocalSellAmt", expectedReadOnly, TestCharge.JR_LocalSellAmtInfo.ReadOnly);
			AssertEquals("JR_RX_NKSellCurrency", expectedReadOnly, TestCharge.JR_RX_NKSellCurrencyInfo.ReadOnly);
		}

		#endregion

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			localClient?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", localClientCFX);
			agent?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", agentCFX);
			return job;
		}

		protected ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			return exchangeRate;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal osCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal osSellAmt, OrgHeader debtor)
		{
			var charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			if (creditor != null)
			{
				charge.JR_OH_CostAccount = creditor.PK;
			}

			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_OSCostAmt = osCostAmt;

			if (debtor != null)
			{
				charge.JR_OH_SellAccount = debtor.PK;
			}
			else
			{
				charge.JR_OH_SellAccount = ZGuid.Empty;
			}
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = osSellAmt;
			return charge;
		}

		#region ABIGAS

		protected OrgHeader fABIGAS;
		protected OrgHeader ABIGAS
		{
			get
			{
				if (fABIGAS == null)
				{
					fABIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				}
				return fABIGAS;
			}
		}

		#endregion

		#region AALSHI

		protected OrgHeader fAALSHI;
		protected OrgHeader AALSHI
		{
			get
			{
				if (fAALSHI == null)
				{
					fAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
					fAALSHI.CompanyData.SetAPTaxApplicable(true);
					fAALSHI.MiscServ.OM_APWHTApplicable = true;
					Factory.Save();
				}
				return fAALSHI;
			}
		}

		#endregion

		#region ZECTRA

		protected OrgHeader fZECTRA;
		protected OrgHeader ZECTRA
		{
			get
			{
				if (fZECTRA == null)
				{
					fZECTRA = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ZECTRA");
				}
				return fZECTRA;
			}
		}

		#endregion

		protected AccChargeCode MRG100
		{
			get { return TestObjectCreator.MRG100; }
		}

		ZDBOnlyQuery CompanyDataQuery(SchemaColumn companyDataField)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(companyDataField, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		class TestingChargeWithCost : ChargeWithCost
		{
			public TestingChargeWithCost(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected internal override ZDecimal CalculateCFXAmt()
			{
				throw new NotImplementedException();
			}

			public void SetBaseSellExchangeRate(ZDecimal value)
			{
				base.JR_OSSellExRate = value;
			}

			public void SetInvoiceTypeRequiresZeroCFX(bool value)
			{
				invoiceTypeRequireZeroCFX = value;
			}

			protected override bool InvoiceTypeRequiresZeroCFX => invoiceTypeRequireZeroCFX;

			bool invoiceTypeRequireZeroCFX;

			public bool BaseInvoiceTypeRequiresZeroCFX => base.InvoiceTypeRequiresZeroCFX;
		}

		#endregion

		#region Concurrency

		public void TestConcurrencyEditPostedChargeCost()
		{
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				Factory.RefreshEnabled = false;
				TestObjectCreator creator = new TestObjectCreator(Factory);

				Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
				ChargeWithCost chargeWithCost = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, AALSHI);
				Factory.Save();

				AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
				AssertNull("Cost Doesn't Exist", chargeWithCost.Cost);

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				TestObjectCreator utils = new TestObjectCreator(newFactory);

				APInvoice aPInvoice = utils.CreateAPInvoice<APInvoice>("I001", TestObjectCreator.AUD, 1M, 100M, 0, 0, 100M, 0, 0);

				ZQuery query = new ZQuery(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job.PK);
				query.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, "ACR");
				WIPAccrualCollection accruals = new WIPAccrualCollection(newFactory, query);
				accruals.Load();

				APInvoiceLine line = (APInvoiceLine)aPInvoice.Lines.AddNew();
				aPInvoice.ImportAccrualsIntoInvoice(accruals.ToArray<BaseWIPAccrual>(), line);

				aPInvoice.SubmittedFromInvoicingForm = true;
				newFactory.Save();

				ErrorReporter.Clear();
				try
				{
					chargeWithCost.JR_OSCostAmt = 150M;
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));

				chargeWithCost.Reload();
				AssertNull("Accrual Doesn't Exist", chargeWithCost.Accrual);
				AssertNotNull("Cost Exists", chargeWithCost.Cost);
			}
		}

		public void TestConcurrencyEditPostedChargeRevenue()
		{
			Factory.RefreshEnabled = false;
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, ABIGAS);
			Factory.Save();

			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertNull("Revenue Doesn't Exist", chargeWithCost.Revenue);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			Job jobNew = newFactory.Load<Job>(job.PK);

			InvoicingPostManager poster = new InvoicingPostManager(jobNew);
			poster.CreateTransactions(JobInvoicingPostingOption.All);

			newFactory.Save();

			ErrorReporter.Clear();
			try
			{
				chargeWithCost.JR_OSSellAmt = 150M;
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));

			chargeWithCost.Reload();
			AssertNotNull("Revenue Exists", chargeWithCost.Revenue);
			AssertNull("WIP Doesn't Exist", chargeWithCost.WIP);
		}

		public void TestConcurrencyDeletePostedChargeCost()
		{
			Factory.RefreshEnabled = false;
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, AALSHI);
			Factory.Save();

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertNull("Cost Doesn't Exist", chargeWithCost.Cost);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			TestObjectCreator utils = new TestObjectCreator(newFactory);

			APInvoice aPInvoice = utils.CreateAPInvoice<APInvoice>("I001", TestObjectCreator.AUD, 1M, 100M, 0, 0, 100M, 0, 0);

			ZQuery query = new ZQuery(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, job.PK);
			query.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.Equal, "ACR");
			WIPAccrualCollection accruals = new WIPAccrualCollection(newFactory, query);
			accruals.Load();
			APInvoiceLine line = (APInvoiceLine)aPInvoice.Lines.AddNew();
			aPInvoice.ImportAccrualsIntoInvoice(accruals.ToArray<BaseWIPAccrual>(), line);

			aPInvoice.SubmittedFromInvoicingForm = true;
			newFactory.Save();

			var palceholder1 = chargeWithCost.CanDelete;
			var palceholder2 = chargeWithCost.ReasonForNotAbleToDelete;
			AssertNotNull("Charge should not be reloaded. Accrual Exists", chargeWithCost.Accrual);
			AssertNull("Charge should not be reloaded. Cost Doesn't Exist", chargeWithCost.Cost);

			job.Charges.RemoveAndDelete(chargeWithCost);
			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: JobCharge
PK: {chargeWithCost.PK}
RowState: Deleted
";
			Assert(string.Format(
@"Message 
{0} 
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		public void TestConcurrencyDeletePostedChargeRevenue()
		{
			Factory.RefreshEnabled = false;
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, ABIGAS);

			Factory.Save();

			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertNull("Revenue Doesn't Exist", chargeWithCost.Revenue);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			Job jobInANewFactory = newFactory.Load<Job>(job.PK);

			InvoicingPostManager poster = new InvoicingPostManager(jobInANewFactory);
			poster.CreateTransactions(JobInvoicingPostingOption.All);

			newFactory.Save();

			var palceholder1 = chargeWithCost.CanDelete;
			var palceholder2 = chargeWithCost.ReasonForNotAbleToDelete;
			AssertNotNull("Charge should not be reloaded. WIP Exists", chargeWithCost.WIP);
			AssertNull("Charge should not be reloaded. Revenue Doesn't Exist", chargeWithCost.Revenue);

			job.Charges.RemoveAndDelete(chargeWithCost);
			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => Factory.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: JobCharge
PK: {chargeWithCost.PK}
RowState: Deleted
";
			Assert(string.Format(
@"Message 
{0} 
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		public void TestConcurrencyReversePostedChargeCost()
		{
			Factory.RefreshEnabled = false;
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, AALSHI);
			Factory.Save();

			ZGuid accrualPK = chargeWithCost.Accrual.PK;

			AssertNotNull("Accrual Exists", chargeWithCost.Accrual);
			AssertNull("Cost Doesn't Exist", chargeWithCost.Cost);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			Job jobInANewFactory = newFactory.Load<Job>(job.PK);
			JobInvoicingReverser reverser = new JobInvoicingReverser(jobInANewFactory);
			reverser.ReversingFactory.RefreshEnabled = false;
			ChargeWithCost chargeWithCostInANewFactory = reverser.ReversingFactory.Load<Charge>(chargeWithCost.PK);
			Assert("Cost should not be posted in a new session", !chargeWithCostInANewFactory.IsCostPosted);

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			chargeWithCost.CreateCostTransactionLine(invoice, ZDateTime.Now);
			Assert("Cost should be posted", chargeWithCost.IsCostPosted);

			Factory.Save();

			reverser.ReverseAllInvoices("Test", "TST");

			ErrorReporter.Clear();
			try
			{
				reverser.ReversingFactory.Save(); //NewFactory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));

			chargeWithCostInANewFactory.Reload();
			AssertNull("Accrual Doesn't Exist", chargeWithCost.Accrual);
			AssertNotNull("Cost Exists", chargeWithCost.Cost);
			Assert(chargeWithCost.IsCostPosted);
		}

		public void TestConcurrencyReversePostedChargeRevenue()
		{
			Factory.RefreshEnabled = false;
			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job job = CreateJob("S001", AALSHI, true, 10M, ABIGAS, true, 10M);
			ChargeWithCost chargeWithCost = CreateCharge(job, creator.CC1, "Cost Transaction Test", creator.AUD, 100M, ZECTRA, creator.AUD, 120M, ABIGAS);
			Factory.Save();

			AssertNotNull("WIP Exists", chargeWithCost.WIP);
			AssertNull("Revenue Doesn't Exist", chargeWithCost.Revenue);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			Job jobInANewFactory = newFactory.Load<Job>(job.PK);
			JobInvoicingReverser reverser = new JobInvoicingReverser(jobInANewFactory);
			reverser.ReversingFactory.RefreshEnabled = false;
			ChargeWithCost chargeWithCostInANewFactory = reverser.ReversingFactory.Load<Charge>(chargeWithCost.PK);
			Assert("Cost should not be posted in a new session", !chargeWithCostInANewFactory.IsRevenuePosted);

			InvoicingPostManager poster = new InvoicingPostManager(job);
			poster.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("Cost should be posted in", chargeWithCost.IsRevenuePosted);

			Factory.Save();

			reverser.ReverseAllInvoices("Test", "tst");

			ErrorReporter.Clear();
			try
			{
				reverser.ReversingFactory.Save(); //NewFactory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			Assert("Must not be mergeable.", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The system cannot automatically merge your changes because there are conflicts with critical fields."));

			chargeWithCostInANewFactory.Reload();
			AssertNotNull("Revenue Exists", chargeWithCost.Revenue);
			AssertNull("WIP Doesn't Exist", chargeWithCost.WIP);
		}

		public void TestGSTApplicability()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.SetARTaxApplicable(true);
			org1.CompanyData.OB_ARWHTApplicable = false;
			org1.OH_FullName = "Test Debtor";
			org1.CompanyData.InvoiceRollupOrGroups[0].PG_InvoicePostingStyle = "DFI";

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			org2.CompanyData.OB_APWHTApplicable = false;
			org2.OH_FullName = "Test Creditor";

			OrgAddress localChargesAddr = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress agentCollectAddr = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "123";
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localChargesAddr.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_InvoiceType = "FIN";
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			JobCollection jobs = new JobCollection(Factory);
			jobs.Add(testJob);

			Assert("Sell TestObjectCreator.GST1 rate should not be actual", !testCharge.IsSellGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org1.CompanyData.SetARTaxApplicable(false);
			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 rate should not be actual", !testCharge.IsSellGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 rate should be actual", testCharge.IsSellGSTRateActual);

			testCharge.JR_OH_CostAccount = org2.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_InvoiceType = "FIN";
			testCharge.JR_OSCostAmt = 100m;

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			Assert("Cost TestObjectCreator.GST1 rate should not be actual", !testCharge.IsCostGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org2.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			Assert("Cost TestObjectCreator.GST1 rate should not be actual", !testCharge.IsCostGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			Factory.Save();

			Assert("Cost TestObjectCreator.GST1 rate should be actual", testCharge.IsCostGSTRateActual);
		}

		public void TestGSTApplicabilityForCommentChargeCodes()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader debtor = TestObjectCreator.AALSHI;
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(true);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.InvoiceRollupOrGroups[0].PG_InvoicePostingStyle = "DFI";

			OrgHeader creditor = TestObjectCreator.ABIGAS;
			creditor.CompanyData.OB_IsCreditor = true;
			creditor.CompanyData.SetAPTaxApplicable(true);
			creditor.CompanyData.OB_APWHTApplicable = false;

			OrgAddress localChargesAddr = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress agentCollectAddr = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = "123";
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			job.JH_OA_LocalChargesAddr = localChargesAddr.PK;
			job.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge charge = job.Charges.AddNew();
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_InvoiceType = "FIN";
			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_InvoiceType = "FIN";

			Factory.Save();

			debtor.CompanyData.SetARTaxApplicable(false);
			creditor.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 rate should be actual (because charge code type is CMT)", charge.IsSellGSTRateActual);
			Assert("Cost TestObjectCreator.GST1 rate should be actual (because charge code type is CMT)", charge.IsCostGSTRateActual);
		}

		public void TestResetTaxDefaults()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.SetARTaxApplicable(false);
			org1.CompanyData.OB_ARWHTApplicable = false;
			org1.OH_FullName = "Test Debtor";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.SetAPTaxApplicable(false);
			org2.CompanyData.OB_APWHTApplicable = false;
			org2.OH_FullName = "Test Creditor";
			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			CommonShipment shipment = consol.Shipments.AddNew();
			Job testJob = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_InvoiceType = "FIN";
			testCharge.JR_OSSellAmt = 100m;

			Charge testCharge2 = testJob.Charges.AddNew();
			testCharge2.JR_OH_CostAccount = org2.PK;
			testCharge2.JR_AC = chargeCode.PK;
			testCharge2.JR_InvoiceType = "FIN";
			testCharge2.JR_OSCostAmt = 100m;
			testCharge2.JR_LocalCostAmt = 100m;

			Assert("Sell TestObjectCreator.GST1 should be actual", testCharge.IsSellGSTRateActual);
			AssertEquals("Sell TestObjectCreator.GST1 should be empty", ZGuid.Empty, testCharge.JR_AT_SellGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be zero", 0M, testCharge.JR_Sell_LocalGSTAmount);
			Assert("Cost TestObjectCreator.GST1 should be actual", testCharge2.IsCostGSTRateActual);
			AssertEquals("Cost TestObjectCreator.GST1 should be empty", ZGuid.Empty, testCharge2.JR_AT_CostGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be zero", 0M, testCharge2.JR_Cost_LocalGSTAmount);

			Factory.Save();

			org1.CompanyData.SetARTaxApplicable(true);
			org2.CompanyData.SetAPTaxApplicable(true);

			Factory.Save();

			Assert("Sell TestObjectCreator.GST1 should not be actual", !testCharge.IsSellGSTRateActual);
			Assert("Cost TestObjectCreator.GST1 should not be actual", !testCharge2.IsCostGSTRateActual);

			testCharge.ResetSellGSTTaxDefault();
			testCharge2.ResetCostGSTTaxDefault();

			Assert("Sell TestObjectCreator.GST1 should be actual", testCharge.IsSellGSTRateActual);
			AssertEquals("Sell TestObjectCreator.GST1 should not be empty", TestObjectCreator.GST1.PK, testCharge.JR_AT_SellGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be 10", 10M, testCharge.JR_Sell_LocalGSTAmount);
			Assert("Cost TestObjectCreator.GST1 should be actual", testCharge2.IsCostGSTRateActual);
			AssertEquals("Cost TestObjectCreator.GST1 should not be empty", TestObjectCreator.GST1.PK, testCharge2.JR_AT_CostGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be 10", 10M, testCharge2.JR_Cost_LocalGSTAmount);

			#region IsRevenuePosted

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("R0001", TestObjectCreator.AUD, 1m, org1);
			var arLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, testJob, chargeCode, TestObjectCreator.AUD, 1m, "AR", testCharge.JR_OSSellAmt);
			testCharge.ReverseWIP(ZDateTime.Now);
			testCharge.JR_AL_ARLine = arLine.PK;
			Assert(testCharge.IsRevenuePosted);

			Assert("Sell TestObjectCreator.GST1 should be actual", testCharge.IsSellGSTRateActual);
			AssertEquals("Sell TestObjectCreator.GST1 should not be empty", TestObjectCreator.GST1.PK, testCharge.JR_AT_SellGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be 10", 10M, testCharge.JR_Sell_LocalGSTAmount);

			org1.CompanyData.SetARTaxApplicable(false);
			AssertEquals("Sell TestObjectCreator.GST1 should not be actual", false, testCharge.IsSellGSTRateActual);

			testCharge.ResetSellGSTTaxDefault();
			AssertEquals("Sell TestObjectCreator.GST1 should not be empty", TestObjectCreator.GST1.PK, testCharge.JR_AT_SellGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be 10", 10M, testCharge.JR_Sell_LocalGSTAmount);

			#endregion

			#region IsCostPosted

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("P00001", TestObjectCreator.AUD, 1m, testCharge2.JR_OSCostAmt, testCharge2.JR_OSCostGSTAmt_Calc, 0m, testCharge2.JR_LocalSellAmt, testCharge2.JR_Calc_LocalSellTaxAmt, 0m, org1);
			var apLine = TestObjectCreator.CreateAPInvoiceLine(apInvoice, testJob, chargeCode, TestObjectCreator.AUD, 1m, "AP", testCharge2.JR_OSCostAmt);
			testCharge2.ReverseAccrual(ZDateTime.Now);
			testCharge2.JR_AL_APLine = apLine.PK;
			Assert(testCharge2.IsCostPosted);

			Assert("Cost TestObjectCreator.GST1 should be actual", testCharge2.IsCostGSTRateActual);
			AssertEquals("Cost TestObjectCreator.GST1 should not be empty", TestObjectCreator.GST1.PK, testCharge2.JR_AT_CostGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be 10", 10M, testCharge2.JR_Cost_LocalGSTAmount);

			org2.CompanyData.SetAPTaxApplicable(false);
			AssertEquals("Cost TestObjectCreator.GST1 should not be actual", false, testCharge2.IsCostGSTRateActual);

			testCharge2.ResetCostGSTTaxDefault();
			AssertEquals("Cost TestObjectCreator.GST1 should not be empty", TestObjectCreator.GST1.PK, testCharge2.JR_AT_CostGSTRate);
			AssertEquals("Sell TestObjectCreator.GST1 amount should be 10", 10M, testCharge2.JR_Cost_LocalGSTAmount);

			#endregion

			#region JR_IsApportioned

			org2.CompanyData.SetAPTaxApplicable(true);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, org2);
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			var apportionedCharge = Factory.Load<Charge>(consolCost.ApportionmentCharges[0].PK);
			Assert(apportionedCharge.JR_IsApportioned);

			AssertEquals("apportionedCharge.JR_AT_CostGSTRate", TestObjectCreator.GST1.PK, apportionedCharge.JR_AT_CostGSTRate);
			org2.CompanyData.SetAPTaxApplicable(false);
			AssertEquals("apportionedCharge.IsCostGSTRateActual", false, apportionedCharge.IsCostGSTRateActual);

			apportionedCharge.ResetCostGSTTaxDefault();
			AssertEquals("apportionedCharge.JR_AT_CostGSTRate should not be changed", TestObjectCreator.GST1.PK, apportionedCharge.JR_AT_CostGSTRate);

			#endregion
		}

		#endregion

		public void TestOnSavingInCompanyContextShouldNotCreateACRLineWhenFactoryDoesNotHasNonAccountingCodeContext()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testCreditor = Factory.New<OrgHeader>();
				testCreditor.OH_Code = "testcreditor";
				var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
				var job = TestObjectCreator.CreateJob(shipment);
				job.Charges.RemoveAndDeleteAll();
				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();

				var charge = job.Charges.AddNew();
				charge.JR_OH_CostAccount = testCreditor.PK;
				charge.JR_AC = chargeCode.PK;
				charge.JR_LocalCostAmt = 11.5M;

				Factory.Save();
				AssertNull(charge.Accrual);

				testCreditor.OH_IsCreditor = true;
				Factory.SetContext(BusinessContext.NonAccountingCode);
				Factory.Save();
				AssertNull(charge.Accrual);

				charge.JR_LocalCostAmt = 11.6M;
				Factory.Save();
				AssertNotNull(charge.Accrual);
			}
		}

		public void TestOnSavingInCompanyContextShouldNotCreateWIPLineWhenFactoryDoesNotHasNonAccountingCodeContext()
		{
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testDebitor = Factory.New<OrgHeader>();
				testDebitor.OH_Code = "testdebitor";
				var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
				var job = TestObjectCreator.CreateJob(shipment);
				job.Charges.RemoveAndDeleteAll();
				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();

				var charge = job.Charges.AddNew();
				charge.JR_OH_SellAccount = testDebitor.PK;
				charge.JR_AC = chargeCode.PK;
				charge.JR_LocalSellAmt = 11.5M;

				Factory.Save();
				AssertNull(charge.WIP);

				testDebitor.OH_IsDebtor = true;
				Factory.SetContext(BusinessContext.NonAccountingCode);
				Factory.Save();
				AssertNull(charge.WIP);

				charge.JR_LocalSellAmt = 11.6M;
				Factory.Save();
				AssertNotNull(charge.WIP);
			}
		}

		public void TestOnFactorySavingInCompanyContextShouldNotCreateACRLineWhenFactoryHasNonAccountingCodeContext()
		{
			using (AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testCreditor = Factory.New<OrgHeader>();
				testCreditor.OH_Code = "testcreditor";
				var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
				var job = TestObjectCreator.CreateJob(shipment);
				job.Charges.RemoveAndDeleteAll();
				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();

				var charge = job.Charges.AddNew();
				charge.JR_OH_CostAccount = testCreditor.PK;
				charge.JR_AC = chargeCode.PK;
				charge.JR_LocalCostAmt = 11.5M;

				Factory.Save();
				AssertNull(charge.Accrual);

				Factory.SetContext(BusinessContext.NonAccountingCode);
				testCreditor.OH_IsCreditor = true;

				Factory.Save();
				AssertNull(charge.Accrual);

				Factory.RemoveContext(BusinessContext.NonAccountingCode);

				Factory.Save();
				AssertNotNull(charge.Accrual);
			}
		}

		public void TestOnFactorySavingInCompanyContextShouldNotCreateWIPLineWhenFactoryHasNonAccountingCodeContext()
		{
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testDebitor = Factory.New<OrgHeader>();
				testDebitor.OH_Code = "testdebitor";
				var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
				var job = TestObjectCreator.CreateJob(shipment);
				job.Charges.RemoveAndDeleteAll();
				var chargeCode = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).First();

				var charge = job.Charges.AddNew();
				charge.JR_OH_SellAccount = testDebitor.PK;
				charge.JR_AC = chargeCode.PK;
				charge.JR_LocalSellAmt = 22.5M;

				Factory.Save();
				AssertNull(charge.WIP);

				Factory.SetContext(BusinessContext.NonAccountingCode);
				testDebitor.OH_IsDebtor = true;

				Factory.Save();
				AssertNull(charge.WIP);

				Factory.RemoveContext(BusinessContext.NonAccountingCode);

				Factory.Save();
				AssertNotNull(charge.WIP);
			}
		}

		[ExpectNoExceptions]
		public void TestCostExRateShouldBeUpdatedWhenCostCurrencyIsChangedOnPostAPInvoice()
		{
			var creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.AUD.Code;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = creator.AUD.Code;
			var shipment = creator.CreateShipment("S001001");
			var job = creator.CreateJob(shipment, false, false);
			creator.SetExchangeRate(job, creator.USD, 2m);

			var charge1 = creator.CreateCharge(job, creator.CC1, "desc", creator.USD, 0m, null, creator.USD, 100m, creator.ABIGAS);
			charge1.JR_GE = creator.FIADepartment.PK;
			var charge2 = creator.CreateCharge(job, creator.CC1, "desc", creator.USD, 0m, null, creator.AUD, 200m, creator.ABIGAS);
			charge2.JR_GE = creator.FIADepartment.PK;
			charge2.JR_RX_NKCostCurrency = creator.USD.RX_Code;
			charge2.JR_OSCostAmt = 0m;

			AssertEquals("should be USD", creator.USD.RX_Code, charge1.JR_RX_NKCostCurrency);
			AssertEquals("should be USD", creator.USD.RX_Code, charge1.JR_RX_NKSellCurrency);
			AssertEquals("should be 2m", 2m, charge1.JR_OSCostExRate);
			AssertEquals("should be 2m", 2m, charge1.JR_OSSellExRate);
			AssertEquals("should be 0m", 0m, charge1.JR_OSCostAmt);
			AssertEquals("should be 100m", 100m, charge1.JR_OSSellAmt);

			AssertEquals("should be USD", creator.USD.RX_Code, charge2.JR_RX_NKCostCurrency);
			AssertEquals("should be AUD", creator.AUD.RX_Code, charge2.JR_RX_NKSellCurrency);
			AssertEquals("should be 2m", 2m, charge2.JR_OSCostExRate);
			AssertEquals("should be 1m", 1m, charge2.JR_OSSellExRate);
			AssertEquals("should be 0m", 0m, charge2.JR_OSCostAmt);
			AssertEquals("should be 200m", 200m, charge2.JR_OSSellAmt);

			new ChargePoster(Factory).Post(charge1);
			Assert("Cost is not posted", !charge1.JR_IsCostPosted);
			Assert("Revenue is posted", charge1.JR_IsRevenuePosted);
			Assert("Cost is not posted", !charge2.JR_IsCostPosted);
			Assert("Revenue is not posted", !charge2.JR_IsRevenuePosted);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var apInvoice = newFactory.NewWithValidTestData<APInvoice>();
			apInvoice.SubmittedFromInvoicingForm = true;
			var apInvoiceLine = (APInvoiceLine)apInvoice.Lines.AddNew();
			apInvoiceLine.AL_JH = job.PK;
			apInvoiceLine.GenericCharge = creator.CC1.PK;
			apInvoiceLine.AL_RX_NKTransactionCurrency = creator.AUD.Code;
			apInvoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
			apInvoiceLine.AL_GE = creator.FIADepartment.PK;
			apInvoiceLine.AL_OSExTaxAmount = 20;
			newFactory.Save();

			AssertEquals("still have two charges", 2, job.Charges.Count);
			Assert("Cost is posted", charge1.JR_IsCostPosted);
			Assert("Revenue is posted", charge1.JR_IsRevenuePosted);
			Assert("Cost is not posted", !charge2.JR_IsCostPosted);
			Assert("Revenue is not posted", !charge2.JR_IsRevenuePosted);

			AssertEquals("should be AUD", creator.AUD.RX_Code, charge1.JR_RX_NKCostCurrency);
			AssertEquals("should be USD", creator.USD.RX_Code, charge1.JR_RX_NKSellCurrency);
			AssertEquals("should be 1m", 1m, charge1.JR_OSCostExRate);
			AssertEquals("should be 2m", 2m, charge1.JR_OSSellExRate);
			AssertEquals("should be 20m", 20m, charge1.JR_OSCostAmt);
			AssertEquals("should be 100m", 100m, charge1.JR_OSSellAmt);

			AssertEquals("should be AUD", creator.AUD.RX_Code, charge2.JR_RX_NKCostCurrency);
			AssertEquals("should be AUD", creator.AUD.RX_Code, charge2.JR_RX_NKSellCurrency);
			AssertEquals("should be 1m", 1m, charge2.JR_OSCostExRate);
			AssertEquals("should be 1m", 1m, charge2.JR_OSSellExRate);
			AssertEquals("should be 0m", 0m, charge2.JR_OSCostAmt);
			AssertEquals("should be 220m", 220m, charge2.JR_OSSellAmt);
		}

		[ExpectNoExceptions]
		public void TestJR_OSCostExRateWhenSetJR_RX_NKCostCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.AUD.Code;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = creator.AUD.Code;
			var shipment = creator.CreateShipment("S001001");
			var job = creator.CreateJob(shipment, false, false);

			var charge = creator.CreateCharge(job, creator.CC1, 0, 0);
			AssertEquals(0m, charge.JR_OSCostAmt);
			charge.JR_RX_NKCostCurrency = creator.USD.Code;
			AssertEquals(1, job.ExchangeRates.Count);
			job.ExchangeRates[0].JF_BaseRate = 2m;
			Assert("Not marked as entered manually. Could be removed when not in use.", !job.ExchangeRates[0].JF_IsTransformed);
			AssertEquals("should be 2m", 2m, charge.JR_OSCostExRate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedJob = newFactory.Load<Job>(job.PK);
			var reloadedCharge = reloadedJob.Charges[0];
			reloadedCharge.JR_RX_NKCostCurrency = creator.AUD.Code;
			AssertEquals("should be 1m", 1m, reloadedCharge.JR_OSCostExRate);
			AssertEquals("Unused Ex Rate should be removed", 0, reloadedJob.ExchangeRates.Count);
		}

		[ExpectNoExceptions]
		public void TestJR_OSSellExRateWhenSetJR_RX_NKSellCurrency()
		{
			var creator = new TestObjectCreator(Factory);
			creator.LocalClient.CompanyData.OB_RX_NKARDDefltCurrency = creator.AUD.Code;
			creator.LocalClient.CompanyData.OB_RX_NKAPDefltCurrency = creator.AUD.Code;
			var shipment = creator.CreateShipment("S001001");
			var job = creator.CreateJob(shipment, false, false);

			var charge = creator.CreateCharge(job, creator.CC1, 0, 0);
			AssertEquals(0m, charge.JR_OSSellAmt);
			charge.JR_RX_NKSellCurrency = creator.USD.Code;
			AssertEquals(1, job.ExchangeRates.Count);
			job.ExchangeRates[0].JF_BaseRate = 2m;
			Assert("Not marked as entered manually. Could be removed when not in use.", !job.ExchangeRates[0].JF_IsTransformed);
			AssertEquals("should be 2m", 2m, charge.JR_OSSellExRate);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedJob = newFactory.Load<Job>(job.PK);
			var reloadedCharge = reloadedJob.Charges[0];
			reloadedCharge.JR_RX_NKSellCurrency = creator.AUD.Code;
			AssertEquals("should be 1m", 1m, reloadedCharge.JR_OSSellExRate);
			AssertEquals("Unused Ex Rate should be removed", 0, reloadedJob.ExchangeRates.Count);
		}

		public void TestResetUnpostedCostTaxDefault()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.SetAPTaxApplicable(false);
			org.CompanyData.OB_APWHTApplicable = false;
			Factory.Save();

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var testCharge = TestObjectCreator.CreateCharge(job, chargeCode, creditor: org, osSellAmt: 100m);
			testCharge.JR_InvoiceType = "FIN";
			Factory.Save();

			AssertEquals("Pre-condition", true, testCharge.IsCostGSTRateActual);
			AssertEquals(false, testCharge.ChargeCode.IsComment);
			AssertEquals(true, AccountingMasterFilesUtils.IsTaxBranchApplicable);
			AssertEquals(false, testCharge.IsCostGSTApplicable);
			AssertEquals(ZGuid.Empty, testCharge.JR_AT_CostGSTRate);
			AssertEquals(0M, testCharge.JR_Cost_LocalGSTAmount);
			AssertEquals(ZGuid.Empty, testCharge.JR_GB_CostTaxBranch);

			org.CompanyData.SetAPTaxApplicable(true);
			Factory.Save();

			AssertEquals("Pre-condition", false, testCharge.IsCostGSTRateActual);
			AssertEquals(true, testCharge.IsCostGSTApplicable);

			testCharge.ResetUnpostedCostTaxDefault();

			AssertEquals(true, testCharge.IsCostGSTRateActual);
			AssertEquals(TestObjectCreator.GST1.PK, testCharge.JR_AT_CostGSTRate);
			AssertEquals(10M, testCharge.JR_Cost_LocalGSTAmount);
			AssertEquals(job.JH_GB_TaxBranch, testCharge.JR_GB_CostTaxBranch);
		}

		public void TestResetUnpostedCostTaxDefaultForCommentCharge()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsCreditor = true;
			org.CompanyData.SetAPTaxApplicable(true);
			org.CompanyData.OB_APWHTApplicable = false;
			Factory.Save();

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var testCharge = TestObjectCreator.CreateCharge(job, chargeCode, creditor: org, osSellAmt: 100m);
			testCharge.JR_GB_CostTaxBranch = ZGuid.Empty;
			testCharge.JR_InvoiceType = "FIN";
			Factory.Save();

			AssertEquals(false, testCharge.ChargeCode.IsComment);
			AssertEquals(false, testCharge.IsSellGSTApplicable);
			AssertEquals(true, AccountingMasterFilesUtils.IsTaxBranchApplicable);
			AssertEquals(ZGuid.Empty, testCharge.JR_GB_CostTaxBranch);

			testCharge.ChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			Factory.Save();

			AssertEquals(true, testCharge.ChargeCode.IsComment);

			testCharge.ResetUnpostedCostTaxDefault();

			AssertEquals(ZGuid.Empty, testCharge.JR_GB_CostTaxBranch);

			testCharge.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			Factory.Save();

			AssertEquals(false, testCharge.ChargeCode.IsComment);

			testCharge.ResetUnpostedCostTaxDefault();

			AssertEquals(job.JH_GB_TaxBranch, testCharge.JR_GB_CostTaxBranch);
		}

		public void TestCreateAccrualCoreWithValidationSuspenderWorks()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var chargeWithCost = CreateCharge(job, MRG100, "Validation Suspender Test", GlbCompany.CurrentCompany.LocalCurrency, 250M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 350M, AALSHI);
			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

			var invalidLocalCostAmt = 9999999999999999999999999999m;

			chargeWithCost.JR_LocalCostAmt = invalidLocalCostAmt;
			AssertHasErrors(chargeWithCost.JR_LocalCostAmtInfo);

			chargeWithCost.CreateAccrualCore_ForTestOnly();

			AssertNotNull(chargeWithCost.Accrual);
			AssertNoErrors("We suspend validation to improve performance as we create/reverse WIP/ACR on saving where we do not care about validation errors.", chargeWithCost.Accrual.AL_LineAmountInfo);

			chargeWithCost.Accrual.Validation.ValidateAL_LineAmount();
			AssertHasErrorContaining("This proofs that charge has validation error if validation is not suspended", chargeWithCost.Accrual.AL_LineAmountInfo, "The number 9,999,999,999,999,999,999,999,999,999 is too large");
		}

		public void TestCreateWIPCoreWithValidationSuspenderWorks()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var chargeWithCost = CreateCharge(job, MRG100, "Validation Suspender Test", GlbCompany.CurrentCompany.LocalCurrency, 250M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 350M, AALSHI);
			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

			chargeWithCost.JR_LocalSellAmt = 9999999999999999999999999999m;
			AssertHasErrors(chargeWithCost.JR_LocalSellAmtInfo);

			chargeWithCost.CreateWIPCore_ForTestOnly();

			AssertNotNull(chargeWithCost.WIP);
			AssertNoErrors("We suspend validation to improve performance as we create/reverse WIP/ACR on saving where we do not care about validation errors.", chargeWithCost.WIP.AL_OSExTaxAmountInfo);

			chargeWithCost.WIP.LineValidation_MightBeNull.ValidateAL_OSExTaxAmount();
			AssertHasErrorContaining("This proofs that charge has validation error if validation is not suspended", chargeWithCost.WIP.AL_OSExTaxAmountInfo, "The number -9,999,999,999,999,999,999,999,999,999 is too large");
		}

		public void TestReverseAccrualWithValidationSuspenderWorksWithForceReverse()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var chargeWithCost = CreateCharge(job, MRG100, "Validation Suspender Test", GlbCompany.CurrentCompany.LocalCurrency, 250M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 350M, AALSHI);
			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

			chargeWithCost.CreateAccrualCore_ForTestOnly();
			AssertNotNull(chargeWithCost.Accrual);

			var accrual = chargeWithCost.Accrual;

			var invalidReverseDate = new ZDateTime(1899, 1, 1, 0, 0, 0);
			chargeWithCost.ReverseAccrual(invalidReverseDate, true);

			AssertNoErrors("We suspend validation to improve performance as we create/reverse WIP/ACR on saving where we do not care about validation errors.", accrual.AL_ReverseDateInfo);

			accrual.Validation.ValidateAL_ReverseDate();
			AssertHasErrorContaining("This proofs that charge has validation error if validation is not suspended", accrual.AL_ReverseDateInfo, "The date '01-Jan-1899' is earlier than '01-Jan-1900', the limit for this field.");
		}

		public void TestReverseWIPWithValidationSuspenderWorksWithForceReverse()
		{
			var job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			var chargeWithCost = CreateCharge(job, MRG100, "Validation Suspender Test", GlbCompany.CurrentCompany.LocalCurrency, 250M, ZECTRA, GlbCompany.CurrentCompany.LocalCurrency, 350M, AALSHI);
			chargeWithCost.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

			chargeWithCost.CreateWIPCore_ForTestOnly();
			AssertNotNull(chargeWithCost.WIP);

			var wip = chargeWithCost.WIP;

			var invalidReverseDate = new ZDateTime(1899, 1, 1, 0, 0, 0);
			chargeWithCost.ReverseWIP(invalidReverseDate, true);

			AssertNoErrors("We suspend validation to improve performance as we create/reverse WIP/ACR on saving where we do not care about validation errors.", wip.AL_ReverseDateInfo);

			wip.Validation.ValidateAL_ReverseDate();
			AssertHasError("This proofs that charge has validation error if validation is not suspended", wip.AL_ReverseDateInfo, "The date '01-Jan-1899' is earlier than '01-Jan-1900', the limit for this field.");
		}

		public void TestUpdateSellExRateAddNegativeExRateMap()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, GlbCompany.CurrentCompany.GC_Code);
			company.GC_IsReciprocal = true;
			company.GC_RX_NKLocalCurrency = Constants.CurrencyCodes.Egypt;
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001000", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRate = TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0.032m);
			exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRate.JF_CFXPercent = 0m;
			exRate.JF_CFXMinimum = 1.9m;
			exRate.JF_OH_Org = GlbCompany.CurrentCompany.OrgProxy.PK;
			Factory.Save();

			var service = Factory.ServiceContainer.GetService<NegativeJobChargeOSSellExRateWhenSaveRecorder>();
			AssertNull(service);

			Factory.IsInSaveTransaction = true;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.DSBChargeCode, "", GlbCompany.CurrentCompany.LocalCurrency, debtor: GlbCompany.CurrentCompany.OrgProxy);
			charge.JR_LocalSellAmt = 1m;
			charge.JR_RX_NKSellCurrency = Constants.CurrencyCodes.UnitedStates;
			charge.JR_RX_NKCostCurrency = Constants.CurrencyCodes.Egypt;
			charge.UpdateSellExRateWithBaseRateFromLocalAmt(0.032m);
			service = Factory.ServiceContainer.GetService<NegativeJobChargeOSSellExRateWhenSaveRecorder>();
			AssertNotNull(service);
			AssertEquals(true, service.IsJobChargeExRateNegative(charge.PK));

			charge.JR_LocalSellAmt = 20m;
			charge.UpdateSellExRateWithBaseRateFromLocalAmt(0.032m);
			AssertEquals(false, service.IsJobChargeExRateNegative(charge.PK));
		}
	}
}
