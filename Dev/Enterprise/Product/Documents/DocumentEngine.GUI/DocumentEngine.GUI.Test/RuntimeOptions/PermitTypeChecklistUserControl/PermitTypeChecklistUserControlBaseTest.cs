using System.Linq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(PermitTypeChecklistUserControl))]
	sealed class PermitTypeChecklistUserControlBaseTest : RuntimeOptionUserControlBaseTest<PermitTypeChecklistUserControl>
	{
		public override void TestChangeLabelSizeForAlignment()
		{
			using (var box = new PermitTypeChecklistUserControl())
			{
				box.Width = 500;
				var label = box.Controls.Find("fieldLabel", true).Single();
				box.ChangeLabelSizeForAlignment(200);
				AssertEquals(200 - label.Left, label.Width);
			}
		}

		public override void TestDesiredCaptionWidth()
		{
			using (var box = new PermitTypeChecklistUserControl())
			{
				box.Width = 500;
				var label = box.Controls.Find("fieldLabel", true).Single();
				AssertEquals(box.Controls.Find("fieldLabelPanel", false).Single().Width - label.Padding.Vertical, label.Width);
			}
		}
	}
}
