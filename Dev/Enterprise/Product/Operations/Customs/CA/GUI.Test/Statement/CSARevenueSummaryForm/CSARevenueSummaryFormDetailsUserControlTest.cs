using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CSARevenueSummaryFormDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestLabels()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;

			using (var form = new ZForm(header))
			using (var control = new CSARevenueSummaryFormDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				var importerFindBox = (ZGuidFindBox)control.Controls.Find("ImporterGuidFindBox", true).First();
				var bnTextBox = (ZTextBox)control.Controls.Find("BusinessNumberTextBox", true).First();
				var periodMonth = (MonthEdit)control.Controls.Find("PeriodMonthEdit", true).First();
				var periodYear = (ZYearEdit)control.Controls.Find("PeriodYearEdit", true).First();
				var startDateEdit = (ZDateEdit)control.Controls.Find("PeriodStartDateEdit", true).First();
				var endDateEdit = (ZDateEdit)control.Controls.Find("PeriodEndDateEdit", true).First();
				var statementDateEdit = (ZDateEdit)control.Controls.Find("StatementDateEdit", true).First();
				var vfdText = (ZTextBox)control.Controls.Find("VFDTextBox", true).First();
				var snText = (ZTextBox)control.Controls.Find("StatementNumberTextBox", true).First();
				var statusText = (ZTextBox)control.Controls.Find("RSFStatusTextBox", true).First();
				var submissionText = (ZTextBox)control.Controls.Find("RSFSubmissionDateTextBox", true).First();
				var acceptedText = (ZTextBox)control.Controls.Find("RSFAcceptedDateTextBox", true).First();

				AssertEquals("Importer", importerFindBox.CaptionResourceString.Caption);
				AssertEquals("Business Number", bnTextBox.CaptionResourceString.Caption);
				AssertEquals("Period", periodMonth.CaptionResourceString.Caption);
				AssertEquals("Period Start", startDateEdit.CaptionResourceString.Caption);
				AssertEquals("Period End", endDateEdit.CaptionResourceString.Caption);
				AssertEquals("Statement Date", statementDateEdit.CaptionResourceString.Caption);
				AssertEquals("VFD", vfdText.CaptionResourceString.Caption);
				AssertEquals("Statement Number", snText.CaptionResourceString.Caption);
				AssertEquals("RSF Status", statusText.CaptionResourceString.Caption);
				AssertEquals("RSF Submission Date", submissionText.CaptionResourceString.Caption);
				AssertEquals("RSF Accepted Date", acceptedText.CaptionResourceString.Caption);
			}
		}
	}
}
