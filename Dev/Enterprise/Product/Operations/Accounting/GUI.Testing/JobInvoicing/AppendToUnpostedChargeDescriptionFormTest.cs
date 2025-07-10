using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AppendToUnpostedChargeDescriptionForm))]
	public class AppendToUnpostedChargeDescriptionFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			var adaptor = new ChargeDescriptionOverrideAdaptor(Factory);
			adaptor.AddToWrappedObjects(new Charge[] { charge });
			return new AppendToUnpostedChargeDescriptionForm(adaptor);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}
		#endregion

		public void TestAllowNew()
		{
			using (var form = (AppendToUnpostedChargeDescriptionForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(false, form.AllowNew_ForTestOnly);
			}
		}

		public void TestSaving_PreventInsertingNullIntoJR_AC()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Charges.Add(charge);

			Factory.Save();

			charge.JR_AC = ZGuid.Empty;
			var adaptor = new ChargeDescriptionOverrideAdaptor(Factory);
			adaptor.AddToWrappedObjects(new Charge[] { charge });

			using (var form = new AppendToUnpostedChargeDescriptionForm(adaptor))
			{
				AssertNotNull(form);
				form.Show();
				AssertNoExceptionThrown(() =>
				{
					form.FireSaveButton();
				});
			}
		}

		public void TestCancelOperation()
		{
			var charge = Factory.NewWithValidTestData<Charge>();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Charges.Add(charge);
			Factory.Save();

			var adaptor = new ChargeDescriptionOverrideAdaptor(Factory);
			adaptor.AddToWrappedObjects(new Charge[] { charge });

			using (var form = new AppendToUnpostedChargeDescriptionForm(adaptor))
			{
				AssertNotNull(form);
				form.Show();

				adaptor.SelectedWrappedCharges[0].TextToAppend = "Appended Text";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.PostingButtonsUserControl_ForTestOnly.CloseButton.PerformClick();
				AssertEquals("Appended Text", adaptor.SelectedWrappedCharges[0].TextToAppend);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.PostingButtonsUserControl_ForTestOnly.CloseButton.PerformClick();
				AssertEquals(ZString.Empty, adaptor.SelectedWrappedCharges[0].TextToAppend);

				adaptor.SelectedWrappedCharges[0].TextToAppend = "Appended Text attemp 2";
				adaptor.ApplyOrCancelChanges(true);
				AssertEquals("Appended Text attemp 2", adaptor.SelectedWrappedCharges[0].TextToAppend);
			}

			using (var form = new AppendToUnpostedChargeDescriptionForm(adaptor))
			{
				AssertNotNull(form);
				form.Show();
				adaptor.SelectedWrappedCharges[0].TextToAppend = "Appended Text attemp 3";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.PostingButtonsUserControl_ForTestOnly.CloseButton.PerformClick();
				AssertEquals("Appended Text attemp 2", adaptor.SelectedWrappedCharges[0].TextToAppend);
			}
		}
	}
}
