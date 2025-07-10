using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Module.Testing
{
	[TestedType(typeof(CommissionManagementModule))]
	public class CommissionManagementModuleTest : ZModuleBasherTest
	{
		#region ID

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Commission;
		}

		#endregion

		#region Allowed Actions

		public void TestAllowDelete()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(false, module.AllowEdit);
			}
		}

		public void TestAllowNew()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowView()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(true, module.AllowView);
			}
		}

		#endregion

		#region Menu Items

		public void TestToolBarButtons()
		{
			using (var module = new CommissionManagementModule())
			{
				var toolBarButtons = module.ToolBarButtons;
				AssertArrayEqualsByElements(
					new[]
					{
						"Agreement Approval",
						"Finalizer",
					},
					toolBarButtons.Skip(toolBarButtons.Length - 3).Take(2).Select(x => x.Text).ToArray());
			}
		}

		public void TestApproveCommissionAgreementsMenuItem()
		{
			using (var module = new CommissionManagementModule())
			{
				var toolBarButtons = module.ToolBarButtons;
				var approveCommissionAgreementsMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Agreement Approval");
				AssertNotNull("approveCommissionAgreementsMenuItem", approveCommissionAgreementsMenuItem);

				Env.Security.CommissionAgreementApproval.IsAllowed = false;
				approveCommissionAgreementsMenuItem.PerformClick();

				AssertEquals(Env.Security.CommissionAgreementApproval.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCommissionFinalizerMenuItem()
		{
			using (var module = new CommissionManagementModule())
			{
				var toolBarButtons = module.ToolBarButtons;
				var commissionFinaliserMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Finalizer");
				AssertNotNull("commissionFinaliserMenuItem", commissionFinaliserMenuItem);

				Env.Security.CommissionFinaliser.IsAllowed = false;
				commissionFinaliserMenuItem.PerformClick();

				AssertEquals(Env.Security.CommissionFinaliser.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (var module = new CommissionManagementModuleForTest())
			{
				AssertEquals("&Cancel", module.GetDeleteMenuItemText_Exposed().Caption);
			}
		}

		#region Regenerate Commissions

		public void TestRegenerateCommissionsMenuItem_NoCommissionGroupingsSelectedMessage()
		{
			using (var module = new CommissionManagementModule())
			{
				var toolBarButtons = module.ToolBarButtons;
				var regenerateCommissionMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Regenerate Commissions");
				AssertNotNull("regenerateCommissionMenuItem", regenerateCommissionMenuItem);

				regenerateCommissionMenuItem.PerformClick();

				AssertEquals("Please select a commission group", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRegenerateCommissionsMenuItem_ConfirmationMessage()
		{
			using (var module = new CommissionManagementModuleForTest(Factory))
			{
				module.SetupCommissionData();
				var results = module.GetSelectedBusinessObjects();

				AssertEquals("Pre-condition", 4, results.Length);

				var toolBarButtons = module.ToolBarButtons;
				var regenerateCommissionMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Regenerate Commissions");
				AssertNotNull("regenerateCommissionMenuItem", regenerateCommissionMenuItem);

				regenerateCommissionMenuItem.PerformClick();

				AssertMultilineASCIIEquals(@"Selecting Yes will queue the following Job(s) / Transaction(s) for regeneration, this action will cancel all existing commission transactions and regenerate their commission lines:

 - 10001
 - 10002
 - S000001

Select Yes to Continue, or No to Cancel this action.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRegenerateCommissionMenuItem_QueueItemAdded()
		{
			using (var module = new CommissionManagementModuleForTest(Factory))
			{
				module.SetupCommissionData();
				var results = module.GetSelectedBusinessObjects();

				AssertEquals("Pre-condition", 4, results.Length);
				AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));

				var toolBarButtons = module.ToolBarButtons;
				var regenerateCommissionMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Regenerate Commissions");
				AssertNotNull("regenerateCommissionMenuItem", regenerateCommissionMenuItem);

				UnitTestUserNotification.Instance.AddYesAnswer();
				regenerateCommissionMenuItem.PerformClick();

				var regenQueueItemsQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_Operation, OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration);

				AssertEquals("3 RGN queue items should be created", 3, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue), regenQueueItemsQuery));
				AssertEquals("Success message should show", "Successfully added to Calculation Queue.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRegenerateCommissionMenuItem_OldQueueItemsAreOverwritten()
		{
			using (var module = new CommissionManagementModuleForTest(Factory))
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));

				module.SetupCommissionData();
				var results = module.GetSelectedBusinessObjects().Cast<ViewCommissionLineGrouping>().ToArray();
				var jobPk = results[3].CommissionLines.First().VCL_GroupingSourceID;

				AssertEquals("Pre-condition", 4, results.Length);

				var queueItem = Factory.New<OrgCommissionCalculationQueue>();
				queueItem.CAQ_JH = jobPk;
				queueItem.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;

				Factory.Save();

				AssertEquals(1, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));

				var toolBarButtons = module.ToolBarButtons;
				var regenerateCommissionMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Regenerate Commissions");
				AssertNotNull("regenerateCommissionMenuItem", regenerateCommissionMenuItem);

				UnitTestUserNotification.Instance.AddYesAnswer();

				regenerateCommissionMenuItem.PerformClick();

				var queueItemForJobQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_Operation, OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration);
				queueItemForJobQuery.AddToFilter(OrgCommissionCalculationQueueSchema.CAQ_JH, jobPk);

				var queueItemForJob = Factory.Load<OrgCommissionCalculationQueue>(queueItemForJobQuery).First();

				AssertNotNull(nameof(queueItemForJob), queueItemForJob);
				AssertNotEquals("OrgCommissionCalculationQueue should have been deleted and overwritten when duplicate was created", queueItemForJob.PK, queueItem.PK);
				AssertEquals("1 created manually (queueItem) + 2 for invoices 10001 and 10002", 3, Factory.Load<OrgCommissionCalculationQueue>(new ZQuery()).Length);
			}
		}

		public void TestRegenerateCommissionMenuItem_ArchivedJobsMessage()
		{
			using (var module = new CommissionManagementModuleForTest(Factory))
			{
				module.SetupCommissionDataForArchivedJobs();
				var results = module.GetSelectedBusinessObjects().Cast<ViewCommissionLineGrouping>().ToArray();

				AssertEquals("Pre-condition", 1, results.Length);
				AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));

				var toolBarButtons = module.ToolBarButtons;
				var regenerateCommissionMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Regenerate Commissions");
				AssertNotNull("regenerateCommissionMenuItem", regenerateCommissionMenuItem);

				UnitTestUserNotification.Instance.AddYesAnswer();
				regenerateCommissionMenuItem.PerformClick();

				var regenQueueItemsQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_Operation, OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration);

				AssertEquals("Zero RGN queue items should be created", 0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue), regenQueueItemsQuery));
				var jobNumber = results[0].CommissionLines.First().VCL_JobNumber;

				AssertMultilineASCIIEquals("Archived Jobs message should show",
					@$"There are commissions with Jobs that have been archived. They cannot have their commissions regenerated. The archived Jobs are:
 - {jobNumber}
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#endregion

		#region Export To Excel

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShouldLoadTop1WhenGridEmpty()
		{
			using (var module = new CommissionManagementModuleForTest())
			{
				Assert(!module.ShouldLoadTop1WhenGridEmptyExposed);
			}
		}

		#endregion

		#region Security

		public void TestSecurity()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(Env.Security.CommissionManager, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region Licence

		public void TestLicenceCheckPoint()
		{
			using (var module = new CommissionManagementModule())
			{
				AssertEquals(Env.Licence.CommissionManager, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region ImportDataWizard

		public void TestImportDataWizardMenuItem()
		{
			AssertImportByDataWizardRequiresImportToSystemPrivilege(Env.Security.CommissionManager, Env.Security.CommissionManagerNew);
		}

		public void TestDisplayImportDataWizardResult()
		{
			var commissionLineCollection = new AccCommissionLineCollection(Factory);
			var flattenedCollection = new CommissionFlattenedCollection(Factory);
			var flattenedCollectionInfo = new CommissionImportCollectionInfo(flattenedCollection, false);
			var processor = new CommissionFlattenedDataTransferProcessorForTest(commissionLineCollection, flattenedCollectionInfo);

			for (var i = 0; i < 10; i++)
			{
				flattenedCollection.AddNew();
			}

			processor.SetNewCountForTesting(4);
			processor.SetOverriddenCountForTesting(3);
			processor.SetIgnoredCountForTesting(1);
			processor.SetErrorCountForTesting(2);
			processor.AddLogForTesting("Line 2: Invalid Organization Code 'XXX'");
			processor.AddLogForTesting("Line 5: Invalid Company Code 'XXX'");

			CommissionManagementModuleForTest.DisplayImportDataWizardResult_Exposed(processor, true);
			AssertEquals("LastMessage.Caption", "Import Canceled", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("LastMessage.Text", "No entity commission was created.", UnitTestUserNotification.Instance.LastMessage.Text);

			var previousShowDialogsInTest = ZFormModaliser.ShowDialogsInTest;
			ZFormModaliser.ShowDialogsInTest = true;
			try
			{
				CommissionManagementModuleForTest.DisplayImportDataWizardResult_Exposed(processor, false);
				using (var dialog = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertType(typeof(ZMessageBox), dialog);
					var messageBox = (ZMessageBox)dialog;

					AssertEquals("messageBox.Text", "Import Completed", messageBox.Text);
					AssertMultilineASCIIEquals("messageBox.Message",
					@"Entity Commissions to Import = 10

Line 2: Invalid Organization Code 'XXX'
Line 5: Invalid Company Code 'XXX'

TOTAL: New Commissions Created = 4, Commissions Overridden = 3, Commissions Ignored = 1, Commissions Excluded = 2",
					messageBox.Message);
				}
			}
			finally
			{
				ZFormModaliser.ShowDialogsInTest = previousShowDialogsInTest;
			}
		}

		#endregion
	}

	class CommissionManagementModuleForTest : CommissionManagementModule
	{
		public CommissionManagementModuleForTest() { }

		public CommissionManagementModuleForTest(BusinessObjectFactory testFactory)
		{
			TestFactory = testFactory;
		}

		public ResourceStringData GetDeleteMenuItemText_Exposed()
		{
			return base.GetDeleteMenuItemText();
		}

		public static void DisplayImportDataWizardResult_Exposed(CommissionFlattenedDataTransferProcessor processor, bool isCancelled)
		{
			DisplayImportDataWizardResult(processor, isCancelled);
		}

		public void SetupCommissionData()
		{
			var objectCreator = new TestObjectCreator(TestFactory);

			var customer = TestFactory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "TSTORG";

			var recipientRatePair1 = CommissionTestObjectCreator.GetNewRecipientRatePair(TestFactory);
			var recipientRatePair2 = CommissionTestObjectCreator.GetNewRecipientRatePair(TestFactory, "TST");

			var arTransaction = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(TestFactory, 1, true, LedgerTypes.AccountsReceivable, null, "10001", recipientRatePair: recipientRatePair1);
			var apTransaction = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(TestFactory, 1, true, LedgerTypes.AccountsPayable, null, "10002", recipientRatePair: recipientRatePair1);

			CommissionTestObjectCreator.SetLineValuesFromRecipientRatePair(arTransaction.AccCommissionHeader.Lines.AddNew(), recipientRatePair2);

			var shipment = objectCreator.CreateShipment("S000001");
			var job = objectCreator.CreateJob(shipment);

			var jobTransaction = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(TestFactory, 1, true, LedgerTypes.AccountsPayable, null, "10003", job: job, recipientRatePair: recipientRatePair1);

			TestFactory.Save();
		}

		public void SetupCommissionDataForArchivedJobs()
		{
			var objectCreator = new TestObjectCreator(TestFactory);

			var recipientRatePair1 = CommissionTestObjectCreator.GetNewRecipientRatePair(TestFactory);

			var shipment = objectCreator.CreateShipment("S000001");
			shipment.JS_IsForwardRegistered = false;
			var job = objectCreator.CreateJob(shipment);

			var jobTransaction = CommissionTestObjectCreator.CreateNewTransactionWithCommissionableLines(TestFactory, 1, true, LedgerTypes.AccountsPayable, null, "10003", job: job, recipientRatePair: recipientRatePair1);
			jobTransaction.AccCommissionHeader.CH0_GroupingSourceID = ZGuid.Empty;

			TestFactory.Save();
		}
		public override BusinessObject[] GetSelectedBusinessObjects()
		{
			// Otherwise groupings will be in random order causing test to fail.
			return Groupings.OrderBy(x => x.SourceNumber).ToArray();
		}

		protected override void QueueSourceItemsForRegeneration(IEnumerable<ViewCommissionLineGrouping> groupings)
		{
			CommissionRegenerator.QueueSourceItemsForRegeneration(TestFactory, groupings);
		}

		public readonly BusinessObjectFactory TestFactory;

		List<ViewCommissionLineGrouping> Groupings => groupings ?? (groupings = CommissionTestObjectCreator.GetGroupings(Factory));
		List<ViewCommissionLineGrouping> groupings;

		internal bool ShouldLoadTop1WhenGridEmptyExposed => base.ShouldLoadTop1WhenGridEmpty;
		public BusinessObjectFactory Factory_ExposedForTest => Factory;
	}

	class CommissionFlattenedDataTransferProcessorForTest : CommissionFlattenedDataTransferProcessor
	{
		public CommissionFlattenedDataTransferProcessorForTest(AccCommissionLineCollection commissionLineCollection, CommissionImportCollectionInfo flattenedCollectionInfo)
			: base(commissionLineCollection, flattenedCollectionInfo)
		{
		}

		public void SetNewCountForTesting(int value)
		{
			NewCount = value;
		}

		public void SetOverriddenCountForTesting(int value)
		{
			OverriddenCount = value;
		}

		public void SetIgnoredCountForTesting(int value)
		{
			IgnoredCount = value;
		}

		public void SetErrorCountForTesting(int value)
		{
			ErrorCount = value;
		}

		public void AddLogForTesting(string value)
		{
			LogList.Add(value);
		}
	}
}
