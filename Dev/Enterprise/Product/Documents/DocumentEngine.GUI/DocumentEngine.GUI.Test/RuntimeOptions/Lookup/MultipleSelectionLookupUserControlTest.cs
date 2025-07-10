using System.Linq;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(MultipleSelectionLookupUserControl))]
	sealed class MultipleSelectionLookupUserControlTest : RuntimeOptionUserControlBaseTest<MultipleSelectionLookupUserControl>
	{
		public override void TestChangeLabelSizeForAlignment()
		{
			using (var box = new MultipleSelectionLookupUserControl())
			{
				box.Width = 500;
				var label = box.Controls.OfType<ZLabel>().SingleOrDefault();
				box.ChangeLabelSizeForAlignment(200);
				AssertEquals(200 - label.Left, label.Width);
				AssertEquals(200, box.Grid.Left);
				AssertEquals(300, box.Grid.Width);
			}
		}

		public override void TestDesiredCaptionWidth()
		{
			using (var box = new MultipleSelectionLookupUserControl())
			{
				var label = box.Controls.OfType<ZLabel>().SingleOrDefault();
				label.CaptionResourceString = Res.GetData("eda71790-51c2-433a-89e1-6a6e9d2ff6cf", "Test Multiple Selection Lookup User Control Desired Caption Width");
				AssertEquals(label.Width, box.DesiredCaptionWidth);
			}
		}
	}
}
