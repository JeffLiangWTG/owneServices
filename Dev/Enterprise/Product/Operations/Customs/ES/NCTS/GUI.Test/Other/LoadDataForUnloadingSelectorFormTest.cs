using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(LoadDataForUnloadingSelectorForm))]
	class LoadDataForUnloadingSelectorFormTest : ZFormBasherTest
	{
		public void TestFields()
		{
			using (var loadDataForUnloadingSelectorForm = new LoadDataForUnloadingSelectorForm())
			{
				var messageLabel = (ZLabel)loadDataForUnloadingSelectorForm.Controls.Find("MessageLabel", true).Single();
				var departureRadioButton = (ZRadioButton)(loadDataForUnloadingSelectorForm.Controls.Find("DepartureRadioButton", true).Single());
				var customsQueryRadioButton = (ZRadioButton)(loadDataForUnloadingSelectorForm.Controls.Find("CustomsQueryRadioButton", true).Single());
				var extraMessageLabel = (ZLabel)loadDataForUnloadingSelectorForm.Controls.Find("ExtraMessageLabel", true).Single();

				loadDataForUnloadingSelectorForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Message Label has the text we expect", "A Departure for that MRN already exists in the system.\nPlease, select the source of the data to be loaded:", messageLabel.Text);
					AssertEquals("DepartureRadioButton Caption", "Departure", departureRadioButton.CaptionResourceString.Caption);
					AssertEquals("CCustomsQueryRadioButton Caption", "Customs Query", customsQueryRadioButton.CaptionResourceString.Caption);
					AssertEquals("Extra Message Label has the text we expect", "Please note existing data in Unloading Remarks could be overwritten.", extraMessageLabel.Text);

					AssertEquals("DepartureRadioButton is selected by default", true, departureRadioButton.Checked);
					AssertEquals("CCustomsQueryRadioButton is not selected by default", false, customsQueryRadioButton.Checked);
				});
			}
		}

		public void TestGetResultFromSelector_DefaultDepartureRadioButton()
		{
			using (var loadDataForUnloadingSelectorForm = new LoadDataForUnloadingSelectorForm())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var departureRadioButton = (ZRadioButton)(loadDataForUnloadingSelectorForm.Controls.Find("DepartureRadioButton", true).Single());
				var customsQueryRadioButton = (ZRadioButton)(loadDataForUnloadingSelectorForm.Controls.Find("CustomsQueryRadioButton", true).Single());

				loadDataForUnloadingSelectorForm.Show();

				CombineAssertions(() =>
				{
					var (answer, departureSelected) = loadDataForUnloadingSelectorForm.GetResultFromSelector();
					AssertEquals("answer false when Cancel", false, answer);
					AssertEquals("departureSelected true by default even if answer is Cancel", true, departureSelected);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					(answer, departureSelected) = loadDataForUnloadingSelectorForm.GetResultFromSelector();
					AssertEquals("answer true when OK", true, answer);
					AssertEquals("departureSelected true by default", true, departureSelected);
				});
			}
		}

		public void TestGetResultFromSelector_DefaultCustomsQueryRadioButton()
		{
			using (var loadDataForUnloadingSelectorForm = new LoadDataForUnloadingSelectorForm())
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				var departureRadioButton = (ZRadioButton)(loadDataForUnloadingSelectorForm.Controls.Find("DepartureRadioButton", true).Single());
				var customsQueryRadioButton = (ZRadioButton)(loadDataForUnloadingSelectorForm.Controls.Find("CustomsQueryRadioButton", true).Single());

				loadDataForUnloadingSelectorForm.Show();
				departureRadioButton.Checked = false;

				CombineAssertions(() =>
				{
					var (answer, departureSelected) = loadDataForUnloadingSelectorForm.GetResultFromSelector();
					AssertEquals("answer false when Cancel", false, answer);
					AssertEquals("departureSelected false even if answer is Cancel", false, departureSelected);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					(answer, departureSelected) = loadDataForUnloadingSelectorForm.GetResultFromSelector();
					AssertEquals("answer true when OK", true, answer);
					AssertEquals("departureSelected false", false, departureSelected);
				});
			}
		}

		protected override Form GetFormToBashCore() => new LoadDataForUnloadingSelectorForm();
	}
}
