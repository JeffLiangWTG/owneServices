using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ErrorReporting.Business;
using Enterprise.ErrorReporting.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ErrorReporting.Module.Test
{
	public class ErrorReportDetailsControllerTest : TestCaseWithFactory
	{
		public void TestSecurityCheckPointForNew()
		{
			var controller = new ErrorReportDetailsController();
			var checkpoint = controller.GetCheckPointForNew(null);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestSecurityCheckPointForEdit()
		{
			var controller = new ErrorReportDetailsController();
			var checkpoint = controller.GetCheckPointForEdit(null);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestSecurityCheckPointForView()
		{
			var controller = new ErrorReportDetailsController();
			var checkpoint = controller.GetCheckPointForView(null);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestSecurityCheckPointForDelete()
		{
			var controller = new ErrorReportDetailsController();
			var checkpoint = controller.GetCheckPointForDelete(null);
			AssertEquals(Env.Security.None, checkpoint);
		}

		public void TestGetForm()
		{
			var controller = new ErrorReportDetailsController();
			var report = Factory.New<StmErrorReport>();
			Factory.Save();

			using (var form = controller.ShowViewForm(report))
			{
				AssertNotNull(form);
				AssertType(typeof(ErrorDetailsForm), form);
			}
		}

		public void TestEditFormIsReadOnly()
		{
			var controller = new ErrorReportDetailsController();
			var report = Factory.New<StmErrorReport>();
			Factory.Save();

			using (var form = controller.ShowEditForm(report))
			{
				AssertNotNull(form);
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
			}
		}
		
		public void TestCurrentRecordNumberCalcEditShouldEditable()
		{
			var xml = "<a><b><c>d</c><d /></b><e f=\"g\">h</e><i j=\"k\" /></a>";
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = xml;
			Factory.Save();

			using (var form = new ErrorReportDetailsController().ShowEditForm(report) as ZForm)
			{
				form.Show();
				AssertEquals("ErrorDetails.CurrentRecordNumberCalcEdit.Readonly should be false", false, form.Controls.Find("CurrentRecordNumberCalcEdit", true).IsReadOnly);
			}
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			var controller = new ErrorReportDetailsController();

			AssertEquals(typeof(StmErrorReport), controller.TypeOfTopLevelBusinessObject);
		}
	}
}
