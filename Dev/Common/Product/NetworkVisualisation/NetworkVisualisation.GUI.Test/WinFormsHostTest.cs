using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class WinFormsHostTest : TestCase
	{
		public void TestRedrawHostControl_ShouldOnlyResizeTwice()
		{
			int controlSize = 500;
			var control = new ElementHost_ForTest()
			{
				Name = "TestControl",
				BackColor = Color.Transparent,
				Dock = DockStyle.Fill,
				Height = controlSize,
				Width = controlSize
			};

			control.InitForTest();
			AssertEquals("Precondition", 0, control.ResizedSizes.Count);

			WinFormsHost.RedrawHostControl_ForTest(control);
			AssertContainsExactElementsInAnyOrder("Should only resize twice, once with 0 pixels and one at full size", new int[] { 0, controlSize * controlSize }, control.ResizedSizes.Select(size => size.Width * size.Height).ToArray());
		}

		class ElementHost_ForTest : ElementHost
		{
			public List<Size> ResizedSizes { get; private set; }

			public void InitForTest()
			{
				ResizedSizes = new List<Size>();
			}

			public override void OnPropertyChanged(string propertyName, object value)
			{
				base.OnPropertyChanged(propertyName, value);
				if (propertyName == "Size")
				{
					this.ResizedSizes?.Add((Size)value);
				}
			}
		}
	}
}
