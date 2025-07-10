using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualiserComponentControlFactoryTest : TestCase
	{
		public void TestCreate()
		{
			VisualiserComponentControlFactory controlFactory = new VisualiserComponentControlFactory();

			using (Control control = controlFactory.Create(new VisualiserComponentLabel(Point.Empty, Size.Empty, "Test", new CellFormat())))
			{
				AssertEquals("control.GetType()", typeof(ZLabel), control.GetType());
			}

			using (Image image = new Bitmap(1, 1))
			{
				using (Control control = controlFactory.Create(new VisualiserComponentImage(Point.Empty, Size.Empty, image)))
				{
					Assert("control.GetType()", typeof(PictureBox).IsAssignableFrom(control.GetType()));
				}
			}

			using (VisualiserDataSet dataSet = new VisualiserDataSet())
			{
				using (Control control = controlFactory.Create(new VisualiserComponentTextBox(Point.Empty, Size.Empty, dataSet, "Test", new CellFormat())))
				{
					AssertEquals("control.GetType()", typeof(ZTextBox), control.GetType());
				}

				using (Control control = controlFactory.Create(new VisualiserComponentGrid(Point.Empty, Size.Empty, dataSet, "Test")))
				{
					AssertEquals("control.GetType()", typeof(DataGridView), control.GetType());
				}
			}

			using (var control = controlFactory.Create(new VisualiserComponentBorder(Point.Empty, Size.Empty, new CellFormat())))
			{
				AssertNull("control", control);
			}

			AssertExceptionThrown("controlFactory.Create(new InvalidVisualiserComponent())", typeof(ArgumentException), delegate
			{ controlFactory.Create(new InvalidVisualiserComponent()); });
		}

		#region Implementation
		class InvalidVisualiserComponent : VisualiserComponent
		{
			internal InvalidVisualiserComponent()
				: base(Point.Empty, Size.Empty)
			{
			}
		}
		#endregion
	}
}
