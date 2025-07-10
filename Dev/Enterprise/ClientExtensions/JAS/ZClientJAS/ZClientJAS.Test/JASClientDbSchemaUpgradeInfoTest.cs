using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Testing
{
	[TestedType(typeof(JASClientDbSchemaUpgradeInfo))]
	class JASClientDbSchemaUpgradeInfoTest : ConstraintForClientSpecificSchema
	{
		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo()
		{
			Db.Connection.BeginTransaction();
			try
			{
				var scripts = new List<DatabaseObjectCreateScript>();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
				scripts.Reverse();
				foreach (var script in scripts)
				{
					try
					{
						ExecuteNonQuery(script.DropScript);
					}
					catch
					{
					}
				}

				scripts.Reverse();
				foreach (var script in scripts)
				{
					ExecuteNonQuery(script.CreateScript);
				}

				AssertEquals("The create script should create the function", true, ExistsDbObject("ClientConvertLocalToForeignAmount"));
				AssertEquals("The create script should create the view", true, ExistsDbObject("ClientAccTransactionHeaderWithJobInfo"));
				AssertEquals("The create script should create the table", true, ExistsDbObject("ClientCognosAccGLAccountDescriptorExtraInfo"));
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					ExecuteNonQuery(script.DropScript);
				}

				AssertEquals("The drop script should drop the view", false, ExistsDbObject("ClientAccTransactionHeaderWithJobInfo"));
				AssertEquals("The drop script should drop the function", false, ExistsDbObject("ClientConvertLocalToForeignAmount"));
				AssertEquals("The drop script should drop the table", false, ExistsDbObject("ClientCognosAccGLAccountDescriptorExtraInfo"));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public override void TestAllIndexesMustSetAllowPageLocksToOffForClientSpecificSchema()
		{
			Assert("Will remove this method next PR", true);
		}

		#region TestClientConvertLocalToForeignAmount
		public void TestClientConvertLocalToForeignAmount()
		{
			String uSD_Code = "USD";
			String jPY_Code = "JPY";
			String jOD_Code = "JOD";
			ExchangeRate exchangeRate = Env.CurrentCompany.ExchangeRate;
			AssertEquals(exchangeRate.LocalToForeign(20m, 0.0233m, "USD"), ExecuteClientConvertLocalToForeignAmount(20m, 0.0233m, Env.CurrentCompany.PK, uSD_Code));
			AssertEquals(exchangeRate.LocalToForeign(100m, 0.2938m, "JPY"), ExecuteClientConvertLocalToForeignAmount(100m, 0.2938m, Env.CurrentCompany.PK, jPY_Code));
			AssertEquals(exchangeRate.LocalToForeign(300m, 1.23m, "JOD"), ExecuteClientConvertLocalToForeignAmount(300m, 1.23m, Env.CurrentCompany.PK, jOD_Code));
			exchangeRate = new ExchangeRate(!Env.CurrentCompany.IsReciprocal, 0, Env.CurrentCompany.PK);
			GlbCompany company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			company.GC_IsReciprocal = !Env.CurrentCompany.IsReciprocal;
			Factory.Save();
			AssertEquals(exchangeRate.LocalToForeign(99.3m, 0.988m, "USD"), ExecuteClientConvertLocalToForeignAmount(99.3m, 0.988m, company.PK.ToGuid(), uSD_Code));
			AssertEquals(exchangeRate.LocalToForeign(2983m, 1.233m, "JPY"), ExecuteClientConvertLocalToForeignAmount(2983m, 1.233m, company.PK.ToGuid(), jPY_Code));
			AssertEquals(exchangeRate.LocalToForeign(498584m, 0.5m, "JOD"), ExecuteClientConvertLocalToForeignAmount(498584m, 0.5m, company.PK.ToGuid(), jOD_Code));
		}

		decimal ExecuteClientConvertLocalToForeignAmount(decimal localAmount, decimal exchangeRate, Guid companyPK, string foreignCurrencyNK)
		{
			string sQLText = string.Format("SELECT dbo.ClientConvertLocalToForeignAmount({0}, {1}, '{2}', '{3}')", localAmount, exchangeRate, companyPK, foreignCurrencyNK);
			return (decimal)ExecuteScalar(sQLText);
		}

		#endregion
		#region TestClientGetGLAutoJournalAmount
		[TestDate(2006, 12, 25)]
		public void TestClientGetGLAutoJournalAmount()
		{
			SetupAccountingPeriodsForTestClientGetGLAutoJournalAmount();
			AssertEquals(500m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2005, 1, 1), new ZDateTime(2005, 5, 13), 100, Env.CurrentCompany.PK));
			AssertEquals(10m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2005, 1, 1), new ZDateTime(2005, 1, 1), 10, Env.CurrentCompany.PK));
			AssertEquals(240m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2006, 1, 1), new ZDateTime(2006, 12, 19), 20, Env.CurrentCompany.PK));
			AssertEquals(40m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2006, 1, 1), new ZDateTime(2006, 2, 3), 20, Env.CurrentCompany.PK));
			AssertEquals(1700m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2005, 1, 1), new ZDateTime(2006, 5, 13), 100, Env.CurrentCompany.PK));
			AssertEquals("Accounting Periods have not been setup for this company", 0m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2005, 1, 1), new ZDateTime(2005, 5, 13), 100, OtherCompany.PK.ToGuid()));
			AssertEquals("ReverseDate is later than current date, should use the current date as the EndDate", 240m, ExecuteClientGetGLAutoJournalAmount(new ZDateTime(2005, 1, 1), new ZDateTime(2007, 8, 13), 10, Env.CurrentCompany.PK));
		}

		void SetupAccountingPeriodsForTestClientGetGLAutoJournalAmount()
		{
			NewYearPeriodSettings year2005PeriodSettings = new NewYearPeriodSettings();
			year2005PeriodSettings.StartDate = new ZDateTime(2005, 1, 1);
			NewYearPeriodSettings year2006PeriodSettings = new NewYearPeriodSettings();
			year2006PeriodSettings.StartDate = new ZDateTime(2006, 1, 1);
			PeriodManager periodManager = new PeriodManager(Factory);
			periodManager.CreatePeriodData(year2005PeriodSettings, Factory);
			periodManager.CreatePeriodData(year2006PeriodSettings, Factory);
			Factory.Save();
		}

		decimal ExecuteClientGetGLAutoJournalAmount(ZDateTime postDate, ZDateTime reverseDate, decimal lineAmount, Guid companyPK)
		{
			string sQLText = string.Format("SELECT dbo.ClientGetGLAutoJournalAmount('{0}', '{1}', '{2}', {3}, '{4}')", postDate.SqlFormat, reverseDate.SqlFormat, ZDateTime.Now.SqlFormat, lineAmount, companyPK);
			return (decimal)ExecuteScalar(sQLText);
		}

		#endregion
		#region TestClientGetFirstPeriodForCurrentYear
		public void TestClientGetFirstPeriodForCurrentYear()
		{
			Guid currentCompanyGuid = GlbCompany.CurrentCompany.PK.ToGuid();
			Guid otherCompanyGuid = OtherCompany.PK.ToGuid();
			SetupPeriodsForTestClientGetFirstPeriodForCurrentYear();
			AssertEquals(200601, ExecuteClientGetFirstPeriodForCurrentYear(currentCompanyGuid, new ZDateTime(2006, 8, 29)));
			AssertEquals(200605, ExecuteClientGetFirstPeriodForCurrentYear(otherCompanyGuid, new ZDateTime(2006, 8, 29)));
			AssertEquals("Period is not setup for year 2005, should return 0", 0, ExecuteClientGetFirstPeriodForCurrentYear(currentCompanyGuid, new ZDateTime(2005, 8, 29)));
		}

		void SetupPeriodsForTestClientGetFirstPeriodForCurrentYear()
		{
			CreatePeriod(200601, new ZDateTime(2006, 1, 1), new ZDateTime(2006, 1, 31), GlbCompany.CurrentCompany);
			CreatePeriod(200602, new ZDateTime(2006, 2, 1), new ZDateTime(2006, 2, 28), GlbCompany.CurrentCompany);
			CreatePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31), GlbCompany.CurrentCompany);
			CreatePeriod(200604, new ZDateTime(2006, 4, 1), new ZDateTime(2006, 4, 30), GlbCompany.CurrentCompany);
			CreatePeriod(200605, new ZDateTime(2006, 5, 1), new ZDateTime(2006, 5, 31), OtherCompany);
			CreatePeriod(200606, new ZDateTime(2006, 6, 1), new ZDateTime(2006, 6, 30), OtherCompany);
			CreatePeriod(200607, new ZDateTime(2006, 7, 1), new ZDateTime(2006, 7, 31), OtherCompany);
			CreatePeriod(200608, new ZDateTime(2006, 8, 1), new ZDateTime(2006, 8, 31), OtherCompany);
			Factory.Save();
		}

		void CreatePeriod(ZInt period, ZDateTime start, ZDateTime end, GlbCompany company)
		{
			AccPeriodManagement periodManagement = Factory.New<AccPeriodManagement>();
			periodManagement.AM_Period = period;
			periodManagement.AM_StartDate = start;
			periodManagement.AM_EndDate = end;
			periodManagement.AM_Year = (ZShort)start.Year;
			periodManagement.AM_GC_Company = company.PK;
		}

		int ExecuteClientGetFirstPeriodForCurrentYear(Guid companyPK, ZDateTime periodDate)
		{
			string sQLText = string.Format("SELECT dbo.ClientGetFirstPeriodForCurrentYear('{0}', '{1}')", companyPK, periodDate.SqlFormat);
			return (int)ExecuteScalar(sQLText);
		}

		#endregion
		#region TestClientGetCognosAgeFromDate
		public void TestClientGetCognosAgeFromDate()
		{
			AssertEquals("Within 0 - 30 days, should return 30", "30", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 1, 1), new ZDateTime(2006, 1, 4)));
			AssertEquals("Within 0 - 30 days, should return 30", "30", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 1, 1), new ZDateTime(2006, 1, 31)));
			AssertEquals("Within 31 - 60 days, should return 60", "60", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 3, 1), new ZDateTime(2006, 4, 1)));
			AssertEquals("Within 31 - 60 days, should return 60", "60", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 3, 1), new ZDateTime(2006, 4, 30)));
			AssertEquals("Within 60 - 90 days, should return 90", "90", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 3, 1), new ZDateTime(2006, 5, 1)));
			AssertEquals("Within 60 - 90 days, should return 90", "90", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 3, 1), new ZDateTime(2006, 5, 30)));
			AssertEquals("More than 90 days, should return ++", "++", ExecuteClientGetCognosAgeFromDate(new ZDateTime(2006, 3, 1), new ZDateTime(2006, 5, 31)));
		}

		string ExecuteClientGetCognosAgeFromDate(ZDateTime transactionDate, ZDateTime exportDate)
		{
			string sQLText = string.Format("SELECT dbo.ClientGetCognosAgeFromDate('{0}', '{1}')", transactionDate.SqlFormat, exportDate.SqlFormat);
			return ExecuteScalar(sQLText).ToString();
		}

		#endregion
		#region TestClientGetMaturityDateForNetting
		[TestDate(2006, 5, 1)]
		public void TestClientGetMaturityDateForNetting()
		{
			JASForwardingShipment shipment1 = Factory.New<JASForwardingShipment>();
			shipment1.FillWithValidTestData();
			shipment1.JS_E_DEP = new ZDateTime(2007, 1, 1);
			JASForwardingConsol consol1 = (JASForwardingConsol)shipment1.Consols.AddNew();
			consol1.FillWithValidTestData();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "MYBAG";
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			Transport transport = consol1.Transports[0];
			transport.JW_ETD = new ZDateTime(2008, 1, 1);
			transport.JW_VoyageFlight = "QF0983";
			Factory.Save();
			AssertEquals(new ZDateTime(2007, 3, 17), ExecuteClientGetMaturityDateForNetting(shipment1, consol1));
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			transport.JW_Vessel = "MAJAPAHIT";
			Factory.Save();
			AssertEquals(new ZDateTime(2007, 3, 17), ExecuteClientGetMaturityDateForNetting(shipment1, consol1));
			shipment1.JS_E_DEP = ZDateTime.Empty;
			Factory.Save();
			AssertEquals(new ZDateTime(2008, 3, 16), ExecuteClientGetMaturityDateForNetting(shipment1, consol1));
			shipment1.Consols.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals(new ZDateTime(2006, 7, 15), ExecuteClientGetMaturityDateForNetting(shipment1, null));
			shipment1.JS_TransportMode = Constants.TransportModes.AirSea;
			Factory.Save();
			AssertEquals(new ZDateTime(2006, 7, 15), ExecuteClientGetMaturityDateForNetting(shipment1, null));
			JASDataRegistry.Instance.NettingPaymentTerms = 20;
			shipment1.JS_TransportMode = Constants.TransportModes.SeaAir;
			Factory.Save();
			AssertEquals(new ZDateTime(2006, 6, 20), ExecuteClientGetMaturityDateForNetting(shipment1, null));
			shipment1.JS_TransportMode = Constants.TransportModes.Road;
			Factory.Save();
			AssertEquals(new ZDateTime(2006, 6, 20), ExecuteClientGetMaturityDateForNetting(shipment1, null));
		}

		ZDateTime ExecuteClientGetMaturityDateForNetting(JASForwardingShipment shipment, JASForwardingConsol consol)
		{
			string etd = (shipment.JS_E_DEP.IsEmpty) ? "NULL" : string.Format("'{0}'", shipment.JS_E_DEP);
			string consolPK = (consol == null) ? "NULL" : string.Format("'{0}'", consol.PK);
			string sQLText = string.Format("SELECT dbo.ClientGetMaturityDateForNetting('{0}', {1}, {2})", shipment.PK, etd, consolPK);
			return new ZDateTime((DateTime)ExecuteScalar(sQLText));
		}

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromHeader, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "101", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "102", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "103", BSHAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader()
		{
			// Included            
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, AUCOROrg, BSHAccount1, MILBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, BSHAccount1, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, USCOROrg, BSHAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, null, BSHAccount1, VALBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, null, BSHAccount1, VALBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, BSHAccount1, MILBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, null, BSHAccount1, MILBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AUCOROrg, PnLAccount1, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, PnLAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, AUCOROrg, PnLAccount1, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, USCOROrg, PnLAccount1, MILBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AUCOROrg, PnLAccount1, MILBranch, MEDepartment);
			// Not Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Overpayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, USCOROrg, BSHAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, false, Core.Constants.CurrencyCodes.Indonesia, DateInPnLPeriod, AUCOROrg, PnLAccount1, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, "", DateInPnLPeriod, null, PnLAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateAfterExportStartDate, AUCOROrg, BSHAccount1, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, null, PnLAccount1, VALBranch, AEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromHeader = @"
