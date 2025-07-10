using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	[TestedType(typeof(VoucherKingDeeK3))]
	public class VoucherKingDee3Test : VoucherTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoucherKingDeeK3();
		}
	}

	[TestedType(typeof(InvoiceReconciliationVoucherCollection))]
	public class InvoiceReconciliationVoucherCollectionTest : VoucherCollectionTest<InvoiceReconciliationVoucherCollection>
	{
		public void TestInvoiceReconciliationCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.Australia))
			{
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 31));

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

				ARInvoice testArInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
				testArInvoice.AH_TransactionNum = "100111";
				testArInvoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
				testArInvoice.AH_PostDate = new ZDateTime(2016, 1, 13);
				testArInvoice.AH_DueDate = new ZDateTime(2016, 1, 18);
				testArInvoice.AH_TransactionReference = "Invoice No";
				testArInvoice.AH_ExchangeRate = 1m;
				testArInvoice.AH_InvoiceAmount = 111m;
				testArInvoice.AH_OutstandingAmount = 111m;
				testArInvoice.AH_Desc = "Invoice Desc";

				APInvoice testApInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
				testApInvoice.AH_TransactionNum = "100222";
				testApInvoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
				testApInvoice.AH_PostDate = new ZDateTime(2016, 1, 13);
				testApInvoice.AH_DueDate = new ZDateTime(2016, 1, 18);
				testApInvoice.AH_TransactionReference = "Invoice No";
				testApInvoice.AH_ExchangeRate = 1m;
				testApInvoice.AH_InvoiceAmount = 111m;
				testApInvoice.AH_OutstandingAmount = 111m;
				testApInvoice.AH_Desc = "Invoice Desc";
				testApInvoice.AH_ComplianceSubType = "TXA";

				Factory.Save();

				InvoiceReconciliationVoucherCollection collection = new InvoiceReconciliationVoucherCollection(Factory, "ALL", "BTH");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(2, collection.Count);

				collection = new InvoiceReconciliationVoucherCollection(Factory, "ALL", "NEX");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(2, collection.Count);

				collection = new InvoiceReconciliationVoucherCollection(Factory, "ALL", "PEX");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(0, collection.Count);

				collection = new InvoiceReconciliationVoucherCollection(Factory, "TXA", "BTH");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(1, collection.Count);

				collection = new InvoiceReconciliationVoucherCollection(Factory, "TXB", "BTH");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(0, collection.Count);

				testApInvoice.AH_ComplianceSubType = "TXB";
				Factory.Save();

				collection = new InvoiceReconciliationVoucherCollection(Factory, "TXB", "BTH");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(1, collection.Count);

				collection = new InvoiceReconciliationVoucherCollection(Factory, "NTX", "BTH");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(1, collection.Count);

				testArInvoice.AH_ComplianceSubType = "TXA";
				Factory.Save();
				collection = new InvoiceReconciliationVoucherCollection(Factory, "TAX", "BTH");
				collection.AddElements(new ZDate(2016, 1, 1), new ZDate(2016, 2, 1), "");
				AssertEquals(2, collection.Count);
			}
		}

		protected override void BuildTransactionAPAssertCore(object target)
		{
			base.BuildTransactionAPAssertCore(target);

			var voucher = (VoucherKingDeeK3)target;

			AssertEquals(new ZDateTime(2006, 3, 13), voucher.PostDate);
			AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), voucher.InvoiceDate);
			AssertEquals("供应商---ABIGAS---ABI GAS & TOOLS", voucher.AccountingItem);
			AssertEquals(1, voucher.TransactionIndex);
			AssertEquals(200m, voucher.CalculatedAmount);
			AssertEquals("AUD", voucher.CalculatedCurrencyCode);
			AssertEquals(2m, voucher.CalculatedExchangeRate);
		}

		protected override void BuildTransactionARAssertCore(object target)
		{
			base.BuildTransactionARAssertCore(target);

			var voucher = (VoucherKingDeeK3)target;

			AssertEquals(new ZDateTime(2006, 3, 13), voucher.PostDate);
			AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), voucher.InvoiceDate);
			AssertEquals("客户---ABIGAS---ABI GAS & TOOLS", voucher.AccountingItem);
			AssertEquals(1, voucher.TransactionIndex);
			AssertEquals(200m, voucher.CalculatedAmount);
			AssertEquals("AUD", voucher.CalculatedCurrencyCode);
			AssertEquals(2m, voucher.CalculatedExchangeRate);
		}

		protected override void BuildTransactionAPWithGSTAssertCore(object target)
		{
			base.BuildTransactionAPWithGSTAssertCore(target);

			var collection = (InvoiceReconciliationVoucherCollection)target;
			var voucher = (VoucherKingDeeK3)collection.Single(y => ((VoucherKingDeeK3)y).GLAccountNumber == "GSTInputAccount");

			AssertEquals(new ZDateTime(2006, 3, 13), voucher.PostDate);
			AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), voucher.InvoiceDate);
			AssertEquals(string.Empty, voucher.AccountingItem);
			AssertEquals(1, voucher.TransactionIndex);
			AssertEquals(10m, voucher.CalculatedAmount);
			AssertEquals("CNY", voucher.CalculatedCurrencyCode);
			AssertEquals(VoucherKingDeeK3.InvalidExchangeRate, voucher.CalculatedExchangeRate);

			var apControlVoucher = (VoucherKingDeeK3)collection.Single(y => ((VoucherKingDeeK3)y).GLAccountNumber == "APControlAccount");

			AssertEquals(new ZDateTime(2006, 3, 13), apControlVoucher.PostDate);
			AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), apControlVoucher.InvoiceDate);
			AssertEquals("供应商---ABIGAS---ABI GAS & TOOLS", apControlVoucher.AccountingItem);
			AssertEquals(1, apControlVoucher.TransactionIndex);
			AssertEquals(220m, apControlVoucher.CalculatedAmount);
			AssertEquals("AUD", apControlVoucher.CalculatedCurrencyCode);
			AssertEquals(2m, apControlVoucher.CalculatedExchangeRate);
		}

		protected override void BuildTransactionARWithGSTAssertCore(object target)
		{
			base.BuildTransactionARWithGSTAssertCore(target);

			var collection = (InvoiceReconciliationVoucherCollection)target;
			var voucher = (VoucherKingDeeK3)collection.Single(y => ((VoucherKingDeeK3)y).GLAccountNumber == "GSTOutputAccount");

			AssertEquals(new ZDateTime(2006, 3, 13), voucher.PostDate);
			AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), voucher.InvoiceDate);
			AssertEquals(string.Empty, voucher.AccountingItem);
			AssertEquals(1, voucher.TransactionIndex);
			AssertEquals(10m, voucher.CalculatedAmount);
			AssertEquals("CNY", voucher.CalculatedCurrencyCode);
			AssertEquals(VoucherKingDeeK3.InvalidExchangeRate, voucher.CalculatedExchangeRate);

			var apControlVoucher = (VoucherKingDeeK3)collection.Single(y => ((VoucherKingDeeK3)y).GLAccountNumber == "ARControlAccount");

			AssertEquals(new ZDateTime(2006, 3, 13), apControlVoucher.PostDate);
			AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), apControlVoucher.InvoiceDate);
			AssertEquals("客户---ABIGAS---ABI GAS & TOOLS", apControlVoucher.AccountingItem);
			AssertEquals(1, apControlVoucher.TransactionIndex);
			AssertEquals(220m, apControlVoucher.CalculatedAmount);
			AssertEquals("AUD", apControlVoucher.CalculatedCurrencyCode);
			AssertEquals(2m, apControlVoucher.CalculatedExchangeRate);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new VoucherKingDeeK3();
		}

		protected override InvoiceReconciliationVoucherCollection GetCollectionToTest()
		{
			return new InvoiceReconciliationVoucherCollection(Factory);
		}
	}
}
