using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.ZArchitecture.GUI.ZForm;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalForm))]
	public class GLJournalFormTest : ZFormBasherTest
	{
		public void TestApprovalAuditTab()
		{
			var journal = Factory.New<GLJournal>();
			var glJournalForm = new GLJournalForm(journal);
			glJournalForm.Show();
			AssertEquals("There should be a Audit Tab", glJournalForm.TopLevelControl.Controls[0].Controls[1].Name, "ApprovalsTab");
			glJournalForm.Close();
		}

		public void TestShowGLAccountsForImportAction()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("TRR");
			Factory.Save();

			var alternateAccount = creator.CreateAccAlternateGlAccount(chart.PK, "111", Core.Constants.AccountType.BalanceSheetAccount);
			var glHeader = creator.CreateGLHeader("3333.33.33");
			var glHeader2 = creator.CreateGLHeader("4444.33.33");
			creator.CreateAccAlternateGlAccountAttribute(alternateAccount, glHeader.PK);
			creator.CreateAccAlternateGlAccountAttribute(alternateAccount, glHeader2.PK);
			Factory.Save();

			var journal = Factory.New<GLJournal>();
			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid()))
			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				AssertNotNull(journal.Lines.ShowGLAccountsForImportAction);
				var grid = (ZGrid)form.Controls.Find("JournalLinesGrid", true)[0];
				grid.SetDataBinding(journal, "GLJournalLines");
				Application.DoEvents();
				var style = grid.Columns["AL_AG"].ColumnStyle;
				grid.BeginEdit(style, 0);
				var codeBox = ((ZGridGuidFindBox)grid.LastFocusedColumn.EditControl).CodeBox;
				codeBox.Text = "111";
				form.Controls.Find("DescriptionTextBox", true)[0].Focus();
				Application.DoEvents();
				var selectionForm = (GLAccountSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(selectionForm);
			}
		}

		public void TestSavingFormDeleteGLDDataWhenGLJournLineDeleted()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);

			var glJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2013, 08, 13), new ZDateTime(2022, 08, 13));
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			TestObjectCreator.MockNudgeGLDProcessData([((INeedRow)line1).Row, ((INeedRow)line2).Row]);

			var sqlCheckAmount = $"Select * From dbo.AccGeneralLedgerData Where GLD_AL_TransactionLine = '{line2.PK.ToGuid()}'";
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlCheckAmount);
			Assert(collection.Any());

			line2.Delete();
			var line3 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			using (var form = new GLJournalForm(glJournal))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();

				collection.RemoveAll();
				collection.Load(sqlCheckAmount);
				Assert(!collection.Any());
			}
		}

		public void TestSavingFormRelinkEdocsToRequest()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form caption", "New GL Journal", form.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.AlwaysCreateApprovalRequest_ForTestOnly = true;

				((IDocManagerSupport)journal).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "INV");
				form.FireSaveButton();

				Assert("Journal is not saved", !journal.IsInDatabase);
				var latestRequestPK = journal.LatestLinkedApprovalRequestPK;
				AssertNotNull("Request should be created", latestRequestPK);
				DocumentFactory documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				StorageMain storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, latestRequestPK));
				AssertNotNull("storageMain should be saved", storageMain);
				AssertEquals("should link to request", latestRequestPK, storageMain.SM_ParentFK);
				AssertEquals("GJR", storageMain.SM_Type);
				AssertEquals(1, storageMain.eDocs.Count);

				((IDocManagerSupport)journal).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 4, 5, 6 }), "TestFile2", "INV");
				form.FireSaveButton();

				Assert("Journal is not saved", !journal.IsInDatabase);
				latestRequestPK = journal.LatestLinkedApprovalRequestPK;
				AssertNotNull("Lastest request should not be null", latestRequestPK);
				documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, latestRequestPK));
				AssertNotNull("storageMain should be saved", storageMain);
				AssertEquals("should link to request", latestRequestPK, storageMain.SM_ParentFK);
				AssertEquals("GJR", storageMain.SM_Type);
				AssertEquals(2, storageMain.eDocs.Count);
			}
		}

		public void TestSavingFormRelinkEdocsToRequest_ThrowsLastExternalStorageException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form caption", "New GL Journal", form.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.AlwaysCreateApprovalRequest_ForTestOnly = true;

				((IDocManagerSupport)journal).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "INV");
				form.FireSaveButton();

				Assert("Journal is not saved", !journal.IsInDatabase);
				var latestRequestPK = journal.LatestLinkedApprovalRequestPK;
				AssertNotNull("Request should be created", latestRequestPK);
				DocumentFactory documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				StorageMain storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, latestRequestPK));
				AssertNotNull("storageMain should be saved", storageMain);
				AssertEquals("should link to request", latestRequestPK, storageMain.SM_ParentFK);
				AssertEquals("GJR", storageMain.SM_Type);
				AssertEquals(1, storageMain.eDocs.Count);

				((IDocManagerSupport)journal).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 4, 5, 6 }), "TestFile2", "INV");

				// Setup to throw LastExternalStorageException
				journal.Factory.SetContext(GLJournalApprovalRequest.Context.Editing);
				StorageFile document = (StorageFile)storageMain.eDocs[0];

				// The emptying of SC_ImageData normally runs after SaveToExternalStorage and IsMovingToExternalStorage will be set to true to avoid RetrieveFromExternalStorage
				var propertyInfo = typeof(StorageDocsWithS3Support).GetProperty("IsMovingToExternalStorage", BindingFlags.Instance | BindingFlags.Public);
				propertyInfo.SetValue(document, true);
				document.SC_ImageData = ZBlob.Empty;
				storageMain.Factory.Save();

				AssertNoExceptionThrown("S3 exception should be handled", () => form.FireSaveButton());
				AssertContains(AllocateErrorMessage.LastExternalStorageExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Journal is not saved", !journal.IsInDatabase);
				latestRequestPK = journal.LatestLinkedApprovalRequestPK;
				AssertNotNull("Lastest request should not be null", latestRequestPK);
				documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, latestRequestPK));
				AssertNotNull("storageMain should be saved", storageMain);
				AssertEquals("should link to request", latestRequestPK, storageMain.SM_ParentFK);
				AssertEquals("GJR", storageMain.SM_Type);
				AssertEquals("2nd eDocs shouldn't be saved because of LastExternalStorage exception", 1, storageMain.eDocs.Count);
			}
		}

		public void TestSavingFormAddsDocumentToEdocs()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("There Shouldn't be any edoc", 0, journal.DocManagerInfo.AllEDocs.Count);

				form.FireSaveButton();

				AssertEquals("There Should be 1 edoc", 1, journal.DocManagerInfo.AllEDocs.Count);
			}
		}

		public void TestSavingFormShouldNotAddDocumentToEdocsWhenNoChanges()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("There Shouldn't be any edoc", 0, journal.DocManagerInfo.AllEDocs.Count);

				form.FireSaveButton();

				AssertEquals("There Should be 1 edoc", 1, journal.DocManagerInfo.AllEDocs.Count);

				var originalValue = journal.Lines.OfType<GLJournalLine>().FirstOrDefault().UnsignedOSLineAmount;
				journal.Lines.OfType<GLJournalLine>().FirstOrDefault().UnsignedOSLineAmount = originalValue + 10m;
				journal.Lines.OfType<GLJournalLine>().FirstOrDefault().UnsignedOSLineAmount = originalValue;

				form.FireSaveButton();
				AssertEquals("There still should be 1 edoc", 1, journal.DocManagerInfo.AllEDocs.Count);
			}
		}

		#region Plug-ins

		public void TestDataExportBatchPluginIsAdded()
		{
			using (var form = (GLJournalForm)GetFormToBashCore())
			{
				var source = form.BusinessEntity as IDataExportBatchSource;
				AssertNotNull("Precondition: entity is IDataExportBatchSource", source);
				AssertNotNull("Precondition: IDataExportBatchSource entity has support", source.IsDataExportBatchSupported);
				AssertNotNull("IDataExportBatchSource entity should have plugin", form.PlugIns.GetPlugIn(ControllerIDs.DataExportBatchPlugin));
			}
		}

		#endregion

		#region Approval Requests

		public void TestClickNoWhenPreSaving()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var glNoteJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.GLHeader2.AG_AccountType = Core.Constants.AccountType.Note;
			TestObjectCreator.CreateGLJournalLine(glNoteJournal, 10, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
			AssertClickNoWhenPreSaving(glNoteJournal);

			var glStandardJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(glStandardJournal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			AssertClickNoWhenPreSaving(glStandardJournal);

			void AssertClickNoWhenPreSaving(GLJournal journal)
			{
				var newValue = new GLJournalApprovalThresholdCollection();
				var threshold = newValue.AddNew();
				threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
				var settings = threshold.AuthorisationSettings.AddNew();
				settings.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
				settings.Amount = 0;
				settings.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
				AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

				using (var form = new GLJournalForm(journal))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					AssertStartsWith("Form caption", "New GL Journal", form.Text);

					Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
					AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

					journal.RunPreSaveValidation();
					AssertNoErrors("Precondition", journal);

					form.FireSaveButton();

					if (journal.IsNoteJournal)
					{
						AssertNull("Precondition: the system will not prompt user to post the imbalance to the 'GL Journal Clearing Account'.", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert("Note Journal is saved.", journal.IsInDatabase);
					}
					else
					{
						AssertEquals("Precondition: Unbalanced Journal message should be shown.", "Journal does not balance. Would you like the balance to be posted to the 'GL Journal Clearing Account'?", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert("Journal is not saved as user clicked No.", !journal.IsInDatabase);
					}
				}
			}
		}

		public void TestAuthorizationOnSaving()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
			var settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			settings.Amount = 0;
			settings.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			using (var form = new GLJournalForm(journal))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form caption", "New GL Journal", form.Text);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.FireSaveButton();
				AssertEquals("Precondition: Unbalanced Journal message should be shown.", "Journal does not balance. Would you like the balance to be posted to the 'GL Journal Clearing Account'?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertType<LoginFormWithRequest>("Should prompt login form", ZFormModaliser.LastFormShownDialogForTest);
				Assert("Journal is not saved as there is no security rights.", !journal.IsInDatabase);
				AssertEquals("DisplayMode should not be set as after saving as saving is canceled.", ODisplayMode.Edit, form.DisplayMode);
				AssertStartsWith("Form caption", "New GL Journal", form.Text);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
				AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.FireSaveButton();
				AssertType<LoginFormWithRequest>("Should prompt login form", ZFormModaliser.LastFormShownDialogForTest);
				Assert("Journal is not saved as user has no rights to approve own journal.", !journal.IsInDatabase);
				AssertEquals("DisplayMode should not be set as after saving as saving is canceled.", ODisplayMode.Edit, form.DisplayMode);
				AssertStartsWith("Form caption", "New GL Journal", form.Text);

				AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.FireSaveButton();
				AssertNull("Should not prompt login form", ZFormModaliser.LastFormShownDialogForTest);
				Assert("Journal is saved as user has rights to approve journal.", journal.IsInDatabase);
				AssertEquals("DisplayMode should be changed as journal is saved.", ODisplayMode.Browse, form.DisplayMode);
				AssertStartsWith("Form caption", "Edit GL Journal", form.Text);
			}
		}

		public void TestAuthorizationOnSavingWithRequest()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form caption", "New GL Journal", form.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.AlwaysCreateApprovalRequest_ForTestOnly = true;
				form.FireSaveButton();
				Assert("Journal HasNotPostedApprovalRequest", journal.HasNotPostedApprovalRequest);
				Assert("Journal is not saved", !journal.IsInDatabase);
				AssertEquals("DisplayMode.", ODisplayMode.Browse, form.DisplayMode);
				AssertStartsWith("Form caption", "New Waiting Approval GL Journal", form.Text);

				journal.AH_Desc += "Desc"; //edit invoice
				form.FireSaveButton();
				var expectedMessage = @"There is another request for this journal. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?";
				AssertEquals("User should be asked to cancel previous request.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAuthorizationOnSavingForEditingRequest()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var journal_NewForRequest = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal_NewForRequest, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal_NewForRequest, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal_NewForRequest.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal_NewForRequest);

			var newFactory = new BusinessObjectFactory();
			var requestInNewFactory = newFactory.New<GLJournalApprovalRequest>();
			requestInNewFactory.Initialize(journal_NewForRequest);
			requestInNewFactory.Factory.Save();

			ReleaseFactory();
			var request = Factory.Load<GLJournalApprovalRequest>(requestInNewFactory.PK);
			var journal = requestInNewFactory.GetLinkedJournal().journal;
			Assert("Precondition: Journal should not be saved.", !journal.IsInDatabase);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			journal.SetContext(GLJournalApprovalRequest.Context.Editing);
			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form caption", "New Waiting Approval GL Journal", form.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				Assert("Journal HasNotPostedApprovalRequest", journal.HasNotPostedApprovalRequest);
				Assert("Journal is not saved.", !journal.IsInDatabase);
				AssertEquals("DisplayMode", ODisplayMode.NewSaved, form.DisplayMode);
				AssertStartsWith("Form caption", "New Waiting Approval GL Journal", form.Text);
				var expectedMessage = @"There is another request for this journal. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?";
				AssertEquals("User should be asked to cancel previous request.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAuthorizationOnSavingForPostingRequest()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var journal_NewForRequest = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal_NewForRequest, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal_NewForRequest, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal_NewForRequest.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal_NewForRequest);

			var newFactory = new BusinessObjectFactory();
			var requestInNewFactory = newFactory.New<GLJournalApprovalRequest>();
			requestInNewFactory.Initialize(journal_NewForRequest);
			requestInNewFactory.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			requestInNewFactory.Factory.Save();

			ReleaseFactory();
			var request = Factory.Load<GLJournalApprovalRequest>(requestInNewFactory.PK);
			var journal = requestInNewFactory.GetLinkedJournal().journal;
			Assert("Precondition: Journal should not be saved.", !journal.IsInDatabase);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			journal.SetContext(GLJournalApprovalRequest.Context.Posting);
			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form caption", "Post GL Journal", form.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Assert("Precondition: Journal HasNotPostedApprovalRequest", journal.HasNotPostedApprovalRequest);
				Assert("Precondition: Journal is not saved.", !journal.IsInDatabase);
				AssertEquals("Precondition: request parent id", ZGuid.Empty, request.XP_ParentID);
				form.FireSaveButton();
				Assert("Journal HasNotPostedApprovalRequest", !journal.HasNotPostedApprovalRequest);
				Assert("Journal is saved.", journal.IsInDatabase);
				AssertEquals("DisplayMode", ODisplayMode.NewSaved, form.DisplayMode);
				AssertStartsWith("Form caption", "Post GL Journal", form.Text);
				AssertNull("User should be not asked about anything.", UnitTestUserNotification.Instance.LastMessage.Text);
				var savedRequest = new BusinessObjectFactory().Load<GLJournalApprovalRequest>(request.PK);
				AssertEquals("request status", Core.Constants.GenApprovalRequestApprovalStatus.Posted, savedRequest.XP_ApprovalStatus);
				AssertEquals("request parent id", journal.PK, savedRequest.XP_ParentID);
				AssertNotEquals("journal AH_TransactionNum", ZString.Empty, journal.AH_TransactionNum);
				AssertEquals("request AH_TransactionNum", journal.AH_TransactionNum, savedRequest.PostingDetails.Journal.AH_TransactionNum);
			}
		}

		public void TestApprovalRequestIsNotCreatedWhenRevertingChangesOnExistingJournal()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();
			Assert("Journal is saved", journal.IsInDatabase);
			var previousApprovalCount = journal.Approvals.Count;
			var originalHeaderDescription = journal.AH_Desc;

			using (var form = new GLJournalForm(journal))
			{
				form.Show();
				Application.DoEvents();
				journal.AH_Desc = "test description";
				journal.AH_Desc = originalHeaderDescription;
				form.FireSaveButton();
				AssertEquals("A new Approval Request should not be created because there are no persistent changes", previousApprovalCount, journal.Approvals.Count);
			}
		}

		public void TestJournalSavingWithApprovalRequestsDoesntTriggerRequestXMLDeserialization()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new GLJournalForm(journal))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
				GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly = 0;

				form.FireSaveButton();

				Assert("Postcondition: Journal is not saved as there is no security rights.", !journal.IsInDatabase);
				AssertEquals("Postcondition: ApprovalRequestStatus", Core.Constants.GenApprovalRequestApprovalStatus.Requested, journal.ApprovalRequestStatus);
				AssertEquals("ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly", 0, GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly);
			}
		}

		#endregion

		#region Reverse Approval Request

		public void TestReverseJournalWithSecurityRight()
		{
			var originalJournal = CreateJournal();

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
			Assert(Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GetGLJournalApprovalThresholdSetup()))
			{
				ZController controller = ZControllerFactory.Create(ControllerIDs.GLJournal);
				using (var reverseForm = controller.ShowDeleteForm(originalJournal) as GLJournalForm)
				{
					AssertNotNull(reverseForm);
					reverseForm.HandleApplyPostingButtonClickUnsafe_ForTestOnly(true);
				}
				var newFactory = new BusinessObjectFactory();
				var originalJournalInNewFactory = newFactory.Load<GLJournal>(originalJournal.PK);
				Assert(originalJournalInNewFactory.IsReversed);
				var approvalRequest = newFactory.Load<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, originalJournal.PK));
				AssertEquals(0, approvalRequest.Length);

				var reverseJournalInNewFactory = newFactory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, originalJournal.AH_TransactionBelongsToGroup)).First(x => x.PK != originalJournal.PK);
				Assert(reverseJournalInNewFactory.IsReversed);
				AssertEquals(originalJournalInNewFactory.AH_TransactionBelongsToGroup, reverseJournalInNewFactory.AH_TransactionBelongsToGroup);

				approvalRequest = newFactory.Load<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, reverseJournalInNewFactory.PK));
				AssertEquals(1, approvalRequest.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest.First().XP_ApprovalStatus);
			}
		}

		public void TestReverseJournalWithoutSecurityRight()
		{
			var journal = CreateJournal();

			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = false;
			Assert(!Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, GetGLJournalApprovalThresholdSetup()))
			{
				SecurityOverrideProviderSource.Get(journal).Provider = new GLJournalSecurityOverrideProvider();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;
					var approvalBulkForm = form as GLJournalApprovalBulkForm;
					if (loginForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
					}
					else if (approvalBulkForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user clicks continue button on approval form
					}
				});

				ZController controller = ZControllerFactory.Create(ControllerIDs.GLJournal);
				using (var reverseForm = controller.ShowDeleteForm(journal) as GLJournalForm)
				{
					AssertNotNull(reverseForm);
					reverseForm.HandleApplyPostingButtonClickUnsafe_ForTestOnly(true);

					var approvalForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertType<GLJournalApprovalBulkForm>(ZFormModaliser.LastFormShownDialogForTest);
				}

				var newFactory = new BusinessObjectFactory();
				var journalInNewFactory = newFactory.Load<GLJournal>(journal.PK);
				Assert(!journalInNewFactory.IsReversed);
				var approvalRequest = newFactory.Load<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.PK, journal.AH_TransactionBelongsToGroup));
				AssertEquals(1, approvalRequest.Length);
			}
		}

		GLJournal CreateJournal()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());

			periodHelper.SetupPeriods();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();
			return journal;
		}

		GLJournalApprovalThresholdCollection GetGLJournalApprovalThresholdSetup()
		{
			var newValue = new GLJournalApprovalThresholdCollection();
			var newThreshold = newValue.AddNew();
			newThreshold.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			return newValue;
		}

		#endregion

		public void TestGLJournalUserControl()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			using (var form = new GLJournalForm(glJournal))
			{
				form.Show();
				var glJournalUserControl = form.GetControl<GLJournalUserControl>("glJournalUserControl");
				Assert("ShowApprovalRequestControls", glJournalUserControl.ShowApprovalRequestControls);
			}
		}

		public void TestFormCaption()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			using (var form = new GLJournalForm(glJournal))
			{
				AssertEquals("GL Journal", form.FormCaption);

				Factory.SetContext(GLJournalApprovalRequest.Context.Editing);
				AssertEquals("Waiting Approval GL Journal", form.FormCaption);

				Factory.RemoveContext(GLJournalApprovalRequest.Context.Editing);
				Factory.SetContext(GLJournalApprovalRequest.Context.Posting);
				AssertEquals("GL Journal", form.FormCaption);
				Factory.RemoveContext(GLJournalApprovalRequest.Context.Posting);
			}

			var fcbJournal = Factory.NewWithValidTestData<FCBAdjustmentJournal>();
			fcbJournal.AH_ReceiptType = ReceiptTypes.ForeignCurrencyBalance;
			using (var form = new GLJournalForm(fcbJournal))
			{
				AssertEquals("Foreign Currency Balance Adjustment Journal", form.FormCaption);

				Factory.SetContext(GLJournalApprovalRequest.Context.Editing);
				AssertEquals("Waiting Approval Foreign Currency Balance Adjustment Journal", form.FormCaption);

				Factory.RemoveContext(GLJournalApprovalRequest.Context.Editing);
				Factory.SetContext(GLJournalApprovalRequest.Context.Posting);
				AssertEquals("Foreign Currency Balance Adjustment Journal", form.FormCaption);
				Factory.RemoveContext(GLJournalApprovalRequest.Context.Posting);
			}
		}

		public void TestValidateAndSaveNullRef()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			Factory.Save();

			((IDbConnected)Factory).Connection.ExecuteNonQuery(
				string.Format(@"Delete From dbo.AccTransactionHeader WHERE AH_PK = '{0}'", glJournal.PK));

			using (var form = new GLJournalForm(glJournal))
			{
				AssertNoExceptionThrown(() => { form.ValidateAndSave_ForTestOnly(); });
			}
		}

		[TestDate(2014, 2, 1)]
		public void TestValidateAfterDeactivateAccount()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Now.Year);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, new ZDateTime(2014, 02, 01), new ZDateTime(2014, 02, 01));
			journal.PostPeriod = 201401;
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);

			Factory.Save();

			var periodManager = new PeriodManager(Factory);
			periodManager.CloseSubLedgerPeriod();
			var period = periodManager.CloseGLPeriod();
			AssertEquals(true, period.AM_IsGeneralLedgerClosed);

			ZQuery query = new ZQuery(AccGLHeaderSchema.PK, line1.AL_AG);
			var glAccount = Factory.LoadTop1<AccGLHeader>(query);
			glAccount.AG_IsActive = false;

			query = new ZQuery(GlbDepartmentSchema.PK, line1.AL_GE);
			var department = Factory.LoadTop1<GlbDepartment>(query);
			department.GE_IsActive = false;
			Factory.Save();

			journal.RunPreSaveValidation();
			AssertNoErrors("Precondition", journal);
		}

		[TestDate(2014, 2, 1)]
		[ExpectNoExceptions("Unable to cast object of type 'Enterprise.Accounting.Business.ARAP.Invoicing.TransactionLineEmptyValidation' to type 'Enterprise.Accounting.Business.GeneralLedger.GLJournals.GLJournalLineValidation'")]
		public void TestValidateAfterClosedPeriod()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Now.Year);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, new ZDateTime(2014, 02, 01), new ZDateTime(2014, 02, 01));
			journal.PostPeriod = 201401;
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			var periodManager = new PeriodManager(Factory);
			periodManager.CloseSubLedgerPeriod();
			periodManager.CloseGLPeriod();
			periodManager.CloseGLPeriodForAdjustments();

			line1.AL_AG = TestObjectCreator.GLHeader2.PK;
			line1.DebitCreditSign = nameof(DebitCredit.DR);
			line1.UnsignedOSLineAmount = 20;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.DebitCreditSign = nameof(DebitCredit.CR);
			line2.UnsignedOSLineAmount = 20;
			Factory.Save();
		}

		[TestDate(2020, 4, 21)]
		public void TestReverseOrCopySubAccounts_WithFormAction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var periodManagementTestHelper = new AccountingPeriodTestHelper();
				periodManagementTestHelper.SetupPeriods();

				var glHeader = TestObjectCreator.CreateGLHeader();
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, true);
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbGroupSchema.Constants.Prefix, true);
				Factory.Save();

				var glStandardGLJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
				AssertReverseOrCopySubAccounts_WithFormAction(glStandardGLJournal);

				var glAutoJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
				AssertReverseOrCopySubAccounts_WithFormAction(glAutoJournal);

				var glReversingJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
				AssertReverseOrCopySubAccounts_WithFormAction(glReversingJournal);

				var adjustmentJournal = TestObjectCreator.CreateGLJournal<FCBAdjustmentJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
				AssertReverseOrCopySubAccounts_WithFormAction(adjustmentJournal);

				void AssertReverseOrCopySubAccounts_WithFormAction(GLJournal journal)
				{
					var line = journal.Lines.AddNew();
					line.AL_AG = glHeader.PK;
					line.AL_Calc_FirstSubClassParentId = TestObjectCreator.ABIGAS.PK;
					line.AL_Calc_SecondSubClassParentId = TestObjectCreator.GS1.PK;
					Factory.Save();
					Assert(line.IsInDatabase);

					var glJournalController = ZControllerFactory.Create(ControllerIDs.GLJournal);
					using (var form = glJournalController.ShowDeleteForm(journal))
					{
						Application.DoEvents();
						AssertReverseOrCopyOfJournal(form.BusinessEntityForPersistingForm as GLJournal);
					}

					using (var form = glJournalController.ShowTemplateCopyForm(journal))
					{
						Application.DoEvents();
						AssertReverseOrCopyOfJournal(form.BusinessEntityForPersistingForm as GLJournal);
					}
				}

				void AssertReverseOrCopyOfJournal(GLJournal journal)
				{
					journal.Lines.SetReadOnlyIncludingChildren(false);
					AssertEquals(1, journal.Lines.Count);
					AssertEquals(TestObjectCreator.ABIGAS.PK, journal.Lines[0].AL_Calc_FirstSubClassParentId);
					AssertEquals(TestObjectCreator.GS1.PK, journal.Lines[0].AL_Calc_SecondSubClassParentId);

					var subAccounts = (journal.Lines[0] as GLJournalLine).SubAccounts;
					AssertEquals(3, subAccounts.Count);
					Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == OrgHeaderSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.ABIGAS.PK));
					Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == GlbStaffSchema.Constants.Prefix && x.SubAccountParentId == TestObjectCreator.GS1.PK));
					Assert(subAccounts.SubAccountElements.Any(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix && x.SubAccountParentId == ZGuid.Empty));

					var subAccountGlbGroup = subAccounts.SubAccountElements.First(x => x.SubAccountTypeParentTableCode == GlbGroupSchema.Constants.Prefix);
					journal.RunPreSaveValidation();
					Assert(journal.NotificationsIncludingChildren.Contains("Error - AL1_SubClassParentId: Please enter a Sub Account."));

					subAccountGlbGroup.SubAccountParentId = TestObjectCreator.GG1.PK;
					journal.RunPreSaveValidation();
					Assert(!journal.NotificationsIncludingChildren.Contains("Error - AL1_SubClassParentId: Please enter a Sub Account."));
				}
			}
		}

		#region TestFormBecomesReadOnlyAfterACriticalValidationError

		[TestDate(2014, 1, 1)]
		public void TestFormBecomesReadOnlyAfterACriticalValidationError()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year);
			Factory.Save();

			var testJournal = testObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, new ZDateTime(ZDateTime.Now.Year, 06, 01), new ZDateTime(ZDateTime.Now.Year, 06, 01));
			testObjectCreator.CreateGLJournalLine(testJournal, 256m, DebitCredit.DR, testObjectCreator.GLHeader1.PK);
			testObjectCreator.CreateGLJournalLine(testJournal, 256m, DebitCredit.CR, testObjectCreator.GLHeader2.PK);
			Factory.Save();

			var bizo = Factory.Load<DummyGLJournalCriticalValidationParent>(testJournal.PK);
			bizo.AH_PostPeriod = 201506;
			using (var form = new TransactionViewFormTestForCriticalValidationException(bizo))
			{
				AssertEquals("Context (Before Critical Validation Error)", false, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertNotEquals("DisplayMode (Before Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);

				bizo.CriticalValidation.RegisterOnSavingCheck();
				form.DisplayMode = ODisplayMode.New;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				form.Show();
				form.OnPostButtonClick_ForTestOnly(null, null);

				AssertEquals("Context (After Critical Validation Error)", true, form.BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation));
				AssertEquals("DisplayMode (After Critical Validation Error)", ODisplayMode.ReadOnly, form.DisplayMode);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		class TransactionViewFormTestForCriticalValidationException : GLJournalForm
		{
			public TransactionViewFormTestForCriticalValidationException(DummyGLJournalCriticalValidationParent bizo)
				: base(bizo)
			{
			}
		}

		class DummyGLJournalCriticalValidationParent : GLJournal, ISupportCriticalValidation
		{
			public DummyGLJournalCriticalValidationParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(this); }
			}

			#endregion
		}

		class DummyCriticalValidation : CriticalValidation<DummyGLJournalCriticalValidationParent>
		{
			public DummyCriticalValidation(DummyGLJournalCriticalValidationParent parent)
				: base(parent)
			{
			}

			protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
			}

			protected override IEnumerable<CriticalValidationResult> DeletedObjectOnSavingCriticalChecks()
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.DummyErrorKeyForTest, ResString.GetMultilingualString("d6217571-847f-472f-9347-47ec07e7c256", "Test error message."), "E=MC2");
			}
		}

		#endregion

		public void TestAnotherUserCurrentlyAccessingThisEntity()
		{
			var initiator = Factory.NewWithValidTestData<GlbStaff>();
			initiator.GS_LoginName = "David Park";
			initiator.GS_Code = "DP";

			Factory.Save();

			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			using (var form1 = new GLJournalForm(glJournal))
			{
				form1.Show();
				Application.DoEvents();

				AssertEquals("The form shouldn't be readonly", ODisplayMode.Edit, form1.DisplayMode);
				Assert("The control on the form shouldn't be readonly", !form1.GlJournalUserControl_ForTestOnly.JournalLinesGrid.GetReadOnly());

				using (Env.SetTemporaryUserContext(initiator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				using (var form2 = new GLJournalForm(glJournal))
				{
					form2.Show();
					Application.DoEvents();

					Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("The GL Journal is currently being edited by user 'CargoWise Support'"));
					AssertEquals("The form should be readonly", ODisplayMode.ReadOnly, form2.DisplayMode);
					Assert("The control on the form should be readonly", form2.GlJournalUserControl_ForTestOnly.JournalLinesGrid.GetReadOnly());
				}
			}

			using (var form3 = new GLJournalForm(glJournal))
			{
				form3.Show();
				Application.DoEvents();

				AssertEquals("The form shouldn't be readonly", ODisplayMode.Edit, form3.DisplayMode);
				Assert("The control on the form shouldn't be readonly", !form3.GlJournalUserControl_ForTestOnly.JournalLinesGrid.GetReadOnly());

				form3.Close();
			}
		}

		public void TestBranchNameAndDepartmentDescriptionColumnsIsVisibleAndIsReadOnly()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			using (var form = new GLJournalForm(glJournal))
			{
				form.Show();
				Application.DoEvents();

				var glJournalUserControl = form.GetControl<GLJournalUserControl>("glJournalUserControl");
				var linesGrid = glJournalUserControl.JournalLinesGrid;
				AssertEquals("BranchName column should be able.", true, linesGrid.Columns.Contains(GLJournalLine.Schema.BranchName));
				AssertEquals("DepartmentDescription column should be able.", true, linesGrid.Columns.Contains(GLJournalLine.Schema.DepartmentDescription));
				AssertEquals("BranchName should be hidden by default.", false, linesGrid.GetColumnStyle(GLJournalLine.Schema.BranchName).IsVisible);
				AssertEquals("DepartmentDescription column should be hidden by default.", false, linesGrid.GetColumnStyle(GLJournalLine.Schema.DepartmentDescription).IsVisible);
				AssertEquals("BranchName column should be readonly by default.", true, linesGrid.GetColumnStyle(GLJournalLine.Schema.BranchName).IsReadOnly);
				AssertEquals("DepartmentDescription column should be readonly by default.", true, linesGrid.GetColumnStyle(GLJournalLine.Schema.DepartmentDescription).IsReadOnly);
			}
		}

		[TestDate(2020, 3, 12)]
		public void TestSubAccountsIsAutoGenenatedWithFormAction()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();
			var currentPeriod = periodManagementTestHelper.CurrentPeriod;

			var glStandardGLJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			TestSubAccountsIsAutoGenenatedWithGLJournal(glStandardGLJournal, typeof(GLJournal));

			var glAutoJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			TestSubAccountsIsAutoGenenatedWithGLJournal(glAutoJournal, typeof(GLJournal));

			var glReversingJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			TestSubAccountsIsAutoGenenatedWithGLJournal(glReversingJournal, typeof(GLJournal));

			var adjustmentJournal = TestObjectCreator.CreateGLJournal<FCBAdjustmentJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Now, ZDateTime.Now);
			TestSubAccountsIsAutoGenenatedWithGLJournal(adjustmentJournal, typeof(FCBAdjustmentJournal));

			void TestSubAccountsIsAutoGenenatedWithGLJournal(GLJournal journal, Type type)
			{
				currentPeriod.AM_IsGeneralLedgerClosed = false;
				currentPeriod.Factory.Save();

				var glJournal1 = journal;
				glJournal1.PostPeriod = currentPeriod.AM_Period;
				var glHeader1 = TestObjectCreator.CreateGLHeader();
				TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, false);
				var line1 = glJournal1.Lines.AddNew();
				line1.AL_AG = glHeader1.PK;
				line1.SubAccounts.FirstSubAccount.AL1_SubClassParentId = TestObjectCreator.AALSHI.PK;
				Factory.Save();

				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal1, true, 1);
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal1, false, 1);

				TestObjectCreator.CreateGLHeaderSubAccount(glHeader1, GlbStaffSchema.Constants.Prefix, true);
				Factory.Save();

				var glJournal2 = (GLJournal)Factory.CreateNewFactory().Load(type, journal.PK);
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal2, true, 1);
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal2, false, 2);

				var glJournal3 = (GLJournal)Factory.CreateNewFactory().Load(type, journal.PK);
				var mutex = new ZArchitecture.Data.Mutex.ZGlobalMutex(MutexIDs.GLJournalForm, $"{glJournal3.PK}_{GlbCompany.CurrentCompany.GC_Code}");
				mutex.Lock();
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal3, true, 1);
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal3, false, 1);
				mutex.Unlock();

				currentPeriod.AM_IsGeneralLedgerClosed = true;
				currentPeriod.Factory.Save();

				var glJournal4 = (GLJournal)Factory.CreateNewFactory().Load(type, journal.PK);
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal4, true, 1);
				AssertSubAccountsIsAutoGenenatedWithFormAction(glJournal4, false, 1);

				void AssertSubAccountsIsAutoGenenatedWithFormAction(GLJournal glJournal, bool isViewForm, int expectedSubAccountCount)
				{
					var controller = ZControllerFactory.Create(ControllerIDs.GLJournal);
					using (var form = (isViewForm ? controller.ShowViewForm(glJournal) : controller.ShowEditForm(glJournal)))
					{
						Application.DoEvents();
						AssertType<GLJournalForm>(form);
						var formGLJournal = (GLJournal)((GLJournalForm)form).BusinessEntity;

						AssertEquals("HasChanges", false, formGLJournal.HasChanges);
						AssertEquals("IsInDatabaseIncludingChildren", true, formGLJournal.IsInDatabaseIncludingChildren);
						AssertEquals("line count", 1, formGLJournal.Lines.Count);
						var line = formGLJournal.Lines[0];
						AssertEquals("sub account count", expectedSubAccountCount, line.SubAccounts.Count);
						if (expectedSubAccountCount == 2)
						{
							var subAccount2 = line.SubAccounts.SecondSubAccount;
							AssertEquals("sub account type", GlbStaffSchema.Constants.Prefix, subAccount2.AL1_SubClassParentTableCode);
							AssertHasError("sub account 2 should have errors", subAccount2.AL1_SubClassParentIdInfo, "Please enter a Sub Account.");
						}
					}
				}
			}
		}

		public void TestAddPeriodQueueWhenReversing()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var periodManager = TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var periodPK = periodManager.Periods[0].PK;
			journal.PeriodPK = periodPK;
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			var reversing = new ReversingFactory().NewReversing(journal);
			reversing.Reverse();

			var reverseJournal = journal.ReverseTransaction as GLJournal;
			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new GLJournalForm(reverseJournal))
			{
				form.Show();
				Application.DoEvents();

				Assert("PeriodPK is valid for reversed journal", reverseJournal.PeriodPK.IsValid);
				form.FireSaveButton();

				var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
				var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
				AssertEquals("AdjustmentQueue Count should be 1", 1, results.Rows.Count);
				AssertEquals("ACA_ParentID", periodPK, results.Rows[0]["ACA_ParentID"]);
				AssertEquals("ACA_ParentTableCode", AccPeriodManagementSchema.Constants.Prefix, results.Rows[0]["ACA_ParentTableCode"]);
				AssertEquals("ACA_GC", GlbCompany.CurrentCompany.PK.ToGuid(), results.Rows[0]["ACA_GC"]);
			}
		}

		public void TestSetReadOnlyIncludingChildren()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var reversing = new GLJournalReversing(journal);
			reversing.Reverse();
			var reverseJournal = journal.ReverseTransaction as GLJournal;

			using (var form = new GLJournalForm(reverseJournal))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.SetReadOnlyIncludingChildren();
				form.Show();
				var glJournalUserControl = form.GetControl<GLJournalUserControl>("glJournalUserControl");
				var descriptionTextBox = glJournalUserControl.GetControl<ZTextBox>("DescriptionTextBox");
				Assert("not readonly for Journal Reversing ", !descriptionTextBox.GetReadOnly());
			}

			using (var form = new GLJournalForm(journal))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.SetReadOnlyIncludingChildren();
				form.Show();
				var glJournalUserControl = form.GetControl<GLJournalUserControl>("glJournalUserControl");
				var descriptionTextBox = glJournalUserControl.GetControl<ZTextBox>("DescriptionTextBox");
				Assert("readonly for non reversing", descriptionTextBox.GetReadOnly());
			}

			var periodManager = TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var periodPK = periodManager.Periods[0].PK;
			journal.PeriodPK = periodPK;
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.GLHeader2.PK);
			Factory.Save();

			reversing = (AutoCurrencyAdjustmentGLJournalReversing)new ReversingFactory().NewReversing(journal);
			reversing.Reverse();
			reverseJournal = journal.ReverseTransaction as GLJournal;
			using (var form = new GLJournalForm(reverseJournal))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.SetReadOnlyIncludingChildren();
				form.Show();
				var glJournalUserControl = form.GetControl<GLJournalUserControl>("glJournalUserControl");
				var descriptionTextBox = glJournalUserControl.GetControl<ZTextBox>("DescriptionTextBox");
				Assert("readonly for AutoCurrency Adjustment GLJournal Reversing", descriptionTextBox.GetReadOnly());
			}
		}

		public void TestShowPreDeleteDialogs()
		{
			var period = Factory.NewWithValidTestData<AccPeriodManagement>();
			Factory.Save();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var reversing = new GLJournalReversing(journal);
			reversing.Reverse();
			var reverseJournal = journal.ReverseTransaction as GLJournal;
			reverseJournal.PostPeriod = 0;

			using (var form = new GLJournalForm(reverseJournal))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.SetReadOnlyIncludingChildren();
				form.Show();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("should not continue with errors", ContinueWithDelete.No, result);
				AssertEquals("PostPeriod should cause invalidate data and cannot save", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}

			reverseJournal.PeriodPK = period.PK;
			using (var form = new GLJournalForm(reverseJournal))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.SetReadOnlyIncludingChildren();
				form.Show();
				var result = form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals("should continue", ContinueWithDelete.Yes, result);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestEditGLJournalWhichIncludedInGLDComplianceReport()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2020, 08, 13), new ZDateTime(2020, 08, 13));
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			var gldPk = CreateDummyGLD(journal);

			Factory.Save();

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var gldReportConfig = CreateConfig("TST", ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData, complianceConfig);
			var ahReportConfig = CreateConfig("STA", ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.TransactionHeader, complianceConfig);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var gldComplianceReportGEN = TestObjectCreator.CreateComplianceReport(gldReportConfig.ReportCode, AccComplianceReport.Status.ReportGenerated);
			var ahComplianceReportFIN = TestObjectCreator.CreateComplianceReport(ahReportConfig.ReportCode, AccComplianceReport.Status.ReportFinalised);

			Factory.Save();

			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{gldPk}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{gldComplianceReportGEN.PK}', 1),
