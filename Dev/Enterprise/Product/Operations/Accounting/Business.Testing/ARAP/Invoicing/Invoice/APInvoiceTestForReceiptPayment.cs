using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using CostVarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle;
using CostVarianceComparisonOption = Enterprise.Core.Constants.CostVarianceComparisonOption;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoice))]
	public class APInvoiceTestForReceiptPayment : InvoiceTestForReceiptPayment
	{
		public override void TestDocManagerInfo()
		{
			InvoicingBase invoice = (InvoicingBase)GetNewBusinessObject();
			AssertEquals("DocManagerInfo.GetType()", typeof(APInvoiceDocManagerInfo), invoice.DocManagerInfo.GetType());
		}

		#region TestChargesAreNotLoadedDuringInvoicePosting

		public void TestChargesAreNotLoadedDuringInvoicePosting_NewCharge()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(false);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_ExistingCharge()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_CarryForwardAmount()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, 1);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_CarryForwardAmountWithZeroCost()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, -1);
		}
		public void TestChargesAreNotLoadedDuringInvoicePosting_CarryForwardAmountWithZeroCostAndPostedRevenue()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, -1, true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_MultiCarryForwardAmount()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, 2);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_NewCharge_ForeignCurrency()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(false, isForeignCurrency: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_ExistingCharge_ForeignCurrency()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, isForeignCurrency: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_CarryForwardAmount_ForeignCurrency()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, 1, isForeignCurrency: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_CarryForwardAmountWithZeroCost_ForeignCurrency()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, -1, isForeignCurrency: true);
		}
		public void TestChargesAreNotLoadedDuringInvoicePosting_CarryForwardAmountWithZeroCostAndPostedRevenue_ForeignCurrency()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, -1, true, isForeignCurrency: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_MultiCarryForwardAmount_ForeignCurrency()
		{
			AssertChargesAreNotLoadedDuringInvoicePosting(true, 2, isForeignCurrency: true);
		}

		void AssertChargesAreNotLoadedDuringInvoicePosting(bool existingCharge, int carryForwardChargeCount = 0, bool isRevenuePosted = false, bool isForeignCurrency = false)
		{
			var currency = TestObjectCreator.AUD;
			if (isForeignCurrency)
			{
				currency = TestObjectCreator.USD;
				TestObjectCreator.CreateExchangeRate(currency, 0.699986m);
			}
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, null, 0);
			new List<byte>(new byte[10]).ForEach(x => TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", currency, 10, null, currency, 10, null));
			var chargeCodeForPosting = TestObjectCreator.CC2;
			var existingChargeCount = !existingCharge ? 0 : (carryForwardChargeCount == 0 ? 1 : Math.Abs(carryForwardChargeCount));
			decimal costAmount = carryForwardChargeCount == 0 ? 80 : (carryForwardChargeCount > 0 ? 150 : 0);
			ARInvoice arInvoice = null;
			if (isRevenuePosted)
			{
				arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.Debtor);
			}
			new List<byte>(new byte[existingChargeCount]).ForEach(x =>
			{
				var charge = TestObjectCreator.CreateCharge(job, chargeCodeForPosting, "", currency, costAmount, null, currency, costAmount, null);
				if (isRevenuePosted)
				{
					TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK);
				}
			});
			Factory.Save();

			ReleaseFactory();
			TestObjectCreator = new TestObjectCreator(Factory);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", currency, organisation: TestObjectCreator.Creditor1);
			invoice.SubmittedFromInvoicingForm = true;
			TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCodeForPosting, 100, setCurrentDepartment: false);
			invoice.RunPreSaveValidation();
			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			Func<int> getChargesInMemoryCount = () => ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Where(x => x.GetType().IsSubclassOf(typeof(JobCharge))).Select(x => x.PK).Distinct().Count();
			AssertEquals("Charges in memory after validation. The idea is to avoid loading charges as much as possible as this consume a lot of memory.", 0, getChargesInMemoryCount());
			AssertNoErrors(invoice);
			Factory.Save();

			AssertEquals("Charges in memory after saving. The idea is to avoid loading charges as much as possible as this consume a lot of memory.", carryForwardChargeCount > 0 || isRevenuePosted ? 2 : 1, getChargesInMemoryCount());
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_NewConsolCost_NoCharge()
		{
			//1 posted Apportioned Charge
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(false);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_NewConsolCost_ExistingCharge()
		{
			//1 posted Apportioned Charge
			//1 Unposted Unapportioned charge
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(existUnapportionedCharge: true, carryForwardChargeCount: 1, existingConsolCost: false);
		}
		public void TestChargesAreNotLoadedDuringInvoicePosting_NewConsolCost_CarryForwardAmountWithZeroCost()
		{
			//1 posted Apportioned Charge
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(existUnapportionedCharge: true, carryForwardChargeCount: -1, expectedCountOfChargeInMemory: 2);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_NewConsolCost_CarryForwardAmountWithZeroCostAndPostedRevenue()
		{
			//1 posted Apportioned Charge
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(existUnapportionedCharge: true, carryForwardChargeCount: -1, isRevenuePosted: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_ExistingConsolCost_ExistingCharge()
		{
			//1 posted Apportioned Charge
			//1 Unposted Carry Forward Apportioned Charge
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(existUnapportionedCharge: true, carryForwardChargeCount: 1, existingConsolCost: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_ExistingConsolCost_CarryForwardAmount()
		{
			//1 posted Apportioned Charge
			//1 Unposted Carry Forward Apportioned Charge
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(existUnapportionedCharge: false, carryForwardChargeCount: 1, existingConsolCost: true);
		}

		public void TestChargesAreNotLoadedDuringInvoicePosting_ExistingConsolCost_MultiCarryForwardAmount()
		{
			//Required to set bringForwardCreditor registry to true to allow multiple same charge code consol cost.
			//After Posting
			//1 Posted Apportioned Charge
			//1 Unposted Carry Forward Apportioned Charge without creditor
			//1 Unposted Carry Forward Apportioned Charge with creditor
			AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(true, 2, existingConsolCost: true, expectedCountOfChargeInMemory: 3, bringForwardCreditor: true);
		}

		void AssertChargesAreNotLoadedDuringInvoicePosting_WithConsolCost(bool existUnapportionedCharge, int carryForwardChargeCount = 0, bool isRevenuePosted = false, bool existingConsolCost = false, int? expectedCountOfChargeInMemory = null, bool bringForwardCreditor = false)
		{
			if (existingConsolCost)
			{
				Assert("There is imposable to not have zero cost as existing consol cost creates non zero charge.", carryForwardChargeCount >= 0);
			}
			using (AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, bringForwardCreditor))
			{
				var consol = TestObjectCreator.CreateConsol("C001", "AUSYD", "NZAKL");
				var shipment = TestObjectCreator.CreateShipment("S001", consol);
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, null, 0);
				new List<byte>(new byte[10]).ForEach(x => TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10));
				var chargeCodeForPosting = TestObjectCreator.CC2;
				var existingChargeCount = !existingConsolCost ? 0 : (carryForwardChargeCount == 0 ? 1 : Math.Abs(carryForwardChargeCount));
				decimal costAmount = carryForwardChargeCount == 0 ? 80 : (carryForwardChargeCount > 0 ? 150 : 0);
				ARInvoice arInvoice = null;
				if (isRevenuePosted)
				{
					arInvoice = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: TestObjectCreator.Debtor);
				}
				if (existingConsolCost)
				{
					new List<byte>(new byte[existingChargeCount]).ForEach(x =>
					{
						var consolCost = TestObjectCreator.CreateConsolCost(consol, chargeCodeForPosting, costAmount);
						if (isRevenuePosted)
						{
							var chrg = Factory.Load<Charge>(consolCost.ApportionmentCharges[0].PK);
							AssertNotNull("Apportionment charge loaded as charge", chrg);
							TestObjectCreator.CreateRevenueLine(chrg, arInvoice.PK);
						}
						AssertNoErrors(consolCost);
					});
				}
				if (existUnapportionedCharge)
				{
					var charge = TestObjectCreator.CreateCharge(job, chargeCodeForPosting, costAmount, costAmount);
					if (isRevenuePosted)
					{
						TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK);
					}
					AssertNoErrors(charge);
				}

				Factory.Save();

				ReleaseFactory();
				TestObjectCreator = new TestObjectCreator(Factory);
				var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
				invoice.SubmittedFromInvoicingForm = true;
				var newConsolCost = TestObjectCreator.CreateConsolCost(invoice, consol, chargeCodeForPosting, 100);
				newConsolCost.RelatedConsolCostPK = ZGuid.Empty;
				invoice.ImportAllApportionmentsFromCosting();
				invoice.RunPreSaveValidation();
				// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
				Func<int> getChargesInMemoryCount = () => ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects.Where(x => x.GetType().IsSubclassOf(typeof(JobCharge))).Select(x => x.PK).Distinct().Count();
				AssertEquals("Charges in memory after validation. The idea is to avoid loading charges as much as possible as this consume a lot of memory.", 1, getChargesInMemoryCount());
				AssertNoErrors(invoice);
				Factory.Save();

				var expectedNumberOfChargeInMemory = expectedCountOfChargeInMemory ?? (carryForwardChargeCount > 0 || isRevenuePosted
															? (isRevenuePosted && !existingConsolCost ? 3 : 2)
															: 1);
				AssertEquals("Charges in memory after saving. The idea is to avoid loading charges as much as possible as this consume a lot of memory.", expectedNumberOfChargeInMemory, getChargesInMemoryCount());
			}
		}

		#endregion

		protected override bool ShouldSupportCalculatingTaxAtHeaderLevel
		{
			get { return false; }
		}

		public virtual void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_LocalInvoiceLocalPayment()
		{
			AssertCashInvoiceWithSPRTaxTransactions_FullyMatched(TestObjectCreator.AUD, TestObjectCreator.AUDBankAccount,
				40m, 4m, // OSTotalAmount = LocalTotalAmount = (40 + 4 = 44)
				2m, 2m,
				42m, 42m, // PaymentOSAmount = PaymentLocalAmount => LocalTotalAmount - taxTransaction_LocalTaxAmt = 44 - 2 = 42
				1m, 1m);
		}

		public virtual void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_ForeignInvoiceLocalPayment()
		{
			AssertCashInvoiceWithSPRTaxTransactions_FullyMatched(TestObjectCreator.USD, TestObjectCreator.AUDBankAccount,
				40m, 4m, // OSTotalAmount (40 + 4 = 44); LocalTotalAmount (80 + 8 = 88)
				6m, 12m,
				76m, 76m, // PaymentOSAmount = PaymentLocalAmount => (LocalTotalAmount - taxTransaction_LocalTaxAmt) = 88 - 12 = 76
				0.5m, 1m);
		}

		public virtual void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_ForeignInvoiceForeignPayment()
		{
			AssertCashInvoiceWithSPRTaxTransactions_FullyMatched(TestObjectCreator.USD, TestObjectCreator.USDBankAccount,
				40m, 4m,//OSTotalAmount = 44 ; LocalTotalAmount => OSTotalAmount/invoice.AH_ExchangeRate => 44 / 0.5 = 88
				6m, 12m,
				38m, // PaymentOSAmount => OSTotalAmount - taxTransaction_OSTaxAmt = 44 - 6
				76m, // PaymentLocalAmount => localTotalAmount - taxTransaction_LocalTaxAmt = 88 - 12
				0.5m, 0.5m);
		}

		public virtual void TestCashInvoiceWithSPRTaxTransactions_BalanceAdjustment_ForeignInvoiceForeignPayment_RoundingErrors()
		{
			AssertCashInvoiceWithSPRTaxTransactions_FullyMatched(TestObjectCreator.USD, TestObjectCreator.USDBankAccount,
				40m, 4m,//OSTotalAmount = 44 ; LocalTotalAmount => OSTotalAmount/invoiceExchangeRate => 44 / 6.188467 = 7.11;
				6m,
				0.88m,
				38m,// PaymentOSAmount => OSTotalAmount - taxTransaction_OSTaxAmt => 44 - 6
				6.23m,// PaymentLocalAmount => LocalTotalAmount - taxTransaction_LocalTaxAmt => 7.11 - 0.88
				6.188467m,// invoiceExchangeRate remains same
				6.099518m // PaymentExchangeRate => PaymentOSAmount / PaymentLocalAmount => 38 / 6.23
				);
		}

		void AssertCashInvoiceWithSPRTaxTransactions_FullyMatched(RefCurrency currency, AccBankAccount accBankAccount, 
				decimal osExTaxAmount, decimal osTaxAmount, 
				decimal taxTransaction_OSTaxAmt, decimal taxTransaction_LocalTaxAmt, 
				decimal expected_PaymentOSAmount, decimal expected_PaymentLocalAmount, 
				decimal invoiceExchangeRate, decimal expected_PaymentExchangeRate)
		{
			var taxAuthority = TFObjectCreator.CreateTaxAuthority("N01AU");
			var taxSystemSPR = TFObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			var taxConfigSPR = TFObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystemSPR, TaxConfigurationLedgers.AccountsPayable.Code);
			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystemSPR);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			Factory.Save();

			var apInvoice = TestObjectCreator.CreateCashInvoice(typeof(APInvoice), "APINV01", currency, invoiceExchangeRate, TestObjectCreator.Creditor1, 40, 4, TestObjectCreator.OverheadChargeCode.PK, accBankAccount);

			var taxTransactionsParams = new CreateTaxTransactionParameters
			{
				CurrencyCode = currency.Code,
				TransactionHeader = apInvoice,
				TaxSystem = taxSystemSPR,
				TaxConfiguration = taxConfigSPR,
				TaxBasis = TaxBasisList.PostingOnMatching.Code,
				RealisationDate = ZDate.Empty,
				TaxControlAccount = TestObjectCreator.GLHeader1,
				LedgerControlAccount = TestObjectCreator.GLHeader2,
				OsTaxAmount = taxTransaction_OSTaxAmt,
				LocalTaxAmount = taxTransaction_LocalTaxAmt,
				AffectsSourceTransactionTotal = false
			};

			var taxTransactionSPR = TFObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			TFObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(apInvoice.Lines[0]));

			Factory.Save();

			var invoice = Factory.Load<APInvoice>(apInvoice.PK);
			var journals = Factory.Load<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal));
			var payments = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment));

			AssertEquals("PostCondition: journal count", 1, journals.Length);
			AssertEquals("PostCondition: payment count", 1, payments.Length);

			Assert("PostCondition: invoice posted", invoice.IsInDatabase);
			Assert("PostCondition: journal posted", journals[0].IsInDatabase);
			Assert("PostCondition: payment posted", payments[0].IsInDatabase);

			AssertEquals("PostCondition: journal.TransactionCategory", TransactionCategory.Codes.PaymentBasisWithholding, journals[0].AH_TransactionCategory);

			AssertNotEquals("PostCondition: invoice amount", 0m, invoice.AH_LocalTotalAmount);
			AssertNotEquals("PostCondition: journal amount", 0m, journals[0].AH_LocalTotalAmount);
			AssertNotEquals("PostCondition: payment amount", 0m, payments[0].AH_LocalTotalAmount);

			AssertEquals("PostCondition: Payment Local Amount", expected_PaymentLocalAmount, payments[0].AH_LocalTotalAmount);
			AssertEquals("PostCondition: Payment OS Amount", expected_PaymentOSAmount, payments[0].AH_OSTotalAmount);

			AssertEquals("PostCondition: invoice ExchangeRate", invoiceExchangeRate, invoice.AH_ExchangeRate);
			AssertEquals("PostCondition: payment ExchangeRate", expected_PaymentExchangeRate, payments[0].AH_ExchangeRate);

			AssertEquals("Invoice Outstanding Amount", 0m, invoice.AH_OutstandingAmount);
			AssertEquals("journal Outstanding Amount", 0m, journals[0].AH_OutstandingAmount);
			AssertEquals("Payment Outstanding Amount", 0m, payments[0].AH_OutstandingAmount);

			Assert("Invoice Fully Paid", !invoice.AH_FullyPaidDate.IsEmpty);
			Assert("journal Fully Paid", !journals[0].AH_FullyPaidDate.IsEmpty);
			Assert("Payment Fully Paid", !payments[0].AH_FullyPaidDate.IsEmpty);
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(APInvoiceLine);
		}

		public override void TestCreditLimitExceededEmailWillBeSentOnlyOnce()
		{
			Assert("AR test", true);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(APInvoiceValidation); }
		}

		public override void TestTransactionNumberOnSave()
		{
			AssertAPItemsRetainUserSetTransactionNum();
		}

		public void TestCodeProperty()
		{
			InvoicingBase invoice = (InvoicingBase)new BusinessObjectFactory().New(GetExpectedBusinessObjectType());
			invoice.AH_ConsolidatedInvoiceRef = "ABC123";
			invoice.AH_TransactionNum = "TransNum";
			AssertEquals("Code Property Should be AH_ConsolidatedInvoiceRef because it's unique for AP Trans", invoice.AH_ConsolidatedInvoiceRef, ((ICodeDescription)invoice).Code);
		}

		public void TestAPItemsRetainUserSetTransactionNumWhenReversing()
		{
			TransactionHeader originalHeader = PrepareTransactionHeaderForTest() as TransactionHeader;
			originalHeader.AH_TransactionNum = "10001005";
			originalHeader.GenerateReverseTransaction(true);
			Header = originalHeader.ReverseTransaction as TransactionHeader;
			AssertAPItemsRetainUserSetTransactionNum();
		}

		public void TestAmendAPInvoiceGenerateInternalRef()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
			Factory.Save();

			var creditNote = (InvoicingBase)((IAmending)invoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
			creditNote.AH_TransactionNum = "TransNum001";
			Factory.Save();
			AssertEquals("number should be generated from number fountain ", "00001000", creditNote.AH_ConsolidatedInvoiceRef);
		}

		#region TestCopyLinesCopyDescription

		public void TestCopyLinesCopyDescription()
		{
			InvoicingBase.Lines.AddNew();
			InvoicingBase.Lines.AddNew();

			InvoicingBase.Lines[0].AL_AC = ChargeCode.PK;
			InvoicingBase.Lines[1].AL_AC = ChargeCode.PK;

			InvoicingBase.Lines[0].AL_Desc = "Line Description 1";
			InvoicingBase.Lines[1].AL_Desc = "Line Description 2";

			TransactionHeader copiedARInvoice = ((APInvoice)InvoicingBase).CopyTransaction_ForTestOnly();

			AssertEquals(ChargeCode.PK, ((APInvoice)copiedARInvoice).Lines[0].AL_AC);
			AssertEquals(ChargeCode.PK, ((APInvoice)copiedARInvoice).Lines[1].AL_AC);
			AssertEquals("Line Description 1", ((APInvoice)copiedARInvoice).Lines[0].AL_Desc);
			AssertEquals("Line Description 2", ((APInvoice)copiedARInvoice).Lines[1].AL_Desc);
		}

		#endregion

		#region Hot Cheque

		#region Test Hot Cheques Events

		#region Test Display Hot Cheques Event

		public void TestDisplayHotChequesEvent()
		{
			OrgHeader org1 = TestObjectCreator.CreateOrgHeader("Org1", true, false);
			OrgHeader org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true);

			// To prove that receipts don't affect the list of hot cheques
			var receipt = ObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, ObjectCreator.AUDBankAccount.PK);
			receipt.AH_ChequeOrReference = "001004";

			AccHotCheque hotCheque1 = CreateHotCheque(org1, "001001", false);
			AccHotCheque hotCheque2 = CreateHotCheque(org1, "001002", false);
			AccHotCheque hotCheque3 = CreateHotCheque(org1, "001003", true);
			AccHotCheque hotCheque4 = CreateHotCheque(org2, "001004", false);

			NumberOfTimesEventWasCalledDuringTest = 0;

			TestInvoice.AH_OH = org2.PK;
			AccHotChequeCollection activeHotCheques = ((APInvoice)TestInvoice).GetActiveHotCheques();

			AssertEquals("Active Hot Cheques Count", 1, activeHotCheques.Count);

			AssertEquals("Active Hot Cheques contains Cheque1", false, activeHotCheques.Contains(hotCheque1));
			AssertEquals("Active Hot Cheques contains Cheque2", false, activeHotCheques.Contains(hotCheque2));
			AssertEquals("Active Hot Cheques contains Cheque3", false, activeHotCheques.Contains(hotCheque3));
			AssertEquals("Active Hot Cheques contains Cheque4", true, activeHotCheques.Contains(hotCheque4));

			TestInvoice.AH_OH = org1.PK;
			activeHotCheques = ((APInvoice)TestInvoice).GetActiveHotCheques();

			AssertEquals("Active Hot Cheques Count", 2, activeHotCheques.Count);
			AssertEquals("Active Hot Cheques contains Cheque1", true, activeHotCheques.Contains(hotCheque1));
			AssertEquals("Active Hot Cheques contains Cheque2", true, activeHotCheques.Contains(hotCheque2));
			AssertEquals("Active Hot Cheques contains Cheque3", false, activeHotCheques.Contains(hotCheque3));
			AssertEquals("Active Hot Cheques contains Cheque4", false, activeHotCheques.Contains(hotCheque4));

			((APInvoice)TestInvoice).DisplayHotCheques += new APInvoice.HotChequeSelectedHandler(TestInvoice_DisplayHotCheques);

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;
			AssertEquals("DisplayHotChequesEvent should have been called", 1, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestInvoice_DisplayHotCheques(object sender, HotChequeLink link)
		{
			NumberOfTimesEventWasCalledDuringTest++;
			Assert("Event Sender should be of type APInvoice", sender is APInvoice);

			APInvoice sourceOfEvent = sender as APInvoice;
			AssertEquals("Event Sender", TestInvoice.PK, sourceOfEvent.PK);
		}

		AccHotCheque CreateHotCheque(OrgHeader header, ZString chequeNumber, bool cancelled)
		{
			AccHotCheque newHotCheque = Factory.New<AccHotCheque>();
			newHotCheque.AQ_OH = header.PK;
			newHotCheque.AQ_AK = TestObjectCreator.AUDChequeBook.PK;
			newHotCheque.AQ_Cancelled = cancelled;
			newHotCheque.AQ_AH = ZGuid.Empty;
			newHotCheque.AQ_ChequeNumber = chequeNumber;
			return newHotCheque;
		}

		#endregion

		#region Test Notify User Payment Uneditable Event

		public void TestNotifyUserPaymentUneditableEvent()
		{
			OrgHeader org1 = TestObjectCreator.CreateOrgHeader("Org1", true, false);
			OrgHeader org2 = TestObjectCreator.CreateOrgHeader("Org2", true, true);

			AccHotCheque hotCheque = Factory.New<AccHotCheque>();
			hotCheque.AQ_OH = org1.PK;
			hotCheque.AQ_Description = "AP INVOICE DESCRIPTION";
			hotCheque.AQ_AK = TestObjectCreator.AUDChequeBook.PK;
			hotCheque.AQ_Amount = 1000M;

			NumberOfTimesEventWasCalledDuringTest = 0;

			((APInvoice)TestInvoice).FImportedHotCheque_ForTestOnly = hotCheque;
			((APInvoice)TestInvoice).NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(TestInvoice_NotifyUserPaymentUneditable);

			ExpectedEventErrorMessage = APInvoice.HotChequeErrorMessages.AH_OHError;
			TestInvoice.AH_OH = org2.PK;

			ExpectedEventErrorMessage = APInvoice.HotChequeErrorMessages.AH_ABError;
			TestInvoice.ReceiptPaymentAH_AB = TestObjectCreator.USDBankAccount.PK;

			ExpectedEventErrorMessage = APInvoice.HotChequeErrorMessages.AK_ABError;
			TestInvoice.ReceiptPaymentAK_AB = TestObjectCreator.USDChequeBook.PK;

			ExpectedEventErrorMessage = APInvoice.HotChequeErrorMessages.AH_ReceiptTypeError;
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;

			ExpectedEventErrorMessage = APInvoice.HotChequeErrorMessages.AH_ChequeOrReferenceError;
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = TestObjectCreator.GetRandomString(6);

			AssertEquals("Event should have been called 5 times", 5, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestInvoice_NotifyUserPaymentUneditable(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;
			AssertEquals("Event Message", ExpectedEventErrorMessage, message);
			ExpectedEventErrorMessage = ZString.Empty;
		}

		#endregion

		int NumberOfTimesEventWasCalledDuringTest;
		string ExpectedEventErrorMessage;

		#endregion

		public void TestOutstandingHotChequesShownWhenTheCashInvoiceChecked()
		{
			SetupHotChequeTestObjects();
			Org.OH_IsCreditor = true;
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_AH = ZGuid.Empty;
			hotCheque.AQ_Cancelled = false;

			APInvoice.DisplayHotCheques += new APInvoice.HotChequeSelectedHandler(IsDisplayHotChequeFired);
			fIsDisplayHotChequeFired = false;
			APInvoice.AH_OH = Org.PK;
			APInvoice.SubmittedFromInvoicingForm = true;
			APInvoice.IsInvoiceReceiptPayment = true;
			Assert("DisplayHotCheque should have been fired", fIsDisplayHotChequeFired);

			APInvoice.IsInvoiceReceiptPayment = false;
			hotCheque.AQ_AH = Factory.New<APPayment>().PK;  // make Hot Cheque InActive
			fIsDisplayHotChequeFired = false;
			APInvoice.IsInvoiceReceiptPayment = true;
			Assert("DisplayHotCheque should not have been fired", !fIsDisplayHotChequeFired);

			APInvoice.IsInvoiceReceiptPayment = false;
			hotCheque.AQ_AH = ZGuid.Empty;
			hotCheque.AQ_Cancelled = true;  // make Hot Cheque cancelled
			fIsDisplayHotChequeFired = false;
			APInvoice.IsInvoiceReceiptPayment = true;
			Assert("DisplayHotCheque should not have been fired", !fIsDisplayHotChequeFired);

			APInvoice.IsInvoiceReceiptPayment = false;
			hotCheque.AQ_OH = Factory.New<OrgHeader>().PK;  // change the Hot Cheque organisation
			hotCheque.AQ_Cancelled = false;
			fIsDisplayHotChequeFired = false;
			APInvoice.IsInvoiceReceiptPayment = true;
			Assert("DisplayHotCheque should not have been fired", !fIsDisplayHotChequeFired);
		}

		public void TestGetActiveHotCheques()
		{
			SetupHotChequeTestObjects();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			ChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;

			AccHotCheque hotCheque_Posted = GetNewHotCheque(Org, ChequeBook);
			hotCheque_Posted.AQ_Cancelled = false;
			hotCheque_Posted.AQ_AH = Factory.New<APPayment>().PK;

			AccHotCheque hotCheque_Cancelled = GetNewHotCheque(Org, ChequeBook);
			hotCheque_Cancelled.AQ_Cancelled = true;
			hotCheque_Cancelled.AQ_AH = ZGuid.Empty;

			AccHotCheque hotCheque_DiffOrg = GetNewHotCheque(org2, ChequeBook);
			hotCheque_DiffOrg.AQ_Cancelled = false;
			hotCheque_DiffOrg.AQ_AH = ZGuid.Empty;

			APInvoice.AH_OH = Org.PK;
			AssertEquals("The test Org should not have active hot cheques", 0, APInvoice.GetActiveHotCheques().Count);

			AccHotCheque hotCheque_Active = GetNewHotCheque(Org, ChequeBook);
			hotCheque_Active.AQ_Cancelled = false;
			hotCheque_Active.AQ_AH = ZGuid.Empty;

			AssertEquals("The test Org should have 1 active hot cheque", 1, APInvoice.GetActiveHotCheques().Count);
		}

		public void TestPopulateFieldsUsingHotCheque()
		{
			SetupHotChequeTestObjects();
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			ChequeBook.AK_AB = bankAccount.PK;
			hotCheque.AQ_AK = ChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "129990";
			hotCheque.AQ_Amount = 90.88m;

			// Reset values
			APInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			APInvoice.ReceiptPaymentAH_AB = ZGuid.Empty;
			APInvoice.ReceiptPaymentAK_AB = ZGuid.Empty;
			APInvoice.ReceiptPaymentAH_ChequeOrReference = ZString.Empty;
			APInvoice.ReceiptPaymentAH_OSTotalAmount = 0m;

			APInvoice.PopulateFieldsUsingHotCheque_ForTestOnly(hotCheque);
			AssertEquals("Payment type should be cheque", ReceiptTypes.Cheque, APInvoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("Bank account should be the test BankAccount", bankAccount.PK, APInvoice.ReceiptPaymentAH_AB);
			AssertEquals("Chequebook should be the test ChequeBook", ChequeBook.PK, APInvoice.ReceiptPaymentAK_AB);
			AssertEquals("Cheque number should be 129990", "129990", APInvoice.ReceiptPaymentAH_ChequeOrReference);
			AssertEquals("OSTotal amount should be 90.88", 90.88m, APInvoice.ReceiptPaymentAH_OSTotalAmount);
		}

		public void TestSetHotChequeInactiveAfterPosting()
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			APInvoice.SubmittedFromInvoicingForm = true;
			APInvoice.IsInvoiceReceiptPayment = true;
			APInvoice.SetHotChequeInactiveWhenPosting_ForTestOnly(hotCheque);
			APInvoice.Factory.Save();
			AssertEquals("AQ_AH should reference the payment", APInvoice.ReceiptPayment.PK, hotCheque.AQ_AH);
		}

		public void TestValidationForHotChequeImport()
		{
			SetupHotChequeTestObjects();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bank_Diff = Factory.NewWithValidTestData<AccBankAccount>();
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;
			AccChequeBook chequeBook_Diff = Factory.NewWithValidTestData<AccChequeBook>();
			ChequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ChequeNumber = "000023";
			hotCheque.AQ_Amount = 684.33m;
			hotCheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Actual;

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertNoErrors(APInvoice.ReceiptPaymentAH_ReceiptTypeInfo);
			AssertNoErrors(APInvoice.ReceiptPaymentAH_ABInfo);
			AssertNoErrors(APInvoice.ReceiptPaymentAK_ABInfo);
			AssertNoErrors(APInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
			AssertNoErrors(APInvoice.ReceiptPaymentAH_OSTotalAmountInfo);
		}

		public void TestSettingReceiptPaymentAH_AB()
		{
			SetupHotChequeTestObjects();
			APInvoice.NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccBankAccount bank = Factory.New<AccBankAccount>();
			AccBankAccount bank_Diff = Factory.New<AccBankAccount>();
			ChequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertEquals("Bank should be imported", bank.PK, APInvoice.ReceiptPaymentAH_AB);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAH_AB = bank.PK;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAH_AB = bank_Diff.PK;
			AssertEquals("Payment Bank should be unchanged", bank.PK, APInvoice.ReceiptPaymentAH_AB);
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		public void TestSettingReceiptPaymentAH_ChequeOrReference()
		{
			SetupHotChequeTestObjects();
			APInvoice.NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccBankAccount bank = Factory.New<AccBankAccount>();
			ChequeBook.AK_StartNo = 1;
			ChequeBook.AK_LastNo = 100;
			ChequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ChequeNumber = "000056";

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertEquals("ReferenceNumber should be imported", "000056", APInvoice.ReceiptPaymentAH_ChequeOrReference);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAH_ChequeOrReference = "000056";
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAH_ChequeOrReference = "000074";
			AssertEquals("ReferenceNumber should be unchanged", "000056", APInvoice.ReceiptPaymentAH_ChequeOrReference);
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		public void TestSettingReceiptPaymentAK_AB()
		{
			SetupHotChequeTestObjects();
			APInvoice.NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertEquals("ChequeBook should be imported", ChequeBook.PK, APInvoice.ReceiptPaymentAK_AB);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAK_AB = ChequeBook.PK;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAK_AB = Factory.New<AccChequeBook>().PK;
			AssertEquals("ChequeBook should be unchanged", ChequeBook.PK, APInvoice.ReceiptPaymentAK_AB);
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		public void TestSettingMaximumAH_OSTotalAmount()
		{
			APInvoiceLine invoiceLine = APInvoice.Lines.AddNew() as APInvoiceLine;
			invoiceLine.AL_OSExTaxAmount = 56m;

			SetupHotChequeTestObjects();
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Max;
			hotCheque.AQ_Amount = 56m;

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertEquals("No error should be there", false, APInvoice.AH_OSTotalAmountInfo.HasErrors());
			invoiceLine.AL_OSExTaxAmount = 45m;
			AssertEquals("No error should be there", false, APInvoice.AH_OSTotalAmountInfo.HasErrors());
			invoiceLine.AL_OSExTaxAmount = 67m;
			AssertEquals("Error should be there", true, APInvoice.AH_OSTotalAmountInfo.HasErrors());
		}

		public void TestSettingActualAH_OSTotalAmount()
		{
			APInvoiceLine invoiceLine = APInvoice.Lines.AddNew() as APInvoiceLine;
			invoiceLine.AL_OSExTaxAmount = 56m;

			SetupHotChequeTestObjects();
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			hotCheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Actual;
			hotCheque.AQ_Amount = 56m;

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertEquals("No error should be there", false, APInvoice.AH_OSTotalAmountInfo.HasErrors());
			invoiceLine.AL_OSExTaxAmount = 45m;
			AssertEquals("Error should be there", true, APInvoice.AH_OSTotalAmountInfo.HasErrors());
			invoiceLine.AL_OSExTaxAmount = 67m;
			AssertEquals("Error should be there", true, APInvoice.AH_OSTotalAmountInfo.HasErrors());
		}

		public void TestSettingReceiptPaymentAH_ReceiptType()
		{
			SetupHotChequeTestObjects();
			APInvoice.NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			AccHotCheque hotCheque = GetNewHotCheque(Org, ChequeBook);
			APInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;

			APInvoice.ImportSelectedHotCheque(hotCheque);
			AssertEquals("ReceiptType should be Cheque", ReceiptTypes.Cheque, APInvoice.ReceiptPaymentAH_ReceiptType);
			Assert("Precondition: NotifyUserUneditable not previously fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			APInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CreditCard;
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
			AssertEquals("ReceiptType should be unchanged", ReceiptTypes.Cheque, APInvoice.ReceiptPaymentAH_ReceiptType);
		}

		public void TestIsImportingHotCheque()
		{
			APInvoice.NotifyUserPaymentUneditable += new APInvoice.PaymentFieldsUneditableHandler(IsNotifyUserUneditableFired);
			APInvoice.BeginImportingHotCheque_ForTestOnly();
			APInvoice.FireNotifyUserPaymentUneditable_ForTestOnly("Test");
			Assert("NotifyUserUneditable should not be fired", !fIsNotifyUserUneditableFired);
			APInvoice.FinishImportingHotCheque_ForTestOnly();
			APInvoice.FireNotifyUserPaymentUneditable_ForTestOnly("Test");
			Assert("NotifyUserUneditable should be fired", fIsNotifyUserUneditableFired);
		}

		void IsNotifyUserUneditableFired(object sender, string message)
		{
			fIsNotifyUserUneditableFired = true;
		}

		bool fIsNotifyUserUneditableFired;

		void IsDisplayHotChequeFired(object sender, HotChequeLink link)
		{
			fIsDisplayHotChequeFired = true;
		}

		bool fIsDisplayHotChequeFired;

		AccHotCheque GetNewHotCheque(OrgHeader org, AccChequeBook chequeBook)
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_OH = org.PK;
			hotCheque.AQ_AK = chequeBook.PK;
			return hotCheque;
		}

		void SetupHotChequeTestObjects()
		{
			Org = Factory.NewWithValidTestData<OrgHeader>();
			ChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fIsNotifyUserUneditableFired = false;
		}

		OrgHeader Org;
		AccChequeBook ChequeBook;

		#endregion

		public void TestSettingReceiptPaymentAH_OSTotalAmountShouldValidateAH_OSTotalAmount()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OSTotalAmount = 200m;
			AssertEquals("HasErrors", false, invoice.AH_OSTotalAmountInfo.HasErrors());

			AccHotCheque hotCheque = Factory.New<AccHotCheque>();
			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Actual;
			hotCheque.AQ_Amount = 100m;
			invoice.ImportSelectedHotCheque(hotCheque);
			AssertHasError("Error expected", invoice.AH_OSTotalAmountInfo, "Invoice amount must be the same as the hot check amount");

			invoice.ClearImportedHotCheque();
			AssertEquals("HasErrors", false, invoice.AH_OSTotalAmountInfo.HasErrors());

			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Max;
			invoice.ImportSelectedHotCheque(hotCheque);
			AssertHasError("Error expected", invoice.AH_OSTotalAmountInfo, "Invoice amount must be less than or equal to the hot check amount");
		}

		public void TestReceiptPaymentAH_ChequeOrReference()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = 5;
			invoice.ReceiptPaymentAH_AB = testBank.PK;
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			invoice.ReceiptPaymentAH_ChequeOrReference = "12";
			AssertEquals("Should be 00012", "00012", invoice.ReceiptPaymentAH_ChequeOrReference);
		}

		public void TestImportJobCharges()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoicingBase.GetType());

			Charge chargeToImport1 = Factory.New<Charge>();
			Charge chargeToImport2 = Factory.New<Charge>();

			chargeToImport1.JR_JH = Job.PK;
			chargeToImport2.JR_JH = Job.PK;

			chargeToImport1.JR_AC = MRG100Code.PK;
			chargeToImport2.JR_AC = DSBChargeCode.PK;

			chargeToImport1.JR_OSCostAmt = 100.00m;
			chargeToImport2.JR_OSCostAmt = 50.00m;

			var jobCharges = new[] { chargeToImport1, chargeToImport2 };

			((System.ComponentModel.IBindingList)invoice.Lines).AddNew();

			invoice.Lines[0].AL_JH = Job.PK;

			((APInvoice)invoice).ImportJobChargesIntoInvoice(jobCharges, (APInvoiceLine)invoice.Lines[0]);

			Assert("Validation should not be suspended", !invoice.IsValidationSuspended);

			AssertEquals("Number of invoice lines should be 2", 2, invoice.Lines.Count);
			AssertEquals("First line should be first accrual in collection", chargeToImport1.PK, ((APInvoiceLine)invoice.Lines[0]).OriginalJobCharge.PK);
			AssertEquals("Second line should be second accrual in collection", chargeToImport2.PK, ((APInvoiceLine)invoice.Lines[1]).OriginalJobCharge.PK);
		}

		public void TestImportingSingleChargeResumesValidationCorrectly()
		{
			InvoicingBase invoice = (InvoicingBase)Factory.New(InvoicingBase.GetType());

			Charge chargeToImport1 = Factory.New<Charge>();
			chargeToImport1.JR_JH = Job.PK;
			chargeToImport1.JR_AC = MRG100Code.PK;
			chargeToImport1.JR_OSCostAmt = 100.00m;

			var jobCharges = new[] { chargeToImport1 };

			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = Job.PK;

			((APInvoice)invoice).ImportJobChargesIntoInvoice(jobCharges, line);
			Assert("Validation should not be suspended", !invoice.IsValidationSuspended);
		}

		public void TestIncrementChequeCurrentNumberForAutoCheque()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 100;
			testChequeBook.AK_CurrentNo = 110;
			testChequeBook.AK_LastNo = 200;
			testChequeBook.AK_AutoPrintCheque = true;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.ReceiptPaymentAK_AB = testChequeBook.PK;

			invoice.ReceiptPaymentAH_ChequeOrReference = "110";
			invoice.IncrementChequeCurrentNumber_ForTestOnly();
			AssertEquals("Current No should not be increased for auto print check book", 110m, testChequeBook.AK_CurrentNo);
		}

		public void TestIncrementChequeCurrentNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 100;
			testChequeBook.AK_CurrentNo = 110;
			testChequeBook.AK_LastNo = 200;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.ReceiptPaymentAK_AB = testChequeBook.PK;

			invoice.ReceiptPaymentAH_ChequeOrReference = "101";
			invoice.IncrementChequeCurrentNumber_ForTestOnly();
			AssertEquals("Current No should be 110", 110m, testChequeBook.AK_CurrentNo);

			invoice.ReceiptPaymentAH_ChequeOrReference = "110";
			invoice.IncrementChequeCurrentNumber_ForTestOnly();
			AssertEquals("Current No should be 111", 111m, testChequeBook.AK_CurrentNo);

			invoice.ReceiptPaymentAH_ChequeOrReference = "111";
			invoice.IncrementChequeCurrentNumber_ForTestOnly();
			AssertEquals("Current No should be 112", 112m, testChequeBook.AK_CurrentNo);

			invoice.ReceiptPaymentAH_ChequeOrReference = "200";
			invoice.IncrementChequeCurrentNumber_ForTestOnly();
			AssertEquals("Current No should be 112", 112m, testChequeBook.AK_CurrentNo);

			invoice.ReceiptPaymentAH_ChequeOrReference = "112.5";
			invoice.IncrementChequeCurrentNumber_ForTestOnly();
			AssertEquals("Cannot increment a cheque reference with decimal", 112m, testChequeBook.AK_CurrentNo);
		}

		public void TestIncrementChequeCurrentNumberQueryDB()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 100;
			testChequeBook.AK_CurrentNo = 110;
			testChequeBook.AK_LastNo = 200;

			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice.SubmittedFromInvoicingForm = true;
			testAPInvoice.IsInvoiceReceiptPayment = true;
			testAPInvoice.ReceiptPaymentAH_AB = testBank.PK;
			testAPInvoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			testAPInvoice.ReceiptPaymentAH_ChequeOrReference = "113";
			Factory.Save();

			ZQuery sQLFilter = new ZQuery(AccChequeBookSchema.AK_CurrentNo, 114);
			sQLFilter.AddToFilter(AccChequeBookSchema.PK, testChequeBook.PK);
			int count = new BusinessObjectFactory().GetDatabaseCount(typeof(AccChequeBook), sQLFilter);
			AssertEquals("Should have 1", 1, count);
		}

		public void TestJobRelatedLogicOnCopiedLine()
		{
			APInvoiceLine lineOld = (APInvoiceLine)GetTestInvoiceLine();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_IsCancelled = false;
			shipment.JS_IsForwardRegistered = true;
			Factory.Save(); // since it is a view
			lineOld.AL_JH = ObjectCreator.Job1.PK;
			APInvoiceLine lineNew = (APInvoiceLine)GetTestInvoiceLine();
			Assert("Precondition: Generic job on LineNew should not be Job1", lineNew.AL_JH != ObjectCreator.Job1.PK);
			APInvoice.JobRelatedLogicOnCopiedLine_ForTestOnly(lineOld, lineNew);
			AssertEquals("Generic job on LineNew should be Job1", ObjectCreator.Job1.PK, lineNew.AL_JH);
		}

		public void TestDefaultChargeCodeOnInvoiceLine()
		{
			APInvoice.Lines.RemoveAndDeleteAll();
			APInvoice.SubmittedFromInvoicingForm = true;

			OrgHeader orgWithDefaultCharge = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			AccChargeCode charge = orgWithDefaultCharge.Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_ChargeType = Constants.ChargeType.Margin;
			charge.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			orgWithDefaultCharge.CompanyData.OB_AC_APDefaultChargeCode = charge.PK;
			orgWithDefaultCharge.Factory.Save();

			OrgHeader orgWithoutDefaultCharge = Factory.NewWithValidTestData<OrgHeader>();

			APInvoice.AH_OH = orgWithoutDefaultCharge.PK;
			AssertEquals("There should be no lines", 0, APInvoice.Lines.Count);
			APInvoice.AH_OH = orgWithDefaultCharge.PK;
			APInvoice.AllowDefaultChargeCodeLineToBeAdded = false;
			AssertEquals("There should be no lines because invoice was not submitted from a form that sets AllowDefaultChargeCodeLineToBeAdded", 0, APInvoice.Lines.Count);

			APInvoice.AllowDefaultChargeCodeLineToBeAdded = true;
			APInvoice.SetIsReversing(true);
			APInvoice.AH_OH = ZGuid.Empty;
			APInvoice.AH_OH = orgWithDefaultCharge.PK;
			AssertEquals("There should be no lines because invoice is being reversed", 0, APInvoice.Lines.Count);

			APInvoice.AllowDefaultChargeCodeLineToBeAdded = true;
			APInvoice.SetIsReversing(false);
			APInvoice.AH_OH = ZGuid.Empty;
			APInvoice.AH_OH = orgWithDefaultCharge.PK;
			APInvoice.Lines[0].AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			AssertEquals("There should be one line", 1, APInvoice.Lines.Count);
			AssertEquals("The line should have the default charge code ", charge.PK, APInvoice.Lines[0].AL_AC);
			AssertNoErrors("There should be no errors on Generic Charge", APInvoice.Lines[0].GenericChargeInfo);

			APInvoice.AH_OH = orgWithoutDefaultCharge.PK;
			APInvoice.AH_OH = orgWithDefaultCharge.PK;
			AssertEquals("There should still be 1 line since charge is defaulted only when there are no lines", 1, APInvoice.Lines.Count);
		}

		protected override void AssertEmailSendStatusForUnpostedInvoice()
		{
			AssertEquals("No Email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public override void TestReceiptPaymentBankAccount()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			org.CompanyData.OB_AB_APDefaultBankAccount = bank.PK;

			APInvoice.SetIsReversing(false);
			APInvoice.SubmittedFromInvoicingForm = true;
			Assert("Default bank account is not the test Bank", bank.PK != APInvoice.DefaultBankAccount_ForTestOnly);
			APInvoice.AH_OH = org.PK;
			AssertEquals("Bank account should default to the test Bank", bank.PK, APInvoice.DefaultBankAccount_ForTestOnly);
		}

		public override void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be APInvoice", CargoWise.Definitions.BusinessContext.APInvoice, APInvoice.DocumentSupporter.BusinessContext);
		}

		#region PaymentIsGroupedWithInvoice Test

		public void TestCashPaymentGroupedWithInvoiceWhenPostedFromForm()
		{
			APInvoice.AH_ExchangeRate = 0.5m;
			APInvoice.AH_OSExTaxAmount = 40m;

			APInvoice.SubmittedFromInvoicingForm = true;
			APInvoice.IsInvoiceReceiptPayment = true;

			APInvoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
			APInvoice.AH_TransactionCount = 10;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice loadedAPInv = newFactory.Load<APInvoice>(APInvoice.PK);
			APPayment loadedAPPay = newFactory.Load<APPayment>(APInvoice.ReceiptPayment.PK);

			Assert("TransactionBelongsToGroup should not be empty on the saved APInvoice", !loadedAPInv.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("The saved APInvoice and APPayment should belong to the same group", loadedAPInv.AH_TransactionBelongsToGroup,
				loadedAPPay.AH_TransactionBelongsToGroup);
			AssertEquals("TransactionCount should be 1 (on the Invoice)", (byte)1, loadedAPInv.AH_TransactionCount);
			AssertEquals("TransactionCount should be 2 (on the Payment)", (byte)2, loadedAPPay.AH_TransactionCount);
			AssertEquals("PaymentForThisCashInvoice must load correct payment.", loadedAPPay, loadedAPInv.PaymentForThisCashInvoice);

			loadedAPPay.AH_IsCancelled = true;
			AssertNull("PaymentForThisCashInvoice must not load reversed payment.", loadedAPInv.PaymentForThisCashInvoice);
		}

		public void TestUnmatchCashInvoice()
		{
			AssertUnmatchCashInvoice(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchCashInvoice_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchCashInvoice(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchCashInvoice(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			APInvoice.AH_ExchangeRate = 0.5m;
			APInvoice.AH_OSExTaxAmount = 40m;

			APInvoice.SubmittedFromInvoicingForm = true;
			APInvoice.IsInvoiceReceiptPayment = true;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APInvoice loadedAPInv = newFactory.Load<APInvoice>(APInvoice.PK);
			APPayment loadedAPPay = newFactory.Load<APPayment>(APInvoice.ReceiptPayment.PK);

			Assert("Precondition: TransactionBelongsToGroup should not be empty on the saved APInvoice", !loadedAPInv.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("Precondition: The saved APInvoice and APPayment should belong to the same group", loadedAPInv.AH_TransactionBelongsToGroup,
				loadedAPPay.AH_TransactionBelongsToGroup);

			((IMatching)loadedAPInv).Unmatch(APInvoice.AH_OSExTaxAmount, APInvoice.AH_OSExTaxAmount);
			Assert("Invoice AH_TransactionBelongsToGroup must be empty after unmatching.", loadedAPInv.AH_TransactionBelongsToGroup.IsEmpty);
			Assert("Payment AH_TransactionBelongsToGroup must be empty after unmatching.", loadedAPPay.AH_TransactionBelongsToGroup.IsEmpty);
		}

		#endregion

		public void TestCashPaymentWithPaymentAddressOverride()
		{
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoice.SubmittedFromInvoicingForm = true;
			APInvoice.IsInvoiceReceiptPayment = true;

			var aPAddress = TestObjectCreator.CreateAddress(APInvoice.Header, OrgAddressType.Payables, true);
			var contact1 = TestObjectCreator.CreateContact(APInvoice.Header, "contact 1");

			APInvoice.ReceiptPaymentAddressOverride = aPAddress.PK;
			APInvoice.ReceiptPaymentContactOverride = contact1.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedAPInv = newFactory.Load<APInvoice>(APInvoice.PK);
			var loadedAPPay = newFactory.Load<APPayment>(APInvoice.ReceiptPayment.PK);

			AssertNotNull(loadedAPInv);
			AssertNotNull(loadedAPPay);

			AssertEquals(loadedAPPay.AH_OA_InvoiceAddressOverride, aPAddress.PK);
			AssertEquals(loadedAPPay.AH_OC_InvoiceContactOverride, contact1.PK);
		}

		#region Importing Accrual Tests

		#region TestImportingChargesRoundsFirstOSLineAmount

		public void TestImportingChargesRoundsFirstOSLineAmount()
		{
			var jobCharges = GetNewCharges(10M);

			APInvoice.AH_RX_NKTransactionCurrency = "USD";
			APInvoice.AH_ExchangeRate = 0.555555M;
			APInvoiceLine aPInvLine = (APInvoiceLine)APInvoice.Lines.AddNew();

			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPInvLine);
			AssertEquals("AH_OSTotalAmount on TransactionHeader should be 5.56 - rounds to 2 decimals", 5.56M, APInvoice.AH_OSTotalAmount);

			jobCharges = GetNewCharges(4.88888M);
			APInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			APInvoice.AH_ExchangeRate = 1M;
			AssertEquals("Exchange rate on Accrual should be 1", 1M, jobCharges[0].JR_OSCostExRate);
			AssertEquals("Exchange rate on line should be 1", 1M, aPInvLine.AL_ExchangeRate);

			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPInvLine);
			AssertEquals("Currency on the line should be Local Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, aPInvLine.AL_RX_NKTransactionCurrency);
			AssertEquals("AH_OSTotalAmount on TransactionHeader should be 4.89 - rounds to 2 decimals", 4.89M, APInvoice.AH_OSTotalAmount);
			AssertEquals("AL_OSAmount on APInvoiceLine should be 4.89 - rounds to 2 decimals", 4.89M, aPInvLine.AL_OSExTaxAmount);

			jobCharges = GetNewCharges(4.88M);
			APInvoice.AH_ExchangeRate = 1.01M;
			APInvoice.AH_RX_NKTransactionCurrency = "JPY";
			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPInvLine);
			AssertEquals("AH_OSTotalAmount on TransactionHeader should be 5 - rounds to 0 decimals", 5M, APInvoice.AH_OSTotalAmount);
			AssertEquals("AL_OSAmount on APInvoiceLine should be 5 - rounds to 0 decimals", 5M, aPInvLine.AL_OSExTaxAmount);

			RefCurrency oldCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			try
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "JPY";
				jobCharges = GetNewCharges(5.22M);
				APInvoice.AH_ExchangeRate = 0.78M;
				APInvoice.AH_RX_NKTransactionCurrency = ZString.Empty;
				APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPInvLine);
				AssertEquals("Overseas amount on APInvoiceLine should be 5 - rounds to 0 decimals", 5M, aPInvLine.AL_OSExTaxAmount);
				AssertEquals("Currency on APInvoiceLine should be JPY", "JPY", aPInvLine.AL_RX_NKTransactionCurrency);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = oldCurrency.RX_Code;
			}
		}

		List<Charge> GetNewCharges(ZDecimal oSExTaxAmount)
		{
			var charges = new List<Charge>();
			Charge charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_OSCostAmt = oSExTaxAmount;
			charges.Add(charge);
			return charges;
		}

		#endregion

		#region TestOSLineAmountRoundedWhenImportingMultipleAccruals

		public void TestOSLineAmountRoundedWhenImportingMultipleCharges()
		{
			Header = Factory.NewWithValidTestData<APInvoice>();
			var jobCharges = GetNewCharges(10M);
			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_OSCostAmt = 30.23M;
			jobCharges.Add(charge2);
			Charge charge3 = Factory.NewWithValidTestData<Charge>();
			charge3.JR_OSCostAmt = 49.90M;
			jobCharges.Add(charge3);

			APInvoice.AH_RX_NKTransactionCurrency = "JPY";
			APInvoice.AH_ExchangeRate = 11M;
			APInvoiceLine aPLine = (APInvoiceLine)APInvoice.Lines.AddNew();

			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPLine);

			AssertEquals("There should be 3 lines in the APInvoice", 3, APInvoice.Lines.Count);
			AssertEquals("Amount on First Line should be 110", 110M, APInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Amount on Second Line should be 333", 333M, APInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("Amount on Third Line should be 549", 549M, APInvoice.Lines[2].AL_OSExTaxAmount);

			Header = Factory.NewWithValidTestData<APInvoice>();

			jobCharges = GetNewCharges(10.7777M);
			charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_OSCostAmt = 20.3333M;
			jobCharges.Add(charge2);
			charge3 = Factory.NewWithValidTestData<Charge>();
			charge3.JR_OSCostAmt = 49.565656M;
			jobCharges.Add(charge3);

			APInvoice.AH_RX_NKTransactionCurrency = ZString.Empty;
			aPLine = (APInvoiceLine)APInvoice.Lines.AddNew();

			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPLine);

			AssertEquals("There should be 3 lines on the APInvoice", 3, APInvoice.Lines.Count);
			AssertEquals("Amount on first line should be 10.78", 10.78M, APInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("Amount on second line should be 20.33", 20.33M, APInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("Amount on third line should be 49.57", 49.57M, APInvoice.Lines[2].AL_OSExTaxAmount);
		}

		#endregion

		#region TestImportingChargesWithTaxOrgs

		public void TestImportingChargesSetsTaxIDAndAmount()
		{
			ImportingChargesWithTaxOrganisationCore(true);
			AssertEquals("Tax ID should be set on first line", TaxRate.AT_Code, APInvoice.Lines[0].TaxRate.AT_Code);
			AssertEquals("Tax Amount should be set on first line", 5m, APInvoice.Lines[0].AL_OSTaxAmount);

			AssertEquals("Tax ID should be defaulted as normal on second line", TaxRate2.PK, APInvoice.Lines[1].AL_AT);
			AssertEquals("Tax Amount should defaulted as normal on second line", 20m, APInvoice.Lines[1].AL_OSTaxAmount);
		}

		public void TestImportingChargesDoesNotSetTaxIDAndAmount()
		{
			ImportingChargesWithTaxOrganisationCore(false);
			Assert("Tax ID should not be set on first line", APInvoice.Lines[0].AL_AT.IsEmpty);
			AssertEquals("Tax Amount should not be set on first line", 0m, APInvoice.Lines[0].AL_OSTaxAmount);

			Assert("Tax ID should not be set on second line", APInvoice.Lines[1].AL_AT.IsEmpty);
			AssertEquals("Tax Amount should not be set on second line", 0m, APInvoice.Lines[0].AL_OSTaxAmount);
		}

		void ImportingChargesWithTaxOrganisationCore(bool aPTaxApplicable)
		{
			var orgTax = Factory.NewWithValidTestData<OrgHeader>();
			orgTax.CompanyData.SetAPTaxApplicable(aPTaxApplicable);

			ChargeCode.AC_AT_GSTRate = TaxRate.PK;
			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = orgTax.PK;
			var firstLine = (APInvoiceLine)APInvoice.Lines.AddNew();

			var jobCharges = GetNewCharges(0m);
			var charge1 = jobCharges[0];
			charge1.JR_AC = ChargeCode.PK;
			charge1.JR_OSCostAmt = 5;
			firstLine.AL_JH = charge1.JR_JH;

			var charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_AC = ChargeCode.PK;
			charge2.JR_OH_CostAccount = orgTax.PK;
			charge2.JR_OSCostAmt = 200;
			jobCharges.Add(charge2);

			if (aPTaxApplicable)
			{
				charge1.JR_OH_CostAccount = orgTax.PK;
				charge2.JR_AT_CostGSTRate = TaxRate2.PK;
				AssertEquals("Charge1 TaxRate", TaxRate.AT_Code, charge1.CostGSTRate.AT_Code);
				AssertEquals("Charge2 TaxRate", TaxRate2.AT_Code, charge2.CostGSTRate.AT_Code);
			}
			else
			{
				Assert("Charge1 TaxRate should be empty", charge1.JR_AT_CostGSTRate.IsEmpty);
				Assert("Charge2 TaxRate should be empty", charge2.JR_AT_CostGSTRate.IsEmpty);
			}

			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), firstLine);
		}

		public void TestIsGSTMandatory()
		{
			bool originalGSTMandatory = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				Header = Factory.NewWithValidTestData<APInvoice>();
				Assert("GST should not be mandatory", !APInvoice.IsApportionmentGSTMandatory_ForTestOnly);

				APInvoice.AH_OH = org.PK;
				Assert("GST should not be mandatory", !APInvoice.IsApportionmentGSTMandatory_ForTestOnly);

				org.CompanyData.SetAPTaxApplicable(true);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Assert("GST should not be mandatory", !APInvoice.IsApportionmentGSTMandatory_ForTestOnly);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				Assert("GST should be mandatory", APInvoice.IsApportionmentGSTMandatory_ForTestOnly);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalGSTMandatory;
			}
		}

		#endregion

		#region TestImportAccrualAmountIntoLine

		public void TestImportChargesAmountIntoLine_DifferentCurrency_ZeroHeaderExchangeRate()
		{
			var headerCurrency = TestObjectCreator.USD;
			var jobChargeCurrency = TestObjectCreator.AUD;

			APInvoiceLine line = Factory.NewWithValidTestData<APInvoiceLine>();

			Charge jobCharge = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_OSCostAmt = 10.46564M;
			jobCharge.JR_RX_NKCostCurrency = jobChargeCurrency.RX_Code;

			APInvoice.Lines.Add(line);
			APInvoice.AH_RX_NKTransactionCurrency = headerCurrency.RX_Code;
			APInvoice.AH_ExchangeRate = 0M;
			APInvoice.ImportJobChargeAmountIntoLine_ForTestOnly(jobCharge, line);
			AssertEquals("Line currency is same as header currency even if header exchange rate is 0", headerCurrency.RX_Code, line.AL_RX_NKTransactionCurrency);
			AssertEquals("Line exchage rate is 0", 0M, line.AL_ExchangeRate);
			AssertEquals("As it is in header", 0M, APInvoice.AH_ExchangeRate);

			APInvoice.AH_ExchangeRate = 0.871M;
			AssertEquals("Line exchange rate is updated with header exchange rate", APInvoice.AH_ExchangeRate, line.AL_ExchangeRate);
		}

		public void TestImportChargesAmountIntoLine()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			currency1.RX_Code = "EWQ";
			currency1.RX_SubUnitRatio = 0;
			Factory.Save();

			APInvoiceLine line = Factory.NewWithValidTestData<APInvoiceLine>();

			Charge jobCharge = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_OSCostAmt = 10.46564M;

			APInvoice.Lines.Add(line);
			APInvoice.AH_RX_NKTransactionCurrency = currency1.RX_Code;
			APInvoice.AH_ExchangeRate = 20M;
			APInvoice.ImportJobChargeAmountIntoLine_ForTestOnly(jobCharge, line);
			AssertEquals("Amount on Line should be 209", 209M, line.AL_OSExTaxAmount);

			line = Factory.NewWithValidTestData<APInvoiceLine>();
			APInvoice.Lines.Add(line);
			APInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			jobCharge = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_OSCostAmt = 10.11111M;

			APInvoice.AH_ExchangeRate = 0M;
			APInvoice.ImportJobChargeAmountIntoLine_ForTestOnly(jobCharge, line);
			AssertEquals("Amount on Line should be 10.11", 10.11M, line.AL_OSExTaxAmount);
		}

		public void TestImportChargesAmountIntoLine_CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency()
		{
			var currency = TestObjectCreator.USD;

			var line = Factory.NewWithValidTestData<APInvoiceLine>();

			var jobCharge = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_OSCostAmt = 100M;
			jobCharge.JR_RX_NKCostCurrency = currency.RX_Code;
			jobCharge.JR_OSCostExRate = 1.3M;

			APInvoice.Lines.Add(line);
			APInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			APInvoice.AH_ExchangeRate = 1.4M;

			AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			APInvoice.ImportJobChargeAmountIntoLine_ForTestOnly(jobCharge, line);
			AssertEquals("CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is true.", true, AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Value);
			AssertEquals("Line currency is same as charge currency", line.AL_RX_NKTransactionCurrency, jobCharge.JR_RX_NKCostCurrency);
			AssertEquals("Line OS amount is same as charge OS amount when CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is true and line currency is same as charge currency.", jobCharge.JR_OSCostAmt, line.AL_OSExTaxAmount);
			AssertNotEquals("Line local amount is different from charge local amount when CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is true and line currency is same as charge currency.", jobCharge.JR_LocalCostAmt, line.AL_LocalExTaxAmount);

			AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			APInvoice.ImportJobChargeAmountIntoLine_ForTestOnly(jobCharge, line);
			AssertEquals("CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is false.", false, AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Value);
			AssertEquals("Line currency is same as charge currency", line.AL_RX_NKTransactionCurrency, jobCharge.JR_RX_NKCostCurrency);
			AssertNotEquals("Line OS amount is different from charge OS amount when CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is false.", jobCharge.JR_OSCostAmt, line.AL_OSExTaxAmount);
			AssertEquals("Line local amount is same as charge local amount when CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is false.", jobCharge.JR_LocalCostAmt, line.AL_LocalExTaxAmount);

			AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			APInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.CNY.RX_Code;
			APInvoice.ImportJobChargeAmountIntoLine_ForTestOnly(jobCharge, line);
			AssertEquals("CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency is true.", true, AccountingConfigurationRegistry.Instance.CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency.Value);
			AssertNotEquals("Line currency is different from charge currency", line.AL_RX_NKTransactionCurrency, jobCharge.JR_RX_NKCostCurrency);
			AssertNotEquals("Line OS amount is different from charge OS amount when line currency is different from charge currency.", jobCharge.JR_OSCostAmt, line.AL_OSExTaxAmount);
			AssertEquals("Line local amount is same as charge local amount when line currency is different from charge currency.", jobCharge.JR_LocalCostAmt, line.AL_LocalExTaxAmount);
		}

		#endregion

		#region ImportFirstAccrualUsesTaxOverride Test

		public void TestImportChargesUsesTaxOverride()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.SetAPTaxApplicable(true);

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var jobCharges = GetNewCharges(12m);
			jobCharges[0].JR_OH_CostAccount = org.PK;
			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			jobCharges.Add(charge2);

			AccTaxRate taxRateOverride = Factory.NewWithValidTestData<AccTaxRate>();

			AccTaxRate taxRateOriginal = Factory.NewWithValidTestData<AccTaxRate>();
			chargeCode1.AC_AT_GSTRate = taxRateOriginal.PK;
			chargeCode2.AC_AT_GSTRate = taxRateOriginal.PK;
			jobCharges[0].JR_AC = chargeCode1.PK;
			charge2.JR_AC = chargeCode2.PK;

			jobCharges[0].JR_AT_CostGSTRate = taxRateOverride.PK;
			charge2.JR_AT_CostGSTRate = taxRateOverride.PK;

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = (APInvoiceLine)APInvoice.Lines.AddNew();

			jobCharges[0].JR_OH_CostAccount = org.PK;
			charge2.JR_OH_CostAccount = org.PK;

			bool originalGSTApplicable = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			try
			{
				APInvoice.AH_OH = org.PK;
				AssertEquals("Precondition: There should be 1 line on the invoice", 1, APInvoice.Lines.Count);
				APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), line);
				AssertEquals("There should be 2 lines on the invoice", 2, APInvoice.Lines.Count);
				AssertEquals("TaxRate on first line should be the OverrideRate, not OriginalRate on the ChargeCode", taxRateOverride.PK, line.AL_AT);
				AssertEquals("TaxRate on second line should be the OverrideRate, not the OriginalRate on the ChargeCode", taxRateOverride.PK, line.AL_AT);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalGSTApplicable;
			}
		}

		public void TestTaxRegistrationSubTypeForMexicoXCLInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

				InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				line1.AL_AT = TestObjectCreator.ExcludedTax.PK;
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();
				AssertEquals(MexicoComplianceInfo.ComplianceSubTypeCodes.XCL, invoice.AH_ComplianceSubType);

				var creditNote = Factory.NewWithValidTestData<APCreditNote>();
				var line2 = (InvoicingLineBase)creditNote.Lines.AddNew();
				line2.AL_AT = TestObjectCreator.ExcludedTax.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;

				var reverseInvoice = Factory.NewWithValidTestData<APInvoice>();
				var line3 = (InvoicingLineBase)reverseInvoice.Lines.AddNew();
				line3.AL_AT = TestObjectCreator.ExcludedTax.PK;
				line3.AL_AG = TestObjectCreator.GLHeader1.PK;

				reverseInvoice.AH_TransactionBelongsToGroup = creditNote.PK;
				reverseInvoice.OriginalTransaction = invoice;

				AssertEquals("Precondition: AH_ComplianceSubType has not been calculated yet", string.Empty, reverseInvoice.AH_ComplianceSubType);

				Factory.Save();
				AssertEquals("AH_ComplianceSubType should be XCL, for reversed invoice", MexicoComplianceInfo.ComplianceSubTypeCodes.XCL, reverseInvoice.AH_ComplianceSubType);
			}
		}

		public void TestTaxRegistrationSubTypeForMexicoTXIInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_TransactionNum = "APINV001";

				var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				line1.AL_AT = TaxRate2.PK;
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();

				AssertEquals("AH_ComplianceSubType should be TXI for original invoice", MexicoComplianceInfo.ComplianceSubTypeCodes.TXI, invoice.AH_ComplianceSubType);

				var creditNote = Factory.NewWithValidTestData<APCreditNote>();
				creditNote.AH_TransactionNum = "APCRD001";

				var line2 = (InvoicingLineBase)creditNote.Lines.AddNew();
				line2.AL_AT = TaxRate2.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;

				Factory.Save();

				var reverseInvoice = TestObjectCreator.CreateReversalTransaction(creditNote, "", "APINV002") as InvoicingBase;

				AssertEquals("Precondition: AH_ComplianceSubType has not been calculated yet", string.Empty, reverseInvoice.AH_ComplianceSubType);

				Factory.Save();

				AssertEquals("AH_ComplianceSubType should be empty, for reversal invoice", MexicoComplianceInfo.ComplianceSubTypeCodes.TDR, reverseInvoice.AH_ComplianceSubType);
			}
		}

		#endregion

		#region ImportChargeSetsLocalAmountAndCurrency Test

		public void TestImportChargeSetsLocalAmountAndCurrency()
		{
			RefCurrency oSCurrency = Factory.NewWithValidTestData<RefCurrency>();
			oSCurrency.RX_Code = "QWE";
			oSCurrency.RX_SubUnitRatio = 100;
			Factory.Save();
			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_RX_NKTransactionCurrency = oSCurrency.RX_Code;
			APInvoice.AH_ExchangeRate = 0.5m;

			var jobCharges = GetNewCharges(12m);
			Charge charge2 = Factory.NewWithValidTestData<Charge>();
			charge2.JR_OSCostAmt = 15m;
			jobCharges.Add(charge2);
			Charge charge3 = Factory.NewWithValidTestData<Charge>();
			charge3.JR_OSCostAmt = 18m;
			jobCharges.Add(charge3);

			APInvoiceLine aPLine = (APInvoiceLine)APInvoice.Lines.AddNew();
			APInvoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), aPLine);
			AssertEquals("There should be 3 lines", 3, APInvoice.Lines.Count);
			APInvoice.Lines.Sort(AccTransactionLinesSchema.AL_OSAmount.Name, System.ComponentModel.ListSortDirection.Ascending);

			AssertEquals("OSExTax on the first line should be 9", 9m, APInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("LocalAmount on the first line should be 18", 18m, APInvoice.Lines[0].AL_LocalExTaxAmount);
			AssertEquals("Currency on the first line should be OSCurrency", oSCurrency.RX_Code, APInvoice.Lines[0].AL_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate on the first line should be 0.5", 0.5m, APInvoice.Lines[0].AL_ExchangeRate);

			AssertEquals("OSExTax on the second line should be 7.5", 7.5m, APInvoice.Lines[1].AL_OSExTaxAmount);
			AssertEquals("LocalAmount on the second line should be 15", 15m, APInvoice.Lines[1].AL_LocalExTaxAmount);
			AssertEquals("Currency on the second line should be OSCurrency", oSCurrency.RX_Code, APInvoice.Lines[1].AL_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate on the second line should be 0.5", 0.5m, APInvoice.Lines[1].AL_ExchangeRate);

			AssertEquals("OSExTax on the third line should be 6", 6m, APInvoice.Lines[2].AL_OSExTaxAmount);
			AssertEquals("LocalAmount on the third line should be 12", 12m, APInvoice.Lines[2].AL_LocalExTaxAmount);
			AssertEquals("Currency on the third line should be OSCurrency", oSCurrency.RX_Code, APInvoice.Lines[2].AL_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate on the third line should be 0.5", 0.5m, APInvoice.Lines[2].AL_ExchangeRate);
		}

		#endregion

		#endregion

		#region TestChequeBookReadOnlyLogic

		public void TestChequeBookReadOnlyLogic()
		{
			Assert("Precondition: ChequeBook is editable", !APInvoice.ReceiptPaymentAK_ABInfo.ReadOnly);
			APInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			Assert("ChequeBook should not be available for payments by CreditCard", APInvoice.ReceiptPaymentAK_ABInfo.ReadOnly);
			APInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			Assert("ChequeBook should not be available for DirectDebit Payments", APInvoice.ReceiptPaymentAK_ABInfo.ReadOnly);

			APInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("ChequeBook should be editable for Cheque Payments", !APInvoice.ReceiptPaymentAK_ABInfo.ReadOnly);
			APInvoice.ReceiptPaymentAK_AB = TestObjectCreator.AUDChequeBook.PK;
			APInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("ChequeBook should be cleared", APInvoice.ReceiptPaymentAK_AB.IsEmpty);
		}

		#endregion

		protected void TestCalc_ChequeNumberIsAutoAllocatedLabel()
		{
			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBankAccount, 1, 3, 2);
			APInvoice invoice = Factory.New<APInvoice>();
			Assert("Default state", invoice.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			invoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", invoice.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("IsAutoAllocationEnabled should return False", !((IChequeNumberAutoAllocation)invoice).IsAutoAllocationEnabled);

			invoice.ReceiptPaymentAK_AB = testBookWithAutoAllocation.PK;
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, invoice.Calc_ChequeNumberIsAutoAllocatedLabel);
			Assert("IsAutoAllocationEnabled should return True", ((IChequeNumberAutoAllocation)invoice).IsAutoAllocationEnabled);
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Payment type not 'Cheque'", invoice.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			invoice.ReceiptPaymentAK_AB = testBookWithAutoAllocation.PK;
			AssertEquals("Pyment type is 'Cheque' again", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, invoice.Calc_ChequeNumberIsAutoAllocatedLabel);
			invoice.FImportedHotCheque_ForTestOnly = Factory.NewWithValidTestData<AccHotCheque>();
			Assert("Hot cheque is imported, won't auto allocate", invoice.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			invoice.FImportedHotCheque_ForTestOnly = null;
			AssertEquals("Hot cheque is not imported, should auto allocate", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, invoice.Calc_ChequeNumberIsAutoAllocatedLabel);
			invoice.ReceiptPaymentAH_ChequeOrReference = "123";
			Assert(invoice.IsChequeNumberAutoAllocated);
			Assert("ReceiptPaymentAH_ChequeOrReference is not empty, label should be empty", invoice.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
		}

		public void TestCalc_ChequeIsAutoPrintedLabel()
		{
			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBankAccount, 1, 3, 2);
			APInvoice invoice = Factory.New<APInvoice>();
			Assert("Label should be empty so far", invoice.Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			invoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", invoice.Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			invoice.ReceiptPaymentAK_AB = testBookWithAutoAllocation.PK;
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, invoice.Calc_ChequeIsAutoPrintedLabel);
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Payment type not 'Cheque'", invoice.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			invoice.ReceiptPaymentAK_AB = testBookWithAutoAllocation.PK;
			AssertEquals("Pyment type is 'Cheque' again", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, invoice.Calc_ChequeIsAutoPrintedLabel);
			invoice.FImportedHotCheque_ForTestOnly = Factory.NewWithValidTestData<AccHotCheque>();
			Assert("Hot cheque is imported, won't auto print", invoice.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
			invoice.FImportedHotCheque_ForTestOnly = null;
			AssertEquals("Hot cheque is not imported, should auto print", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, invoice.Calc_ChequeIsAutoPrintedLabel);
			invoice.ReceiptPaymentAH_ChequeOrReference = "123";
			Assert(invoice.IsChequeNumberAutoAllocated);
			Assert("ReceiptPaymentAH_ChequeOrReference is not empty, label should be empty", invoice.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
		}

		public void TestSettingReceiptPaymentAK_ABWillResetChequeNo()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBankAccount, 1, 3, 2);
			AccChequeBook testBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			testBook.AK_CurrentNo = 2;
			invoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			invoice.ReceiptPaymentAK_AB = testBook.PK;
			AssertEquals(invoice.ReceiptPaymentAH_ChequeOrReference, "2");
			Assert("Cheque number should not be read only", !invoice.ReceiptPaymentAH_ChequeOrReferenceInfo.ReadOnly);

			testBookWithAutoAllocation.AK_CurrentNo = 1;
			invoice.ReceiptPaymentAK_AB = testBookWithAutoAllocation.PK;
			Assert("Cheque number should be cleared", invoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
			AssertEquals("Cheque book current NO should remain old", 1m, testBookWithAutoAllocation.AK_CurrentNo);
			Assert("Cheque number field should become read only", invoice.ReceiptPaymentAH_ChequeOrReferenceInfo.ReadOnly);

			invoice.ReceiptPaymentAK_AB = testBook.PK;
			AssertEquals(invoice.ReceiptPaymentAH_ChequeOrReference, "2");
			Assert("Cheque number field should become editable again", !invoice.ReceiptPaymentAH_ChequeOrReferenceInfo.ReadOnly);
		}

		#region IChequeNumberAutoAllocation Members Tests

		public void TestIChequeNumberAutoAllocation_ChequeBookPK()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AssertNull("Should return empty cheque book", ((IChequeNumberAutoAllocation)invoice).ChequeBook);
			invoice.ReceiptPaymentAK_AB = chequeBook.PK;
			AssertEquals("Should return fTestChequeBook PK", chequeBook, ((IChequeNumberAutoAllocation)invoice).ChequeBook);
		}

		public void TestIChequeNumberAutoAllocation_AssignChequeNumber()
		{
			SetUpInvoiceForSaving();
			((IChequeNumberAutoAllocation)APInvoice).AssignChequeNumber("000123");
			AssertEquals("ReceiptPaymentAH_ChequeOrReference should be set", "000123", APInvoice.ReceiptPaymentAH_ChequeOrReference);
			Factory.Save();
			AssertNotNull("Receipt should be created", APInvoice.ReceiptPayment);
			AssertEquals("Cheque number should be passed to the payment", "000123", APInvoice.ReceiptPayment.AH_ChequeOrReference);
		}

		public void TestIChequeNumberAutoAllocation_IsAllocationPerformed()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			Assert("Should return False by defualt", !((IChequeNumberAutoAllocation)invoice).IsAllocationPerformed);
			((IChequeNumberAutoAllocation)invoice).AssignChequeNumber("123");
			Assert(((IChequeNumberAutoAllocation)invoice).IsAllocationPerformed);
		}

		public void TestIChequeNumberAutoAllocation_Printing_ObjectPK()
		{
			SetUpInvoiceForSaving();
			AssertEquals("Should return Null as the payment is not yet created", Guid.Empty, ((IChequeNumberAutoAllocation)APInvoice).Printing_ObjectPK);
			Factory.Save();
			AssertEquals(APInvoice.ReceiptPayment.PK, ((IChequeNumberAutoAllocation)APInvoice).Printing_ObjectPK);
		}

		public void TestIChequeNumberAutoAllocation_Printing_PrinterPK()
		{
			BusinessObject stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_SQ = stmPrintQueue.PK;
			AssertEquals("Should return empty cheque book", ZGuid.Empty, ((IChequeNumberAutoAllocation)APInvoice).Printing_PrinterPK);
			APInvoice.ReceiptPaymentAK_AB = chequeBook.PK;
			AssertEquals("Should return fTestChequeBook PK", stmPrintQueue.PK, ((IChequeNumberAutoAllocation)APInvoice).Printing_PrinterPK);
		}

		public void TestIChequeNumberAutoAllocation_ChequeIsAutoPrinted()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			Assert("Default value", !((IChequeNumberAutoAllocation)invoice).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)invoice).ChequeIsAutoPrinted = ZBool.True;
			Assert("Value should be changed", ((IChequeNumberAutoAllocation)invoice).ChequeIsAutoPrinted);
		}

		public void TestIChequeNumberAutoAllocation_AllocationOrPrintingFailed()
		{
			SetUpInvoiceForSaving();
			APInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			Factory.Save();
			APInvoice.ChequeBook.AK_IsActive = ZBool.True;
			((IChequeNumberAutoAllocation)APInvoice).AllocationOrPrintingFailed();
			Assert("ChequeNumber should be reset", APInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
			Assert("ChequeNumber should be reset on payment as well", APInvoice.ReceiptPayment.AH_ChequeOrReference.IsEmpty);
			Assert("Cheque book should be reloaded", !APInvoice.ChequeBook.AK_IsActive);
		}

		public override void TestSetReceiptPaymentAK_AB()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(testBank, 1, 3, 2);
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 2;

			Invoice testInvoice = Factory.New(GetExpectedBusinessObjectType()) as Invoice;
			testInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;

			testInvoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			AssertEquals("Cheque Number should be populated", "2", testInvoice.ReceiptPaymentAH_ChequeOrReference);
			Assert("Cheque Number should not be readonly", !testInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo.ReadOnly);

			testInvoice.ReceiptPaymentAK_AB = testBookWithAutoAllocation.PK;
			Assert("Cheque number should be reset", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
			Assert("Cheque number should be read only", testInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo.ReadOnly);
		}

		public override void TestSettingDrawerDetailsForInvoice()
		{
			base.TestSettingDrawerDetailsForInvoice();
			AssertEquals("Cheque Drawer is not set", TestInvoice.ReceiptPayment.AH_ChequeDrawer, ZString.Empty);
			AssertEquals("Drawer Bank is not set", TestInvoice.ReceiptPayment.AH_DrawerBank, ZString.Empty);
			AssertEquals("Drawer Branch is not set", TestInvoice.ReceiptPayment.AH_DrawerBranch, ZString.Empty);
		}

		public override void TestCheckBankAccountAndCheckBookValidationOnChangingPaymentType()
		{
			base.TestCheckBankAccountAndCheckBookValidationOnChangingPaymentType();
			TestInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

			AssertHasNotifications("Please enter a Check Book.", TestInvoice.ReceiptPaymentAK_ABInfo);
			AssertHasNotifications("Please enter a value.", TestInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
		}

		#endregion

		public override void TestNumberFountainInternalRef()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			ShareSequentialReferenceNumbers item = new ShareSequentialReferenceNumbers();
			item.Value = true;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain(), invoice.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APInvoiceNo.GetTodaysPeriodFountain(), invoice.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			item.Value = false;
			AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceReferenceNumbers.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, item);
			AssertEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APInvoiceInternalRef.GetTodaysPeriodFountain(), invoice.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
			AssertNotEquals("Should be APInvoiceInternalRef number fountain", Env.NumberFountains.APInvoiceNo.GetTodaysPeriodFountain(), invoice.NumberFountainForInternalRef_ForTestOnly.numberFountainPooler.GetTodaysPeriodFountain());
		}

		#region LevelAuthorization

		protected void TestLevelAuthorizationBase()
		{
			base.TestLevelAuthorization();
		}

		public override void TestLevelAuthorization()
		{
			SetUpLevelAuthorization();

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			AssertCorrectAuthorisationRequired(false, false, null, false);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = true;

			AssertCorrectAuthorisationRequired(false, false, null, false);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			AssertCorrectAuthorisationRequired(false, false, null, false);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = true;

			AssertCorrectAuthorisationRequired(false, false, null, false);

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVariance;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo2.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above2 = valuesForTest.AuthorisationRequirements.AddNew();
			above2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above2.Range = RangeCodes.Above;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			//variance < 0
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			upTo1.Amount = 1M;
			upTo2.Amount = 3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, null, false);

			//variance = 1.5152%
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			upTo1.Amount = 2M;
			upTo2.Amount = 3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, upTo1, false);

			upTo1.Amount = 1M;
			upTo2.Amount = 2M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);

			upTo1.Amount = 1M;
			upTo2.Amount = 1.5M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = true;

			upTo1.Amount = 2M;
			upTo2.Amount = 3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, upTo1, false);

			upTo1.Amount = 1M;
			upTo2.Amount = 2M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);

			upTo1.Amount = 1M;
			upTo2.Amount = 1.5M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, false);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			upTo1.Amount = 2M;
			upTo2.Amount = 3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, upTo1, false);

			upTo1.Amount = 1M;
			upTo2.Amount = 2M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, false);

			upTo1.Amount = 1M;
			upTo2.Amount = 1.5M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = true;

			upTo1.Amount = 2M;
			upTo2.Amount = 3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, upTo1, false);

			upTo1.Amount = 1M;
			upTo2.Amount = 2M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, false);

			upTo1.Amount = 1M;
			upTo2.Amount = 1.5M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, false);
		}

		public void TestLevelAuthorization_LocalCostAmount()
		{
			SetUpLevelAuthorization();

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo2.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above2 = valuesForTest.AuthorisationRequirements.AddNew();
			above2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above2.Range = RangeCodes.Above;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			//variance = $20
			upTo1.Amount = 20M;
			upTo2.Amount = 30M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, upTo1, false);

			upTo1.Amount = 18M;
			upTo2.Amount = 19M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);

			//variance = $50
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.JobAndChargeCode;
			upTo1.Amount = 20M;
			upTo2.Amount = 50M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);

			upTo1.Amount = 20M;
			upTo2.Amount = 49M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);
		}

		public void TestLevelAuthorization_PercentageVariance()
		{
			SetUpLevelAuthorization();

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVariance;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo2.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above2 = valuesForTest.AuthorisationRequirements.AddNew();
			above2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above2.Range = RangeCodes.Above;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			//variance = 1.5384%
			upTo1.Amount = 2M;
			upTo2.Amount = 3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, upTo1, false);

			//variance = 8.3333%
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.JobAndChargeCode;
			upTo1.Amount = 4M;
			upTo2.Amount = 8.4M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);

			upTo1.Amount = 4M;
			upTo2.Amount = 8.3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);

			APInvoice.Lines[1].AL_LocalExTaxAmount = 500M;
			APInvoice.Lines[2].AL_LocalExTaxAmount = 710M;
			//variance = 25%
			//difference = $100 - don't affect
			upTo1.Amount = 4M;
			upTo1.LocalCostAmount = 200;
			upTo2.Amount = 25M;
			upTo2.LocalCostAmount = 100;
			above2.Amount = upTo2.Amount;
			above2.LocalCostAmount = 10;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);

			upTo1.Amount = 4M;
			upTo1.LocalCostAmount = 200;
			upTo2.Amount = 25M;
			upTo2.LocalCostAmount = 99;
			above2.Amount = upTo2.Amount;
			above2.LocalCostAmount = 10;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);
		}

		public void TestLevelAuthorization_VarianceWithMaximum()
		{
			SetUpLevelAuthorization();

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo1.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo2.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above2 = valuesForTest.AuthorisationRequirements.AddNew();
			above2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above2.Range = RangeCodes.Above;

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			//variance = 8.3333%
			//difference = $50
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.JobAndChargeCode;
			upTo1.Amount = 4M;
			upTo1.LocalCostAmount = 100;
			upTo2.Amount = 8.4M;
			upTo2.LocalCostAmount = 100;
			above2.Amount = upTo2.Amount;
			above2.LocalCostAmount = 100;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo2, true);

			upTo1.Amount = 4M;
			upTo2.Amount = 8.3M;
			above2.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);

			APInvoice.Lines[1].AL_LocalExTaxAmount = 500M;
			APInvoice.Lines[2].AL_LocalExTaxAmount = 710M;
			//variance = 25%
			//difference = $100
			upTo1.Amount = 4M;
			upTo1.LocalCostAmount = 100;
			upTo2.Amount = 25M;
			upTo2.LocalCostAmount = 100;
			above2.Amount = upTo2.Amount;
			above2.LocalCostAmount = 100;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above2, true);

			//UpTo1.Amount = 4M;
			//UpTo1.LocalCostAmount = 200;
			//UpTo2.Amount = 25M;
			//UpTo2.LocalCostAmount = 99;
			//Above2.Amount = UpTo2.Amount;
			//Above2.LocalCostAmount = 10;
			//AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValuesForTest);
			//AssertCorrectAuthorisationRequired(false, true, Above2, true);

			//UpTo1.Amount = 4M;
			//UpTo1.LocalCostAmount = 10;
			//UpTo2.Amount = 10M;
			//UpTo2.LocalCostAmount = 50;
			//Above2.Amount = UpTo2.Amount;
			//Above2.LocalCostAmount = 80;
			//AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ValuesForTest);
			//AssertCorrectAuthorisationRequired(false, true, Above2, true);
		}

		public void TestLevelAuthorization_ExtremeValues()
		{
			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo = valuesForTest.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above.Range = RangeCodes.Above;

			//valid charges and empty invoice
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			SetUpLevelAuthorization_Charges(job);

			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;

			upTo.Amount = 100M;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, false, null, false);

			//no charges and valid invoice
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));

			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 100M;
			line.AL_OSExTaxAmount = 100M;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			AssertCorrectAuthorisationRequired(true, false, upTo, true);

			upTo.Amount = 99M;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above, true);

			//valid charges and invoice with zero amount line
			valuesForTest.VarianceCalculationStyle = CostVarianceCalculationStyle.PercentageVariance;
			upTo.Amount = 99M;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001003"));
			SetUpLevelAuthorization_Charges(job);

			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 0M;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			AssertCorrectAuthorisationRequired(false, false, null, false);

			//charge with zero amount and valid invoice
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001004"));
			Charge charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 0M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 100M;
			line.AL_OSExTaxAmount = 100M;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			AssertCorrectAuthorisationRequired(false, true, above, true);

			upTo.Amount = 100M;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above, true);

			//charge with zero amount and invoice with zero amount
			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001005"));
			charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 0M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 0M;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			AssertCorrectAuthorisationRequired(false, false, null, false);
		}

		public void TestLevelAuthorization_ProccessOnlyNotPostedCharges()
		{
			Env.Security.CostVarianceApprovalLevel1.IsAllowed = false;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo = valuesForTest.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo.Range = RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above.Range = RangeCodes.Above;

			AssertNotNull(SYD);
			AssertNotNull(FEA);
			AssertNotNull(CAF);

			TestObjectCreator.Creditor1.CompanyData.SetAPTaxApplicable(false);
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			Charge charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_AT_CostGSTRate = ZGuid.Empty;
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;
			InvoiceLine costLine = GetTestInvoiceLine();
			costLine.AL_OSAmount = costLine.AL_LineAmount = -charge.JR_OSCostAmt;
			charge.JR_AL_APLine = costLine.PK;

			Factory.Save();

			Header = Factory.NewWithValidTestData<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 100M;

			upTo.Amount = 100M;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(true, false, upTo, true);

			upTo.Amount = 99M;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AssertCorrectAuthorisationRequired(false, true, above, true);
		}

		void SetUpLevelAuthorization()
		{
			Job job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));
			SetUpLevelAuthorization_Charges(job1);

			Job job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));
			SetUpLevelAuthorization_Charges(job2);

			Job notInvolvedJob = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001003"));
			SetUpLevelAuthorization_Charges(notInvolvedJob);

			Factory.Save();

			Header = Factory.New<APInvoice>();
			APInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			//job1 lines
			APInvoiceLine line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job1.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 250M;

			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job1.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = FRT.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 420M;

			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job1.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = BAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 650M;

			//job2 lines
			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job2.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = CAF.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 310M;

			line = (APInvoiceLine)APInvoice.Lines.AddNew();
			line.AL_JH = job2.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = FRT.PK;
			line.AL_GB = SYD.PK;
			line.AL_GE = FEA.PK;
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			line.AL_ExchangeRate = 1M;
			line.AL_LocalExTaxAmount = 395M;
		}

		void SetUpLevelAuthorization_Charges(Job job)
		{
			AssertNotNull(SYD);
			AssertNotNull(BNE);
			AssertNotNull(CEA);
			AssertNotNull(FEA);
			AssertNotNull(CUSDSB);
			AssertNotNull(CAF);
			AssertNotNull(FRT);
			AssertNotNull(BAF);
			AssertNotNull(DOF);

			Charge charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 200M, null,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 200M, TestObjectCreator.Creditor2,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			charge = TestObjectCreator.CreateCharge(job, CAF, "",
				TestObjectCreator.AUD, 300M, null,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = BNE.PK;
			charge.JR_GE = FEA.PK;

			charge = TestObjectCreator.CreateCharge(job, FRT, "",
				TestObjectCreator.AUD, 400M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			charge = TestObjectCreator.CreateCharge(job, CUSDSB, "",
				TestObjectCreator.AUD, 500M, null,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = BNE.PK;
			charge.JR_GE = CEA.PK;

			charge = TestObjectCreator.CreateCharge(job, BAF, "",
				TestObjectCreator.AUD, 600M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;

			charge = TestObjectCreator.CreateCharge(job, DOF, "",
				TestObjectCreator.AUD, 700M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = SYD.PK;
			charge.JR_GE = FEA.PK;
		}

		#endregion

		#region Implementation

		GlbBranch SYD
		{
			get { return fSYD ?? (fSYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD")); }
		}
		GlbBranch fSYD;

		GlbBranch BNE
		{
			get { return fBNE ?? (fBNE = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BNE")); }
		}
		GlbBranch fBNE;

		GlbDepartment CEA
		{
			get { return fCEA ?? (fCEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "CEA")); }
		}
		GlbDepartment fCEA;

		GlbDepartment FEA
		{
			get { return fFEA ?? (fFEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA")); }
		}
		GlbDepartment fFEA;

		AccChargeCode CAF
		{
			get { return TestObjectCreator.CC5; }
		}

		AccChargeCode FRT
		{
			get { return TestObjectCreator.CC6; }
		}

		AccChargeCode BAF
		{
			get { return TestObjectCreator.CC7; }
		}

		AccChargeCode CUSDSB
		{
			get { return TestObjectCreator.CC8; }
		}

		AccChargeCode DOF
		{
			get { return TestObjectCreator.CC9; }
		}

		APInvoice APInvoice
		{
			get { return (APInvoice)HeaderWithLines; }
		}

		TestObjectCreator fObjectCreator;
		protected TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}

		AccChargeCode fDSBChargeCode;
		protected AccChargeCode DSBChargeCode
		{
			get
			{
				if (fDSBChargeCode == null)
				{
					fDSBChargeCode = ObjectCreator.CreateChargeCode("TSTDSB", "TESTDSBCHARGE", Core.Constants.ChargeType.Disbursement, 100m, null, null, "ALL");
				}
				return fDSBChargeCode;
			}
		}

		AccChargeCode fMRGChargeCode;
		protected AccChargeCode MRG100Code
		{
			get
			{
				if (fMRGChargeCode == null)
				{
					fMRGChargeCode = ObjectCreator.CreateChargeCode("TST", "TESTMRG", Core.Constants.ChargeType.Margin, 100m, null, null, "ALL");
				}
				return fMRGChargeCode;
			}
		}

		TaxFrameworkTestObjectCreator TFObjectCreator => tfObjectCreator ?? (tfObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator tfObjectCreator;

		void SetUpInvoiceForSaving()
		{
			AccBankAccount testBankAcoount = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_IsActive = ZBool.False;
			testChequeBook.AK_AB = testBankAcoount.PK;

			APInvoice.SubmittedFromInvoicingForm = true;
			APInvoice.IsInvoiceReceiptPayment = true;
			APInvoice.ReceiptPaymentAH_AB = testChequeBook.AK_AB;
			APInvoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			APInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
		}

		void AssertCorrectAuthorisationRequired(bool firstRequired, bool secondRequired,
			CostVarianceApprovalAuthorisationRequirement setting, bool levelAuthorizationRequired)
		{
			APInvoice.ClearCachedCostVarianceApproval_ForTestOnly();
			AssertEquals("First Authorisation Required", firstRequired, APInvoice.Level1AuthorisationRequired_ForTestOnly(APInvoice.AuthorisationRequired));
			AssertEquals("Second Authorisation Required", secondRequired, APInvoice.Level2AuthorisationRequired_ForTestOnly(APInvoice.AuthorisationRequired));

			if (setting != null)
			{
				AssertNotNull("Authorisation Requirement should be determined.", APInvoice.AuthorisationRequired);
				AssertEquals("Authorisation Requirement", setting.AuthorisationRequirement,
					APInvoice.AuthorisationRequired.AuthorisationRequirement);
			}

			AssertEquals("Level Authorization Required", levelAuthorizationRequired, APInvoice.LevelAuthorizationRequired);
			AssertEquals("Check Level Security Rights", !levelAuthorizationRequired, APInvoice.CheckLevelSecurityRights());
		}

		#endregion

		protected override ZString ExpectedTransactionTypeForIncomplete
		{
			get { return TransactionTypes.IncompleteInvoice; }
		}

		protected override BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForTransactionRuleRegistry => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions;

		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceBase = base.GetNewBusinessObject();
			TestObjectCreator.FillInvoiceWithMinimumTestData((InvoicingBase)invoiceBase);

			return invoiceBase;
		}

		protected override bool ShouldExpectTaxTotal => true;

		protected override bool CouldHaveAssociatedDraftInvoice => true;
	}
}
