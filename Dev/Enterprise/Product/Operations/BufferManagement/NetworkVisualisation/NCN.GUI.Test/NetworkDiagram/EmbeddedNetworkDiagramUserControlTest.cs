using System.Linq;
using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class EmbeddedNetworkDiagramUserControlTest : NetworkTestCase
	{
		public void TestUnhookEvents()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();
			Window popoutWindow = null;
			NetworkViewModel networkViewModel;
			JobNetwork network;

			try
			{
				using (DisableAsyncBehaviour())
				using (var control = new WorkflowsUserControl())
				using (var form = new ZForm(viewModel))
				{
					control.SetDataBinding(viewModel, string.Empty);
					form.Controls.Add(control);
					form.Show();
					System.Windows.Forms.Application.DoEvents();

					control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;
					System.Windows.Forms.Application.DoEvents();

					using (var host = control.WorkflowNCNTabControl_Exposed.FindAll<ZElementHost>().Single())
					{
#pragma warning disable CS0618 // Type or member is obsolete
						var networkUserControl = (NetworkUserControl)host.Child;
#pragma warning restore CS0618 // Type or member is obsolete
						popoutWindow = networkUserControl.PopOut();

						form.BringToFront();
						networkViewModel = networkUserControl.ViewModel.NetworkViewModel;
						network = (JobNetwork)networkUserControl.ViewModel.NetworkViewModel.Network;
					}
				}

				AssertNoExceptionThrown(() => networkViewModel.CreateNewWorkflow(network.DiagramShape));
			}
			finally
			{
				if (popoutWindow != null)
				{
					popoutWindow.Close();
				}
			}
		}

		public void TestEmbeddedNetworkDiagramPopoutIsClosedWhenParentFormIsClosed()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();
			Window popoutWindow = null;
			using (DisableAsyncBehaviour())
			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;

				using (var host = control.WorkflowNCNTabControl_Exposed.FindAll<ZElementHost>().Single())
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var networkUserControl = (NetworkUserControl)host.Child;
#pragma warning restore CS0618 // Type or member is obsolete
					popoutWindow = networkUserControl.PopOut();
				}
			}
			Assert(!popoutWindow.IsActive);
		}

		public void TestEmbeddedNetworkDiagramPopoutChildrenAreClosedWhenParentFormIsClosed()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();
			Window popoutWindow = null;
			Window popoutWindow2 = null;
			using (DisableAsyncBehaviour())
			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;

				using (var host = control.WorkflowNCNTabControl_Exposed.FindAll<ZElementHost>().Single())
				{
#pragma warning disable CS0618 // Type or member is obsolete
					var networkUserControl = (NetworkUserControl)host.Child;
					popoutWindow = networkUserControl.PopOut();
					var foo = (NetworkUserControl)popoutWindow.Content;
#pragma warning restore CS0618 // Type or member is obsolete
					popoutWindow2 = foo.PopOut();
				}
			}

			Assert(!popoutWindow.IsActive);
			Assert(!popoutWindow2.IsActive);
		}
	}
}
