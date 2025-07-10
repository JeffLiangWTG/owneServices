using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	[TestedType(typeof(Voucher))]
	public class VoucherTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T206", Voucher.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Voucher();
		}
	}

	public abstract class VoucherCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : VoucherCollection
	{
		public void TestWIPAccrualVoucher()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var startDate = new ZDateTime(2000, 1, 1);
				var endDate = new ZDateTime(2000, 1, 31, 23, 59, 59);
				new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(200001, startDate, endDate);
				var accrual = testObjectCreator.CreateAccrual();
				accrual.AL_PostDate = new ZDateTime(2000, 1, 31, 23, 59, 59);
				testObjectCreator.CreateAccountDescriptor(accrual.AL_AG, "1000000.1", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "Desc", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				testObjectCreator.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, "88.88.888.8", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "CostControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				Factory.Save();

				var collection = (VoucherCollection)GetCollectionToTest();
				collection.AddElements(startDate, endDate.Date.AddDays(1), ZString.Empty);

				AssertEquals("Should contain 1 + 1 = 2 vouchers", 2, collection.Count);
				AssertEquals(1, collection.Cast<Voucher>().Count(x => x.GLAccountNumber == "88.88.888.8"));
			}
		}

		public void TestCashFlowVoucher()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();

				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31));

				AccGLHeader apControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
				AccGLHeader arControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

				AccGLAccountDescriptor arControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
				arControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
				arControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
				arControlLocal.ParentGLHeaderPK = arControl.PK;
				arControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
				arControlLocal.AJ_AccountDescription = "ARControlDescription";

				AccGLAccountDescriptor apControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
				apControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
				apControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
				apControlLocal.ParentGLHeaderPK = apControl.PK;
				apControlLocal.AJ_LocalAccountNumber = "APControlAccount";
				apControlLocal.AJ_AccountDescription = "APControlDescription";

				AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControl.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControl.PK.ToGuid());

				ARPayment header = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_GE = GlbDepartment.CurrentDepartment.PK;
				header.AH_InvoiceDate = new ZDateTime(2006, 3, 13, 23, 59, 00);
				header.AH_PostDate = new ZDateTime(2006, 3, 13, 23, 59, 00);
				header.AH_Ledger = "AR";

				header.AH_RX_NKTransactionCurrency = "CNY";
				header.AH_ExchangeRate = 1m;
				header.AH_OSExTaxAmount = 10m;
				header.AH_OSTaxAmount = 10m;
				header.AH_OutstandingAmount = 20m;
				header.AH_AB = bank.PK;
				header.AH_AG = arControl.PK;
				Factory.Save();

				var collection = (VoucherCollection)GetCollectionToTest();
				var propertyInfo = typeof(VoucherCollection).GetProperty("AllowCashFlow", BindingFlags.NonPublic | BindingFlags.Instance);
				var value = (bool)propertyInfo.GetValue(collection);
				if (!value)
				{
					Assert(true);
					return;
				}

				collection.FillCashFlowVoucherCollection(new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 4, 1, 0, 0, 0), "");
				AssertEquals(1, collection.Count);

				Voucher voucher = collection[0];
				AssertEquals("20060313", voucher.VoucherDate);
			}
		}

		[TestDate(2006, 3, 20, 10, 30, 1)]
		public void TestBuildTransactionsAR()
		{
			Action prepare = () =>
			{
				var testHelper = new TestObjectCreator(Factory);
				var invoice = testHelper.CreateARInvoice<ARInvoice>("WillBeOverride", testHelper.AUD, 2, testHelper.ABIGAS);
				var line = testHelper.CreateInvoiceLine(invoice, testHelper.AUD, 2, 200, 0, 0, 100, 0, 0);
				invoice.AH_PostDate = new ZDateTime(2006, 3, 13);
				invoice.AH_DueDate = new ZDateTime(2006, 3, 18);
				invoice.AH_TransactionReference = "Invoice No";
				invoice.AH_NumberOfSupportingDocuments = 3;
				invoice.AH_Desc = "Invoice Desc";
				Factory.Save();
			};

			Action<object> assertFunc = (x) =>
			{
				var collection = (VoucherCollection)x;
				AssertEquals(1, collection.Count);
				BuildTransactionARAssertCore(collection[0]);
			};

			BaseTestCreateVoucher(prepare, assertFunc);
		}

		[TestDate(2006, 3, 20, 10, 30, 1)]
		public void TestBuildTransactionsAP()
		{
			Action prepare = () =>
			{
				var testHelper = new TestObjectCreator(Factory);
				var invoice = testHelper.CreateAPInvoice<APInvoice>("100111", testHelper.AUD, 2, 200, 0, 0, 100, 0, 0, testHelper.ABIGAS);
				invoice.AH_PostDate = new ZDateTime(2006, 3, 13);
				invoice.AH_DueDate = new ZDateTime(2006, 3, 18);
				invoice.AH_TransactionReference = "Invoice No";
				invoice.AH_NumberOfSupportingDocuments = 5;
				invoice.AH_Desc = "Invoice Desc";
				Factory.Save();
			};

			Action<object> assert = (x) =>
			{
				var collection = (VoucherCollection)x;
				AssertEquals(1, collection.Count);
				BuildTransactionAPAssertCore(collection[0]);
			};

			BaseTestCreateVoucher(prepare, assert);
		}

		[TestDate(2006, 3, 20, 10, 30, 1)]
		public void TestBuildTransactionsAPWithGST()
		{
			Action prepare = () =>
			{
				var testHelper = new TestObjectCreator(Factory);
				var invoice = testHelper.CreateAPInvoice<APInvoice>("100111", testHelper.AUD, 2, 200, 20, 0, 100, 10, 0, testHelper.ABIGAS);
				invoice.AH_PostDate = new ZDateTime(2006, 3, 13);
				invoice.AH_DueDate = new ZDateTime(2006, 3, 18);
				invoice.AH_TransactionReference = "Invoice No";
				invoice.AH_NumberOfSupportingDocuments = 5;
				invoice.AH_Desc = "Invoice Desc";
				invoice.Lines[0].AL_AT = testHelper.GST1.PK;
				Factory.Save();
			};

			Action<object> assert = (x) =>
			{
				var collection = (VoucherCollection)x;
				BuildTransactionAPWithGSTAssertCore(collection);
			};

			BaseTestCreateVoucher(prepare, assert);
		}

		[TestDate(2006, 3, 20, 10, 30, 1)]
		public void TestBuildTransactionsARWithGST()
		{
			Action prepare = () =>
			{
				var testHelper = new TestObjectCreator(Factory);
				var invoice = testHelper.CreateARInvoice<ARInvoice>("WillBeOverride", testHelper.AUD, 2, testHelper.ABIGAS);
				var line = testHelper.CreateInvoiceLine(invoice, testHelper.AUD, 2, 200, 20, 0, 100, 10, 0);
				invoice.AH_PostDate = new ZDateTime(2006, 3, 13);
				invoice.AH_DueDate = new ZDateTime(2006, 3, 18);
				invoice.AH_TransactionReference = "Invoice No";
				invoice.AH_Desc = "Invoice Desc";
				invoice.AH_NumberOfSupportingDocuments = 3;
				line.AL_AT = testHelper.GST1.PK;
				Factory.Save();
			};

			Action<object> assert = (x) =>
			{
				var collection = (VoucherCollection)x;
				BuildTransactionARWithGSTAssertCore(collection);
			};

			BaseTestCreateVoucher(prepare, assert);
		}

		protected void BaseTestCreateVoucher(Action prepareData, Action<object> assertFunc)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var testHelper = new TestObjectCreator(Factory);

				var aRControlAccount = testHelper.CreateARControlAccount();
				var aPControlAccount = testHelper.CreateAPControlAccount();
				var gSTOutputControlAccount = testHelper.GSTOutputControlAccount();
				var gSTInputControlAccount = testHelper.GSTInputControlAccount();

				var staff = testHelper.CreateStaff("TST");
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
				staff.GS_FullName = "TestClient";

				Factory.Save();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), staff.HomeBranch.PK.ToGuid(), staff.HomeDepartment.PK.ToGuid()))
				using (AccountingConfigurationRegistry.Instance.ARControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControlAccount.PK.ToGuid()))
				using (AccountingConfigurationRegistry.Instance.APControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aPControlAccount.PK.ToGuid()))
				using (AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gSTOutputControlAccount.PK.ToGuid()))
				using (AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gSTInputControlAccount.PK.ToGuid()))
				{
					var language = DataInterfaceUtils.GetLocalLanguage();
					var arControlLocal = testHelper.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.ARControlAccount.Value, "ARControlAccount", AccGLAccountDescriptor.ReportTypeCOA, "", language, "ARControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
					var apControlLocal = testHelper.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.APControlAccount.Value, "APControlAccount", AccGLAccountDescriptor.ReportTypeCOA, "", language, "APControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Credit);
					var gstInputControl = testHelper.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, "GSTInputAccount", AccGLAccountDescriptor.ReportTypeCOA, "", language, "GSTInputAccountDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
					var gstOutputControll = testHelper.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, "GSTOutputAccount", AccGLAccountDescriptor.ReportTypeCOA, "", language, "GSTOutputAccountDescription", Constants.CountryCodes.China, Constants.DebitCredit.Credit);

					Factory.Save();

					var periodHelper = new AccountingPeriodTestHelper(Factory);
					periodHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), (new ZDateTime(2006, 4, 1)).AddSeconds(-1));

					Factory.Save();

					prepareData();

					var collection = (VoucherCollection)GetCollectionToTest();
					collection.AddElements(new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 4, 1, 00, 00, 00), ZString.Empty);

					assertFunc(collection);
				}
			}
		}

		protected virtual void BuildTransactionAPAssertCore(object target)
		{
			var voucher = (Voucher)target;

			AssertEquals("20060313", voucher.VoucherDate);
			AssertEquals(2006, voucher.FinancialYear);
			AssertEquals(200603, voucher.Period);
			AssertEquals("1", voucher.VoucherTypeNumber);
			AssertEquals("APINV0603001000", voucher.VoucherNumber);
			AssertEquals("1", voucher.VoucherLineNumber);
			AssertEquals("营业成本 - Invoice Desc", voucher.VoucherDescription);
			AssertEquals("APControlAccount", voucher.GLAccountNumber);
			AssertEquals("AUD", voucher.CurrencyCode);
			AssertEquals(0m, voucher.DebitCurrencyAmount);
			AssertEquals(0m, voucher.DebitAmountLocalCurrency);
			AssertEquals(200m, voucher.CreditCurrencyAmount);
			AssertEquals(100m, voucher.CreditAmountLocalCurrency);
			AssertEquals("1", voucher.ExRateTypeNumber);
			AssertEquals(2m, voucher.ExRate);
			AssertEquals("", voucher.PaymentTypeCode);
			AssertEquals("APINV", voucher.VoucherType);
			AssertEquals("100111", voucher.VoucherDocNumber);
			AssertEquals("20060313", voucher.VoucherDocDate);
			AssertEquals(5, voucher.Attachments);
			AssertEquals("", voucher.Reviwer);
			AssertEquals("TestClient", voucher.EnteredBy);
			AssertEquals("", voucher.Cashier);
			AssertEquals(1, voucher.AccountingFlag);
			AssertEquals(0, voucher.VoidFlag);
		}

		protected virtual void BuildTransactionARAssertCore(object target)
		{
			var voucher = (Voucher)target;

			AssertEquals("20060313", voucher.VoucherDate);
			AssertEquals(2006, voucher.FinancialYear);
			AssertEquals(200603, voucher.Period);
			AssertEquals("1", voucher.VoucherTypeNumber);
			AssertEquals("ARINV0603001000", voucher.VoucherNumber);
			AssertEquals("1", voucher.VoucherLineNumber);
			AssertEquals("营业收入 - Invoice Desc", voucher.VoucherDescription);
			AssertEquals("ARControlAccount", voucher.GLAccountNumber);
			AssertEquals("AUD", voucher.CurrencyCode);
			AssertEquals(200m, voucher.DebitCurrencyAmount);
			AssertEquals(100m, voucher.DebitAmountLocalCurrency);
			AssertEquals(0m, voucher.CreditCurrencyAmount);
			AssertEquals(0m, voucher.CreditAmountLocalCurrency);
			AssertEquals("1", voucher.ExRateTypeNumber);
			AssertEquals(2m, voucher.ExRate);
			AssertEquals("", voucher.PaymentTypeCode);
			AssertEquals("ARINV", voucher.VoucherType);
			AssertEquals("Invoice No", voucher.VoucherDocNumber);
			AssertEquals("20060313", voucher.VoucherDocDate);
			AssertEquals(3, voucher.Attachments);
			AssertEquals("", voucher.Reviwer);
			AssertEquals("TestClient", voucher.EnteredBy);
			AssertEquals("", voucher.Cashier);
			AssertEquals(1, voucher.AccountingFlag);
			AssertEquals(0, voucher.VoidFlag);
		}

		protected virtual void BuildTransactionAPWithGSTAssertCore(object target)
		{
			var collection = (VoucherCollection)target;

			AssertEquals(2, collection.Count);
			AssertEquals(1, collection.Count(y => ((Voucher)y).GLAccountNumber == "GSTInputAccount"));
			AssertEquals(1, collection.Count(y => ((Voucher)y).GLAccountNumber == "APControlAccount"));

			var voucher = (Voucher)collection.Single(y => ((Voucher)y).GLAccountNumber == "GSTInputAccount");

			AssertEquals("20060313", voucher.VoucherDate);
			AssertEquals(2006, voucher.FinancialYear);
			AssertEquals(200603, voucher.Period);
			AssertEquals("1", voucher.VoucherTypeNumber);
			AssertEquals("APINV0603001000", voucher.VoucherNumber);
			AssertEquals("1", voucher.VoucherLineNumber);
			AssertEquals("营业成本 - Invoice Desc", voucher.VoucherDescription);
			AssertEquals("GSTInputAccount", voucher.GLAccountNumber);
			AssertEquals(AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value, voucher.AccountPK);
			AssertEquals(20m, voucher.DebitCurrencyAmount);
			AssertEquals(10m, voucher.DebitAmountLocalCurrency);
			AssertEquals(0m, voucher.CreditCurrencyAmount);
			AssertEquals(0m, voucher.CreditAmountLocalCurrency);
			AssertEquals("AUD", voucher.CurrencyCode);
			AssertEquals("1", voucher.ExRateTypeNumber);
			AssertEquals(2m, voucher.ExRate);
			AssertEquals("", voucher.PaymentTypeCode);
			AssertEquals("APINV", voucher.VoucherType);
			AssertEquals("100111", voucher.VoucherDocNumber);
			AssertEquals("20060313", voucher.VoucherDocDate);
			AssertEquals(5, voucher.Attachments);
			AssertEquals("", voucher.Reviwer);
			AssertEquals("TestClient", voucher.EnteredBy);
			AssertEquals("", voucher.Cashier);
			AssertEquals(1, voucher.AccountingFlag);
			AssertEquals(0, voucher.VoidFlag);

			var apControlVoucher = (Voucher)collection.Single(y => ((Voucher)y).GLAccountNumber == "APControlAccount");

			AssertEquals("20060313", apControlVoucher.VoucherDate);
			AssertEquals(2006, apControlVoucher.FinancialYear);
			AssertEquals(200603, apControlVoucher.Period);
			AssertEquals("1", apControlVoucher.VoucherTypeNumber);
			AssertEquals("APINV0603001000", apControlVoucher.VoucherNumber);
			AssertEquals("2", apControlVoucher.VoucherLineNumber);
			AssertEquals("营业成本 - Invoice Desc", apControlVoucher.VoucherDescription);
			AssertEquals("APControlAccount", apControlVoucher.GLAccountNumber);
			AssertEquals("AUD", apControlVoucher.CurrencyCode);
			AssertEquals(0m, apControlVoucher.DebitCurrencyAmount);
			AssertEquals(0m, apControlVoucher.DebitAmountLocalCurrency);
			AssertEquals(220m, apControlVoucher.CreditCurrencyAmount);
			AssertEquals(110m, apControlVoucher.CreditAmountLocalCurrency);
			AssertEquals("1", apControlVoucher.ExRateTypeNumber);
			AssertEquals(2m, apControlVoucher.ExRate);
			AssertEquals("", apControlVoucher.PaymentTypeCode);
			AssertEquals("APINV", apControlVoucher.VoucherType);
			AssertEquals("100111", apControlVoucher.VoucherDocNumber);
			AssertEquals("20060313", apControlVoucher.VoucherDocDate);
			AssertEquals(5, apControlVoucher.Attachments);
			AssertEquals("", apControlVoucher.Reviwer);
			AssertEquals("TestClient", apControlVoucher.EnteredBy);
			AssertEquals("", apControlVoucher.Cashier);
			AssertEquals(1, apControlVoucher.AccountingFlag);
			AssertEquals(0, apControlVoucher.VoidFlag);
		}

		protected virtual void BuildTransactionARWithGSTAssertCore(object target)
		{
			var collection = (VoucherCollection)target;

			AssertEquals(2, collection.Count);
			AssertEquals(1, collection.Count(y => ((Voucher)y).GLAccountNumber == "GSTOutputAccount"));
			AssertEquals(1, collection.Count(y => ((Voucher)y).GLAccountNumber == "ARControlAccount"));

			var voucher = (Voucher)collection.Single(y => ((Voucher)y).GLAccountNumber == "GSTOutputAccount");

			AssertEquals("20060313", voucher.VoucherDate);
			AssertEquals(2006, voucher.FinancialYear);
			AssertEquals(200603, voucher.Period);
			AssertEquals("1", voucher.VoucherTypeNumber);
			AssertEquals("ARINV0603001000", voucher.VoucherNumber);
			AssertEquals("2", voucher.VoucherLineNumber);
			AssertEquals("营业收入 - Invoice Desc", voucher.VoucherDescription);
			AssertEquals("GSTOutputAccount", voucher.GLAccountNumber);
			AssertEquals(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value, voucher.AccountPK);
			AssertEquals(0m, voucher.DebitCurrencyAmount);
			AssertEquals(0m, voucher.DebitAmountLocalCurrency);
			AssertEquals(20m, voucher.CreditCurrencyAmount);
			AssertEquals(10m, voucher.CreditAmountLocalCurrency);
			AssertEquals("AUD", voucher.CurrencyCode);
			AssertEquals("1", voucher.ExRateTypeNumber);
			AssertEquals(2m, voucher.ExRate);
			AssertEquals("", voucher.PaymentTypeCode);
			AssertEquals("ARINV", voucher.VoucherType);
			AssertEquals("Invoice No", voucher.VoucherDocNumber);
			AssertEquals("20060313", voucher.VoucherDocDate);
			AssertEquals(3, voucher.Attachments);
			AssertEquals("", voucher.Reviwer);
			AssertEquals("TestClient", voucher.EnteredBy);
			AssertEquals("", voucher.Cashier);
			AssertEquals(1, voucher.AccountingFlag);
			AssertEquals(0, voucher.VoidFlag);

			var arControlVoucher = (Voucher)collection.Single(y => ((Voucher)y).GLAccountNumber == "ARControlAccount");

			AssertEquals("20060313", arControlVoucher.VoucherDate);
			AssertEquals(2006, arControlVoucher.FinancialYear);
			AssertEquals(200603, arControlVoucher.Period);
			AssertEquals("1", arControlVoucher.VoucherTypeNumber);
			AssertEquals("ARINV0603001000", arControlVoucher.VoucherNumber);
			AssertEquals("1", arControlVoucher.VoucherLineNumber);
			AssertEquals("营业收入 - Invoice Desc", arControlVoucher.VoucherDescription);
			AssertEquals("ARControlAccount", arControlVoucher.GLAccountNumber);
			AssertEquals("AUD", arControlVoucher.CurrencyCode);
			AssertEquals(220m, arControlVoucher.DebitCurrencyAmount);
			AssertEquals(110m, arControlVoucher.DebitAmountLocalCurrency);
			AssertEquals(0m, arControlVoucher.CreditCurrencyAmount);
			AssertEquals(0m, arControlVoucher.CreditAmountLocalCurrency);
			AssertEquals("1", arControlVoucher.ExRateTypeNumber);
			AssertEquals(2m, arControlVoucher.ExRate);
			AssertEquals("", arControlVoucher.PaymentTypeCode);
			AssertEquals("ARINV", arControlVoucher.VoucherType);
			AssertEquals("Invoice No", arControlVoucher.VoucherDocNumber);
			AssertEquals("20060313", arControlVoucher.VoucherDocDate);
			AssertEquals(3, arControlVoucher.Attachments);
			AssertEquals("", arControlVoucher.Reviwer);
			AssertEquals("TestClient", arControlVoucher.EnteredBy);
			AssertEquals("", arControlVoucher.Cashier);
			AssertEquals(1, arControlVoucher.AccountingFlag);
			AssertEquals(0, arControlVoucher.VoidFlag);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Voucher();
		}

		#endregion
	}

	[TestedType(typeof(VoucherCollection))]
	public class VoucherCollectionTest : VoucherCollectionTest<VoucherCollection>
	{
		protected override VoucherCollection GetCollectionToTest()
		{
			return new VoucherCollection(Factory);
		}
	}
}