101,++,,,,MI,MIL,,-100,AUD,-100,
101,30,,,,MI,MIL,,-100,AUD,-100,
101,60,,,,AE,MIL,,-100,AUD,-100,
101,90,,,,AE,VAL,,-200,AUD,-200,
101,90,AUCOR,INT,INT,AI,MIL,,-100,AUD,-100,ANZ
101,90,USCOR,INT,INT,MI,VAL,,-100,USD,-50,NAM
102,30,AUCOR,INT,INT,AI,VAL,,-100,AUD,-100,ANZ
102,60,,,,MI,VAL,,-100,AUD,-100,
102,60,AUCOR,INT,INT,ME,MIL,,-100,USD,-50,ANZ
102,60,AUCOR,INT,INT,ME,VAL,,-100,USD,-50,ANZ
102,60,USCOR,INT,INT,MI,MIL,,-100,USD,-50,NAM".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank
		[SuspendCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromHeaderBank, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "104", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "105", BSHAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "106", BSHAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank()
		{
			// Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Payment, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, ARBUEOrg, BankAccount1, MILBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Receipt, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, IDJKTOrg, BankAccount1, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Receipt, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, ARBUEOrg, BankAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Payment, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, ARBUEOrg, BankAccount1, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Transfer, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, ARBUEOrg, BankAccount1, VALBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, BankAccount1, MILBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, BankAccount1, MILBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Receipt, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, IDJKTOrg, BankAccount1, MILBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, ARBUEOrg, BankAccount2, VALBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, IDJKTOrg, BankAccount2, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Transfer, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, IDJKTOrg, BankAccount2, VALBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Transfer, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, IDJKTOrg, BankAccount2, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Receipt, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, null, BankAccount2, MILBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Payment, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, null, BankAccount2, MILBranch, MIDepartment);
			// Not Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.WIPAccrualJournal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, AUCOROrg, BankAccount2, MILBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, null, BankAccount2, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Overpayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, USCOROrg, BankAccount2, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Indonesia, DateInPnLPeriod, AUCOROrg, BankAccount2, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Transfer, true, "", DateInPnLPeriod, null, BankAccount2, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AUCOROrg, BankAccount2, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Payment, true, Core.Constants.CurrencyCodes.Australia, DateAfterExportStartDate, null, BankAccount2, VALBranch, AEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromHeaderBank = @"
104,++,IDJKT,TPY,ASC,MI,MIL,,-100,AUD,-100,SEA
104,30,,,,ME,MIL,,200,AUD,200,
104,60,IDJKT,TPY,ASC,AE,MIL,,-100,AUD,-100,SEA
104,90,ARBUE,INT,INT,AE,VAL,,100,AUD,100,SAM
104,90,ARBUE,INT,INT,AI,MIL,,-100,AUD,-100,SAM
104,90,ARBUE,INT,INT,AI,VAL,,-100,AUD,-100,SAM
104,90,ARBUE,INT,INT,MI,VAL,,-100,USD,-50,SAM
105,++,,,,MI,MIL,,-200,AUD,-200,
105,30,IDJKT,TPY,ASC,AE,MIL,,100,USD,50,SEA
105,30,IDJKT,TPY,ASC,ME,VAL,,100,USD,50,SEA
105,90,ARBUE,INT,INT,AE,VAL,,100,USD,50,SAM
105,90,IDJKT,TPY,ASC,AE,VAL,,100,USD,50,SEA".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "107", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "108", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.ProfitAndLossAccount, "109", PnLAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine()
		{
			// Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, PnLAccount1, ROMBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, ROMBranch, AEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, MILBranch, AIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AEDXBOrg, PnLAccount1, VALBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, DEFRAOrg, PnLAccount1, ROMBranch, MEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, DEFRAOrg, PnLAccount1, ROMBranch, MEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, AEDXBOrg, BSHAccount1, ROMBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, AEDXBOrg, BSHAccount1, ROMBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, BSHAccount1, VALBranch, MEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, BSHAccount1, VALBranch, MEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, DEFRAOrg, BSHAccount1, VALBranch, AIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, DEFRAOrg, BSHAccount1, MILBranch, AEDepartment);
			// Not Included
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, null, PnLAccount1, ROMBranch, MIDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, ROMBranch, AEDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Contra, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, MILBranch, AIDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AEDXBOrg, PnLAccount1, VALBranch, MIDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, DEFRAOrg, PnLAccount1, ROMBranch, MEDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, "", DateInPnLPeriod, DEFRAOrg, PnLAccount1, ROMBranch, MEDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, DEFRAOrg, PnLAccount1, ROMBranch, MEDepartment);
			//CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateAfterExportStartDate, DEFRAOrg, BSHAccount1, ROMBranch, MEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine = @"
107,30,,,,MI,ROM,,-30,USD,-15,
107,60,AEDXB,ASC,ASC,AE,ROM,,-30,USD,-15,MEA
107,60,AEDXB,ASC,ASC,AI,MIL,,-30,USD,-15,MEA
107,60,AEDXB,ASC,ASC,MI,VAL,,-30,AUD,-30,MEA
107,60,DEFRA,,TPY,ME,ROM,,-60,USD,-30,EUR
108,++,AEDXB,ASC,ASC,MI,ROM,,-60,AUD,-60,MEA
108,30,,,,ME,VAL,,-60,AUD,-60,
108,60,DEFRA,,TPY,AI,VAL,,-30,AUD,-30,EUR
108,90,DEFRA,,TPY,AE,MIL,,-30,AUD,-30,EUR".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "110", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "111", BSHAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "112", BSHAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank()
		{
			// Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateNotWithinCognosAccountAgingRange, null, BankAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, BankAccount1, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, INBOMOrg, BankAccount1, ROMBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, INBOMOrg, BankAccount1, ROMBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, ZACPTOrg, BankAccount2, ROMBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZACPTOrg, BankAccount2, MILBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, ZACPTOrg, BankAccount2, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, ZACPTOrg, BankAccount2, ROMBranch, MIDepartment);
			// Not Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, false, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, null, BankAccount1, ROMBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, "", DateNotWithinCognosAccountAgingRange, null, BankAccount2, ROMBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.Australia, DateAfterExportStartDate, null, BankAccount2, MILBranch, MEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank = @"
110,++,,,,MI,VAL,,110,USD,55,
110,30,,,,AE,MIL,,110,USD,55,
110,60,INBOM,INT,ASC,AI,ROM,,110,USD,55,OTH
110,90,INBOM,INT,ASC,ME,ROM,,110,USD,55,OTH
111,++,ZACPT,TPY,,MI,ROM,,110,AUD,110,AFR
111,30,ZACPT,TPY,,ME,MIL,,110,AUD,110,AFR
111,60,ZACPT,TPY,,AI,VAL,,110,AUD,110,AFR
111,90,ZACPT,TPY,,MI,ROM,,110,AUD,110,AFR".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj
		[SuspendCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromInvCrdAdj, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "113", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "114", PnLAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.ProfitAndLossAccount, "115", PnLAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj()
		{
			// Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, null, MILBranch, AEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.CreditNote, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, null, ROMBranch, AIDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, USCOROrg, null, VALBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, USCOROrg, PnLAccount1, MILBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, AUCOROrg, null, MILBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, AUCOROrg, null, ROMBranch, MIDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.CreditNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, null, null, ROMBranch, AEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, null, PnLAccount2, VALBranch, AIDepartment);
			// Not Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Contra, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, USCOROrg, null, MILBranch, AEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, USCOROrg, null, ROMBranch, AIDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, USCOROrg, null, VALBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, USCOROrg, PnLAccount1, MILBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice, true, "", ExportStartDate, AUCOROrg, null, MILBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, AUCOROrg, null, ROMBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.CreditNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AUCOROrg, null, ROMBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.WIP);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AUCOROrg, null, VALBranch, MEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromInvCrdAdj = @"
113,30,,,,AE,MIL,,-30,AUD,-30,
113,30,,,,AI,ROM,,-30,AUD,-30,
113,60,USCOR,INT,INT,ME,VAL,,-30,AUD,-30,NAM
113,60,USCOR,INT,INT,MI,MIL,,-30,AUD,-30,NAM
114,30,AUCOR,INT,INT,ME,MIL,,-30,USD,-15,ANZ
114,30,AUCOR,INT,INT,MI,ROM,,-30,USD,-15,ANZ
114,60,,,,AE,ROM,,-30,USD,-15,
114,60,,,,AI,VAL,,-30,USD,-15,
114,60,AUCOR,INT,INT,ME,ROM,,-30,USD,-15,ANZ".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX
		[SuspendCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromCFX, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "116", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "117", PnLAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.ProfitAndLossAccount, "118", PnLAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX()
		{
			// Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, INBOMOrg, null, MILBranch, AEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, INBOMOrg, null, ROMBranch, AIDepartment, true, ZArchitecture.Core.TransactionLineTypes.Accrual);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, null, VALBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Accrual);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, PnLAccount1, MILBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, AUCOROrg, null, MILBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, AUCOROrg, null, ROMBranch, MIDepartment, true, ZArchitecture.Core.TransactionLineTypes.WIP);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, null, null, ROMBranch, AEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, null, PnLAccount2, VALBranch, AIDepartment);
			// Not Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, INBOMOrg, null, MILBranch, AEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Cost);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, INBOMOrg, null, ROMBranch, AIDepartment, true, ZArchitecture.Core.TransactionLineTypes.Accrual);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, null, VALBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Accrual);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, "", DateInPnLPeriod, null, PnLAccount1, MILBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, AUCOROrg, null, MILBranch, MEDepartment, true, ZArchitecture.Core.TransactionLineTypes.Revenue);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, null, null, VALBranch, AIDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromCFX = @"
116,30,INBOM,INT,ASC,AE,MIL,,-30,AUD,-30,OTH
116,30,INBOM,INT,ASC,AI,ROM,,0,AUD,0,OTH
116,60,,,,ME,VAL,,0,AUD,0,
116,60,,,,MI,MIL,,30,AUD,30,
117,30,AUCOR,INT,INT,ME,MIL,,-30,USD,-15,ANZ
117,30,AUCOR,INT,INT,MI,ROM,,0,USD,0,ANZ
117,60,,,,AE,ROM,,-30,USD,-15,
117,60,,,,AI,VAL,,30,USD,15,".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals
		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromGLJournals, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "119", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "120", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.ProfitAndLossAccount, "121", PnLAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals()
		{
			// Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLStandardJournal, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, PnLAccount1, ROMBranch, MIDepartment);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLReversingJournal, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, PnLAccount1, ROMBranch, AEDepartment, DateAfterExportStartDate);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLAutoJournal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, MILBranch, AIDepartment, DateAfterExportStartDate);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLStandardJournal, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, VALBranch, MIDepartment);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLAutoJournal, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, DEFRAOrg, BSHAccount1, ROMBranch, MIDepartment, DateAfterExportStartDate);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLStandardJournal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, BSHAccount1, VALBranch, MEDepartment);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLReversingJournal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, BSHAccount1, VALBranch, MEDepartment, DateAfterExportStartDate);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLAutoJournal, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, DEFRAOrg, BSHAccount1, VALBranch, AIDepartment, DateInPnLPeriod);
			// Not Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.JobCosting, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, PnLAccount1, ROMBranch, MIDepartment);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLAutoJournal, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, AEDXBOrg, PnLAccount1, MILBranch, AIDepartment, DateAfterExportStartDate);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLStandardJournal, true, "", DateInPnLPeriod, AEDXBOrg, PnLAccount1, VALBranch, MIDepartment);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLAutoJournal, true, Core.Constants.CurrencyCodes.Australia, DateAfterExportStartDate, DEFRAOrg, BSHAccount1, MILBranch, AEDepartment, DateAfterExportStartDate.AddMonths(1));
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLStandardJournal, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, null, VALBranch, MEDepartment);
			CreateTransactionForGLJournals(ZArchitecture.Core.LedgerTypes.General, ZArchitecture.Core.TransactionTypes.GLReversingJournal, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, BSHAccount1, VALBranch, MEDepartment, ExportStartDate);
		}

		void CreateTransactionForGLJournals(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccGLHeader gLAccount, GlbBranch branch, GlbDepartment department, ZDateTime dueDate)
		{
			AccTransactionHeader transaction = CreateTransactionWithLine(ledgerType, transactionType, postToGL, currencyCode, postDate, counterCompany, gLAccount, branch, department);
			transaction.AH_DueDate = dueDate;
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromGLJournals = @"
119,30,,,,AE,ROM,,30,USD,15,
119,30,,,,MI,ROM,,30,USD,15,
119,30,AEDXB,ASC,ASC,AI,MIL,,30,USD,15,MEA
119,60,AEDXB,ASC,ASC,AI,MIL,,30,USD,15,MEA
119,60,AEDXB,ASC,ASC,MI,VAL,,30,USD,15,MEA
119,90,AEDXB,ASC,ASC,AI,MIL,,30,USD,15,MEA
120,++,DEFRA,,TPY,AI,VAL,,30,AUD,30,EUR
120,++,DEFRA,,TPY,MI,ROM,,360,AUD,360,EUR
120,30,,,,ME,VAL,,30,AUD,30,
120,30,DEFRA,,TPY,MI,ROM,,30,AUD,30,EUR
120,60,,,,ME,VAL,,30,AUD,30,
120,60,DEFRA,,TPY,MI,ROM,,30,AUD,30,EUR
120,90,DEFRA,,TPY,AI,VAL,,30,AUD,30,EUR
120,90,DEFRA,,TPY,MI,ROM,,30,AUD,30,EUR".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR
		[SuspendCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromWIPACR, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR()
		{
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "122", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "123", PnLAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.ProfitAndLossAccount, "124", PnLAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR()
		{
			// Included
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ExportStartDate, null, ChargeCode, ROMBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ExportStartDate, null, ChargeCode, VALBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, DateAfterExportStartDate, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ExportStartDate, null, ChargeCode, ROMBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, VALBranch, MEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, MILBranch, MIDepartment);
			// Not Included
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, true, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, false, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, ROMBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, "", ExportStartDate, DateAfterExportStartDate, null, ChargeCode, VALBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, DateNotWithinCognosAccountAgingRange, DateAfterExportStartDate, null, null, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, ExportStartDate, null, null, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, OTHBranch, MIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, OTHBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ExportStartDate, null, ChargeCode, OTHBranch, AIDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromWIPACR = @"
122,30,,,,AE,MIL,,30,AUD,30,
122,30,,,,AE,VAL,,-30,AUD,-30,
122,30,,,,AE,VAL,,30,AUD,30,
122,30,,,,AI,ROM,,-30,AUD,-30,
122,30,,,,AI,ROM,,30,AUD,30,
122,60,,,,AE,MIL,,30,AUD,30,
123,30,,,,AE,MIL,,30,USD,15,
123,30,,,,AI,ROM,,-30,USD,-15,
123,60,,,,AI,ROM,,30,USD,15,
123,60,,,,ME,VAL,,30,USD,15,
123,60,,,,MI,MIL,,30,USD,15,".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromARAPControlAccounts, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts()
		{
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount2.PK.ToGuid());
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "125", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "126", BSHAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "127", BSHAccount2);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts()
		{
			AccGLHeader nullAccount = null;
			// Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Journal, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, AUCOROrg, nullAccount, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Payment, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, AUCOROrg, nullAccount, MILBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Receipt, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AUCOROrg, nullAccount, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Contra, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AUCOROrg, nullAccount, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, null, nullAccount, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Discount, true, Core.Constants.CurrencyCodes.Australia, DateInBSHPeriodButNotPnLPeriod, null, nullAccount, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Overpayment, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, USCOROrg, nullAccount, ROMBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, USCOROrg, nullAccount, ROMBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, USCOROrg, nullAccount, ROMBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.CreditNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, null, nullAccount, MILBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Transfer, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, null, nullAccount, MILBranch, MIDepartment);
			// Not Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Contra, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AUCOROrg, nullAccount, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, "", DateInBSHPeriodButNotPnLPeriod, null, nullAccount, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Discount, true, Core.Constants.CurrencyCodes.Australia, DateAfterExportStartDate, null, nullAccount, VALBranch, MIDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromARAPControlAccounts = @"
125,++,AUCOR,INT,INT,AE,MIL,,100,AUD,100,ANZ
125,30,AUCOR,INT,INT,AI,MIL,,100,AUD,100,ANZ
125,60,AUCOR,INT,INT,AI,VAL,,100,AUD,100,ANZ
125,60,AUCOR,INT,INT,ME,VAL,,100,AUD,100,ANZ
125,90,,,,ME,VAL,,100,AUD,100,
125,90,,,,MI,VAL,,100,AUD,100,
126,30,USCOR,INT,INT,AE,ROM,,210,USD,105,NAM
126,60,USCOR,INT,INT,ME,ROM,,110,USD,55,NAM
126,90,,,,AI,MIL,,110,USD,55,
126,90,,,,MI,MIL,,100,USD,50,".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts()
		{
			AccountingConfigurationRegistry.Instance.OverpaymentsAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount1.PK.ToGuid());
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "128", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "129", PnLAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "130", PnLAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "131", BSHAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts()
		{
			AccGLHeader nullAccount = null;
			// Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Overpayment, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, null, nullAccount, MILBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Overpayment, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, AUCOROrg, nullAccount, VALBranch, AIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Discount, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, null, PnLAccount1, VALBranch, MIDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Discount, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AUCOROrg, PnLAccount1, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Discount, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, null, PnLAccount1, VALBranch, MEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, PnLAccount2, ROMBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, null, PnLAccount2, ROMBranch, AEDepartment);
			CreateTransaction(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, USCOROrg, PnLAccount2, ROMBranch, MEDepartment);
			// Not Included
			CreateTransaction(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.ExchangeDifference, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, null, PnLAccount2, ROMBranch, AEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts = @"
128,++,,,,AE,MIL,,-100,AUD,-100,
128,30,AUCOR,INT,INT,AI,VAL,,-100,AUD,-100,ANZ
129,30,,,,MI,VAL,,-100,AUD,-100,
129,60,,,,ME,VAL,,-100,AUD,-100,
129,60,AUCOR,INT,INT,ME,VAL,,-100,AUD,-100,ANZ
130,30,,,,AE,ROM,,-200,USD,-100,
130,60,USCOR,INT,INT,ME,ROM,,-100,USD,-50,NAM".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts
		[SuspendGLAccountAndChargeCodeCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromGSTControlAccounts, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts()
		{
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount2.PK.ToGuid());
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "132", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "133", BSHAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "134", BSHAccount2);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts()
		{
			AccGLHeader nullAccount = null;
			// Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.CreditNote, true, Core.Constants.CurrencyCodes.Australia, DateNotWithinCognosAccountAgingRange, null, nullAccount, MILBranch, AEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, AUCOROrg, nullAccount, MILBranch, AIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, true, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, AUCOROrg, nullAccount, VALBranch, AIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectReceipt, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, USCOROrg, nullAccount, VALBranch, MEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, USCOROrg, nullAccount, VALBranch, AEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, USCOROrg, nullAccount, VALBranch, MIDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.CreditNote, true, Core.Constants.CurrencyCodes.UnitedStates, DateInBSHPeriodButNotPnLPeriod, null, nullAccount, ROMBranch, AEDepartment);
			// Not Included
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.CashBook, ZArchitecture.Core.TransactionTypes.DirectPayment, false, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, USCOROrg, nullAccount, VALBranch, MEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.AdjustmentNote, true, "", DateInPnLPeriod, USCOROrg, nullAccount, VALBranch, AEDepartment);
			CreateTransactionWithLine(ZArchitecture.Core.LedgerTypes.AccountsPayable, ZArchitecture.Core.TransactionTypes.Invoice, true, Core.Constants.CurrencyCodes.UnitedStates, DateAfterExportStartDate, USCOROrg, nullAccount, VALBranch, MIDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromGSTControlAccounts = @"
132,++,,,,AE,MIL,,-10,AUD,-10,
132,30,AUCOR,INT,INT,AI,MIL,,-10,AUD,-10,ANZ
132,60,AUCOR,INT,INT,AI,VAL,,-10,AUD,-10,ANZ
133,30,USCOR,INT,INT,ME,VAL,,-10,USD,-5,NAM
133,60,USCOR,INT,INT,AE,VAL,,-10,USD,-5,NAM
133,90,,,,AE,ROM,,-10,USD,-5,
133,90,USCOR,INT,INT,MI,VAL,,-10,USD,-5,NAM".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts
		[SuspendCriticalValidation]
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts()
		{
			SetupMappingsForCognosRawAggregateTests();
			SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts();
			using (new CognosTempTableCreator())
			{
				ExecuteNonQueryForCognosRawAggregateTests(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts, generatedTempTableString, '\n');
			}
		}

		void SetupDataForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts()
		{
			SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts();
			SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts();
			Factory.Save();
		}

		void SetupCognosAccountsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts()
		{
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, BSHAccount2.PK.ToGuid());
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "135", BSHAccount1);
			CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "136", BSHAccount2);
			CreateCognosAccount(Core.Constants.GLLanguages.English, Core.Constants.AccountType.BalanceSheetAccount, "137", BSHAccount1);
		}

		void SetupTransactionsForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts()
		{
			// Included
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ExportStartDate, null, ChargeCode, ROMBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ExportStartDate, null, ChargeCode, VALBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, DateAfterExportStartDate, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ExportStartDate, null, ChargeCode, ROMBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, VALBranch, MEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, MILBranch, MIDepartment);
			// Not Included
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, true, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, false, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, ROMBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, true, "", ExportStartDate, DateAfterExportStartDate, null, ChargeCode, VALBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, ExportStartDate, ExportStartDate, null, null, MILBranch, AEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, true, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ExportStartDate, null, ChargeCode, OTHBranch, AIDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.WIP, true, false, Core.Constants.CurrencyCodes.UnitedStates, DateInPnLPeriod, ZDateTime.Empty, null, ChargeCode, OTHBranch, MEDepartment);
			CreateTransactionLine(ZArchitecture.Core.TransactionLineTypes.Accrual, true, false, Core.Constants.CurrencyCodes.Australia, ExportStartDate, ZDateTime.Empty, null, ChargeCode, OTHBranch, AEDepartment);
		}

		readonly string ExpectedCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts = @"
135,30,,,,AE,MIL,,-30,AUD,-30,
135,30,,,,AE,VAL,,-30,AUD,-30,
135,30,,,,AE,VAL,,30,AUD,30,
135,30,,,,AI,ROM,,-30,AUD,-30,
135,30,,,,AI,ROM,,30,AUD,30,
135,60,,,,AE,MIL,,-30,AUD,-30,
136,30,,,,AE,MIL,,-30,USD,-15,
136,30,,,,AI,ROM,,30,USD,15,
136,60,,,,AI,ROM,,-30,USD,-15,
136,60,,,,ME,VAL,,-30,USD,-15,
136,60,,,,MI,MIL,,-30,USD,-15,".TrimStart();

		#endregion
		#region TestClientInvertCognosRawAggregateSignageIfApplicable
		public void TestClientInvertCognosRawAggregateSignageIfApplicable()
		{
			using (new CognosTempTableCreator())
			{
				SetupRawAggregateTableForTestClientInvertCognosRawAggregateSignageIfApplicable();
				Factory.Save();
				AssertMultilineEquals("Pre-condition", ExpectedCognosRawAggregateTableBeforeClientInvertCognosRawAggregateSignageIfApplicable, LoadFromCognosRawAggregateTempTableAsCsvString(), '\n');
				ExecuteNonQuery("EXEC " + JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInvertCognosRawAggregateSignageIfApplicable);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTableAfterClientInvertCognosRawAggregateSignageIfApplicable, generatedTempTableString, '\n');
			}
		}

		void SetupRawAggregateTableForTestClientInvertCognosRawAggregateSignageIfApplicable()
		{
			CognosAccGLAccountDescriptor debitAcc1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "138", Core.Constants.DebitCredit.Debit, BSHAccount1);
			CognosAccGLAccountDescriptor creditAcc1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "139", Core.Constants.DebitCredit.Credit, BSHAccount2);
			CognosAccGLAccountDescriptor creditAcc2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "140", Core.Constants.DebitCredit.Credit, PnLAccount1);
			CognosAccGLAccountDescriptor debitAcc2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "141", Core.Constants.DebitCredit.Debit, PnLAccount2);
			InsertRecordIntoCognosRawAggregateTable(debitAcc1, "", null, "", "", 1m, "", 1m, "");
			InsertRecordIntoCognosRawAggregateTable(creditAcc1, "30", null, "", "", -2m, "", -2m, "");
			InsertRecordIntoCognosRawAggregateTable(creditAcc1, "60", null, "", "", 3m, "", 3m, "");
			InsertRecordIntoCognosRawAggregateTable(creditAcc2, "", null, "", "", -4m, "", -4m, "");
			InsertRecordIntoCognosRawAggregateTable(creditAcc2, "30", null, "", "", 5m, "", 5m, "");
			InsertRecordIntoCognosRawAggregateTable(creditAcc2, "60", null, "", "", -6m, "", -6m, "");
			InsertRecordIntoCognosRawAggregateTable(debitAcc2, "60", null, "", "", -7m, "", -7m, "");
			InsertRecordIntoCognosRawAggregateTable(debitAcc2, "90", null, "", "", 8m, "", 8m, "");
		}

		readonly string ExpectedCognosRawAggregateTableBeforeClientInvertCognosRawAggregateSignageIfApplicable = @"
138,,,,,,,,1,,1,
139,30,,,,,,,-2,,-2,
139,60,,,,,,,3,,3,
140,,,,,,,,-4,,-4,
140,30,,,,,,,5,,5,
140,60,,,,,,,-6,,-6,
141,60,,,,,,,-7,,-7,
141,90,,,,,,,8,,8,".TrimStart();

		readonly string ExpectedCognosRawAggregateTableAfterClientInvertCognosRawAggregateSignageIfApplicable = @"
138,,,,,,,,1,,1,
139,30,,,,,,,2,,2,
139,60,,,,,,,-3,,-3,
140,,,,,,,,4,,4,
140,30,,,,,,,-5,,-5,
140,60,,,,,,,6,,6,
141,60,,,,,,,-7,,-7,
141,90,,,,,,,8,,8,".TrimStart();

		#endregion
		#region TestClientUpdateCognosRawAggregateRecordsWithALTAccounts
		public void TestClientUpdateCognosRawAggregateRecordsWithALTAccounts()
		{
			using (new CognosTempTableCreator())
			{
				SetupRawAggregateTableForTestClientUpdateCognosRawAggregateRecordsWithALTAccounts();
				Factory.Save();
				AssertMultilineEquals("Pre-condition", ExpectedCognosRawAggregateTableBeforeClientUpdateCognosRawAggregateRecordsWithALTAccounts, LoadFromCognosRawAggregateTempTableAsCsvString(), '\n');
				ExecuteNonQuery("EXEC " + JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientUpdateCognosRawAggregateRecordsWithALTAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTableAfterClientUpdateCognosRawAggregateRecordsWithALTAccounts, generatedTempTableString, '\n');
			}
		}

		void SetupRawAggregateTableForTestClientUpdateCognosRawAggregateRecordsWithALTAccounts()
		{
			CognosAccGLAccountDescriptor account1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "142", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor account2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "143", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor account3 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "144", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor altAccount1 = CreateAlternateCognosAccount("145", Core.Constants.DebitCredit.Credit, account1);
			CognosAccGLAccountDescriptor altAccount2 = CreateAlternateCognosAccount("146", Core.Constants.DebitCredit.Credit, account2);
			CognosAccGLAccountDescriptor altAccount3 = CreateAlternateCognosAccount("147", Core.Constants.DebitCredit.Debit, account3);
			InsertRecordIntoCognosRawAggregateTable(account1, "", null, "", "", 1m, "", 1m, "");
			InsertRecordIntoCognosRawAggregateTable(account1, "30", null, "", "", -2m, "", -2m, "");
			InsertRecordIntoCognosRawAggregateTable(account1, "60", null, "", "", 3m, "", 3m, "");
			InsertRecordIntoCognosRawAggregateTable(account1, "90", null, "", "", -4m, "", -4m, "");
			InsertRecordIntoCognosRawAggregateTable(account2, "", null, "", "", 5m, "", 5m, "");
			InsertRecordIntoCognosRawAggregateTable(account2, "30", null, "", "", -6m, "", -6m, "");
			InsertRecordIntoCognosRawAggregateTable(account2, "60", null, "", "", -5m, "", -5m, "");
			InsertRecordIntoCognosRawAggregateTable(account2, "90", null, "", "", -8m, "", -8m, "");
			InsertRecordIntoCognosRawAggregateTable(account3, "", null, "", "", 1m, "", 1m, "");
			InsertRecordIntoCognosRawAggregateTable(account3, "30", null, "", "", 2m, "", 2m, "");
			InsertRecordIntoCognosRawAggregateTable(account3, "60", null, "", "", -7m, "", -7m, "");
			InsertRecordIntoCognosRawAggregateTable(account3, "90", null, "", "", 8m, "", 8m, "");
		}

		CognosAccGLAccountDescriptor CreateAlternateCognosAccount(ZString localAccountNumber, ZString debitCredit, CognosAccGLAccountDescriptor originalAccount)
		{
			CognosAccGLAccountDescriptor result = CreateCognosAccount(originalAccount.AJ_Language, Core.Constants.AccountType.Alternate, localAccountNumber, debitCredit, null);
			originalAccount.AJ_AJ_AlternativeNum = result.PK;
			return result;
		}

		readonly string ExpectedCognosRawAggregateTableBeforeClientUpdateCognosRawAggregateRecordsWithALTAccounts = @"
142,,,,,,,,1,,1,
142,30,,,,,,,-2,,-2,
142,60,,,,,,,3,,3,
142,90,,,,,,,-4,,-4,
143,,,,,,,,5,,5,
143,30,,,,,,,-6,,-6,
143,60,,,,,,,-5,,-5,
143,90,,,,,,,-8,,-8,
144,,,,,,,,1,,1,
144,30,,,,,,,2,,2,
144,60,,,,,,,-7,,-7,
144,90,,,,,,,8,,8,".TrimStart();

		readonly string ExpectedCognosRawAggregateTableAfterClientUpdateCognosRawAggregateRecordsWithALTAccounts = @"
144,,,,,,,,1,,1,
144,30,,,,,,,2,,2,
144,60,,,,,,,-7,,-7,
144,90,,,,,,,8,,8,
145,,,,,,,,-1,,-1,
145,30,,,,,,,2,,2,
145,60,,,,,,,-3,,-3,
145,90,,,,,,,4,,4,
146,,,,,,,,5,,5,
146,30,,,,,,,-6,,-6,
146,60,,,,,,,-5,,-5,
146,90,,,,,,,-8,,-8,".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts()
		{
			using (new CognosTempTableCreator())
			{
				SetupRawAggregateTableForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts();
				Factory.Save();
				AssertMultilineEquals("Pre-condition", ExpectedCognosRawAggregateTableBeforeCFWAndTTLCalculation, LoadFromCognosRawAggregateTempTableAsCsvString(), '\n');
				ExecuteNonQueryForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts();
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTableAfterCFWAndTTLCalculation, generatedTempTableString, '\n');
			}
		}

		void SetupRawAggregateTableForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts()
		{
			CognosAccGLAccountDescriptor bSHAccount1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "150", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor totalAccount1 = CreateCognosTotalAccount("151", Core.Constants.DebitCredit.Credit, 1);
			CognosAccGLAccountDescriptor bSHAccount2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "152", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor bSHAccount3 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "153", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor totalAccount2 = CreateCognosTotalAccount("154", Core.Constants.DebitCredit.Credit, 1);
			CognosAccGLAccountDescriptor bSHAccount4 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "155", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor totalAccount3 = CreateCognosTotalAccount("157", Core.Constants.DebitCredit.Credit, 2);
			CognosAccGLAccountDescriptor pnLAccount1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "200", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor totalAccount4 = CreateCognosTotalAccount("201", Core.Constants.DebitCredit.Debit, 3);
			CognosAccGLAccountDescriptor pnLAccount2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "202", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor pnLAccount3 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "203", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor totalAccount5 = CreateCognosTotalAccount("204", Core.Constants.DebitCredit.Credit, 3);
			CognosAccGLAccountDescriptor pnLAccount4 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "205", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor totalAccount6 = CreateCognosTotalAccount("206", Core.Constants.DebitCredit.Debit, 4);
			CognosAccGLAccountDescriptor cFWAccount = CreateCognosCFWAccount("156", Core.Constants.DebitCredit.Credit, totalAccount6);
			InsertRecordIntoCognosRawAggregateTable(bSHAccount1, "30", null, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount1, "60", null, "", "", 20m, "", 20m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount2, "30", null, "", "", 3m, "", 3m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount2, "30", null, "", "", -4m, "", -4m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount3, "60", null, "", "", 5m, "", 5m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount3, "60", null, "", "", -6m, "", -6m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount4, "60", null, "", "", 5m, "", 5m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount1, "30", null, "", "", 8m, "", 8m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount2, "30", null, "", "", 1m, "", 1m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount3, "60", null, "", "", 2m, "", 2m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount3, "60", null, "", "", -7m, "", -7m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount4, "30", null, "", "", 8m, "", 8m, "");
		}

		void ExecuteNonQueryForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts()
		{
			DbCommand command = GetDbCommand("EXEC " + JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts + " @PnLStartAccount, @BSHStartAccount");
			command.AddParameter("@PnLStartAccount", SqlDbType.VarChar, "199");
			command.AddParameter("@BSHStartAccount", SqlDbType.VarChar, "149");
			command.ExecuteNonQuery();
		}

		CognosAccGLAccountDescriptor CreateCognosTotalAccount(ZString localAccountNumber, ZString debitCredit, ZShort totalLevel)
		{
			CognosAccGLAccountDescriptor result = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.Total, localAccountNumber, debitCredit, null);
			result.AJ_TotalLevel = totalLevel;
			return result;
		}

		CognosAccGLAccountDescriptor CreateCognosCFWAccount(ZString localAccountNumber, ZString debitCredit, CognosAccGLAccountDescriptor totalAccount)
		{
			CognosAccGLAccountDescriptor result = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, AccountTypeComboBoxConstants.CarriedForwardAccount, localAccountNumber, debitCredit, null);
			totalAccount.AJ_AJ_CarriedForwardAccount = result.PK;
			return result;
		}

		readonly string ExpectedCognosRawAggregateTableBeforeCFWAndTTLCalculation = @"
