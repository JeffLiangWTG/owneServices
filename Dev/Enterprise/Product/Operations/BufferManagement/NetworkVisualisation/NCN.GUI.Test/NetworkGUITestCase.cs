using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Test;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public abstract class NetworkGUITestCase : NetworkTestCase
	{
		#region Helpers

		public static void DoEventsThoroughly()
		{
			ApplicationHelper.DoEvents();
			Application.DoEvents();
		}

		public static NetworkDiagramForm FindAndClickOpenAsDiagramMenuItem(NetworkDiagramForm form, BMNCNShape shapeToOpen)
		{
			var control = FindNetworkUserControl(form);
			var networkViewModel = control.ViewModel.NetworkViewModel;
			var menuItem = networkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(shapeToOpen, "Open as Diagram");

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shapeToOpen))
			{
				menuItem.Action.Execute();
			}
			DoEventsThoroughly();

			return Application.OpenForms.OfType<NetworkDiagramForm>().SingleOrDefault(f => f.BusinessEntity.Identifier == shapeToOpen.PK);
		}

		public static void FindAndClickOpenShapeListMenuItem(NetworkDiagramForm form, BMNCNShape shapeToOpen, Action<ZFilterGridModule> action)
		{
			var control = FindNetworkUserControl(form);
			var networkViewModel = control.ViewModel.NetworkViewModel;
			var menuItem = networkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeToOpen, "Actions", "Open Shape List");

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(innerForm =>
			{
				var popup = (EmbeddedModulePopup)innerForm;
				popup.Shown += (s, e) =>
				{
					var result = (ZFilterGridModule)popup.Module_ForTest;
					action(result);
				};
			});

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shapeToOpen))
			{
				menuItem.Action.Execute();
			}
			DoEventsThoroughly();
		}

		public static void InvokeEditPropertiesActionOnFormAndEnterValueIntoField<T>(NetworkDiagramForm formToExecuteOn, string name, string value)
			where T : Control
		{
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var editForm = (ShapeEntityDetailsForm)dialog;
				editForm.Shown += (s, e) =>
				{
					var tabControl = editForm.FindSingle<ZTabControl>("ShapeTabControl");
					var tabPage = tabControl.TabPages.Cast<ZTabPage>().Single(x => x.Name == "ShapeDetailsTabPage");
					tabControl.SelectTab(tabPage);
					Application.DoEvents();

					var field = tabPage.FindSingle<T>(name);
					field.Text = value;
					Application.DoEvents();

					var okButton = editForm.FindSingle<ZButton>("OKButton");
					okButton.PerformClick();
					Application.DoEvents();

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				};
			});

			var elementHost = formToExecuteOn.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
			var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
			var diagramControl = networkControl.MainDiagramControl;

			var networkViewModel = diagramControl.NetworkViewModel;
			var action = new EditPropertiesAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();
			DoEventsThoroughly();
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
		}

		public static IBMNetworkEntityController CreateController()
		{
			return new BMNetworkEntityController(new BMNetworkUserInteractionImplementor(new DummyProgressReporterProvider()), () => ContinueWithSave.No);
		}

		static ShapeNetworkEntity GetShape(NodeItem item)
		{
			return (ShapeNetworkEntity)((NodeViewModel)item.DataContext).Entity;
		}

#if !WINZOR
		public static void DoubleClickNode(BMNCNShape shapeToDoubleClick, NetworkDiagramForm form, object clickSource = null)
		{
			var networkControl = FindNetworkUserControl(form);
			var node = FindNode(shapeToDoubleClick, form);

			if (shapeToDoubleClick != null)
			{
				node.IsSelected = true;
			}

			try
			{
				var zoomAndPanControl = (ZoomAndPanControl)networkControl.MainDiagramControl.FindName("ZoomAndPanControl");
				var mouseEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
				{
					RoutedEvent = System.Windows.Controls.Control.MouseDoubleClickEvent,
					Source = clickSource,
				};

				zoomAndPanControl.RaiseEvent(mouseEventArgs);
			}
			finally
			{
				if (shapeToDoubleClick != null)
				{
					node.IsSelected = false;
				}
			}
		}

		public static NodeItem FindNode(BMNCNShape shape, NetworkDiagramForm form)
		{
			var elementHost = form.FindAll<KElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
			var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

			return shape == null ? null : networkControl.FindNodeItem(shape.Name);
		}
