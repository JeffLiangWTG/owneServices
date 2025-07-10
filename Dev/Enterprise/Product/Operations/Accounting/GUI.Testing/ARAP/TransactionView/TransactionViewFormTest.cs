using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;

namespace Enterprise.Accounting.GUI.ARAP.TransactionView.Testing
{
	[TestedType(typeof(TransactionViewForm))]
	public class TransactionViewFormTest : ZFormBasherTest
	{
		protected APOverpayment Payment;
		protected MatchingBase TestMatchingBase;

		protected override Form GetFormToBashCore()
		{
			TestMatchingBase = new ARMatchingBase(Factory);
			Payment = Factory.New<APOverpayment>();
			((IMiscellaneousTransaction)Payment).MatchingBizO = TestMatchingBase;
			return new TransactionViewForm(Payment);
		}

		#region Plug-ins

		public void TestDataExportBatchPluginIsAdded()
		{
			using (var form = (TransactionViewForm)GetFormToBashCore())
			{
				var source = form.BusinessEntity as IDataExportBatchSource;
				AssertNotNull("Precondition: entity is IDataExportBatchSource", source);
				AssertNotNull("Precondition: IDataExportBatchSource entity has support", source.IsDataExportBatchSupported);
				AssertNotNull("IDataExportBatchSource entity should have plugin", form.PlugIns.GetPlugIn(ControllerIDs.DataExportBatchPlugin));
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (TransactionViewForm)GetFormToBashCore())
			{
				var formType = form.GetType();
				Assert($"{formType} should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#endregion

		#region TestFormProperties

		public void TestFormBorderStyle()
		{
			using (var form = (TransactionViewForm)GetFormToBashCore())
			{
				var formType = form.GetType();
				AssertEquals($"The FormBorderStyle of {formType} should be Sizable", FormBorderStyle.Sizable, form.FormBorderStyle);
			}
		}

		#endregion

		#region TestSavingNewDoesNotSaveToDB

		// Clicking post should not save to DB
		public void TestSavingNewDoesNotSaveToDB()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.Show();
				testForm.DisplayMode = ODisplayMode.New;
				testForm.ValidateAndSave_ForTestOnly();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				Overpayment loadedOVP = newFactory.Load<APOverpayment>(Payment.PK);
				AssertNull("The overpayment should not be saved to the DB", loadedOVP);
				Assert("The overpayment should not be deleted since it was just saved", !Payment.IsDeleted);
			}
		}

		#endregion

		#region TestCancelNewUnsavedFormDeletesBizO

		public void TestCancelNewUnsavedFormDeletesBizO()
		{
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.FCancelButton_Click_ForTestOnly(testForm, EventArgs.Empty);
				Assert("Overpayment should be deleted since it was cancelled", Payment.IsDeleted);
			}
		}

		#endregion

		#region TestSavingEditDoesNotDelete