150,30,,,,,,,10,,10,
150,60,,,,,,,20,,20,
152,30,,,,,,,-4,,-4,
152,30,,,,,,,3,,3,
153,60,,,,,,,-6,,-6,
153,60,,,,,,,5,,5,
155,60,,,,,,,5,,5,
200,30,,,,,,,8,,8,
202,30,,,,,,,1,,1,
203,60,,,,,,,-7,,-7,
203,60,,,,,,,2,,2,
205,30,,,,,,,8,,8,".TrimStart();

		readonly string ExpectedCognosRawAggregateTableAfterCFWAndTTLCalculation = @"
150,30,,,,,,,10,,10,
150,60,,,,,,,20,,20,
151,30,,,,,,,10,,10,
151,60,,,,,,,20,,20,
152,30,,,,,,,-4,,-4,
152,30,,,,,,,3,,3,
153,60,,,,,,,-6,,-6,
153,60,,,,,,,5,,5,
154,30,,,,,,,-1,,-1,
154,60,,,,,,,1,,1,
155,60,,,,,,,5,,5,
156,30,,,,,,,-17,,-17,
156,60,,,,,,,5,,5,
157,30,,,,,,,-8,,-8,
157,60,,,,,,,21,,21,
200,30,,,,,,,8,,8,
201,30,,,,,,,8,,8,
202,30,,,,,,,1,,1,
203,60,,,,,,,-7,,-7,
203,60,,,,,,,2,,2,
204,30,,,,,,,1,,1,
204,60,,,,,,,-5,,-5,
205,30,,,,,,,8,,8,
206,30,,,,,,,17,,17,
206,60,,,,,,,-5,,-5,".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts
		public void TestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts()
		{
			using (new CognosTempTableCreator())
			{
				SetupRawAggregateTableForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts();
				Factory.Save();
				AssertMultilineEquals("Pre-condition", ExpectedCognosRawAggregateTableBeforeCLNCalculation, LoadFromCognosRawAggregateTempTableAsCsvString(), '\n');
				ExecuteNonQuery("EXEC " + JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTableAfterCLNCalculation, generatedTempTableString, '\n');
			}
		}

		void SetupRawAggregateTableForTestClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts()
		{
			CognosAccGLAccountDescriptor bSHAccount1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "300.1", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor bSHAccount2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "300.2", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor bSHAccount3 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "300.3", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor consolidation1 = CreateCognosConsolidationAccount("300", Core.Constants.DebitCredit.Debit, bSHAccount1, bSHAccount2, bSHAccount3);
			CognosAccGLAccountDescriptor pnLAccount1 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "301.1", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor pnLAccount2 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "301.2", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor pnLAccount3 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.ProfitAndLossAccount, "301.3", Core.Constants.DebitCredit.Debit, null);
			CognosAccGLAccountDescriptor consolidation2 = CreateCognosConsolidationAccount("301", Core.Constants.DebitCredit.Credit, pnLAccount1, pnLAccount2, pnLAccount3);
			InsertRecordIntoCognosRawAggregateTable(bSHAccount1, "60", null, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount2, "60", null, "", "", 3m, "", 3m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount2, "90", null, "", "", -4m, "", -4m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount3, "60", null, "", "", 5m, "", 5m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount3, "60", null, "", "", -6m, "", -6m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount1, "60", null, "", "", -8m, "", -8m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount1, "90", null, "", "", 1m, "", 1m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount2, "90", null, "", "", 1m, "", 1m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount3, "90", null, "", "", 2m, "", 2m, "");
			InsertRecordIntoCognosRawAggregateTable(pnLAccount3, "90", null, "", "", -7m, "", -7m, "");
		}

		CognosAccGLAccountDescriptor CreateCognosConsolidationAccount(ZString localAccountNumber, ZString debitCredit, params CognosAccGLAccountDescriptor[] toBeConsolidated)
		{
			CognosAccGLAccountDescriptor result = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.Consolidation, localAccountNumber, debitCredit, null);
			foreach (CognosAccGLAccountDescriptor account in toBeConsolidated)
			{
				account.AJ_AJ_ConsolidationNum = result.PK;
			}

			return result;
		}

		readonly string ExpectedCognosRawAggregateTableBeforeCLNCalculation = @"
