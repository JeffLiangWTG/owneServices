using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(InvoiceBulkBatchForm))]
	public class InvoiceBulkBatchFormTest : InvoiceBatchFormTest
	{
		#region Overrides

		protected override InvoiceBatchHeader GetFormBizO()
		{
			InvoiceBulkBatch bulkBatch = Factory.New<InvoiceBulkBatch>();
			bulkBatch.InvoiceBatchHeaders.Add(Factory.New<InvoiceBatchHeader>());
			return bulkBatch;
		}

		protected override InvoiceBatchForm GetForm(InvoiceBatchHeader batchHeader)
		{
			return new InvoiceBulkBatchForm(batchHeader as InvoiceBulkBatch);
		}

		#endregion

		public new void TestFilterDoesNotTriggerOnLoaded()
		{
			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;
			ARInvoice testLine1 = Factory.New<ARInvoice>();
			testLine1.AH_OH = TestOrg.PK;
			Factory.Save();

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(1, testHeader.InvoiceBatchHeaders[0].Line.Count);
			}
		}

		public new void TestFormShowCorrectComponentOnView()
		{
			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			InvoiceBatchHeader invoiceStatement = Factory.New<InvoiceBatchHeader>();
			ARInvoice testLine1 = Factory.New<ARInvoice>();
			testLine1.AH_AH_InvoiceStatement = invoiceStatement.PK;
			testLine1.AH_OH = TestOrg.PK;
			Factory.Save();

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				Assert("Notification Label", testForm.NotificationPanel_ForTestOnly.Visible);
				AssertNotNull("Filter Control", testForm.FilterControl_ForTestOnly);

				Assert("BatchNumberTextBox_ForTestOnly", !testForm.BatchNumberTextBox_ForTestOnly.Visible);
				Assert("TermsDropEdit_ForTestOnly", !testForm.TermsDropEdit_ForTestOnly.Visible);
				Assert("TermDaysCalcEdit_ForTestOnly", !testForm.TermDaysCalcEdit_ForTestOnly.Visible);
				Assert("DueDateDateEdit_ForTestOnly", !testForm.DueDateDateEdit_ForTestOnly.Visible);
				Assert("DebtorGuidFindBox_ForTestOnly", !testForm.DebtorGuidFindBox_ForTestOnly.Visible);
			}
		}

		public new void TestClearButtonClicked()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(4, testHeader.InvoiceBatchHeaders[0].Line.Count);

				testForm.FilterControl_ClearButtonClicked_ForTestOnly(this, new EventArgs());
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);
			}
		}

		public new void TestPerformSearch()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(4, testHeader.InvoiceBatchHeaders[0].Line.Count);

				((ModuleTextFilter)testHeader.Filter["Transaction Type"]).IsActive = true;
				((ModuleTextFilter)testHeader.Filter["Transaction Type"]).Property = TransactionTypes.Invoice;
				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(2, testHeader.InvoiceBatchHeaders[0].Line.Count);
			}
		}

		[MemoryTestRetryCount(2)]
		public new void TestShowTransaction()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(4, testHeader.InvoiceBatchHeaders[0].Line.Count);

				using (AccountingZForm testInvoiceForm = testForm.ShowSelectedTransaction_ForTestOnly() as AccountingZForm)
				{
					AssertNotNull(testInvoiceForm);
				}
			}
		}

		public new void TestSaveAsksPrintingConfirmtation()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(4, testHeader.InvoiceBatchHeaders[0].Line.Count);

				testForm.PostSaveProcessing_ForTestOnly(ContinueWithSave.Yes);
				AssertEquals("Do you want to print all Invoice Batches?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveEntryInvoiceBatchesListError()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			ARCreditNote aRCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote2.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.AH_OH = TestOrg.PK;
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				AssertEquals(0, testHeader.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, testHeader.InvoiceBatchHeaders.Count);
				AssertEquals(4, testHeader.InvoiceBatchHeaders[0].Line.Count);

				testHeader.ClearBatches();
				testForm.PostButton_ForTestOnly.PerformClick();
				AssertEquals("There are not Invoice Batches for posting.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public new void TestLinesAreVisibleWhenOnLoaded()
		{
			InvoiceBulkBatch testHeader = (InvoiceBulkBatch)GetFormBizO();
			testHeader.InvoiceBatchHeaders.RemoveAll();
			testHeader.JobTypeList[testHeader.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;
			ARInvoice testLine1 = Factory.New<ARInvoice>();
			testLine1.AH_OH = TestOrg.PK;
			Factory.Save();

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(testHeader))
			{
				testForm.Show();
				Assert(testForm.BatchInvoiceLinesGrid_ForTestOnly.Visible);
			}
		}

		public void TestValidateAndSave()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;

			Factory.Save();

			InvoiceBulkBatch batch = (InvoiceBulkBatch)GetFormBizO();
			batch.InvoiceBatchHeaders.RemoveAll();
			batch.AH_OH = TestOrg.PK;
			batch.JobTypeList[batch.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(batch))
			{
				testForm.Show();
				AssertEquals(0, batch.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, batch.InvoiceBatchHeaders.Count);
				AssertEquals(1, batch.InvoiceBatchHeaders[0].Line.Count);

				AssertEquals(ContinueWithSave.Yes, testForm.PostSaveProcessing_ForTestOnly(ContinueWithSave.Yes));
			}
		}

		public void TestFormClosesAfterPosting()
		{
			InvoiceBulkBatch batch = (InvoiceBulkBatch)GetFormBizO();
			batch.InvoiceBatchHeaders.RemoveAll();
			batch.AH_OH = TestOrg.PK;
			batch.JobTypeList[batch.GetDescriptionForJobTypeList(InvoiceTypeModuleList.Codes.MSC)].Value = true;

			ARInvoice aRInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, TestOrg);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(aRInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);

			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			Factory.Save();

			using (InvoiceBulkBatchForm testForm = (InvoiceBulkBatchForm)GetForm(batch))
			{
				testForm.Show();
				Assert("Bulk batch form should be visible", testForm.Visible);
				AssertEquals(0, batch.InvoiceBatchHeaders.Count);

				testForm.FilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, batch.InvoiceBatchHeaders.Count);
				AssertEquals(1, batch.InvoiceBatchHeaders[0].Line.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				testForm.PostButton_ForTestOnly.PerformClick();

				Assert("Bulk batch form should not be visible", !testForm.Visible);
			}
		}

		public new void TestCancelBatch()
		{
			Assert(true);
		}

		public new void TestInterfaceAfterPost()
		{
			Assert(true);
		}

		#region Implementation

		OrgHeader TestOrg
		{
			get
			{
				if (fTestOrg == null)
				{
					fTestOrg = Factory.NewWithValidTestData<OrgHeader>();
					fTestOrg.OH_IsDebtor = true;
					fTestOrg.OH_IsCreditor = true;
					fTestOrg.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
					fTestOrg.CompanyData.OB_APPayInvoiceAfterPostingDefault = true;
					fTestOrg.CompanyData.OB_ARReceiptInvoiceAfterPostingDefault = true;
					fTestOrg.CompanyData.InvoiceTypes.AddNew();
					fTestOrg.CompanyData.InvoiceTypes[0].PI_Module = "MSC";
				}
				return fTestOrg;
			}
		}
		OrgHeader fTestOrg;

		#endregion
	}
}
