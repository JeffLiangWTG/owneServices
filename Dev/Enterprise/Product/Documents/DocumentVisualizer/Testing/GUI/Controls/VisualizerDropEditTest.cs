using System.Reflection;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class VisualizerDropEditTest : ZControlBaseTestCase<VisualizerDropEdit>
	{
		public void TestSynchroniseControlSizesDoesNotCauseOutOfBoundsInForm()
		{
			using (var dropEdit = new VisualizerDropEditForTest())
			{
				var correctWidth = dropEdit.Width;
				dropEdit.DropButtonExposed.Width = dropEdit.Width + 2;
				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals(correctWidth, dropEdit.Width);
				AssertEquals(correctWidth, dropEdit.DropButtonExposed.Width);
			}
		}

		public void TestChangingControlHeightSynchroniseElementsHeight()
		{
			using (var dropEdit = new VisualizerDropEditForTest())
			{
				var expectedHeight = dropEdit.Height + 20;
				dropEdit.Height = expectedHeight;

				dropEdit.SynchroniseControlSizesExposed();
				AssertEquals(expectedHeight, dropEdit.DescriptionBox.Height);
				AssertEquals(expectedHeight, dropEdit.DropButtonExposed.Height);
			}
		}

		public void TestDropButtonButton()
		{
			using (var dropEdit = new VisualizerDropEditForTest())
			{
				var buttonAspecthRatio = 0.7;

				dropEdit.Width = 100;
				dropEdit.Height = 50;

				AssertEquals(typeof(VisualizerDropButton), dropEdit.DropButtonExposed.GetType());

				var buttonRectangleProperty = typeof(ZDropButton).GetProperty("ButtonRectangle", BindingFlags.Instance | BindingFlags.NonPublic);
				var buttonRectangle = (System.Drawing.Rectangle)buttonRectangleProperty.GetValue(dropEdit.DropButtonExposed);

				var expectedWidth = buttonRectangle.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3) + ControlDpiScalingHelper.ScaleToCurrentDpiX(1);

				AssertEquals("Rectangle X should be offset 1 pixel for depth effect", (int)(dropEdit.Width - (dropEdit.Height * buttonAspecthRatio) - 1), buttonRectangle.X);
				AssertEquals("Rectangle Y should be offset 1 pixel for depth effect", 1, buttonRectangle.Y);

				AssertEquals("ExpectedWidth ajusted to account for padding and additional space", expectedWidth, dropEdit.GetButtonWidthExposed());
				AssertEquals("2 Pixel adjustment to account for depth effect for top and bottom", dropEdit.Height - 2, buttonRectangle.Height);
			}
		}

		class VisualizerDropEditForTest : VisualizerDropEdit
		{
			public ZDropButton DropButtonExposed { get { return DropButton; } }

			public int GetButtonWidthExposed()
			{
				return GetButtonWidth();
			}

			public void SynchroniseControlSizesExposed()
			{
				SynchroniseControlSizes();
			}
		}

		#region Implementation

		protected override string[] BindablePropertyNames
		{
			get
			{
				return new string[] { "Text", "ReadOnly", "IsVisibleForBinding", "ShowDescriptionBox", "ShowDescriptionInDropDown" };
			}
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Size"; }
		}

		protected override void BindControl()
		{
			base.BindControl();
			Control.SetDataBinding(Dummy, DummyBizoSchema.Z0_Description.Name);
		}

		#endregion

	}
}
