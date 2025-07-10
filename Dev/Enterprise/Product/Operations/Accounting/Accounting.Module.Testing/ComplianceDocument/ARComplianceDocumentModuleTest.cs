using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARComplianceDocumentModule))]
	public class ARComplianceDocumentModuleTest : ComplianceDocumentModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ARComplianceDocument;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			return factory.NewWithValidTestData<ARComplianceDocumentHeader>();
		}

		protected override AccComplianceDocumentHeader NewComplianceDocumentHeader()
		{
			return Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
		}

		protected override InvoicingBase CreateInvoice()
		{
			return TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		protected override SecurityCheckpoint CheckpointForVoid
		{
			get { return Env.Security.VoidReceivablesComplianceDocuments; }
		}

		#region EInvoicing Requeue test cases

		public void TestResetStatusToQueuedMenuItemVisibility()
		{
			Assert("Registry is off by default", !AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var resetStatusToQueuedMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
				AssertNull("'Reset Status to Queued' Menu Item should not be visible when 'Enable E-Reporting Functionality' regsitry is off", resetStatusToQueuedMenuItem);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert("Registry is on", AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);
				var resetStatusToQueuedMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
				AssertNotNull("'Reset Status to Queued' Menu Item should be visible when 'Enable E-Reporting Functionality' regsitry is on", resetStatusToQueuedMenuItem);
			}
		}

		public void TestWhenAllSelectedComplianceDocumentsHaveNoErrors()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var resetStatusToQueuedMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
				AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItem);

				module.PerformSearch_ForTest();
				var collection = module.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("grid is empty", 0, collection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				resetStatusToQueuedMenuItem.PerformClick();
				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				var complianceDocument1 = CreateINVComplianceDocumentHeader();
				complianceDocument1.ADH_DocumentNumber = "AA00000001";
				complianceDocument1.ADH_ComplianceSubType = "TXE";
				complianceDocument1.ADH_DocumentDate = ZDate.Today;
				complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				var complianceDocument2 = CreateINVComplianceDocumentHeader();
				complianceDocument2.ADH_DocumentNumber = "AA00000002";
				complianceDocument2.ADH_ComplianceSubType = "TCE";
				complianceDocument2.ADH_DocumentDate = ZDate.Today;
				complianceDocument2.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				module.PerformSearch_ForTest();
				collection = module.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two compliance documents in the grid", 2, collection.Count);

				module.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				resetStatusToQueuedMenuItem.PerformClick();
				AssertMultilineASCIIEquals(@"You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.
No compliance documents will be reset.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
			}
		}

		public void TestWhenAllSelectedComplianceDocumentsHaveErrors()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument1 = CreateINVComplianceDocumentHeader();
				complianceDocument1.ADH_DocumentNumber = "AA00000001";
				complianceDocument1.ADH_ComplianceSubType = "TXE";
				complianceDocument1.ADH_DocumentDate = ZDate.Today;
				complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				var complianceDocument2 = CreateINVComplianceDocumentHeader();
				complianceDocument2.ADH_DocumentNumber = "AA00000002";
				complianceDocument2.ADH_ComplianceSubType = "TCE";
				complianceDocument2.ADH_DocumentDate = ZDate.Today;
				complianceDocument2.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = CreateAccEInvoicingBatch();
				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				UpdatePivot(complianceDocument1.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				UpdatePivot(complianceDocument2.PK, EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var resetStatusToQueuedMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
					AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItem);

					module.PerformSearch_ForTest();
					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Two compliance documents in the grid", 2, collection.Count);

					module.DisplayGrid.SelectAllElements();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItem.PerformClick();
					AssertEquals("Compliance Documents were successfully reset.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
					Assert("No warning message is displayed when all selected compliance documents gets requeued", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Only compliance documents that satisfy this criteria will be reset."));

					AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		public void TestWhenSelectedComplianceDocumentsHaveDiscardedBatch()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument1 = CreateINVComplianceDocumentHeader();
				complianceDocument1.ADH_DocumentNumber = "AA00000001";
				complianceDocument1.ADH_ComplianceSubType = "TXE";
				complianceDocument1.ADH_DocumentDate = ZDate.Today;
				complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = CreateAccEInvoicingBatch();
				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				UpdatePivot(complianceDocument1.PK, EInvoicingPivotState.Batched, "", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Batched, "", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var resetStatusToQueuedMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
					AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItem);

					module.PerformSearch_ForTest();
					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("One compliance documents in the grid", 1, collection.Count);

					module.DisplayGrid.SelectAllElements();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItem.PerformClick();
					AssertEquals(@"You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.
No compliance documents will be reset.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					UpdateBatch(batchPK);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItem.PerformClick();
					AssertEquals("Compliance Documents were successfully reset.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);

					AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		public void TestWhenSomeOfTheSelectedComplianceDocumentsHaveNoErrors()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument1 = CreateINVComplianceDocumentHeader();
				complianceDocument1.ADH_DocumentNumber = "AA00000001";
				complianceDocument1.ADH_ComplianceSubType = "TXE";
				complianceDocument1.ADH_DocumentDate = ZDate.Today;
				complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				var complianceDocument2 = CreateINVComplianceDocumentHeader();
				complianceDocument2.ADH_DocumentNumber = "AA00000002";
				complianceDocument2.ADH_ComplianceSubType = "TCE";
				complianceDocument2.ADH_DocumentDate = ZDate.Today;
				complianceDocument2.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = CreateAccEInvoicingBatch();
				UpdatePivot(complianceDocument1.PK, EInvoicingPivotState.Succeed, "", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.False);
				UpdatePivot(complianceDocument2.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var resetStatusToQueuedMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
					AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItem);

					module.PerformSearch_ForTest();
					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Two compliance documents in the grid", 2, collection.Count);

					module.DisplayGrid.SelectAllElements();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItem.PerformClick();
					AssertMultilineASCIIEquals(@"You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.
Only compliance documents that satisfy this criteria will be reset.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[1].WasWarning);
					AssertEquals("Compliance Documents were successfully reset.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					Assert(UnitTestUserNotification.Instance.PreviousMessages[0].WasInformation);

					AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Succeed, ZString.Empty, batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.False);
					AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		public void TestComplianceDocumentPivotStatusConcurrency_WithReloadingComplianceDocumentsInNewFactory()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument1 = CreateINVComplianceDocumentHeader();
				complianceDocument1.ADH_DocumentNumber = "AA00000001";
				complianceDocument1.ADH_ComplianceSubType = "TXE";
				complianceDocument1.ADH_DocumentDate = ZDate.Today;
				complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				var complianceDocument2 = CreateINVComplianceDocumentHeader();
				complianceDocument2.ADH_DocumentNumber = "AA00000002";
				complianceDocument2.ADH_ComplianceSubType = "TCE";
				complianceDocument2.ADH_DocumentDate = ZDate.Today;
				complianceDocument2.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = CreateAccEInvoicingBatch();
				UpdatePivot(complianceDocument1.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				UpdatePivot(complianceDocument2.PK, EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				using (var module = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var resetStatusToQueuedMenuItemInModule1 = module.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
					AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItemInModule1);

					module.PerformSearch_ForTest();
					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Two compliance documents in the grid", 2, collection.Count);

					module.DisplayGrid.SelectAllElements();

					using (var module1 = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
					using (var form1 = new ZForm())
					{
						form1.Controls.Add(module1.EmbeddedControl);
						form1.Show();

						var resetStatusToQueuedMenuItemInModule2 = module1.GetNewActionMenuItems_ForTestOnly().FindByText("Reset Status to Queued");
						AssertNotNull("Reset Status to Queued Menu Item should exist", resetStatusToQueuedMenuItemInModule2);

						module1.PerformSearch_ForTest();
						collection = module1.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertEquals("Two compliance documents in the grid", 2, collection.Count);

						module1.DisplayGrid.SelectAllElements();

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItemInModule2.PerformClick();
						AssertEquals("Compliance Documents were successfully reset.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);

						AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
						AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItemInModule1.PerformClick();
					AssertMultilineASCIIEquals(@"You can only reset compliance documents where E-Reporting pivot status is 'FAL' - Fail or 'BER' - Batched with errors or 'BCH' - Batched and batch status is 'DCD' - Discarded.
No compliance documents will be reset.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		public void TestComplianceDocumentPivotStatusConcurrency_BatchingServiceTaskRunningInBetweenUserOperationsCore()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceDocument1 = CreateINVComplianceDocumentHeader();
				complianceDocument1.ADH_DocumentNumber = "AA00000001";
				complianceDocument1.ADH_ComplianceSubType = "TXE";
				complianceDocument1.ADH_DocumentDate = ZDate.Today;
				complianceDocument1.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument1.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				var complianceDocument2 = CreateINVComplianceDocumentHeader();
				complianceDocument2.ADH_DocumentNumber = "AA00000002";
				complianceDocument2.ADH_ComplianceSubType = "TCE";
				complianceDocument2.ADH_DocumentDate = ZDate.Today;
				complianceDocument2.ADH_ReportingPeriod = ZDateTime.Today.Year * 100 + ZDateTime.Today.Month;
				complianceDocument2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
				Factory.Save();

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				var batchPK = CreateAccEInvoicingBatch();
				UpdatePivot(complianceDocument1.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				UpdatePivot(complianceDocument2.PK, EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Failed, "This compliance document was rejected by IIS site", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);
				AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.BatchedWithError, "This compliance document is batched with errors", batchPK, ZDateTime.Today, ZDateTime.Today, ZBool.True);

				using (var testModule = new ARComplianceDocumentModuleForConcurrencyTesting())
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)testModule).PerformSearch();
					AssertEquals("Two compliance documents in the grid", 2, testModule.GridCollection.Count);
					testModule.DisplayGrid.SelectAllElements();
					AssertEquals("Two compliance documents selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandleResetStatusToQueued_ForTestOnly(this, EventArgs.Empty);
					AssertEquals("While you were working, another user has modified these compliance documents. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					var newBatchPK = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, SQLComparisonOperator.NotEqual, batchPK)).PK;
					AssertPivotDetails(complianceDocument1.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					AssertPivotDetails(complianceDocument2.PK, EInvoicingPivotState.Batched, ZString.Empty, newBatchPK, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		protected static void StimulateAnotherUserRequeuingAndServiceTaskRun(List<AccEInvoicingTransactionPivot> complianceDocumentPivotsToRequeue)
		{
			//another user requeues Compliance Document
			var anotherUserFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pivotsInAnotherUserFactory = anotherUserFactory.Load<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.PK, complianceDocumentPivotsToRequeue.Select(x => x.PK)));
			foreach (var pivot in pivotsInAnotherUserFactory)
			{
				var batch = anotherUserFactory.Load<AccEInvoicingBatch>(pivot.AIP_AIB);
				batch.AIB_Status = EInvoicingBatchState.Discarded;
				pivot.AIP_AIB = ZGuid.Empty;
				pivot.AIP_Status = EInvoicingPivotState.Queued;
				pivot.AIP_ErrorDescription = ZString.Empty;
				pivot.AIP_IsNotifiedByEmail = false;
				pivot.AIP_LastResponseReceivedUtc = ZDateTime.Empty;
				pivot.AIP_LastSentTimeUtc = ZDateTime.Empty;
			}
			anotherUserFactory.Save();

			//service task runs right after that
			var batchPk = Guid.Empty;
			var insertSQL = "INSERT INTO dbo.AccEInvoicingBatch (AIB_PK, AIB_GC, AIB_BatchNumber, AIB_SystemCreateTimeUtc, AIB_SystemCreateUser) " +
									"OUTPUT INSERTED.AIB_PK " +
									"VALUES (newid(), @companyPK, @batchNo, @createdTime, @createdUser)";

			using (var insertCommand = Db.Connection.Command(insertSQL))
			{
				insertCommand.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				insertCommand.AddParameter("@batchNo", SqlDbType.Int, new Random().Next());
				insertCommand.AddParameter("@createdTime", SqlDbType.SmallDateTime, ZDateTime.UtcNow.ToDateTime());
				insertCommand.AddParameter("@createdUser", SqlDbType.VarChar, GlbStaff.CurrentUser.GS_Code.ToString());
				using (var reader = insertCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						batchPk = reader.GetGuid(0);
					}
				}
			}

			var updateSQL =
				"UPDATE dbo.AccEInvoicingTransactionPivot SET AIP_AIB = @batchPK, AIP_Status = @pivotStatus, AIP_SystemLastEditTimeUtc = GETUTCDATE(), AIP_SystemLastEditUser = @SystemLastEditUser WHERE AIP_PK IN (SELECT value FROM @parentPKs)";
			using (var updateCommand = Db.Connection.Command(updateSQL))
			{
				updateCommand.AddParameter("@batchPK", SqlDbType.UniqueIdentifier, batchPk);
				updateCommand.AddParameter("@pivotStatus", SqlDbType.Char, EInvoicingPivotState.Batched);
				updateCommand.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				updateCommand.AddTableValuedParameter("@parentPKs", AccEInvoicingTransactionPivotSchema.PK, complianceDocumentPivotsToRequeue.Select(x => x.PK));
				updateCommand.ExecuteNonQuery();
			}
		}

		void AssertPivotDetails(ZGuid parentPk, ZString expectedStatus, ZString expectedErrorDescription, ZGuid expectedBatchPK, ZDateTime expectedLastResponseReceived, ZDateTime expectedSentTime, ZBool expectedIsNotifiedByEmail)
		{
			var pivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk));
			AssertNotNull(pivot);
			AssertEquals(expectedStatus, pivot.AIP_Status);
			AssertEquals(expectedErrorDescription, pivot.AIP_ErrorDescription);
			AssertEquals(expectedBatchPK, pivot.AIP_AIB);
			AssertEquals(expectedLastResponseReceived, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals(expectedSentTime, pivot.AIP_LastSentTimeUtc);
			AssertEquals(expectedIsNotifiedByEmail, pivot.AIP_IsNotifiedByEmail);
		}

		ZGuid CreateAccEInvoicingBatch()
		{
			var factory = new BusinessObjectFactory();
			var batch = factory.New<AccEInvoicingBatch>();
			batch.AIB_Status = EInvoicingBatchState.Ready;
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;
			batch.AIB_BatchNumber = new Random().Next();
			batch.AIB_SystemCreateTimeUtc = ZDateTime.Now.ToDateTime();
			batch.AIB_SystemCreateUser = GlbStaff.CurrentUser.GS_Code.ToString();
			factory.Save();
			return batch.PK;
		}

		void UpdateBatch(ZGuid batchPk)
		{
			var factory = new BusinessObjectFactory();
			var batch = factory.Load<AccEInvoicingBatch>(batchPk);
			batch.AIB_Status = EInvoicingBatchState.Discarded;
			factory.Save();
		}

		void UpdatePivot(ZGuid parentPk, ZString errorStatus, ZString errorDescription, ZGuid batchPK, ZDateTime lastResponseReceived, ZDateTime sentTime, ZBool isNotifiedByEmail)
		{
			var factory = new BusinessObjectFactory();
			var pivot = factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentPk));
			AssertNotNull(pivot);
			pivot.AIP_AIB = batchPK;
			pivot.AIP_Status = errorStatus;
			pivot.AIP_ErrorDescription = errorDescription;
			pivot.AIP_LastResponseReceivedUtc = lastResponseReceived;
			pivot.AIP_LastSentTimeUtc = sentTime;
			pivot.AIP_IsNotifiedByEmail = isNotifiedByEmail;
			pivot.AIP_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			pivot.AIP_SystemLastEditUser = GlbStaff.CurrentUser.GS_Code;
			factory.Save();
		}

		#endregion
		public void TestLockComplianceBookMenuItem()
		{
			var italyCompany = TestObjectCreator.CreateNewCompany("ITL", CountryCodes.Italy);
			var italyBranch = TestObjectCreator.CreateNewBranch(italyCompany, "ITL");
			OrgHeader italyCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ITORGPROXY", true, true, "ITMIL");
			italyCompany.GC_OH_OrgProxy = italyCompanyOrgProxy.PK;

			var afghanistanCompany = TestObjectCreator.CreateNewCompany("AFG", CountryCodes.Afghanistan);
			var afghanistanBranch = TestObjectCreator.CreateNewBranch(afghanistanCompany, "AFB");
			OrgHeader afghanistanCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("AFGORGPROX", true, true, "AFBIN");
			afghanistanCompany.GC_OH_OrgProxy = afghanistanCompanyOrgProxy.PK;

			var chinaCompany = TestObjectCreator.CreateNewCompany("CHN", CountryCodes.China);
			var chinaBranch = TestObjectCreator.CreateNewBranch(chinaCompany, "NJG");
			OrgHeader chinaCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("CHNORGPROXY", true, true, "NJGIM");
			chinaCompany.GC_OH_OrgProxy = chinaCompanyOrgProxy.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

				var lockComplianceBookMenuItem = menu.FindByText("Lock Counter Compliance Book");
				AssertNotNull("Lock Counter Compliance Book menu item should be present in Italy (because there are sub-types configured for it and present not in china)", lockComplianceBookMenuItem);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
					form.Show();

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					lockComplianceBookMenuItem.PerformClick();

					var lastShownForm = arComplianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm.GetType());

					if (lastShownForm != null)
					{
						lastShownForm.Close();
						if (!lastShownForm.IsDisposed)
						{
							lastShownForm.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					lockComplianceBookMenuItem.PerformClick();

					var lastShownForm2 = arComplianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm2.GetType());

					if (lastShownForm2 != null)
					{
						lastShownForm2.Close();
						if (!lastShownForm2.IsDisposed)
						{
							lastShownForm2.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					lockComplianceBookMenuItem.PerformClick();
					AssertEquals(Env.Security.ComplianceSequencesModifyLockRelease.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					lockComplianceBookMenuItem.PerformClick();
					AssertEquals(Env.Security.ComplianceSequencesModifyLockRelease.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

				Assert(GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				Assert(!GlbCompany.CurrentCompany.Country.HasAccComplianceSequence);

				var lockComplianceBookMenuItem = menu.FindByText("Lock Counter Compliance Book");
				AssertNull("Lock Counter Compliance Book menu item should not be present in china (because china not have AccComplianceSequence Module)", lockComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

				Assert(!GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem lockComplianceBookMenuItem = menu.FindByText("Lock Counter Compliance Book");
				AssertNull("Lock Counter Compliance Book menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", lockComplianceBookMenuItem);
			}
		}

		public void TestReleaseComplianceBookMenuItem()
		{
			var italyCompany = TestObjectCreator.CreateNewCompany("ITL", CountryCodes.Italy);
			var italyBranch = TestObjectCreator.CreateNewBranch(italyCompany, "ITL");
			OrgHeader italyCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ITORGPROXY", true, true, "ITMIL");
			italyCompany.GC_OH_OrgProxy = italyCompanyOrgProxy.PK;

			var afghanistanCompany = TestObjectCreator.CreateNewCompany("AFG", CountryCodes.Afghanistan);
			var afghanistanBranch = TestObjectCreator.CreateNewBranch(afghanistanCompany, "AFB");
			OrgHeader afghanistanCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("AFGORGPROX", true, true, "AFBIN");
			afghanistanCompany.GC_OH_OrgProxy = afghanistanCompanyOrgProxy.PK;

			var chinaCompany = TestObjectCreator.CreateNewCompany("CHN", CountryCodes.China);
			var chinaBranch = TestObjectCreator.CreateNewBranch(chinaCompany, "NJG");
			OrgHeader chinaCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("CHNORGPROXY", true, true, "NJGIM");
			chinaCompany.GC_OH_OrgProxy = chinaCompanyOrgProxy.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), italyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

				var releaseComplianceBookMenuItem = menu.FindByText("Release Counter Compliance Book");
				AssertNotNull("Release Counter Compliance Book menu item should be present in Italy (because there are sub-types configured for it and present not in china)", releaseComplianceBookMenuItem);
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
					form.Show();

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					releaseComplianceBookMenuItem.PerformClick();

					var lastShownForm = arComplianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm.GetType());

					if (lastShownForm != null)
					{
						lastShownForm.Close();
						if (!lastShownForm.IsDisposed)
						{
							lastShownForm.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = true;

					releaseComplianceBookMenuItem.PerformClick();

					var lastShownForm2 = arComplianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm2.GetType());

					if (lastShownForm2 != null)
					{
						lastShownForm2.Close();
						if (!lastShownForm2.IsDisposed)
						{
							lastShownForm2.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = true;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					releaseComplianceBookMenuItem.PerformClick();

					var lastShownForm3 = arComplianceDocumentModule.LastShownComplianceDocumentForm_ForTest_ForTestOnly;
					AssertEquals("LastFormShownDialogForTest", typeof(LockReleaseComplianceBookForm), lastShownForm3.GetType());

					if (lastShownForm3 != null)
					{
						lastShownForm3.Close();
						if (!lastShownForm3.IsDisposed)
						{
							lastShownForm3.Dispose();
						}
					}

					Env.Security.ComplianceSequencesModifyLockRelease.IsAllowed = false;
					Env.Security.ComplianceSequencesModifyReleaseOtherStaff.IsAllowed = false;

					releaseComplianceBookMenuItem.PerformClick();
					AssertEquals(@"You do not have the appropriate security rights to run this function. 

If you require access to lock/release ‘CTR’ Allocation Level Compliance Invoice Book, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyLockRelease.DisplayTextPathToSecurityRight + @"

If you require access to release ‘CTR’ Allocation Level Compliance Invoice Book locked by other staff, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

" + Env.Security.ComplianceSequencesModifyReleaseOtherStaff.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), chinaBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

				Assert(GlbCompany.CurrentCompany.Country.SupportComplianceSubType);
				Assert(!GlbCompany.CurrentCompany.Country.HasAccComplianceSequence);

				var releaseComplianceBookMenuItem = menu.FindByText("Release Counter Compliance Book");
				AssertNull("Release Counter Compliance Book menu item should not be present in china (because china not have AccComplianceSequence Module)", releaseComplianceBookMenuItem);
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), afghanistanBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

				Assert(!GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				MenuItem releaseComplianceBookMenuItem = menu.FindByText("Release Counter Compliance Book");
				AssertNull("Release Counter Compliance Book menu item should not be present in Afghanistan (because there are NO sub-types configured for it)", releaseComplianceBookMenuItem);
			}
		}

		public void TestVoidWithFinalisedComplianceReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = "TW1";
				report.ACR_DateFrom = ZDate.Today;
				report.ACR_DateTo = ZDate.Today.AddDays(1);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var reportConfigurations = new ComplianceReportConfigurationCollection(Factory);
				var config = TestObjectCreator.CreateConfigurationForComplianceReport(report, reportConfigurations, AccComplianceDocumentHeaderSchema.Constants.Prefix, reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsReceivable, "", complianceSubType: "TXC");
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
				Factory.Save();

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = Factory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 1, complianceDocuments.Length);

				complianceDocuments[0].ADH_ComplianceSubType = "TXC";
				complianceDocuments[0].ADH_DocumentNumber = "001";
				Factory.Save();

				report.GenerateFromQueue();
				report.Finalise();
				AssertEquals("Report is Finalised", AccComplianceReport.Status.ReportFinalised, report.ACR_Status);

				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var voidComplianceDocumentMenuItem = arMenu.FindByText("Void");
					AssertNotNull("Void menu item should exist.", voidComplianceDocumentMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.Select(0);
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);
						voidComplianceDocumentMenuItem.PerformClick();

						AssertEquals("This compliance document is already finalized.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItem()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var header = CreateINVComplianceDocumentHeader();

				var allocateComplianceSequenceNumberOldSecurity = Env.Security.AllocateComplianceSequenceNumber.IsAllowed;

				try
				{
					using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
					{
						var arMenu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

						var allocateComplianceDocumentMenuItem = arMenu.FindByText("Allocate Compliance Sequence Number");
						AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);
						using (ZForm form = new ZForm())
						{
							form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
							form.Show();

							Env.Security.AllocateComplianceSequenceNumber.IsAllowed = false;
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals(Env.Security.AllocateComplianceSequenceNumber.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
							UnitTestUserNotification.Instance.ClearMessages();

							Env.Security.AllocateComplianceSequenceNumber.IsAllowed = true;
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

							arComplianceDocumentModule.PerformSearch_ForTest();
							arComplianceDocumentModule.DisplayGrid.Select(0);
							AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals("The compliance document(s) you selected no matching compliance invoice book found.", UnitTestUserNotification.Instance.LastMessage.Text);

							header.ADH_DocumentNumber = "test01";
							Factory.Save();

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals("The compliance document(s) record that you have selected already has a compliance number allocated.", UnitTestUserNotification.Instance.LastMessage.Text);

							header.ADH_DocumentNumber = ZString.Empty;
							var sequenceAAA = CreateComplianceSequence("AAA", "TDP", ComplianceBookAllocationLevel.Counter, true);
							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header.ADH_ComplianceSubType = "TDP";
							Factory.Save();

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
							allocateComplianceDocumentMenuItem.PerformClick();

							AssertEquals("Allocate Compliance Sequence Number successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("compliance document header has AAA sequence book", sequenceAAA.PK, header.ADH_XD_ComplianceBook);
							AssertEquals("compliance document header has document number", "000000001", header.ADH_DocumentNumber);

							var header2 = CreateINVComplianceDocumentHeader();

							arComplianceDocumentModule.PerformSearch_ForTest();

							arComplianceDocumentModule.DisplayGrid.SelectAllElements();
							AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

							allocateComplianceDocumentMenuItem.PerformClick();

							AssertEquals(@"No compliance document allocated due to one of the following reasons:
1. No matching compliance invoice book.
2. Compliance number has already been allocated.
3. Compliance number has already been voided.", UnitTestUserNotification.Instance.LastMessage.Text);

							header.ADH_DocumentNumber = string.Empty;
							header.ADH_ComplianceSubType = string.Empty;
							header.ADH_XD_ComplianceBook = ZGuid.Empty;
							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header2.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header2.ADH_ComplianceSubType = "TDP";
							Factory.Save();

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

							allocateComplianceDocumentMenuItem.PerformClick();

							AssertEquals(@"Some of the compliance document records that you have selected cannot be allocated due to one of the following reasons:
1. No matching compliance invoice book.
2. Compliance number has already been allocated.
3. Compliance number has already been voided.", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
				finally
				{
					Env.Security.AllocateComplianceSequenceNumber.IsAllowed = allocateComplianceSequenceNumberOldSecurity;
				}
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItem_Void()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var header = CreateINVComplianceDocumentHeader();

				var allocateComplianceSequenceNumberOldSecurity = Env.Security.AllocateComplianceSequenceNumber.IsAllowed;

				try
				{
					using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
					{
						var arMenu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

						var allocateComplianceDocumentMenuItem = arMenu.FindByText("Allocate Compliance Sequence Number");
						AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);
						using (ZForm form = new ZForm())
						{
							form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
							form.Show();

							Env.Security.AllocateComplianceSequenceNumber.IsAllowed = false;
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals(Env.Security.AllocateComplianceSequenceNumber.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
							UnitTestUserNotification.Instance.ClearMessages();

							Env.Security.AllocateComplianceSequenceNumber.IsAllowed = true;
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

							arComplianceDocumentModule.PerformSearch_ForTest();
							arComplianceDocumentModule.DisplayGrid.Select(0);
							AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

							header.ADH_DocumentNumber = "test01";
							Factory.Save();

							header.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
							Factory.Save();

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
							UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
							allocateComplianceDocumentMenuItem.PerformClick();
							AssertEquals("The compliance document(s) record that you have selected already has been voided.", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
				finally
				{
					Env.Security.AllocateComplianceSequenceNumber.IsAllowed = allocateComplianceSequenceNumberOldSecurity;
				}
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItemWhenNumberAndDateNotSynchronise()
		{
			var header = CreateINVComplianceDocumentHeader();
			var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header.ADH_ComplianceSubType = "TXC";
			header.ADH_XD_ComplianceBook = sequence.PK;
			header.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			header.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header.ADH_DocumentDate = new ZDateTime(2018, 3, 20);
			header.ADH_DocumentNumber = "AA12345678";
			header.ADH_ReportingPeriod = 201902;

			var header2 = CreateINVComplianceDocumentHeader();
			header2.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header2.ADH_ComplianceSubType = "TXC";
			header2.ADH_XD_ComplianceBook = sequence.PK;
			header2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			header2.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header2.ADH_DocumentDate = new ZDateTime(2018, 2, 20);
			header2.ADH_ReportingPeriod = 201902;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var allocateComplianceSequenceNumberOldSecurity = Env.Security.AllocateComplianceSequenceNumber.IsAllowed;

				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

					var allocateComplianceDocumentMenuItem = arMenu.FindByText("Allocate Compliance Sequence Number");
					AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						allocateComplianceDocumentMenuItem.PerformClick();
						AssertEquals(@"No compliance document allocated due to one of the following reasons:
1. No matching compliance invoice book.
2. Compliance number has already been allocated.
3. Compliance number has already been voided.
4. Allocation of compliance sequence number to Credit Note compliance document is not allowed.
The Credit Note compliance document’s number must reference an existing Invoice compliance document as per system configuration.
5. Document number cannot be allocated for Invoice Book AA as number sequence and date sequence will not synchronize if allocated.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItemWhenDocumentConfigurationEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var crdHeader = CreateCRDComplianceDocumentHeader();
				var allocateComplianceSequenceNumberOldSecurity = Env.Security.AllocateComplianceSequenceNumber.IsAllowed;

				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

					var allocateComplianceDocumentMenuItem = arMenu.FindByText("Allocate Compliance Sequence Number");
					AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						allocateComplianceDocumentMenuItem.PerformClick();

						AssertEquals(@"Allocation of compliance sequence number to Credit Note compliance document is not allowed.
The Credit Note compliance document’s number must reference an existing Invoice compliance document as per system configuration.", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						var invHeader = CreateINVComplianceDocumentHeader();
						var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
						invHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						invHeader.ADH_ComplianceSubType = "TXC";
						invHeader.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						allocateComplianceDocumentMenuItem.PerformClick();
						AssertEquals(@"Some of the compliance document records that you have selected cannot be allocated due to one of the following reasons:
1. No matching compliance invoice book.
2. Compliance number has already been allocated.
3. Compliance number has already been voided.
4. Allocation of compliance sequence number to Credit Note compliance document is not allowed.
The Credit Note compliance document’s number must reference an existing Invoice compliance document as per system configuration.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItemWithComplianceSequenceRelatedException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var header = CreateINVComplianceDocumentHeader();

				var allocateComplianceSequenceNumberOldSecurity = Env.Security.AllocateComplianceSequenceNumber.IsAllowed;

				try
				{
					using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
					{
						var arMenu = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly();

						var allocateComplianceDocumentMenuItem = arMenu.FindByText("Allocate Compliance Sequence Number");
						AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);
						using (ZForm form = new ZForm())
						{
							form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
							form.Show();

							Env.Security.AllocateComplianceSequenceNumber.IsAllowed = true;

							arComplianceDocumentModule.PerformSearch_ForTest();

							arComplianceDocumentModule.DisplayGrid.Select(0);
							AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

							Factory.Save();

							header.ADH_DocumentNumber = ZString.Empty;
							var sequenceAAA = CreateComplianceSequence("AAA", "TDP", ComplianceBookAllocationLevel.Counter, true);
							sequenceAAA.XD_ExpiryDate = sequenceAAA.XD_ExpiryDate = new ZDateTime(2019, 2, 24);
							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header.ADH_ComplianceSubType = "TDP";
							header.ADH_XD_ComplianceBook = sequenceAAA.PK;
							header.ADH_DocumentDate = new ZDateTime(2019, 2, 25);
							header.ADH_ReportingPeriod = 201902;
							Factory.Save();

							allocateComplianceDocumentMenuItem.PerformClick();
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The compliance document cannot be allocated.\r\nPlease check your Compliance Book Setups."));
						}
					}
				}
				finally
				{
					Env.Security.AllocateComplianceSequenceNumber.IsAllowed = allocateComplianceSequenceNumberOldSecurity;
				}
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItem_TestRegistryEnableEReportingFunctionality()
		{
			var mockIComplianceDocumentNumberProvider = new Mock<IComplianceDocumentNumberProvider>();
			mockIComplianceDocumentNumberProvider.Setup(x => x.AllocateComplianceDocumentNumberErrorMessage(It.IsAny<IEnumerable<ARComplianceDocumentHeader>>())).Returns(string.Empty);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentNumberProvider>>().Setup(x => x.Get()).Returns(mockIComplianceDocumentNumberProvider.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm())
			{
				var allocateComplianceDocumentMenuItem = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Sequence Number");
				AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);

				form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
				form.Show();

				var headerTXE = CreateINVComplianceDocumentHeader();
				headerTXE.ADH_DocumentNumber = ZString.Empty;
				var sequenceAAA = CreateComplianceSequence("AAA", "TXE", ComplianceBookAllocationLevel.Counter, true);
				headerTXE.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				headerTXE.ADH_ComplianceSubType = "TXE";
				Factory.Save();

				arComplianceDocumentModule.PerformSearch_ForTest();
				arComplianceDocumentModule.DisplayGrid.SelectAllElements();
				AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				allocateComplianceDocumentMenuItem.PerformClick();

				AssertEquals("Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.", UnitTestUserNotification.Instance.LastMessage.Text);

				var headerTCE = CreateINVComplianceDocumentHeader();
				headerTCE.ADH_DocumentNumber = ZString.Empty;
				var sequenceBBB = CreateComplianceSequence("BBB", "TCE", ComplianceBookAllocationLevel.Counter, true);
				headerTCE.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				headerTCE.ADH_ComplianceSubType = "TCE";
				Factory.Save();

				arComplianceDocumentModule.PerformSearch_ForTest();
				arComplianceDocumentModule.DisplayGrid.SelectAllElements();
				AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				allocateComplianceDocumentMenuItem.PerformClick();

				AssertEquals(@"Some of the compliance document records that you have selected cannot be allocated due to one of the following reasons:
1. No matching compliance invoice book.
2. Compliance number has already been allocated.
3. Compliance number has already been voided.
4. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("headerTXE doesn't have sequence book", ZGuid.Empty, headerTXE.ADH_XD_ComplianceBook);
				AssertEquals("headerTXE doesn't have document number", ZString.Empty, headerTXE.ADH_DocumentNumber);
				AssertEquals("headerTCE has BBB sequence book", sequenceBBB.PK, headerTCE.ADH_XD_ComplianceBook);
				AssertEquals("headerTCE has document number", "000000001", headerTCE.ADH_DocumentNumber);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertEquals(true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				allocateComplianceDocumentMenuItem.PerformClick();
				AssertEquals("Allocate Compliance Sequence Number successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllocateComplianceSequenceNumberMenuItem_ComplianceDocumentNumberProvider()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mockIComplianceDocumentNumberProvider = new Mock<IComplianceDocumentNumberProvider>();

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentNumberProvider>>().Setup(x => x.Get()).Returns(mockIComplianceDocumentNumberProvider.Object);

			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm())
			{
				var allocateComplianceDocumentMenuItem = arComplianceDocumentModule.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Sequence Number");
				AssertNotNull("Allocate Compliance Sequence Number menu item should exist.", allocateComplianceDocumentMenuItem);

				form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
				form.Show();

				var headerTXE = CreateINVComplianceDocumentHeader();
				headerTXE.ADH_DocumentNumber = ZString.Empty;
				var sequenceAAA = CreateComplianceSequence("AAA", "TXE", ComplianceBookAllocationLevel.Counter, true);
				headerTXE.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				headerTXE.ADH_ComplianceSubType = "TXE";
				Factory.Save();

				arComplianceDocumentModule.PerformSearch_ForTest();
				arComplianceDocumentModule.DisplayGrid.SelectAllElements();
				AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

				AssertErrorMessageAllocateComplianceDocumentNumberErrorMessage("AllocateComplianceDocumentNumberErrorMessage1", "AllocateComplianceDocumentNumberErrorMessage1");
				mockIComplianceDocumentNumberProvider.Verify(x => x.CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument(It.IsAny<List<string>>(), It.Is<List<ARComplianceDocumentHeader>>(y => y.Count == 1 && y.FirstOrDefault().PK == headerTXE.PK)), Times.Never);

				AssertErrorMessageAllocateComplianceDocumentNumberErrorMessage(string.Empty, "Allocate Compliance Sequence Number successfully.");
				mockIComplianceDocumentNumberProvider.Verify(x => x.CheckCanAllocateComplianceDocumentNumberForSomeComplianceDocument(It.IsAny<List<string>>(), It.Is<List<ARComplianceDocumentHeader>>(y => y.Count == 1 && y.FirstOrDefault().PK == headerTXE.PK)), Times.Once);

				void AssertErrorMessageAllocateComplianceDocumentNumberErrorMessage(string mockErrorMessage, string expectedNotificationMsg)
				{
					mockIComplianceDocumentNumberProvider.Setup(x => x.AllocateComplianceDocumentNumberErrorMessage(It.Is<IEnumerable<ARComplianceDocumentHeader>>(y => y.Count() == 1 && y.FirstOrDefault().PK == headerTXE.PK))).Returns(mockErrorMessage);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					allocateComplianceDocumentMenuItem.PerformClick();
					mockIComplianceDocumentNumberProvider.Verify(x => x.AllocateComplianceDocumentNumberErrorMessage(It.Is<IEnumerable<ARComplianceDocumentHeader>>(y => y.Count() == 1 && y.FirstOrDefault().PK == headerTXE.PK)), Times.AtLeastOnce);
					AssertEquals(expectedNotificationMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		AccComplianceSequence CreateComplianceSequence(string code, string sequenceClass, string allocationLevel, bool lockBy = false)
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_SequenceClass = sequenceClass;
			sequence.XD_Code = code;
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 100;
			sequence.XD_NextNumber = 1;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_AllocationLevel = allocationLevel;
			if (sequence.AllocationStrategy.IsBranchApplicable)
			{
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			}
			if (sequence.AllocationStrategy.IsDepartmentApplicable)
			{
				sequence.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			}
			if (lockBy)
			{
				sequence.XD_LockBy = GlbStaff.CurrentUser.PK;
			}
			return sequence;
		}

		ARComplianceDocumentHeader CreateCRDComplianceDocumentHeader()
		{
			var vat4 = TestObjectCreator.CreateTaxRate("VAT4", "", 4);
			vat4.AT_PostingGroupId = 0;

			var ac2 = TestObjectCreator.CreateChargeCode("AC2");
			ac2.AC_AT_GSTRate = vat4.PK;
			ac2.AC_Desc = "desc";

			var creditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.Agent, GlbCompany.CurrentCompany.LocalCurrency, 1m);
			creditNote.AH_OH = TestObjectCreator.Agent.PK;
			var creditNoteLine = creditNote.Lines.AddNew() as ARCreditNoteLine;
			creditNoteLine.AL_JH = TestObjectCreator.Job1.PK;
			creditNoteLine.AL_AC = ac2.PK;
			creditNoteLine.AL_AT = vat4.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = TestObjectCreator.Job1.PK;
			charge.JR_LocalCostAmt = 10m;
			charge.JR_OSCostAmt = 10m;
			charge.JR_AC = ac2.PK;
			charge.JR_AT_CostGSTRate = creditNoteLine.AL_AT;
			charge.JR_AL_ARLine = creditNoteLine.PK;

			var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			header.ADH_Ledger = creditNote.AH_Ledger;
			header.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			header.ADH_DocumentNumber = ZString.Empty;
			header.ADH_TransactionType = TransactionTypes.CreditNote;

			var line = Factory.New<AccComplianceDocumentLine>();
			line.ADL_Sequence = 1;
			line.ADL_ADH = header.PK;
			line.ADL_Description = "desc";

			var pivot = Factory.New<AccComplianceDocumentPivot>();
			pivot.ADP_ADL = line.PK;
			pivot.ADP_AL = creditNoteLine.PK;

			Factory.Save();

			return header;
		}

		public void TestPrintMenuItem()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var header = (ARComplianceDocumentHeader)CreateINVComplianceDocumentHeader();

				var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;

				try
				{
					using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
					{
						var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

						var printMenuItem = arMenu.FindByText("Print");
						AssertNotNull("Print menu item should exist.", printMenuItem);
						using (ZForm form = new ZForm())
						{
							form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
							form.Show();

							Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = false;

							printMenuItem.PerformClick();

							AssertEquals(Env.Security.PrintReceivablesComplianceDocuments.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.ClearMessages();

							Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

							printMenuItem.PerformClick();

							AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.ClearMessages();

							arComplianceDocumentModule.PerformSearch_ForTest();

							arComplianceDocumentModule.DisplayGrid.Select(0);
							AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

							header.ADH_DocumentNumber = ZString.Empty;
							var sequenceAAA = CreateComplianceSequence("AAA", "TDP", ComplianceBookAllocationLevel.Counter, true);
							sequenceAAA.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.SinglePageSummarize;
							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header.ADH_ComplianceSubType = "TDP";
							header.ADH_PrintCount = 1;
							Factory.Save();

							var complianceDocumentsHeader = new AccComplianceDocumentHeader[1] { header };

							ComplianceDocumentHelper.AllocateComplianceDocuments(Factory, complianceDocumentsHeader);

							Factory.Save();

							AssertEquals(ComplianceRollupBehaviourType.SinglePageSummarize, header.ComplianceBook.XD_RollupBehaviourWhenMaxExceeded);

							printMenuItem.PerformClick();

							AssertEquals("The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.ClearMessages();

							StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
							menu.SU_MenuName = "test";
							menu.SU_BusinessContext = "ARComplianceDocument";
							menu.SU_MenuPath = ZString.Empty;
							menu.SU_IsSystemDefined = true;
							menu.SU_MenuType = "DOC";
							menu.SU_GS_NKStaffCode = ZString.Empty;
							menu.SU_ContactType = "NCT";
							menu.SU_IsPublished = true;

							var sequenceBBB = CreateComplianceSequence("BBB", "TXC", ComplianceBookAllocationLevel.Counter, true);
							sequenceBBB.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
							sequenceBBB.XD_SU_MenuItem = menu.PK;
							Factory.Save();

							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header.ADH_ComplianceSubType = "TXC";
							header.ADH_XD_ComplianceBook = sequenceBBB.PK;

							var newlist1 = new ComplianceDocumentRePrintRestrictionCollection();
							newlist1.Add(new ComplianceDocumentRePrintRestriction() { OrganizationCategory = "BUS", NumberOfReprintAllowed = 1, ComplianceDocumentMenu = ((CargoWise.Integration.ICodeDescription)menu).Code.ToUpper() });

							AccountingMasterFilesRegistry.Instance.ComplianceDocumentRePrintRestriction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newlist1);
							header.ADH_PrintCount = 2;
							header.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
							header.Organisation.OH_Category = "BUS";
							Factory.Save();

							printMenuItem.PerformClick();

							AssertEquals("The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.ClearMessages();

							header.ADH_ComplianceSubType = "TTT";
							header.ADH_XD_ComplianceBook = ZGuid.Empty;
							header.ADH_DocumentNumber = ZString.Empty;
							header.ADH_PrintCount = 0;
							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							Factory.Save();

							printMenuItem.PerformClick();

							AssertEquals("The compliance document cannot be printed as no matching compliance invoice book is found.", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.ClearMessages();

							var header2 = CreateINVComplianceDocumentHeader();

							arComplianceDocumentModule.PerformSearch_ForTest();
							arComplianceDocumentModule.DisplayGrid.SelectAllElements();
							AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

							printMenuItem.PerformClick();

							AssertEquals(@"No compliance document printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing.", UnitTestUserNotification.Instance.LastMessage.Text);

							UnitTestUserNotification.Instance.ClearMessages();

							header.ADH_ComplianceSubType = "TXC";
							Factory.Save();

							arComplianceDocumentModule.PerformSearch_ForTest();
							arComplianceDocumentModule.DisplayGrid.SelectAllElements();
							printMenuItem.PerformClick();

							AssertEquals("compliance document header has BBB sequence book", sequenceBBB.PK, header.ADH_XD_ComplianceBook);
							AssertEquals("compliance document header has document number", "000000001", header.ADH_DocumentNumber);

							AssertEquals(@"Some of the compliance document records selected cannot be printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing.", UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
				finally
				{
					Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
				}
			}
		}

		public void TestPrintMenuItemWhenCreditNoteComplianceDocumentConfigurationEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var crdHeader = CreateCRDComplianceDocumentHeader();
				var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;

				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						printMenuItem.PerformClick();

						AssertEquals(@"The compliance document cannot be printed as no matching compliance invoice book is found.", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						var crdHeader1 = CreateCRDComplianceDocumentHeader();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						printMenuItem.PerformClick();

						AssertEquals(@"No compliance document printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						var invHeader = CreateINVComplianceDocumentHeader();
						var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
						invHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						invHeader.ADH_ComplianceSubType = "TXC";
						invHeader.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(3, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						printMenuItem.PerformClick();
						AssertEquals(@"Some compliance document have skipped printing due to their Compliance Invoice Book setups do not fully support government invoice printing: No Compliance Invoice Document will be Printed.
 This Compliance Book is not configured for printing.
 If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestPrintComplianceDocumentHeaderWithNumberAndDateNotSynchronise()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);

			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = "Electronic test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;

			var header = CreateINVComplianceDocumentHeader();
			var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequence.XD_SU_MenuItem = menu.PK;
			header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header.ADH_ComplianceSubType = "TXC";
			header.ADH_XD_ComplianceBook = sequence.PK;
			header.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			header.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header.ADH_DocumentDate = new ZDateTime(2018, 3, 20);
			header.ADH_DocumentNumber = "AA12345678";
			header.ADH_ReportingPeriod = 201902;

			var header2 = CreateINVComplianceDocumentHeader();
			header2.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header2.ADH_ComplianceSubType = "TXC";
			header2.ADH_XD_ComplianceBook = sequence.PK;
			header2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			header2.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header2.ADH_DocumentDate = new ZDateTime(2018, 2, 20);
			header2.ADH_ReportingPeriod = 201902;

			Factory.Save();

			var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;
			try
			{
				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"Some of the compliance document records selected cannot be printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.
7. Document number cannot be allocated for Invoice Book AA as number sequence and date sequence will not synchronize if allocated.

Please review compliance invoice books, registry settings and selection before printing."));
						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
			}
			finally
			{
				Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
			}
		}

		public void TestPrintComplianceDocumentHeaderWithMIDOrPIGCustomCode()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);

			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = "Electronic test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;

			var header = CreateINVComplianceDocumentHeader();
			var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequence.XD_SU_MenuItem = menu.PK;
			header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header.ADH_ComplianceSubType = "TXE";
			header.ADH_XD_ComplianceBook = sequence.PK;
			header.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			header.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header.Organisation.CustomsCodes.AddNew("MCI", "123");

			Factory.Save();

			var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;
			try
			{
				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

						arComplianceDocumentModule.PerformSearch_ForTest();

						arComplianceDocumentModule.DisplayGrid.Select(0);
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						printMenuItem.PerformClick();
						Assert(header.ShouldPreventPrintDocument);
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Document(s) cannot be printed for a TXE compliance document as the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code."));

						var header1 = CreateINVComplianceDocumentHeader();
						header1.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						header1.ADH_ComplianceSubType = "TXC";
						header1.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();
						Assert(!header1.ShouldPreventPrintDocument);

						arComplianceDocumentModule.PerformSearch_ForTest();

						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"Some of the compliance document records selected cannot be printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing."));
						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
			}
			finally
			{
				Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
			}
		}

		public void TestPrintComplianceDocumentWithNumberAndNoBook()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);
			var complianceDocument = CreateINVComplianceDocumentHeader();

			var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;

			var menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = "test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;

			var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequence.XD_SU_MenuItem = menu.PK;

			try
			{
				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

						arComplianceDocumentModule.PerformSearch_ForTest();

						arComplianceDocumentModule.DisplayGrid.Select(0);
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);
						complianceDocument.ADH_DocumentNumber = "000000001";
						Factory.Save();

						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The selected compliance document(s) cannot be printed as no matching compliance invoice book is found."));
						UnitTestUserNotification.Instance.ClearMessages();

						var complianceDocument2 = CreateINVComplianceDocumentHeader();
						complianceDocument2.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						complianceDocument2.ADH_ComplianceSubType = "TXC";
						complianceDocument2.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);
						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"Some of the compliance document records selected cannot be printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing."));
						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
			}
			finally
			{
				Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
			}
		}

		public void TestPrintVoidedComplianceDocumentHeader()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);

			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = "test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;

			var header = CreateINVComplianceDocumentHeader();
			var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequence.XD_SU_MenuItem = menu.PK;

			var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;

			try
			{
				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

						arComplianceDocumentModule.PerformSearch_ForTest();

						arComplianceDocumentModule.DisplayGrid.Select(0);
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						header.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
						header.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();
						Assert("compliance document has been voided.", header.IsVoided);

						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The selected compliance document has been voided and cannot be printed."));
						UnitTestUserNotification.Instance.ClearMessages();

						var header1 = CreateINVComplianceDocumentHeader();
						header1.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
						header1.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();
						Assert("compliance document has been voided.", header1.IsVoided);

						arComplianceDocumentModule.PerformSearch_ForTest();

						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);
						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("These selected compliance documents have been voided and cannot be printed."));
						UnitTestUserNotification.Instance.ClearMessages();

						var header2 = CreateINVComplianceDocumentHeader();
						header2.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						header2.ADH_ComplianceSubType = "TXC";
						header2.ADH_XD_ComplianceBook = sequence.PK;
						Factory.Save();
						Assert("compliance document not voided.", !header2.IsVoided);

						arComplianceDocumentModule.PerformSearch_ForTest();

						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(3, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);
						printMenuItem.PerformClick();
						Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"Some of the compliance document records selected cannot be printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing."));
						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
			}
			finally
			{
				Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
			}
		}

		public void TestPrintMenuItemWithComplianceSequenceRelatedException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var header = CreateINVComplianceDocumentHeader();

				var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;

				try
				{
					using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
					{
						var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

						var printMenuItem = arMenu.FindByText("Print");
						AssertNotNull("Print menu item should exist.", printMenuItem);
						using (ZForm form = new ZForm())
						{
							form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
							form.Show();

							Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

							arComplianceDocumentModule.PerformSearch_ForTest();

							arComplianceDocumentModule.DisplayGrid.Select(0);
							AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

							header.ADH_DocumentNumber = ZString.Empty;
							var sequenceAAA = CreateComplianceSequence("AAA", "TDP", ComplianceBookAllocationLevel.Counter, true);
							sequenceAAA.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.SinglePageSummarize;
							sequenceAAA.XD_ExpiryDate = new ZDateTime(2019, 2, 24);
							header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
							header.ADH_ComplianceSubType = "TDP";
							header.ADH_PrintCount = 1;
							header.ADH_XD_ComplianceBook = sequenceAAA.PK;
							header.ADH_DocumentNumber = ZString.Empty;
							header.ADH_DocumentDate = new ZDateTime(2019, 2, 25);
							header.ADH_ReportingPeriod = 201902;
							Factory.Save();

							printMenuItem.PerformClick();
							Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("The compliance document cannot be printed.\r\nPlease check your Compliance Book Setups."));
						}
					}
				}
				finally
				{
					Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
				}
			}
		}

		public void TestPrintMenuItemWithSuppressShowComplianceBookHasNoTemplateWarning()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						var header = CreateINVComplianceDocumentHeader();

						StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
						menu.SU_MenuName = "test";
						menu.SU_BusinessContext = "ARComplianceDocument";
						menu.SU_MenuPath = ZString.Empty;
						menu.SU_IsSystemDefined = true;
						menu.SU_MenuType = "DOC";
						menu.SU_GS_NKStaffCode = ZString.Empty;
						menu.SU_ContactType = "NCT";
						menu.SU_IsPublished = true;

						var sequenceBBB = CreateComplianceSequence("BBB", "TXC", ComplianceBookAllocationLevel.Counter, true);
						sequenceBBB.XD_RollupBehaviourWhenMaxExceeded = ComplianceRollupBehaviourType.MultiPageNoLimitation;
						sequenceBBB.XD_SU_MenuItem = menu.PK;

						header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						header.ADH_ComplianceSubType = "TXC";
						header.ADH_XD_ComplianceBook = sequenceBBB.PK;
						header.ADH_DocumentNumber = "000000001";
						Factory.Save();

						Assert(!AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.Value);

						sequenceBBB.XD_SU_MenuItem = ZGuid.Empty;
						Factory.Save();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();

						printMenuItem.PerformClick();

						AssertEquals(@"Some compliance document have skipped printing due to their Compliance Invoice Book setups do not fully support government invoice printing: No Compliance Invoice Document will be Printed.
 This Compliance Book is not configured for printing.
 If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessages();

						AccountingMasterFilesRegistry.Instance.SuppressShowComplianceBookHasNoTemplateWarning.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

						printMenuItem.PerformClick();

						AssertNotEquals(@"Some compliance document have skipped printing due to their Compliance Invoice Book setups do not fully support government invoice printing: No Compliance Invoice Document will be Printed.
 This Compliance Book is not configured for printing.
 If you want to print a Compliance Document please amend your Compliance Book setups and nominate an appropriate document menu for printing.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestPrintMenuItem_TestRegistryEnableEReportingFunctionality()
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

			GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan);

			var menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = "Electronic test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;

			var sequence = CreateComplianceSequence("AA", "TXC", ComplianceBookAllocationLevel.Counter, true);
			sequence.XD_SU_MenuItem = menu.PK;

			var header = CreateINVComplianceDocumentHeader();
			header.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			header.ADH_ComplianceSubType = "TXE";
			header.ADH_XD_ComplianceBook = sequence.PK;
			header.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			header.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			header.ADH_ReportingPeriod = 201902;
			Factory.Save();

			var printOldSecurity = Env.Security.PrintReceivablesComplianceDocuments.IsAllowed;
			try
			{
				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentNumberProvider>>().Setup(x => x.Get()).Returns(() => null);

				var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				using (var arComplianceDocumentModule = (ARComplianceDocumentModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					var arMenu = arComplianceDocumentModule.GetNewAdditionalMenuItems_ForTestOnly();

					var printMenuItem = arMenu.FindByText("Print");
					AssertNotNull("Print menu item should exist.", printMenuItem);
					using (var form = new ZForm())
					{
						form.Controls.Add(arComplianceDocumentModule.EmbeddedControl);
						form.Show();

						Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = true;

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(1, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						printMenuItem.PerformClick();
						AssertEquals("Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.", UnitTestUserNotification.Instance.LastMessage.Text);

						var header2 = CreateINVComplianceDocumentHeader();
						header2.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
						header2.ADH_ComplianceSubType = "TXC";
						header2.ADH_XD_ComplianceBook = sequence.PK;
						header2.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
						header2.Organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
						header2.ADH_DocumentDate = new ZDateTime(2018, 2, 20);
						header2.ADH_ReportingPeriod = 201902;
						Factory.Save();

						arComplianceDocumentModule.PerformSearch_ForTest();
						arComplianceDocumentModule.DisplayGrid.SelectAllElements();
						AssertEquals(2, arComplianceDocumentModule.DisplayGrid.SelectedElements.Length);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						printMenuItem.PerformClick();
						AssertEquals(@"Some of the compliance document records selected cannot be printed due to one of the following reasons:
1. The compliance document has already been printed and re-print is not allowed as the compliance invoice book’s document printing style is set to SRA/SSM.
2. The compliance document cannot be printed as no matching compliance invoice book is found.
3. The compliance document cannot be printed as print count exceeded the re-print restriction set for the Organization Category in Accounting > Government Compliance Invoice Document > Compliance Document Re-Print Restriction.
4. The compliance document has already been voided.
5. The TXE compliance document with the Debtor's organization category is 'NAT' and has a MCI-Mobile Carrier ID / PIG-Public Interest Group registration code.
6. Compliance document number cannot be allocated to TXE compliance document as 'Enable E-Reporting Functionality' registry is set to 'No'.

Please review compliance invoice books, registry settings and selection before printing.", UnitTestUserNotification.Instance.LastMessage.Text);

						AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
						AssertEquals(true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						printMenuItem.PerformClick();
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.PrintReceivablesComplianceDocuments.IsAllowed = printOldSecurity;
			}
		}

		class ARComplianceDocumentModuleForConcurrencyTesting : ARComplianceDocumentModule
		{
			protected override void HandleResetStatusToQueuedCore(List<AccEInvoicingTransactionPivot> complianceDocumentsWithFailedStatus)
			{
				StimulateAnotherUserRequeuingAndServiceTaskRun(complianceDocumentsWithFailedStatus);
				//Current user try to requeue the transactions again
				base.HandleResetStatusToQueuedCore(complianceDocumentsWithFailedStatus);
			}
		}
	}
}
