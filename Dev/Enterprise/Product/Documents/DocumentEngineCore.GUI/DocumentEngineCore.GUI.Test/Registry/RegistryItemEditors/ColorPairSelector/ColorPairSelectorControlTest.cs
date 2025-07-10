using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(ColorPairSelectorControl))]
	sealed class ColorPairSelectorControlTest : RegistryZUserControlTestCase
	{
		public void TestCanOpenControlChangeTheColoursAndItWillWriteBackToTheRegistryItem()
		{
			var selector = new ColorPairSelector
			{
				PrimaryColor = Color.DarkTurquoise,
				SecondaryColor = Color.Cornsilk
			};

			using (var control = new ColorPairSelectorControlForTesting())
			{
				control.ColorPair = selector;

				AssertEquals("control.PreviewPanel.ForeColor", Color.DarkTurquoise, control.PreviewPanel.ForeColor);
				AssertEquals("control.PreviewPanel.BackColor", Color.Cornsilk, control.PreviewPanel.BackColor);

				control.ResultForNextColorDialog = Color.DarkGoldenrod;
				control.PrimaryColorChangeButton.PerformClick();
				AssertEquals("Expected Font color to be updated.", Color.DarkGoldenrod, control.PreviewPanel.ForeColor);
				AssertEquals("selector.PrimaryColor", Color.DarkGoldenrod, selector.PrimaryColor);

				control.ResultForNextColorDialog = Color.HotPink;
				control.SecondaryColorChangeButton.PerformClick();
				AssertEquals("Expected Background color to be updated.", Color.HotPink, control.PreviewPanel.BackColor);
				AssertEquals("selector.SecondaryColor", Color.HotPink, selector.SecondaryColor);
			}
		}

		#region Implementation

		class ColorPairSelectorControlForTesting : ColorPairSelectorControl
		{
			public Color ResultForNextColorDialog;

			protected override Color GetColorDialogResponse(Color initialColor)
			{
				return ResultForNextColorDialog;
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ColorPairSelector();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			ColorPairSelectorControl control = (ColorPairSelectorControl)control1;
			return !control.PrimaryColorChangeButton.Enabled && !control.SecondaryColorChangeButton.Enabled;
		}
		#endregion
	}
}
