using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	public class InvoiceReconciliationExportAdapterTest : TestCaseWithFactory
	{
		public void TestInvoiceReconciliationNotIncludeWIPAccrualVoucher()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 31));

				var testObjectCreator = new TestObjectCreator(Factory);
				var accrual = testObjectCreator.CreateAccrual();
				accrual.AL_PostDate = new ZDateTime(2016, 01, 15);
				testObjectCreator.CreateAccountDescriptor(accrual.AL_AG, "1000000.1", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "Desc", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				testObjectCreator.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, "88.88.888.8", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "CostControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				Factory.Save();

				var wrapper = new ChinaReconciliationExportWrapper();
				wrapper.PostDateFrom = new ZDate(2016, 1, 1);
				wrapper.PostDateTo = new ZDate(2016, 1, 31);
				wrapper.ComplianceSubType = "ALL";
				wrapper.ExportStatus = "BTH";
				var adapter = new InvoiceReconciliationExportAdapter(Factory, wrapper);

				var listObjects = adapter.GetBusinessObjectsForExport();
				AssertEquals(0, listObjects.Count());
			}
		}

		public void TestInvoiceReconciliationLogs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				PrepareTestEnvironment();

				ChinaReconciliationExportWrapper wrapper = new ChinaReconciliationExportWrapper();
				wrapper.PostDateFrom = new ZDate(2016, 1, 1);
				wrapper.PostDateTo = new ZDate(2016, 1, 31);
				wrapper.ComplianceSubType = "ALL";
				wrapper.ExportStatus = "BTH";
				InvoiceReconciliationExportAdapter adapter = new InvoiceReconciliationExportAdapter(Factory, wrapper);

				var listObjects = adapter.GetBusinessObjectsForExport();
				listObjects.ForEach(x => AssertType<InvoiceReconciliation>(x));

				AssertEquals(2, listObjects.Count());

				adapter.AddEventForExportedTransaction();
				var arInvoice = Factory.CreateNewFactory().Load<ARInvoice>(testArInvoice.PK);
				Assert(arInvoice.Logs.HasLogWith(c => c.SL_SE_NKEvent == AutoEvents.DataExportCode));

				var apInvoice = Factory.CreateNewFactory().Load<APInvoice>(testApInvoice.PK);
				Assert(apInvoice.Logs.HasLogWith(c => c.SL_SE_NKEvent == AutoEvents.DataExportCode));
			}
		}

		public void TestGLMappingNotifications()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				PrepareTestEnvironment();

				var wrapper = new ChinaReconciliationExportWrapper();
				wrapper.PostDateFrom = new ZDate(2016, 1, 1);
				wrapper.PostDateTo = new ZDate(2016, 1, 31);
				wrapper.ComplianceSubType = "ALL";
				wrapper.ExportStatus = "BTH";
				var adapter = new InvoiceReconciliationExportAdapter(Factory, wrapper);

				var listObjects = adapter.GetBusinessObjectsForExport();
				AssertEquals(2, listObjects.Count());
				Assert(string.IsNullOrEmpty(adapter.Notification));

				apControlLocal.Delete();
				Factory.Save();

				listObjects = adapter.GetBusinessObjectsForExport();
				AssertEquals(1, listObjects.Count());
				AssertEquals(@"The Invoice Reconciliation Export cannot be completed as there are GL Accounts used in posted transactions that have not been properly mapped.

Please create  GL Mappings for the following Parent GL Accounts before running the export.

Parent GL Accounts without mapping:
10010000", adapter.Notification);
			}
		}

		public void TestNotifications()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				PrepareTestEnvironment();

				var wrapper = new ChinaReconciliationExportWrapper();
				wrapper.PostDateFrom = new ZDate(2016, 1, 1);
				wrapper.PostDateTo = new ZDate(2016, 1, 31);
				wrapper.ComplianceSubType = "ALL";
				wrapper.ExportStatus = "BTH";
				var adapter = new InvoiceReconciliationExportAdapter(Factory, wrapper);

				var listObjects = adapter.GetBusinessObjectsForExport();
				AssertEquals(2, listObjects.Count());
				Assert(string.IsNullOrEmpty(adapter.Notification));

				adapter.ReconciliationVoucherCollectionExposedForTest.Notifications.Add(new ValidationInfo() { Message = "TestMessage", Key = VoucherLineErrorType.NoVoucherDescription });

				AssertEquals(@"Some voucher lines are invalid. Please fix them before running the export:
TestMessage", adapter.Notification);
			}
		}

		public void TestNewVoucherLineErrorTypesForNotification()
		{
			// Add new VoucherLine error types you want to exclude from default display group here.
			var specialErrorTypes = new VoucherLineErrorType[] { VoucherLineErrorType.LocalGLNotMapped };
			// Add new VoucherLine error types you want to show in default display group here.
			var standardErrorTypes = new VoucherLineErrorType[] { VoucherLineErrorType.NoVoucherDescription };

			var wrapper = new ChinaReconciliationExportWrapper();
			wrapper.PostDateFrom = new ZDate(2016, 1, 1);
			wrapper.PostDateTo = new ZDate(2016, 1, 31);
			wrapper.ComplianceSubType = "ALL";
			wrapper.ExportStatus = "BTH";
			var adapter = new InvoiceReconciliationExportAdapter(Factory, wrapper);
			adapter.GetBusinessObjectsForExport();

			Assert(string.IsNullOrEmpty(adapter.Notification));

			foreach (VoucherLineErrorType enumValue in Enum.GetValues(typeof(VoucherLineErrorType)))
			{
				if (specialErrorTypes.Contains(enumValue))
				{ continue; }
				adapter.ReconciliationVoucherCollectionExposedForTest.Notifications.Add(new ValidationInfo() { Message = enumValue.ToString(), Key = enumValue });
			}

			var builder = new ZStringBuilder(@"Some voucher lines are invalid. Please fix them before running the export:");
			foreach (var enumValue in standardErrorTypes)
			{
				builder.AppendLine().Append(enumValue.ToString());
			}

			AssertEquals(@"Standard VoucherLineErrorType should be displayed in the same notification message.
When adding a new VoucherLineErrorType:
 * If you want to display the new error notification in default group, please add it into 'standardErrorTypes' collection.
 * If you already had the error notification displayed in its own group and want to exclude it from default group, please add it into 'specialErrorTypes' collection.", builder.ToString(), adapter.Notification);
		}

		#region Implementation

		ARInvoice testArInvoice;
		APInvoice testApInvoice;
		AccGLAccountDescriptor arControlLocal;
		AccGLAccountDescriptor apControlLocal;

		void PrepareTestEnvironment()
		{
			new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(201601, new ZDateTime(2016, 1, 1), new ZDateTime(2016, 1, 31));

			AccGLHeader apControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			apControl.AG_AccountNum = "10010000";
			AccGLHeader arControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			arControl.AG_AccountNum = "10100000";

			arControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			arControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			arControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			arControlLocal.ParentGLHeaderPK = arControl.PK;
			arControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
			arControlLocal.AJ_AccountDescription = "ARControlDescription";

			apControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			apControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			apControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			apControlLocal.ParentGLHeaderPK = apControl.PK;
			apControlLocal.AJ_LocalAccountNumber = "APControlAccount";
			apControlLocal.AJ_AccountDescription = "APControlDescription";

			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControl.PK.ToGuid());

			testArInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testArInvoice.AH_TransactionNum = "100111";
			testArInvoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
			testArInvoice.AH_PostDate = new ZDateTime(2016, 1, 13);
			testArInvoice.AH_DueDate = new ZDateTime(2016, 1, 18);
			testArInvoice.AH_TransactionReference = "Invoice No";
			testArInvoice.AH_ExchangeRate = 1m;
			testArInvoice.AH_InvoiceAmount = 111m;
			testArInvoice.AH_OutstandingAmount = 111m;
			testArInvoice.AH_Desc = "Invoice Desc";
			testArInvoice.AH_ConsolidatedInvoiceRef = "AR1001";

			testApInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
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
			testApInvoice.AH_ConsolidatedInvoiceRef = "AP1000";

			testArInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			testApInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;

			testArInvoice.AH_TransactionNum = "100111";
			testApInvoice.AH_TransactionNum = "100222";
			Factory.Save();

			testArInvoice.AH_ConsolidatedInvoiceRef = "AR1001";
			testApInvoice.AH_ConsolidatedInvoiceRef = "AP1000";

			Factory.Save();
		}

		#endregion
	}
}
