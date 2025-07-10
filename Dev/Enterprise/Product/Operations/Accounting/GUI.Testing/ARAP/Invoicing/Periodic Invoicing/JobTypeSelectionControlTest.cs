using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.Testing
{
	public class JobTypeSelectionControlTest : TestCaseWithFactory
	{
		public void TestJobSelection()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var supporter = new DummyJobTypePickerSupporter(false, objectCreator.Debtor);
			var picker = new JobTypePicker(supporter);

			using (AccountingZForm testForm = new AccountingZForm(Factory.New<DummyBusinessObject>()))
			{
				JobTypeSelectionControl control = new JobTypeSelectionControl();
				control.BindingSource.SetDataBinding(picker, null);
				testForm.Controls.Add(control);
				testForm.Show();

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;
				control.JobTypesForm_ForTestOnly.SelectJobTypes_ForTestOnly();

				AssertEquals("Number of Selected Jobs", 2, control.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("Number of Selected Jobs in the Picker", 2, picker.SelectedJobTypeList.Count);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, picker.SelectedJobTypeList[0].Description);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, picker.SelectedJobTypeList[1].Description);
			}
		}

		public void TestSingleClickItemCheck()
		{
			using (var control = new JobTypeSelectionControl())
			{
				AssertEquals(true, control.JobTypeCheckedListBox_ForTestOnly.CheckOnClick);
			}
		}

		public void TestJobSelectionUndo()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var supporter = new DummyJobTypePickerSupporter(false, objectCreator.Debtor);
			var picker = new JobTypePicker(supporter);

			using (AccountingZForm testForm = new AccountingZForm(Factory.New<DummyBusinessObject>()))
			{
				JobTypeSelectionControl control = new JobTypeSelectionControl();
				control.BindingSource.SetDataBinding(picker, null);
				testForm.Controls.Add(control);
				testForm.Show();

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;
				control.JobTypesForm_ForTestOnly.SelectJobTypes_ForTestOnly();

				AssertEquals("Number of Selected Jobs", 2, control.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("Number of Selected Jobs in the Picker", 2, picker.SelectedJobTypeList.Count);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, picker.SelectedJobTypeList[0].Description);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, picker.SelectedJobTypeList[1].Description);

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.TransportBooking.Code).Value = true;
				AssertEquals("Number of Tiked on Job Types in the Picker Form", 3, picker.JobTypeList.Where(x => x.Value).Count());

				control.JobTypesForm_ForTestOnly.UndoJobTypeSelection_ForTestOnly();
				AssertEquals("Number of Tiked on Job Types in the Picker Form", 2, picker.JobTypeList.Where(x => x.Value).Count());
				AssertEquals("Number of Selected Jobs", 2, control.JobTypesPicker_ForTestOnly.SelectedJobTypeList.Count);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, picker.SelectedJobTypeList[0].Description);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, picker.SelectedJobTypeList[1].Description);
			}
		}

		public void TestUnTickedJobTypesAreNotInSelectedJobTypeList()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var supporter = new DummyJobTypePickerSupporter(false, objectCreator.Debtor);
			var picker = new JobTypePicker(supporter);

			using (AccountingZForm testForm = new AccountingZForm(Factory.New<DummyBusinessObject>()))
			{
				JobTypeSelectionControl control = new JobTypeSelectionControl();
				control.BindingSource.SetDataBinding(picker, null);
				testForm.Controls.Add(control);
				testForm.Show();

				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;
				picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.TransportBooking.Code).Value = true;
				picker.SetSelectedJobTypeList();
				AssertEquals("Number of Tiked-On Job Types in the List Box", 3, control.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("Number of Selected Jobs Types", 3, picker.SelectedJobTypeList.Count);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, picker.SelectedJobTypeList[0].Description);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, picker.SelectedJobTypeList[1].Description);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.TransportBooking.Code, picker.SelectedJobTypeList[2].Description);

				control.JobTypeCheckedListBox_ForTestOnly.BindingItems[0].Value = false;
				AssertEquals("Number of Tiked-On Job Types in the List Box", 2, control.JobTypeCheckedListBox_ForTestOnly.CheckedItems.Count);
				AssertEquals("Number of Selected Jobs Types", 2, picker.SelectedJobTypeList.Count);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, picker.SelectedJobTypeList[0].Description);
				AssertEquals("Selected Job Type", JobInvoicingConsumerTypes.TransportBooking.Code, picker.SelectedJobTypeList[1].Description);
			}
		}
	}
}
