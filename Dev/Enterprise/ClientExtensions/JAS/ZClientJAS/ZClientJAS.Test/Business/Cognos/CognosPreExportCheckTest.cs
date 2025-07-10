using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosPreExportCheckTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Notifications, PreExportCheck.Notifications);
		}

		public void TestEnsureCanExport()
		{
			MapGLAccount("101");
			MapGLAccount("103");
			JASDataRegistry.Instance.CognosSalesGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "ALL").PK;
			Factory.Save();
			Assert("Still a WIP. Always returns true at the moment", PreExportCheck.EnsureCanExport());
			AssertEquals("There should be 2 warning notifications", 4, Notifications.Events.Length);
			AssertNotification(Notifications.Events[0], typeof(InfoNotification), "Start Pre-Export Check");
			string expectedGLAccountsWarning = @"
The following GL Accounts are not mapped.
- 102 Account 102
- 104 Account 104
- 105 Account 105
".TrimStart();
			AssertNotification(Notifications.Events[1], typeof(WarningNotification), expectedGLAccountsWarning);
			string expectedGroupRegistryWarning = @"
The following user groups have not been mapped in the Cognos registry. Please configure through Registry > JAS Client Extension > Cognos
- Administration Group
- Shipment Control Group
".TrimStart();
			AssertNotification(Notifications.Events[2], typeof(WarningNotification), expectedGroupRegistryWarning);
			AssertNotification(Notifications.Events[3], typeof(InfoNotification), "Finish Pre-Export Check\r\n");
		}

		void AssertNotification(INotification notification, Type expectedNotificationType, ZString expectedMessage)
		{
			AssertEquals("Expected Notification Type", expectedNotificationType, notification.GetType());
			AssertEquals("AdditionalInfo not as expected", expectedMessage, ((INotificationSubscriberNotification)notification).AdditionalInfo);
		}

		#region Implementation
		CognosPreExportCheck PreExportCheck
		{
			get
			{
				if (fPreExportCheck == null)
				{
					fPreExportCheck = new CognosPreExportCheck(Notifications);
				}

				return fPreExportCheck;
			}
		}

		CognosNotificationBufferForTest Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new CognosNotificationBufferForTest();
				}

				return fNotifications;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupGLAccounts();
		}

		void MapGLAccount(ZString accountNumber)
		{
			AccGLAccountDescriptor localAccount = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			localAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			localAccount.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			localAccount.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			localAccount.AJ_LocalAccountNumber = "1" + accountNumber;
			localAccount.ParentGLHeaderPK = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, accountNumber).PK;
		}

		void SetupGLAccounts()
		{
			MakeBaseDataAccountGLHeadersInactive();
			CreateNewGLAccount("101", "Account 101", Core.Constants.AccountType.BalanceSheetAccount);
			CreateNewGLAccount("102", "Account 102", Core.Constants.AccountType.ProfitAndLossAccount);
			CreateNewGLAccount("103", "Account 103", Core.Constants.AccountType.BalanceSheetAccount);
			CreateNewGLAccount("104", "Account 104", Core.Constants.AccountType.ProfitAndLossAccount);
			CreateNewGLAccount("105", "Account 105", Core.Constants.AccountType.BalanceSheetAccount);
			CreateNewGLAccount("106", "Account 106", Core.Constants.AccountType.Header);
			CreateNewGLAccount("107", "Account 106", Core.Constants.AccountType.Consolidation);
			CreateNewGLAccount("108", "Account 106", Core.Constants.AccountType.Total);
			CreateNewGLAccount("109", "Account 106", Core.Constants.AccountType.Alternate);
			Factory.Save();
		}

		void MakeBaseDataAccountGLHeadersInactive()
		{
			ZQuery filter = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.ProfitAndLossAccount);
			filter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, Core.Constants.AccountType.BalanceSheetAccount);
			foreach (AccGLHeader account in Factory.Load<AccGLHeader>(filter))
			{
				account.AG_IsActive = false;
			}
		}

		AccGLHeader CreateNewGLAccount(ZString accountNumber, ZString accountDescription, ZString accountType)
		{
			AccGLHeader result = Factory.NewWithValidTestData<AccGLHeader>();
			result.AG_AccountNum = accountNumber;
			result.AG_AccountType = accountType;
			result.AG_Description = accountDescription;
			return result;
		}

		CognosPreExportCheck fPreExportCheck;
		CognosNotificationBufferForTest fNotifications;
		#endregion
	}
}
