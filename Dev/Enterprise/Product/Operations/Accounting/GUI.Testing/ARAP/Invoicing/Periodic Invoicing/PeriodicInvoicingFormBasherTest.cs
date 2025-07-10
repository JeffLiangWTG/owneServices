using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AllJobTypesForm))]
	public class PeriodicInvoicingFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return AllJobTypesForm.CreateAllJobTypesForm_ForTestOnly();
		}

		public void TestSelectAllButtonTextChangesCorrectly()
		{
			var supporter = new DummyJobTypePickerSupporter(ObjectCreator.Debtor);
			var picker = new JobTypePicker(supporter);

			using (var testForm = new AllJobTypesForm(picker))
			{
				testForm.Show();
				AssertNotNull(testForm.JobTypePicker_ForTestOnly);
				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", 0, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("BtnSelectAll_ForTestOnly Text", "Select All", testForm.BtnSelectAll_ForTestOnly.Text);

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;

				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", 2, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("BtnSelectAll_ForTestOnly Text", "Deselect All", testForm.BtnSelectAll_ForTestOnly.Text);

				picker.JobTypeList.ToList().ForEach(x => x.Value = true);
				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("BtnSelectAll_ForTestOnly Text", "Deselect All", testForm.BtnSelectAll_ForTestOnly.Text);

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = false;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = false;
				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", picker.JobTypeList.Count - 2, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("BtnSelectAll_ForTestOnly Text", "Deselect All", testForm.BtnSelectAll_ForTestOnly.Text);

				picker.JobTypeList.ToList().ForEach(x => x.Value = false);
				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", 0, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("BtnSelectAll_ForTestOnly Text", "Select All", testForm.BtnSelectAll_ForTestOnly.Text);
			}
		}

		public void TestEventFiresCorrectly()
		{
			var supporter = new DummyJobTypePickerSupporter(ObjectCreator.Debtor);
			var picker = new JobTypePicker(supporter);

			//btnSelect
			using (var testForm = new AllJobTypesForm(picker))
			{
				testForm.JobTypeSelectionChanged += TestForm_JobTypeSelectionChanged;
				testForm.JobTypeSelectionCanceled += TestForm_JobTypeSelectionCanceled;
				testForm.Show();
				AssertNotNull(testForm.JobTypePicker_ForTestOnly);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				firedEventName = ZString.Empty;
				testForm.BtnSelect_ForTestOnly.PerformClick();
				AssertEquals("Number of Job Types in the Listbox", "No Job Type is selected. Please tick-on the left side box in the list to select a job type", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Number of Job Types in the Listbox", ZString.Empty, firedEventName);

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;

				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", 2, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);

				firedEventName = ZString.Empty;
				testForm.BtnSelect_ForTestOnly.PerformClick();
				AssertEquals("Number of Job Types in the Listbox", "JobTypeSelectionChanged", firedEventName);
				AssertEquals("Form Should be Closed", true, testForm.IsDisposed);
			}

			//btnCancel
			using (var testForm = new AllJobTypesForm(picker))
			{
				testForm.JobTypeSelectionChanged += TestForm_JobTypeSelectionChanged;
				testForm.JobTypeSelectionCanceled += TestForm_JobTypeSelectionCanceled;
				testForm.Show();
				AssertNotNull(testForm.JobTypePicker_ForTestOnly);

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;
				AssertEquals("Number of Job Types in the Listbox", picker.JobTypeList.Count, testForm.JobTypeCheckedListBox_ForTestOnly.Items.Count);
				AssertEquals("Number of Job types Checked in the Listbox", 2, testForm.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);

				firedEventName = ZString.Empty;
				testForm.BtnCancel_ForTestOnly.PerformClick();
				AssertEquals("Number of Job Types in the Listbox", "JobTypeSelectionCanceled", firedEventName);
				AssertEquals("Form Should be Closed", true, testForm.IsDisposed);
			}
		}

		void TestForm_JobTypeSelectionCanceled(object sender, EventArgs e)
		{
			firedEventName = "JobTypeSelectionCanceled";
		}

		void TestForm_JobTypeSelectionChanged(object sender, EventArgs e)
		{
			firedEventName = "JobTypeSelectionChanged";
		}

		ZString firedEventName;

		TestObjectCreator ObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
