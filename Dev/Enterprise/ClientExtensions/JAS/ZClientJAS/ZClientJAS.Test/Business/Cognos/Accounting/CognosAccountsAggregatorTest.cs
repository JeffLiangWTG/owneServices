using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.JAS.Business.Cognos.Testing;
using Enterprise.Client.JAS.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	class CognosAccountsAggregatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", new ZDateTime(2006, 5, 5), Aggregator.ExportStartDateTime);
			AssertEquals("Should be assigned in the constructor", Notifications, Aggregator.Notifications);
		}

		#region TestAggregateToExportTempTable
		public void TestAggregateToExportTempTable()
		{
			using (new CognosTempTableCreator())
			{
				SetupAccountDescriptorsForTest();
				Aggregator.AggregateToExportTempTable();
				AssertCognosExportTempTable();
				AssertNotificationsAndMethodsExecutionOrder();
			}
		}

		#region Assertion
		void AssertCognosExportTempTable()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("SELECT * FROM #CognosExport");
			AssertEquals("Should contain all valid accounts", ValidAccounts.Count, collection.Count);
			foreach (AccGLAccountDescriptor account in ValidAccounts)
			{
				string failureMessage = string.Format("Could not find Account (\"{0}\" - {1}) in temporary CognosExport table", account.AJ_AccountDescription, account.PK);
				Assert(failureMessage, ContainAccount(collection, account));
			}
		}

		void AssertNotificationsAndMethodsExecutionOrder()
		{
			AssertEquals("There should be 18 InfoNotifications notified", 18, Notifications.Events.Length);
			AssertInfoNotification(0, "Aggregating AR / AP Journal Transactions", 0);
			AssertInfoNotification(1, "Aggregating Payment, Receipt, Cashbook Exchange Difference and Cashbook Transfer Transactions", 5);
			AssertInfoNotification(2, "Aggregating Direct Receipt, Direct Payment Transactions", 10);
			AssertInfoNotification(3, "Aggregating Bank Direct Receipt, Bank Direct Payment Transactions", 15);
			AssertInfoNotification(4, "Aggregating Invoices, Credit Notes and Adjustment Notes", 20);
			AssertInfoNotification(5, "Aggregating Job Costing Journal Transactions", 25);
			AssertInfoNotification(6, "Aggregating GL Standard, Auto and Reverse Journal Transactions", 30);
			AssertInfoNotification(7, "Aggregating WIP and Accruals", 35);
			AssertInfoNotification(8, "Aggregating AR / AP Control Accounts", 40);
			AssertInfoNotification(9, "Aggregating Exchange Difference, Discount and Overpayment Control Accounts", 45);
			AssertInfoNotification(10, "Aggregating Tax Amounts", 50);
			AssertInfoNotification(11, "Aggregating WIP and Accruals Control Accounts", 55);
			AssertInfoNotification(12, "Applying signage conversion for BSH and P&L Accounts (if applicable)", 60);
			AssertInfoNotification(13, "Calculating Alternative Accounts", 62);
			AssertInfoNotification(14, "Calculating Total Accounts", 67);
			AssertInfoNotification(15, "Calculating Consolidation Accounts", 72);
			AssertInfoNotification(16, "Calculating Sub-Classification Accounts", 77);
			AssertInfoNotification(17, "Preparing Cognos Account Records to be Exported", 82);
			AssertEquals("CurrentProgress should be 87%", 87, Notifications.CurrentProgress);
		}

		bool ContainAccount(DynamicBusinessObjectCollection collection, AccGLAccountDescriptor account)
		{
			foreach (DynamicBusinessObject dynamicBizO in collection)
			{
				if (new ZGuid(((IBusinessObjectInternals)dynamicBizO).Row["T6_AJ"]) == account.PK)
				{
					return true;
				}
			}

			return false;
		}

		void AssertInfoNotification(int eventIndex, string message, int expectedCurrentProgressPercentage)
		{
			INotification expectedNotification = Notifications.Events[eventIndex];
			AssertEquals("Notification should be an InfoNotification", typeof(InfoNotification), expectedNotification.GetType());
			AssertEquals("AdditionalInfo not as expected", message, ((INotificationSubscriberNotification)expectedNotification).AdditionalInfo);
			AssertEquals("CurrentProgressPercentage not as expected", expectedCurrentProgressPercentage, Notifications.GetProgressPercentageAtEventIndex(eventIndex));
		}

		#endregion
		#region Setup
		void SetupAccountDescriptorsForTest()
		{
			// BSH, PnL
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts();
			SetupAccountForClientInvertCognosRawAggregateSignageIfApplicable();
			// ALT, TTL, CFW, CLN
			SetupAccountForClientUpdateCognosRawAggregateRecordsWithALTAccounts();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts();
			SetupAccountForClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts();
			// Grouping
			SetupAccountForClientInsertCognosAccountsIntoCognosExportTempTable();
			SetupAccountsWhichWillNotBeIncludedInTheAggregation();
			Factory.Save();
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader()
		{
			CreateNewAccountDescriptor("GLAggregationFromHeader", "DEM", new ZDateTime(2006, 5, 6));
			CreateNewAccountDescriptor("GLAggregationFromHeader", "EDI", new ZDateTime(2006, 5, 6));
			CreateNewAccountDescriptor("GLAggregationFromHeader", "DEM", new ZDateTime(2006, 5, 4));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromHeader", "EDI", new ZDateTime(2006, 5, 5)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank()
		{
			CreateNewAccountDescriptor("GLAggregationFromHeaderBank", "DEM", new ZDateTime(2006, 5, 6));
			CreateNewAccountDescriptor("GLAggregationFromHeaderBank", "EDI", new ZDateTime(2006, 5, 6));
			CreateNewAccountDescriptor("GLAggregationFromHeaderBank", "DEM", new ZDateTime(2006, 5, 4));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromHeaderBank", "EDI", new ZDateTime(2006, 5, 4)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine()
		{
			CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentLine", "DEM", new ZDateTime(2006, 5, 6));
			CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentLine", "EDI", new ZDateTime(2006, 5, 6));
			CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentLine", "DEM", new ZDateTime(2006, 5, 4));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentLine", "EDI", new ZDateTime(2004, 10, 12)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank()
		{
			CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentBank", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentBank", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentBank", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromDirectReceiptPaymentBank", "EDI", new ZDateTime(2006, 1, 1)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj()
		{
			CreateNewAccountDescriptor("GLAggregationFromInvCrdAdj", "DEM", new ZDateTime(2006, 5, 2));
			CreateNewAccountDescriptor("GLAggregationFromInvCrdAdj", "EDI", new ZDateTime(2007, 1, 10));
			CreateNewAccountDescriptor("GLAggregationFromInvCrdAdj", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromInvCrdAdj", "EDI", new ZDateTime(2006, 1, 2)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX()
		{
			CreateNewAccountDescriptor("GLAggregationFromCFX", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromCFX", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromCFX", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromCFX", "EDI", new ZDateTime(2006, 1, 3)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals()
		{
			CreateNewAccountDescriptor("GLAggregationFromGLJournals", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromGLJournals", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromGLJournals", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromGLJournals", "EDI", new ZDateTime(2006, 1, 4)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR()
		{
			CreateNewAccountDescriptor("GLAggregationFromWIPACR", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromWIPACR", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromWIPACR", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromWIPACR", "EDI", new ZDateTime(2006, 1, 5)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts()
		{
			CreateNewAccountDescriptor("GLAggregationFromARAPControlAccounts", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromARAPControlAccounts", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromARAPControlAccounts", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromARAPControlAccounts", "EDI", new ZDateTime(2006, 1, 6)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts()
		{
			CreateNewAccountDescriptor("GLAggregationFromExchangeDiscountOverpaymentControlAccounts", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromExchangeDiscountOverpaymentControlAccounts", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromExchangeDiscountOverpaymentControlAccounts", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromExchangeDiscountOverpaymentControlAccounts", "EDI", new ZDateTime(2006, 1, 7)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts()
		{
			CreateNewAccountDescriptor("GLAggregationFromGSTControlAccounts", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromGSTControlAccounts", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromGSTControlAccounts", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromGSTControlAccounts", "EDI", new ZDateTime(2006, 1, 8)));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts()
		{
			CreateNewAccountDescriptor("GLAggregationFromWIPACRControlAccounts", "DEM", new ZDateTime(2006, 5, 1));
			CreateNewAccountDescriptor("GLAggregationFromWIPACRControlAccounts", "EDI", new ZDateTime(2006, 5, 10));
			CreateNewAccountDescriptor("GLAggregationFromWIPACRControlAccounts", "DEM", new ZDateTime(2006, 5, 9));
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromWIPACRControlAccounts", "EDI", new ZDateTime(2006, 1, 9)));
		}

		void SetupAccountForClientInvertCognosRawAggregateSignageIfApplicable()
		{
			ValidAccounts.Add(CreateNewAccountDescriptor("InvertCognosRawAggregateSignageIfApplicable"));
		}

		void SetupAccountForClientUpdateCognosRawAggregateRecordsWithALTAccounts()
		{
			ValidAccounts.Add(CreateNewAccountDescriptor(CreateNewAccountDescriptor("UpdateCognosRawAggregateRecordsWithALTAccounts"), Core.Constants.AccountType.Alternate));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts()
		{
			//SetupAccountingReportOrder();
			CreateNewAccountDescriptor(CreateNewAccountDescriptor("GLAggregationFromCFWAndTTLAccounts", "10000"), Core.Constants.AccountType.Total);
			CreateNewAccountDescriptor(CreateNewAccountDescriptor("GLAggregationFromCFWAndTTLAccounts", "51000"), Core.Constants.AccountType.Total);
			ValidAccounts.Add(CreateNewAccountDescriptor(CreateNewAccountDescriptor("GLAggregationFromCFWAndTTLAccounts", "20000"), Core.Constants.AccountType.Total));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts()
		{
			ValidAccounts.Add(CreateNewAccountDescriptor("GLAggregationFromCLNAccounts"));
		}

		void SetupAccountForClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts()
		{
			ValidAccounts.Add(CreateNewAccountDescriptor("FromSubClassificationAccounts"));
		}

		void SetupAccountForClientInsertCognosAccountsIntoCognosExportTempTable()
		{
			ValidAccounts.Add(CreateNewAccountDescriptor("InsertCognosAccountsIntoCognosExportTempTable"));
		}

		void SetupAccountsWhichWillNotBeIncludedInTheAggregation()
		{
			CreateNewAccountDescriptor("NotIncluded1");
			CreateNewAccountDescriptor("NotIncluded2");
			CreateNewAccountDescriptor("NotIncluded3");
			CreateNewAccountDescriptor("NotIncluded4").AJ_ReportType = "abc";
		}

		AccGLAccountDescriptor CreateNewAccountDescriptor(ZString description)
		{
			AccGLAccountDescriptor result = CreateNewAccountDescriptor(Factory.NewWithValidTestData<AccGLAccountDescriptor>(), Core.Constants.AccountType.BalanceSheetAccount);
			result.AJ_AccountDescription = description;
			return result;
		}

		AccGLAccountDescriptor CreateNewAccountDescriptor(AccGLAccountDescriptor result, ZString reportCategory)
		{
			result.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			result.AJ_ReportCategory = reportCategory;
			return result;
		}

		AccGLAccountDescriptor CreateNewAccountDescriptor(ZString description, ZString dummyCompanyCode, ZDateTime dummyDate)
		{
			AccGLAccountDescriptor result = CreateNewAccountDescriptor(description);
			DummyBusinessObject dummy = CreateDummy(result);
			dummy.Z0_FK_Code = dummyCompanyCode;
			dummy.Z0_SmallDateTime = dummyDate;
			return result;
		}

		AccGLAccountDescriptor CreateNewAccountDescriptor(ZString description, ZString dummyNVarChar)
		{
			AccGLAccountDescriptor result = CreateNewAccountDescriptor(description);
			DummyBusinessObject dummy = CreateDummy(result);
			dummy.Z0_NVarChar = dummyNVarChar;
			return result;
		}

		DummyBusinessObject CreateDummy(AccGLAccountDescriptor account)
		{
			DummyBusinessObject result = Factory.New<DummyBusinessObject>();
			result.Z0_Guid = account.PK;
			return result;
		}

		List<AccGLAccountDescriptor> ValidAccounts
		{
			get
			{
				if (fValidAccounts == null)
				{
					fValidAccounts = new List<AccGLAccountDescriptor>();
				}

				return fValidAccounts;
			}
		}

		List<AccGLAccountDescriptor> fValidAccounts;
		#endregion
		#endregion
		public void TestBSHStartAccount()
		{
			AssertEquals("Should use the separator account when Account starts with PnL", "50000", Aggregator.InternalBSHStartAccountTest);
			ReportOrderCollection reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
			ReportOrder reportOrder = reportOrders[0];
			reportOrder.AccountsOrderBeginsWith = nameof(AccountOrderType.BalanceSheet);
			AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrders);
			Aggregator = new CognosAccountsAggregator(new ZDateTime(2006, 5, 5), Notifications);
			AssertEquals("Should use the first account when Account starts with BSH", "11000", Aggregator.InternalBSHStartAccountTest);
		}

		public void TestPnLStartAccount()
		{
			AssertEquals("Should use the first account when Account starts with PnL", "11000", Aggregator.InternalPnLStartAccountTest);
			ReportOrderCollection reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
			ReportOrder reportOrder = reportOrders[0];
			reportOrder.AccountsOrderBeginsWith = nameof(AccountOrderType.BalanceSheet);
			AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrders);
			Aggregator = new CognosAccountsAggregator(new ZDateTime(2006, 5, 5), Notifications);
			AssertEquals("Should use the separator account when Account starts with BSH", "50000", Aggregator.InternalPnLStartAccountTest);
		}

		public void TestGetStoredProcDbCommand()
		{
			DbCommand command = Aggregator.GetStoredProcDbCommand("MEH");
			AssertEquals("Timeout should be set to 600 seconds", 600, command.CommandTimeout);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			OriginalDbSchemaUpgradeInfo = ClientOverride.Instance.DbSchemaExtensionObjects;
			ClientOverride.Instance.SetDbSchemaUpgradeInfoForTest(new DummyJASClientDbSchemaUpgradeInfo());
			TestCaseHelper.RunClientDbCreateScripts(true);
			Notifications = new CognosNotificationBufferForTest();
			Aggregator = new CognosAccountsAggregator(new ZDateTime(2006, 5, 5), Notifications);
			SetupAccountingReportOrder();
		}

		protected override void TearDown()
		{
			ClientOverride.Instance.SetDbSchemaUpgradeInfoForTest(OriginalDbSchemaUpgradeInfo);
			base.TearDown();
		}

		void SetupAccountingReportOrder()
		{
			AccGLAccountDescriptor firstLocalAccount = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			firstLocalAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			firstLocalAccount.AJ_RN_NKCountryOfCompliance = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			firstLocalAccount.AJ_LocalAccountNumber = "11000";
			firstLocalAccount.AJ_ReportCategory = Core.Constants.AccountType.Header;
			firstLocalAccount.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			AccGLAccountDescriptor reportOrderSeparatorAccount = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			reportOrderSeparatorAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			reportOrderSeparatorAccount.AJ_RN_NKCountryOfCompliance = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportOrderSeparatorAccount.AJ_LocalAccountNumber = "50000";
			reportOrderSeparatorAccount.AJ_ReportCategory = Core.Constants.AccountType.Header;
			reportOrderSeparatorAccount.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			Factory.Save();
			ReportOrderCollection reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
			reportOrders.RemoveAndDeleteAll();
			ReportOrder reportOrder = reportOrders.AddNew();
			reportOrder.Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			reportOrder.CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportOrder.AccountsOrderBeginsWith = nameof(AccountOrderType.ProfitAndLoss);
			reportOrder.GLAccountSecondReportStartsFrom = reportOrderSeparatorAccount.PK;
			AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrders);
		}

		IExtensionObjects OriginalDbSchemaUpgradeInfo;
		CognosAccountsAggregator Aggregator;
		CognosNotificationBufferForTest Notifications;
		#endregion
	}
}
