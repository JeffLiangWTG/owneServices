using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	[TestedType(typeof(InvoiceReconciliation))]
	public class InvoiceReconciliationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceReconciliation(Factory);
		}

		public void TestFormatGLAccountNum()
		{
			var invoiceReconcil = GetNewBusinessObject();
			var methodInfo = typeof(InvoiceReconciliation).GetMethod("FormatGLAccountNum", BindingFlags.NonPublic | BindingFlags.Instance);

			AssertEquals("1100", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11000000") }));
			AssertEquals("1110", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11100000") }));
			AssertEquals("1111", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11110000") }));
			AssertEquals("111110", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11111000") }));
			AssertEquals("111111", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11111100") }));
			AssertEquals("11111110", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11111110") }));
			AssertEquals("11111111", methodInfo.Invoke(invoiceReconcil, new object[] { new ZString("11111111") }));
			AssertEquals(string.Empty, methodInfo.Invoke(invoiceReconcil, new object[] { null }));
		}

		[TestDate(2006, 3, 20, 10, 30, 1)]
		public void TestSetReconciliationValue()
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

					var invoice = testHelper.CreateAPInvoice<APInvoice>("100111", testHelper.AUD, 2, 200, 0, 0, 100, 0, 0, testHelper.ABIGAS);
					invoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
					invoice.AH_PostDate = new ZDateTime(2006, 3, 13);
					invoice.AH_DueDate = new ZDateTime(2006, 3, 18);
					invoice.AH_TransactionReference = "Invoice No";
					invoice.AH_Desc = "Invoice Desc";
					invoice.AH_NumberOfSupportingDocuments = 4;

					invoice.AH_AB = testHelper.AUDBankAccount.PK;

					var jobCharge = testHelper.CreateJobCharge(invoice.Lines[0], testHelper.Job1, testHelper.CC1, testHelper.AUD);
					Factory.Save();

					var wrapper = new ChinaReconciliationExportWrapper();
					wrapper.PostDateFrom = new ZDate(2006, 3, 13);
					wrapper.PostDateTo = new ZDate(2006, 3, 18);
					wrapper.ComplianceSubType = "ALL";
					wrapper.ExportStatus = "BTH";
					wrapper.Branch = GlbBranch.CurrentBranch.PK;

					var collection = new InvoiceReconciliationVoucherCollection(Factory, wrapper.ComplianceSubType, wrapper.ExportStatus);
					collection.FillCashFlowVoucherCollection(new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2016, 3, 31, 23, 59, 00), "");
					collection.AddElements(wrapper.PostDateFrom, wrapper.PostDateTo.AddDays(1), wrapper.BranchCode);

					var voucher = (VoucherKingDeeK3)collection[0];
					var reconciliation = (InvoiceReconciliation)GetNewBusinessObject();
					reconciliation.SetReconciliationValue(voucher);

					AssertEquals("20060313", reconciliation.VoucherDate);
					AssertEquals("20060313", reconciliation.TransDate);
					AssertEquals(3, reconciliation.Period);
					AssertEquals("1", reconciliation.VoucherTypeNumber);
					AssertEquals("APINV0603001000", reconciliation.VoucherNumber);
					AssertEquals("1", reconciliation.VoucherLineNumber);
					AssertEquals("营业成本 - Invoice Desc", reconciliation.VoucherDescription);
					AssertEquals("100111", reconciliation.VoucherDocNumber);
					AssertEquals("AUD", reconciliation.CurrencyCode);
					AssertEquals(2m, reconciliation.ExchangeRate);
					AssertEquals("C", reconciliation.DebitOrCredit);
					AssertEquals(100m, reconciliation.LocalAmount);
					AssertEquals(0m, reconciliation.DebitCurrencyAmount);
					AssertEquals(200m, reconciliation.CreditCurrencyAmount);
					AssertEquals("", reconciliation.BankDeposit);
					AssertEquals("TestClient", reconciliation.PreparedBy);
					AssertEquals("", reconciliation.CheckedBy);
					AssertEquals(4, reconciliation.Attachments);
					AssertEquals(new ZDateTime(2006, 3, 13), reconciliation.PostDate);
					AssertEquals(new ZDateTime(2006, 3, 20, 10, 30, 1), reconciliation.InvoiceDate);
					AssertEquals("供应商---AALSHI---A.A.L. SHIPPING AGENCIES P/L", reconciliation.AccountingItem);
					AssertEquals(2006, reconciliation.Year);
					AssertEquals(1, reconciliation.TransactionIndex);
					AssertEquals("APControlAccount", reconciliation.GLAccountNumber);
					AssertEquals("AUD", reconciliation.CalculatedCurrencyCode);
					AssertEquals(0m, reconciliation.CalculatedDebitAmount);
					AssertEquals(100m, reconciliation.CalculatedCreditAmount);
					AssertEquals(200m, reconciliation.CalculatedAmount);
					AssertEquals(2m, reconciliation.CalculatedExchangeRate);
				}
			}
		}

		[TestDate(2006, 3, 20, 10, 30, 1)]
		public void TestAddNewElementWithoutBankAccount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var testHelper = new TestObjectCreator(Factory);

				var staff = testHelper.CreateStaff("TST");
				staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
				staff.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;
				staff.GS_FullName = "TestClient";

				Factory.Save();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), staff.HomeBranch.PK.ToGuid(), staff.HomeDepartment.PK.ToGuid()))
				using (AccountingConfigurationRegistry.Instance.ARControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
				using (AccountingConfigurationRegistry.Instance.APControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
				using (AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
				using (AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
				{
					var periodHelper = new AccountingPeriodTestHelper(Factory);
					periodHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), (new ZDateTime(2006, 4, 1)).AddSeconds(-1));
					Factory.Save();

					var invoice = testHelper.CreateAPInvoice<APInvoice>("100111", testHelper.AUD, 1, 100, 0, 0, 100, 0, 0, testHelper.ABIGAS);
					invoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
					invoice.AH_PostDate = new ZDateTime(2006, 3, 13);
					invoice.AH_DueDate = new ZDateTime(2006, 3, 18);
					invoice.AH_TransactionReference = "Invoice No";
					invoice.AH_Desc = "Invoice Desc";

					var jobCharge = testHelper.CreateJobCharge(invoice.Lines[0], testHelper.Job1, testHelper.CC1, testHelper.AUD);
					Factory.Save();

					var wrapper = new ChinaReconciliationExportWrapper();
					wrapper.PostDateFrom = new ZDate(2006, 3, 13);
					wrapper.PostDateTo = new ZDate(2006, 3, 18);
					wrapper.ComplianceSubType = "ALL";
					wrapper.ExportStatus = "BTH";
					wrapper.Branch = GlbBranch.CurrentBranch.PK;

					var collection = new InvoiceReconciliationVoucherCollection(Factory, wrapper.ComplianceSubType, wrapper.ExportStatus);
					collection.FillCashFlowVoucherCollection(new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2016, 3, 31, 23, 59, 00), "");
					collection.AddElements(wrapper.PostDateFrom, wrapper.PostDateTo.AddDays(1), wrapper.BranchCode);

					AssertEquals(0, collection.Count);
				}
			}
		}
	}
}
