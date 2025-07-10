using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ShowResultsGridForm))]
	class ShowResultsGridFormTest : ZFormBasherTest
	{
		#region Overrides

		protected override Form GetFormToBashCore() => new ShowResultsGridForm(new WriteOffResultCollection(Factory));

		#endregion

		public void TestFormControls()
		{
			var writeOffResultCollection = new WriteOffResultCollection(Factory);

			using (var showResultsGridForm = new ShowResultsGridForm(writeOffResultCollection))
			{
				var button = (ZButton)showResultsGridForm.Controls.Find("ButtonOK", true).Single();
				var resultsGrid = (ZGrid)showResultsGridForm.Controls.Find("ResultsGrid", true).Single();

				showResultsGridForm.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Button has the correct Text", "OK", button.Text);
					AssertEquals("Results Grid is visible", true, resultsGrid.Visible);
					AssertEquals("Results Grid has 4 columns", 4, resultsGrid.ColumnStyles.Count);
					TestHelper.AssertColumnStyleWithCaption<ZTextBoxColumnStyleInfo>("Job Number", resultsGrid, "JobNumber");
					TestHelper.AssertColumnStyleWithCaption<ZTextBoxColumnStyleInfo>("MRN", resultsGrid, "Mrn");
					TestHelper.AssertColumnStyleWithCaption<ZTextBoxColumnStyleInfo>("Guarantee Status", resultsGrid, "GuaranteeStatus");
					TestHelper.AssertColumnStyleWithCaption<ZTextBoxColumnStyleInfo>("Declaration Status", resultsGrid, "DeclarationStatus");
				});
			}
		}
	}
}