300.1,60,,,,,,,10,,10,
300.2,60,,,,,,,3,,3,
300.2,90,,,,,,,-4,,-4,
300.3,60,,,,,,,-6,,-6,
300.3,60,,,,,,,5,,5,
301.1,60,,,,,,,-8,,-8,
301.1,90,,,,,,,1,,1,
301.2,90,,,,,,,1,,1,
301.3,90,,,,,,,-7,,-7,
301.3,90,,,,,,,2,,2,".TrimStart();

		readonly string ExpectedCognosRawAggregateTableAfterCLNCalculation = @"
300,60,,,,,,,-14,,-14,
300,90,,,,,,,4,,4,
300.1,60,,,,,,,10,,10,
300.2,60,,,,,,,3,,3,
300.2,90,,,,,,,-4,,-4,
300.3,60,,,,,,,-6,,-6,
300.3,60,,,,,,,5,,5,
301,60,,,,,,,-8,,-8,
301,90,,,,,,,7,,7,
301.1,60,,,,,,,-8,,-8,
301.1,90,,,,,,,1,,1,
301.2,90,,,,,,,1,,1,
301.3,90,,,,,,,-7,,-7,
301.3,90,,,,,,,2,,2,".TrimStart();

		#endregion
		#region TestClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts
		public void TestClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts()
		{
			using (new CognosTempTableCreator())
			{
				SetupRawAggregateTableForTestClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts();
				Factory.Save();
				AssertMultilineEquals("Pre-condition", ExpectedCognosRawAggregateTableBeforeSubClassificationAccountCalculation, LoadFromCognosRawAggregateTempTableAsCsvString(), '\n');
				ExecuteNonQuery("EXEC " + JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts);
				string generatedTempTableString = LoadFromCognosRawAggregateTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosRawAggregateTableAfterSubClassificationAccountCalculation, generatedTempTableString, '\n');
			}
		}

		void SetupRawAggregateTableForTestClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts()
		{
			CognosAccGLAccountDescriptor bSHAccount = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "305", Core.Constants.DebitCredit.Credit, null);
			CognosAccGLAccountDescriptor subClassificationAccount1 = CreateAgeSubClassificationAccount("306", Core.Constants.DebitCredit.Debit, bSHAccount, "60");
			CognosAccGLAccountDescriptor subClassificationAccount2 = CreateDebtorCreditorSubClassificationAccount("307", Core.Constants.DebitCredit.Credit, bSHAccount, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor, "TPY", "ASC");
			CognosAccGLAccountDescriptor subClassificationAccount3 = CreateDebtorCreditorSubClassificationAccount("308", Core.Constants.DebitCredit.Debit, bSHAccount, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor, "INT");
			CognosAccGLAccountDescriptor subClassificationAccount4 = CreateDebtorCreditorSubClassificationAccount("309", Core.Constants.DebitCredit.Credit, subClassificationAccount1, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor, "TPY", "ASC");
			CognosAccGLAccountDescriptor subClassificationAccount5 = CreateDebtorCreditorSubClassificationAccount("310", Core.Constants.DebitCredit.Debit, subClassificationAccount1, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor, "ASC");
			CognosAccGLAccountDescriptor subClassificationAccount6 = CreateDebtorCreditorSubClassificationAccount("311", Core.Constants.DebitCredit.Credit, subClassificationAccount2, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByDebtor, "ASC");
			CognosAccGLAccountDescriptor subClassificationAccount7 = CreateAgeSubClassificationAccount("312", Core.Constants.DebitCredit.Credit, subClassificationAccount2, "30");
			CognosAccGLAccountDescriptor subClassificationAccount8 = CreateDebtorCreditorSubClassificationAccount("313", Core.Constants.DebitCredit.Debit, subClassificationAccount3, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor, "INT");
			CognosAccGLAccountDescriptor subClassificationAccount9 = CreateAgeSubClassificationAccount("314", Core.Constants.DebitCredit.Debit, subClassificationAccount3, "90");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "30", AUCOROrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "60", AUCOROrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "90", AUCOROrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "30", IDJKTOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "60", IDJKTOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "90", IDJKTOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "30", ZACPTOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "60", ZACPTOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "90", ZACPTOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "30", DEFRAOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "60", DEFRAOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "90", DEFRAOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "30", INBOMOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "60", INBOMOrg, "", "", 10m, "", 10m, "");
			InsertRecordIntoCognosRawAggregateTable(bSHAccount, "90", INBOMOrg, "", "", 10m, "", 10m, "");
		}

		CognosAccGLAccountDescriptor CreateDebtorCreditorSubClassificationAccount(ZString localAccountNumber, ZString debitCredit, CognosAccGLAccountDescriptor accountToBeSubclassified, ZString subClassificationCode, params ZString[] debtorCreditorCodes)
		{
			CognosAccGLAccountDescriptor result = CreateSubClassificationAccount(localAccountNumber, debitCredit, accountToBeSubclassified, subClassificationCode);
			if (subClassificationCode == CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByCreditor)
			{
				foreach (ZString code in debtorCreditorCodes)
				{
					result.ExtraInfo.MappedCreditorGroups.Add(Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, code));
				}
			}
			else
			{
				foreach (ZString code in debtorCreditorCodes)
				{
					result.ExtraInfo.MappedDebtorGroups.Add(Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, code));
				}
			}

			return result;
		}

		CognosAccGLAccountDescriptor CreateAgeSubClassificationAccount(ZString localAccountNumber, ZString debitCredit, CognosAccGLAccountDescriptor accountToBeSubclassified, ZString ageCode)
		{
			CognosAccGLAccountDescriptor result = CreateSubClassificationAccount(localAccountNumber, debitCredit, accountToBeSubclassified, CognosAccGLAccountDescriptorExtraInfo.SubClassificationCodes.SubClassifiedByAge);
			result.ExtraInfo.T9_AccountAge = ageCode;
			return result;
		}

		CognosAccGLAccountDescriptor CreateSubClassificationAccount(ZString localAccountNumber, ZString debitCredit, CognosAccGLAccountDescriptor accountToBeSubclassified, ZString subClassificationCode)
		{
			CognosAccGLAccountDescriptor result = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, CognosAccGLAccountDescriptor.CognosSubClassificationAccountType, localAccountNumber, debitCredit, null);
			result.ExtraInfo.T9_AJ_AccountToBeSubClassified = accountToBeSubclassified.PK;
			result.ExtraInfo.T9_SubClassificationCode = subClassificationCode;
			return result;
		}

		readonly string ExpectedCognosRawAggregateTableBeforeSubClassificationAccountCalculation = @"
