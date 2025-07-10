using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Test;
using Enterprise.BufferManagement.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class ZoomAndPanUserControlTest : TestCase
	{
		public void TestPopOutRetainsCorrectTemplateSelector()
		{
			window.Show();
			popOutWindow = control.PopOut();
			AssertNotNull("PopOut Window", popOutWindow);
#pragma warning disable CS0618 // Type or member is obsolete
			var popoutControl = popOutWindow.Content as NetworkUserControl;
#pragma warning restore CS0618 // Type or member is obsolete
			AssertNotNull(popoutControl);
			AssertNotNull("TemplateSelector should not be removed", popoutControl.ViewModel.TemplateSelector);
			AssertEquals(provider, popoutControl.ViewModel.NetworkViewModel.NodeViewModelProvider);
			AssertEquals("Untitled", popOutWindow.Title);
		}

		DummyNetwork network;
#pragma warning disable CS0618 // Type or member is obsolete
		NetworkUserControl control;
#pragma warning restore CS0618 // Type or member is obsolete
		Window window;
		NodeViewModelProvider provider;
		Window popOutWindow;

		protected override void SetUp()
		{
			base.SetUp();
			var diagramEntity = new Entity
			{
				IsDiagramScaled = true,
				Scale = 100,
				ResolutionIncrement = 50,
				ScaleUnitPixelSize = 200,
			};
			var entity = new Entity { Width = 200, Height = 100, ShapeType = ShapeTypeList.Codes.Buffer };
			var refresher = new NetworkRefresher();
			network = new DummyNetwork { DiagramEntity = diagramEntity, Refresher = refresher };
			refresher.AssociateWithNetwork(network);

			network.Entities.Add(entity);

			provider = new DummyNodeViewModelProvider();

			window = new Window();
#pragma warning disable CS0618 // Type or member is obsolete
			control = new NetworkUserControl(diagramEntity, refresher, new EntityTemplateSelector(), provider);
#pragma warning restore CS0618 // Type or member is obsolete
			window.Content = control;
			control.SetDataContext(network, isReloading: false);

			control.Height = 600;
			control.Width = 800;
			network.DiagramEntity.CornerRadius = 10;
		}

		protected override void TearDown()
		{
			ApplicationHelper.DoEvents();
			window.Close();
			control.Dispose();
			control = null;
			window = null;
			if (popOutWindow != null)
			{
				popOutWindow.Close();
				popOutWindow = null;
			}
			base.TearDown();
		}

		class DummyNodeViewModelProvider : NodeViewModelProvider
		{
			public DummyNodeViewModelProvider()
				: base()
			{ }
		}
	}
}
