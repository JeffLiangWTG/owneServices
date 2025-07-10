using System.Drawing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.UPE.GUI.Testing
{
	class ZDashboardZLabelTest : ZLabelTest
	{
		public void TestFont()
		{
			using (ZDashboardZLabel label = new ZDashboardZLabel())
			{
				AssertEquals("Font should be 'Arial Black'", "Arial Black", label.Font.FontFamily.Name);
				AssertEquals("Font size should be 23F", 23F, label.Font.Size);
				AssertEquals("Font Style should be Bold", FontStyle.Bold, label.Font.Style);
				Assert("Should be Bold", label.Font.Bold);
			}
		}

		protected override ZLabel GetNewControl()
		{
			return new ZDashboardZLabel();
		}
	}
}
