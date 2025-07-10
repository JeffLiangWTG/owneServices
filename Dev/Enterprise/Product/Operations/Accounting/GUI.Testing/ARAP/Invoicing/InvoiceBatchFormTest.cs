using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(InvoiceBatchForm))]
	public class InvoiceBatchFormTest : AccountingZFormBasherTest
	{
		protected virtual InvoiceBatchHeader GetFormBizO()
		{
			return Factory.New<InvoiceBatchHeader>();
		}

		protected virtual InvoiceBatchForm GetForm(InvoiceBatchHeader batchHeader)
		{
			return new InvoiceBatchForm(batchHeader);
		}

		protected override Form GetFormToBashCore()
		{
			return GetForm(GetFormBizO());
		}

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				AssertEquals("Verb should be 'Cancel'", "Cancel", testForm.FormVerb);
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
			}
		}

		public void TestFormReadOnly()
		{
			using (InvoiceBatchForm testForm = GetFormToBash() as InvoiceBatchForm)
			{
				testForm.Show();

				testForm.DisplayMode = ODisplayMode.New;
				Assert("Close Button is enabled", testForm.CloseButton_ForTestOnly.Enabled);
				Assert("Post Button is enabled", testForm.PostButton_ForTestOnly.Enabled);
				Assert("Job Type Checked List Box is enabled", testForm.JobTypeCheckedListBox_ForTestOnly.Enabled);

				testForm.DisplayMode = ODisplayMode.ReadOnly;
				Assert("Close Button is enabled", testForm.CloseButton_ForTestOnly.Enabled);
				Assert("Post Button is disabled", !testForm.PostButton_ForTestOnly.Enabled);
				Assert("Job Type Checked List Box is disabled", !testForm.JobTypeCheckedListBox_ForTestOnly.Enabled);
			}
		}

		public override void TestPrevAndNextButton()
		{
			using (InvoiceBatchForm testForm = GetFormToBash() as InvoiceBatchForm)
			{
				testForm.Show();
				Assert(testForm.AutoAddPreviousNextButtons);
			}
		}

		public void TestCancelledLabelShownIfCancelled()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_IsCancelled = ZBool.True;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				testForm.Show();

				Assert(testForm.CancelledBatchLabel_ForTestOnly.Visible);
			}
		}

		public void TestCancelledLabelNotShownIfNotCancelled()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_IsCancelled = ZBool.False;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				testForm.Show();

				Assert(!testForm.CancelledBatchLabel_ForTestOnly.Visible);
			}
		}

		public void TestFilterDoesNotTriggerOnLoaded()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			ARInvoice testLine1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			testLine1.AH_OH = TestOrg.PK;

			testLine1.AH_AH_InvoiceStatement = testHeader.PK;
			Factory.Save();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.Line.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.Line.Count);
			}
		}

		public void TestFormShowCorrectComponentOnNew()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			ARInvoice testLine1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			testLine1.AH_AH_InvoiceStatement = testHeader.PK;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				Assert("Notification Label", testForm.NotificationLabel_ForTestOnly.Visible);
				Assert("Filter Control", testForm.FilterControl_ForTestOnly.Visible);
			}
		}

		public void TestFormShowCorrectComponentOnView()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			ARInvoice testLine1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			testLine1.AH_AH_InvoiceStatement = testHeader.PK;
			Factory.Save();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				Assert("Notification Label", !testForm.NotificationPanel_ForTestOnly.Visible);
				AssertNull("Filter Control", testForm.FilterControl_ForTestOnly);
			}
		}

		public void TestLineFactory()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				AssertNotNull("Line Factory should not be null", testForm.LineFactory_ForTestOnly);
			}
		}

		public void TestPerformSearch()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.Line.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(4, testHeader.Line.Count);

				((ModuleTextFilter)testHeader.Filter["Transaction Type"]).IsActive = true;
				((ModuleTextFilter)testHeader.Filter["Transaction Type"]).Property = TransactionTypes.Invoice;
				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, testHeader.Line.Count);
			}
		}

		OrgHeader TestOrg
		{
			get { return fTestOrg ?? (fTestOrg = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fTestOrg;

		public void TestClearButtonClicked()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.Line.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(4, testHeader.Line.Count);

				testForm.FilterControl_ClearButtonClicked_ForTestOnly(this, new EventArgs());
				AssertEquals(0, testHeader.Line.Count);
			}
		}

		[MemoryTestRetryCount(2)]
		public void TestShowTransaction()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.Line.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(4, testHeader.Line.Count);

				using (AccountingZForm testInvoiceForm = testForm.ShowSelectedTransaction_ForTestOnly() as AccountingZForm)
				{
					AssertNotNull(testInvoiceForm);
				}
			}
		}

		public void TestSaveAsksPrintingConfirmtation()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCredtiNote2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			aRCredtiNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.Line.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(4, testHeader.Line.Count);

				testForm.PostSaveProcessing_ForTestOnly(ContinueWithSave.Yes);
				AssertEquals("Do you want to print Invoice Batch?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreventFindActionWithoutSelectedJobtypes()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals("At least one Job Type should be selected.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;
				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoSearchWithStatusText()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			ARCreditNote aRCredtiNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			ARCreditNote aRCredtiNote2 = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;

			Factory.Save();

			InvoiceBatchHeader testHeader = GetFormBizO();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.Line.Count);
				int count = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
				testForm.DoSearch_ForTestOnly(new ZQuery(), count + 1);

				AssertEquals("Too many records to display (" + (count + 1) + "). Please fill in more of the search screen and then click 'Find'.", testForm.NotificationLabel_ForTestOnly.Text);
			}
		}

		public void TestHideNonApplicableControls()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals("AH_OSExtraTaxAmount should not be avaliable", true, testForm.BatchInvoiceLinesGrid_ForTestOnly.GetColumnStyle(TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount).IsUnavailable);
				AssertEquals("AH_LocalExtraTaxAmount should not be avaliable", true, testForm.BatchInvoiceLinesGrid_ForTestOnly.GetColumnStyle(TransactionHeaderWithLines.Schema.AH_LocalExtraTaxAmount).IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
			try
			{
				using (InvoiceBatchForm testForm = GetForm(testHeader))
				{
					testForm.Show();
					AssertEquals("AH_OSExtraTaxAmount should not be avaliable", false, testForm.BatchInvoiceLinesGrid_ForTestOnly.GetColumnStyle(TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount).IsUnavailable);
					AssertEquals("AH_LocalExtraTaxAmount should not be avaliable", false, testForm.BatchInvoiceLinesGrid_ForTestOnly.GetColumnStyle(TransactionHeaderWithLines.Schema.AH_LocalExtraTaxAmount).IsUnavailable);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestLinesAreVisibleWhenOnLoaded()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			ARInvoice testLine1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			testLine1.AH_OH = TestOrg.PK;

			testLine1.AH_AH_InvoiceStatement = testHeader.PK;
			Factory.Save();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.Show();
				Assert(testForm.BatchInvoiceLinesGrid_ForTestOnly.Visible);
			}
		}

		public void TestCancelBatch()
		{
			InvoiceBatchHeader testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			ARInvoice testLine1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			testLine1.AH_OH = TestOrg.PK;

			testLine1.AH_AH_InvoiceStatement = testHeader.PK;
			Factory.Save();

			using (InvoiceBatchForm testForm = GetForm(testHeader))
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				AssertEquals("Cancel Batch operation should cancel all invoice in the batch. It should not show IncludeInTheBatch checkBox", false, testForm.BatchInvoiceLinesGrid_ForTestOnly.Columns.Contains("IncludeInTheBatch"));
			}
		}

		public void TestInterfaceAfterPost()
		{
			var testHeader = GetFormBizO();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			var testLine1 = Factory.New(typeof(ARInvoice)) as ARInvoice;
			testLine1.AH_OH = TestOrg.PK;

			testLine1.AH_AH_InvoiceStatement = testHeader.PK;
			Factory.Save();

			using (var testForm = new InvoiceBatchFormForTest(testHeader))
			{
				testForm.Show();
				AssertEquals("New", testForm.PostButtonForTest.Text);
				AssertEquals(false, testForm.JobTypeCheckedListBox_ForTestOnly.Enabled);
				AssertEquals(false, testForm.BatchInvoiceLinesGrid_ForTestOnly.Columns.Contains("IncludeInTheBatch"));
			}
		}

		public void TestOutstandingAmountCaption()
		{
			var testHeader = GetFormBizO();
			using (var testForm = new InvoiceBatchFormForTest(testHeader))
			{
				testForm.Show();
				Application.DoEvents();

				var grid = testForm.BatchInvoiceLinesGrid_ForTestOnly;
				var columnName = "AH_OutstandingAmount";
				var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
				AssertNotNull(columnInfo);
				AssertNull(columnInfo.CaptionResourceString.Caption);
				AssertEquals("Outstanding Amount", grid.Columns[columnName].ColumnStyle.HeaderText);
			}
		}

		class InvoiceBatchFormForTest : InvoiceBatchForm
		{
			public InvoiceBatchFormForTest(InvoiceBatchHeader batchHeader)
				: base(batchHeader)
			{
			}

			public ZPostOrCancelButton PostButtonForTest
			{
				get { return base.PostButton; }
			}
		}
	}
}