305,30,AUCOR,INT,INT,,,,10,,10,
305,30,DEFRA,,TPY,,,,10,,10,
305,30,IDJKT,TPY,ASC,,,,10,,10,
305,30,INBOM,INT,ASC,,,,10,,10,
305,30,ZACPT,TPY,,,,,10,,10,
305,60,AUCOR,INT,INT,,,,10,,10,
305,60,DEFRA,,TPY,,,,10,,10,
305,60,IDJKT,TPY,ASC,,,,10,,10,
305,60,INBOM,INT,ASC,,,,10,,10,
305,60,ZACPT,TPY,,,,,10,,10,
305,90,AUCOR,INT,INT,,,,10,,10,
305,90,DEFRA,,TPY,,,,10,,10,
305,90,IDJKT,TPY,ASC,,,,10,,10,
305,90,INBOM,INT,ASC,,,,10,,10,
305,90,ZACPT,TPY,,,,,10,,10,".TrimStart();

		readonly string ExpectedCognosRawAggregateTableAfterSubClassificationAccountCalculation = @"
305,30,AUCOR,INT,INT,,,,10,,10,
305,30,DEFRA,,TPY,,,,10,,10,
305,30,IDJKT,TPY,ASC,,,,10,,10,
305,30,INBOM,INT,ASC,,,,10,,10,
305,30,ZACPT,TPY,,,,,10,,10,
305,60,AUCOR,INT,INT,,,,10,,10,
305,60,DEFRA,,TPY,,,,10,,10,
305,60,IDJKT,TPY,ASC,,,,10,,10,
305,60,INBOM,INT,ASC,,,,10,,10,
305,60,ZACPT,TPY,,,,,10,,10,
305,90,AUCOR,INT,INT,,,,10,,10,
305,90,DEFRA,,TPY,,,,10,,10,
305,90,IDJKT,TPY,ASC,,,,10,,10,
305,90,INBOM,INT,ASC,,,,10,,10,
305,90,ZACPT,TPY,,,,,10,,10,
306,60,AUCOR,INT,INT,,,,-10,,-10,
306,60,DEFRA,,TPY,,,,-10,,-10,
306,60,IDJKT,TPY,ASC,,,,-10,,-10,
306,60,INBOM,INT,ASC,,,,-10,,-10,
306,60,ZACPT,TPY,,,,,-10,,-10,
307,30,IDJKT,TPY,ASC,,,,10,,10,
307,30,ZACPT,TPY,,,,,10,,10,
307,60,IDJKT,TPY,ASC,,,,10,,10,
307,60,ZACPT,TPY,,,,,10,,10,
307,90,IDJKT,TPY,ASC,,,,10,,10,
307,90,ZACPT,TPY,,,,,10,,10,
308,30,AUCOR,INT,INT,,,,-10,,-10,
308,60,AUCOR,INT,INT,,,,-10,,-10,
308,90,AUCOR,INT,INT,,,,-10,,-10,
309,60,DEFRA,,TPY,,,,10,,10,
309,60,IDJKT,TPY,ASC,,,,10,,10,
309,60,INBOM,INT,ASC,,,,10,,10,
311,30,IDJKT,TPY,ASC,,,,10,,10,
311,60,IDJKT,TPY,ASC,,,,10,,10,
311,90,IDJKT,TPY,ASC,,,,10,,10,
312,30,IDJKT,TPY,ASC,,,,10,,10,
312,30,ZACPT,TPY,,,,,10,,10,
313,30,AUCOR,INT,INT,,,,-10,,-10,
313,60,AUCOR,INT,INT,,,,-10,,-10,
313,90,AUCOR,INT,INT,,,,-10,,-10,
314,90,AUCOR,INT,INT,,,,-10,,-10,".TrimStart();

		#endregion
		#region TestClientInsertCognosAccountsIntoCognosExportTempTable
		public void TestClientInsertCognosAccountsIntoCognosExportTempTable()
		{
			using (new CognosTempTableCreator())
			{
				SetupRawAggregateTableForTestClientInsertCognosAccountsIntoCognosExportTempTable();
				Factory.Save();
				ExecuteNonQuery("EXEC " + JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertCognosAccountsIntoCognosExportTempTable);
				string generatedTempTableString = LoadFromCognosExportTempTableAsCsvString();
				AssertMultilineEquals("Check the query first before modifying the test data", ExpectedCognosExportTempTableAsCsvString, generatedTempTableString, '\n');
			}
		}

		void SetupRawAggregateTableForTestClientInsertCognosAccountsIntoCognosExportTempTable()
		{
			CognosAccGLAccountDescriptor account401 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("401", false, 0, 1, 2, 3, ClientCognosGroupingFlagsLookups.IntercompanyCodes.IncludeWithCurrencyAndAmount);
			CognosAccGLAccountDescriptor account402 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("402", true, 0, 1, 2, 3, ClientCognosGroupingFlagsLookups.IntercompanyCodes.IncludeWithCurrencyAndAmount);
			CognosAccGLAccountDescriptor account403 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("403", true, 0, 0, 1, 0, ClientCognosGroupingFlagsLookups.IntercompanyCodes.Exclude);
			CognosAccGLAccountDescriptor account404 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("404", true, 1, 2, 3, 4, ClientCognosGroupingFlagsLookups.IntercompanyCodes.ICTOTA);
			CognosAccGLAccountDescriptor account405 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("405", true, 0, 0, 0, 1, ClientCognosGroupingFlagsLookups.IntercompanyCodes.Include);
			CognosAccGLAccountDescriptor account406 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("406", true, 1, 0, 0, 0, ClientCognosGroupingFlagsLookups.IntercompanyCodes.Include);
			CognosAccGLAccountDescriptor account407 = CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable("407", true, 4, 2, 3, 1, ClientCognosGroupingFlagsLookups.IntercompanyCodes.IncludeWithCurrencyAndAmount);
			CognosAccGLAccountDescriptor account408 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "408");
			CognosAccGLAccountDescriptor account409 = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, "409");
			InsertRecordIntoCognosRawAggregateTable(account401, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account402, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account403, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account404, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account405, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account406, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account407, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account408, "", AUCOROrg, "ME", "SYD", 200m, "AUD", 200m, "ANZ");
			InsertRecordIntoCognosRawAggregateTable(account409, "", null, null, null, 200m, null, 200m, null);
		}

		string LoadFromCognosExportTempTableAsCsvString()
		{
			return ConvertCollectionToCsvString(LoadFromCognosExportTempTable());
		}

		CognosAccGLAccountDescriptor CreateCognosAccountForTestClientInsertCognosAccountsIntoCognosExportTempTable(ZString localAccountNumber, bool isPublished, ZByte branchGrouping, ZByte modeGrouping, ZByte businessTypeGrouping, ZByte geographicalGrouping, ZString companyGrouping)
		{
			CognosAccGLAccountDescriptor result = CreateCognosAccount(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, Core.Constants.AccountType.BalanceSheetAccount, localAccountNumber, Core.Constants.DebitCredit.Credit, null);
			result.ExtraInfo.T9_IsPublished = isPublished;
			result.GroupingFlags.T4_Branch = branchGrouping;
			result.GroupingFlags.T4_Mode = modeGrouping;
			result.GroupingFlags.T4_BusinessType = businessTypeGrouping;
			result.GroupingFlags.T4_Geographical = geographicalGrouping;
			result.GroupingFlags.T4_Company = companyGrouping;
			return result;
		}

		DynamicBusinessObjectCollection LoadFromCognosExportTempTable()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(@"
SELECT      AJ_LocalAccountNumber, T6_CompanyCode, T6_Mode, T6_Branch, T6_BusinessType, T6_Amount, T6_TransactionCurrency, T6_TransactionAmount, T6_Geographical
FROM        #CognosExport
            INNER JOIN dbo.AccGLAccountDescriptor ON T6_AJ = AJ_PK            
ORDER BY    AJ_LocalAccountNumber, T6_CompanyCode, T6_Mode, T6_Branch, T6_BusinessType, T6_TransactionCurrency, T6_Geographical, T6_Amount, T6_TransactionAmount");
			return collection;
		}

		readonly string ExpectedCognosExportTempTableAsCsvString = @"
