using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ImportAccountsControllerForm.Testing
{
	public class ImportAccountsControllerFormTest : TestCaseWithFactory
	{
		public void TestImportAccountsButton_Click()
		{
			ImportAccountsControllerFormForTest.RunImportAccountsButton_Click();
			AssertEquals("Should Prompt Import Form", typeof(DataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(false, ((DataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);

			AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ImportAccountsControllerFormForTest.RunImportAccountsButton_Click();
			AssertEquals(true, ((DataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);
		}

		public void TestImportAlternateGLAccountsButton_Click()
		{
			using (var form = new ImportAccountsControllerForm(new AccountsImportBusinessObject(Factory)))
			{
				form.Show();
				form.RunImportAlternateGLAccountsButton_Click();
				AssertEquals("Should Prompt Import Form", typeof(DataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var import = ((DataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer;
				Assert(import is AlternateGLAccountWithAttributeFlatFileDataImporter);
				AssertEquals(false, import.OnlySaveDataWhenNoRecordsHaveErrors);

				var controls = form.Controls.Find("ImportAlternateGLAccountsButton", false);
				AssertEquals(1, controls.Length);
				Assert(!controls[0].Visible);
			}

			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			using (var form = new ImportAccountsControllerForm(new AccountsImportBusinessObject(Factory)))
			{
				form.Show();
				var controls = form.Controls.Find("ImportAlternateGLAccountsButton", false);

				AssertEquals(1, controls.Length);
				Assert(controls[0].Visible);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestDeleteChargeCodesButton_Click()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode)) > 0);
			ImportAccountsControllerFormForTest.RunDeleteChargeCodesButton_Click();
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode)) == 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestDeleteGLHeadersButton_Click()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Assert(Factory.GetDatabaseCount(typeof(AccGLHeader)) > 0);
			//ClearGLHeadersFromRegistryForTests();
			ImportAccountsControllerFormForTest.RunDeleteChargeCodesButton_Click();
			ImportAccountsControllerFormForTest.RunDeleteGLHeadersButton_Click();
			Assert(Factory.GetDatabaseCount(typeof(AccGLHeader)) == 0);

			AssertEquals("There should not be Control Account registry items set", false, AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());
			ImportAccountsControllerFormForTest.fAccountsImportBuzinessObject.RevertAllControlAccounts();
			Assert("There should be Control Account registry items set", AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());

			AccGLHeader glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());

			AccTransactionHeader transHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transHeader.AH_AG = glHeader.PK;
			try
			{
				Factory.Save();
				ImportAccountsControllerFormForTest.RunDeleteGLHeadersButton_Click();
				Assert("There should be Control Account registry items set", AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());
				AssertNotNull("The last showed message should not be null", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			catch
			{
				Fail("Should not get in to catch statement - exception should be handled");
			}
		}

		public void TestExceptionWhenDeleteChargesCodesWithOtherTableRecordsReferencesTo()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			GlbDepartment department = Factory.New<GlbDepartment>();
			department.GE_Code = "YYY";

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "TESTCODE";

			GlbDeptCharges charge = Factory.New<GlbDeptCharges>();
			charge.GD_AC = chargeCode.PK;
			charge.GD_GC = GlbCompany.CurrentCompany.PK;
			charge.GD_GE = department.PK;

			chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "TESTCODE1";

			charge = Factory.New<GlbDeptCharges>();
			charge.GD_AC = chargeCode.PK;
			charge.GD_GC = GlbCompany.CurrentCompany.PK;
			charge.GD_GE = department.PK;

			try
			{
				Factory.Save();
				ImportAccountsControllerFormForTest.RunDeleteChargeCodesButton_Click();
				AssertNotNull("The last shown message should not be null", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("The message should differ from MessageBeforeDeleteChargeCodes", ImportAccountsControllerFormForTest.GetUsedMessageBeforeDeleteChargeCodes() != UnitTestUserNotification.Instance.LastMessage.Text);
			}
			catch
			{
				Fail("Should not get in to catch statement - exception should be handled");
			}
		}

		public void TestExceptionWhenDeleteGLHeadersWithOtherTableRecordsReferencesTo()
		{
			ClearGLHeadersFromRegistryForTests();
			AccGLHeader gLHeader = Factory.New<AccGLHeader>();
			gLHeader.AG_AccountNum = "9999.99.99";
			gLHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			gLHeader.AG_DebitCredit = Core.Constants.DebitCredit.Credit;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "TESTCODE";
			chargeCode.AC_AG_AccrualAccount = gLHeader.PK;

			gLHeader = Factory.New<AccGLHeader>();
			gLHeader.AG_AccountNum = "9999.99.9";
			gLHeader.AG_AccountType = Core.Constants.AccountType.Alternate;
			gLHeader.AG_DebitCredit = Core.Constants.DebitCredit.Credit;

			chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "TESTCODE2";
			chargeCode.AC_AG_AccrualAccount = gLHeader.PK;

			try
			{
				Factory.Save();
				ImportAccountsControllerFormForTest.RunDeleteGLHeadersButton_Click();
				AssertNotNull("The last showed message should not be null", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			catch
			{
				Fail("Should not get in to catch statement - exception should be handled");
			}
		}

		public void TestDeleteChargeCodesMessageBox()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "TESTCODE";

			Factory.Save();

			ImportAccountsControllerFormForTest.RunDeleteChargeCodesButton_Click();
			AccChargeCode testChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, chargeCode.PK));
			AssertNotNull("Charge Code should not be deleted", testChargeCode);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			ImportAccountsControllerFormForTest.RunDeleteChargeCodesButton_Click();
			testChargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, chargeCode.PK));
			AssertNull("Charge Code should be deleted", testChargeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ImportAccountsControllerFormForTest = new ImportAccountsControllerForm(new AccountsImportBusinessObject(Factory));
		}

		protected override void TearDown()
		{
			if (ImportAccountsControllerFormForTest != null && !ImportAccountsControllerFormForTest.IsDisposed)
			{
				ImportAccountsControllerFormForTest.Dispose();
			}

			base.TearDown();
		}

		#region Implementation

		ImportAccountsControllerForm ImportAccountsControllerFormForTest;

		void ClearGLHeadersFromRegistryForTests()
		{
			AccountingConfigurationRegistry.Instance.ClearAllControlAccountRegistryItems();
		}
		#endregion
	}
}
