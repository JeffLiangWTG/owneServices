using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	[TestDate(2015, 7, 14)]
	class NetworkDiagramConstrainedSizeTest : NetworkGUITestCase
	{
		public void TestNonScaledNetworkDiagram_WhenChildShapeOpenedInOwnForm_ShouldBeSizedConstrained()
		{
			SetupDiagramAndShowForm(isScaled: false);
			AssertDiagramSizeIsFixed(diagramForm, widthShouldBeFixed: false, heightShouldBeFixed: false);

			OpenShapeInOwnForm();
			AssertDiagramSizeIsFixed(shapeForm, widthShouldBeFixed: true, heightShouldBeFixed: true);
		}

		public void TestScaledNetworkDiagram_WithNoScheduledTimes_WhenChildShapeOpenedInOwnForm_ShouldBeSizedConstrained()
		{
			SetupDiagramAndShowForm(isScaled: true);
			AssertDiagramSizeIsFixed(diagramForm, widthShouldBeFixed: false, heightShouldBeFixed: false);

			OpenShapeInOwnForm();
			AssertDiagramSizeIsFixed(shapeForm, widthShouldBeFixed: true, heightShouldBeFixed: true);
		}

		public void TestScaledNetworkDiagram_WithScheduledStartTime_WhenChildShapeOpenedInOwnForm_ShouldBeSizedConstrained()
		{
			SetupDiagramAndShowForm(isScaled: true, scheduledStartUtc: ZDateTime.UtcNow.AddDays(-7));
			AssertDiagramSizeIsFixed(diagramForm, widthShouldBeFixed: false, heightShouldBeFixed: false);

			OpenShapeInOwnForm();
			AssertDiagramSizeIsFixed(shapeForm, widthShouldBeFixed: true, heightShouldBeFixed: true);
		}

		public void TestScaledNetworkDiagram_WithScheduledFinishTime_WhenChildShapeOpenedInOwnForm_ShouldBeSizedConstrained()
		{
			SetupDiagramAndShowForm(isScaled: true, scheduledFinishUtc: ZDateTime.UtcNow.AddDays(7));
			AssertDiagramSizeIsFixed(diagramForm, widthShouldBeFixed: false, heightShouldBeFixed: false);

			OpenShapeInOwnForm();
			AssertDiagramSizeIsFixed(shapeForm, widthShouldBeFixed: true, heightShouldBeFixed: true);
		}

		public void TestScaledNetworkDiagram_WithScheduledStartAndFinishTime_WhenChildShapeOpenedInOwnForm_ShouldBeSizedConstrained()
		{
			SetupDiagramAndShowForm(isScaled: true, scheduledStartUtc: ZDateTime.UtcNow.AddDays(-3), scheduledFinishUtc: ZDateTime.UtcNow.AddDays(2));
			AssertDiagramSizeIsFixed(diagramForm, widthShouldBeFixed: true, heightShouldBeFixed: false);

			OpenShapeInOwnForm();
			AssertDiagramSizeIsFixed(shapeForm, widthShouldBeFixed: true, heightShouldBeFixed: true);
		}

		#region Implementation

		BMNCNShape diagram, shape;
		NetworkDiagramForm diagramForm, shapeForm;

		void SetupDiagramAndShowForm(bool isScaled, ZDateTime? scheduledStartUtc = null, ZDateTime? scheduledFinishUtc = null)
		{
			diagram = CreateDiagram(Factory, name: "String of strings!");
			shape = CreateShape(diagram, name: "Forever, and ever");
			shape.Left = 100;
			shape.Top = 100;
			shape.Width = 400;
			shape.Height = 100;

			if (isScaled)
			{
				var network = CreateNetwork(diagram);

				if (scheduledStartUtc != null)
				{
					diagram.ScheduledStartTimeUtc = scheduledStartUtc.Value;
				}

				if (scheduledFinishUtc != null)
				{
					diagram.ScheduledFinishTimeUtc = scheduledFinishUtc.Value;
				}

				network.SwitchToScaled();
				network.RefreshSchedules();

				var entity = network.Entities.GetInstance(shape);
				entity.X = 100;
				entity.Y = 100;

				network.RefreshSchedules();
			}

			diagramForm = new NetworkDiagramForm(diagram);
			diagramForm.Show();
		}

		static void AssertDiagramSizeIsFixed(NetworkDiagramForm form, bool widthShouldBeFixed, bool heightShouldBeFixed)
		{
			form.Size = ControlDpiScalingHelper.NewScaledSize(1200, 800);
			DoEventsThoroughly();

			var control = FindNetworkUserControl(form);

			var startingWidth = control.MainDiagramControl.ViewModel.ContentWidth;
			var startingHeight = control.MainDiagramControl.ViewModel.ContentHeight;

			form.Size = ControlDpiScalingHelper.NewScaledSize(1300, 900);
			DoEventsThoroughly();

			if (widthShouldBeFixed)
			{
				AssertEquals("ContentWidth of diagram canvas should not change when the form is resized because the diagram surface is fixed.", startingWidth, control.MainDiagramControl.ViewModel.ContentWidth);
			}
			else
			{
				AssertGreaterThanOrEqualTo("ContentWidth of diagram canvas should expand when the form is resized because the diagram surface is not fixed.", control.MainDiagramControl.ViewModel.ContentWidth, startingWidth);
			}

			if (heightShouldBeFixed)
			{
				AssertEquals("ContentHeight of diagram canvas should not change when the form is resized because the diagram surface is fixed.", startingHeight, control.MainDiagramControl.ViewModel.ContentHeight);
			}
			else
			{
				AssertGreaterThanOrEqualTo("ContentHeight of diagram canvas should expand when the form is resized because the diagram surface is not fixed.", control.MainDiagramControl.ViewModel.ContentHeight, startingHeight);
			}
		}

		void OpenShapeInOwnForm()
		{
			shapeForm = FindAndClickOpenAsDiagramMenuItem(diagramForm, shape);
		}

		protected override void TearDown()
		{
			base.TearDown();

			DoEventsThoroughly();
			diagramForm?.Dispose();
			shapeForm?.Dispose();
		}

		#endregion
	}
}
