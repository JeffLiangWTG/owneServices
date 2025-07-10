using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class OrgCreditLimitAndBalanceDetailsTest : ScriptTest
	{
		public void TestBalanceCorrectWhenOutstandingAmountSetFromAccountingWebService_AR()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1, 100);
			Factory.Save();
			AssertEquals("AR Invoice amount", 100M, arInvoice.AH_InvoiceAmount);
			AssertEquals("AR Invoice GST amount", 10M, arInvoice.AH_GSTAmount);
			AssertEquals("AR Invoice Outstanding amount", 110M, arInvoice.AH_OutstandingAmount);

			AssertBalanceCorrectWhenOutstandingAmountSetFromAccountingWebService(LedgerTypes.AccountsReceivable, 110M, arInvoice.PK);
		}

		public void TestBalanceCorrectWhenOutstandingAmountSetFromAccountingWebService_AP()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1, 200, 20, 0, 200, 20, 0, TestObjectCreator.AALSHI);
			Factory.Save();
			AssertEquals("AP Invoice amount", -200M, apInvoice.AH_InvoiceAmount);
			AssertEquals("AP Invoice GST amount", -20M, apInvoice.AH_GSTAmount);
			AssertEquals("AP Invoice Outstanding amount", -220M, apInvoice.AH_OutstandingAmount);

			AssertBalanceCorrectWhenOutstandingAmountSetFromAccountingWebService(LedgerTypes.AccountsPayable, -220M, apInvoice.PK);
		}

		public void TestOrgCreditLimitAndBalanceDetailsWithNoArithmeticOverflowError()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.AALSHI);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1, 922337203685477.58m, 0m, TestObjectCreator.GLHeader1.PK);
				Factory.Save();

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1, 1m, 0m, TestObjectCreator.GLHeader1.PK);
				AssertNoExceptionThrown("No Arithmetic Overflow Error", Factory.Save);

				var result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(922337203685478.58m, result.Rows[0]["AccountBalanceTotal"]);
			}
		}

		public void TestOrgCreditLimitAndBalanceDetailsWithExcludeOpenClaimsAmounts_AR()
		{
			var orgContact = TestObjectCreator.AALSHI.Contacts.AddNew();
			orgContact.OC_ContactName = "Fred";
			TestObjectCreator.AALSHI.CompanyData.OB_ARCreditLimit = 41m;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1m, 50m, 0m, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateClaim(typeof(ARAccQueryClaim), 10m, arInvoice.PK, TestObjectCreator.AALSHI.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			Factory.Save();
			var result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(0m, result.Rows[0]["ClaimTotal"]);
			AssertEquals("Y", result.Rows[0]["IsOverCreditLimit"]);

			using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(10m, result.Rows[0]["ClaimTotal"]);
				AssertEquals("N", result.Rows[0]["IsOverCreditLimit"]);
			}
		}

		public void TestOrgCreditLimitAndBalanceDetailsWithExcludeOpenClaimsAmounts_AP()
		{
			var orgContact = TestObjectCreator.AALSHI.Contacts.AddNew();
			orgContact.OC_ContactName = "Fred";
			TestObjectCreator.AALSHI.CompanyData.OB_APCreditLimit = 41m;

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1m, 50m, 0m, 0m, 50m, 0m, 0m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateClaim(typeof(APAccQueryClaim), 10m, apInvoice.PK, TestObjectCreator.AALSHI.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			Factory.Save();
			var result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AP", 30, false, "BAL");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(0m, result.Rows[0]["ClaimTotal"]);
			AssertEquals("Y", result.Rows[0]["IsOverCreditLimit"]);

			using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AP", 30, false, "BAL");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(10m, result.Rows[0]["ClaimTotal"]);
				AssertEquals("N", result.Rows[0]["IsOverCreditLimit"]);
			}
		}

		public void TestOrgCreditLimitAndBalanceDetailsWithGlobalExcludeOpenClaimsAmounts()
		{
			var orgContact = TestObjectCreator.AALSHI.Contacts.AddNew();
			orgContact.OC_ContactName = "Fred";

			var orgCompanyData = TestObjectCreator.AALSHI.CompanyData;
			orgCompanyData.OB_IsDebtor = true;

			var miscServ = TestObjectCreator.AALSHI.MiscServ;
			miscServ.OM_GC_CMPreferredPaymentCompany = GlbCompany.CurrentCompany.PK;
			miscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";
			miscServ.OM_ARGlobalCreditApproved = true;
			miscServ.OM_ARGlobalCreditLimit = 130m;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1m, 50m, 0m, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateClaim(typeof(ARAccQueryClaim), 10m, arInvoice.PK, TestObjectCreator.AALSHI.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			Factory.Save();

			var nonCurrentCompanyBranch = TestObjectCreator.NonCurrentCompanyBranch;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, nonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				nonCurrentCompanyBranch.Company.GC_RX_NKLocalCurrency = "CNY";
				var orgCompanyData2 = TestObjectCreator.AALSHI.CompanyData;
				orgCompanyData2.OB_IsDebtor = true;

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "GCB", 2m);

				var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", TestObjectCreator.CNY, 1m, 50m, 0, 50m, 0m);
				arInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateClaim(typeof(ARAccQueryClaim), 10m, arInvoice2.PK, TestObjectCreator.AALSHI.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
				Factory.Save();
			}

			var result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "", 30, false, "BAL");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(0m, result.Rows[0]["GlobalClaim"]);
			Assert(result.Rows[0].Field<bool>("IsOverGlobalCreditLimit"));

			using (AccountingConfigurationRegistry.Instance.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "", 30, false, "BAL");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(30m, result.Rows[0]["GlobalClaim"]);
				Assert(!result.Rows[0].Field<bool>("IsOverGlobalCreditLimit"));
			}
		}

		public void TestOrgCreditLimitAndBalanceDetailsWithExcludeOpenClaimsAmountsForSystemLevelRegistry()
		{
			var orgContact = TestObjectCreator.AALSHI.Contacts.AddNew();
			orgContact.OC_ContactName = "Fred";
			TestObjectCreator.AALSHI.CompanyData.OB_ARCreditLimit = 41m;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1m, 50m, 0m, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateClaim(typeof(ARAccQueryClaim), 10m, arInvoice.PK, TestObjectCreator.AALSHI.PK, orgContact.PK, QueryClaimStatusCodeList.Codes.QCStatus1Open);
			Factory.Save();

			var result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
			AssertEquals(1, result.Rows.Count);
			AssertEquals(0m, result.Rows[0]["ClaimTotal"]);
			AssertEquals("Y", result.Rows[0]["IsOverCreditLimit"]);

			using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(10m, result.Rows[0]["ClaimTotal"]);
				AssertEquals("N", result.Rows[0]["IsOverCreditLimit"]);
			}

			using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(0m, result.Rows[0]["ClaimTotal"]);
				AssertEquals("Y", result.Rows[0]["IsOverCreditLimit"]);

				using (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					result = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, "AR", 30, false, "BAL");
					AssertEquals(1, result.Rows.Count);
					AssertEquals(10m, result.Rows[0]["ClaimTotal"]);
					AssertEquals("N", result.Rows[0]["IsOverCreditLimit"]);
				}
			}
		}

		#region Implementation

		/// <summary>
		/// The Accounting web service has a method that allows an external system to update
		/// in our system the outstanding amount a client owes. It does this by updating AH_OutstandingAmount
		/// but there is no corresponding receipt transaction. This test checks that the procedure doesn't
		/// get confused by this.
		/// </summary>
		void AssertBalanceCorrectWhenOutstandingAmountSetFromAccountingWebService(string ledgerToTest, decimal totalAmount, ZGuid invoiceToUpdatePK)
		{
			var ledgerOptions = new[] { String.Empty, ledgerToTest };

			var testOptions =
				from ledgerOption in ledgerOptions
				from balanceOverdueAgingOptionOption in BalanceOverdueAgingOptionOptions
				select new Tuple<string, string>(ledgerOption, balanceOverdueAgingOptionOption);

			Func<string, Tuple<string, string>, string> scenarioDescription = (testDescription, testOption) =>
				String.Format("{0} (Ledger option={1}, BalanceOverdueAgingOption={2})",
				testDescription, testOption.Item1, testOption.Item2);

			foreach (var testOption in testOptions)
			{
				var data = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, testOption.Item1, 30, false, testOption.Item2);
				AssertEquals(scenarioDescription("Just one row", testOption), 1, data.Rows.Count);
				var expectedTotalAmount = testOption.Item1 == LedgerTypes.AccountsPayable ? -totalAmount : totalAmount;
				AssertEquals(scenarioDescription("Outstanding amount returned correctly", testOption), expectedTotalAmount, data.Rows[0]["AccountBalanceTotal"]);
			}

			var newOutstandingAmount = totalAmount < 0 ? -50M : 50M;
			SimulateUpdateInvoicePaymentDetailsService(invoiceToUpdatePK, newOutstandingAmount, totalAmount, totalAmount);

			foreach (var testOption in testOptions)
			{
				var data = RunScript(TestObjectCreator.AALSHI.OH_Code, GlbCompany.CurrentCompany.GC_Code, testOption.Item1, 30, false, testOption.Item2);
				AssertEquals(scenarioDescription("Just one row", testOption), 1, data.Rows.Count);
				var expectednewOutstandingAmount = testOption.Item1 == LedgerTypes.AccountsPayable ? -newOutstandingAmount : newOutstandingAmount;
				AssertEquals(scenarioDescription("Outstanding amount returned correctly", testOption), expectednewOutstandingAmount, data.Rows[0]["AccountBalanceTotal"]);
			}
		}

		static string[] BalanceOverdueAgingOptionOptions
		{
			get
			{
				var query =
					from x in typeof(Constants.BalanceOverdueAgingOption).GetFields()
					where x.IsLiteral
					select (string)x.GetValue(null);

				var result = query.ToArray();

				AssertEquals("There are 4 options", 4, result.Length);
				Assert("There is AGE", result.Contains("AGE"));
				Assert("There is BAL", result.Contains("BAL"));
				Assert("There is OVR", result.Contains("OVR"));
				Assert("There is REV", result.Contains("REV"));
				return result.ToArray();
			}
		}

		static void SimulateUpdateInvoicePaymentDetailsService(ZGuid invoiceToUpdatePK, decimal newOutstandingAmount, decimal oldOutstandingAmount, decimal totalAmount)
		{
			string sql = @"UPDATE dbo.AccTransactionHeader 
SET AH_OutstandingAmount = @NewOutstandingAmount, AH_FullyPaidDate = @Date, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST'
WHERE AH_PK = @PK AND AH_OutstandingAmount = @OldOutstandingAmount AND (AH_InvoiceAmount + AH_GSTAmount) = @TotalAmount
IF @@ROWCOUNT = 0 RAISERROR('ConcurrencyError', 16, 1)";
			var command = Db.Connection.Command(sql);
			command.AddParameter("@NewOutstandingAmount", SqlDbType.Decimal, newOutstandingAmount);
			command.AddParameter("@PK", SqlDbType.UniqueIdentifier, invoiceToUpdatePK.ToGuid());
			command.AddParameter("@Date", SqlDbType.DateTime, DBNull.Value);
			command.AddParameter("@OldOutstandingAmount", SqlDbType.Decimal, oldOutstandingAmount);
			command.AddParameter("@TotalAmount", SqlDbType.Decimal, totalAmount);
			command.ExecuteNonQuery();
		}

		DataTable RunScript(
			string orgCode,
			string companyCode,
			string accLedger,
			int agingPeriod,
			bool useSettlementGroup,
			string balanceOverdueAgingOption)
		{
			string sql = string.Format(@"Exec OrgCreditLimitAndBalanceDetails 
			'{0}', --@OrgCode
			'{1}', --@CompanyCode
			'{2}', --@AccLedger
			 {3},  --@AgingPeriod
			'{4}', --@UseSettlementGroup
			'{5}'  --@BalanceOverdueAgingOption
			",
			 orgCode,
			 companyCode,
			 accLedger,
			 agingPeriod,
			 useSettlementGroup ? "Y" : "",
			 balanceOverdueAgingOption);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		#endregion
	}
}

