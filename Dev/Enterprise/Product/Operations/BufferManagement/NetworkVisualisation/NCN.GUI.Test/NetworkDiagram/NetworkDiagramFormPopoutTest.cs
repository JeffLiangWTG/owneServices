using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class NetworkDiagramFormPopoutTest : NetworkGUITestCase
	{
		public void TestWidthBindsToViewModel_AdjustToScaleStillWorks()
		{
			var diagram = CreateDiagram(Factory, name: "NotEveryoneGetsTheApple", isScaled: true);
			var shape = CreateShape(diagram, name: "Shape1");

			using (DisableAsyncBehaviour())
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				shape.AsEntity(form.Network).SetCoordinates(width: 300, height: 200, x: 100, y: 100);

				var networkUserControl = GetNetworkUserControl(form);
				popOutWindow = networkUserControl.PopOut();

				form.BringToFront();
#pragma warning disable CS0618 // Type or member is obsolete
				var popOutUserControl = popOutWindow.FindChildren<NetworkUserControl>().Single();
#pragma warning restore CS0618 // Type or member is obsolete
				var popoutShapeModel = popOutUserControl.FindShapeViewModel("Shape1");
				System.Windows.Forms.Application.DoEvents();

				var shape1PopoutViewModel = popOutUserControl.FindShapeViewModel("Shape1");
				shape1PopoutViewModel.Width = 395d;

				var thumbs = popOutUserControl.FindChildren<ResizeThumb>();
				AssertEquals("4 thumbs are expected, one at each corner", 4, thumbs.Count());
				var resizeThumb = thumbs.Single(t => t.HorizontalAlignment == HorizontalAlignment.Right && t.VerticalAlignment == VerticalAlignment.Bottom);
				resizeThumb.RaiseEvent(new DragCompletedEventArgs(95, 0, false));
				GCWrapper.ReclaimMemory(ref popOutUserControl);

				popOutWindow.Close();
				GCWrapper.ReclaimMemory(ref popOutWindow);

				AssertCoordinates(shape1PopoutViewModel, x: 100, y: 100, width: 400, height: 200);

				var shape1Node = networkUserControl.FindNodeItem("Shape1");

				GCWrapper.ReclaimMemory(ref networkUserControl);

				AssertCoordinates(shape1Node, x: 100, y: 100, width: 400, height: 200);
			}
		}

		public void TestCloseForm_ShouldAlsoClosePopOuts()
		{
			var diagram = CreateDiagram(Factory, name: "NotEveryoneGetsTheApple", isScaled: true);

			using (DisableAsyncBehaviour())
			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				var shape = CreateShape(diagram);

				popOutWindow = GetNetworkUserControl(form).PopOut();

				Assert(popOutWindow.IsActive);
				form.Close();
				Assert(!popOutWindow.IsActive);
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		NetworkUserControl GetNetworkUserControl(NetworkDiagramForm form)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return (NetworkUserControl)form.NetworkDiagramControl.FindSingle<ZElementHost>().Child;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		Window popOutWindow;

		protected override void TearDown()
		{
			GC.Collect();
			if (popOutWindow != null)
			{
				popOutWindow.Close();
				popOutWindow = null;
			}
			base.TearDown();
		}
	}
}
