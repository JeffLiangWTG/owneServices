using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class ReceiptPaymentBaseTest : TransactionHeaderTest
	{
		#region Implementation

		protected virtual ReceiptPaymentBase ReceiptPaymentBase
		{
			get { return (ReceiptPaymentBase)Header; }
		}

		protected AccBankAccount GetNewBankAccountWithCurrency(RefCurrency currency, ZBool isDefaultForAR)
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			bank.AB_IsDefaultReceiptBankAccount = isDefaultForAR;
			return bank;
		}

		protected AccBankAccount GetNewBankAccountWithCompany(GlbCompany company)
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GC = company.PK;
			return bank;
		}

		protected AccBankAccount GetNewBankAccountWithCompanyAndCurrency(GlbCompany company, RefCurrency currency, ZBool isDefaultForAR)
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GC = company.PK;
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			bank.AB_IsDefaultReceiptBankAccount = isDefaultForAR;
			return bank;
		}

		protected AccChequeBook GetNewChequeBook(AccBankAccount bank, GlbBranch branch)
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 10;
			chequeBook.AK_AB = bank.PK;
			chequeBook.AK_GB = branch.PK;
			return chequeBook;
		}

		protected MatchingBase fMatchingBaseObject;

		#endregion

		public void TestInfoIsCollectedWhenOSPartialPaymentAmountIsGreaterThanOutstandingAmount()
		{
			if (ReceiptPaymentBase.AH_Ledger == LedgerTypes.AccountsPayable && ReceiptPaymentBase.AH_TransactionType == TransactionTypes.Payment)
			{
				ReceiptPaymentBase.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				ReceiptPaymentBase.AH_InvoiceAmount = 100m;
				ReceiptPaymentBase.AH_OSTotal = 100m;
				ReceiptPaymentBase.AH_OutstandingAmount = 100m;

				var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				string GetCriticalInfo(ZGuid paymentPk) => infoCollector.GetInfo(paymentPk, CriticalValidationInfoCollectorServiceKeyType.OSPartialPaymentAmountIsGreaterThanOutstandingAmount);

				ReceiptPaymentBase.OSPartialPaymentAmount = 50m;
				AssertContains("Info should not be collected because OSPartialPaymentAmount is less than AH_OutstandingAmount.", "OSPartialPaymentAmountIsGreaterThanOutstandingAmount: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", GetCriticalInfo(ReceiptPaymentBase.PK));

				ReceiptPaymentBase.OSPartialPaymentAmount = 200m;
				var info = GetCriticalInfo(ReceiptPaymentBase.PK);
				AssertContains("OSPartialPaymentAmount: 200", info);
				AssertContains("OSOutstandingAmountMatching: 100", info);
				AssertContains("AH_Calc_OSOutstandingAmount: 100", info);
				AssertContains("LocalPartialPaymentAmount: 200", info);
				AssertContains("OutstandingAmountMatching: 100", info);
				AssertContains("AH_OutstandingAmount: 100", info);
				AssertContains("AH_RX_NKTransactionCurrency: AUD", info);
				AssertContains("AH_ExchangeRate: 1", info);
				AssertContains("ValidationType: Enterprise.Accounting.Business.ARAP.ReceiptPayment.PaymentValidation", info);
				AssertContains("IsValidationSuspended: No", info);
				AssertContains("Stack Trace:    at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", info);
			}
			else
			{
				Assert("Information is collected for AP Payment only.",true);
			}
		}

		// This override is required because of issues in setting AH_AB and AH_RX_NKTransactionCurrency
		protected override void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			BankAccount.AB_RX_NKAccountCurrency = ForeignCurrency.RX_Code;
			Factory.Save();
			base.SetupHeaderForReversing(aH_OSExTaxAmount, aH_OSTaxAmount);
			Organisation.CompanyData.OB_AB_APDefaultBankAccount = BankAccount.PK;
		}

		public void TestUniversalDataContextAttribute()
		{
			object manager = null;
			AssertNoExceptionThrown(() => { manager = ReceiptPaymentBase.GetUniversalDataContextManager(); });
			AssertNotNull("ReceiptPaymentBase should have [UniversalDataContext(ReceiptPaymentBase)] attribute", manager);
		}

		public void TestIMatchingProperties()
		{
			AssertEquals(ReceiptPaymentBase.AH_RX_NKTransactionCurrencyInfo.Name, ((IMatching)ReceiptPaymentBase).PaymentCurrencyCodeInfo.Name);
		}

		#region TestZDecimalsHaveCorrectDecimalPlacesReceiptPaymentBase

		public void TestZDecimalsHaveCorrectDecimalPlacesReceiptPaymentBase()
		{
			var osList = new List<string>
				{
					nameof(ReceiptPaymentBase.OSPartialPaymentAmount)
				};

			var tester = new DecimalPlacesAttributeTester(ReceiptPaymentBase, ReceiptPaymentBase.Company);
			tester.CheckNonLocalCurrency(osList, nameof(ReceiptPaymentBase.OSCurrencyDecimals), nameof(ReceiptPaymentBase.AH_RX_NKTransactionCurrency), ReceiptPaymentBase);
		}

		#endregion

		#region TestDisplayCashFlowCategoryOverride

		public void TestDisplayCashFlowCategoryOverride()
		{
			AssertEquals(ZString.Empty, ReceiptPaymentBase.AH_TransactionCategory);
			AssertEquals(ZString.Empty, ReceiptPaymentBase.DisplayCashFlowCategoryOverride);
			ReceiptPaymentBase.DisplayCashFlowCategoryOverride = "O01";
			AssertEquals("O01", ReceiptPaymentBase.DisplayCashFlowCategoryOverride);
			AssertEquals("O01", ReceiptPaymentBase.AH_TransactionCategory);
			ReceiptPaymentBase.AH_TransactionCategory = "O02";
			AssertEquals("O02", ReceiptPaymentBase.DisplayCashFlowCategoryOverride);
		}

		public void TestDisplayCashFlowCategoryOverrides()
		{
			AssertEquals(AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value.Count - 4, ReceiptPaymentBase.DisplayCashFlowCategoryOverrides.Count);
			foreach (CashFlowActivityConfiguration x in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
			{
				if (x.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Undefined &&
						x.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Cash &&
						x.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.NonCash &&
						x.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Exchange)
				{
					Assert(ReceiptPaymentBase.DisplayCashFlowCategoryOverrides.ContainsCode(x.Code));
				}
				else
				{
					Assert(!ReceiptPaymentBase.DisplayCashFlowCategoryOverrides.ContainsCode(x.Code));
				}
			}
		}

		public void TestDefaultCashFlowCategory()
		{
			OrgDebtorGroup debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			OrgCreditorGroup creditorGroup = Factory.NewWithValidTestData<OrgCreditorGroup>();
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = true;
			testOrg.OH_IsCreditor = true;
			testOrg.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			testOrg.CompanyData.OB_OG_APCreditorGroup = creditorGroup.PK;
			Factory.Save();

			CashFlowCategoryBasedOnDebtorGroupCollection debtorGroupConfigurationCollection = new CashFlowCategoryBasedOnDebtorGroupCollection();
			CashFlowCategoryBasedOnDebtorGroup debtorGroupConfiguration = debtorGroupConfigurationCollection.AddNew();
			debtorGroupConfiguration.OrgGroupPK = debtorGroup.PK;
			debtorGroupConfiguration.CashFlowCategory = "O02";
			AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnDebtorGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, debtorGroupConfigurationCollection);

			CashFlowCategoryBasedOnCreditorGroupCollection creditorGroupConfigurationCollection = new CashFlowCategoryBasedOnCreditorGroupCollection();
			CashFlowCategoryBasedOnCreditorGroup creditorGroupConfiguration = creditorGroupConfigurationCollection.AddNew();
			creditorGroupConfiguration.OrgGroupPK = creditorGroup.PK;
			creditorGroupConfiguration.CashFlowCategory = "I01";
			AccountingMasterFilesRegistry.Instance.CashFlowCategoryBasedOnCreditorGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creditorGroupConfigurationCollection);

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			ReceiptPaymentBase.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals("O02", ReceiptPaymentBase.DefaultCashFlowCategory_ForTestOnly);

			ReceiptPaymentBase.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals("I01", ReceiptPaymentBase.DefaultCashFlowCategory_ForTestOnly);
		}

		#endregion

		#region TestFullyPay

		public void TestFullyPay()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestFullyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertFullyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			ReceiptPaymentBase.AH_OSExTaxAmount = 300m;
			ReceiptPaymentBase.AH_OSTaxAmount = 30.00m;
			ReceiptPaymentBase.AH_OutstandingAmount = 330.00m;
			ReceiptPaymentBase.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(330m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 330m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)ReceiptPaymentBase).FullyPay(expectedFullyPaidDate);

			AssertEquals("Fully Paid date on ReceiptPaymentBase", expectedFullyPaidDate, ReceiptPaymentBase.AH_FullyPaidDate);
			AssertEquals("Outstanding Amount on ReceiptPaymentBase", 0m, ReceiptPaymentBase.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
			//AssertEquals("Private field should be set by calling method", 110.00m, ReceiptPaymentBase.CurrentPaidAmount);
		}

		#endregion

		#region TestPartiallyPay

		public void TestPartiallyPay()
		{
			AssertPartiallyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestPartiallyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertPartiallyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertPartiallyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			ReceiptPaymentBase.AH_LocalExTaxAmount = 100M;
			ReceiptPaymentBase.AH_OSTotalAmount = 100M;
			ReceiptPaymentBase.AH_LocalOutstandingAmount = 70M;
			ReceiptPaymentBase.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 70m * ReceiptPaymentBase.Multiplier_ForTestOnly;
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 70m, ReceiptPaymentBase.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 60M;

			IMatching thisIMatching = ReceiptPaymentBase;
			thisIMatching.OSPartialPaymentAmount = AmountWithMultiplier;
			thisIMatching.PartiallyPay();

			AssertEquals("Local Outstanding Amount should be 10", 10M, ReceiptPaymentBase.AH_LocalOutstandingAmount);
			AssertEquals("Should not be fully paid", ZDateTime.Empty, ReceiptPaymentBase.AH_FullyPaidDate);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? 10m : 0m, ReceiptPaymentBase.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);

			thisIMatching.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = thisIMatching.CurrentMatchGroup;

			AssertEquals("There should be 1 matchlink", 1, matchLinks.Count);
			TransactionMatchLink matchLink = matchLinks[0];
			AssertEquals("Amount should be +60/-60 depending on ReceiptPaymentBase type", AmountWithMultiplier, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), matchLink.AP_OSAmount);
		}

		#endregion

		#region TestGenerateMatchlinks

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchlinks(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestGenerateMatchLinks_EnableNewOSOutstandingAmountFeature()
		{
			AssertGenerateMatchlinks(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertGenerateMatchlinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			ReceiptPaymentBase.AH_OSExTaxAmount = 100m;
			ReceiptPaymentBase.AH_OSTaxAmount = 10.00m;
			ReceiptPaymentBase.AH_OutstandingAmount = 110.00m;
			ReceiptPaymentBase.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(100m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 100m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)ReceiptPaymentBase).FullyPay(expectedFullyPaidDate);
			ReceiptPaymentBase.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = ((IMatching)ReceiptPaymentBase).CurrentMatchGroup;

			AssertEquals("Should have one matchlink record generated", 1, matchLinks.Count);

			TransactionMatchLink singleLink = matchLinks[0];

			AssertEquals("Match Amount should be same as outstanding amount after fully paying",
				110.00m, singleLink.AP_Amount);
			AssertEquals("AP_OSAmount",
				isEnableNewOSOutstandingAmountFeature ? 100.00m : 0m, singleLink.AP_OSAmount);
			AssertEquals("Match FK should be this header", ReceiptPaymentBase.PK, singleLink.AP_AH);
		}

		#endregion

		#region TestGeneratePaymentApprovalItems

		public void TestGeneratePaymentApprovalItems()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: false);

		public void TestGeneratePaymentApprovalItems_EnableNewOSOutstandingAmountFeature()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: true);

		void AssertGeneratePaymentApprovalItems(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			ReceiptPaymentBase.AH_OSExTaxAmount = 100m;
			ReceiptPaymentBase.AH_OSTaxAmount = 10.00m;
			ReceiptPaymentBase.AH_OutstandingAmount = 110.00m;
			ReceiptPaymentBase.AH_FullyPaidDate = ZDateTime.Empty;

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable = isEnableNewOSOutstandingAmountFeature;
			PaymentApprovalBase newApproval = Factory.New<APPaymentApprovalWithAuthorisation>();

			IMatching receiptPaymentBaseAsIMatching = ReceiptPaymentBase;

			receiptPaymentBaseAsIMatching.GeneratePaymentApprovalItems(newApproval);
			AssertEquals("Should have one payment approval record generated", 1, receiptPaymentBaseAsIMatching.PaymentApprovalItems.Count);

			AssertEquals("Payment Approval Item Amount",
				receiptPaymentBaseAsIMatching.LocalPartialPaymentAmount,
				receiptPaymentBaseAsIMatching.PaymentApprovalItems[0].A2_PaymentThisRun
			);
			AssertEquals("Payment Approval Item Payment OS Amount",
				isEnableNewOSOutstandingAmountFeature ? receiptPaymentBaseAsIMatching.OSPartialPaymentAmount : new ZDecimal(0m),
				receiptPaymentBaseAsIMatching.PaymentApprovalItems[0].A2_OSPaymentThisRun
			);
		}

		#endregion

		#region TestAH_OH

		public void TestAH_OH()
		{
			ReceiptPaymentBase.AH_OH = ZGuid.Invalid;
			Assert("Organisation should have errors because invalid guid", ReceiptPaymentBase.AH_OHInfo.HasErrors());
			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			Assert("Organisation should have errors because empty guid", ReceiptPaymentBase.AH_OHInfo.HasErrors());
		}

		#endregion

		#region TestDefaultBankAccount

		public abstract void TestDefaultBankAccount();

		#region DefaultBankAccountARTest

		protected void DefaultBankAccountARTest()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = true;
			testOrg.CompanyData.OB_RX_NKARDDefltCurrency = testCurrency.RX_Code;

			AccBankAccount defaultBankCurrentBranch = GetNewBankAccountWithCurrency(testCurrency, true);
			defaultBankCurrentBranch.AB_GB = GlbBranch.CurrentBranch.PK;

			AccBankAccount defaultBank = GetNewBankAccountWithCurrency(testCurrency, true);
			defaultBank.AB_GB = ZGuid.Empty;

			AccBankAccount testNonDefaultBank = GetNewBankAccountWithCurrency(testCurrency, false);

			AccBankAccount localCurrencyBank = GetNewBankAccountWithCurrency(GlbCompany.CurrentCompany.LocalCurrency, true);

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_IsDebtor = true;
			testOrg2.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			Header = GetNewBusinessObject() as ReceiptPaymentBase;
			ReceiptPaymentBase.AH_OH = testOrg.PK;
			AssertEquals("Bank account should be DefaultBankCurrentBranch", defaultBankCurrentBranch.PK, ReceiptPaymentBase.AH_AB);

			ReceiptPaymentBase.AH_OH = testOrg2.PK;
			AssertEquals("Bank account should be DefaultBank2", localCurrencyBank.PK, ReceiptPaymentBase.AH_AB);

			ReceiptPaymentBase.AH_AB = defaultBankCurrentBranch.PK;
			localCurrencyBank.Delete();
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			AccBankAccount bank_DiffCompany = GetNewBankAccountWithCompanyAndCurrency(company, GlbCompany.CurrentCompany.LocalCurrency, true);
			ReceiptPaymentBase.AH_ExchangeRate = 1m;
			Factory.Save();

			Header = GetNewBusinessObject() as ReceiptPaymentBase;
			Assert("Precondition: bank account should be empty", ReceiptPaymentBase.AH_AB.IsEmpty);
			ReceiptPaymentBase.AH_OH = testOrg2.PK;
			Assert("Bank account should be empty because the only bank account with the default currency is for another company",
				ReceiptPaymentBase.AH_AB.IsEmpty);
		}

		#endregion

		#region DefaultBankAccountAPTest

		protected void DefaultBankAccountAPTest()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			AccBankAccount bank_DiffCompany = GetNewBankAccountWithCompany(company);
			testOrg.CompanyData.OB_AB_APDefaultBankAccount = bank_DiffCompany.PK;
			Factory.Save();

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			Assert("Bank account should not default to field on CompanyData since it is from a different company", ReceiptPaymentBase.AH_AB.IsEmpty);

			AccBankAccount bank_CurrentCompany = GetNewBankAccountWithCompany(GlbCompany.CurrentCompany);
			testOrg.CompanyData.OB_AB_APDefaultBankAccount = bank_CurrentCompany.PK;
			Factory.Save();

			Header = GetNewBusinessObject() as ReceiptPaymentBase;
			ReceiptPaymentBase.AH_OH = testOrg.PK;
			AssertEquals("Bank account should default to AP Bank field on Company Data", bank_CurrentCompany.PK, ReceiptPaymentBase.AH_AB);
		}

		#endregion

		#endregion

		#region TestDefaultTransNumSettings

		public void TestDefaultTransNumSettings()
		{
			Assert("Precondition: Transaction number should be read only", ReceiptPaymentBase.AH_TransactionNumInfo.ReadOnly);
		}

		#endregion

		#region TestDefaultDescription

		public abstract void TestDefaultDescription();

		#endregion

		#region TestDefaultPaymentReceiptType

		public void TestDefaultPaymentReceiptType()
		{
			AssertEquals("Default Receipt/Payment type should be CHQ", ZArchitecture.Core.ReceiptTypes.Cheque, ReceiptPaymentBase.AH_ReceiptType);
		}

		#endregion

		#region TestReceiptType

		public abstract void TestSetReceiptType();

		public void TestDefaultReceiptType()
		{
			if (ReceiptPaymentBase is Payment)
			{
				AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
				Header = Factory.New(GetExpectedBusinessObjectType()) as Payment;
				AssertEquals(ReceiptTypes.Cash, ReceiptPaymentBase.AH_ReceiptType);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.DefaultReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
				Header = Factory.New(GetExpectedBusinessObjectType()) as Receipt;
				AssertEquals(ReceiptTypes.Cash, ReceiptPaymentBase.AH_ReceiptType);
			}
		}

		public void TestDefaultReceiptTypeReferenceNumber()
		{
			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			AccountingConfigurationRegistry.Instance.DefaultReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			if (ReceiptPaymentBase is Payment)
			{
				Header = Factory.New(GetExpectedBusinessObjectType()) as Payment;
				Header.AH_ReceiptType = ReceiptTypes.CreditCard;
				AssertEquals(ReceiptTypes.CreditCard, Header.AH_ReceiptType);
				AssertEquals("Test1", Header.AH_ChequeOrReference);

				Header.AH_ReceiptType = ReceiptTypes.Cash;
				AssertEquals(ReceiptTypes.Cash, Header.AH_ReceiptType);
				AssertEquals("CSH", Header.AH_ChequeOrReference);
			}
			else
			{
				Header = Factory.New(GetExpectedBusinessObjectType()) as Receipt;
				Header.AH_ReceiptType = ReceiptTypes.CreditCard;
				AssertEquals(ReceiptTypes.CreditCard, Header.AH_ReceiptType);
				AssertEquals("Test1", Header.AH_ChequeOrReference);

				Header.AH_ReceiptType = ReceiptTypes.Cash;
				AssertEquals(ReceiptTypes.Cash, Header.AH_ReceiptType);
				AssertEquals("CSH", Header.AH_ChequeOrReference);
			}
		}

		#endregion

		#region TestDefaultCurrency

		public void TestDefaultCurrency()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency.RX_Code;

			Factory.Save();

			Assert("Precondition: the currency should be readonly", ReceiptPaymentBase.AH_RX_NKTransactionCurrencyInfo.ReadOnly);

			ReceiptPaymentBase.AH_AB = testBankAccount.PK;
			AssertEquals("The Receipt/Payment currency should default to the Bank Account's currency", testCurrency.RX_Code, ReceiptPaymentBase.AH_RX_NKTransactionCurrency);

			ReceiptPaymentBase.AH_AB = ZGuid.Invalid;
		}

		#endregion

		#region TestTransactionCurrencyReadOnly

		public void TestTransactionCurrencyReadOnlyBasedOnIsBankCurrencyLocal()
		{
			var testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			var testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency.RX_Code;

			var localCurrencyBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			localCurrencyBankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			Assert(!ReceiptPaymentBase.IsBankCurrencyLocal);
			Assert("The currency should be readonly", ReceiptPaymentBase.AH_RX_NKTransactionCurrency_ReadOnly);

			ReceiptPaymentBase.AH_AB = testBankAccount.PK;
			AssertNotEquals(testBankAccount.AB_RX_NKAccountCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert(!ReceiptPaymentBase.IsBankCurrencyLocal);
			Assert("The currency should be readonly", ReceiptPaymentBase.AH_RX_NKTransactionCurrency_ReadOnly);

			ReceiptPaymentBase.AH_AB = localCurrencyBankAccount.PK;
			Assert(ReceiptPaymentBase.IsBankCurrencyLocal);
			Assert("The currency should not be readonly", !ReceiptPaymentBase.AH_RX_NKTransactionCurrency_ReadOnly);
		}

		#endregion

		#region TestLocalCurrencySettings

		public void TestLocalCurrencySettings()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount testLocalBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testLocalBankAccount.AB_RX_NKAccountCurrency = ReceiptPaymentBase.AH_Calc_LocalRXCode;

			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency.RX_Code;

			ReceiptPaymentBase.AH_AB = testBankAccount.PK;
			ReceiptPaymentBase.AH_ExchangeRate = 0.78M;
			AssertEquals("Precondition: Exchange Rate is not 1", 0.78M, ReceiptPaymentBase.AH_ExchangeRate);

			ReceiptPaymentBase.AH_AB = testLocalBankAccount.PK;
			Assert("Exchange rate should be readonly because currency is local", ReceiptPaymentBase.AH_ExchangeRateInfo.ReadOnly);
			AssertEquals("Exchange rate should be 1 because currency is local", 1M, ReceiptPaymentBase.AH_ExchangeRate);

			ReceiptPaymentBase.AH_AB = testBankAccount.PK;
			Assert("Exchange rate should not be readonly", !ReceiptPaymentBase.AH_ExchangeRateInfo.ReadOnly);
		}

		#endregion

		#region TestLocalPartialPaymentAmount

		public void TestLocalPartialPaymentAmount()
		{
			ReceiptPaymentBase.AH_OSExTaxAmount = 377.13M;
			ReceiptPaymentBase.AH_ExchangeRate = 0.6873M;
			ReceiptPaymentBase.AH_LocalExTaxAmount = 548.72M; // set here because exchange rate resets outstanding

			AmountWithMultiplier = 377.13M;
			((IMatching)ReceiptPaymentBase).OSPartialPaymentAmount = AmountWithMultiplier;

			AmountWithMultiplier = 548.72M;
			AssertEquals("LocalPartialPayment amount should be 548.72 i.e. same as AH_OutstandingAmount",
				AmountWithMultiplier, ((IMatching)ReceiptPaymentBase).LocalPartialPaymentAmount);

			AmountWithMultiplier = 140M;
			((IMatching)ReceiptPaymentBase).OSPartialPaymentAmount = AmountWithMultiplier;

			AmountWithMultiplier = 203.70M;
			AssertEquals("LocalPartialPayment amount should be 203.70",
				AmountWithMultiplier, ((IMatching)ReceiptPaymentBase).LocalPartialPaymentAmount);
		}

		#endregion

		#region TestAmountDefaultSettings

		public void TestAmountDefaultSettings()
		{
			Assert("Precondition: Local Amount is readonly", ReceiptPaymentBase.AH_LocalExTaxAmountInfo.ReadOnly);
			Assert("Precondition: Local Currency is readonly", ReceiptPaymentBase.AH_Calc_LocalRXCodeInfo.ReadOnly);
		}

		#endregion

		#region TestOrgHeaders

		public abstract void TestOrgHeaders();

		public void TestOrgHeaderDefaultARAPBankAccountChangeDoesNotUpdateBankAccountOnPostedTransaction()
		{
			var oldBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var newBankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			var orgHeader1 = TestObjectCreator.ActiveOrg;
			orgHeader1.CompanyData.OB_AB_ARPayToAccount = oldBankAccount.PK;
			orgHeader1.CompanyData.OB_AB_APDefaultBankAccount = oldBankAccount.PK;
			var orgHeader2 = TestObjectCreator.TestOrganisation;
			orgHeader2.CompanyData.OB_AB_ARPayToAccount = newBankAccount.PK;
			orgHeader2.CompanyData.OB_AB_APDefaultBankAccount = newBankAccount.PK;

			var transaction = GetNewBusinessObject() as ReceiptPaymentBase;
			transaction.AH_OH = orgHeader1.PK;

			orgHeader1.CompanyData.OB_AB_ARPayToAccount = newBankAccount.PK;
			orgHeader1.CompanyData.OB_AB_APDefaultBankAccount = newBankAccount.PK;
			transaction.AH_OH = orgHeader1.PK; // This is to simulate refresh binding when OrgHeader business object is updated in database.
			AssertEquals("AH_AB", oldBankAccount.PK, transaction.AH_AB);

			transaction.AH_OH = orgHeader2.PK;
			AssertEquals("AH_AB", newBankAccount.PK, transaction.AH_AB);

			orgHeader1.CompanyData.OB_AB_ARPayToAccount = oldBankAccount.PK;
			orgHeader1.CompanyData.OB_AB_APDefaultBankAccount = oldBankAccount.PK;
			transaction.AH_OH = orgHeader1.PK;
			AssertEquals("AH_AB", oldBankAccount.PK, transaction.AH_AB);

			Factory.Save();
			transaction.AH_OH = orgHeader2.PK;
			AssertEquals("AH_AB", oldBankAccount.PK, transaction.AH_AB);
		}

		#endregion

		#region TestBankAccounts

		public void TestBankAccounts()
		{
			Factory.Save();
			GlbBranch diffBranch = Factory.NewWithValidTestData<GlbBranch>();
			diffBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbCompany diffCompany = Factory.NewWithValidTestData<GlbCompany>();

			// assume that AccBankAccount was created during Setup() in TransactionHeader
			AccBankAccount emptyBranchBank = Factory.LoadTop1<AccBankAccount>(new ZQuery());
			emptyBranchBank.AB_GB = ZGuid.Empty;
			emptyBranchBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccBankAccount currentBranchBank = Factory.NewWithValidTestData<AccBankAccount>();
			currentBranchBank.AB_GB = GlbBranch.CurrentBranch.PK;
			currentBranchBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccBankAccount diffBranchBank = Factory.NewWithValidTestData<AccBankAccount>();
			diffBranchBank.AB_GB = diffBranch.PK;
			diffBranchBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccBankAccount diffCompanyBank = Factory.NewWithValidTestData<AccBankAccount>();
			diffCompanyBank.AB_GC = diffCompany.PK;

			Factory.Save();

			ReceiptPaymentBase.BankAccounts.Load();
			AssertEquals("There should be 2 bank accounts in the list", 2, ReceiptPaymentBase.BankAccounts.Count);
			Assert("List should contain CurrentBranchBank", ReceiptPaymentBase.BankAccounts.Contains(currentBranchBank));
			Assert("List should contain EmptyBranchBank", ReceiptPaymentBase.BankAccounts.Contains(emptyBranchBank));
		}

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			AccBankAccount activeBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			ReceiptPaymentBase.BankAccounts.Load();
			AssertEquals("Should contain active bank", true, ReceiptPaymentBase.BankAccounts.Contains(activeBank));
			AssertEquals("Should not contain inactive bank", false, ReceiptPaymentBase.BankAccounts.Contains(inactiveBank));
		}

		#endregion

		#region TestMatchingBaseObject

		public abstract void TestMatchingBaseObject();

		#endregion

		#region TestSetIsLoadedFromGUI

		public void TestSetIsLoadedFromGUIDefaultValue()
		{
			ReceiptPaymentBase.IsLoadedFromGUI = false;
			AssertEquals(false, ReceiptPaymentBase.IsLoadedFromGUI);
			ReceiptPaymentBase.IsLoadedFromGUI = true;
			AssertEquals(true, ReceiptPaymentBase.IsLoadedFromGUI);
		}

		public void TestMatchingBaseObjectUseIsLoadedFromGUI()
		{
			ReceiptPaymentBase.IsLoadedFromGUI = false;
			MatchingBase matchingBaseObject = ReceiptPaymentBase.MatchingBaseObject;
			AssertEquals(false, matchingBaseObject.GetIsLoadedFromGUIForTest());

			ReceiptPaymentBase.fMatchingBaseObject_ForTestOnly = null;
			ReceiptPaymentBase.IsLoadedFromGUI = true;
			matchingBaseObject = ReceiptPaymentBase.MatchingBaseObject;
			AssertEquals(true, matchingBaseObject.GetIsLoadedFromGUIForTest());
		}

		#endregion

		#region TestTransactionsCanNotBeMatched

		public void TestTransactionsCanNotBeMatched()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.OH_IsDebtor = ZBool.True;
			newOrganisation.OH_IsCreditor = ZBool.True;

			ARReceipt testReceipt = Factory.NewWithValidTestData<ARReceipt>();
			testReceipt.AH_OH = newOrganisation.PK;
			testReceipt.AH_LocalExTaxAmount = 10M;
			testReceipt.AH_OSExTaxAmount = 10M;

			ARReceipt testReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			testReceipt2.AH_OH = newOrganisation.PK;
			testReceipt2.AH_LocalExTaxAmount = 10M;
			testReceipt2.AH_OSExTaxAmount = 10M;

			Factory.Save();

			ReceiptPaymentBase.AH_OH = newOrganisation.PK;
			AssertEquals("Unmatching colleciton should contain 2 outstanding Transactions for NewOrganisation", 2, ReceiptPaymentBase.MatchingBaseObject.UnmatchedTransactions.Count);

			ReceiptPaymentBase.AddTransactionThatExcludedFromUnmatchedList(testReceipt2);
			AssertEquals("Unmatching colleciton should contain 1 outstanding Transaction for NewOrganisation", 1, ReceiptPaymentBase.MatchingBaseObject.UnmatchedTransactions.Count);
			Assert("The transaction should be TestReceipt", ReceiptPaymentBase.MatchingBaseObject.UnmatchedTransactions.Contains(testReceipt));
		}

		#endregion

		#region Validation Tests

		public void TestCheckAH_ReceiptType()
		{
			ReceiptPaymentBase.AH_ReceiptType = ZString.Empty;
			Assert("Receipt type should have errors since it can't be empty", ReceiptPaymentBase.AH_ReceiptTypeInfo.HasErrors());
		}

		#region TestCheckAH_OSExTaxAmount

		public virtual void TestCheckAH_OSExTaxAmount()
		{
			Assert("Precondition: OSAmount should not have errors", !ReceiptPaymentBase.AH_OSExTaxAmountInfo.HasErrors());

			ReceiptPaymentBase.AH_OSExTaxAmount = 0M;
			Assert("OSAmount should have errors since cannot be 0", ReceiptPaymentBase.AH_OSExTaxAmountInfo.HasErrors());

			ReceiptPaymentBase.AH_OSExTaxAmount = -90M;
			Assert("OSAmount should have errors since cannot be negative", ReceiptPaymentBase.AH_OSExTaxAmountInfo.HasErrors());
		}

		#endregion

		#endregion

		#region TestAH_OSOutstandingAmount

		public override void TestAH_OSOutstandingAmount()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 0); //there is no point of setting non zero tax amount here as payment/receipt does not have VAT taxes.
			Header.AH_LocalOutstandingAmount = 378.02m;
			AssertEquals("OS Outstanding Amount", 215.473m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_LocalOutstandingAmount = 110.540m;
			AssertEquals("OS Outstanding Amount with decimal not matching local total (i.e. invoiceamount+gstamount)",
				63.008m, Header.AH_Calc_OSOutstandingAmount);

			SetupHeaderExRatesAndAmounts(0.1234m, 1000, 200.453m, 0); //there is no point of setting non zero tax amount here as payment/receipt does not have VAT taxes.
			AssertEquals("OS Outstanding Amount with decimal and very small Exchange Rate", 200.453m, Header.AH_Calc_OSOutstandingAmount);
		}

		#endregion

		#region Exchange Rate Tests

		#region TestExchangeRateDefaultsCorrectly

		public void TestExchangeRateDefaultsCorrectly()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var currency = Factory.NewWithValidTestData<RefCurrency>();

			var exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_ExRateType = ReceiptPaymentBase.ExchangeRateType;
			exRate.RE_SellRate = 1.22M;
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exRate.RE_RX_NKExCurrency = currency.RX_Code;

			var expiredCurrency = Factory.NewWithValidTestData<RefCurrency>();
			var expiredExRate = Factory.NewWithValidTestData<RefExchangeRate>();
			expiredExRate.RE_ExRateType = ReceiptPaymentBase.ExchangeRateType;
			expiredExRate.RE_SellRate = 2M;
			expiredExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			expiredExRate.RE_StartDate = ZDateTime.Today.AddDays(-3);
			expiredExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
			expiredExRate.RE_RX_NKExCurrency = expiredCurrency.RX_Code;

			var bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			bank1.AB_RX_NKAccountCurrency = currency.RX_Code;

			var bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_RX_NKAccountCurrency = expiredCurrency.RX_Code;
			Factory.Save();

			ReceiptPaymentBase.AH_AB = bank1.PK;
			AssertEquals("Exchange rate should be pulled out automatically by RefetchExchangeRate", 1.22M, ReceiptPaymentBase.AH_ExchangeRate);

			ReceiptPaymentBase.AH_AB = bank2.PK;
			AssertEquals("Exchange rate should be 0 due to RefetchExchangeRate", 0M, ReceiptPaymentBase.AH_ExchangeRate);
		}

		#endregion

		#region ExchangeRateFallback Test

		public void TestExchangeRateFallback()
		{
			bool previousExchangeRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				AccBankAccount bank = GetNewBankAccountWithCompanyAndCurrency(GlbCompany.CurrentCompany, currency, false);
				AccBankAccount bankLocal = GetNewBankAccountWithCompanyAndCurrency(GlbCompany.CurrentCompany, GlbCompany.CurrentCompany.LocalCurrency, true);

				RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate.RE_RX_NKExCurrency = currency.RX_Code;
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_ExRateType = ReceiptPaymentBase.ExchangeRate.Type == ZArchitecture.Core.ExchangeRateType.Buy ? Core.Constants.ExchangeRateTypes.Code.BuyRate : Core.Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-3);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
				exRate.RE_SellRate = 4.55m;

				ReceiptPaymentBase.AH_ExchangeRate = 0m;
				ReceiptPaymentBase.AH_AB = bank.PK;
				AssertEquals("ExchangeRate should be 4.55", 4.55m, ReceiptPaymentBase.AH_ExchangeRate);
				AssertHasWarning(ReceiptPaymentBase.AH_ExchangeRateInfo, (ReceiptPaymentBase.ExchangeRate as ZAccExchangeRate).ExpiryDateWarning);

				RefExchangeRate exRateToday = Factory.NewWithValidTestData<RefExchangeRate>();
				exRateToday.RE_RX_NKExCurrency = currency.RX_Code;
				exRateToday.RE_GC = GlbCompany.CurrentCompany.PK;
				exRateToday.RE_ExRateType = ReceiptPaymentBase.ExchangeRate.Type == ZArchitecture.Core.ExchangeRateType.Buy ? Core.Constants.ExchangeRateTypes.Code.BuyRate : Core.Constants.ExchangeRateTypes.Code.SellRate;
				exRateToday.RE_StartDate = ZDateTime.Today.AddDays(-1);
				exRateToday.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
				exRateToday.RE_SellRate = 5.66m;
				Factory.Save();

				ReceiptPaymentBase.AH_AB = bankLocal.PK;
				ReceiptPaymentBase.AH_AB = bank.PK;
				AssertEquals("Exchange rate should be 5.66", 5.66m, ReceiptPaymentBase.AH_ExchangeRate);
				AssertNoWarning(ReceiptPaymentBase.AH_ExchangeRateInfo, (ReceiptPaymentBase.ExchangeRate as ZAccExchangeRate).ExpiryDateWarning);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, previousExchangeRateFallback);
			}
		}

		#endregion

		#endregion

		#region TestChangingBankAccountResetsChequeNumber

		public void TestChangingBankAccountResetsChequeNumber()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			ReceiptPaymentBase.AH_ChequeOrReference = "34";
			ReceiptPaymentBase.AH_AB = bank.PK;
			AssertEquals("Changing bank account should reset the Cheque/Reference Number", ZString.Empty, ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			ReceiptPaymentBase.AH_AB = ZGuid.Empty;

			ReceiptPaymentBase.AH_ChequeOrReference = "123";
			ReceiptPaymentBase.AH_AB = bank.PK;
			AssertEquals("Changing bank account should not reset the Cheque/Reference Number", "123", ReceiptPaymentBase.AH_ChequeOrReference);
		}

		#endregion

		#region TestAH_ChequeOrReference

		public void TestChangingAH_ChequeOrReference()
		{
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			ReceiptPaymentBase.AH_ChequeOrReference = "cash";
			AssertEquals("Cash type", "cash", ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			ReceiptPaymentBase.AH_ChequeOrReference = "456187789554";
			AssertEquals("CreditCard type", "456187789554", ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			ReceiptPaymentBase.AH_ChequeOrReference = "DirectCredit";
			AssertEquals("DirectCredit type", "DirectCredit", ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			ReceiptPaymentBase.AH_ChequeOrReference = "DirectDebit";
			AssertEquals("DirectDebit type", "DirectDebit", ReceiptPaymentBase.AH_ChequeOrReference);
		}

		#endregion

		#region TestPrepareReceiptPaymentForMatching

		public virtual void TestPrepareReceiptPaymentForMatching()
		{
			Assert("OSPartialPaymentAmount should be editable", !((IMatching)ReceiptPaymentBase).OSPartialPaymentAmountInfo.ReadOnly);
			ReceiptPaymentBase.PrepareReceiptPaymentForMatching_ForTestOnly();
			Assert("OSPartialPaymentAmount should be readonly", ((IMatching)ReceiptPaymentBase).OSPartialPaymentAmountInfo.ReadOnly);
		}

		#endregion

		#region Test Reversing

		[SuspendCriticalValidation]
		public virtual void TestReverseTransactionHasSameBank()
		{
			var defaultCurrency = Factory.NewWithValidTestData<RefCurrency>();

			var defaultBank = GetNewBankAccountWithCurrency(defaultCurrency, true);
			defaultBank.AB_GB = GlbBranch.CurrentBranch.PK;

			var nonDefaultBank = GetNewBankAccountWithCurrency(defaultCurrency, false);

			var chequeBook = GetNewChequeBook(nonDefaultBank, GlbBranch.CurrentBranch);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = defaultCurrency.RX_Code;
			org.CompanyData.OB_AB_APDefaultBankAccount = defaultBank.PK;
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;
			Factory.Save();

			var receiptPaymentBase = PrepareTransactionHeaderForTest() as ReceiptPaymentBase;
			receiptPaymentBase.AH_OH = org.PK;
			receiptPaymentBase.AH_AB = nonDefaultBank.PK;
			receiptPaymentBase.AH_ChequeOrReference = "3";
			receiptPaymentBase.AH_OSExTaxAmount = 100M;
			Factory.Save();

			var originalTransaction = receiptPaymentBase as IReversing;
			originalTransaction.GenerateReverseTransaction(true);

			var reverseTransaction = (ReceiptPaymentBase)originalTransaction.ReverseTransaction;
			AssertEquals("Bank accounts should be the same", receiptPaymentBase.AH_AB, reverseTransaction.AH_AB);
			AssertEquals("ExchangeRates should be the same", receiptPaymentBase.AH_ExchangeRate, reverseTransaction.AH_ExchangeRate);
			AssertEquals("Currencies should be the same", receiptPaymentBase.AH_RX_NKTransactionCurrency, reverseTransaction.AH_RX_NKTransactionCurrency);
		}

		public void TestReverseTransactionValues()
		{
			bool previousExRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
				bank.AB_RX_NKAccountCurrency = currency.RX_Code;
				RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
				exRate.RE_RX_NKExCurrency = currency.RX_Code;
				exRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exRate.RE_ExRateType = ReceiptPaymentBase.Ledger_ForTestOnly == LedgerTypes.AccountsPayable ? Constants.ExchangeRateTypes.Code.BuyRate : Constants.ExchangeRateTypes.Code.SellRate;
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-3);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
				exRate.RE_SellRate = 0.257m;

				ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				ReceiptPaymentBase.AH_AB = bank.PK;
				ReceiptPaymentBase.AH_ChequeOrReference = "99";
				ReceiptPaymentBase.AH_ExchangeRate = 0.789m;
				Factory.Save();

				ReceiptPaymentBase loadedRecPay = (ReceiptPaymentBase)new BusinessObjectFactory().Load(GetExpectedBusinessObjectType(), ReceiptPaymentBase.PK);
				IReversing beingReversed = loadedRecPay;
				beingReversed.GenerateReverseTransaction(true);
				ReceiptPaymentBase reverseTransaction = (ReceiptPaymentBase)beingReversed.ReverseTransaction;
				AssertEquals("Exchange Rate should be 0.789", 0.789m, reverseTransaction.AH_ExchangeRate);
				AssertEquals("Cheque number should be 99(Receipt) or 000099(Payment)", (ReceiptPaymentBase is Receipt ? "99" : "000099"), reverseTransaction.AH_ChequeOrReference);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, previousExRateFallback);
			}
		}

		[SuspendCriticalValidation]
		public void TestApplyWorkFlowTemplateAfterBOFieldsAreSetWhenReversing()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			if (ReceiptPaymentBase is ARReceipt)
			{
				template.P0_ProcessType = WorkflowDescriptors.ARReceiptWorkflowDescriptorCode;
			}
			else if (ReceiptPaymentBase is ARPayment)
			{
				template.P0_ProcessType = WorkflowDescriptors.ARPaymentWorkflowDescriptorCode;
			}
			else if (ReceiptPaymentBase is APReceipt)
			{
				template.P0_ProcessType = WorkflowDescriptors.APReceiptWorkflowDescriptorCode;
			}
			else if (ReceiptPaymentBase is APPayment)
			{
				template.P0_ProcessType = WorkflowDescriptors.APPaymentWorkflowDescriptorCode;
			}
			else
			{
				Fail("Unknow ReceiptPaymentBase type detected");
			}
			var task = template.WorkflowItems.AddNew();
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "milestone test 1";
			milestone.TriggerConditions.TriggerEventCode = "ADD";
			milestone.TemplateConditions.TemplateCondition2 = "UDF";
			milestone.TemplateConditions.TemplateCondition2Value = "\"<AH_Desc>\" != \"\"";
			Factory.Save();

			var defaultCurrency = Factory.NewWithValidTestData<RefCurrency>();
			var defaultBank = GetNewBankAccountWithCurrency(defaultCurrency, true);
			defaultBank.AB_GB = GlbBranch.CurrentBranch.PK;
			var nonDefaultBank = GetNewBankAccountWithCurrency(defaultCurrency, false);
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = defaultCurrency.RX_Code;
			org.CompanyData.OB_AB_APDefaultBankAccount = defaultBank.PK;
			org.OH_IsDebtor = true;
			org.OH_IsCreditor = true;
			Factory.Save();

			var receiptPaymentBase = PrepareTransactionHeaderForTest() as ReceiptPaymentBase;
			receiptPaymentBase.AH_OH = org.PK;
			receiptPaymentBase.AH_AB = nonDefaultBank.PK;
			receiptPaymentBase.AH_ChequeOrReference = "3";
			receiptPaymentBase.AH_OSExTaxAmount = 100M;
			Factory.Save();

			var workflowProvider = (IWorkflowProvider)receiptPaymentBase;
			AssertEquals("should create workflow", 2, workflowProvider.WorkflowItems.Count);
			AssertEquals("should create milestone", 1, workflowProvider.WorkflowItems.Where(x => x.IsMilestone).Count());

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(receiptPaymentBase);
			reversing.Reverse();
			var reversedTransaction = receiptPaymentBase.ReverseTransaction_ForTestOnly;
			AssertNotNull("Reversed Transaction", reversedTransaction);
			Assert(!reversedTransaction.AH_Desc.IsEmpty);
			workflowProvider = (IWorkflowProvider)reversedTransaction;
			AssertEquals("Reversed transaction should create workflow", 2, workflowProvider.WorkflowItems.Count);
			AssertEquals("Milestone should be created", 1, workflowProvider.WorkflowItems.Where(x => x.IsMilestone).Count());
			Factory.Save();
			AssertEquals("Save should not create more workflow since it's cancelled", 2, workflowProvider.WorkflowItems.Count);
			AssertEquals("Save should not create more workflow since it's cancelled", 1, workflowProvider.WorkflowItems.Where(x => x.IsMilestone).Count());
		}

		#endregion

		#region Debit Credit Tests

		public void TestDebitCredit()
		{
			ReceiptPaymentBase.AH_OSTotal = 100m;
			AssertEquals("Debit should be 0", 0m, ReceiptPaymentBase.Debit);
			AssertEquals("Credit should be 100", 100m, ReceiptPaymentBase.Credit);

			ReceiptPaymentBase.AH_OSTotal = -10m;
			AssertEquals("Debit should be 10", 10m, ReceiptPaymentBase.Debit);
			AssertEquals("Credit should be 0", 0m, ReceiptPaymentBase.Credit);
		}

		public override void TestLocalCredit()
		{
			AssertLocalCreditValueForNormalAndOpeningRecPay();
		}

		public override void TestLocalDebit()
		{
			AssertLocalDebitValueForNormalAndOpeningRecPay();
		}

		public void TestWithInvoiceAmountSignDifferentfromTotalSign()
		{
			AmountWithMultiplier = 1;
			var multiplier = AmountWithMultiplier;
			ReceiptPaymentBase.AH_InvoiceAmount = 402.9M * multiplier;
			ReceiptPaymentBase.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(ReceiptPaymentBase.AH_InvoiceAmount) == Math.Sign(ReceiptPaymentBase.AH_InvoiceAmount + ReceiptPaymentBase.AH_GSTAmount));
			var matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)ReceiptPaymentBase).CanUnmatch(matchLinkAmount));

			AmountWithMultiplier = -1;
			multiplier = AmountWithMultiplier;
			ReceiptPaymentBase.AH_InvoiceAmount = 402.9M * multiplier;
			ReceiptPaymentBase.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(ReceiptPaymentBase.AH_InvoiceAmount) == Math.Sign(ReceiptPaymentBase.AH_InvoiceAmount + ReceiptPaymentBase.AH_GSTAmount));
			matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)ReceiptPaymentBase).CanUnmatch(matchLinkAmount));
		}

		#endregion

		#region Post Date

		public override void TestDefaultPostDateReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		public override void TestAH_PostDate_ReadOnly()
		{
			Assert("AH_PostDate should not be readonly", !Header.AH_PostDateInfo.ReadOnly);
		}

		#endregion

		#region Match Status

		public void TestMatchStatusAndMatchStatusReasonCodeAfterSave()
		{
			var matchingCollection = new IMatchingCollection(Factory);
			var newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			newOrganisation.OH_IsDebtor = ZBool.True;
			newOrganisation.OH_IsCreditor = ZBool.True;

			var testReceipt = Factory.NewWithValidTestData<ARReceipt>();

			var testReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			matchingCollection.Add(testReceipt2);

			var testPayment = Factory.NewWithValidTestData<APPayment>();

			var testPayment2 = Factory.NewWithValidTestData<APPayment>();
			matchingCollection.Add(testPayment2);

			Factory.Save();

			AssertEquals("Match Status should be UAC", "UAC", testReceipt.AH_MatchStatus);
			AssertEquals("Match Reason Code should be ADV", "ADV", testReceipt.AH_MatchStatusReasonCode);

			AssertEquals("Match Status should be empty", string.Empty, testReceipt2.AH_MatchStatus);
			AssertEquals("Match Reason Code should be empty", string.Empty, testReceipt2.AH_MatchStatusReasonCode);

			AssertEquals("Match Status should be UAC", "UAC", testPayment.AH_MatchStatus);
			AssertEquals("Match Reason Code should be ADV", "ADV", testPayment.AH_MatchStatusReasonCode);

			AssertEquals("Match Status should be empty", string.Empty, testPayment2.AH_MatchStatus);
			AssertEquals("Match Reason Code should be empty", string.Empty, testPayment2.AH_MatchStatusReasonCode);
		}

		#endregion

		#region Test Workflow

		public void TestApplyWorkflowTemplateWhenSavingAndReversing()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExceptedWorkflowType;
			var task = template.WorkflowItems.Tasks.AddNew();
			var milestone = template.WorkflowItems.Milestones.AddNew();
			var tigger = template.WorkflowItems.Triggers.AddNew();
			newFactory.Save();

			var workflowProvider = (IWorkflowProvider)Header;
			AssertEquals("Per Condition", false, Header.IsInDatabase);
			AssertEquals("Per Condition", 0, workflowProvider.WorkflowItems.Count);
			Factory.Save();
			AssertEquals("Per Condition", true, Header.IsInDatabase);
			AssertEquals("Invoice should create workflow", 3, workflowProvider.WorkflowItems.Count);
			AssertEquals("Invoice should create Task", 1, workflowProvider.WorkflowItems.Tasks.Count);
			AssertEquals("Invoice should create Milestone", 1, workflowProvider.WorkflowItems.Milestones.Count);
			AssertEquals("Invoice should create Tigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(Header);
			reversing.Reverse();
			Factory.Save();

			var reversedInvoice = Factory.Load(Header.ReverseTransaction.GetType(), Header.ReverseTransaction.PK);
			workflowProvider = (IWorkflowProvider)reversedInvoice;
			AssertNotNull(reversedInvoice);
			AssertEquals("Per Condition", true, reversedInvoice.IsInDatabase);
			AssertEquals("Reversed Invoice should create workflow", 3, workflowProvider.WorkflowItems.Count);
			AssertEquals("Reversed Invoice should create Task", 1, workflowProvider.WorkflowItems.Tasks.Count);
			AssertEquals("Reversed Invoice should create Milestone", 1, workflowProvider.WorkflowItems.Milestones.Count);
			AssertEquals("Reversed Invoice should create Tigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
		}

		public void TestDeleteWorkflowItems()
		{
			var workflowProvider = (IWorkflowProvider)Header;
			var task = workflowProvider.WorkflowItems.Tasks.AddNew();
			AssertEquals("Per Condition", 1, workflowProvider.WorkflowItems.Count);
			Header.Delete();
			AssertEquals("No WorkflowItems After Delete", 0, workflowProvider.WorkflowItems.Count);
		}

		public void TestGetWorkflowInformationProvider()
		{
			var workflowInformationProvider = (Header as IWorkflowProvider).GetWorkflowInformationProvider();
			AssertNotNull(workflowInformationProvider);
			AssertEquals("Origin", "", workflowInformationProvider.Origin);
			AssertEquals("Destination", "", workflowInformationProvider.Destination);
			AssertEquals("Business Context", TrackingConstants.BusinessContext.Transaction, workflowInformationProvider.BusinessContext);
			AssertContainsExactElementsInAnyOrder("Companies", new[] { GlbCompany.CurrentCompany.PK }, workflowInformationProvider.Companies);
		}

		public void TestWorkflowItems()
		{
			var workflowProvider = (IWorkflowProvider)Header;
			AssertNotNull(workflowProvider.WorkflowItems);
			AssertEquals(true, Header.IsRegisteredEditableChildObject(workflowProvider.WorkflowItems));
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var workflowProvider = (IWorkflowProvider)Header;
			var columnValueRanker = workflowProvider.GetTemplateSelectionCriteria() as ColumnValueRanker;
			AssertNotNull(columnValueRanker);
			AssertArrayEqualsByElements(new object[] { Header.AH_OH, ZGuid.Empty }, columnValueRanker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public abstract string ExceptedWorkflowType { get; }

		public void TestWorkflowType()
		{
			var workflowProvider = (IWorkflowProvider)Header;
			AssertEquals(ExceptedWorkflowType, workflowProvider.WorkflowType);
		}

		#endregion
	}
}
