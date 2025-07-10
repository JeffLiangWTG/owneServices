using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(JobTypePicker))]
	public class JobTypePickerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobTypePicker(Supporter);
		}

		JobTypePicker Picker
		{
			get
			{
				return picker ?? (picker = GetNewBusinessObject() as JobTypePicker);
			}
		}
		JobTypePicker picker;

		public void TestJobTypeSelection()
		{
			AssertEquals("Two object must be equal", Picker.JobTypeList, Supporter.JobTypeList);

			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;

			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("Number of Selected Job Types", 2, Picker.SelectedJobTypeList.Count);
			AssertEquals("Code of first Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, Picker.SelectedJobTypeList[0].Description);
			AssertEquals("Code of Second Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, Picker.SelectedJobTypeList[1].Description);

			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("ValidateJobTypes function must be called. Validation function will set the HasError flag to true", true, Supporter.HasError.Value);

			ObjectCreator.CreateOrgInvoiceType(Debtor.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", "STD", "INV", "INV");
			supporter.ServiceLevel = "STD";

			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("ValidateJobTypes function must be called. Validation function will set the HasError flag to true", true, Supporter.HasError.Value);

			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = false;
			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("Number of Selected Job Types", 1, Picker.SelectedJobTypeList.Count);
			AssertEquals("Code of first Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, Picker.SelectedJobTypeList[0].Description);

			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("ValidateJobTypes function must be called. Validation function will set the HasError flag to false", false, Supporter.HasError.Value);

			Supporter.SelectedJobTypes = System.Array.Empty<ZString>();
			Picker.SelectedJobTypeList[0].Value = false;
			AssertEquals("No Job Type should remain selected", false, Picker.JobTypeList.Any(x => x.Value));
			AssertEquals("ValidateJobTypes function must be called. Validation function will set the HasError flag to false", false, Supporter.HasError.Value);
		}

		public void TestUndoJobTypeSelection()
		{
			AssertEquals("Two object must be equal", Picker.JobTypeList, Supporter.JobTypeList);

			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;
			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.TransportBooking.Code).Value = true;

			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("Number of Selected Job Types", 3, Picker.SelectedJobTypeList.Count);
			AssertEquals("Code of first Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, Picker.SelectedJobTypeList[0].Description);
			AssertEquals("Code of Second Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, Picker.SelectedJobTypeList[1].Description);
			AssertEquals("Code of Third Selected Job Type", JobInvoicingConsumerTypes.TransportBooking.Code, Picker.SelectedJobTypeList[2].Description);

			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.LocalCartage.Code).Value = true;
			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBooking.Code).Value = true;

			AssertEquals("Number of Selected Job Types in JobTypeList", 5, Picker.JobTypeList.Count(x => x.Value));

			Picker.UndoSelectedJobTypes();
			AssertEquals("Will undo the recently selected 2 Job Types", 3, Picker.JobTypeList.Count(x => x.Value));
			AssertEquals("AgencyBillOfLading", true, Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value);
			AssertEquals("Shipment", true, Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value);
			AssertEquals("TransportBooking", true, Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.TransportBooking.Code).Value);
			AssertEquals("LocalCartage", false, Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.LocalCartage.Code).Value);
			AssertEquals("AgencyBooking", false, Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBooking.Code).Value);
		}

		public void TestReset()
		{
			AssertEquals("Two object must be equal", Picker.JobTypeList, Supporter.JobTypeList);

			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.AgencyBillOfLading.Code).Value = true;
			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.Shipment.Code).Value = true;
			Picker.JobTypeList.First(x => x.Description == JobInvoicingConsumerTypes.TransportBooking.Code).Value = true;

			Supporter.SelectedJobTypes = Picker.SelectedJobTypeList.Select(x => x.Description).ToArray();
			Picker.SetSelectedJobTypeList();
			AssertEquals("Number of Selected Job Types", 3, Picker.SelectedJobTypeList.Count);
			AssertEquals("Code of first Selected Job Type", JobInvoicingConsumerTypes.AgencyBillOfLading.Code, Picker.SelectedJobTypeList[0].Description);
			AssertEquals("Code of Second Selected Job Type", JobInvoicingConsumerTypes.Shipment.Code, Picker.SelectedJobTypeList[1].Description);
			AssertEquals("Code of Third Selected Job Type", JobInvoicingConsumerTypes.TransportBooking.Code, Picker.SelectedJobTypeList[2].Description);

			Picker.Reset();
			AssertEquals("Number of Selected Job Types in JobTypeList", false, Picker.JobTypeList.Any(x => x.Value));
			AssertEquals("Number of Selected Job Types", 0, Picker.SelectedJobTypeList.Count);
		}

		DummyJobTypePickerSupporter Supporter
		{
			get
			{
				return supporter ?? (supporter = new DummyJobTypePickerSupporter(Debtor));
			}
		}
		DummyJobTypePickerSupporter supporter;

		OrgHeader Debtor
		{
			get
			{
				if (debtor == null)
				{
					debtor = ObjectCreator.Debtor;
				}
				return debtor;
			}
		}
		OrgHeader debtor;

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
