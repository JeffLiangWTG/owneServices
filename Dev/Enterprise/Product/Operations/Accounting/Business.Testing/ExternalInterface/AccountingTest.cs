using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.Business.ExternalInterface.Testing
{
	public class AccountingTest : TestCaseWithFactory
	{
		public void TestSetTaxIDsAttractingStampDutyForTransformation()
		{
			var creator = new TestObjectCreator(Factory);
			var company = creator.CreateNewCompany("ITC", Constants.CountryCodes.Italy);
			Factory.Save();
			AccTaxRate existingESCLUSEB;
			existingESCLUSEB = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "ESCLUSEB"));
			if (existingESCLUSEB != null)
			{
				existingESCLUSEB.Delete();
			}

			Factory.Save();
			var eSCLUSEB = Factory.NewWithValidTestData<AccTaxRate>(); // Cannot use TestObjectCreator because it adds "ZZ" to AT_Code
			eSCLUSEB.AT_Code = "ESCLUSEB";
			eSCLUSEB.AT_Type = "EXL";
			eSCLUSEB.AT_RN_NKCountry = Constants.CountryCodes.Italy;
			eSCLUSEB.SetRateNumerator_ForTestOnly(0);
			Factory.Save();
			var accounting = new Accounting();
			accounting.SetTaxIDsAttractingStampDutyForTransformation();
			AssertEquals(true, AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty).Contains(eSCLUSEB.PK.ToString()));
		}

		public void TestIsGLAccountUsedInSystemLevelRegistry()
		{
			Accounting testAccounting = new Accounting();
			Guid glHeaderPK1 = Factory.NewWithValidTestData<AccGLHeader>().PK.ToGuid();
			Guid glHeaderPK2 = Factory.NewWithValidTestData<AccGLHeader>().PK.ToGuid();
			IRegistryItem[] registryItemsToCheck = {
			AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount,
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
			AccountingConfigurationRegistry.Instance.APControlAccount,
			AccountingConfigurationRegistry.Instance.ARControlAccount,
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount,
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount,
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount,
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount,
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
			AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
			AccountingConfigurationRegistry.Instance.APJournalAccount,
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount,
			AccountingConfigurationRegistry.Instance.ARJournalAccount,
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount,
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount,
			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount,
			AccountingConfigurationRegistry.Instance.ARDiscountAccount,
			AccountingConfigurationRegistry.Instance.APDiscountAccount,
			AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
			AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
			AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount };
			foreach (var registryItem in registryItemsToCheck)
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderPK1);
			}

			Assert(testAccounting.IsGLAccountUsedInSystemLevelRegistry(glHeaderPK1));
			Assert(!testAccounting.IsGLAccountUsedInSystemLevelRegistry(glHeaderPK2));
		}

		public void TestGoodsReceivedStatusCodesList()
		{
			Accounting testAccounting = new Accounting();
			var x = testAccounting.GoodsReceivedStatusCodesList;
			AssertEquals("NOT, FRC, PRC, OSP, OIV, UIV", ((ReadOnlyCodeDescriptionPairList)x).CodesAsString);
			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("AAA", "test aaa"));
			newlist1.Add(new CodeDescriptionPair("BBB", "test bbb"));
			AccountingConfigurationRegistry.Instance.GoodsReceivedStatusCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			AssertEquals("AAA, BBB", AccountingConfigurationRegistry.Instance.GoodsReceivedStatusCodesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).CodesAsString);
			x = testAccounting.GoodsReceivedStatusCodesList;
			AssertEquals("AAA, BBB", ((ReadOnlyCodeDescriptionPairList)x).CodesAsString);
			CodeDescriptionPairList newlist2 = new CodeDescriptionPairList();
			newlist2.Add(new CodeDescriptionPair("CCC", "test ccc"));
			newlist2.Add(new CodeDescriptionPair("DDD", "test ddd"));
			AccountingConfigurationRegistry.Instance.GoodsReceivedStatusCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			AssertEquals("CCC, DDD", AccountingConfigurationRegistry.Instance.GoodsReceivedStatusCodesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).CodesAsString);
			x = testAccounting.GoodsReceivedStatusCodesList;
			AssertEquals("CCC, DDD", ((ReadOnlyCodeDescriptionPairList)x).CodesAsString);
		}

		public void TestGLPresentationJournalCategoriesList()
		{
			var company = Guid.NewGuid();

			Accounting testAccounting = new Accounting();
			testAccounting.SetupGLPresentationJournalCategoriesListRegistry(Guid.Empty, "GP1", "Group 1");

			Assert(testAccounting.GLPresentationJournalCategoriesList(Guid.Empty).ContainsCode("GP1"));
			Assert(testAccounting.GLPresentationJournalCategoriesList(company).ContainsCode("GP1"));

			var companyCategorySetList = new GLPresentationJournalCategoryCollection();
			var category2 = companyCategorySetList.AddNew();
			category2.Code = "IOS";
			category2.Description = (NoResString)"Category 2";
			var category3 = companyCategorySetList.AddNew();
			category3.Code = "DOC";
			category3.ParentCode = category2.Code;
			category3.Description = (NoResString)"Category 2";

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(company, Guid.Empty, Guid.Empty, companyCategorySetList);

			var systemLevelCategoryList = testAccounting.GLPresentationJournalCategoriesList(Guid.Empty);
			var companyCategoryList = testAccounting.GLPresentationJournalCategoriesList(company);

			Assert(systemLevelCategoryList.ContainsCode("GP1"));
			Assert(!systemLevelCategoryList.ContainsCode("IOS"));
			Assert(!systemLevelCategoryList.ContainsCode("DOC"));

			Assert(companyCategoryList.ContainsCode("IOS"));
			Assert(companyCategoryList.ContainsCode("DOC"));
			Assert(!companyCategoryList.ContainsCode("GP1"));
		}

		public void TestGetCategorisWithChildren()
		{
			var companyCategorySetList = new GLPresentationJournalCategoryCollection();
			var category1 = companyCategorySetList.AddNew();
			category1.Code = "IOS";
			category1.Description = (NoResString)"Category 1";
			var category2 = companyCategorySetList.AddNew();
			category2.Code = "EOC";
			category2.ParentCode = category1.Code;
			category2.Description = (NoResString)"Category 2";
			var category3 = companyCategorySetList.AddNew();
			category3.Code = "DOC";
			category3.ParentCode = category1.Code;
			category3.Description = (NoResString)"Category 3";
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyCategorySetList);

			var testAccounting = new Accounting();
			testAccounting.SetupGLPresentationJournalCategoriesListRegistry(Guid.Empty, "GP1", "Group 1");
			AssertEquals("Empty", ZString.Empty, testAccounting.GetCategorisWithChildren(ZString.Empty));
			AssertEquals("Has Child Categories", "IOS,DOC,EOC", testAccounting.GetCategorisWithChildren("IOS"));
			AssertEquals("Has no Child Categories", "EOC", testAccounting.GetCategorisWithChildren("EOC"));
		}

		public void TestGLPresentationJournalCategoriesGroupList()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "GP1";
			category1.Description = (NoResString)"Group 1";
			var category2 = list.AddNew();
			category2.Code = "IOS";
			category2.ParentCode = category1.Code;
			category2.Description = (NoResString)"Category 2";
			var category3 = list.AddNew();
			category3.Code = "DOC";
			category3.ParentCode = category1.Code;
			category3.Description = (NoResString)"Category 2";

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			Accounting testAccounting = new Accounting();
			var groupCategoriesList = testAccounting.GLPresentationJournalCategoriesGroupList;
			AssertEquals(4, groupCategoriesList.Count);
			Assert(groupCategoriesList.ContainsCode("GP1"));
			Assert(groupCategoriesList.ContainsCode("IOS"));
			Assert(groupCategoriesList.ContainsCode("DOC"));
			Assert(groupCategoriesList.ContainsCode("GP1,IOS,DOC"));
		}

		public void TestCashFlowCategoryCodeDescriptionList()
		{
			Accounting testAccounting = new Accounting();
			var x = testAccounting.CashFlowCategoryCodeDescriptionList;
			foreach (CashFlowActivityConfiguration item in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
			{
				if (item.Code.EqualsIgnoringCase("CSH") || item.Code.EqualsIgnoringCase("NON"))
				{
					Assert(!x.ContainsCode(item.Code));
				}
				else
				{
					Assert(x.ContainsCode(item.Code));
				}
			}

			Assert(x.ContainsCode("ZZZ"));
		}

		public void TestReversalReasonCodesList()
		{
			Accounting testAccounting = new Accounting();
			var x = testAccounting.ReversalReasonCodesList;
			AssertEquals(((ReadOnlyCodeDescriptionPairList)x).CodesAsString, "IDE, WOR, IAM, TXT");
			CodeDescriptionPairList newlist1 = new CodeDescriptionPairList();
			newlist1.Add(new CodeDescriptionPair("AAA", "test aaa"));
			newlist1.Add(new CodeDescriptionPair("BBB", "test bbb"));
			AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newlist1);
			x = testAccounting.ReversalReasonCodesList;
			AssertEquals(((ReadOnlyCodeDescriptionPairList)x).CodesAsString, "AAA, BBB");
			CodeDescriptionPairList newlist2 = new CodeDescriptionPairList();
			newlist2.Add(new CodeDescriptionPair("CCC", "test ccc"));
			newlist2.Add(new CodeDescriptionPair("DDD", "test ddd"));
			AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist2);
			x = testAccounting.ReversalReasonCodesList;
			AssertEquals(((ReadOnlyCodeDescriptionPairList)x).CodesAsString, "CCC, DDD");
			AssertEquals(AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).CodesAsString, "AAA, BBB");
			AssertEquals(AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).CodesAsString, "CCC, DDD");
		}

		public void TestGetCaptionsOfRegistryItemsUsingGLHeader()
		{
			Accounting testAccounting = new Accounting();
			Guid headerPK = Guid.NewGuid();
			AssertNotNull(testAccounting.GetCaptionsOfRegistryItemsUsingGLHeader(headerPK));
			AssertEquals("Empty Array if not used", 0, testAccounting.GetCaptionsOfRegistryItemsUsingGLHeader(headerPK).Length);
			AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, headerPK);
			AssertEquals("Assigned to RegistryItem", headerPK, AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.Value);
			AssertEquals("Array of Caption for assigned RegistryItem", 1, testAccounting.GetCaptionsOfRegistryItemsUsingGLHeader(headerPK).Length);
			AssertEquals("Array of Caption for assigned RegistryItem", AccountingConfigurationRegistry.Instance.BankTransactionGLAccount.Caption, testAccounting.GetCaptionsOfRegistryItemsUsingGLHeader(headerPK)[0]);
		}

		public void TestGetCaptionsOfRegistryItemsUsingGLHeader_Coverage()
		{
			Accounting testAccounting = new Accounting();
			Guid headerPK = Guid.NewGuid();
			IRegistryItem[] registryItemsToCheck = {
			AccountingConfigurationRegistry.Instance.BankTransactionGLAccount,
			AccountingConfigurationRegistry.Instance.CASSGLAccount,
			AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount,
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
			AccountingConfigurationRegistry.Instance.APControlAccount,
			AccountingConfigurationRegistry.Instance.APJournalAccount,
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
			AccountingConfigurationRegistry.Instance.ARControlAccount,
			AccountingConfigurationRegistry.Instance.ARJournalAccount,
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.CFXAccount,
			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
			AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
			AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount,
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount,
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
			AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount,
			AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
			AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount };
			foreach (IRegistryItem item in registryItemsToCheck)
			{
				if (item.Value is Guid)
				{
					item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, headerPK);
					AssertEquals("Value should be assigned to RegistryItem " + item.Caption, headerPK, (Guid)item.Value);
				}
			}

			string[] result = testAccounting.GetCaptionsOfRegistryItemsUsingGLHeader(headerPK);
			AssertEquals("Array of Captions for assigned RegistryItems", registryItemsToCheck.Length, result.Length);
			for (int i = 0; i < result.Length; i++)
			{
				AssertEquals("Caption for assigned RegistryItem", registryItemsToCheck[i].Caption, result[i]);
			}
		}

		public void TestIsNotAllowedForSeparateNumbering()
		{
			IRegistryItem[] registryItemsToCheck =
			{
				AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
				AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.ARDiscountAccount,
				AccountingConfigurationRegistry.Instance.APDiscountAccount,
				AccountingConfigurationRegistry.Instance.ARJournalAccount,
				AccountingConfigurationRegistry.Instance.APJournalAccount,
				AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount,
				AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount,
				AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount,
				AccountingConfigurationRegistry.Instance.PeriodApportionmentARClearingAccount,
				AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount,
				AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
			};

			CheckControlAccounts(registryItemsToCheck, (accounting, glHeaderPK) => accounting.IsNotAllowedForSeparateNumbering(glHeaderPK));
		}

		public void TestIsNotAllowedForDissectionAttributes()
		{
			IRegistryItem[] registryItemsToCheck =
			{
				AccountingConfigurationRegistry.Instance.CFXAccount,
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
				AccountingConfigurationRegistry.Instance.APControlAccountAdjustment,
				AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount,
				AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
				AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
				AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.PLAppropriationAccount,
				AccountingConfigurationRegistry.Instance.AdvancedTurnoverTaxReturnAccount,
				AccountingConfigurationRegistry.Instance.SpecialVATPrepaymentAccount,
				AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
				AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
			};

			CheckControlAccounts(registryItemsToCheck, (accounting, glHeaderPK) => accounting.IsNotAllowedForDissectionAttributes(glHeaderPK));
		}

		void CheckControlAccounts(IRegistryItem[] controlRegistryItems, Func<Accounting, ZGuid, bool> checkAccount)
		{
			var testAccounting = new Accounting();
			var accountList = new Dictionary<IRegistryItem, ZGuid>();

			foreach (var registry in controlRegistryItems)
			{
				var account = ZGuid.NewZGuid();
				accountList.Add(registry, account);
				registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, account.ToGuid());
			}

			CombineAssertions("Disallowed", () =>
			{
				foreach (var account in accountList)
				{
					Assert(account.Key.Caption, checkAccount(testAccounting, account.Value));
				}
			});

			Assert("Allowed", !checkAccount(testAccounting, ZGuid.NewZGuid()));
		}

		public void TestCollectConstructorCallStackDetails()
		{
			Accounting testAccounting = new Accounting();
			AssertEquals("Registry CollectConstructorCallStackDetailsToReportInCriticalValidationErrors", AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value, testAccounting.CollectConstructorCallStackDetails);
			AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, !testAccounting.CollectConstructorCallStackDetails);
			AssertEquals("Registry CollectConstructorCallStackDetailsToReportInCriticalValidationErrors reversed", AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value, testAccounting.CollectConstructorCallStackDetails);
		}

		public void TestRegistry()
		{
			Accounting testAccounting = new Accounting();
			AssertEquals(AccountingConfigurationRegistry.Instance.CreditorCreditLimitNotifyGroup, testAccounting.Registry.CreditorCreditLimitNotifyGroup);
			AssertEquals(AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule, testAccounting.Registry.JobBranchDefaultOrderRule);
			AssertEquals(AccountingConfigurationRegistry.Instance.ARCreditControlledDocumentsApprovalNotifyGroup, testAccounting.Registry.ARCreditControlledDocumentsApprovalNotifyGroup);
			AssertEquals(AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit, testAccounting.Registry.UseWebServiceForCreditLimit);
			AssertEquals(AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance, testAccounting.Registry.UseWebServiceForOutstandingBalance);
			AssertEquals(AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue, testAccounting.Registry.UseWebServiceForUnpostedRevenue);
			AssertEquals(AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted, testAccounting.Registry.CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly);
			AssertEquals(AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob, testAccounting.Registry.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly);
			AssertEquals(AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled, testAccounting.Registry.JobInvoicingCFXEnabled);
			AssertEquals(AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation, testAccounting.Registry.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation);
			AssertEquals(AccountingConfigurationRegistry.Instance.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation, testAccounting.Registry.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation);
		}

		public void TestCreditControlledDocumentsCheckConfiguration()
		{
			Accounting testAccounting = new Accounting();
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();
			CreditControlledDocumentsCheckConfiguration upToConfiguration = collection.AddNew();
			upToConfiguration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			upToConfiguration.NumberOfDaysOverdue = 10;
			upToConfiguration.Amount = 1000;
			upToConfiguration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToConfiguration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			CreditControlledDocumentsCheckConfiguration configuration = collection.AddNew();
			configuration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			configuration.NumberOfDaysOverdue = 10;
			configuration.Amount = 1000;
			configuration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			configuration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.CreditLimitCheckOverdueInvoicesStatusCheck.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var creditControlledDocumentsCheckConfiguration = testAccounting.GetCreditControlledDocumentsCheckConfiguration();
			AssertEquals(2, creditControlledDocumentsCheckConfiguration.Length);
			AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, creditControlledDocumentsCheckConfiguration[0].InvoiceType);
			AssertEquals(10, creditControlledDocumentsCheckConfiguration[0].NumberOfDaysOverdue);
			AssertEquals(1000m, creditControlledDocumentsCheckConfiguration[0].Amount);
		}

		public void TestGlobalCreditControlledDocumentsCheckConfiguration()
		{
			Accounting testAccounting = new Accounting();
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();
			CreditControlledDocumentsCheckConfiguration upToConfiguration = collection.AddNew();
			upToConfiguration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			upToConfiguration.NumberOfDaysOverdue = 10;
			upToConfiguration.Amount = 1000;
			upToConfiguration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToConfiguration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			CreditControlledDocumentsCheckConfiguration configuration = collection.AddNew();
			configuration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			configuration.NumberOfDaysOverdue = 10;
			configuration.Amount = 1000;
			configuration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			configuration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.GlobalCreditLimitCheckOverdueInvoicesStatusCheck.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var globalcreditControlledDocumentsCheckConfiguration = testAccounting.GetGlobalCreditControlledDocumentsCheckConfiguration();
			AssertEquals(2, globalcreditControlledDocumentsCheckConfiguration.Length);
			AssertEquals(OrgARTermsLookups.InvoiceTypes.All.Code, globalcreditControlledDocumentsCheckConfiguration[0].InvoiceType);
			AssertEquals(10, globalcreditControlledDocumentsCheckConfiguration[0].NumberOfDaysOverdue);
			AssertEquals(1000m, globalcreditControlledDocumentsCheckConfiguration[0].Amount);
		}

		public void TestIncludeUnpostedRevenueInCreditLimitCalculation()
		{
			Accounting testAccounting = new Accounting();
			AssertEquals("Registry IncludeUnpostedRevenueInCreditLimitCalculation", AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value, testAccounting.IncludeUnpostedRevenueInCreditLimitCalculation.Value);
			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
			AssertEquals("Registry IncludeUnpostedRevenueInCreditLimitCalculation", AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value, testAccounting.IncludeUnpostedRevenueInCreditLimitCalculation.Value);
			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
			AssertEquals("Registry IncludeUnpostedRevenueInCreditLimitCalculation", AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value, testAccounting.IncludeUnpostedRevenueInCreditLimitCalculation.Value);
			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted);
			AssertEquals("Registry IncludeUnpostedRevenueInCreditLimitCalculation", AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value, testAccounting.IncludeUnpostedRevenueInCreditLimitCalculation.Value);
		}

		public void TestPlAppropriationAccountRegistryItem()
		{
			var testAccounting = new Accounting();
			AssertEquals("Registry PlAppropriationAccount", AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value, testAccounting.PlAppropriationAccountRegistryItem.Value);
			var glHeaderPK = Guid.NewGuid();
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderPK);
			AssertEquals("Registry PlAppropriationAccount", AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value, testAccounting.PlAppropriationAccountRegistryItem.Value);
		}

		public void TestSetTaxIDConfigurationRelatedRegistry()
		{
			Accounting testAccounting = new Accounting();
			Guid company = Guid.NewGuid();
			Guid taxId1 = Guid.NewGuid();
			Guid taxId2 = Guid.NewGuid();
			Guid taxId3 = Guid.NewGuid();
			Guid taxId4 = Guid.NewGuid();
			Guid taxId5 = Guid.NewGuid();
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.MainGSTTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetMainGSTTaxIDConfiguration(company, taxId1);
			AssertEquals(taxId1, AccountingConfigurationRegistry.Instance.MainGSTTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.MainGSTReverseTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetMainGSTReverseTaxIDConfiguration(company, taxId2);
			AssertEquals(taxId2, AccountingConfigurationRegistry.Instance.MainGSTReverseTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.MainFreeGSTTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetMainFreeGSTTaxIDConfiguration(company, taxId3);
			AssertEquals(taxId3, AccountingConfigurationRegistry.Instance.MainFreeGSTTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.MainFreeGSTReverseTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetMainFreeGSTReverseTaxIDConfiguration(company, taxId4);
			AssertEquals(taxId4, AccountingConfigurationRegistry.Instance.MainFreeGSTReverseTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetMainNotReportableTaxIDConfiguration(company, taxId5);
			AssertEquals(taxId5, AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
		}

		public void TestWIPMustHaveDebtorCode()
		{
			Accounting testAccounting = new Accounting();
			Guid company = Guid.NewGuid();
			AssertEquals(AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value, testAccounting.WIPMustHaveDebtorCode(Environment.Env.CurrentCompany.PK));
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value, testAccounting.WIPMustHaveDebtorCode(Environment.Env.CurrentCompany.PK));
		}

		public void TestAccrualMustHaveCreditorCode()
		{
			Accounting testAccounting = new Accounting();
			Guid company = Guid.NewGuid();
			AssertEquals(AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value, testAccounting.AccrualMustHaveCreditorCode(Environment.Env.CurrentCompany.PK));
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.Value, testAccounting.AccrualMustHaveCreditorCode(Environment.Env.CurrentCompany.PK));
		}

		public void TestSetWIPMustHaveDebtorCode()
		{
			Accounting testAccounting = new Accounting();
			Guid company = Guid.NewGuid();
			AssertEquals(false, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetWIPMustHaveDebtorCode(company, true);
			AssertEquals(true, AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
		}

		public void TestSetAccrualMustHaveCreditorCode()
		{
			Accounting testAccounting = new Accounting();
			Guid company = Guid.NewGuid();
			AssertEquals(false, AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
			testAccounting.SetAccrualMustHaveCreditorCode(company, true);
			AssertEquals(true, AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.GetValueWithoutFallback(company, Guid.Empty, Guid.Empty));
		}

		public void TestShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob()
		{
			var testObjectCreater = new TestObjectCreator(Factory);
			var shipment = testObjectCreater.CreateShipment("S00001", "AUBNE", "JPTYO");

			var accounting = new Accounting();
			AssertEquals(((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob).Location, accounting.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation);

			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment));
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment));

			AssertEquals(false, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			AssertEquals(0, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Count);
			AssertEquals(false, accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			AssertEquals(0, AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Count);
			AssertEquals(false, accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			var config = collection.AddNew();
			config.JobType = "SHP";
			config.StartDate = ZDate.Today.AddDays(-1);
			config.EndDate = ZDate.Today.AddDays(1);
			AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals(false, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			Assert(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Cast<ElectronicProcessingChargeConfiguration>().Any(x => x.JobType == shipment.InvoicingSupporter.ConsumerType.Code && ZDateTime.Today >= x.StartDate && (x.EndDate.IsEmpty || ZDateTime.Today < x.EndDate.AddDays(1))));
			AssertEquals(false, accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment));

			AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(true, AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value);
			Assert(AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Cast<ElectronicProcessingChargeConfiguration>().Any(x => x.JobType == shipment.InvoicingSupporter.ConsumerType.Code && ZDateTime.Today >= x.StartDate && (x.EndDate.IsEmpty || ZDateTime.Today < x.EndDate.AddDays(1))));
			AssertEquals(true, accounting.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(shipment));
		}

		public void TestUseWebServiceForCreditLimit()
		{
			var testAccounting = new Accounting();
			AssertEquals(AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Value, testAccounting.UseWebServiceForCreditLimit);
			AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Value, testAccounting.UseWebServiceForCreditLimit);
		}

		public void TestDefaultPaymentType()
		{
			var testAccounting = new Accounting();
			string defaultValue = AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value;
			AssertEquals(defaultValue, testAccounting.DefaultPaymentType);

			string systemLevelValue = "EFT";
			AssertNotEquals("Value to assign at System level should be different from current value.", systemLevelValue, AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value);
			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLevelValue);
			AssertEquals(systemLevelValue, testAccounting.DefaultPaymentType);

			string companyLevelValue = "CSH";
			AssertNotEquals("Value to assign at Company level should be different from default value.", companyLevelValue, defaultValue);
			AssertNotEquals("Value to assign at Company level should be different from current value.", companyLevelValue, AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value);
			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, companyLevelValue);
			AssertEquals(companyLevelValue, testAccounting.DefaultPaymentType);
		}

		public void TestIsMiscInvoiceInPeriodicInvoiceEnabled()
		{
			var testAccounting = new Accounting();
			AssertEquals(AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.Value, testAccounting.IsMiscInvoiceInPeriodicInvoiceEnabled(Guid.Empty));
			AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.Value, testAccounting.IsMiscInvoiceInPeriodicInvoiceEnabled(Guid.Empty));
		}

		public void TestTaxRecognitionDefaultingRules_IsOrganisationOverridePermitted()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_IsGSTCashBasis = true;
			Factory.Save();
			var testAccounting = new Accounting();
			Assert(!testAccounting.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(LedgerTypes.AccountsPayable));
			Assert(!testAccounting.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(LedgerTypes.AccountsReceivable));
			var registryValue = new TaxRecognitionDefaultingRules();
			registryValue.APOrganizationOverride = TaxRecognitionDefaultingRules.OrganisationOverrideTypesYesCode;
			registryValue.AROrganizationOverride = TaxRecognitionDefaultingRules.OrganisationOverrideTypesYesCode;
			AccountingConfigurationRegistry.Instance.TaxRecognitionDefaultingRules.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue);
			Assert(testAccounting.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(LedgerTypes.AccountsPayable));
			Assert(testAccounting.TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(LedgerTypes.AccountsReceivable));
		}

		public void TestJobInvoicingCFXEnabled()
		{
			var testAccounting = new Accounting();
			AssertEquals(AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.Value, testAccounting.JobInvoicingCFXEnabled(Env.CurrentCompany.PK));
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value as set", true, AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.Value);
			AssertEquals("JobInvoicingCFXEnabled", true, testAccounting.JobInvoicingCFXEnabled(Env.CurrentCompany.PK));
		}

		public void TestIsIncludedInElectronicProcessingChargeConfiguration()
		{
			var accounting = ObjectFactory.Get<IAccounting>();
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			var config = collection.AddNew();
			config.JobType = "SHP";
			config.StartDate = ZDate.Today.AddDays(-1);
			config.EndDate = ZDate.Today.AddDays(1);

			using (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				Assert(!accounting.IsIncludedInElectronicProcessingChargeConfiguration(ZDate.Today.AddDays(-2), "SHP"));
				Assert(accounting.IsIncludedInElectronicProcessingChargeConfiguration(ZDate.Today.AddDays(-1), "SHP"));
				Assert(!accounting.IsIncludedInElectronicProcessingChargeConfiguration(ZDate.Today.AddDays(2), "SHP"));
				Assert(!accounting.IsIncludedInElectronicProcessingChargeConfiguration(ZDate.Today.AddDays(1), "BRK"));
			}
		}

		public void TestJobInvoicingCFXEnabledUseChargeJR_GCNotCurrenyCompanyPK()
		{
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_Name = "Your US Company";
			uSCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var chicagoBranch = Factory.NewWithValidTestData<GlbBranch>();
			chicagoBranch.GB_GC = uSCompany.PK;
			Factory.Save();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(uSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertRegistryValue(chicagoBranch, charge.JR_GC.ToGuid(), true);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(uSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertRegistryValue(chicagoBranch, charge.JR_GC.ToGuid(), false);
		}

		void AssertRegistryValue(GlbBranch nonCurrentCompanyBranch, Guid chargeCompanyPK, bool expectedRegistryValue)
		{
			var testAccounting = new Accounting();
			var message = string.Format("Registry should return {0} for charge company", expectedRegistryValue.ToString());
			AssertEquals("Eagle Datamation International", GlbCompany.CurrentCompany.GC_Name);
			AssertEquals(message, expectedRegistryValue, testAccounting.JobInvoicingCFXEnabled(chargeCompanyPK));
			message = string.Format("Registry should return {0} for charge company, even when charge is used within US company context", expectedRegistryValue.ToString());
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, nonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Your US Company", GlbCompany.CurrentCompany.GC_Name);
				AssertEquals(message, expectedRegistryValue, testAccounting.JobInvoicingCFXEnabled(chargeCompanyPK));
			}
		}

		public void TestHasSubAccounts()
		{
			var testAccounting = new Accounting();
			var testObjectCreator = new TestObjectCreator(Factory);
			var line = testObjectCreator.CreateInvoiceWithLine(typeof(ARAP.Invoicing.APInvoice), "T001", testObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m).Lines[0];
			AssertEquals("Pre-condition", 0, line.SubAccounts.Count);
			AssertEquals(true, line is ISupportMultiSubAccounts);
			AssertEquals("ISupportMultipleSubAccount bizO without sub accounts", false, testAccounting.HasSubAccounts(line));

			var subAccount = testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, testObjectCreator.AALSHI.PK);
			line.SubAccounts.Add(subAccount);
			AssertEquals("ISupportMultipleSubAccount bizO with sub accounts", true, testAccounting.HasSubAccounts(line));

			AssertEquals("Non-ISupportMultipleSubAccount bizO", false, testAccounting.HasSubAccounts(DummyBusinessObject.New(Factory)));
			AssertEquals("Null value", false, testAccounting.HasSubAccounts(null));
		}

		public void TestGetSubAccountsInfo()
		{
			var testAccounting = new Accounting();
			var testObjectCreator = new TestObjectCreator(Factory);
			var line = testObjectCreator.CreateInvoiceWithLine(typeof(ARAP.Invoicing.APInvoice), "T001", testObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m).Lines[0];
			AssertEquals("Pre-condition", 0, line.SubAccounts.Count);
			AssertEquals(true, line is ISupportMultiSubAccounts);
			AssertEquals("ISupportMultipleSubAccount bizO without sub accounts", string.Empty, testAccounting.GetSubAccountsInfo(line));

			var subAccount = testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, testObjectCreator.AALSHI.PK);
			line.SubAccounts.Add(subAccount);
			AssertEquals("ISupportMultipleSubAccount bizO with one sub account", $"ORG:{testObjectCreator.AALSHI.PK}", testAccounting.GetSubAccountsInfo(line));

			subAccount = testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.StaffAndResources, testObjectCreator.Staff.PK);
			line.SubAccounts.Add(subAccount);
			AssertEquals("ISupportMultipleSubAccount bizO with one sub account", $"ORG:{testObjectCreator.AALSHI.PK} STR:{testObjectCreator.Staff.PK}", testAccounting.GetSubAccountsInfo(line));

			AssertEquals("Non-ISupportMultipleSubAccount bizO", string.Empty, testAccounting.GetSubAccountsInfo(DummyBusinessObject.New(Factory)));
			AssertEquals("Null value", string.Empty, testAccounting.GetSubAccountsInfo(null));
		}

		public void TestControlAccounts()
		{
			var apControlAccount = ZGuid.NewZGuid();
			var arControlAccount = ZGuid.NewZGuid();
			var gstInputAccount = ZGuid.NewZGuid();
			var gstOutputAccount = ZGuid.NewZGuid();
			var pengdingGstInputAccount = ZGuid.NewZGuid();
			var pengdingGstOutputAccount = ZGuid.NewZGuid();

			var testAccounting = new Accounting();
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.ToGuid());
			AssertEquals(testAccounting.APControlAccount, apControlAccount);
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.ToGuid());
			AssertEquals(testAccounting.ARControlAccount, arControlAccount);

			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gstInputAccount.ToGuid());
			AssertEquals(testAccounting.GSTInputControlAccount, gstInputAccount);
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, gstOutputAccount.ToGuid());
			AssertEquals(testAccounting.GSTOutputControlAccount, gstOutputAccount);
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pengdingGstInputAccount.ToGuid());
			AssertEquals(testAccounting.PendingGSTInputControlAccount, pengdingGstInputAccount);
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pengdingGstOutputAccount.ToGuid());
			AssertEquals(testAccounting.PendingGSTOutputControlAccount, pengdingGstOutputAccount);
		}

		public void TestNoteGLAccountsStatisticalUnitsofMeasurement()
		{
			var testAccounting = new Accounting();

			CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
			lookUpList.AddPair("KWH", "Kilowatt Hours");
			lookUpList.AddPair("KG", "Kilograms");
			lookUpList.AddPair("TON", "Ton");
			lookUpList.AddPair("TEU", "Twenty-foot Equivalent Unit");
			lookUpList.AddPair("HCT", "Headcount");
			AssertContainsExactElementsInAnyOrder(lookUpList.Cast<ICodeDescription>().Select(x => x.Code), testAccounting.Registry.NoteGLAccountsStatisticalUnitsofMeasurement(GlbCompany.CurrentCompany.PK.ToGuid()).Cast<ICodeDescription>().Select(x => x.Code));

			lookUpList.AddPair("XXX", "XXXX");
			AccountingConfigurationRegistry.Instance.NoteGLAccountsStatisticalUnitsofMeasurement.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, lookUpList);
			AssertContainsExactElementsInAnyOrder(lookUpList.Cast<ICodeDescription>().Select(x => x.Code), testAccounting.Registry.NoteGLAccountsStatisticalUnitsofMeasurement(GlbCompany.CurrentCompany.PK.ToGuid()).Cast<ICodeDescription>().Select(x => x.Code));
		}

		public void TestTaxMessageGroupsManagement()
		{
			var testAccounting = new Accounting();
			var result = new CodeDescriptionBoolRelatedItemCollection();
			result.Add("N1", (NoResString)"Description N1", true, "N1.0");
			result.Add("N2", (NoResString)"Description N2", false, "N2.0");
			using (AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, result))
			{
				AssertContainsExactElementsInAnyOrder(new string[] { "N1" }, testAccounting.Registry.TaxMessageGroupsManagement(GlbCompany.CurrentCompany.PK.ToGuid()).Cast<ICodeDescription>().Select(x => x.Code));
			}
		}

		public void TestGetGenerateJournalEntriesStartDate()
		{
			var creator = new TestObjectCreator(Factory);
			var testAccounting = new Accounting();
			var curCompany = GlbCompany.CurrentCompany;
			var testDateTime = new DateTime(2023, 1, 1);
			creator.CreateTestPeriodsForEntireYear(curCompany, 2023);
			creator.SetControlAccountsForGenerateJournalEntriesStartDate();

			AssertEquals("Default value is DateTime.MinValue", DateTime.MinValue, testAccounting.GetGenerateJournalEntriesStartDate(curCompany.PK.ToGuid()));

			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(curCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testDateTime);
			AssertEquals(testDateTime, testAccounting.GetGenerateJournalEntriesStartDate(curCompany.PK.ToGuid()));
		}

		public void TestIsNotAllowedForSeparateNumberingRegistry()
		{
			IRegistryItem[] registryItemsToCheck =
			{
				AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
				AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.ARDiscountAccount,
				AccountingConfigurationRegistry.Instance.APDiscountAccount,
				AccountingConfigurationRegistry.Instance.ARJournalAccount,
				AccountingConfigurationRegistry.Instance.APJournalAccount,
				AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount,
				AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount,
				AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount,
				AccountingConfigurationRegistry.Instance.PeriodApportionmentARClearingAccount,
				AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount,
				AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
			};
			var accounting = new Accounting();

			foreach (var registry in registryItemsToCheck)
			{
				Assert(accounting.IsNotAllowedForSeparateNumberingRegistry(registry));
			}
		}

		public void TestIsNotAllowedForDissectionAttributesRegistry()
		{
			IRegistryItem[] registryItemsToCheck =
			{
				AccountingConfigurationRegistry.Instance.CFXAccount,
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
				AccountingConfigurationRegistry.Instance.APControlAccountAdjustment,
				AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount,
				AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
				AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
				AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.PLAppropriationAccount,
				AccountingConfigurationRegistry.Instance.AdvancedTurnoverTaxReturnAccount,
				AccountingConfigurationRegistry.Instance.SpecialVATPrepaymentAccount,
				AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
				AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
			};
			var accounting = new Accounting();

			foreach (var registry in registryItemsToCheck)
			{
				Assert(accounting.IsNotAllowedForDissectionAttributesRegistry(registry));
			}
		}

		[TestDate(2024, 09, 27)]
		public void TestGetGSTVATConversionExchangeRate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var companyAU = GlbCompany.CurrentCompany;
			companyAU.GC_RX_NKLocalCurrency = testObjectCreator.USD.RX_Code;

			AssertEquals("PreCond: companyAU use currency USD", testObjectCreator.USD.RX_Code, companyAU.GC_RX_NKLocalCurrency);
			AssertEquals("PreCond: Today's date is 27/09/2024", new ZDateTime(2024, 09, 27), ZDateTime.Today);
			
			ExchangeRateReader.GetReaderInstance().ClearCache();
			testObjectCreator.CreateExchangeRate(testObjectCreator.AUD, "SEL", 4m, new ZDateTime(2024, 09, 27), new ZDateTime(2024, 09, 27));
			testObjectCreator.CreateExchangeRate(testObjectCreator.AUD, "SEL", 5m, new ZDateTime(2024, 09, 28), new ZDateTime(2024, 09, 28));
			testObjectCreator.CreateExchangeRate(testObjectCreator.AUD, "SEL", 6m, new ZDateTime(2024, 09, 29), new ZDateTime(2024, 09, 29));
			testObjectCreator.CreateExchangeRate(testObjectCreator.AUD, "SEL", 7m, new ZDateTime(2024, 09, 30), new ZDateTime(2024, 09, 30));

			var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 1m, 100m, 0m, 100m, 0m);
			arInvoice1.AH_ExchangeRate = 1;
			arInvoice1.AH_PostDate = new ZDateTime(2024, 09, 28);
			arInvoice1.AH_InvoiceDate = new ZDateTime(2024, 10, 01);

			var line = arInvoice1.Lines[0];
			line.AL_AT = testObjectCreator.FREEVAT.PK;
			line.AL_TaxDate = new ZDate(2024, 09, 26);
			Factory.Save();

			var transactionHeaderPk = arInvoice1.PK;
			var companyPk = companyAU.PK;

			SetRegistryExRateOptionRegistry(ExRateOption.TodayExchangeRate.Code);
			AssertGetOverrideExchangeRateResult(0.2m, 0.2m, transactionHeaderPk, companyPk);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertGetOverrideExchangeRateResult(0m, 0.142857m, transactionHeaderPk, companyPk);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertGetOverrideExchangeRateResult(0.166667m, 0.166667m, transactionHeaderPk, companyPk);

			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertEquals(new ZDate(2024, 09, 26), arInvoice1.InvoiceTaxDate);
			AssertGetOverrideExchangeRateResult(0.25m, 0.25m, transactionHeaderPk, companyPk);

			var arInvoice2 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", testObjectCreator.AUD, 10m, null);
			Factory.Save();
			transactionHeaderPk = arInvoice2.PK;
			AssertEquals(GlbCompany.CurrentCompany.Country.LocalCurrency.Code, arInvoice2.AH_RX_NKTransactionCurrency);
			AssertGetOverrideExchangeRateResult(1m, 1m, transactionHeaderPk, companyPk);
			void SetRegistryExRateOptionRegistry(string exRateOptionValue)
			{
				var collectionAR = new InvoicePostingExRateOptionCollection()
				{
					new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, exRateOptionValue, offSet: 1),

					new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, exRateOptionValue, offSet: 0)
				};
				AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);
			}
		}

		[TestDate(2024, 11, 06)]
		public void TestGetGSTVATConversionExchangeRateOnCashBook()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var companyAU = GlbCompany.CurrentCompany;
			companyAU.GC_RX_NKLocalCurrency = testObjectCreator.USD.RX_Code;

			AssertEquals("PreCond: companyAU use currency USD", testObjectCreator.USD.RX_Code, companyAU.GC_RX_NKLocalCurrency);
			AssertEquals("PreCond: Today's date is 06/11/2024", new ZDateTime(2024, 11, 06), ZDateTime.Today);

			ExchangeRateReader.GetReaderInstance().ClearCache();
			testObjectCreator.CreateExchangeRate(testObjectCreator.AUD, "SEL", 4m, new ZDateTime(2024, 11, 06), new ZDateTime(2024, 11, 06));

			var payment = testObjectCreator.CreateDirectPayment(new ZDateTime(2024, 11, 06), 51592M, 0M, 51592M, 0M, testObjectCreator.USDBankAccount.PK, 10M);
			Factory.Save();

			var transactionHeaderPk = payment.PK;
			var companyPk = companyAU.PK;

			AssertEquals( new ZDate(2024, 11, 06), payment.AH_SystemCreateTimeUtc);
			AssertGetOverrideExchangeRateResult(2.5m, 2.5m, transactionHeaderPk, companyPk);

			var receipt = testObjectCreator.CreateDirectReceipt(new ZDateTime(2024, 11, 06), 51592M, 0M, 51592M, 0m, testObjectCreator.USDBankAccount.PK, 1M);
			Factory.Save();
			transactionHeaderPk = receipt.PK;
			AssertEquals(new ZDate(2024, 11, 06), receipt.AH_SystemCreateTimeUtc);
			AssertGetOverrideExchangeRateResult(0.25m, 0.25m, transactionHeaderPk, companyPk);
		}

		[TestDate(2024, 11, 06)]
		public void TestGetGSTVATConversionExchangeRateOnCashBookWithoutExchangeRate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var companyAU = GlbCompany.CurrentCompany;
			companyAU.GC_RX_NKLocalCurrency = testObjectCreator.USD.RX_Code;

			AssertEquals("PreCond: companyAU use currency USD", testObjectCreator.USD.RX_Code, companyAU.GC_RX_NKLocalCurrency);
			AssertEquals("PreCond: Today's date is 06/11/2024", new ZDateTime(2024, 11, 06), ZDateTime.Today);

			var payment = testObjectCreator.CreateDirectPayment(new ZDateTime(2024, 11, 06), 51592M, 0M, 51592M, 0M, testObjectCreator.USDBankAccount.PK, 10M);
			Factory.Save();

			AssertEquals(new ZDate(2024, 11, 06), payment.AH_SystemCreateTimeUtc);
			AssertGetOverrideExchangeRateResult(0m, 0m, payment.PK, companyAU.PK);
		}

		void AssertGetOverrideExchangeRateResult(decimal expextedConversionExchangeRateWithoutFallBack, decimal expextedConversionExchangeRateWithFallBack, ZGuid transactionHeaderPk, ZGuid companyPk)
		{
			var accounting = new Accounting();
			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(expextedConversionExchangeRateWithoutFallBack, accounting.GetGSTVATConversionExchangeRate(transactionHeaderPk, companyPk));
			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(expextedConversionExchangeRateWithFallBack, accounting.GetGSTVATConversionExchangeRate(transactionHeaderPk, companyPk));
		}
	}
}