(NEWID(), NEWID(), 'AH', '{GlbCompany.CurrentCompany.PK}', '{ahComplianceReportFIN.PK}', 2)";

			TestConnection.ExecuteNonQuery(sql);

			using (var form = new GLJournalForm(journal))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				journal.AH_Desc += "test";
				form.FireSaveButton();

				AssertComplianceReport(false, true, gldComplianceReportGEN, 0);
			}

			var gldComplianceReportFIN = TestObjectCreator.CreateComplianceReport(gldReportConfig.ReportCode, AccComplianceReport.Status.ReportFinalised);
			Factory.Save();

			sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{gldPk}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{gldComplianceReportFIN.PK}', 3)";

			TestConnection.ExecuteNonQuery(sql);

			using (var form = new GLJournalForm(journal))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				journal.AH_Desc += "test";
				form.FireSaveButton();

				AssertComplianceReport(true, false, gldComplianceReportFIN, 1);
			}

			void AssertComplianceReport(bool hasErrorMessage, bool hasInvReport, AccComplianceReport report, int count)
			{
				var expectedMessage = $"The journal entries of this GL Journal have been included in the finalized compliance report <{report.ACR_ReportType}> and cannot be edited.";

				AssertEquals(hasErrorMessage, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				var newFactory = Factory.CreateNewFactory();
				var complianceReports = newFactory.Load<AccComplianceReport>(new ZQuery());
				AssertEquals(hasInvReport, complianceReports.Any(x => x.ACR_Status == AccComplianceReport.Status.ReportInvalidated && x.PK == report.PK));

				DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccComplianceReportTransactionPivot WHERE ACL_ACR_Report = '{report.PK}'");
				AssertEquals(count, result.Rows.Count);
			}
		}

		[TestDate(2020, 6, 20)]
		public void TestEditGLJournalWhichIncludedInAllTransactionComplianceReport()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

			var journal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			Factory.Save();

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var allTransactionReportConfig = CreateConfig("TST", ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions, complianceConfig);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var complianceReport = TestObjectCreator.CreateComplianceReport(allTransactionReportConfig.ReportCode, AccComplianceReport.Status.ReportCreated);
			complianceReport.ACR_DateFrom = new ZDate(2020, 6, 1);
			complianceReport.ACR_DateTo = new ZDate(2020, 6, 30);
			Factory.Save();
			AssertComplianceReport(journal, complianceReport, hasCannotEditErrorMessage: false, AccComplianceReport.Status.ReportCreated);

			complianceReport.ACR_Status = AccComplianceReport.Status.ReportPendingQueueing;
			Factory.Save();
			AssertComplianceReport(journal, complianceReport, hasCannotEditErrorMessage: false, AccComplianceReport.Status.ReportPendingQueueing);

			complianceReport.ACR_Status = AccComplianceReport.Status.ReportDataQueued;
			Factory.Save();
			CreateAccTransactionComplianceReportQueue();
			AssertAccTransactionComplianceReportQueue(complianceReport, 2);
			AssertComplianceReport(journal, complianceReport, hasCannotEditErrorMessage: false, AccComplianceReport.Status.ReportInvalidated);
			AssertAccTransactionComplianceReportQueue(complianceReport, 0);

			Factory.ReloadAll<AccComplianceReport>();
			complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Factory.Save();
			CreateAccComplianceReportTransactionPivot();
			AssertComplianceReport(journal, complianceReport, hasCannotEditErrorMessage: false, AccComplianceReport.Status.ReportInvalidated);

			Factory.ReloadAll<AccComplianceReport>();
			complianceReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Factory.Save();
			CreateAccComplianceReportTransactionPivot();
			AssertComplianceReport(journal, complianceReport, hasCannotEditErrorMessage: true, AccComplianceReport.Status.ReportFinalised, pivotCount: 2);

			void CreateAccTransactionComplianceReportQueue()
			{
				var sql = $@"
INSERT INTO AccTransactionComplianceReportQueue (ACQ_PK, ACQ_ReportType, ACQ_GC_Company, ACQ_GB_Branch, ACQ_ParentID, ACQ_ParentTableCode, ACQ_ReportSubCode, ACQ_Date) VALUES
(NEWID(), '{allTransactionReportConfig.ReportCode}', '{GlbCompany.CurrentCompany.PK}', '{line1.AL_GB}', '{line1.PK}', 'AL', '*GL*{line1.AL_LineType}**', '{line1.AL_PostDate}'),
(NEWID(), '{allTransactionReportConfig.ReportCode}', '{GlbCompany.CurrentCompany.PK}', '{line2.AL_GB}', '{line2.PK}', 'AL', '*GL*{line2.AL_LineType}**', '{line2.AL_PostDate}')
";
				TestConnection.ExecuteNonQuery(sql);
			}

			void CreateAccComplianceReportTransactionPivot()
			{
				var sql = $@"
INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{line1.PK}', 'AL', '{GlbCompany.CurrentCompany.PK}', '{complianceReport.PK}', 1),
(NEWID(), '{line2.PK}', 'AL', '{GlbCompany.CurrentCompany.PK}', '{complianceReport.PK}', 2)
";
				TestConnection.ExecuteNonQuery(sql);
			}

			void AssertAccTransactionComplianceReportQueue(AccComplianceReport report, int count)
			{
				var queryQueueSql = $@"
SELECT *
FROM AccTransactionComplianceReportQueue
WHERE ACQ_ReportType = '{report.ACR_ReportType}'
	AND ACQ_GC_Company = '{GlbCompany.CurrentCompany.PK}'
	AND ACQ_Date >= '{report.ACR_DateFrom}'
	AND ACQ_Date < '{report.ACR_DateTo.AddDays(1)}'
";
				AssertEquals(count, DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue").Rows.Count);
			}

			void AssertComplianceReport(GLJournal journal, AccComplianceReport report, bool hasCannotEditErrorMessage, string status, int pivotCount = 0)
			{
				{
					using (var form = new GLJournalForm(journal))
					{
						form.DisplayMode = ODisplayMode.Edit;
						form.Show();
						Application.DoEvents();
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						journal.AH_Desc += "test";
						form.FireSaveButton();

						var expectedMessage = $"The journal entries of this GL Journal have been included in the finalized compliance report <{report.ACR_ReportType}> and cannot be edited.";

						AssertEquals(hasCannotEditErrorMessage, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

						var newFactory = Factory.CreateNewFactory();
						var complianceReports = newFactory.Load<AccComplianceReport>(new ZQuery());
						Assert(complianceReports.Any(x => x.ACR_Status == status && x.PK == report.PK));
						AssertEquals(pivotCount, DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM AccComplianceReportTransactionPivot WHERE ACL_ACR_Report = '{report.PK}'").Rows.Count);

						form.Dispose();
					}
				}
			}
		}
		ComplianceReportConfiguration CreateConfig(string reportCode, string tablePrefix, ComplianceReportConfigurationCollection complianceConfig)
		{
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = reportCode;
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportBaseTablePrefix = tablePrefix;
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
			return reportConfig;
		}

		ZGuid CreateDummyGLD(GLJournal gLJournal)
		{
			var narrowGLD = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			narrowGLD.GLD_PostDate = gLJournal.AH_PostDate;
			narrowGLD.GLD_GC_Company = Env.CurrentCompany.PK;
			narrowGLD.GLD_Type = "PST";
			narrowGLD.GLD_GLAccountType = "ARC";
			narrowGLD.GLD_AH_TransactionHeader = gLJournal.PK;
			narrowGLD.GLD_PostPeriod = 202301;
			narrowGLD.GLD_JournalEntriesNumber = "";
			narrowGLD.GLD_JournalEntriesNumberRuleCode = "";
			return narrowGLD.PK;
		}

		[TestDate(2024, 3, 1)]
		public void TestReverseJournalAndPairJournalForChina()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2024);

				Factory.Save();

				var originalJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2024, 03, 31), new ZDateTime(2024, 03, 31));
				var line1 = TestObjectCreator.CreateGLJournalLine(originalJournal, 100M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				var line2 = TestObjectCreator.CreateGLJournalLine(originalJournal, 100M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

				var reversing = new AutoCurrencyAdjustmentGLJournalReversingChina(originalJournal);
				originalJournal.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				originalJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;

				var pairJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2024, 04, 30), new ZDateTime(2024, 04, 30));
				var line3 = TestObjectCreator.CreateGLJournalLine(pairJournal, 100M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				var line4 = TestObjectCreator.CreateGLJournalLine(pairJournal, 100M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

				pairJournal.AH_TransactionType = originalJournal.AH_TransactionType;
				pairJournal.AH_TransactionBelongsToGroup = originalJournal.AH_TransactionBelongsToGroup;
				Factory.Save();

				reversing.Reverse();

				using (var form = new GLJournalForm(originalJournal.ReverseTransaction as GLJournal))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();
					form.FireSaveButton();

					var accGlAggregates = Factory.Load<AccGLAggregate>(new ZQuery());
					AssertEquals(4, accGlAggregates.Length);

					form.Close();
				}
			}
		}

		public void TestFormHasAuditPlugin()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			using var form = new GLJournalForm(glJournal);

			Assert("GLJournalForm should have the Audit plugin.", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			GLJournal journal = Factory.New<GLJournal>();
			return new GLJournalForm(journal);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