402,AUCOR,ME,,,200,AUD,200,ANZ
403,,,,,200,,0,
404,ICTOTA,ME,SYD,,200,,0,ANZ
405,AUCOR,,,,200,,0,ANZ
406,AUCOR,,SYD,,200,,0,
407,AUCOR,ME,SYD,,200,AUD,200,ANZ
408,,,,,200,,0,
409,,,,,200,,0,".TrimStart();

		#endregion
		#region Implementation
		object ExecuteScalar(string sQL)
		{
			DbCommand cmd = GetDbCommand(sQL);
			return cmd.ExecuteScalar();
		}

		void ExecuteNonQuery(string sQL)
		{
			DbCommand cmd = GetDbCommand(sQL);
			cmd.ExecuteNonQuery();
		}

		bool ExistsDbObject(string objectName)
		{
			DbCommand cmd = Db.Connection.Command("select name FROM sys.objects where name='" + objectName + "'");
			object result = cmd.ExecuteScalar();
			return (result != null);
		}

		void ExecuteNonQueryForCognosRawAggregateTests(string procedureName)
		{
			string sQL = string.Format("EXEC {0} @CompanyPK, @ExportStartDate", procedureName);
			DbCommand cmd = GetDbCommand(sQL);
			cmd.AddParameter("@ExportStartDate", System.Data.SqlDbType.SmallDateTime, ExportStartDate.ToDateTime());
			cmd.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, Env.CurrentCompany.PK);
			cmd.ExecuteNonQuery();
		}

		DbCommand GetDbCommand(string sQL)
		{
			return Db.Connection.Command(sQL);
		}

		DynamicBusinessObjectCollection LoadFromCognosRawAggregateTempTable()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(@"
SELECT      AJ_LocalAccountNumber, T5_Age, T5_CompanyCode, OG_Code, OJ_Code, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical
FROM        #CognosRawAggregate
            INNER JOIN dbo.AccGLAccountDescriptor ON T5_AJ = AJ_PK
            LEFT OUTER JOIN dbo.OrgCreditorGroup ON T5_OG_CreditorGroup = OG_PK
            LEFT OUTER JOIN dbo.OrgDebtorGroup ON T5_OJ_DebtorGroup = OJ_PK
ORDER BY    AJ_LocalAccountNumber, T5_Age, T5_CompanyCode, OG_Code, OJ_Code, T5_Mode, T5_Branch, T5_BusinessType, T5_TransactionCurrency, T5_Geographical, T5_Amount, T5_TransactionAmount");
			return collection;
		}

		string LoadFromCognosRawAggregateTempTableAsCsvString()
		{
			return ConvertCollectionToCsvString(LoadFromCognosRawAggregateTempTable());
		}

		string ConvertCollectionToCsvString(DynamicBusinessObjectCollection collection)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (DynamicBusinessObject aggregateLine in collection)
			{
				object[] itemArray = ((IBusinessObjectInternals)aggregateLine).Row.ItemArray;
				var line = new OCsvLine(new string[itemArray.Length], new bool[itemArray.Length]);
				for (int i = 0; i < itemArray.Length; i++)
				{
					object fieldValue = itemArray[i];
					if (fieldValue == DBNull.Value)
					{
						line.FieldValues[i] = "";
					}
					else if (fieldValue is decimal)
					{
						line.FieldValues[i] = new ZDecimal(fieldValue).ToString(0);
					}
					else
					{
						line.FieldValues[i] = fieldValue.ToString();
					}
				}

				builder.Append(line.ToString());
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}

		protected override void TearDown()
		{
			if (userContextChange != null)
			{
				userContextChange.Dispose();
			}

			base.TearDown();
		}

		#region Methods to Setup Test Data
		AccTransactionHeader CreateTransactionWithLine(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccGLHeader gLAccount, GlbBranch branch, GlbDepartment department)
		{
			return CreateTransactionWithLine(ledgerType, transactionType, postToGL, currencyCode, postDate, counterCompany, gLAccount, branch, department, false, "");
		}

		AccTransactionHeader CreateTransactionWithLine(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccGLHeader gLAccount, GlbBranch branch, GlbDepartment department, bool hasChargeCode, ZString lineType)
		{
			if (lineType.IsEmpty)
			{
				lineType = TransactionLineTypes.WIP;
			}

			AccTransactionLines line = CreateTransactionLine(lineType, postToGL, false, currencyCode, postDate, postDate, gLAccount, null, branch, department);
			if (hasChargeCode)
			{
				line.AL_LineType = lineType;
				line.AL_AC = ChargeCode.PK;
				if (gLAccount == null && lineType != TransactionLineTypes.WIP && lineType != TransactionLineTypes.Accrual)
				{
					line.AL_AG = ZGuid.Empty;
				}
			}

			return CreateTransaction(ledgerType, transactionType, postToGL, currencyCode, postDate, counterCompany, gLAccount, branch, department, line);
		}

		AccTransactionHeader CreateTransaction(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccGLHeader gLAccount, GlbBranch branch, GlbDepartment department, AccTransactionLines line)
		{
			AccTransactionHeader result = CreateTransaction(ledgerType, transactionType, postToGL, currencyCode, postDate, counterCompany, gLAccount, branch, department);
			result.AH_AG = ZGuid.Empty;
			line.AL_AH = result.PK;
			return result;
		}

		AccTransactionHeader CreateTransaction(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccBankAccount bankAccount, GlbBranch branch, GlbDepartment department)
		{
			return CreateTransaction(ledgerType, transactionType, postToGL, currencyCode, postDate, counterCompany, null, bankAccount, branch, department);
		}

		AccTransactionHeader CreateTransaction(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccGLHeader gLAccount, GlbBranch branch, GlbDepartment department)
		{
			return CreateTransaction(ledgerType, transactionType, postToGL, currencyCode, postDate, counterCompany, gLAccount, null, branch, department);
		}

		AccTransactionHeader CreateTransaction(ZString ledgerType, ZString transactionType, ZBool postToGL, ZString currencyCode, ZDateTime postDate, JASOrgHeader counterCompany, AccGLHeader gLAccount, AccBankAccount bankAccount, GlbBranch branch, GlbDepartment department)
		{
			AccTransactionHeader result = Factory.NewWithValidTestData<AccTransactionHeader>();
			result.AH_Ledger = ledgerType;
			result.AH_TransactionType = transactionType;
			result.AH_PostToGL = postToGL.ToString();
			if (!currencyCode.IsEmpty)
			{
				result.AH_RX_NKTransactionCurrency = currencyCode;
			}

			result.AH_PostDate = postDate;
			if (counterCompany != null)
			{
				result.AH_OH = counterCompany.PK;
			}

			if (gLAccount != null)
			{
				result.AH_AG = gLAccount.PK;
			}

			if (bankAccount != null)
			{
				result.AH_AB = bankAccount.PK;
			}

			result.AH_ExchangeRate = (GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == currencyCode) ? 1m : 0.5m;
			result.AH_InvoiceAmount = 100m;
			result.AH_GSTAmount = 10m;
			result.AH_OutstandingAmount = 110M;
			result.AH_GB = branch.PK;
			result.AH_GE = department.PK;
			result.AH_OSTotal = (result.AH_InvoiceAmount + result.AH_GSTAmount) * result.AH_ExchangeRate;
			bool miscLedgerAndType = (ledgerType == LedgerTypes.AccountsReceivable || ledgerType == LedgerTypes.AccountsPayable) && (transactionType == TransactionTypes.ExchangeDifference || transactionType == TransactionTypes.Discount || transactionType == TransactionTypes.Overpayment);
			if (miscLedgerAndType) // Need to create a Journal to match it because we cannot save miscellaneous transaction with a non zero outstanding amount
			{
				AccTransactionHeader journalToMatch = Factory.New<AccTransactionHeader>();
				journalToMatch.AH_OH = result.AH_OH;
				journalToMatch.AH_AG = result.AH_AG;
				journalToMatch.AH_AB = result.AH_AB;
				journalToMatch.AH_InvoiceDate = DateTime.Now;
				journalToMatch.AH_Ledger = ledgerType;
				journalToMatch.AH_TransactionType = TransactionTypes.Journal;
				journalToMatch.AH_RX_NKTransactionCurrency = result.AH_RX_NKTransactionCurrency;
				journalToMatch.AH_ExchangeRate = result.AH_ExchangeRate;
				journalToMatch.AH_InvoiceAmount = -100m;
				journalToMatch.AH_GSTAmount = -10m;
				journalToMatch.AH_OutstandingAmount = -110m;
				journalToMatch.AH_PostDate = postDate;
				journalToMatch.AH_GB = branch.PK;
				journalToMatch.AH_GE = department.PK;
				journalToMatch.AH_TransactionNum = (TransactionTypes.Journal + result.AH_TransactionNum).Substring(1, journalToMatch.AH_TransactionNumInfo.MaxLength);
				journalToMatch.AH_PostToGL = "N";
				AccTransactionMatchLink link1 = Factory.New<AccTransactionMatchLink>();
				AccTransactionMatchLink link2 = Factory.New<AccTransactionMatchLink>();
				link1.AP_AH = result.PK;
				link1.AP_Amount = result.AH_OutstandingAmount;
				link2.AP_AH = journalToMatch.PK;
				link2.AP_Amount = journalToMatch.AH_OutstandingAmount;
				link1.AP_MatchDate = link2.AP_MatchDate = DateTime.Now;
				link1.AP_MatchGroupNum = link1.AP_MatchGroupNum = ("M" + result.AH_TransactionNum).Substring(1, link1.AP_MatchGroupNumInfo.MaxLength);
				TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
				matchlinks.Add(link1);
				matchlinks.Add(link2);
				result.AH_OutstandingAmount = 0m;
				journalToMatch.AH_OutstandingAmount = 0m;
			}

			return result;
		}

		AccTransactionLines CreateTransactionLine(ZString lineType, ZBool postToGL, ZBool reverseToGL, ZString currencyCode, ZDateTime postDate, ZDateTime reverseDate, AccGLHeader gLAccount, AccChargeCode chargeCode, GlbBranch branch, GlbDepartment department)
		{
			AccTransactionLines result = Factory.NewWithValidTestData<AccTransactionLines>();
			ZString ledger = lineType == TransactionLineTypes.Cost ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
			result.AL_AH = CreateTransaction(ledger, TransactionTypes.Invoice, postToGL, currencyCode, postDate, AUCOROrg, BSHAccount1, branch, department).PK;
			result.AL_LineType = lineType;
			result.AL_PostToGL = postToGL.ToString();
			result.AL_ReverseToGL = reverseToGL.ToString();
			if (!currencyCode.IsEmpty)
			{
				result.AL_RX_NKTransactionCurrency = currencyCode;
			}

			result.AL_ExchangeRate = (GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == currencyCode) ? 1m : 0.5m;
			result.AL_PostDate = postDate;
			result.AL_ReverseDate = reverseDate;
			if (gLAccount != null)
			{
				result.AL_AG = gLAccount.PK;
			}

			if (chargeCode != null)
			{
				result.AL_AC = chargeCode.PK;
			}

			result.AL_LineAmount = 30m;
			result.AL_GSTVAT = 10m;
			result.AL_OSAmount = (result.AL_LineAmount + result.AL_GSTVAT) * result.AL_ExchangeRate;
			result.AL_GB = branch.PK;
			result.AL_GE = department.PK;
			return result;
		}

		CognosAccGLAccountDescriptor CreateCognosAccount(ZString language, ZString accountType, ZString localAccountNumber)
		{
			return CreateCognosAccount(language, accountType, localAccountNumber, null);
		}

		CognosAccGLAccountDescriptor CreateCognosAccount(ZString language, ZString accountType, ZString localAccountNumber, AccGLHeader gLAccount)
		{
			return CreateCognosAccount(language, accountType, localAccountNumber, Core.Constants.DebitCredit.Debit, gLAccount);
		}

		CognosAccGLAccountDescriptor CreateCognosAccount(ZString language, ZString accountType, ZString localAccountNumber, ZString debitCredit, AccGLHeader gLAccount)
		{
			CognosAccGLAccountDescriptor result = Factory.NewWithValidTestData<CognosAccGLAccountDescriptor>();
			result.AJ_Language = language;
			result.AJ_ReportCategory = accountType;
			result.AJ_ReportType = "COA";
			result.AJ_LocalAccountNumber = localAccountNumber;
			result.AJ_DebitCredit = debitCredit;
			if (gLAccount != null)
			{
				result.ParentGLHeaderPK = gLAccount.PK;
			}

			return result;
		}

		void InsertRecordIntoCognosRawAggregateTable(CognosAccGLAccountDescriptor account, ZString age, JASOrgHeader counterCompany, ZString mode, ZString branch, decimal amount, ZString currencyCode, decimal transactionAmount, ZString geographical)
		{
			string sqlText = @"
INSERT INTO #CognosRawAggregate
VALUES (@T5_AJ, @T5_Age, @T5_CompanyCode, @T5_OG_CreditorGroup, @T5_OJ_DebtorGroup, @T5_Mode, @T5_Branch, @T5_BusinessType, @T5_Amount, @T5_TransactionCurrency, @T5_TransactionAmount, @T5_Geographical)";
			DbCommand command = GetDbCommand(sqlText);
			command.AddParameter("@T5_AJ", SqlDbType.UniqueIdentifier, account.PK.ToGuid());
			command.AddParameter("@T5_Age", SqlDbType.VarChar, 2, age.ToString());
			if (counterCompany != null)
			{
				command.AddParameter("@T5_CompanyCode", SqlDbType.NVarChar, 8, counterCompany.NettingCode.ToString());
				if (counterCompany.CompanyData.OB_OG_APCreditorGroup.IsEmpty)
				{
					command.AddParameter("@T5_OG_CreditorGroup", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@T5_OG_CreditorGroup", SqlDbType.UniqueIdentifier, counterCompany.CompanyData.OB_OG_APCreditorGroup.ToGuid());
				}

				if (counterCompany.CompanyData.OB_OJ_ARDebtorGroup.IsEmpty)
				{
					command.AddParameter("@T5_OJ_DebtorGroup", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@T5_OJ_DebtorGroup", SqlDbType.UniqueIdentifier, counterCompany.CompanyData.OB_OJ_ARDebtorGroup.ToGuid());
				}

				command.AddParameter("@T5_BusinessType", SqlDbType.NVarChar, 3, DBNull.Value);
			}
			else
			{
				command.AddParameter("@T5_CompanyCode", SqlDbType.NVarChar, 8, DBNull.Value);
				command.AddParameter("@T5_OG_CreditorGroup", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@T5_OJ_DebtorGroup", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@T5_BusinessType", SqlDbType.NVarChar, 3, DBNull.Value);
			}

			command.AddParameter("@T5_Mode", SqlDbType.NVarChar, 4, (mode.IsEmpty ? DBNull.Value : mode.ToString()));
			command.AddParameter("@T5_Branch", SqlDbType.NVarChar, 3, (branch.IsEmpty ? DBNull.Value : branch.ToString()));
			command.AddParameter("@T5_Amount", SqlDbType.Money, amount);
			command.AddParameter("@T5_TransactionCurrency", SqlDbType.VarChar, 3, (currencyCode.IsEmpty ? DBNull.Value : currencyCode.ToString()));
			command.AddParameter("@T5_TransactionAmount", SqlDbType.Money, transactionAmount);
			command.AddParameter("@T5_Geographical", SqlDbType.NVarChar, 3, (geographical.IsEmpty ? DBNull.Value : geographical.ToString()));
			command.ExecuteNonQuery();
		}

		#endregion
		#region Setup for CognosRawAggregateTests
		void SetupMappingsForCognosRawAggregateTests()
		{
			SetupNewCompanyAndActiveBranches();
			SetupModeMappingsForCognosRawAggregateTests();
			SetupAccountingPeriodsForCognosRawAggregateTests();
			Factory.Save();
		}

		void SetupNewCompanyAndActiveBranches()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "JAS";
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			MILBranch = company.Branches.AddNew();
			MILBranch.FillWithValidTestData();
			MILBranch.GB_Code = "MIL";
			MILBranch.GB_RL_NKHomePort = "ITMIL";
			VALBranch = company.Branches.AddNew();
			VALBranch.FillWithValidTestData();
			VALBranch.GB_Code = "VAL";
			VALBranch.GB_RL_NKHomePort = "ITVAL";
			ROMBranch = company.Branches.AddNew();
			ROMBranch.FillWithValidTestData();
			ROMBranch.GB_Code = "ROM";
			ROMBranch.GB_RL_NKHomePort = "ITROM";
			GlbCompany otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "OIU";
			OTHBranch = otherCompany.Branches.AddNew();
			OTHBranch.FillWithValidTestData();
			OTHBranch.GB_Code = "999";
			OTHBranch.GB_RL_NKHomePort = "USNYC";
			Factory.Save();
			userContextChange = Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, MILBranch.PK.ToGuid(), Env.CurrentDepartment.PK);
		}

		void SetupModeMappingsForCognosRawAggregateTests()
		{
			CognosModeMapping modeMapping = JASDataRegistry.Instance.CognosModeMapping;
			modeMapping.SelectedMode = nameof(CognosModes.AI);
			modeMapping.MapDepartments(AIDepartment);
			modeMapping.SelectedMode = nameof(CognosModes.AE);
			modeMapping.MapDepartments(AEDepartment);
			modeMapping.SelectedMode = nameof(CognosModes.MI);
			modeMapping.MapDepartments(MIDepartment);
			modeMapping.SelectedMode = nameof(CognosModes.ME);
			modeMapping.MapDepartments(MEDepartment);
			JASDataRegistry.Instance.CognosModeMappingItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, modeMapping);
		}

		void SetupAccountingPeriodsForCognosRawAggregateTests()
		{
			NewYearPeriodSettings year2005PeriodSettings = new NewYearPeriodSettings();
			year2005PeriodSettings.StartDate = new ZDateTime(2005, 1, 1);
			NewYearPeriodSettings year2006PeriodSettings = new NewYearPeriodSettings();
			year2006PeriodSettings.StartDate = new ZDateTime(2006, 1, 1);
			PeriodManager periodManager = new PeriodManager(Factory);
			periodManager.CreatePeriodData(year2005PeriodSettings, Factory);
			periodManager.CreatePeriodData(year2006PeriodSettings, Factory);
		}

		IDisposable userContextChange;
		GlbBranch MILBranch;
		GlbBranch VALBranch;
		GlbBranch ROMBranch;
		GlbBranch OTHBranch;
		#endregion
		#region Companies
		JASOrgHeader AUCOROrg
		{
			get
			{
				if (fAUCOROrg == null)
				{
					fAUCOROrg = CreateNewCompany("AUCOR", "INT", "INT", "", "ANZ");
				}

				return fAUCOROrg;
			}
		}

		JASOrgHeader ARBUEOrg
		{
			get
			{
				if (fARBUEOrg == null)
				{
					fARBUEOrg = CreateNewCompany("ARBUE", "INT", "INT", "", "SAM");
				}

				return fARBUEOrg;
			}
		}

		JASOrgHeader USCOROrg
		{
			get
			{
				if (fUSCOROrg == null)
				{
					fUSCOROrg = CreateNewCompany("USCOR", "INT", "INT", "", "NAM");
				}

				return fUSCOROrg;
			}
		}

		JASOrgHeader IDJKTOrg
		{
			get
			{
				if (fIDJKTOrg == null)
				{
					fIDJKTOrg = CreateNewCompany("IDJKT", "ASC", "TPY", "", "SEA");
				}

				return fIDJKTOrg;
			}
		}

		JASOrgHeader AEDXBOrg
		{
			get
			{
				if (fAEDXBOrg == null)
				{
					fAEDXBOrg = CreateNewCompany("AEDXB", "ASC", "ASC", "", "MEA");
				}

				return fAEDXBOrg;
			}
		}

		JASOrgHeader DEFRAOrg
		{
			get
			{
				if (fDEFRAOrg == null)
				{
					fDEFRAOrg = CreateNewCompany("DEFRA", "TPY", "", "", "EUR");
				}

				return fDEFRAOrg;
			}
		}

		JASOrgHeader ZACPTOrg
		{
			get
			{
				if (fZACPTOrg == null)
				{
					fZACPTOrg = CreateNewCompany("ZACPT", "", "TPY", "", "AFR");
				}

				return fZACPTOrg;
			}
		}

		JASOrgHeader INBOMOrg
		{
			get
			{
				if (fINBOMOrg == null)
				{
					fINBOMOrg = CreateNewCompany("INBOM", "ASC", "INT", "", "OTH");
				}

				return fINBOMOrg;
			}
		}

		JASOrgHeader CreateNewCompany(ZString orgCode, ZString debtorCode, ZString creditorCode, ZString businessType, ZString geographical)
		{
			JASOrgHeader result = Factory.NewWithValidTestData<JASOrgHeader>();
			result.NettingCode = orgCode;
			if (!creditorCode.IsEmpty)
			{
				result.CompanyData.OB_OG_APCreditorGroup = Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, creditorCode).PK;
			}

			if (!debtorCode.IsEmpty)
			{
				result.CompanyData.OB_OJ_ARDebtorGroup = Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, debtorCode).PK;
			}

			result.OH_RL_NKClosestPort = GetUNLOCOFromGeographicalCode(geographical);
			return result;
		}

		ZString GetUNLOCOFromGeographicalCode(ZString geographicalCode)
		{
			ZString result = "";
			switch (geographicalCode)
			{
				case "SAM":
					result = "ARBUE";
					break;
				case "NAM":
					result = "USSEA";
					break;
				case "SEA":
					result = "IDJKT";
					break;
				case "ANZ":
					result = "AUSYD";
					break;
				case "MEA":
					result = "AEDXB";
					break;
				case "EUR":
					result = "DEFRA";
					break;
				case "AFR":
					result = "ZACPT";
					break;
				case "OTH":
					result = "INBOM";
					break;
			}

			return result;
		}

		JASOrgHeader fAUCOROrg;
		JASOrgHeader fARBUEOrg;
		JASOrgHeader fUSCOROrg;
		JASOrgHeader fIDJKTOrg;
		JASOrgHeader fAEDXBOrg;
		JASOrgHeader fDEFRAOrg;
		JASOrgHeader fZACPTOrg;
		JASOrgHeader fINBOMOrg;
		#endregion
		#region Departments
		GlbCompany OtherCompany
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			}
		}

		GlbDepartment AIDepartment
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIA");
			}
		}

		GlbDepartment AEDepartment
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			}
		}

		GlbDepartment MIDepartment
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIS");
			}
		}

		GlbDepartment MEDepartment
		{
			get
			{
				return Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FES");
			}
		}

		#endregion
		#region Dates
		ZDateTime ExportStartDate
		{
			get
			{
				if (fExportStartDate.IsEmpty)
				{
					fExportStartDate = new ZDateTime(2006, 3, 6);
				}

				return fExportStartDate;
			}
		}

		ZDateTime DateAfterExportStartDate
		{
			get
			{
				if (fDateAfterExportStartDate.IsEmpty)
				{
					fDateAfterExportStartDate = new ZDateTime(2006, 3, 7);
				}

				return fDateAfterExportStartDate;
			}
		}

		ZDateTime DateInPnLPeriod
		{
			get
			{
				if (fDateInPnLPeriod.IsEmpty)
				{
					fDateInPnLPeriod = new ZDateTime(2006, 1, 16);
				}

				return fDateInPnLPeriod;
			}
		}

		ZDateTime DateInBSHPeriodButNotPnLPeriod
		{
			get
			{
				if (fDateInBSHPeriodButNotPnLPeriod.IsEmpty)
				{
					fDateInBSHPeriodButNotPnLPeriod = new ZDateTime(2005, 12, 31);
				}

				return fDateInBSHPeriodButNotPnLPeriod;
			}
		}

		ZDateTime DateNotWithinCognosAccountAgingRange
		{
			get
			{
				if (fDateNotWithinCognosAccountAgingRange.IsEmpty)
				{
					fDateNotWithinCognosAccountAgingRange = new ZDateTime(2005, 1, 1);
				}

				return fDateNotWithinCognosAccountAgingRange;
			}
		}

		ZDateTime fExportStartDate;
		ZDateTime fDateAfterExportStartDate;
		ZDateTime fDateInPnLPeriod;
		ZDateTime fDateInBSHPeriodButNotPnLPeriod;
		ZDateTime fDateNotWithinCognosAccountAgingRange;
		#endregion
		#region GL Accounts
		AccGLHeader PnLAccount1
		{
			get
			{
				if (fPnLAccount1 == null)
				{
					fPnLAccount1 = CreateNewGLAccount(Core.Constants.AccountType.ProfitAndLossAccount);
				}

				return fPnLAccount1;
			}
		}

		AccGLHeader PnLAccount2
		{
			get
			{
				if (fPnLAccount2 == null)
				{
					fPnLAccount2 = CreateNewGLAccount(Core.Constants.AccountType.ProfitAndLossAccount);
				}

				return fPnLAccount2;
			}
		}

		AccGLHeader BSHAccount1
		{
			get
			{
				if (fBSHAccount1 == null)
				{
					fBSHAccount1 = CreateNewGLAccount(Core.Constants.AccountType.BalanceSheetAccount);
				}

				return fBSHAccount1;
			}
		}

		AccGLHeader BSHAccount2
		{
			get
			{
				if (fBSHAccount2 == null)
				{
					fBSHAccount2 = CreateNewGLAccount(Core.Constants.AccountType.BalanceSheetAccount);
				}

				return fBSHAccount2;
			}
		}

		AccGLHeader CreateNewGLAccount(ZString accountType)
		{
			AccGLHeader result = Factory.NewWithValidTestData<AccGLHeader>();
			result.AG_AccountType = accountType;
			return result;
		}

		AccGLHeader fPnLAccount1;
		AccGLHeader fPnLAccount2;
		AccGLHeader fBSHAccount1;
		AccGLHeader fBSHAccount2;
		#endregion
		#region Bank Accounts
		AccBankAccount BankAccount1
		{
			get
			{
				if (fBankAccount1 == null)
				{
					fBankAccount1 = CreateBankAccount(BSHAccount1);
				}

				return fBankAccount1;
			}
		}

		AccBankAccount BankAccount2
		{
			get
			{
				if (fBankAccount2 == null)
				{
					fBankAccount2 = CreateBankAccount(BSHAccount2);
				}

				return fBankAccount2;
			}
		}

		AccBankAccount CreateBankAccount(AccGLHeader gLAccount)
		{
			AccBankAccount result = Factory.NewWithValidTestData<AccBankAccount>();
			result.AB_AG = gLAccount.PK;
			return result;
		}

		AccBankAccount fBankAccount1;
		AccBankAccount fBankAccount2;
		#endregion
		#region Charge Code
		AccChargeCode ChargeCode
		{
			get
			{
				if (fChargeCode == null)
				{
					fChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					fChargeCode.AC_AG_AccrualAccount = PnLAccount1.PK;
					fChargeCode.AC_AG_CostAccount = PnLAccount1.PK;
					fChargeCode.AC_AG_WIPAccount = PnLAccount2.PK;
					fChargeCode.AC_AG_RevenueAccount = PnLAccount2.PK;
				}

				return fChargeCode;
			}
		}

		AccChargeCode fChargeCode;
		#endregion
		#endregion
	}
}