		public void TestSavingEditDoesNotDelete()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.Show();
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.ValidateAndSave_ForTestOnly();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				Overpayment loadedOVP = newFactory.Load<APOverpayment>(Payment.PK);
				AssertNull("The overpayment should not be saved to the DB", loadedOVP);
				Assert("The overpayment should not be deleted", !Payment.IsDeleted);
			}
		}

		#endregion

		#region TestCancelingEditDoesNotDelete

		public void TestCancelingEditDoesNotDelete()
		{
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.FCancelButton_Click_ForTestOnly(testForm, EventArgs.Empty);
				Assert("Overpayment should not be deleted", !Payment.IsDeleted);
			}
		}

		#endregion

		#region TestClosingEditDoesNotDelete

		public void TestClosingEditDoesNotDelete()
		{
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				Payment.AH_LocalExTaxAmount = 45M;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("Overpayment should not be deleted", !Payment.IsDeleted);
			}
		}

		#endregion

		#region TestClosingNewDeletesBizO

		public void TestClosingNewDeletesBizO()
		{
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("Overpayment should be deleted", Payment.IsDeleted);
			}
		}

		#endregion

		#region TestClosingAmountNonZeroDoesNotDelete

		public void TestClosingAmountNonZeroDoesNotDelete()
		{
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				Payment.AH_LocalExTaxAmount = 90M;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("OVP should not be deleted", !Payment.IsDeleted);
			}
		}

		#endregion

		#region TestClosingAmountZeroDeletesBizO

		public void TestClosingAmountZeroDeletesBizO()
		{
			using (TransactionViewForm testForm = (TransactionViewForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				Payment.AH_LocalExTaxAmount = 50M;
				Payment.AH_LocalExTaxAmount = 0M;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("OVP should be deleted", Payment.IsDeleted);
			}
		}

		#endregion

		#region TestIsNewUnsavedObject

		public void TestIsNewUnsavedObject()
		{
			APMatchingBase testAPMatching = new APMatchingBase(Factory);

			APOverpayment testOVP = Factory.NewWithValidTestData<APOverpayment>();

			((IMiscellaneousTransaction)testOVP).MatchingBizO = null;
			using (TransactionViewForm testForm = new TransactionViewForm(testOVP))
			{
				Assert("Matching BizO is null so IsNewUnsavedObject_ForTestOnly should be false", !testForm.IsNewUnsavedObject_ForTestOnly);

				((IMiscellaneousTransaction)testOVP).MatchingBizO = testAPMatching;

				Assert("IsNewUnsavedObject_ForTestOnly should be true since MatchingBase doesnot refer to TestOVP", testForm.IsNewUnsavedObject_ForTestOnly);

				testAPMatching.AddMiscellaneousTransaction(testOVP);
				Assert("IsNewUnsavedObject_ForTestOnly should be false since MatchingBase refers to TestOVP", !testForm.IsNewUnsavedObject_ForTestOnly);
			}
		}

		#endregion

		#region TestIsCurrentContextMatching

		public void TestIsCurrentContextMatching()
		{
			APDiscount testDisc = Factory.NewWithValidTestData<APDiscount>();
			((IMiscellaneousTransaction)testDisc).MatchingBizO = null;
			using (TransactionViewForm testForm = new TransactionViewForm(testDisc))
			{
				Assert("Current context is not matching since MatchingBizO is null", !testForm.IsCurrentContextMatching_ForTestOnly);
				testForm.Close();
			}

			((IMiscellaneousTransaction)testDisc).MatchingBizO = new ARMatchingBase(Factory);
			using (TransactionViewForm testForm = new TransactionViewForm(testDisc))
			{
				Assert("Current context is matching since MatchingBizO is not null", testForm.IsCurrentContextMatching_ForTestOnly);
				testForm.Close();
			}
		}

		#endregion

		public void TestFormButtonsText()
		{
			APDiscount testBizObj = null;
			TransactionViewForm testForm = null;

			Action initTest = () =>
			{
				testBizObj = Factory.NewWithValidTestData<APDiscount>();
				testForm = new TransactionViewForm(testBizObj);
			};

			AssertButton assertButton = (button, isButtonVisible, isButtonEnabled, buttonText) =>
			{
				AssertEquals(isButtonVisible, button.Visible);
				AssertEquals(isButtonEnabled, button.Enabled);
				AssertEquals(buttonText, button.Text);
			};

			//saved bizObj - edit
			initTest();
			Factory.Save();
			using (testForm)
			{
				testForm.Show();
				testForm.DisplayMode = ODisplayMode.Edit;
				Assert(!testForm.IsCurrentContextMatching_ForTestOnly);
				assertButton(testForm.FApplyButton_ForTestOnly, false, true, "&New");
				assertButton(testForm.FCancelButton_ForTestOnly, true, true, "&Close");
				assertButton(testForm.FPostButton_ForTestOnly, true, false, "&Save");
				AssertStartsWith("Saved transaction can't be edited and should always have View caption.", "View", testForm.Text);
				testForm.Close();
			}

			//saved bizObj - browse
			initTest();
			Factory.Save();
			using (testForm)
			{
				testForm.Show();
				testForm.DisplayMode = ODisplayMode.Browse;
				Assert(!testForm.IsCurrentContextMatching_ForTestOnly);
				assertButton(testForm.FApplyButton_ForTestOnly, false, true, "&New");
				assertButton(testForm.FCancelButton_ForTestOnly, true, true, "&Close");
				assertButton(testForm.FPostButton_ForTestOnly, true, false, "&Save");
				AssertStartsWith("Saved transaction can't be edited and should always have View caption.", "View", testForm.Text);
				testForm.Close();
			}

			//matching
			initTest();
			using (testForm)
			{
				((IMiscellaneousTransaction)testBizObj).MatchingBizO = new ARMatchingBase(Factory);
				testForm.Show();
				testForm.DisplayMode = ODisplayMode.New;
				Assert(testForm.IsCurrentContextMatching_ForTestOnly);
				assertButton(testForm.FApplyButton_ForTestOnly, false, false, "&Save");
				assertButton(testForm.FCancelButton_ForTestOnly, true, true, "&Cancel");
				assertButton(testForm.FPostButton_ForTestOnly, true, true, "Add");
				AssertStartsWith("New transaction.", "New", testForm.Text);
				testForm.Close();
			}

			//Browse and edit from payment batch form
			using (new DisposableAction(() => Factory.SetContext(BusinessContext.EditingPaymentApprovalBatch),
										() => {
											Factory.RemoveContext(BusinessContext.EditingPaymentApprovalBatch);
											testForm?.Dispose();
										}))
			{
				initTest();

				testForm.Show();
				testForm.DisplayMode = ODisplayMode.Browse;

				assertButton(testForm.FApplyButton_ForTestOnly, false, false, "&Save");
				assertButton(testForm.FCancelButton_ForTestOnly, true, true, "Edit");
				assertButton(testForm.FPostButton_ForTestOnly, false, false, "S&ave && Close");

				testBizObj.AH_InvoiceAmount += 10m;

				AssertEquals("pre-condition", ODisplayMode.Edit, testForm.DisplayMode);

				assertButton(testForm.FApplyButton_ForTestOnly, false, false, "&Save");
				assertButton(testForm.FCancelButton_ForTestOnly, true, true, "Edit");
				assertButton(testForm.FPostButton_ForTestOnly, false, false, "S&ave && Close");
			}
		}

		#region Implementation

		delegate void AssertButton(IButton button, bool isButtonVisible, bool isButtonEnabled, string buttonText);

		#endregion
	}
}
