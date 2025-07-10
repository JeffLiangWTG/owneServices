using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(Invoice))]
	public abstract class InvoiceTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion

		public void TestReceiptPaymentAH_InvoiceDateInfo_HumanReadableName()
		{
			var newInvoice = CreateInvoice(GetExpectedBusinessObjectType(), AUD, 1m);
			AssertEquals("Receipt/Payment Invoice Date", newInvoice.ReceiptPaymentAH_InvoiceDateInfo.HumanReadableName);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestRecalculateAllValues()
		{
			Invoice invoice = CreateInvoice(typeof(APInvoice), USD, .7M);
			AssertEquals("Invoice Amount", 0M, invoice.AH_InvoiceAmount);
			AssertEquals("OS Ex Tax Amount", 0M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 0M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 0M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 0M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 0M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 0M, invoice.AH_LocalTotalAmount);

			CreateInvoiceLine(invoice, USD, .7M, 100M);
			AssertEquals("OS Ex Tax Amount", 100M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 10M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 5M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 110M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 142.86M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 14.29M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 7.14M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 157.15M, invoice.AH_LocalTotalAmount);

			CreateInvoiceLine(invoice, USD, .7M, 200M);
			AssertEquals("OS Ex Tax Amount", 300M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 30M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 15M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 330M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 428.57M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 42.86M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 21.43M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 471.43M, invoice.AH_LocalTotalAmount);

			InvoiceLine line3 = CreateInvoiceLine(invoice, USD, .7M, 300M);
			AssertEquals("OS Ex Tax Amount", 600M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 60M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 30M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 660M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 857.14M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 85.72M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 42.86M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 942.86M, invoice.AH_LocalTotalAmount);

			InvoiceLine line4 = CreateInvoiceLine(invoice, USD, .7M, 400M);
			AssertEquals("OS Ex Tax Amount", 1000M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 100M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 50M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 1100M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 1428.57M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 142.86M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 71.43M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 1571.43M, invoice.AH_LocalTotalAmount);

			invoice.Lines.RemoveAndDelete(line4);
			AssertEquals("OS Ex Tax Amount", 600M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 60M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 30M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 660M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 857.14M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 85.72M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 42.86M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 942.86M, invoice.AH_LocalTotalAmount);

			line3.AL_OSExTaxAmount = 1000M;
			AssertEquals("OS Ex Tax Amount", 1300M, invoice.AH_OSExTaxAmount);
			AssertEquals("OS Tax Amount   ", 130M, invoice.AH_OSTaxAmount);
			AssertEquals("OS WHT Amount   ", 65M, invoice.AH_OSWHTAmount);
			AssertEquals("OS Total Amount ", 1430M, invoice.AH_OSTotalAmount);

			AssertEquals("Local Ex Tax Amount", 1857.14M, invoice.AH_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 185.72M, invoice.AH_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 92.86M, invoice.AH_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 2042.86M, invoice.AH_LocalTotalAmount);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesInvoice()
		{
			ARInvoice invoice = (ARInvoice)CreateInvoice(typeof(ARInvoice), TestObjectCreator.EUR, 1);
			AssertNotNull("Company should not be null", invoice.Company);

			var osList = new List<string>
			{
				nameof(invoice.ReceiptPaymentAH_OSTotalAmount),
				nameof(invoice.ReceiptPaymentAH_OSTotalAmount_ReadOnly)
			};

			var tester = new DecimalPlacesAttributeTester(invoice, invoice.Company);
			tester.CheckNonLocalCurrency(osList, nameof(invoice.OSCurrencyDecimals), nameof(invoice.AH_RX_NKTransactionCurrency), invoice);
		}

		public void TestResetBankAccountCollectionOnAH_GB()
		{
			var testBranch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount3 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount1.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount2.AB_GB = testBranch.PK;
			bankAccount3.AB_GB = testBranch.PK;

			var invoiceType = GetExpectedBusinessObjectType();
			var invoice = CreateInvoice(invoiceType, AUD, 1M);
			invoice.IsInvoiceReceiptPayment = true;
			Assert("Precondition: invoice.IsInDatabase", !invoice.IsInDatabase);

			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.BankAccountLookup.Load();
			Assert("BankAccountLookups should contain bankAccount1", invoice.BankAccountLookup.Contains(bankAccount1.PK));
			AssertContainsExactElementsInAnyOrder("BankAccountCollection contain 1 item", new[] { bankAccount1 }, invoice.BankAccountCollection);

			invoice.AH_GB = testBranch.PK;
			invoice.BankAccountLookup.Load();
			AssertContainsExactElementsInAnyOrder("BankAccountLookups should contain bankAccount2 and bankAccount3", new[] { bankAccount2, bankAccount3 }, invoice.BankAccountLookup);
			AssertContainsExactElementsInAnyOrder("BankAccountCollection contain 2 items", new[] { bankAccount2, bankAccount3 }, invoice.BankAccountCollection);

			bankAccount1.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			bankAccount2.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			bankAccount3.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.LNK;
			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.eNettCreditCard;

			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.CreditCardBankAccountLookup.Load();
			Assert("CreditCardBankAccountLookup should contain bankAccount1", invoice.CreditCardBankAccountLookup.Contains(bankAccount1.PK));
			AssertContainsExactElementsInAnyOrder("CreditCardBankAccountCollection contain 1 item", new[] { bankAccount1 }, invoice.CreditCardBankAccountCollection);

			invoice.AH_GB = testBranch.PK;
			invoice.CreditCardBankAccountLookup.Load();
			AssertContainsExactElementsInAnyOrder("CreditCardBankAccountLookup should contain bankAccount2 and bankAccount3", new[] { bankAccount2, bankAccount3 }, invoice.CreditCardBankAccountLookup);
			AssertContainsExactElementsInAnyOrder("CreditCardBankAccountCollection contain 2 items", new[] { bankAccount2, bankAccount3 }, invoice.CreditCardBankAccountCollection);
		}

		public void TestGetGlobalChargeCodes()
		{
			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("MyCC");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				"MyCode0", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);

			Factory.Save();

			// Act 
			var globalCodes = invoice.GetGlobalChargeCodes(chargeCode.PK).ToList();

			// Assert
			AssertEquals(1, globalCodes.Count);
			AssertEquals("MyCode0", globalCodes[0].YG_Code);
			AssertEquals("DescriptionOfChargeCode", globalCodes[0].YG_Desc);
		}

		public void TestGetGlobalChargeCodeAddedWithOutOfDateFetch()
		{
			// Arrange
			var chargeCode = TestObjectCreator.CreateChargeCode("MyCC");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("MyCC2");
			var orgHeader = TestObjectCreator.CreateOrgHeader("OH1", false, true);
			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice.AH_OH = orgHeader.PK;

			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode.PK;

			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				"MyCode0", "DescriptionOfChargeCode", orgHeader.PK, chargeCode.PK, LedgerTypes.AccountsReceivable);
			var globalChargeCode2 = TestObjectCreator.CreateGlobalChargeCodeMapWithPivot(
				 "MyCode1", "DescriptionOfChargeCode", orgHeader.PK, chargeCode2.PK, LedgerTypes.AccountsReceivable);

			Factory.Save();

			invoice.GetGlobalChargeCodes(chargeCode.PK); // Cause fetch of charge codes in one hit
			line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = chargeCode2.PK; // Charge code added post-fetch

			// Act 
			var globalCodes = invoice.GetGlobalChargeCodes(chargeCode2.PK).ToList();

			// Assert
			AssertEquals(1, globalCodes.Count);
			AssertEquals("MyCode1", globalCodes[0].YG_Code);
			AssertEquals("DescriptionOfChargeCode", globalCodes[0].YG_Desc);
		}

		public void TestIsPostedProperty()
		{
			int invoiceNumber = 0;
			foreach (Type typeToTest in new[] { typeof(ARInvoice), typeof(APInvoice), typeof(ARCreditNote), typeof(APCreditNote), typeof(ARAdjustmentNote), typeof(APAdjustmentNote) })
			{
				InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeToTest, (++invoiceNumber).ToString(), TestObjectCreator.AUD, 1);
				AssertEquals("Not saved means not posted", false, invoice.IsPosted);
				Factory.Save();
				AssertEquals("Saved means posted", true, invoice.IsPosted);
			}

			foreach (Type typeToTest in new[] { typeof(APCreditNote), typeof(APInvoice), typeof(APAdjustmentNote) })
			{
				InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeToTest, (++invoiceNumber).ToString(), TestObjectCreator.AUD, 1);
				invoice.SaveAsIncomplete();
				Factory.Save();
				Assert("Precondition", invoice.IsInDatabase);
				AssertEquals("Saved as incomplete means not posted", false, invoice.IsPosted);
			}
		}

		protected Invoice CreateInvoice(Type invoiceType, RefCurrency currency, decimal exchangeRate)
		{
			Invoice invoice = (Invoice)Factory.New(invoiceType);
			invoice.AH_Desc = "Test Invoice";
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			invoice.AH_ExchangeRate = exchangeRate;
			return invoice;
		}

		protected InvoiceLine CreateInvoiceLine(Invoice invoice, RefCurrency currency, decimal exchangeRate, decimal aH_OSExTaxAmount)
		{
			InvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AT = GST1.PK;
			line.AL_AW = WHT1.PK;
			line.AL_RX_NKTransactionCurrency = currency.RX_Code;
			line.AL_ExchangeRate = exchangeRate;
			line.AL_OSExTaxAmount = aH_OSExTaxAmount;
			return line;
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		public void TestReceiptPaymentAH_PostDate_ReadOnly()
		{
			foreach (Type typeToTest in new[] { typeof(ARInvoice), typeof(APInvoice) })
			{
				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				TestObjectCreator.ResetSecurityCore();
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

				InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeToTest, TestObjectCreator.AUD, 1);
				AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("ReceiptPaymentAH_PostDate_ReadOnly", invoice));
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
				AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("ReceiptPaymentAH_PostDate_ReadOnly", invoice));

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
				if (typeToTest == typeof(ARInvoice))
				{
					Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
					Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				}
				else
				{
					Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
					Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				}

				AssertEquals("Allowed to post past so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("ReceiptPaymentAH_PostDate_ReadOnly", invoice));

				if (typeToTest == typeof(ARInvoice))
				{
					Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
					Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				}
				else
				{
					Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
					Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				}
				AssertEquals("Allowed to post future or past, so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("ReceiptPaymentAH_PostDate_ReadOnly", invoice));
			}
		}

		public void TestCopyTransaction_SetAL_SupplyType()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);

			invoice.Lines.AddNew();
			invoice.Lines.AddNew();

			invoice.Lines[0].AL_SupplyType = "LOC";
			invoice.Lines[1].AL_SupplyType = "LOX";

			var copiedARInvoice = ((ARInvoice)invoice).CopyTransaction_ForTestOnly();

			AssertEquals("copy invoice line 1 Supply is LOC", "LOC", ((ARInvoice)copiedARInvoice).Lines[0].AL_SupplyType);
			AssertEquals("copy invoice line 2 Supply is LOX", "LOX", ((ARInvoice)copiedARInvoice).Lines[1].AL_SupplyType);
		}

		public void TestCopyTransaction_SetSupplyTypeWillNotChangeTaxRate()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.FRT.TaxOverrides.RemoveAndDeleteAll();
			TestObjectCreator.CreateTaxOverrides(TestObjectCreator.FRT
				, taxOverride => {
					taxOverride.AO_AT = TestObjectCreator.GST2.PK;
					taxOverride.AO_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				});
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice.Lines.AddNew();
			invoice.Lines[0].GenericCharge = TestObjectCreator.FRT.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			AssertEquals(TestObjectCreator.GST2.PK, invoice.Lines[0].AL_AT);
			invoice.Lines[0].AL_AT = GST1.PK;
			AssertEquals(GST1.PK, invoice.Lines[0].AL_AT);

			var copiedAPInvoice = ((APInvoice)invoice).CopyTransaction_ForTestOnly();
			AssertEquals("Tax Rate should still be GST1", GST1.PK, ((APInvoice)copiedAPInvoice).Lines[0].AL_AT);
		}

		public void TestCopyTransaction_SetTaxBranch()
		{
			var branch = TestObjectCreator.CreateBranch("TST", GlbCompany.CurrentCompany);
			var branch1 = TestObjectCreator.CreateBranch("TS1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("TS2", GlbCompany.CurrentCompany);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice.AH_GB_TaxBranch = branch.PK;

			invoice.Lines.AddNew();
			invoice.Lines.AddNew();

			invoice.Lines[0].AL_GB_TaxBranch = branch1.PK;
			invoice.Lines[1].AL_GB_TaxBranch = branch2.PK;

			var copiedARInvoice = ((ARInvoice)invoice).CopyTransaction_ForTestOnly();

			AssertEquals("copy invoice Tax Branch", branch.PK, ((ARInvoice)copiedARInvoice).AH_GB_TaxBranch);
			AssertEquals("copy invoice line 1 Tax Branch", branch1.PK, ((ARInvoice)copiedARInvoice).Lines[0].AL_GB_TaxBranch);
			AssertEquals("copy invoice line 2 Tax Branch", branch2.PK, ((ARInvoice)copiedARInvoice).Lines[1].AL_GB_TaxBranch);
		}

		public void TestEnableValidationOfValidateExpectedInvoiceTotal() => ExpectedAmountTestHelper.AssertEnableValidationOfValidateExpectedInvoiceTotal();

		public void TestExpectedInvoiceTotalAdjustmentSuspender() => ExpectedAmountTestHelper.TestExpectedInvoiceTotalAdjustmentSuspender();

		public void TestSetExpectedOSAmountFromAccDraftInvoice() => ExpectedAmountTestHelper.AssertSetExpectedOSAmountFromAccDraftInvoice();

		public void TestUpdateExpectedAmountFromOSAmount() => ExpectedAmountTestHelper.AssertUpdateExpectedAmountFromOSAmount(ShouldAssignExpectedAmountFromOSAmount, ShouldExpectTaxTotal, IsInvoiceSupportingTaxAmount);

		ExpectedAmountTestHelper ExpectedAmountTestHelper => (expectedAmountTestHelper ??= new ExpectedAmountTestHelper(() => (InvoicingBase)Factory.New(GetExpectedBusinessObjectType())));
		ExpectedAmountTestHelper expectedAmountTestHelper;

		protected virtual bool ShouldAssignExpectedAmountFromOSAmount => true;

		protected virtual bool ShouldExpectTaxTotal => false;

		protected virtual bool IsInvoiceSupportingTaxAmount => true;

		public void TestReadOnlyPropertiesForAssociatedDraftInvoice()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var invoicingBase = GetNewBusinessObject() as InvoicingBase;
			invoicingBase.AH_OH = TestObjectCreator.AALSHI.PK;
			invoicingBase.AH_TransactionNum = "INV-007";
			invoicingBase.ValidateExpectedInvoiceTotal = true;
			invoicingBase.OriginalTransactionReference = ZGuid.Empty;

			var alwaysReadOnlyProperties = invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable
				? new[] { nameof(InvoicingBase.AH_DueDate), nameof(InvoicingBase.AH_TransactionNum) }
				: Array.Empty<string>();

			DraftInvoiceReadonlyHelper.AssertReadOnlyPropertiesForAssociatedDraftInvoice(invoicingBase, CouldHaveAssociatedDraftInvoice, alwaysReadOnlyProperties);
		}

		DraftInvoiceReadonlyHelper DraftInvoiceReadonlyHelper => draftInvoiceReadonlyHelper ??= new DraftInvoiceReadonlyHelper();
		DraftInvoiceReadonlyHelper draftInvoiceReadonlyHelper;

		protected virtual bool CouldHaveAssociatedDraftInvoice => false;

		#region Implementation

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			job.JH_LocalChargesCFX = localClientCFX;
			job.JH_AgentChargesCFX = agentCFX;

			return job;
		}

		protected ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			return exchangeRate;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		#region USD

		protected RefCurrency fUSD;
		protected RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				}
				return fUSD;
			}
		}

		#endregion

		#region AUD

		protected RefCurrency fAUD;
		protected RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region GBP

		protected RefCurrency fGBP;
		protected RefCurrency GBP
		{
			get
			{
				if (fGBP == null)
				{
					fGBP = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "GBP");
				}
				return fGBP;
			}
		}

		#endregion

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

		#region USLAX

		protected RefUNLOCO fUSLAX;
		protected RefUNLOCO USLAX
		{
			get
			{
				if (fUSLAX == null)
				{
					fUSLAX = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
				}
				return fUSLAX;
			}
		}

		#endregion

		#region AUSYD

		protected RefUNLOCO fAUSYD;
		protected RefUNLOCO AUSYD
		{
			get
			{
				if (fAUSYD == null)
				{
					fAUSYD = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				}
				return fAUSYD;
			}
		}

		#endregion

		#region GBLON

		protected RefUNLOCO fGBLON;
		protected RefUNLOCO GBLON
		{
			get
			{
				if (fGBLON == null)
				{
					fGBLON = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON");
				}
				return fGBLON;
			}
		}

		#endregion

		#region MRG80_1

		protected AccChargeCode fMRG80_1;
		protected AccChargeCode MRG80_1
		{
			get
			{
				if (fMRG80_1 == null)
				{
					fMRG80_1 = CreateChargeCode("MRG80_1", "Margin 80 Charge Code 1", Constants.ChargeType.Margin, 80, null, null);
				}
				return fMRG80_1;
			}
		}

		#endregion

		#region DIS

		protected AccChargeCode fDIS;
		protected AccChargeCode DIS
		{
			get
			{
				if (fDIS == null)
				{
					fDIS = CreateChargeCode("DIS", "Disbursment Charge Code", Constants.ChargeType.Disbursement, 100, null, null);
				}
				return fDIS;
			}
		}

		#endregion

		#region MRG0_1

		protected AccChargeCode fMRG0_1;
		protected AccChargeCode MRG0_1
		{
			get
			{
				if (fMRG0_1 == null)
				{
					fMRG0_1 = CreateChargeCode("MRG0_1", "Margin 0 Charge Code 1", Constants.ChargeType.Margin, 0, null, null);
				}
				return fMRG0_1;
			}
		}

		#endregion

		#region REV

		protected AccChargeCode fREV;
		protected AccChargeCode REV
		{
			get
			{
				if (fREV == null)
				{
					fREV = CreateChargeCode("REV", "Revenue Charge Code", Constants.ChargeType.Revenue, 80, null, null);
				}
				return fREV;
			}
		}

		#endregion

		#region MRG100

		protected AccChargeCode fMRG100;
		protected AccChargeCode MRG100
		{
			get
			{
				if (fMRG100 == null)
				{
					fMRG100 = CreateChargeCode("MRG100", "Margin 100 With GST & WHT", Constants.ChargeType.Margin, 100, GST1, WHT1);
				}
				return fMRG100;
			}
		}

		#endregion

		#region GST1

		protected AccTaxRate fGST1;
		protected AccTaxRate GST1
		{
			get
			{
				if (fGST1 == null)
				{
					fGST1 = CreateTaxRate("GST1", "GST Rate 1", 10);
				}
				return fGST1;
			}
		}

		#endregion

		#region WHT1

		protected AccWithholding fWHT1;
		protected AccWithholding WHT1
		{
			get
			{
				if (fWHT1 == null)
				{
					fWHT1 = CreateWithholdingTax("WHT1", "WHT Rate 1", 5);
				}
				return fWHT1;
			}
		}

		#endregion

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_AT_GSTRate = GST1.PK;
			chargeCode.AC_AW_WithholdingTaxRate = WHT1.PK;
			return chargeCode;
		}

		protected AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		protected AccWithholding CreateWithholdingTax(string code, string description, decimal rate)
		{
			AccWithholding taxRate = Factory.New<AccWithholding>();
			taxRate.AW_Code = code;
			taxRate.AW_Description = description;
			taxRate.AW_IsActive = true;
			taxRate.AW_Rate = rate;
			taxRate.AW_GC = GlbCompany.CurrentCompany.PK;
			return taxRate;
		}

		#endregion
	}
}
