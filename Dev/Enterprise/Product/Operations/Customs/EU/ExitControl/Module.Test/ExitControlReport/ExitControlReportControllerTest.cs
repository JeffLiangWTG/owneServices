using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlReportController))]
	sealed class ExitControlReportControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.EU.ExitControlReport;

		public void TestReloadFormWillNotCauseError()
		{
			var controller = new ExitControlReportController();
			var exitReport = GetExitReportForTesting();
			Factory.Save();
			AssertNoExceptionThrown("Reload form will not cause error", () =>
			{
				using (var form = controller.ShowEditForm(exitReport) as ZForm)
				{
					form.Show();
					form.ReloadForm();
					var formCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
					Application.DoEvents();
					formCreatedByReloading.Close();
				}
			});
		}

		public void TestOpenedFormShowsReportsTab()
		{
			AssertControllerNotNull();
			var controller = new ExitControlReportController();
			var exitReport = GetExitReportForTesting();
			Factory.Save();
			using (var form = controller.ShowEditForm(exitReport) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				var exitControlUserControl = form.FindSingle<ExitControlUserControl>();
				var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
				AssertEquals(nameof(ExitControlUserControl.ReportsTabPage), exitControlTabControl.SelectedTab.Name);
			}
		}

		public void TestOpenFormForDeclaration()
		{
			var controller = new ExitControlReportController();
			var exitReport = GetExitReportForTesting();
			var declaration = Factory.New<JobDeclaration>();
			exitReport.Header.Parent = declaration;
			Factory.Save();

			using (var form = controller.ShowEditForm(exitReport) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<EU.GUI.JobDeclarationForm>("Should open a Declaration form for exit header ", form);
			}
		}

		public void TestOpenFormForShipment()
		{
			var controller = new ExitControlReportController();
			var exitReport = GetExitReportForTesting();
			exitReport.Header.Parent = Factory.New<ForwardingShipment>();

			Factory.Save();

			using (var form = controller.ShowEditForm(exitReport) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<ShipmentForm>("Should open a Shipment Form for exit report. ", form);
			}
		}

		public void TestOpenFormForConsol()
		{
			var controller = new ExitControlReportController();
			var exitReport = GetExitReportForTesting();
			exitReport.Header.Parent = Factory.New<ForwardingConsol>();

			Factory.Save();

			using (var form = controller.ShowEditForm(exitReport) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<ConsolForm>("Should open a Consol Form for exit report. ", form);
			}
		}

		public CusExitReport GetExitReportForTesting()
		{
			var header1 = Factory.NewWithValidTestData<CusExitHeader>();
			header1.CXH_JobReference = header1.PK.ToString().Substring(0, 35);
			var consignment1 = header1.CusExitConsignments.AddNew();

			var report1 = Factory.NewWithValidTestData<CusExitReport>();
			report1.CER_CXC_Consignment = consignment1.PK;
			report1.CER_OfficeOfExit = "DE001";
			report1.CER_CXH_Header = header1.PK;

			return report1;
		}
	}
}