#endif

		public static ShapeNetworkEntity FindShapeOnForm(BMNCNShape shape, NetworkDiagramForm form)
		{
			var elementHost = form.FindAll<KElementHost>().Single();
			return (ShapeNetworkEntity)form.Network.Entities.First(entity => ((ShapeNetworkEntity)entity).Shape == shape);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public static NetworkUserControl FindNetworkUserControl(NetworkDiagramForm form)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var elementHost = form.FindSingle<KElementHost>();
#pragma warning disable CS0618 // Type or member is obsolete
			return (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		public static OpenedNetworkDiagramFormInfo GetOpenedDiagramFormInfoAndCloseOnDispose() => new OpenedNetworkDiagramFormInfo();

		#region OpenedNetworkDiagramFormInfo

		public class OpenedNetworkDiagramFormInfo : Assertion, IDisposable
		{
			public OpenedNetworkDiagramFormInfo()
			{
				Form = CargoWise.Windows.UI.ZApplication.GetOpenForms().OfType<NetworkDiagramForm>().SingleOrDefault();
				AssertNotNull(Form);
				NetworkDiagramUserControl = Form.FindSingle<NetworkDiagramUserControl>();
				AssertNotNull(NetworkDiagramUserControl);

				Application.DoEvents(); //to replace default dummy network with real JobNetwork
				AssertNotNull(JobNetwork);
			}

			public NetworkDiagramForm Form { get; }
			public NetworkDiagramUserControl NetworkDiagramUserControl { get; }
			public NetworkViewModel NetworkViewModel => NetworkDiagramUserControl.NetworkViewModel;
			public IJobNetwork JobNetwork => NetworkViewModel.GetJobNetwork();

			public void Dispose()
			{
				Form.Close();
			}
		}

		#endregion

		#endregion

		#region Assertions

		public static void AssertCoordinates(NodeViewModel viewModel, double? x = null, double? y = null, double? width = null, double? height = null)
		{
			AssertCoordinates(string.Empty, viewModel, x, y, width, height);
		}

		public static void AssertCoordinates(string message, NodeViewModel viewModel, double? x = null, double? y = null, double? width = null, double? height = null)
		{
			var shape = (ShapeNetworkEntity)viewModel.Entity;
			var name = shape.Name;
			CombineAssertions(message, () =>
			{
				AssertIfNotNull(name, "X", x, viewModel.X);
				AssertIfNotNull(name, "Y", y, viewModel.Y);
				AssertIfNotNull(name, "Width", width, viewModel.Width);
				AssertIfNotNull(name, "Height", height, viewModel.Height);
			});

			AssertCoordinates(message, shape, x, y, width, height);
		}

		public static void AssertCoordinates(NodeItem node, double? x = null, double? y = null, double? width = null, double? height = null)
		{
			var shape = GetShape(node);
			var name = shape.Name;
			CombineAssertions(() =>
			{
				AssertIfNotNull(name, "X", x, shape.X);
				AssertIfNotNull(name, "Y", y, shape.Y);
				AssertIfNotNull(name, "Width", width, shape.Width);
				AssertIfNotNull(name, "Height", height, shape.Height);
			});
		}

		public static void AssertIfNotNull(string objectName, string propertyName, double? expected, double actual)
		{
			if (expected.HasValue)
			{
				var message = string.Format("Comparing [{0}] {1}", objectName, propertyName);
				AssertEquals(message, expected.Value, actual);
			}
		}

		public static void AssertAttachmentExsists(string attachmentType, NodeItem owner, NodeItem fromNode, NodeItem toNode)
		{
			AssertAttachmentExists(attachmentType, GetShape(owner).Shape, GetShape(fromNode).Shape, GetShape(toNode).Shape);
		}

		public static void AssertAttachmentExists(string attachmentType, BMNCNShape owner, BMNCNShape fromShape, BMNCNShape toShape)
		{
			var attachment = owner.AllAttachments.SingleOrDefault(a => a.BNA_BNS_FromShape == toShape.PK && a.BNA_BNS_FromShape == fromShape.PK && a.BNA_Type == attachmentType);
			AssertNotNull(string.Format("Expect attachment type [{0}] with owner [{1}], from [{2}] to [{3}].", attachmentType, owner, fromShape, toShape));
		}

#if !WINZOR
#pragma warning disable CS0618 // Type or member is obsolete
		public static void AssertArrowCount(int arrowCount, NetworkUserControl networkDiagram)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			AssertEquals(arrowCount, networkDiagram.FindChildren<CurvedArrow>().Count());
		}
#endif
		#endregion
	}
}
