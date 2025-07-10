using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_OutstandingARAPTransactionsListingTest : ScriptTest
	{
		#region New OS Outstanding Amount Feature

		[TestDate(2022, 10, 1)]
		public void TestNewOSOutstandingAmountFeature()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeature(true);
				AssertForNewOSOutstandingAmountFeature(false);
			}

			//Note:
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// OutstandingAmount = AH_OSOutstandingAmount
			//					 = 12000000.00m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// OutstandingAmount = AH_OutstandingAmount * AH_ExchangeRate
			//					 = 857.14 * 14000.00
			//					 = 11999960.00m

			void AssertForNewOSOutstandingAmountFeature(bool enableNewOSOutstandingAmountFeature)
			{
				AssertEquals("Pre-Condition", "00001000", arInvoiceEnabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "002", apInvoiceEnabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "00001001", arInvoiceDisabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "004", apInvoiceDisabledNewFeature.AH_TransactionNum);

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				{
					AssertReportValues();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, !enableNewOSOutstandingAmountFeature))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				{
					AssertReportValues();
				}

				void AssertReportValues()
				{
					var expetceOutstandingAmount = enableNewOSOutstandingAmountFeature ? 12000000.00m : 11999960.00m;
					var result = RunScript("", orderby: "order by AH_Ledger, AH_TransactionType, AH_TransactionNum");
					AssertDataTableAllRows($"EnableNewOSOutstandingAmountFeature = {enableNewOSOutstandingAmountFeature}",
						result,
						new[] { "AH_TransactionNum", "OutstandingAmount", "LocalOutstandingAmount" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum, -expetceOutstandingAmount, -857.14m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum, -11999960.00m, -857.14m },
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum, expetceOutstandingAmount, 857.14m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum, 11999960.00m, 857.14m },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 15000000m, 0m, 1071.43m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 15000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(15000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-15000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//Match: os amount 3000000m, local amount 214.29m
				TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 3000000m, "M001");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -3000000m, "M002");
				TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 3000000m, "M003");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -3000000m, "M004", 1m, true);
				Factory.Save();

				AssertEquals(12000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-12000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(857.14m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-857.14m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(857.14m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-857.14m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		[TestDate(2022, 10, 1)]
		public void TestNewOSOutstandingAmountFeature_OSAmountPaidAfterReportDate()
		{
			Invoice arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature, arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature;
			APPayment paymentEnabledNewFeature, paymentDisabledNewFeature;
			ARReceipt receiptEnabledNewFeature, receiptDisabledNewFeature;
			PrepareDataForNewOSOutstandingAmountFeature();

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			{
				AssertForNewOSOutstandingAmountFeature(true);
				AssertForNewOSOutstandingAmountFeature(false);
			}

			//Note:
			//
			// For arInvoiceEnabledNewFeature, apInvoiceEnabledNewFeature
			// OutstandingAmount = AH_OSOutstandingAmount + TotalAP_OSAmount
			//					 = 14000000.00 + 1000000.00
			//					 = 15000000.00m
			//
			// For arInvoiceDisabledNewFeature, apInvoiceDisabledNewFeature
			// OutstandingAmount = (AH_OutstandingAmount + TotalAP_Amount) * AH_ExchangeRate
			//					 = (1000.00 + 71.43) * 14000.00
			//					 = 1071.43 * 14000.00
			//					 = 15000020.00m

			void AssertForNewOSOutstandingAmountFeature(bool enableNewOSOutstandingAmountFeature)
			{
				AssertEquals("Pre-Condition", "00001000", arInvoiceEnabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "002", apInvoiceEnabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "00001001", arInvoiceDisabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "004", apInvoiceDisabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "00001002", paymentEnabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "00001003", paymentDisabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "00001002", receiptEnabledNewFeature.AH_TransactionNum);
				AssertEquals("Pre-Condition", "00001003", receiptDisabledNewFeature.AH_TransactionNum);

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				{
					AssertReportValues();
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, !enableNewOSOutstandingAmountFeature))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableNewOSOutstandingAmountFeature))
				{
					AssertReportValues();
				}

				void AssertReportValues()
				{
					var expetceOutstandingAmount = enableNewOSOutstandingAmountFeature ? 15000000.00m : 15000020.00m;
					var result = RunScript("", orderby: "order by AH_Ledger, AH_TransactionType, AH_TransactionNum");
					AssertDataTableAllRows($"EnableNewOSOutstandingAmountFeature = {enableNewOSOutstandingAmountFeature}",
						result,
						new[] { "AH_TransactionNum", "OutstandingAmount", "LocalOutstandingAmount" },
						new object[][]
						{
							new object[] { apInvoiceEnabledNewFeature.AH_TransactionNum, -expetceOutstandingAmount, -1071.43m },
							new object[] { apInvoiceDisabledNewFeature.AH_TransactionNum, -15000020.00m, -1071.43m },
							new object[] { paymentEnabledNewFeature.AH_TransactionNum, 71.43m, 71.43m },
							new object[] { paymentDisabledNewFeature.AH_TransactionNum, 71.43m, 71.43m },
							new object[] { arInvoiceEnabledNewFeature.AH_TransactionNum, expetceOutstandingAmount, 1071.43m },
							new object[] { arInvoiceDisabledNewFeature.AH_TransactionNum, 15000020.00m, 1071.43m },
							new object[] { receiptEnabledNewFeature.AH_TransactionNum, -71.43m, -71.43m },
							new object[] { receiptDisabledNewFeature.AH_TransactionNum, -71.43m, -71.43m },
						}
					);
				}
			}

			void PrepareDataForNewOSOutstandingAmountFeature()
			{
				arInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
				apInvoiceEnabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "002", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as APInvoice;
				arInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as ARInvoice;
				apInvoiceDisabledNewFeature = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.IDR, 14000m, 29000000m, 0m, 2071.43m, 0m) as APInvoice;

				arInvoiceEnabledNewFeature.AH_OH = arInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Debtor1.PK;
				apInvoiceEnabledNewFeature.AH_OH = apInvoiceDisabledNewFeature.AH_OH = TestObjectCreator.Creditor1.PK;

				arInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				apInvoiceEnabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				arInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;
				apInvoiceDisabledNewFeature.Lines[0].AL_OSExTaxAmount = 29000000m;

				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature);
				TestObjectCreator.UpdateInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature);

				Factory.Save();

				AssertEquals(29000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-29000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(2071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-2071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(2071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-2071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//1st Match (this month): os amount 14000000m, local amount 1000m
				TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 1, 14000000m, "M001");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 1, -14000000m, "M002");
				TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), 14000000m, "M003");
				TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(1), -14000000m, "M004", 1m, true);
				Factory.Save();

				AssertEquals(15000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-15000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1071.43m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1071.43m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1071.43m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);

				//2nd Match (next month): os amount 1000000m, local amount 71.43m
				receiptEnabledNewFeature = TestObjectCreator.CreateAndMatchARReceiptForARInvoiceForNewOSOutstandingAmountFeature(arInvoiceEnabledNewFeature, 31, 1000000m, "M005");
				paymentEnabledNewFeature = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoiceForNewOSOutstandingAmountFeature(apInvoiceEnabledNewFeature, 31, -1000000m, "M006");
				receiptDisabledNewFeature = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), 1000000m, "M007");
				paymentDisabledNewFeature = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoiceDisabledNewFeature, ZDateTime.Today.AddDays(31), -1000000m, "M008", 1m, true);
				Factory.Save();

				AssertEquals(14000000m, arInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(-14000000m, apInvoiceEnabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, arInvoiceDisabledNewFeature.AH_OSOutstandingAmount);
				AssertEquals(0m, apInvoiceDisabledNewFeature.AH_OSOutstandingAmount);

				AssertEquals(1000.00m, arInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1000.00m, apInvoiceEnabledNewFeature.AH_OutstandingAmount);
				AssertEquals(1000.00m, arInvoiceDisabledNewFeature.AH_OutstandingAmount);
				AssertEquals(-1000.00m, apInvoiceDisabledNewFeature.AH_OutstandingAmount);
			}
		}

		#endregion

		public void TestAdditionalCompanyNameForReceivables()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

				var expectedAdditionalCompanyName = "人生得意须尽欢";

				AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

				var orgHeader = TestObjectCreator.CreateOrgHeader("ABC", false, true);
				var orgAddress = TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "莫使金樽空对月", expectedAdditionalCompanyName, OrgAddressType.Receivables, OrgAddressType.Receivables);

				Factory.Save();

				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);

				Factory.Save();

				DataTable resultForOrg = RunScript(LedgerTypes.AccountsReceivable);

				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		public void TestAdditionalCompanyNameForReceivablesWhenCompanyOrgProxyIsNull()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var companyWithoutOrgProxy = testObjectCreator.CreateNewCompany("ABC");
			companyWithoutOrgProxy.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			companyWithoutOrgProxy.GC_OH_OrgProxy = ZGuid.Empty;

			var branchWithoutOrgProxy = testObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB1");
			branchWithoutOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			var branchOrgProxy = testObjectCreator.CreateOrgHeader("ORGBB2", false, true);
			branchOrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
			var branchWithOrgProxy = testObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB2");
			branchWithOrgProxy.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			var expectedAdditionalCompanyName = "人生得意须尽欢";
			var glAccount = testObjectCreator.GetGLAccountFromDB();
			var orgHeader = testObjectCreator.CreateOrgHeader("ABC", false, true);
			testObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "莫使金樽空对月", expectedAdditionalCompanyName, OrgAddressType.Receivables, OrgAddressType.Receivables);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				testObjectCreator.CreateTestPeriods(ZDateTime.Today);
				var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", testObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);
				Factory.Save();

				var resultForOrg = RunScript(LedgerTypes.AccountsReceivable);
				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithoutOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var resultForOrg = RunScript(LedgerTypes.AccountsReceivable);
				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be null", DBNull.Value, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		public void TestAdditionalCompanyNameForPayables()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

				var expectedAdditionalCompanyName = "人生得意须尽欢";

				AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

				var orgHeader = TestObjectCreator.CreateOrgHeader("ABC", true, false);
				var orgAddress = TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "莫使金樽空对月", expectedAdditionalCompanyName, OrgAddressType.Receivables, OrgAddressType.Receivables);

				Factory.Save();

				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);

				Factory.Save();

				DataTable resultForOrg = RunScript(LedgerTypes.AccountsPayable);

				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		public void TestAdditionalCompanyNameForPayablesWhenCompanyOrgProxyIsNull()
		{
			var companyWithoutOrgProxy = TestObjectCreator.CreateNewCompany("ABC");
			companyWithoutOrgProxy.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			companyWithoutOrgProxy.GC_OH_OrgProxy = ZGuid.Empty;

			var branchWithoutOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB1");
			branchWithoutOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGBB2", false, true);
			branchOrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
			var branchWithOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB2");
			branchWithOrgProxy.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			var expectedAdditionalCompanyName = "人生得意须尽欢";
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var orgHeader = TestObjectCreator.CreateOrgHeader("ABC", true, false);
			TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "莫使金樽空对月", expectedAdditionalCompanyName, OrgAddressType.Receivables, OrgAddressType.Receivables);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);
				Factory.Save();

				var resultForOrg = RunScript(LedgerTypes.AccountsPayable);
				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithoutOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var resultForOrg = RunScript(LedgerTypes.AccountsPayable);
				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be null", DBNull.Value, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		[TestDate(2020, 12, 30)]
		public void TestShouldHitIndexNR_RX__AH_GC_AH_Ledger_AH_FullyPaidDate_AH_PostDate_AH_TransactionType()
		{
			var period = new AccountingPeriodTestHelper(Factory);
			period.SetupSinglePeriod(202012, new ZDateTime(2020, 12, 01), new ZDateTime(2020, 12, 31));

			PrepareDate();

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var rowCount = 0;
				var query = GetQueryWith(202012, LedgerTypes.AccountsReceivable);
				TestConnection.ExecuteReader(query, r => rowCount++);
				AssertEquals(40, rowCount);

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("Report_OutstandingARAPTransactionsListing"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				AssertEquals(1, queryPlanAnalyzer.IndexSeeks.Count(x => x.IndexName == AccTransactionHeaderSchema.Constants.Indexes.NR_RX__AH_GC_AH_Ledger_AH_FullyPaidDate_AH_PostDate_AH_TransactionType));
				Assert("Should not contains any Lookup", !queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexKind.Contains("Lookup") && x.TableName == AccTransactionHeaderSchema.Constants.TableName));
				Assert("Should not contains any TableScan", queryPlanAnalyzer.TableScans.All(x => x.TableName != AccTransactionHeaderSchema.Constants.TableName));
				Assert("Should not contains any IndexScans", queryPlanAnalyzer.IndexScans.All(x => x.TableName != AccTransactionHeaderSchema.Constants.TableName));
			}

			void PrepareDate()
			{
				TestConnection.ExecuteNonQuery($@"
DECLARE @TotalCount int = 0;
WHILE @TotalCount < 20
BEGIN
-- Should be returned(AH_PostDate <= Period.PeriodEnd)
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'AR', 'INV', 'INV' + CAST(@TotalCount AS varchar(10)), '2020-12-30 15:00:00', '2020-12-30 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2020-12-30 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

-- Should be returned(AH_FullyPaidDate > Period.PeriodEnd)
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_FullyPaidDate, AH_OutstandingAmount, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'AR', 'INV', 'INV3' + CAST(@TotalCount AS varchar(10)), '2021-01-01 15:00:00', 0.000, '2020-12-30 15:00:00', '2020-12-30 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2020-12-30 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

-- Should not be returned(Post Date bigger than '2020-12-30')
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'AR', 'INV', 'INV5' + CAST(@TotalCount AS varchar(10)), '2021-01-01 15:00:00', '2021-01-01 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2021-01-01 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

-- Should not be returned(Leger not equal to 'AR' or 'AP')
INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_InvoiceDate, AH_DueDate, AH_PostToGL, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_OH, AH_GC, AH_GB, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
	VALUES(NEWID(), 'UA', 'UAI', 'UAI' + CAST(@TotalCount AS varchar(10)), '2020-12-30 15:00:00', '2020-12-30 15:00:00', 'Y', 100.0000, 100.0000, 'CNY', 1, '2020-12-30 15:00:00', '{TestObjectCreator.Debtor.PK}', '{GlbCompany.CurrentCompany.PK}', '{GlbBranch.CurrentBranch.PK}', '{GlbDepartment.CurrentDepartment.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

	SET @TotalCount = @TotalCount + 1
END");
			}
		}

		[TestDate(2020, 12, 30)]
		public void TestPredicateOfAH_FullyPaidDateIsNull()
		{
			var period = new AccountingPeriodTestHelper(Factory);
			period.SetupSinglePeriod(202012, new ZDateTime(2020, 12, 01), new ZDateTime(2020, 12, 31));

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK);
			var arReceipt = TestObjectCreator.CreateARReceipt(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var matchingBase = new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase.AddIMatching(arInvoice1);
			matchingBase.AddIMatching(arReceipt);
			matchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { arInvoice1, arReceipt });
			matchingBase.Match_ForTestOnly();
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("arInvoice2 should be loaded as the AH_FullyPaidDate is null.", arInvoice2.AH_TransactionNum, result.Rows[0]["AH_TransactionNum"]);
		}

		[TestDate(2020, 12, 30)]
		public void TestPredicateOfAH_FullyPaidDateIsBiggerThanPeriodEnd()
		{
			var period = new AccountingPeriodTestHelper(Factory);
			period.SetupSinglePeriod(202012, new ZDateTime(2020, 12, 01), new ZDateTime(2020, 12, 31));

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK);
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK);
			var arReceipt1 = TestObjectCreator.CreateARReceipt(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var arReceipt2 = TestObjectCreator.CreateARReceipt(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var matchingBase1 = new ARMatchingBase(Factory);
			matchingBase1.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase1.AddIMatching(arInvoice1);
			matchingBase1.AddIMatching(arReceipt1);
			matchingBase1.MoveFromUnmatchToMatch(new BusinessObject[] { arInvoice1, arReceipt1 });
			matchingBase1.Match_ForTestOnly();

			var matchingBase2 = new ARMatchingBase(Factory);
			matchingBase2.MatchDate = new ZDateTime(2021, 01, 01);
			matchingBase2.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase2.AddIMatching(arInvoice2);
			matchingBase2.AddIMatching(arReceipt2);
			matchingBase2.MoveFromUnmatchToMatch(new BusinessObject[] { arInvoice2, arReceipt2 });
			matchingBase2.Match_ForTestOnly();
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsReceivable, period: 202012);
			AssertEquals(2, result.Rows.Count);
			var results = result.Rows.Cast<DataRow>().Select(x => x["AH_TransactionNum"]).ToArray();
			AssertCollectionContains(arInvoice2.AH_TransactionNum.ToString(), results);
			AssertCollectionContains(arReceipt2.AH_TransactionNum.ToString(), results);
		}

		public void TestARSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsReceivable);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has ABIGAS as settlemet code", 3, resultForSettlementGroup.Select("ARSettlementGroupCode = 'ABIGAS'").Length);
		}

		public void TestAPSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.AALSHI.APSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.Creditor1.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.Creditor2.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsPayable);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has AALSHI as settlemet code", 3, resultForSettlementGroup.Select("APSettlementGroupCode = 'AALSHI'").Length);
		}

		public void TestARCountryList()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice1.AH_OH = orgHeader1.PK;

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice2.AH_OH = orgHeader2.PK;

			Factory.Save();

			DataTable resultForUS = RunScript(
				LedgerTypes.AccountsReceivable,
				null,
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScript(
				LedgerTypes.AccountsReceivable,
				null,
				Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);
		}

		public void TestAPCountryList()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var invoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice1.AH_OH = orgHeader1.PK;

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice2.AH_OH = orgHeader2.PK;

			Factory.Save();

			DataTable resultForUS = RunScript(
				LedgerTypes.AccountsPayable,
				null,
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScript(
				LedgerTypes.AccountsPayable,
				null,
				Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);
		}

		public void TestOSOutstandingAmountForForeignCurrencyInvoice()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			var invoicingBase = TestObjectCreator.CreateInvoice(typeof(APInvoice), EUR, 105.17M, TestObjectCreator.AALSHI);

			TestObjectCreator.CreateInvoiceLine(invoicingBase, EUR, 105.17M, 2363.74M, 0M, 0M, glAccount.PK);
			TestObjectCreator.CreateInvoiceLine(invoicingBase, EUR, 105.17M, -2261.01M, 0M, 0M, glAccount.PK);
			TestObjectCreator.CreateInvoiceLine(invoicingBase, EUR, 105.17M, -102M, 0M, 0M, glAccount.PK);
			Factory.Save();

			DataTable result = RunScript(LedgerTypes.AccountsPayable);

			AssertEquals("1 transaction found", 1, result.Rows.Count);
			AssertNotEquals("OS outstanding amount and local outstanding amount can not be the same", Utilities.Round(Convert.ToDecimal(result.Rows[0]["OutstandingAmount"]), 4), Utilities.Round(Convert.ToDecimal(result.Rows[0]["LocalOutstandingAmount"]), 4));
		}

		public void TestConsolTypeBillIssueDateShipmentTypeServiceLevelChargeableWeightSpotRate_OneConsolOneShipment()
		{
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			shipment.JS_ShipmentType = "CLD";
			shipment.JS_RS_NKServiceLevel = "COL";
			shipment.JS_ActualChargeable = 166.67;
			shipment.JS_UnitFreightRate = 3;

			var consol = TestObjectCreator.CreateConsol("CNTES", "AUBRS", "CSL000000002");
			consol.JK_AgentType = "ABC";
			consol.JK_MasterBillNum = "123";
			consol.JK_MasterBillIssueDate = new ZDateTime(2011, 2, 2);
			consol.Shipments.Add(shipment);

			TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 1000);

			Factory.Save();

			var job = shipment.Job as Job;
			job.Charges[0].JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			var apCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", GlbCompany.CurrentCompany.LocalCurrency, 1000m, TestObjectCreator.Creditor1, GlbCompany.CurrentCompany.LocalCurrency, 0m, TestObjectCreator.AALSHI);
			apCharge.JR_APInvoiceNum = "5";
			apCharge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);

			Factory.Save();

			var invoicingPostManager = new InvoicingPostManager(job);
			var transactions = invoicingPostManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			var expectConsolNumAndType = string.Join(string.Empty, consol.JK_UniqueConsignRef, " (ABC )");
			var expectMasterBillNumAndIssueDate = string.Join(string.Empty, consol.JK_MasterBillNum, " (02-Feb-11 )");

			var result = RunScript(LedgerTypes.AccountsReceivable);

			AssertEquals("1 transactions found", 1, result.Rows.Count);
			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, result.Rows[0]["ConsolNumAndType"]);
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, result.Rows[0]["MasterBillNumAndIssueDate"]);
			AssertEquals("Shipment Type must be CLD", "CLD", result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be COL", "COL", result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be 166.67", new decimal(166.67), result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be 3", new decimal(3), result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "ABC");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			var startDateContainData = new DateTime(2011, 1, 1);
			var startDateNoData = new DateTime(2012, 3, 1);
			var endDate = new DateTime(2014, 1, 1);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, shipmentType: "CLD");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, shipmentType: "ABC");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, result.Rows[0]["ConsolNumAndType"]);
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, result.Rows[0]["MasterBillNumAndIssueDate"]);
			AssertEquals("Shipment Type must be CLD", "CLD", result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be COL", "COL", result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be 166.67", new decimal(166.67), result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be 3", new decimal(3), result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "ABC");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, shipmentType: "CLD");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, shipmentType: "ABC");
			AssertEquals("No transactions found", 0, result.Rows.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestConsolTypeBillIssueDateShipmentTypeServiceLevelChargeableWeightSpotRate_MutipleConsolOneShipment()
		{
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			shipment.JS_ShipmentType = "CLD";
			shipment.JS_RS_NKServiceLevel = "COL";
			shipment.JS_ActualChargeable = 166.67;
			shipment.JS_UnitFreightRate = 3;

			var consolList = new List<ForwardingConsol>();
			{
				var consol = TestObjectCreator.CreateConsol("CNTES", "AUBRS", "CSL000000001");
				consolList.Add(consol);
				consol.JK_AgentType = "AGT";
				consol.JK_MasterBillNum = "123";
				consol.JK_MasterBillIssueDate = new ZDateTime(2011, 2, 2);
				consol.Shipments.Add(shipment);

				consol = TestObjectCreator.CreateConsol("AUBRS", "TWTPE", "CSL000000002");
				consolList.Add(consol);
				consol.JK_AgentType = "ABC";
				consol.JK_MasterBillNum = "456";
				consol.JK_MasterBillIssueDate = new ZDateTime(2012, 2, 2);
				consol.Shipments.Add(shipment);
			}

			foreach (var consol in consolList)
			{
				TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 1000);
			}

			Factory.Save();

			var job = shipment.Job as Job;
			job.Charges[0].JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			var apCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", GlbCompany.CurrentCompany.LocalCurrency, 1000m, TestObjectCreator.Creditor1, GlbCompany.CurrentCompany.LocalCurrency, 0m, TestObjectCreator.AALSHI);
			apCharge.JR_APInvoiceNum = "5";
			apCharge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);

			Factory.Save();

			var invoicingPostManager = new InvoicingPostManager(job);
			var transactions = invoicingPostManager.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			var expectConsolNumAndTypeList = consolList.Select(consol => string.Join(string.Empty, consol.JK_UniqueConsignRef, " (", consol.JK_AgentType, " )")).OrderBy(x => x).ToList();
			var expectConsolNumAndType = string.Join(", ", expectConsolNumAndTypeList);
			var expectMasterBillNumAndIssueDateList = consolList.Select(consol => string.Join(string.Empty, consol.JK_MasterBillNum, " (", consol.JK_MasterBillIssueDate.ToString("dd-MMM-yy"), " )")).OrderBy(x => x).ToList();
			var expectMasterBillNumAndIssueDate = string.Join(", ", expectMasterBillNumAndIssueDateList);

			var result = RunScript(LedgerTypes.AccountsReceivable);
			var consolNumAndTypes = ((string)result.Rows[0]["ConsolNumAndType"]).Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();
			var masterBillNumAndIssueDates = ((string)result.Rows[0]["MasterBillNumAndIssueDate"]).Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();

			AssertEquals("1 transactions found", 1, result.Rows.Count);
			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, string.Join(", ", consolNumAndTypes));
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, string.Join(", ", masterBillNumAndIssueDates));
			AssertEquals("Shipment Type must be CLD", "CLD", result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be COL", "COL", result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be 166.67", new decimal(166.67), result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be 3", new decimal(3), result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "AGT");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			var startDateContainData = new DateTime(2011, 1, 1);
			var startDateNoData = new DateTime(2013, 3, 1);
			var endDate = new DateTime(2014, 1, 1);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, shipmentType: "CLD");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, shipmentType: "ABC");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable);
			consolNumAndTypes = ((string)result.Rows[0]["ConsolNumAndType"]).Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();
			masterBillNumAndIssueDates = ((string)result.Rows[0]["MasterBillNumAndIssueDate"]).Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();

			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, string.Join(", ", consolNumAndTypes));
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, string.Join(", ", masterBillNumAndIssueDates));
			AssertEquals("Shipment Type must be CLD", "CLD", result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be COL", "COL", result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be 166.67", new decimal(166.67), result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be 3", new decimal(3), result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "AGT");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "AGT", masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "DEF", masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "AGT", masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, shipmentType: "CLD");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, shipmentType: "ABC");
			AssertEquals("No transactions found", 0, result.Rows.Count);
		}

		public void TestConsolTypeBillIssueDateShipmentTypeServiceLevelChargeableWeightSpotRate_OneConsolMultipleShipment()
		{
			var consol = TestObjectCreator.CreateConsol("CNTES", "AUBRS", "CSL000000002");
			consol.JK_AgentType = "ABC";
			consol.JK_MasterBillNum = "123";
			consol.JK_MasterBillIssueDate = new ZDateTime(2011, 2, 2);

			var shipment1 = TestObjectCreator.CreateShipment("S00000001");
			shipment1.JS_ShipmentType = "CLD";
			shipment1.JS_RS_NKServiceLevel = "COL";
			shipment1.JS_ActualChargeable = 166.67;
			shipment1.JS_UnitFreightRate = 3;

			var shipment2 = TestObjectCreator.CreateShipment("S00000002");
			shipment2.JS_ShipmentType = "ABC";
			shipment2.JS_RS_NKServiceLevel = "DEF";
			shipment2.JS_ActualChargeable = 100;
			shipment2.JS_UnitFreightRate = 4;

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			var apportionments = new ApportionmentListing(Factory, consol);
			var cost = apportionments.CostsCollection.TryAddNew();

			cost.E6_ApportionToRelatedShipments = true;
			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				charge.JR_IsUsedForApportionment = true;
			}
			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
			cost.E6_RX_NKCurrency = TestObjectCreator.USD.RX_Code;
			cost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			cost.E6_ExchangeRate = 0.6m;
			cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			cost.E6_OSCostAmount = 171.35m;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_AT_TaxRate = TestObjectCreator.GST1.PK;

			shipment1.Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.Addresses[0].PK;
			shipment2.Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.Addresses[0].PK;

			Factory.Save();

			((Job)shipment1.Job).Charges[0].JR_OH_SellAccount = TestObjectCreator.Agent.PK;
			((Job)shipment2.Job).Charges[0].JR_OH_SellAccount = TestObjectCreator.Agent.PK;

			Factory.Save();

			var jobs = new List<Job> { shipment1.Job as Job, shipment2.Job as Job };
			foreach (ApportionSplitCharge charge in cost.ApportionmentCharges)
			{
				jobs.Add(Factory.Load<Job>(charge.JR_JH));
			}

			var consolInvoicingPostManager = new ConsolInvoicingPostManager(Factory, jobs, consol, apportionments);
			var transactions = consolInvoicingPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			var arTransactions = transactions.GetAllARTransactions();
			var apTransactions = transactions.GetAllAPTransactions();

			AssertEquals("Must have two lines in ARInvoice", 2, ((ARInvoice)arTransactions[0]).Lines.Count);
			AssertEquals("Must have two lines in APInvoice", 2, ((APInvoice)apTransactions[0]).Lines.Count);

			var expectConsolNumAndType = string.Join(string.Empty, consol.JK_UniqueConsignRef, " (ABC )");
			var expectMasterBillNumAndIssueDate = string.Join(string.Empty, consol.JK_MasterBillNum, " (02-Feb-11 )");

			var result = RunScript(LedgerTypes.AccountsReceivable);

			AssertEquals("1 transactions found", 1, result.Rows.Count);
			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, result.Rows[0]["ConsolNumAndType"]);
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, result.Rows[0]["MasterBillNumAndIssueDate"]);
			AssertEquals("Shipment Type must be empty", DBNull.Value, result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be empty", DBNull.Value, result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be empty", DBNull.Value, result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be empty", DBNull.Value, result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "ABC");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			var startDateContainData = new DateTime(2011, 1, 1);
			var startDateNoData = new DateTime(2012, 3, 1);
			var endDate = new DateTime(2014, 1, 1);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("1 transactions found", 1, result.Rows.Count);
			AssertEquals("ConsolNumAndType must be empty", DBNull.Value, result.Rows[0]["ConsolNumAndType"]);
			AssertEquals("MasterBillNumAndIssueDate must be empty", DBNull.Value, result.Rows[0]["MasterBillNumAndIssueDate"]);
			AssertEquals("Shipment Type must be empty", DBNull.Value, result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be empty", DBNull.Value, result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be empty", DBNull.Value, result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be empty", DBNull.Value, result.Rows[0]["JS_UnitFreightRate"]);
		}

		public void TestConsolTypeBillIssueDateShipmentTypeServiceLevelChargeableWeightSpotRate_GatewayConsol()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);

			TestObjectCreator.CreateJob(consol);
			consol.JK_AgentType = "ABC";
			consol.JK_MasterBillNum = "123";
			consol.JK_MasterBillIssueDate = new ZDateTime(2011, 2, 2);

			Factory.Save();

			var job = consol.Job as Job;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Cost Transaction Test", TestObjectCreator.AUD, 100M, TestObjectCreator.ZECTRA, TestObjectCreator.AUD, 120M, TestObjectCreator.ABIGAS);
			charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);

			Factory.Save();

			var invoicingPostManager = new InvoicingPostManager(job);
			var transactions = invoicingPostManager.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			var expectConsolNumAndType = string.Join(string.Empty, consol.JK_UniqueConsignRef, " (ABC )");
			var expectMasterBillNumAndIssueDate = string.Join(string.Empty, consol.JK_MasterBillNum, " (02-Feb-11 )");

			var result = RunScript(LedgerTypes.AccountsReceivable);

			AssertEquals("1 transactions found", 1, result.Rows.Count);
			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, result.Rows[0]["ConsolNumAndType"]);
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, result.Rows[0]["MasterBillNumAndIssueDate"]);
			AssertEquals("Shipment Type must be empty", DBNull.Value, result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be empty", DBNull.Value, result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be empty", DBNull.Value, result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be empty", DBNull.Value, result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "ABC");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			var startDateContainData = new DateTime(2011, 1, 1);
			var startDateNoData = new DateTime(2012, 3, 1);
			var endDate = new DateTime(2014, 1, 1);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsReceivable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable);
			AssertEquals("1 transactions found", 1, result.Rows.Count);
			AssertEquals($"ConsolNumAndType must be {expectConsolNumAndType}", expectConsolNumAndType, result.Rows[0]["ConsolNumAndType"]);
			AssertEquals($"MasterBillNumAndIssueDate must be {expectMasterBillNumAndIssueDate}", expectMasterBillNumAndIssueDate, result.Rows[0]["MasterBillNumAndIssueDate"]);
			AssertEquals("Shipment Type must be empty", DBNull.Value, result.Rows[0]["JS_ShipmentType"]);
			AssertEquals("Service Level must be empty", DBNull.Value, result.Rows[0]["JS_RS_NKServiceLevel"]);
			AssertEquals("Chargeable Weight must be empty", DBNull.Value, result.Rows[0]["JS_ActualChargeable"]);
			AssertEquals("Spot Rate must be empty", DBNull.Value, result.Rows[0]["JS_UnitFreightRate"]);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "ABC");
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, consolType: "DEF");
			AssertEquals("No transactions found", 0, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, masterBillIssueStartDate: startDateContainData, masterBillIssueEndDate: endDate);
			AssertEquals("1 transactions found", 1, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, masterBillIssueStartDate: startDateNoData, masterBillIssueEndDate: endDate);
			AssertEquals("No transactions found", 0, result.Rows.Count);
		}

		const string Scenario_RegularConsolNumber = "";
		const string Scenario_LongerConsolNumber = "LongerConsolNumber";

		void SetUpConsolLevelInvoice(string scenario)
		{
			string consolNumber = Scenario_LongerConsolNumber == "LongerConsolNumber" ? "CSL0000001" : "C00000001";
			bool twoInvoices = scenario == Scenario_MultiInvoices;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", consolNumber);
			consol.JK_MasterBillNum = "MBN001";
			var s1 = TestObjectCreator.CreateShipment("S00000001", consol);
			var s2 = TestObjectCreator.CreateShipment("S00000002", consol);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 1000);

			var debtor1 = TestObjectCreator.Agent;
			var debtor2 = twoInvoices ? TestObjectCreator.Agent2 : TestObjectCreator.Agent;

			s1.Job.JH_OA_AgentCollectAddr = debtor1.Addresses[0].PK;
			s2.Job.JH_OA_AgentCollectAddr = debtor2.Addresses[0].PK;
			Factory.Save();

			((Job)s1.Job).Charges[0].JR_OH_SellAccount = debtor1.PK;
			((Job)s2.Job).Charges[0].JR_OH_SellAccount = debtor2.PK;
			Factory.Save();

			var jobs = new Job[] { s1.Job as Job, s2.Job as Job };
			var transactionCreator = new ConsolInvoicingPostManager(Factory, jobs, consol, new ApportionmentListing(Factory, consol));
			var transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			var arTransactions = transactions.GetAllARTransactions();

			// Sanity checks
			AssertEquals("Precondition: Number of invoices", twoInvoices ? 2 : 1, arTransactions.Length);
			AssertEquals("Precondition: No job header", ZGuid.Empty, arTransactions[0].AH_JH);
			AssertEquals("Precondition: AH_ConsolidatedInvoiceRef set properly", consolNumber, arTransactions[0].AH_ConsolidatedInvoiceRef);

			if (twoInvoices)
			{
				AssertEquals("Precondition: AH_ConsolidatedInvoiceRef set properly", consolNumber + "/A", arTransactions[1].AH_ConsolidatedInvoiceRef);
			}
		}

		const string Scenario_SingleInvoicesAndConsols = "";
		const string Scenario_MultiInvoices = "MultiInvoices";
		const string Scenario_MultiConsols = "MultiConsols";

		void SetupShipmentLevelInvoice(string scenario)
		{
			bool twoInvoices = scenario == Scenario_MultiInvoices;
			bool overTwoConsols = scenario == Scenario_MultiConsols;

			// Create shipment and consol(s)
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var consolList = new List<ForwardingConsol>();
			{
				var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "CSL000000001");
				consolList.Add(consol);
				consol.JK_MasterBillNum = "MBN001";
				consol.Shipments.Add(shipment);

				if (overTwoConsols)
				{
					consol = TestObjectCreator.CreateConsol("AUSYD", "AUBRS", "CSL000000002");
					consolList.Add(consol);
					consol.JK_MasterBillNum = "MBN002";
					consol.Shipments.Add(shipment);
				}
			}

			// Create consol costs
			foreach (var consol in consolList)
			{
				TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 1000);

				if (twoInvoices)
				{
					TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC2, 1000);
				}
			}

			Factory.Save();

			// Set charge debtors.
			var job = shipment.Job as Job;
			AssertEquals("Precondition: Correct number of charges", (twoInvoices || overTwoConsols ? 2 : 1), job.Charges.Count);
			job.Charges[0].JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			if (twoInvoices)
			{
				job.Charges[1].JR_OH_SellAccount = TestObjectCreator.LocalClient2.PK;
			}

			Factory.Save();

			// Post
			var transactionCreator = new InvoicingPostManager(job);
			var transactions = transactionCreator.CreateTransactions(JobInvoicingPostingOption.All);
			Factory.Save();

			// Check data is setup as expected for this test
			var arTransactions = transactions.GetAllARTransactions();

			AssertEquals("Precondition: Number of invoices", twoInvoices ? 2 : 1, arTransactions.Length);

			foreach (var txn in arTransactions)
			{
				AssertEquals("Precondition: One charge", 1, ((ARInvoice)txn).Lines.Count);
				AssertEquals("Precondition: Job header set", false, txn.AH_JH.IsEmpty);
			}

			AssertEquals("Precondition: AH_ConsolidatedInvoiceRef set", "S00000001", arTransactions[0].AH_ConsolidatedInvoiceRef);

			if (twoInvoices)
			{
				AssertEquals("Precondition: AH_ConsolidatedInvoiceRef set", "S00000001/A", arTransactions[1].AH_ConsolidatedInvoiceRef);
			}
		}

		public void TestMasterBillNumWhenConsolidatedInvoiceRefPostfixLetters_ConsolLevelInvoice()
		{
			SetUpConsolLevelInvoice(Scenario_MultiInvoices);

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals("2 transactions found", 2, result.Rows.Count);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[0]["JK_MasterBillNum"]);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[1]["JK_MasterBillNum"]);
		}

		public void TestMasterBillNumWhenStandardConsolPrefix_ConsolLevelInvoice()
		{
			SetUpConsolLevelInvoice(Scenario_RegularConsolNumber);

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals("1 transaction found", 1, result.Rows.Count);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[0]["JK_MasterBillNum"]);
		}

		public void TestMasterBillNumWhenNonStandardConsolPrefix_ConsolLevelInvoice()
		{
			SetUpConsolLevelInvoice(Scenario_LongerConsolNumber);

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals("1 transaction found", 1, result.Rows.Count);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[0]["JK_MasterBillNum"]);
		}

		public void TestMasterBillNumWhenNonStandardConsolPrefix_ShipmentLevelInvoice()
		{
			SetupShipmentLevelInvoice(Scenario_SingleInvoicesAndConsols);

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals("1 transaction found", 1, result.Rows.Count);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[0]["JK_MasterBillNum"]);
		}

		public void TestMasterBillNumWhenConsolidatedInvoiceRefPostfixLetters_ShipmentLevelInvoice()
		{
			SetupShipmentLevelInvoice(Scenario_MultiInvoices);

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals("2 transactions found", 2, result.Rows.Count);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[0]["JK_MasterBillNum"]);
			AssertEquals("Master Bill Number Comes Through", "MBN001", result.Rows[1]["JK_MasterBillNum"]);
		}

		public void TestMasterBillNumWhenMultipleConsols_ShipmentLevelInvoice()
		{
			SetupShipmentLevelInvoice(Scenario_MultiConsols);

			DataTable result = RunScript(LedgerTypes.AccountsReceivable);
			AssertEquals("1 transaction found", 1, result.Rows.Count);

			var masterBillNum = result.Rows[0]["JK_MasterBillNum"].ToString();
			var sortedNums = masterBillNum.Split(',').Select(x => x.Trim()).OrderBy(x => x).ToList();
			AssertEquals("Master Bill Numbers Come Through", "MBN001, MBN002", string.Concat(sortedNums[0], ", ", sortedNums[1]));
		}

		[TestDate(2020, 02, 10)]
		public void TestTransactionMatchedInFutureDate()
		{
			var period = new AccountingPeriodTestHelper(Factory);
			period.SetupSinglePeriod(202001, new ZDateTime(2020, 01, 01), new ZDateTime(2020, 01, 31));
			period.SetupSinglePeriod(202002, new ZDateTime(2020, 02, 01), new ZDateTime(2020, 02, 28));
			period.SetupSinglePeriod(202003, new ZDateTime(2020, 03, 01), new ZDateTime(2020, 03, 31));
			period.SetupSinglePeriod(202004, new ZDateTime(2020, 04, 01), new ZDateTime(2020, 04, 30));

			var invHeader = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 6000m, 0m, 6000m, 0m, TestObjectCreator.Debtor, TestObjectCreator.FRT.PK
				, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), false);

			var recHeader1 = TestObjectCreator.CreateARReceipt(1m, 1000m, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var recHeader2 = TestObjectCreator.CreateARReceipt(1m, 2000m, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			var recHeader3 = TestObjectCreator.CreateARReceipt(1m, 3000m, new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var recHeaderFuture = TestObjectCreator.CreateARReceipt(1m, 1234m, new DateTime(2020, 03, 01), new DateTime(2020, 03, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			var matchingBase1 = new ARMatchingBase(Factory);
			matchingBase1.MatchDate = new DateTime(2020, 02, 01);
			matchingBase1.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase1.AddIMatching(invHeader);
			matchingBase1.AddIMatching(recHeader1);
			matchingBase1.MoveFromUnmatchToMatch(new BusinessObject[] { invHeader, recHeader1 });
			((IMatching)invHeader).OSPartialPaymentAmount = 1000;
			((IMatching)recHeader1).OSPartialPaymentAmount = -1000;
			matchingBase1.Match_ForTestOnly();

			var matchingBase2 = new ARMatchingBase(Factory);
			matchingBase2.MatchDate = new DateTime(2020, 03, 01);
			matchingBase2.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase2.AddIMatching(invHeader);
			matchingBase2.AddIMatching(recHeader2);
			matchingBase2.MoveFromUnmatchToMatch(new BusinessObject[] { invHeader, recHeader2 });
			((IMatching)invHeader).OSPartialPaymentAmount = 2000;
			((IMatching)recHeader2).OSPartialPaymentAmount = -2000;
			matchingBase2.Match_ForTestOnly();

			var matchingBase3 = new ARMatchingBase(Factory);
			matchingBase3.MatchDate = new DateTime(2020, 04, 01);
			matchingBase3.PrimaryOrganization = TestObjectCreator.Debtor.PK;
			matchingBase3.AddIMatching(invHeader);
			matchingBase3.AddIMatching(recHeader3);
			matchingBase3.MoveFromUnmatchToMatch(new BusinessObject[] { invHeader, recHeader3 });
			((IMatching)invHeader).OSPartialPaymentAmount = 3000;
			((IMatching)recHeader3).OSPartialPaymentAmount = -3000;
			matchingBase3.Match_ForTestOnly();

			Factory.Save();

			var resultMonth01 = RunScript(LedgerTypes.AccountsReceivable, period: 202001).Rows.Cast<DataRow>();
			AssertEquals("before matched , showing all transactions", 4, resultMonth01.Count());
			AssertEquals("before matched , showing all transactions", 0m, resultMonth01.Sum(row => (decimal)row["OutstandingAmount"]));
			AssertEquals(6000m, resultMonth01.First(row => CompareTransaction(row, invHeader))["OutstandingAmount"]);
			AssertEquals(true, resultMonth01.Any(row => CompareTransaction(row, recHeader1)));
			AssertEquals(true, resultMonth01.Any(row => CompareTransaction(row, recHeader2)));
			AssertEquals(true, resultMonth01.Any(row => CompareTransaction(row, recHeader3)));

			var resultMonth02 = RunScript(LedgerTypes.AccountsReceivable, period: 202002).Rows.Cast<DataRow>();
			AssertEquals("after matched 1 transaction , showing 3 transactions", 3, resultMonth02.Count());
			AssertEquals("before matched , showing all transactions", 0m, resultMonth02.Sum(row => (decimal)row["OutstandingAmount"]));
			AssertEquals(5000m, resultMonth02.First(row => CompareTransaction(row, invHeader))["OutstandingAmount"]);
			AssertEquals(false, resultMonth02.Any(row => CompareTransaction(row, recHeader1)));
			AssertEquals(true, resultMonth02.Any(row => CompareTransaction(row, recHeader2)));
			AssertEquals(true, resultMonth02.Any(row => CompareTransaction(row, recHeader3)));

			var resultMonth03 = RunScript(LedgerTypes.AccountsReceivable, period: 202003).Rows.Cast<DataRow>();
			AssertEquals("after matched 2 transaction , showing 2 transactions plus the future one", 3, resultMonth03.Count());
			AssertEquals("before matched , showing all transactions", -1234m, resultMonth03.Sum(row => (decimal)row["OutstandingAmount"]));
			AssertEquals(3000m, resultMonth03.First(row => CompareTransaction(row, invHeader))["OutstandingAmount"]);
			AssertEquals(false, resultMonth03.Any(row => CompareTransaction(row, recHeader1)));
			AssertEquals(false, resultMonth03.Any(row => CompareTransaction(row, recHeader2)));
			AssertEquals(true, resultMonth03.Any(row => CompareTransaction(row, recHeader3)));
			AssertEquals(true, resultMonth03.Any(row => CompareTransaction(row, recHeaderFuture)));

			var resultMonth04 = RunScript(LedgerTypes.AccountsReceivable, period: 202004).Rows.Cast<DataRow>();
			AssertEquals("after matched all transaction , showing the future one only", 1, resultMonth04.Count());
			AssertEquals(true, resultMonth04.Any(row => CompareTransaction(row, recHeaderFuture)));

			bool CompareTransaction(DataRow row, AccTransactionHeader header)
			{
				return (string)row["AH_TransactionNum"] == header.AH_TransactionNum
				&& (string)row["AH_Ledger"] == header.AH_Ledger
				&& (string)row["AH_TransactionType"] == header.AH_TransactionType;
			}
		}

		protected RefCurrency eur;
		protected RefCurrency EUR
		{
			get
			{
				if (eur == null)
				{
					eur = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
				}
				return eur;
			}
		}

		DataTable RunScript(string ledger, string country = null, string[] includingCountryList = null, string[] excludedCountryList = null, string consolType = null, DateTime? masterBillIssueStartDate = null, DateTime? masterBillIssueEndDate = null, string shipmentType = null, int? period = null, string orderby = "")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_OutstandingARAPTransactionsListing(
{0},	--@Period
'',		--@CurrentCountry
'{1}',	--@Company
'{9}',	--@Branch
'{2}',	--@Ledger
'{3}',	--@CountryList
'{4}',	--@ExCountryList
'{5}',	--@ConsolType
{6},	--@MasterBillIssueStartDate
{7},	--@MasterBillIssueEndDate
'{8}'	--@ShipmentType
)
{10}
",
			period ?? PeriodCalculator.GetPeriodFromDate(ZDateTime.Now),
			GlbCompany.CurrentCompany.PK,
			ledger,
			includingCountryList == null ? "" : new ZStringBuilder(includingCountryList).ToStringWithDelimiterBetweenAppends(","),
			excludedCountryList == null ? "" : new ZStringBuilder(excludedCountryList).ToStringWithDelimiterBetweenAppends(","),
			consolType ?? string.Empty,
			masterBillIssueStartDate.HasValue ? $"'{masterBillIssueStartDate.Value}'" : "NULL",
			masterBillIssueEndDate.HasValue ? $"'{masterBillIssueEndDate.Value}'" : "NULL",
			shipmentType ?? string.Empty,
			GlbBranch.CurrentBranch.PK,
			orderby
			));
		}
		string GetQueryWith(int period, string ledger)
		{
			return $@"SELECT * FROM Report_OutstandingARAPTransactionsListing(
'{period}',									--@Period
'',											--@CurrentCountry
'{GlbCompany.CurrentCompany.PK.ToGuid()}',	--@Company
'{GlbBranch.CurrentBranch.PK.ToGuid()}',	--@Branch
'{ledger}',									--@Ledger
'',											--@CountryList
'',											--@ExCountryList
'',											--@ConsolType
NULL,										--@MasterBillIssueStartDate
NULL,										--@MasterBillIssueEndDate
''											--@ShipmentType
)";
		}
	}
}


