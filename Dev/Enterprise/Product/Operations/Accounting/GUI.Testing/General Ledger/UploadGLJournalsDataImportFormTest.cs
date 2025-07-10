using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.GUI.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(UploadGLJournalsDataImportForm))]
	sealed class UploadGLJournalsDataImportFormTest : DataImporterFormTest
	{
		public void TestImportFileFilter()
		{
			using (var form = new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test))
			{
				AssertEquals("CSV Files (*.csv)|*.csv", form.ImportFileFilter_ForTestOnly);
			}
		}

		[TestDate(2005, 08, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.csv";

				using (var form = new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test))
				{
					form.ImportFromFile_ForTestOnly(fileName);

					var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporter;
					AssertEquals("There will be 2 imported journals.", 2, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly.Count);
					AssertEquals("First Jounal is saved.", true, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly[0].IsInDatabase);
					AssertEquals("Second Jounal is saved.", true, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly[1].IsInDatabase);
				}
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithSavingException()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.csv";
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				using (var form = new UploadGLJournalsDataImportFormForTesting(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test, new MultiCompaniesGLJournalFlatFileDataImporterWithException()))
				{
					form.ImportFromFile_ForTestOnly(fileName);
					var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporter;

					AssertEquals("First Jounal is not saved yet because of exception.", false, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly[0].IsInDatabase);
					AssertEquals("Second Jounal is not saved yet.", false, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly[1].IsInDatabase);
					ErrorReporter.Clear();
				}
			});
		}

		[TestDate(2005, 08, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithError()
		{
			var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\InvalidGJLJournal_InvalidBranch.csv";
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			using (var form = new UploadGLJournalsDataImportFormForTesting(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test, new MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter()))
			{
				form.ImportFromFile_ForTestOnly(fileName);

				var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter;
				AssertEquals(true, multiCompaniesGLJournalFlatFileDataImporter.HasError);
				AssertEquals("There should be no imported journals.", 0, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithAdditionalValidationError()
		{
			var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\InvalidGJLJournal_UnbalancedJournalHeader.csv";
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2005, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			using (var form = new UploadGLJournalsDataImportFormForTesting(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test, new MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter()))
			{
				form.ImportFromFile_ForTestOnly(fileName);

				var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter;
				AssertEquals(true, multiCompaniesGLJournalFlatFileDataImporter.HasError);
				AssertEquals("There should be no saved journals.", false, multiCompaniesGLJournalFlatFileDataImporter.ImportedJournals_ForTestOnly.Any(j => j.IsInDatabase));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 08, 10)]
		public void TestImport_Aggregation()
		{
			AssertAggregation(2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 08, 10)]
		public void TestImport_Aggregation_ApprovalRequestNotPost()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertAggregation(0);
		}

		void AssertAggregation(int expectAddAggregationCount)
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = true;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AccountingConfigurationRegistry.Instance.AllowOnTheSpotApprovalsOfGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var glAccount1 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2010.00.00"));
			var glAccount2 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "2020.00.00"));
			var aggregationForGLAccount1 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, glAccount1.PK));
			var aggregationForGLAccount2 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, glAccount2.PK));
			var originalAggregationCountForGLAccount1 = aggregationForGLAccount1.Length;
			var originalAggregationCountForGLAccount2 = aggregationForGLAccount2.Length;

			using (var form = new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test))
			{
				var multiCompaniesGLJournalFlatFileDataImporter = new MultiCompaniesGLJournalFlatFileDataImporter();
				form.Importer = multiCompaniesGLJournalFlatFileDataImporter;
				form.ImportFromFile_ForTestOnly(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal_Aggregation.csv");
			}

			aggregationForGLAccount1 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, glAccount1.PK));
			aggregationForGLAccount2 = Factory.Load<AccGLAggregate>(new ZQuery(AccGLAggregateSchema.AA_AG, glAccount2.PK));

			AssertEquals(originalAggregationCountForGLAccount1 + expectAddAggregationCount, aggregationForGLAccount1.Length);
			AssertEquals(originalAggregationCountForGLAccount2 + expectAddAggregationCount, aggregationForGLAccount2.Length);
		}

		[DeveloperOnlyTest]
		public void TestClipboardCopyButton_Click()
		{
			using (var testForm = new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test))
			{
				var progressTextBox = testForm.Controls.Find("ProgressTextBox", true).First() as TextBox;
				progressTextBox.Text = "testfile";

				testForm.Show();

				testForm.ClipboardCopyButton_Click_ForTestOnly(testForm, EventArgs.Empty);
				IDataObject clipBoardInfo = SafeClipboard.GetDataObject();
				string result = clipBoardInfo.GetData(DataFormats.Text) as string;

				AssertEquals("testfile", result);
			}
		}

		[TestDate(2005, 08, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreventUploadSameFile()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.csv";

				using (var form = new UploadGLJournalsDataImportFormForTesting(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test, new MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter()))
				{
					form.ImportFromFile_ForTestOnly(fileName);
					form.ImportFromFile_ForTestOnly(fileName);

					var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter;
					AssertEquals(true, multiCompaniesGLJournalFlatFileDataImporter.HasError);
					AssertContains($"A flat file with the same Hash was imported by E in company DAN at 10-Aug-05 00:00:00 +00:00 with file name {fileName}.", form.ProgressTextBox_ForTestOnly.Text);
				}
			});
		}

		[TestDate(2022, 08, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportSuccessfulMessage_GLJH()
		{
			TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(() =>
			{
				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
				var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.csv";

				using (var form = new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test))
				{
					form.ImportFromFile_ForTestOnly(fileName);

					var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporter;
					AssertContains("Journals Processed Successfully: 2 Journal Headers, 4 Journal Lines", form.ProgressTextBox_ForTestOnly.Text);
				}
			});
		}

		void TestWithSwitchCurrentBranchToSY1ForValidGJLJournal(Action action)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DAN";
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			var sY1Branch = testObjectCreator.CreateNewBranch(company, "SY1");

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sY1Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				action.Invoke();
			}
		}

		[TestDate(2022, 08, 10)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportSuccessfulMessage_GLJF()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var company = testObjectCreator.CreateNewCompany("TS3");
			testObjectCreator.CreateNewBranch(company, "TS3");
			company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;
			Factory.Save();

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(2022, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			periodTestHelper.PostPeriodsForEntireYear(2022, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var fileName = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournalWithFileHeader.csv";

			using (var form = new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test))
			{
				form.ImportFromFile_ForTestOnly(fileName);

				var multiCompaniesGLJournalFlatFileDataImporter = form.Importer as MultiCompaniesGLJournalFlatFileDataImporter;
				AssertContains("Journals Processed Successfully: 1 File Header, 8 Journal Lines", form.ProgressTextBox_ForTestOnly.Text);
			}
		}

		protected override DataImporterForm NewDataImporterForm()
		{
			return new UploadGLJournalsDataImportForm(new DataImporterBusinessObject(new BusinessObjectFactory()), null, BillingInterfaceName.Test);
		}

		protected override Form GetFormToBashCore()
		{
			return NewDataImporterForm();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testObjectCreator = new TestObjectCreator(Factory);

			var org1 = testObjectCreator.CreateOrgHeader("2010.00.01", true, true, true, true, true, true);
			org1.OH_Code = "2010.00.01";
			var org2 = testObjectCreator.CreateOrgHeader("2020.00.01", true, true, true, true, true, true);
			org2.OH_Code = "2020.00.01";
			var org3 = testObjectCreator.CreateOrgHeader("2010.00.05", true, true, true, true, true, true);
			org3.OH_Code = "2010.00.05";
			var org4 = testObjectCreator.CreateOrgHeader("2020.00.05", true, true, true, true, true, true);
			org4.OH_Code = "2020.00.05";
			testObjectCreator.CreateSalesGroup("2010.00.02");
			testObjectCreator.CreateSalesGroup("2020.00.02");
			testObjectCreator.CreateSalesGroup("2010.00.06");
			testObjectCreator.CreateSalesGroup("2020.00.06");
			testObjectCreator.CreateStaff("AAA");
			testObjectCreator.CreateStaff("BBB");
			testObjectCreator.CreateStaff("CCC");
			testObjectCreator.CreateStaff("DDD");
			testObjectCreator.CreateStaffGroup("2010.00.04");
			testObjectCreator.CreateStaffGroup("2020.00.04");
			testObjectCreator.CreateStaffGroup("2010.00.08");
			testObjectCreator.CreateStaffGroup("2020.00.08");

			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "TST"));

			if (company == null)
			{
				company = testObjectCreator.CreateNewCompany("TST");
				testObjectCreator.CreateNewBranch(company, "TST");
				company.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;

				var periodTestHelper = new AccountingPeriodTestHelper();
				periodTestHelper.PostPeriodsForEntireYear(2022, company.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

				Factory.Save();
			}
		}

		class UploadGLJournalsDataImportFormForTesting : UploadGLJournalsDataImportForm
		{
			public UploadGLJournalsDataImportFormForTesting(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName, MultiCompaniesGLJournalFlatFileDataImporter testingImporter)
				: base(businessEntity, formCaption, interfaceName)
			{
				this.testingImporter = testingImporter;
			}

			readonly MultiCompaniesGLJournalFlatFileDataImporter testingImporter;

			protected override bool OnBeforeImport()
			{
				var result = base.OnBeforeImport();
				Importer = testingImporter;

				return result;
			}
		}

		class MultiCompaniesGLJournalFlatFileDataImporterWithErrorReporter : MultiCompaniesGLJournalFlatFileDataImporter
		{
			public bool HasError { get; private set; }

			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, CargoWise.ComponentModel.INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				var result = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
				HasError = notifications is NotificationBuffer notificationBuffer && notificationBuffer.HasErrors;

				return result;
			}
		}

		class MultiCompaniesGLJournalFlatFileDataImporterWithException : MultiCompaniesGLJournalFlatFileDataImporter
		{
			protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, CargoWise.ComponentModel.INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				var result =  base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
				LastImportedJournal.Factory.Saving += x =>
				{
					throw new Exception("Test exception in save process.");
				};

				return result;
			}
		}
	}
}
