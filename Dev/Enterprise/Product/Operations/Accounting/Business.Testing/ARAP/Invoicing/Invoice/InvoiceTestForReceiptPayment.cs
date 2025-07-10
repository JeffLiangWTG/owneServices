using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceTestForReceiptPayment : InvoicingBaseTest
	{
		public virtual void TestCashInvoice_TaxRealisationEnablerIsCalledBeforeBeforeGLMovementProcessor()
		{
			var org = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.OverheadChargeCode;

			TFObjectCreator.SetupMinimumSettingsForTaxFramework(GlbCompany.GetCurrentCompany(Factory), org, chargeCode, TaxConfigurationLedgers.AccountsPayable.Code);
			Assert("Precondition: IsEnabledForTaxFrameworkConfiguration", GlbCompany.CurrentCompany.IsEnabledForTaxFrameworkConfiguration(Factory));

			var invoice = TestObjectCreator.CreateCashInvoice(GetExpectedBusinessObjectType(), TestObjectCreator.GetRandomString(10), TestObjectCreator.AUD, 1, org, 100, 10, chargeCode.PK, TestObjectCreator.AUDBankAccount);

			var taxTransactionsParams = new CreateTaxTransactionParameters
			{
				TransactionHeader = invoice,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				AffectsSourceTransactionTotal = false,
			};
			var taxTransaction = TFObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			TFObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoice.Lines[0]));

			Assert("Precondition: Fully Paid Date ", invoice.AH_FullyPaidDate.IsEmpty);

			var sequence = new MockSequence();

			var taxRealisationEnablerMock = new Mock<ITaxRealisationEnabler>(MockBehavior.Strict);
			TaxFrameworkObjectFactory.SubstituteTaxRealisationEnabler_ForTestOnly(Factory, taxRealisationEnablerMock.Object);
			taxRealisationEnablerMock.InSequence(sequence).Setup(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), It.IsAny<IWithholdingJournalCreationManager>(), It.IsAny<ZDate>())).Callback(() => taxTransaction.ATT_RealisationDate = ZDate.Today).Returns(string.Empty);
			var glMovementProcessor = new Mock<IGLMovementProcessor>(MockBehavior.Strict);
			taxTransaction.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessor.Object);
			glMovementProcessor.InSequence(sequence).Setup(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()));
			var tfDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			var emptyValidatorMock = new Mock<IAccTaxTransactionCriticalValidator>();
			tfDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(taxTransaction)).Returns(emptyValidatorMock.Object);
			ObjectFactory.Substitute(tfDependencyFactory.Object);

			Factory.Save();

			Assert("Postcondition: Fully Paid Date is set", !invoice.AH_FullyPaidDate.IsEmpty);
			taxRealisationEnablerMock.Verify(x => x.RealiseTaxIfApplicable(It.IsAny<IMatchingCollection>(), It.IsAny<IWithholdingJournalCreationManager>(), It.IsAny<ZDate>()), Times.Once);
			glMovementProcessor.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Once);
		}

		public void TestPaymentNotCreatedWhenSavingInvoiceAsIncomplete()
		{
			Invoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			invoice.ReceiptPaymentAK_AB = new TestObjectCreator(Factory).AUDBankAccount.PK;
			invoice.SaveAsIncomplete();

			Assert("Invoice shouldn't be matched with anything", invoice.AH_TransactionBelongsToGroup.IsEmpty);
		}

		public void TestPaymentNotCreatedWhenInvoiceIsAlreadyPosted()
		{
			Invoice invoice = (Invoice)TestObjectCreator.CreateInvoiceWithLine(GetExpectedBusinessObjectType(), "000001", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			Factory.Save();

			Assert("Precondition: invoice is posted", invoice.IsInDatabase);

			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			invoice.HandleReceiptPayment();

			Assert("Invoice shouldn't be matched with anything", invoice.AH_TransactionBelongsToGroup.IsEmpty);

			Factory.Save();
		}

		public void TestPaymentCreatedWhenInvoiceIsIncomplete()
		{
			Invoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			invoice.ReceiptPaymentAK_AB = new TestObjectCreator(Factory).AUDBankAccount.PK;
			invoice.SaveAsIncomplete();

			Assert("Precondition: invoice is saved in database", invoice.IsInDatabase);

			invoice.AH_Ledger = "AP";
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;

			invoice.HandleReceiptPayment();

			AssertNotNull("Receipt Payment shouldn't be null", invoice.ReceiptPayment);
		}

		public void TestPaymentCreatedFromInvoiceHasMatchingDeptAndBranchAfterOverride()
		{
			Invoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			invoice.ReceiptPaymentAK_AB = new TestObjectCreator(Factory).AUDBankAccount.PK;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			invoice.AH_GB = branch.PK;
			invoice.AH_GE = department.PK;

			invoice.HandleReceiptPayment();
			AssertNotNull("Receipt Payment shouldn't be null", invoice.ReceiptPayment);
			AssertEquals("branch matches", invoice.ReceiptPayment.AH_GB, invoice.AH_GB);
			AssertEquals("dept matches", invoice.ReceiptPayment.AH_GE, invoice.AH_GE);
		}

		public void TestSetAH_OHCallsValidateOrgDependantLineItems()
		{
			ChargeCode.AC_AT_GSTRate = GST10TaxRate.PK;
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			Invoice invoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());

			AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			invoice.Lines.AddNew();
			invoice.Lines.AddNew();

			Assert(!invoice.Lines[0].AL_ATInfo.HasError("Please enter a Tax ID."));
			Assert(!invoice.Lines[1].AL_ATInfo.HasError("Please enter a Tax ID."));

			invoice.Lines[0].GenericCharge = ChargeCode.PK;

			invoice.AH_OH = GSTRegisteredOrg.PK;

			Assert(!invoice.Lines[0].AL_ATInfo.HasError("Please enter a Tax ID."));
			Assert(invoice.Lines[1].AL_ATInfo.HasError("Please enter a Tax ID."));
		}

		AccTaxRate fGST10TaxRate;
		OrgHeader fGSTRegisteredOrg;

		protected AccTaxRate GST10TaxRate
		{
			get
			{
				if (fGST10TaxRate == null)
				{
					fGST10TaxRate = TestObjectCreator.CreateTaxRate("TSTGST", "Test GST Code", 10);
				}
				return fGST10TaxRate;
			}
		}

		protected OrgHeader GSTRegisteredOrg
		{
			get
			{
				if (fGSTRegisteredOrg == null)
				{
					fGSTRegisteredOrg = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true, true, false, true, false);
				}
				return fGSTRegisteredOrg;
			}
		}

		public void TestReceiptPyamentAH_Desc()
		{
			if (TestInvoice.IsInvoiceReceiptPayment)
			{
				var expectedDesc = "The quick brown fox jumps over the lazy dog.";

				TestInvoice.SubmittedFromInvoicingForm = true;
				TestInvoice.IsInvoiceReceiptPayment = true;

				TestInvoice.ReceiptPaymentAH_Desc = string.Empty;
				AssertHasError(TestInvoice.ReceiptPaymentAH_DescInfo, "Please enter a value.");

				TestInvoice.ReceiptPaymentAH_Desc = expectedDesc;

				Factory.Save();

				if (TestInvoice is ARInvoice)
				{
					var result = Factory.Load<ARReceipt>(TestInvoice.ReceiptPayment.PK);
					AssertEquals("AH_Desc should be expectedDesc.", expectedDesc, result.AH_Desc);
				}
				else
				{
					var result = Factory.Load<APPayment>(TestInvoice.ReceiptPayment.PK);
					AssertEquals("AH_Desc should be expectedDesc.", expectedDesc, result.AH_Desc);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestReceiptPaymentTypeDefault()
		{
			Invoice invoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			AssertEquals(ReceiptTypes.Cheque, invoice.ReceiptPaymentAH_ReceiptType);

			if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				AccountingConfigurationRegistry.Instance.DefaultReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
				invoice.SetReceiptPaymentDefaults();
				AssertEquals(ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
				invoice.SetReceiptPaymentDefaults();
				AssertEquals(ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
			}
		}

		public void TestReceiptPaymentTypeReferenceNumberDefault()
		{
			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			AccountingConfigurationRegistry.Instance.DefaultReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			Invoice invoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			AssertEquals(ReceiptTypes.CreditCard, invoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("Test1", invoice.ReceiptPaymentAH_ChequeOrReference);

			invoice.ReceiptPaymentAH_ChequeOrReference = "";
			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals(ReceiptTypes.Cash, invoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("CASH", invoice.ReceiptPaymentAH_ChequeOrReference);
		}

		public void TestReceiptPaymentType()
		{
			Invoice invoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals(invoice.NewReceiptPayment_ForTestOnly.GetType(), (invoice is ARInvoice ? typeof(ARReceipt) : typeof(APPayment)));
		}

		public void TestCopyTransaction()
		{
			AccGLHeader glHeader1 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2010.00.00"));
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, true);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefCurrency currency = newFactory.NewWithValidTestData<RefCurrency>();
			RefCurrency defaultCurrency = newFactory.NewWithValidTestData<RefCurrency>();

			RefExchangeRate buyExRate = newFactory.NewWithValidTestData<RefExchangeRate>();
			buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			buyExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			buyExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			buyExRate.RE_SellRate = 0.8833m;
			buyExRate.RE_RX_NKExCurrency = currency.RX_Code;
			buyExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;

			RefExchangeRate sellExRate = newFactory.NewWithValidTestData<RefExchangeRate>();
			sellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			sellExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			sellExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			sellExRate.RE_SellRate = 0.8833m;
			sellExRate.RE_RX_NKExCurrency = currency.RX_Code;
			sellExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			newFactory.Save();

			AccChargeCode charge = Factory.NewWithValidTestData<AccChargeCode>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = defaultCurrency.RX_Code;
			org.CompanyData.OB_RX_NKAPDefltCurrency = defaultCurrency.RX_Code;
			org.CompanyData.OB_AC_APDefaultChargeCode = charge.PK;
			Factory.Save();

			var address = TestObjectCreator.CreateAddress(org);
			var contact = TestObjectCreator.CreateContact(org);

			GlbDepartment dept = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment deptLine1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment deptLine2 = Factory.NewWithValidTestData<GlbDepartment>();

			Invoice originalInvoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());
			originalInvoice.AH_OH = org.PK;
			originalInvoice.AH_GE = dept.PK;
			originalInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			originalInvoice.AH_OA_InvoiceAddressOverride = address.PK;
			originalInvoice.AH_OC_InvoiceContactOverride = contact.PK;
			InvoiceLine line1 = (InvoiceLine)originalInvoice.Lines.AddNew();
			line1.GenericCharge = glHeader1.PK;
			line1.AL_Sequence = (short)1;
			line1.AL_GE = deptLine1.PK;
			line1.AL_Desc = "monday";
			line1.AL_GovtChargeCode = "OVERRIDE";

			var isSupportMultipleSubAccounts = (originalInvoice as ISupportMultiSubAccounts)?.IsMultiSubAccountsSupported ?? false;
			if (isSupportMultipleSubAccounts)
			{
				TestObjectCreator.SetUpTransactionLineSubAccount(line1, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.ABIGAS.PK);
			}

			InvoiceLine line2 = (InvoiceLine)originalInvoice.Lines.AddNew();
			line2.AL_Sequence = (short)2;
			line2.AL_GE = deptLine1.PK;
			line2.AL_Desc = "tuesday";

			InvoiceLine line3 = (InvoiceLine)originalInvoice.Lines.AddNew();
			line3.AL_Sequence = (short)3;
			line3.AL_GE = deptLine1.PK;
			line3.AL_Desc = "wednesday";

			InvoiceLine line4 = (InvoiceLine)originalInvoice.Lines.AddNew();
			line4.AL_Sequence = (short)4;
			line4.AL_GE = deptLine1.PK;
			line4.AL_Desc = "thursday";
			line4.AL_Calc_InputGSTVATRecoverablePercentage = 12;

			dept.GE_IsActive = true;
			deptLine1.GE_IsActive = true;

			AssertEquals("Precondition: Line1 should have sequence num 1", (short)1, line1.AL_Sequence);
			AssertEquals("Precondition: Line2 should have sequence num 2", (short)2, line2.AL_Sequence);
			AssertEquals("Precondition: Line3 should have sequence num 3", (short)3, line3.AL_Sequence);
			AssertEquals("Precondition: Line4 should have sequence num 4", (short)4, line4.AL_Sequence);

			Invoice copiedInvoice = (Invoice)(originalInvoice.CopyTransaction_ForTestOnly());
			AssertEquals(originalInvoice.GetType(), copiedInvoice.GetType());
			AssertEquals(originalInvoice.Lines.Count, copiedInvoice.Lines.Count);

			AssertEquals(copiedInvoice.AH_JH, originalInvoice.AH_JH);
			AssertEquals(copiedInvoice.AH_GB, originalInvoice.AH_GB);
			AssertEquals(copiedInvoice.AH_GE, dept.PK);
			AssertEquals(copiedInvoice.AH_AG, originalInvoice.AH_AG);
			AssertEquals(copiedInvoice.AH_Ledger, originalInvoice.AH_Ledger);
			AssertEquals(copiedInvoice.AH_TransactionType, originalInvoice.AH_TransactionType);
			AssertEquals(copiedInvoice.AH_Desc, originalInvoice.AH_Desc);
			AssertEquals(copiedInvoice.AH_InvoiceDate.Date, originalInvoice.AH_InvoiceDate.Date);
			AssertEquals(copiedInvoice.AH_PostDate.Date, originalInvoice.AH_PostDate.Date);
			AssertEquals(copiedInvoice.AH_DueDate.Date, originalInvoice.AH_DueDate.Date);
			AssertEquals(copiedInvoice.AH_CashBasisGSTIndicator, originalInvoice.AH_CashBasisGSTIndicator);
			AssertEquals(copiedInvoice.AH_TransactionCategory, originalInvoice.AH_TransactionCategory);
			AssertEquals(copiedInvoice.AH_InvoiceTerm, originalInvoice.AH_InvoiceTerm);
			AssertEquals(copiedInvoice.AH_TransactionCategory, originalInvoice.AH_TransactionCategory);
			AssertEquals(copiedInvoice.AH_OSExTaxAmount, originalInvoice.AH_OSExTaxAmount);
			AssertEquals(copiedInvoice.AH_OSTaxAmount, originalInvoice.AH_OSTaxAmount);
			AssertEquals(copiedInvoice.AH_OSTotalAmount, originalInvoice.AH_OSTotalAmount);
			AssertEquals(copiedInvoice.AH_RX_NKTransactionCurrency, originalInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate should be 0.8833", 0.8833m, copiedInvoice.AH_ExchangeRate);
			AssertEquals(copiedInvoice.AH_OH, originalInvoice.AH_OH);
			AssertEquals(copiedInvoice.AH_OA_InvoiceAddressOverride, originalInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals(copiedInvoice.AH_OC_InvoiceContactOverride, originalInvoice.AH_OC_InvoiceContactOverride);

			int originalInvoiceIndex = 0;
			foreach (InvoiceLine line in copiedInvoice.Lines)
			{
				AssertEquals(line.AL_JH, originalInvoice.Lines[originalInvoiceIndex].AL_JH);
				AssertEquals(line.GenericCharge, originalInvoice.Lines[originalInvoiceIndex].GenericCharge);
				AssertEquals(line.AL_LineType, originalInvoice.Lines[originalInvoiceIndex].AL_LineType);
				AssertEquals(line.AL_GB, originalInvoice.Lines[originalInvoiceIndex].AL_GB);
				AssertEquals(line.AL_GE, deptLine1.PK);
				AssertEquals(line.AL_LineAmount, originalInvoice.Lines[originalInvoiceIndex].AL_OSAmount);
				AssertEquals(line.AL_LineType, originalInvoice.Lines[originalInvoiceIndex].AL_LineType);
				AssertEquals(line.AL_OSExTaxAmount, originalInvoice.Lines[originalInvoiceIndex].AL_OSExTaxAmount);
				AssertEquals(line.AL_OSTaxAmount, originalInvoice.Lines[originalInvoiceIndex].AL_OSTaxAmount);
				AssertEquals(line.AL_AC, originalInvoice.Lines[originalInvoiceIndex].AL_AC);
				AssertEquals(line.AL_AG, originalInvoice.Lines[originalInvoiceIndex].AL_AG);
				AssertEquals(line.AL_AT, originalInvoice.Lines[originalInvoiceIndex].AL_AT);
				AssertEquals(line.AL_A9_VATClass, originalInvoice.Lines[originalInvoiceIndex].AL_A9_VATClass);
				AssertEquals("Lines in copied invoice should be in the same order as original invoice", (short)(originalInvoiceIndex + 1), line.AL_Sequence);

				if (isSupportMultipleSubAccounts && line.AL_Desc == "monday")
				{
					var subAccounts = line.SubAccounts.Cast<AccTransactionLineSubAccount>();
					AssertEquals("Sub Accounts should also be cloned to copied transaction", 1, subAccounts.Count());
					Assert(subAccounts.Any(x => x.AL1_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix && x.AL1_SubClassParentId == TestObjectCreator.ABIGAS.PK));
				}

				originalInvoiceIndex++;
			}
			AssertEquals("OriginalInvoiceIndex should be 4", 4, originalInvoiceIndex);

			AssertEquals("First line should be monday", "monday", copiedInvoice.Lines[0].AL_Desc);
			AssertEquals("First line should have govt charge code as OVERRIDE", "OVERRIDE", copiedInvoice.Lines[0].AL_GovtChargeCode);
			AssertEquals("Second line should be tuesday", "tuesday", copiedInvoice.Lines[1].AL_Desc);
			AssertEquals("Third line should be wednesday", "wednesday", copiedInvoice.Lines[2].AL_Desc);
			AssertEquals("Fourth line should be thursday", "thursday", copiedInvoice.Lines[3].AL_Desc);
			AssertEquals("Fourth line AL_Calc_InputGSTVATRecoverablePercentage", 12m, copiedInvoice.Lines[3].AL_Calc_InputGSTVATRecoverablePercentage);

			dept.GE_IsActive = false;
			deptLine1.GE_IsActive = true;
			Invoice copiedInvoiceByTestDep = (Invoice)(originalInvoice.CopyTransaction_ForTestOnly());
			AssertEquals(copiedInvoiceByTestDep.AH_GE, GlbDepartment.CurrentDepartment.PK);

			foreach (InvoiceLine line in copiedInvoiceByTestDep.Lines)
			{ AssertEquals(line.AL_GE, deptLine1.PK); }

			dept.GE_IsActive = true;
			deptLine1.GE_IsActive = false;
			deptLine2.GE_IsActive = false;
			line4.AL_GE = deptLine2.PK;
			copiedInvoiceByTestDep = (Invoice)(originalInvoice.CopyTransaction_ForTestOnly());
			AssertEquals(copiedInvoiceByTestDep.AH_GE, dept.PK);
			foreach (InvoiceLine line in copiedInvoiceByTestDep.Lines)
			{
				AssertEquals(line.AL_GE, dept.PK);
			}

			dept.GE_IsActive = true;
			deptLine1.GE_IsActive = true;
			deptLine2.GE_IsActive = false;
			line4.AL_GE = deptLine2.PK;
			copiedInvoiceByTestDep = (Invoice)(originalInvoice.CopyTransaction_ForTestOnly());
			AssertEquals(copiedInvoiceByTestDep.AH_GE, dept.PK);
			originalInvoiceIndex = 0;
			foreach (InvoiceLine line in copiedInvoiceByTestDep.Lines)
			{
				if (originalInvoiceIndex < 3)
				{
					AssertEquals(line.AL_GE, deptLine1.PK);
				}
				else
				{
					AssertEquals(line.AL_GE, dept.PK);
				}

				originalInvoiceIndex++;
			}
		}

		[TestDate(2020, 05, 26, 12, 00, 00)]
		public void TestCopyTransactionWithMultiSubAccount()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccGLHeader glHeader1 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2010.00.00"));
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, AccGroupsSchema.Constants.Prefix, false);

			RefCurrency currency = newFactory.NewWithValidTestData<RefCurrency>();
			RefCurrency defaultCurrency = newFactory.NewWithValidTestData<RefCurrency>();

			RefExchangeRate buyExRate = newFactory.NewWithValidTestData<RefExchangeRate>();
			buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			buyExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			buyExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			buyExRate.RE_SellRate = 0.8833m;
			buyExRate.RE_RX_NKExCurrency = currency.RX_Code;
			buyExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;

			RefExchangeRate sellExRate = newFactory.NewWithValidTestData<RefExchangeRate>();
			sellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			sellExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			sellExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			sellExRate.RE_SellRate = 0.8833m;
			sellExRate.RE_RX_NKExCurrency = currency.RX_Code;
			sellExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
			newFactory.Save();

			AccChargeCode charge = Factory.NewWithValidTestData<AccChargeCode>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = defaultCurrency.RX_Code;
			org.CompanyData.OB_RX_NKAPDefltCurrency = defaultCurrency.RX_Code;
			org.CompanyData.OB_AC_APDefaultChargeCode = charge.PK;
			Factory.Save();

			var address = TestObjectCreator.CreateAddress(org);
			var contact = TestObjectCreator.CreateContact(org);

			GlbDepartment dept = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment deptLine1 = Factory.NewWithValidTestData<GlbDepartment>();

			Invoice originalInvoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());
			originalInvoice.AH_TransactionNum = "INV0001";
			originalInvoice.AH_OH = org.PK;
			originalInvoice.AH_GE = dept.PK;
			originalInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			originalInvoice.AH_OA_InvoiceAddressOverride = address.PK;
			originalInvoice.AH_OC_InvoiceContactOverride = contact.PK;

			InvoiceLine line1 = (InvoiceLine)originalInvoice.Lines.AddNew();
			line1.GenericCharge = glHeader1.PK;
			line1.AL_Sequence = (short)1;
			line1.AL_GE = deptLine1.PK;
			line1.AL_Desc = "monday";
			line1.AL_GovtChargeCode = "OVERRIDE";
			var subaccount = line1.SubAccounts.AddNew();
			subaccount.SubAccountTypeDisplayCode = Core.Constants.SubAccountType.Organization;
			subaccount.AL1_SubClassParentId = TestObjectCreator.ABIGAS.PK;
			subaccount = line1.SubAccounts.AddNew();
			subaccount.SubAccountTypeDisplayCode = Core.Constants.SubAccountType.SalesGroup;
			subaccount.AL1_SubClassParentId = TestObjectCreator.AR1.PK;
			Factory.Save();

			Invoice copiedInvoice = (Invoice)(originalInvoice.CopyTransaction_ForTestOnly());
			AssertEquals(originalInvoice.GetType(), copiedInvoice.GetType());
			AssertEquals(originalInvoice.Lines.Count, copiedInvoice.Lines.Count);

			AssertEquals(copiedInvoice.AH_JH, originalInvoice.AH_JH);
			AssertEquals(copiedInvoice.AH_GB, originalInvoice.AH_GB);
			AssertEquals(copiedInvoice.AH_GE, dept.PK);
			AssertEquals(copiedInvoice.AH_AG, originalInvoice.AH_AG);
			AssertEquals(copiedInvoice.AH_Ledger, originalInvoice.AH_Ledger);
			AssertEquals(copiedInvoice.AH_TransactionType, originalInvoice.AH_TransactionType);
			AssertEquals(copiedInvoice.AH_Desc, originalInvoice.AH_Desc);
			AssertEquals(copiedInvoice.AH_InvoiceDate.Date, originalInvoice.AH_InvoiceDate.Date);
			AssertEquals(copiedInvoice.AH_PostDate.Date, originalInvoice.AH_PostDate.Date);
			AssertEquals(copiedInvoice.AH_DueDate.Date, originalInvoice.AH_DueDate.Date);
			AssertEquals(copiedInvoice.AH_CashBasisGSTIndicator, originalInvoice.AH_CashBasisGSTIndicator);
			AssertEquals(copiedInvoice.AH_TransactionCategory, originalInvoice.AH_TransactionCategory);
			AssertEquals(copiedInvoice.AH_InvoiceTerm, originalInvoice.AH_InvoiceTerm);
			AssertEquals(copiedInvoice.AH_TransactionCategory, originalInvoice.AH_TransactionCategory);
			AssertEquals(copiedInvoice.AH_OSExTaxAmount, originalInvoice.AH_OSExTaxAmount);
			AssertEquals(copiedInvoice.AH_OSTaxAmount, originalInvoice.AH_OSTaxAmount);
			AssertEquals(copiedInvoice.AH_OSTotalAmount, originalInvoice.AH_OSTotalAmount);
			AssertEquals(copiedInvoice.AH_RX_NKTransactionCurrency, originalInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate should be 0.8833", 0.8833m, copiedInvoice.AH_ExchangeRate);
			AssertEquals(copiedInvoice.AH_OH, originalInvoice.AH_OH);
			AssertEquals(copiedInvoice.AH_OA_InvoiceAddressOverride, originalInvoice.AH_OA_InvoiceAddressOverride);
			AssertEquals(copiedInvoice.AH_OC_InvoiceContactOverride, originalInvoice.AH_OC_InvoiceContactOverride);

			AssertEquals(1, originalInvoice.Lines.Count);
			AssertEquals(1, copiedInvoice.Lines.Count);
			AssertEquals(copiedInvoice.Lines[0].AL_JH, originalInvoice.Lines[0].AL_JH);
			AssertEquals(copiedInvoice.Lines[0].GenericCharge, originalInvoice.Lines[0].GenericCharge);
			AssertEquals(copiedInvoice.Lines[0].AL_LineType, originalInvoice.Lines[0].AL_LineType);
			AssertEquals(copiedInvoice.Lines[0].AL_GB, originalInvoice.Lines[0].AL_GB);
			AssertEquals(copiedInvoice.Lines[0].AL_GE, deptLine1.PK);
			AssertEquals(copiedInvoice.Lines[0].AL_LineAmount, originalInvoice.Lines[0].AL_OSAmount);
			AssertEquals(copiedInvoice.Lines[0].AL_LineType, originalInvoice.Lines[0].AL_LineType);
			AssertEquals(copiedInvoice.Lines[0].AL_OSExTaxAmount, originalInvoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals(copiedInvoice.Lines[0].AL_OSTaxAmount, originalInvoice.Lines[0].AL_OSTaxAmount);
			AssertEquals(copiedInvoice.Lines[0].AL_AC, originalInvoice.Lines[0].AL_AC);
			AssertEquals(copiedInvoice.Lines[0].AL_AG, originalInvoice.Lines[0].AL_AG);
			AssertEquals(copiedInvoice.Lines[0].AL_AT, originalInvoice.Lines[0].AL_AT);
			AssertEquals(copiedInvoice.Lines[0].AL_A9_VATClass, originalInvoice.Lines[0].AL_A9_VATClass);
			AssertNoExceptionThrown(() => copiedInvoice.Lines[0].SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
			AssertNoExceptionThrown(() => copiedInvoice.Lines[0].SubAccounts.SubAccountElements.Single(x => x.SubAccountTypeParentTableCode == AccGroupsSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.AR1.PK));
			AssertEquals("Lines in copied invoice should be in the same order as original invoice", (short)1, copiedInvoice.Lines[0].AL_Sequence);
			AssertEquals("First line should be monday", "monday", copiedInvoice.Lines[0].AL_Desc);
			AssertEquals("First line should have govt charge code as OVERRIDE", "OVERRIDE", copiedInvoice.Lines[0].AL_GovtChargeCode);

			copiedInvoice.AH_TransactionNum = "INV002";
			AssertNoExceptionThrown("Should save succesfully", () => Factory.Save());
		}

		public void TestCopyTransactionWithExchangeRate()
		{
			CopyTransactionWithExchangeRate(true, false, false);
			CopyTransactionWithExchangeRate(false, false, false);
			if (ExpectedBusinessObjectType == typeof(APInvoice))
			{
				CopyTransactionWithExchangeRate(false, true, false);
				CopyTransactionWithExchangeRate(false, true, true);
			}
			CopyTransactionWithExchangeRate(true, false, true);
			CopyTransactionWithExchangeRate(false, false, true);
		}

		void CopyTransactionWithExchangeRate(bool isHeaderLocal, bool jobExchRate, bool setupExchRate)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefCurrency foreignCurrency = TestObjectCreator.GBP;
			RefCurrency localCurrency = TestObjectCreator.AUD;

			ZString headerCurrency = isHeaderLocal ? localCurrency.RX_Code : foreignCurrency.RX_Code;
			ZString lineCurrency = foreignCurrency.RX_Code;

			ZDecimal headerExpectedRate = isHeaderLocal ? 1m : (setupExchRate ? 0.8833m : 0m);
			ZDecimal lineExpectedRate = jobExchRate ? 4.1234m : (setupExchRate ? 0.8833m : 0m);

			var isBuyExRateSetUp = ExchangeRateReader.GetReaderInstance().GetRate(GlbCompany.CurrentCompany.PK.ToGuid(), Core.Constants.ExchangeRateTypes.Code.BuyRate, foreignCurrency.RX_Code, DateTime.Now) != 0m;
			var isSellExRateSetUp = ExchangeRateReader.GetReaderInstance().GetRate(GlbCompany.CurrentCompany.PK.ToGuid(), Core.Constants.ExchangeRateTypes.Code.SellRate, foreignCurrency.RX_Code, DateTime.Now) != 0m;

			if (setupExchRate && !isBuyExRateSetUp && !isSellExRateSetUp)
			{
				RefExchangeRate buyExRate = newFactory.NewWithValidTestData<RefExchangeRate>();
				buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
				buyExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
				buyExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
				buyExRate.RE_SellRate = 0.8833m;
				buyExRate.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				buyExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;

				RefExchangeRate sellExRate = newFactory.NewWithValidTestData<RefExchangeRate>();
				sellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
				sellExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
				sellExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
				sellExRate.RE_SellRate = 0.8833m;
				sellExRate.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				sellExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;

				newFactory.Save();
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();

			Invoice originalInvoice = (Invoice)newFactory.New(GetExpectedBusinessObjectType());
			originalInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			originalInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			originalInvoice.AH_RX_NKTransactionCurrency = headerCurrency;
			InvoiceLine line = (InvoiceLine)originalInvoice.Lines.AddNew();
			line.AL_RX_NKTransactionCurrency = lineCurrency;
			line.SetExchangeRate();

			if (jobExchRate)
			{
				Job job = newFactory.NewJobForTesting<Job>();
				ExchangeRate exchRate = job.ExchangeRates.AddNew();
				exchRate.JF_RX_NKRateCurrency = foreignCurrency.RX_Code;
				exchRate.JF_BaseRate = 4.1234m;
				line.AL_JH = job.PK;
				originalInvoice.AH_PostedToEFT = true;
				headerExpectedRate = 1m;
			}

			Invoice copiedInvoice = (Invoice)(originalInvoice.CopyTransaction_ForTestOnly());

			ZBool hasHeaderError = headerExpectedRate == 0;
			ZBool hasLineError = lineExpectedRate == 0;

			AssertEquals("Check header currency value", originalInvoice.AH_RX_NKTransactionCurrency, copiedInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("Job exch rate ticked or not ticked", originalInvoice.AH_PostedToEFT, copiedInvoice.AH_PostedToEFT);
			AssertEquals("Correct currency is copied", originalInvoice.Lines[0].AL_RX_NKTransactionCurrency, copiedInvoice.Lines[0].AL_RX_NKTransactionCurrency);
			AssertEquals("Checks if there is an expected header error", hasHeaderError, copiedInvoice.AH_ExchangeRateInfo.HasErrors());
			AssertEquals("Checks if there is an expected line error", hasLineError, copiedInvoice.Lines[0].AL_ExchangeRateInfo.HasErrors());
			AssertEquals("ExchangeRate of line", lineExpectedRate, copiedInvoice.Lines[0].AL_ExchangeRate);
			AssertEquals("ExchangeRate of header", headerExpectedRate, copiedInvoice.AH_ExchangeRate);
		}

		[TestDate(2015, 1, 1)]
		public void TestCopyTransactionGetDefaultARInvoiceAndPostDate()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			ARDefaultInvoiceAndPostDateCalculatorTest.ValidateDefaultARInvoiceAndPostDate(invoice, () =>
			{
				var copiedInvoice = (ARInvoice)(invoice.CopyTransaction_ForTestOnly());
				return copiedInvoice;
			});
		}

		public void TestBankAccountCollectionFilter()
		{
			Assert(!TestInvoice.BankAccountCollectionFilter_ForTestOnly.IsEmpty);
		}

		public void TestBankAccountCollection_ContainsOnlyActiveBanks()
		{
			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			TestInvoice.BankAccountCollection.Load();
			AssertEquals("Should contain active bank", true, TestInvoice.BankAccountCollection.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, TestInvoice.BankAccountCollection.Contains(inactiveBank));
		}

		public void TestCheckBookCollectionFilter()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.NewWithValidTestData(typeof(AccChequeBook));
			TestInvoice.ReceiptPaymentAH_AB = bankAccount.PK;
			Assert(!TestInvoice.CheckBookCollectionFilter_ForTestOnly.IsEmpty);
		}

		public void TestReceiptPaymentChequeReferenceLabel()
		{
			TestInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			AssertEquals(TestInvoice.ChequeNumberLabel, TestInvoice.ReceiptPaymentAH_ChequeReferenceLabel);
			TestInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals(TestInvoice.ReferenceNumberLabel, TestInvoice.ReceiptPaymentAH_ChequeReferenceLabel);
		}

		public virtual void TestReceiptPaymentBankAccount()
		{
			AccBankAccount bankAccountDummy1 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccountDummy1.AB_IsDefaultReceiptBankAccount = ZBool.True;
			bankAccountDummy1.AB_RX_NKAccountCurrency = TestInvoice.AH_RX_NKTransactionCurrency;
			bankAccountDummy1.AB_GB = ZGuid.Empty;

			TestInvoice.ReceiptPaymentAH_AB = TestInvoice.DefaultBankAccount_ForTestOnly;
			AssertEquals(bankAccountDummy1, TestInvoice.ReceiptPaymentBankAccount);

			AccBankAccount bankAccountDummy2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccountDummy2.AB_IsDefaultReceiptBankAccount = ZBool.True;
			bankAccountDummy2.AB_RX_NKAccountCurrency = TestInvoice.AH_RX_NKTransactionCurrency;
			bankAccountDummy2.AB_GB = ZGuid.Empty;

			TestInvoice.ReceiptPaymentAH_AB = TestInvoice.DefaultBankAccount_ForTestOnly;
			AssertEquals(bankAccountDummy1, TestInvoice.ReceiptPaymentBankAccount);

			AccBankAccount bankAccountDummyWithBranch = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccountDummyWithBranch.AB_IsDefaultReceiptBankAccount = ZBool.True;
			bankAccountDummyWithBranch.AB_RX_NKAccountCurrency = TestInvoice.AH_RX_NKTransactionCurrency;
			bankAccountDummyWithBranch.AB_GB = GlbBranch.CurrentBranch.PK;

			TestInvoice.ReceiptPaymentAH_AB = TestInvoice.DefaultBankAccount_ForTestOnly;
			AssertEquals(bankAccountDummyWithBranch, TestInvoice.ReceiptPaymentBankAccount);

			GlbCompany diffCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch diffBranch = Factory.NewWithValidTestData<GlbBranch>();
			diffBranch.GB_GC = diffCompany.PK;
			RefCurrency overseasCurrency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount bankAccount_DiffCompany = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount_DiffCompany.AB_IsDefaultReceiptBankAccount = true;
			bankAccount_DiffCompany.AB_RX_NKAccountCurrency = overseasCurrency.RX_Code;
			bankAccount_DiffCompany.AB_GC = diffCompany.PK;
			//BankAccount_DiffCompany

			TestInvoice.AH_RX_NKTransactionCurrency = overseasCurrency.RX_Code;
			Assert("Default Bank Account should be empty", TestInvoice.DefaultBankAccount_ForTestOnly.IsEmpty);
		}

		public virtual void TestSetReceiptPaymentAK_AB()
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
			AssertEquals("Cheque Number should be populated", "2", testInvoice.ReceiptPaymentAH_ChequeOrReference);
			Assert("Cheque Number should not be readonly", !testInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo.ReadOnly);
		}

		public void TestCheckBookCollectionOfTheValidType()
		{
			Invoice testInvoice = Factory.New(GetExpectedBusinessObjectType()) as Invoice;
			AssertEquals("CheckBookCollection is of the valid type", typeof(ActiveChequeBookCollection), testInvoice.CheckBookCollection.GetType());
		}

		public virtual void TestSettingDrawerDetailsForInvoice()
		{
			TestObjectCreator.AALSHI.MiscServ.OM_ARPreviousChequeDrawer = "Cheque Drawer";
			TestObjectCreator.AALSHI.MiscServ.OM_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestObjectCreator.AALSHI.MiscServ.OM_ARPreviousChequeDrawerBankBranch = "Drawer Branch";
			Factory.Save();

			TestInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			TestInvoice.AH_TransactionNum = "Tran.Num";
			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = TestObjectCreator.AUDChequeBook.PK;
			TestInvoice.IsInvoiceReceiptPayment = true;

			Factory.Save();
			AssertNotNull("Receipt Payment not null", TestInvoice.ReceiptPayment);
		}

		public virtual void TestCheckBankAccountAndCheckBookValidationOnChangingPaymentType()
		{
			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;
			TestInvoice.AH_InvoiceDate = ZDateTime.Now;
			TestInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;

			AssertNoNotifications("Please enter a Check Book.", TestInvoice.ReceiptPaymentAK_ABInfo);
			AssertNoNotifications("Please enter a value.", TestInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateReceiptPaymentCorrectlyWhenBothCurrenciesAreForeign()
		{
			bool previousExRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				ZDateTime now = ZDateTime.Now;
				TestObjectCreator creator = new TestObjectCreator(Factory);
				TestInvoice.AH_OH = creator.AALSHI.PK;
				TestInvoice.AH_TransactionNum = "Tran.Num";
				TestInvoice.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
				TestInvoice.AH_ExchangeRate = 0.65m;

				ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
				TestInvoice.AH_PostDate = postDate;

				RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_RX_NKExCurrency = creator.USD.RX_Code;
				exRate.RE_ExRateType = TestInvoice.Ledger_ForTestOnly == LedgerTypes.AccountsPayable ? Constants.ExchangeRateTypes.Code.BuyRate : Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
				exRate.RE_SellRate = 0.77m; // different to rate set on invoice

				InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
				InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();

				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line1.AL_OSExTaxAmount = 200m;
				line1.AL_AT = creator.GST1.PK;
				line1.AL_OSTaxAmount = 20m;

				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_OSExTaxAmount = 100m;
				line2.AL_AT = creator.GST1.PK;
				line2.AL_OSTaxAmount = 5m;

				TestInvoice.SubmittedFromInvoicingForm = true;
				TestInvoice.IsInvoiceReceiptPayment = true;
				TestInvoice.ReceiptPaymentAH_AB = creator.USDBankAccount.PK;
				TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
				TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
				TestInvoice.ReceiptPaymentAH_DrawerBank = "Drawer Bank";
				TestInvoice.ReceiptPaymentAH_DrawerBranch = "Drawer Branch";
				TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				TestInvoice.ReceiptPaymentAK_AB = creator.AUDChequeBook.PK;

				TestInvoice.Factory.Save();

				ReceiptPaymentBase receiptOrPayment = TestInvoice.ReceiptPayment;

				AssertEquals("Receipt/Payment total amount - should be local value of invoice", 325m, receiptOrPayment.AH_OSExTaxAmount);
				AssertEquals("Receipt/Payment total local amount", 500.00m, receiptOrPayment.AH_LocalExTaxAmount);
				AssertEquals("Receipt/Payment exchange rate should be same as invoice", 0.65m, receiptOrPayment.AH_ExchangeRate);
				AssertEquals("Currency of receipt or payment should be currency of bank account", creator.USDBankAccount.AB_RX_NKAccountCurrency, receiptOrPayment.AH_RX_NKTransactionCurrency);

				AssertEquals("Receipt/Payment GST amount is always zero", 0m, receiptOrPayment.AH_OSTaxAmount);
				AssertEquals("Receipt/Payment outstanding amount should be zero because it gets matched",
					0m, receiptOrPayment.AH_Calc_OSOutstandingAmount);

				AssertEquals("Receipt/Payment Organisation should be same as invoice", creator.AALSHI.PK, receiptOrPayment.AH_OH);
				AssertZDatesWithin5Minutes(AccTransactionHeaderSchema.Constants.AH_FullyPaidDate, receiptOrPayment.AH_PostDate.Date, receiptOrPayment.AH_FullyPaidDate);

				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
				matchLinks.Load();
				AssertEquals("Should be two match link rows generated", 2, matchLinks.Count);

				TransactionMatchLink[] matchLinksForInvoices = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestInvoice.PK));
				TransactionMatchLink[] matchLinksForReceiptOrPayment = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptOrPayment.PK));

				AssertEquals("Match Link for invoice should have been posted", 1, matchLinksForInvoices.Length);
				AssertEquals("Match Link for receipt/payment should have been posted", 1, matchLinksForReceiptOrPayment.Length);

				TransactionMatchLink matchLinkForInvoice = matchLinksForInvoices[0];
				TransactionMatchLink matchLinkForReceiptOrPayment = matchLinksForReceiptOrPayment[0];

				AssertEquals("Match amount for invoice matchlink", TestInvoice.AH_GSTAmount + TestInvoice.AH_InvoiceAmount, matchLinkForInvoice.AP_Amount);
				AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForInvoice.AP_MatchDate);

				AssertEquals("Match amount for receipt matchlink", receiptOrPayment.AH_InvoiceAmount, matchLinkForReceiptOrPayment.AP_Amount);
				AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForReceiptOrPayment.AP_MatchDate);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			}
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateReceiptPaymentCorrectlyWhenInvoiceIsForeignAndPaymentReceiptIsLocal()
		{
			ZDateTime now = ZDateTime.Now;
			TestObjectCreator creator = new TestObjectCreator(Factory);
			TestInvoice.AH_OH = creator.AALSHI.PK;
			TestInvoice.AH_TransactionNum = "Tran.Num";
			TestInvoice.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			TestInvoice.AH_ExchangeRate = 0.65m;

			ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
			TestInvoice.AH_PostDate = postDate;

			InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();

			line1.AL_OSExTaxAmount = 200m;
			line1.AL_AT = creator.GST1.PK;
			line1.AL_OSTaxAmount = 20m;
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;

			line2.AL_OSExTaxAmount = 100m;
			line2.AL_AT = creator.GST1.PK;
			line2.AL_OSTaxAmount = 5m;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;
			TestInvoice.ReceiptPaymentAH_AB = creator.AUDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "Drawer Bank";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Drawer Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = creator.AUDChequeBook.PK;

			TestInvoice.Factory.Save();

			ReceiptPaymentBase receiptOrPayment = TestInvoice.ReceiptPayment;

			AssertEquals("Receipt/Payment total amount - should be local value of invoice", 500m, receiptOrPayment.AH_OSExTaxAmount);
			AssertEquals("Receipt/Payment total local amount", 500.00m, receiptOrPayment.AH_LocalExTaxAmount);
			AssertEquals("Receipt/Payment exchange rate", 1m, receiptOrPayment.AH_ExchangeRate);
			AssertEquals("Currency of receipt or payment should be local because bank account is local", creator.AUDBankAccount.AB_RX_NKAccountCurrency, receiptOrPayment.AH_RX_NKTransactionCurrency);

			AssertEquals("Receipt/Payment GST amount is always zero", 0m, receiptOrPayment.AH_OSTaxAmount);
			AssertEquals("Receipt/Payment outstanding amount should be zero because it gets matched",
				0m, receiptOrPayment.AH_Calc_OSOutstandingAmount);

			AssertEquals("Receipt/Payment currency should be currency of bank account", creator.AUDBankAccount.AB_RX_NKAccountCurrency, receiptOrPayment.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange rate for local currency payment/Receipt should be 1", 1m, receiptOrPayment.AH_ExchangeRate);
			AssertEquals("Receipt/Payment Organisation should be same as invoice", creator.AALSHI.PK, receiptOrPayment.AH_OH);
			AssertZDatesWithin5Minutes(AccTransactionHeaderSchema.Constants.AH_FullyPaidDate, receiptOrPayment.AH_PostDate.Date, receiptOrPayment.AH_FullyPaidDate);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load();
			AssertEquals("Should be two match link rows generated", 2, matchLinks.Count);

			TransactionMatchLink[] matchLinksForInvoices = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestInvoice.PK));
			TransactionMatchLink[] matchLinksForReceiptOrPayment = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptOrPayment.PK));

			AssertEquals("Match Link for invoice should have been posted", 1, matchLinksForInvoices.Length);
			AssertEquals("Match Link for receipt/payment should have been posted", 1, matchLinksForReceiptOrPayment.Length);

			TransactionMatchLink matchLinkForInvoice = matchLinksForInvoices[0];
			TransactionMatchLink matchLinkForReceiptOrPayment = matchLinksForReceiptOrPayment[0];

			AssertEquals("Match amount for invoice matchlink", TestInvoice.AH_GSTAmount + TestInvoice.AH_InvoiceAmount, matchLinkForInvoice.AP_Amount);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForInvoice.AP_MatchDate);

			AssertEquals("Match amount for receipt matchlink", receiptOrPayment.AH_InvoiceAmount, matchLinkForReceiptOrPayment.AP_Amount);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForReceiptOrPayment.AP_MatchDate);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateClearingJournalForCashInvoice()
		{
			AccGLHeader glAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			TestInvoice.AH_OH = creator.AALSHI.PK;
			TestInvoice.AH_TransactionNum = "Tran.Num";
			TestInvoice.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			TestInvoice.AH_ExchangeRate = 0.65m;

			ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
			TestInvoice.AH_PostDate = postDate;

			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_RX_NKExCurrency = creator.USD.RX_Code;
			exRate.RE_ExRateType = TestInvoice.Ledger_ForTestOnly == LedgerTypes.AccountsPayable ? Constants.ExchangeRateTypes.Code.BuyRate : Constants.ExchangeRateTypes.Code.SellRate;
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exRate.RE_SellRate = 0.77m; // different to rate set on invoice

			InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_OSExTaxAmount = 200m;
			line1.AL_AT = creator.GST1.PK;
			line1.AL_OSTaxAmount = 20m;

			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_OSExTaxAmount = 100m;
			line2.AL_AT = creator.GST1.PK;
			line2.AL_OSTaxAmount = 5m;

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;
			TestInvoice.ReceiptPaymentAH_AB = creator.USDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "Drawer Bank";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Drawer Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = creator.AUDChequeBook.PK;

			TestInvoice.Factory.Save();

			ReceiptPaymentBase receiptOrPayment = TestInvoice.ReceiptPayment;

			AssertEquals("Receipt/Payment total amount - should be local value of invoice", 325m, receiptOrPayment.AH_OSExTaxAmount);
			AssertEquals("Receipt/Payment total local amount", 500.00m, receiptOrPayment.AH_LocalExTaxAmount);

			AssertEquals("Receipt/Payment outstanding amount should be zero because it gets matched",
				0m, receiptOrPayment.AH_Calc_OSOutstandingAmount);

			AssertEquals("Receipt/Payment Organisation should be same as invoice", creator.AALSHI.PK, receiptOrPayment.AH_OH);
			AssertZDatesWithin5Minutes(AccTransactionHeaderSchema.Constants.AH_FullyPaidDate, receiptOrPayment.AH_PostDate.Date, receiptOrPayment.AH_FullyPaidDate);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load();
			AssertEquals("Should be two match link rows generated", 4, matchLinks.Count);

			TransactionMatchLink[] matchLinksForInvoices = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestInvoice.PK));
			TransactionMatchLink[] matchLinksForReceiptOrPayment = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptOrPayment.PK));

			AssertEquals("Match Link for invoice should have been posted", 1, matchLinksForInvoices.Length);
			AssertEquals("Match Link for receipt/payment should have been posted", 1, matchLinksForReceiptOrPayment.Length);

			TransactionMatchLink matchLinkForInvoice = matchLinksForInvoices[0];
			TransactionMatchLink matchLinkForReceiptOrPayment = matchLinksForReceiptOrPayment[0];

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchLinkForInvoice.AP_MatchGroupNum);
			filter.AddToFilter(AccTransactionMatchLinkSchema.PK, SQLComparisonOperator.NotEqual, new[] { matchLinkForInvoice.PK, matchLinkForReceiptOrPayment.PK });
			TransactionMatchLink[] matchLinksForClearingJournals = (TransactionMatchLink[])matchLinks.Find(filter);
			AssertEquals("Match Links for clearing journals should have been posted", 2, matchLinksForClearingJournals.Length);

			AssertEquals("Match amount for invoice matchlink", TestInvoice.AH_GSTAmount + TestInvoice.AH_InvoiceAmount, matchLinkForInvoice.AP_Amount);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForInvoice.AP_MatchDate);

			AssertEquals("Match amount for receipt matchlink", receiptOrPayment.AH_InvoiceAmount, matchLinkForReceiptOrPayment.AP_Amount);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForReceiptOrPayment.AP_MatchDate);

			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinksForClearingJournals[0].AP_MatchDate);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinksForClearingJournals[1].AP_MatchDate);
			if (-receiptOrPayment.AH_InvoiceAmount == matchLinksForClearingJournals[0].AP_Amount)
			{
				AssertEquals("Match amount for receipt matchlink", -receiptOrPayment.AH_InvoiceAmount, matchLinksForClearingJournals[0].AP_Amount);
				AssertEquals("Match amount for invoice matchlink", -TestInvoice.AH_GSTAmount - TestInvoice.AH_InvoiceAmount, matchLinksForClearingJournals[1].AP_Amount);
			}
			else
			{
				AssertEquals("Match amount for receipt matchlink", -receiptOrPayment.AH_InvoiceAmount, matchLinksForClearingJournals[1].AP_Amount);
				AssertEquals("Match amount for invoice matchlink", -TestInvoice.AH_GSTAmount - TestInvoice.AH_InvoiceAmount, matchLinksForClearingJournals[0].AP_Amount);
			}
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestMultipleCallHandleReceiptPaymentIsSafeAndDataIsUpdatedBeforeSaving()
		{
			AccGLHeader glAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			TestInvoice.AH_OH = creator.AALSHI.PK;
			TestInvoice.AH_TransactionNum = "Tran.Num";
			TestInvoice.AH_RX_NKTransactionCurrency = creator.USD.RX_Code;
			TestInvoice.AH_ExchangeRate = 0.65m;

			ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
			TestInvoice.AH_PostDate = postDate;

			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_RX_NKExCurrency = creator.USD.RX_Code;
			exRate.RE_ExRateType = TestInvoice.Ledger_ForTestOnly == LedgerTypes.AccountsPayable ? Constants.ExchangeRateTypes.Code.BuyRate : Constants.ExchangeRateTypes.Code.SellRate;
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exRate.RE_SellRate = 0.77m; // different to rate set on invoice

			InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();

			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_AT = creator.GST1.PK;
			line1.AL_OSTaxAmount = 10m;

			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_OSExTaxAmount = 100m;
			line2.AL_AT = creator.GST1.PK;
			line2.AL_OSTaxAmount = 5m;

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;
			TestInvoice.ReceiptPaymentAH_AB = creator.USDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "Drawer Bank";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Drawer Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = creator.AUDChequeBook.PK;

			TestInvoice.HandleReceiptPayment();
			TestInvoice.HandleReceiptPayment();

			line1.AL_OSExTaxAmount = 200m;
			line1.AL_OSTaxAmount = 20m;

			TestInvoice.Factory.Save();

			ReceiptPaymentBase receiptOrPayment = TestInvoice.ReceiptPayment;

			AssertEquals("Receipt/Payment total amount - should be local value of invoice", 325m, receiptOrPayment.AH_OSExTaxAmount);
			AssertEquals("Receipt/Payment total local amount", 500.00m, receiptOrPayment.AH_LocalExTaxAmount);

			AssertEquals("Receipt/Payment outstanding amount should be zero because it gets matched",
				0m, receiptOrPayment.AH_Calc_OSOutstandingAmount);

			AssertEquals("Receipt/Payment Organisation should be same as invoice", creator.AALSHI.PK, receiptOrPayment.AH_OH);
			AssertZDatesWithin5Minutes(AccTransactionHeaderSchema.Constants.AH_FullyPaidDate, receiptOrPayment.AH_PostDate.Date, receiptOrPayment.AH_FullyPaidDate);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load();
			AssertEquals("Should be two match link rows generated", 4, matchLinks.Count);

			TransactionMatchLink[] matchLinksForInvoices = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestInvoice.PK));
			TransactionMatchLink[] matchLinksForReceiptOrPayment = (TransactionMatchLink[])matchLinks.Find(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptOrPayment.PK));

			AssertEquals("Match Link for invoice should have been posted", 1, matchLinksForInvoices.Length);
			AssertEquals("Match Link for receipt/payment should have been posted", 1, matchLinksForReceiptOrPayment.Length);

			TransactionMatchLink matchLinkForInvoice = matchLinksForInvoices[0];
			TransactionMatchLink matchLinkForReceiptOrPayment = matchLinksForReceiptOrPayment[0];

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchLinkForInvoice.AP_MatchGroupNum);
			filter.AddToFilter(AccTransactionMatchLinkSchema.PK, SQLComparisonOperator.NotEqual, new[] { matchLinkForInvoice.PK, matchLinkForReceiptOrPayment.PK });
			TransactionMatchLink[] matchLinksForClearingJournals = (TransactionMatchLink[])matchLinks.Find(filter);
			AssertEquals("Match Links for clearing journals should have been posted", 2, matchLinksForClearingJournals.Length);

			AssertEquals("Match amount for invoice matchlink", TestInvoice.AH_GSTAmount + TestInvoice.AH_InvoiceAmount, matchLinkForInvoice.AP_Amount);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForInvoice.AP_MatchDate);

			AssertEquals("Match amount for receipt matchlink", receiptOrPayment.AH_InvoiceAmount, matchLinkForReceiptOrPayment.AP_Amount);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinkForReceiptOrPayment.AP_MatchDate);

			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinksForClearingJournals[0].AP_MatchDate);
			AssertZDatesWithin5Minutes(AccTransactionMatchLinkSchema.Constants.AP_MatchDate, TestInvoice.AH_PostDate.Date, matchLinksForClearingJournals[1].AP_MatchDate);
			if (-receiptOrPayment.AH_InvoiceAmount == matchLinksForClearingJournals[0].AP_Amount)
			{
				AssertEquals("Match amount for receipt matchlink", -receiptOrPayment.AH_InvoiceAmount, matchLinksForClearingJournals[0].AP_Amount);
				AssertEquals("Match amount for invoice matchlink", -TestInvoice.AH_GSTAmount - TestInvoice.AH_InvoiceAmount, matchLinksForClearingJournals[1].AP_Amount);
			}
			else
			{
				AssertEquals("Match amount for receipt matchlink", -receiptOrPayment.AH_InvoiceAmount, matchLinksForClearingJournals[1].AP_Amount);
				AssertEquals("Match amount for invoice matchlink", -TestInvoice.AH_GSTAmount - TestInvoice.AH_InvoiceAmount, matchLinksForClearingJournals[0].AP_Amount);
			}
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public virtual void TestCashPaymentReceiptDetailsSetOnSaving()
		{
			ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
			TestInvoice.AH_PostDate = postDate;
			TestInvoice.SubmittedFromInvoicingForm = true; // So that when this object is used in other places, it doesn't create unexpected payments/receipts - this would be a problem - See Imraan for details
			TestInvoice.IsInvoiceReceiptPayment = true;

			TestDataCreator.AALSHI.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
			TestDataCreator.AALSHI.CompanyData.OB_APPayInvoiceAfterPostingDefault = true;
			TestInvoice.AH_OH = TestDataCreator.AALSHI.PK;

			InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();

			line1.AL_OSExTaxAmount = 200.00m;
			line2.AL_OSExTaxAmount = 100.00m;

			TestDataCreator.AUDBankAccount.AB_ChequeNumDigits = 5;
			TestInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;

			try
			{
				Db.Connection.BeginTransaction();
				TestInvoice.OnSaving();

				// ReceiptPayment values should be set
				AssertEquals("AH_OH should be set", TestDataCreator.AALSHI.PK, TestInvoice.ReceiptPayment.AH_OH);

				ZString correctDescription = TestInvoice.AH_Ledger == LedgerTypes.AccountsReceivable ? "AR CASH RECEIPT" : "AP CASH PAYMENT";
				AssertEquals("Description should be set", correctDescription, TestInvoice.ReceiptPayment.AH_Desc);
				AssertEquals("Currency should be same as invoice", TestInvoice.AH_RX_NKTransactionCurrency, TestInvoice.ReceiptPayment.AH_RX_NKTransactionCurrency);
				//AssertEquals("ExchangeRate should be same as invoice", 0.7M, TestInvoice.ReceiptPayment.AH_ExchangeRate);
				AssertEquals("Post Date should be same as invoice", postDate, TestInvoice.ReceiptPayment.AH_PostDate);
				AssertEquals("FullyPaidDate should be same as invoice", TestInvoice.ReceiptPayment.AH_PostDate.Date, TestInvoice.ReceiptPayment.AH_FullyPaidDate);
				AssertEquals("InvoiceDate should be same as invoice", TestInvoice.AH_InvoiceDate, TestInvoice.ReceiptPayment.AH_InvoiceDate);
				AssertEquals("OSExTaxAmount should be same as invoice", TestInvoice.AH_OSExTaxAmount, TestInvoice.ReceiptPayment.AH_OSExTaxAmount);
				AssertEquals("Outstanding amount should be 0", 0M, TestInvoice.ReceiptPayment.AH_OutstandingAmount);
				AssertEquals("AH_AB should be AUDBankAccount", TestDataCreator.AUDBankAccount.PK, TestInvoice.ReceiptPayment.AH_AB);
				AssertEquals("ChequeDrawer should be 'ChequeDrawer'", "Cheque Drawer", TestInvoice.ReceiptPayment.AH_ChequeDrawer);

				if (TestInvoice is APInvoice)
				{
					AssertEquals("ChequeNumber should be 00001", "00001", TestInvoice.ReceiptPayment.AH_ChequeOrReference);
				}
				else if (TestInvoice is ARInvoice)
				{
					AssertEquals("ChequeNumber should be 1", "1", TestInvoice.ReceiptPayment.AH_ChequeOrReference);
				}
				AssertEquals("Drawer bank should be NAB", "NAB", TestInvoice.ReceiptPayment.AH_DrawerBank);
				AssertEquals("Drawer branch should be Branch", "Branch", TestInvoice.ReceiptPayment.AH_DrawerBranch);
				AssertEquals("ReceiptType should be Cheque", ZArchitecture.Core.ReceiptTypes.Cheque, TestInvoice.ReceiptPayment.AH_ReceiptType);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public virtual void TestCreateInvoiceAndReceiptMatchLinkChooseCorrectMatchDate()
		{
			ZDateTime invoicePostDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate.AddDays(-20).Date;
			ZDateTime invoiceInvoicingDate = invoicePostDate.AddDays(-1);
			ZDateTime receiptPaymentInvoicingDate = invoicePostDate.AddDays(5);
			ZDateTime receiptPaymentPostDate = invoicePostDate.AddDays(10);

			TestInvoice.AH_PostDate = invoicePostDate;
			TestInvoice.AH_InvoiceDate = invoiceInvoicingDate;
			TestInvoice.SubmittedFromInvoicingForm = true; // So that when this object is used in other places, it doesn't create unexpected payments/receipts - this would be a problem - See Imraan for details
			TestInvoice.IsInvoiceReceiptPayment = true;

			TestDataCreator.AALSHI.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
			TestDataCreator.AALSHI.CompanyData.OB_APPayInvoiceAfterPostingDefault = true;
			TestInvoice.AH_OH = TestDataCreator.AALSHI.PK;

			InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();
			line1.AL_OSExTaxAmount = 200.00m;
			line2.AL_OSExTaxAmount = 100.00m;

			TestDataCreator.AUDBankAccount.AB_ChequeNumDigits = 5;
			TestInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;
			TestInvoice.ReceiptPaymentAH_InvoiceDate = receiptPaymentInvoicingDate;
			TestInvoice.ReceiptPaymentAH_PostDate = receiptPaymentPostDate;

			try
			{
				Db.Connection.BeginTransaction();
				TestInvoice.OnSaving();

				if ((TestInvoice is APInvoice && !(TestInvoice is UAInvoice)) || TestInvoice is ARInvoice)
				{
					AssertEquals("Invoice's Post Date", invoicePostDate, TestInvoice.AH_PostDate);
					AssertEquals("Invoice's Invoicing Date", invoiceInvoicingDate, TestInvoice.AH_InvoiceDate);
					AssertEquals("Invoice's Final Paid Date should be ReceiptPayment's PostDate as it's greater than Invoice's PostDate", receiptPaymentPostDate, TestInvoice.AH_FullyPaidDate);

					AssertEquals("ReceiptPayment's Invoicing Date", receiptPaymentInvoicingDate, TestInvoice.ReceiptPayment.AH_InvoiceDate);
					AssertEquals("ReceiptPayment's Post Date", receiptPaymentPostDate, TestInvoice.ReceiptPayment.AH_PostDate);
					AssertEquals("ReceiptPayment's Final Paid Date should be ReceiptPayment's PostDate as it's greater than Invoice's PostDate", receiptPaymentPostDate, TestInvoice.ReceiptPayment.AH_FullyPaidDate);
				}
				else
				{
					Assert(true);
				}
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public virtual void TestCallingOnSavingTwiceDoesntCreateTwoPayments()
		{
			ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
			RefCurrency ausCur = Factory.New<RefCurrency>();
			ausCur.RX_Code = "AU";
			RefCurrency uSCur = Factory.New<RefCurrency>();
			uSCur.RX_Code = "US";
			TestInvoice.AH_PostDate = postDate;
			TestInvoice.SubmittedFromInvoicingForm = true; // So that when this object is used in other places, it doesn't create unexpected payments/receipts - this would be a problem - See Imraan for details
			TestInvoice.IsInvoiceReceiptPayment = true;

			TestDataCreator.AALSHI.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
			TestDataCreator.AALSHI.CompanyData.OB_APPayInvoiceAfterPostingDefault = true;
			TestInvoice.AH_OH = TestDataCreator.AALSHI.PK;

			InvoiceLine line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			InvoiceLine line2 = (InvoiceLine)TestInvoice.Lines.AddNew();

			line1.AL_OSExTaxAmount = 200.00m;
			line2.AL_OSExTaxAmount = 100.00m;

			TestDataCreator.AUDBankAccount.AB_ChequeNumDigits = 5;
			TestInvoice.Header.CompanyData.OB_RX_NKAPDefltCurrency = ausCur.RX_Code;
			TestInvoice.Header.CompanyData.OB_RX_NKARDDefltCurrency = uSCur.RX_Code;
			TestInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				TestInvoice.OnSaving();
				TestInvoice.OnSaving();
			}
			TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(Factory);
			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestInvoice.PK);
			filter.AddToFilter(JoinCondition.Or, AccTransactionMatchLinkSchema.AP_AH, TestInvoice.ReceiptPayment.PK);
			matchLinkCollection.Load(filter);
			AssertEquals("matchLinkCollection.Count", 2, matchLinkCollection.Count);
		}

		public virtual void TestCashBasisVATForCashInvoice()
		{
			var currentCompany = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsGSTCashBasis = true;
			currentCompany.Factory.Save();

			var chargeCode = TestDataCreator.CC1;
			AssertEquals("Precondition: AC_GoodsServiceType", GoodServiceTypes.Codes.SRV, chargeCode.AC_GoodsServiceType);
			var taxRecognitionDefaultingRules = new TaxRecognitionDefaultingRules()
			{
				APInputServices = TaxRecognitionDefaultingRules.RecognitionTypesCashCode,
				AROutputServices = TaxRecognitionDefaultingRules.RecognitionTypesCashCode
			};
			AccountingConfigurationRegistry.Instance.TaxRecognitionDefaultingRules.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, taxRecognitionDefaultingRules);
			Factory.Save();

			var job = TestDataCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, false);

			ZDateTime postDate = PeriodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate;
			RefCurrency ausCur = Factory.New<RefCurrency>();
			ausCur.RX_Code = "AU";
			RefCurrency uaCur = Factory.New<RefCurrency>();
			uaCur.RX_Code = "US";
			TestInvoice.AH_PostDate = postDate;
			TestInvoice.SubmittedFromInvoicingForm = true; // So that when this object is used in other places, it doesn't create unexpected payments/receipts - this would be a problem - See Imraan for details
			TestInvoice.IsInvoiceReceiptPayment = true;

			TestDataCreator.AALSHI.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
			TestDataCreator.AALSHI.CompanyData.OB_APPayInvoiceAfterPostingDefault = true;
			TestInvoice.AH_OH = TestDataCreator.AALSHI.PK;

			var line1 = (InvoiceLine)TestInvoice.Lines.AddNew();
			var line2 = (InvoiceLine)TestInvoice.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line1.GenericCharge = TestDataCreator.CC1.PK;
			line1.AL_JH = job.PK;
			AssertNotNull("Precondition: invoice Tax Rate", line1.TaxRate);

			line1.AL_OSExTaxAmount = 200.00m;
			line2.AL_OSExTaxAmount = 100.00m;
			if (TestInvoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				TestDataCreator.CreateCharge(line1);
			}

			TestDataCreator.AUDBankAccount.AB_ChequeNumDigits = 5;
			TestInvoice.Header.CompanyData.OB_RX_NKAPDefltCurrency = ausCur.RX_Code;
			TestInvoice.Header.CompanyData.OB_RX_NKARDDefltCurrency = uaCur.RX_Code;
			TestInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
			TestInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			TestInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			TestInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
			TestInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
			TestInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;

			Factory.Save();
			TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(Factory);
			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestInvoice.PK);
			filter.AddToFilter(JoinCondition.Or, AccTransactionMatchLinkSchema.AP_AH, TestInvoice.ReceiptPayment.PK);
			matchLinkCollection.Load(filter);
			AssertEquals("Precondition: matchLinkCollection.Count", 2, matchLinkCollection.Count);

			AssertEquals("AL_GSTVATBasis must be set to Cash.", AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code, line1.AL_GSTVATBasis);
			var cashBasisVATs = new BusinessObjectFactory().Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, line1.PK));
			AssertEquals("CashBasisVAT record must be created for the invoice line with Cash basis tax.", 1, cashBasisVATs.Length);
			var cashBasisVAT = cashBasisVATs[0];
			AssertEquals("YC_MatchGroupNum", matchLinkCollection[0].AP_MatchGroupNum, cashBasisVAT.YC_MatchGroupNum);
			AssertEquals("YC_PostDate", matchLinkCollection[0].AP_MatchDate, cashBasisVAT.YC_PostDate);
			AssertEquals("YC_TaxBaseAmount", line1.AL_LineAmount, cashBasisVAT.YC_TaxBaseAmount);
			AssertEquals("YC_TaxAmount", line1.AL_GSTVAT, cashBasisVAT.YC_TaxAmount);

			AssertEquals("AL_GSTVATBasis must not be set to Cash as line is not setup for Cash Basis VAT.", AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code, line2.AL_GSTVATBasis);
			var cashBasisVATs2 = new BusinessObjectFactory().Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, line2.PK));
			AssertEquals("CashBasisVAT record must not be created for the invoice line with Accrual basis tax.", 0, cashBasisVATs2.Length);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestBankAccountValidation()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			RefCurrency overseasCurrency = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount bank = GetTestBankAccount(company, overseasCurrency);

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;
			TestInvoice.AH_RX_NKTransactionCurrency = overseasCurrency.RX_Code;
			TestInvoice.BankAccountLookup.Load();
			Assert("BankLookups should not contain test Bank", !TestInvoice.BankAccountLookup.Contains(bank.PK));
			TestInvoice.ReceiptPaymentAH_AB = bank.PK;
			((InvoiceValidation)TestInvoice.Validation).ValidateReceiptPaymentAH_AB();
			Assert("Bank should have errors since it is not for the current company", TestInvoice.ReceiptPaymentAH_ABInfo.HasErrors());
		}

		public void TestReceiptPaymentLocalAmount()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			TestInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			TestInvoice.AH_OSExTaxAmount = 90m;
			TestInvoice.AH_LocalExTaxAmount = 80m;
			TestInvoice.ReceiptPaymentAH_AB = bank.PK;

			ReceiptPaymentBase recPayBase = TestInvoice.CreateReceiptPayment_ForTestOnly();
			AssertEquals("OSExTaxAmount should be 90", 90m, recPayBase.AH_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount should be 80", 80m, recPayBase.AH_LocalExTaxAmount);
		}

		public void TestReceiptPaymentLocalAmountCountOnOtherTaxesAmount()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			TestInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			TestInvoice.AH_OSExTaxAmount = 90m;
			TestInvoice.AH_LocalExTaxAmount = 80m;
			TestInvoice.AH_OSTaxAmountOtherTaxes = 3m * TestInvoice.Multiplier_ForTestOnly;
			TestInvoice.AH_LocalTaxAmountOtherTaxes = 2m * TestInvoice.Multiplier_ForTestOnly;
			TestInvoice.ReceiptPaymentAH_AB = bank.PK;

			ReceiptPaymentBase recPayBase = TestInvoice.CreateReceiptPayment_ForTestOnly();
			AssertEquals("OSExTaxAmount", 93m, recPayBase.AH_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 82m, recPayBase.AH_LocalExTaxAmount);

			TestInvoice.ReceiptPayment = null;

			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			recPayBase = TestInvoice.CreateReceiptPayment_ForTestOnly();
			AssertEquals("OSExTaxAmount", 82m, recPayBase.AH_OSExTaxAmount);
			AssertEquals("LocalExTaxAmount", 82m, recPayBase.AH_LocalExTaxAmount);
		}

		public void TestReceiptPaymentPostDateAndInvoiceDate()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			TestInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			TestInvoice.AH_OSExTaxAmount = 90m;
			TestInvoice.AH_LocalExTaxAmount = 80m;
			TestInvoice.ReceiptPaymentAH_AB = bank.PK;
			TestInvoice.AH_PostDate = new ZDateTime(2011, 10, 21);
			TestInvoice.AH_InvoiceDate = new ZDateTime(2011, 10, 15);

			TestInvoice.SetReceiptPaymentDefaults();
			AssertEquals("default PostDate should be invoice's PostDate", new ZDateTime(2011, 10, 21), TestInvoice.ReceiptPaymentAH_PostDate);
			AssertEquals("default InvoiceDate should be invoice's InvoiceDate", new ZDateTime(2011, 10, 15), TestInvoice.ReceiptPaymentAH_InvoiceDate);

			TestInvoice.ReceiptPaymentAH_PostDate = new ZDateTime(2011, 11, 13);
			TestInvoice.ReceiptPaymentAH_InvoiceDate = new ZDateTime(2011, 11, 08);
			ReceiptPaymentBase recPayBase = TestInvoice.CreateReceiptPayment_ForTestOnly();

			AssertEquals("default PostDate should be ReceiptPaymentAH_PostDate", new ZDateTime(2011, 11, 13), recPayBase.AH_PostDate);
			AssertEquals("default InvoiceDate should be ReceiptPaymentAH_InvoiceDate", new ZDateTime(2011, 11, 08), recPayBase.AH_InvoiceDate);

			TestInvoice.IsReceiptPaymentFromFileImport = true;
			TestInvoice.SetReceiptPaymentDefaults();

			AssertEquals("If receipt payment is imported from a file, default values shouldn't be set", new ZDateTime(2011, 11, 13), TestInvoice.ReceiptPaymentAH_PostDate);
			AssertEquals("If receipt payment is imported from a file, default values shouldn't be set", new ZDateTime(2011, 11, 08), TestInvoice.ReceiptPaymentAH_InvoiceDate);

			TestInvoice.IsReceiptPaymentFromFileImport = false;
			TestInvoice.SetReceiptPaymentDefaults();

			AssertEquals("If receipt payment is made in a form, default values should be set", new ZDateTime(2011, 10, 21), TestInvoice.ReceiptPaymentAH_PostDate);
			AssertEquals("If receipt payment is made in a form, default values should be set", new ZDateTime(2011, 10, 15), TestInvoice.ReceiptPaymentAH_InvoiceDate);
		}

		public void TestClearReceiptPaymentFields()
		{
			RefCurrency currency = Factory.New<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			TestInvoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			TestInvoice.AH_OSExTaxAmount = 90m;
			TestInvoice.AH_LocalExTaxAmount = 80m;
			TestInvoice.ReceiptPaymentAH_AB = bank.PK;
			TestInvoice.AH_PostDate = new ZDateTime(2011, 10, 21);
			TestInvoice.AH_InvoiceDate = new ZDateTime(2011, 10, 15);

			TestInvoice.ClearReceiptPaymentFields_ForTestOnly();
			AssertEquals("ReceiptPaymentAH_ReceiptType should be", ZString.Empty, TestInvoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("ReceiptPaymentAH_AB should be", ZGuid.Empty, TestInvoice.ReceiptPaymentAH_AB);
			AssertEquals("ReceiptPaymentAK_AB should be", ZGuid.Empty, TestInvoice.ReceiptPaymentAK_AB);
			AssertEquals("ReceiptPaymentAH_InvoiceDate", ZDateTime.Empty, TestInvoice.ReceiptPaymentAH_InvoiceDate);
			AssertEquals("ReceiptPaymentAH_PostDate", ZDateTime.Empty, TestInvoice.ReceiptPaymentAH_PostDate);
			AssertEquals("ReceiptPaymentAH_ChequeOrReference", ZString.Empty, TestInvoice.ReceiptPaymentAH_ChequeOrReference);
			AssertEquals("ReceiptPaymentAH_ChequeDrawer", ZString.Empty, TestInvoice.ReceiptPaymentAH_ChequeDrawer);
			AssertEquals("ReceiptPaymentAH_DrawerBank", ZString.Empty, TestInvoice.ReceiptPaymentAH_DrawerBank);
			AssertEquals("ReceiptPaymentAH_DrawerBranch", ZString.Empty, TestInvoice.ReceiptPaymentAH_DrawerBranch);

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = true;

			AssertEquals("ReceiptPaymentAH_ReceiptType should be", "CHQ", TestInvoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("ReceiptPaymentAH_AB should be", ZGuid.Empty, TestInvoice.ReceiptPaymentAH_AB);
			AssertEquals("ReceiptPaymentAK_AB should be", ZGuid.Empty, TestInvoice.ReceiptPaymentAK_AB);
			AssertEquals("ReceiptPaymentAH_InvoiceDate", new ZDateTime(2011, 10, 15), TestInvoice.ReceiptPaymentAH_InvoiceDate);
			AssertEquals("ReceiptPaymentAH_PostDate", new ZDateTime(2011, 10, 21), TestInvoice.ReceiptPaymentAH_PostDate);
			AssertEquals("ReceiptPaymentAH_ChequeOrReference", ZString.Empty, TestInvoice.ReceiptPaymentAH_ChequeOrReference);
			AssertEquals("ReceiptPaymentAH_ChequeDrawer", ZString.Empty, TestInvoice.ReceiptPaymentAH_ChequeDrawer);
			AssertEquals("ReceiptPaymentAH_DrawerBank", ZString.Empty, TestInvoice.ReceiptPaymentAH_DrawerBank);
			AssertEquals("ReceiptPaymentAH_DrawerBranch", ZString.Empty, TestInvoice.ReceiptPaymentAH_DrawerBranch);

			TestInvoice.SubmittedFromInvoicingForm = true;
			TestInvoice.IsInvoiceReceiptPayment = false;

			AssertEquals("ReceiptPaymentAH_ReceiptType should be", ZString.Empty, TestInvoice.ReceiptPaymentAH_ReceiptType);
			AssertEquals("ReceiptPaymentAH_AB should be", ZGuid.Empty, TestInvoice.ReceiptPaymentAH_AB);
			AssertEquals("ReceiptPaymentAK_AB should be", ZGuid.Empty, TestInvoice.ReceiptPaymentAK_AB);
			AssertEquals("ReceiptPaymentAH_InvoiceDate", ZDateTime.Empty, TestInvoice.ReceiptPaymentAH_InvoiceDate);
			AssertEquals("ReceiptPaymentAH_PostDate", ZDateTime.Empty, TestInvoice.ReceiptPaymentAH_PostDate);
			AssertEquals("ReceiptPaymentAH_ChequeOrReference", ZString.Empty, TestInvoice.ReceiptPaymentAH_ChequeOrReference);
			AssertEquals("ReceiptPaymentAH_ChequeDrawer", ZString.Empty, TestInvoice.ReceiptPaymentAH_ChequeDrawer);
			AssertEquals("ReceiptPaymentAH_DrawerBank", ZString.Empty, TestInvoice.ReceiptPaymentAH_DrawerBank);
			AssertEquals("ReceiptPaymentAH_DrawerBranch", ZString.Empty, TestInvoice.ReceiptPaymentAH_DrawerBranch);
		}

		Invoice fTestInvoice;
		protected Invoice TestInvoice
		{
			get
			{
				if (fTestInvoice == null)
				{
					fTestInvoice = (Invoice)Factory.New(GetExpectedBusinessObjectType());
					fTestInvoice.AH_TransactionNum = "Test Invoice";
				}
				return fTestInvoice;
			}
		}

		protected InvoiceLine GetTestInvoiceLine()
		{
			var line = (InvoiceLine)TestInvoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			return line;
		}

		protected AccBankAccount GetTestBankAccount(GlbCompany company, RefCurrency currency)
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			bank.AB_GC = company.PK;
			return bank;
		}

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			return chequeBook;
		}

		TaxFrameworkTestObjectCreator TFObjectCreator => tfObjectCreator ?? (tfObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator tfObjectCreator;
	}
}
