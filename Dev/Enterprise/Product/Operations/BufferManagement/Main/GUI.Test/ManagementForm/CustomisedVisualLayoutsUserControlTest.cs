using System.Drawing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class CustomisedVisualLayoutsUserControlTest : BMSTestCaseWithFactory
	{
		public void TestPreview()
		{
			var system = CreateSystem();

			var customisation1 = system.CustomisedControls.AddNew();
			customisation1.FM_Name = "customisation1";
			customisation1.Width = 150;
			customisation1.Height = 100;
			customisation1.BackgroundColor = Color.Azure.Name;

			var customisation2 = system.CustomisedControls.AddNew();
			customisation2.FM_Name = "customisation2";
			customisation2.Width = 150;
			customisation2.Height = 100;
			customisation2.BackgroundColor = Color.Beige.Name;

			Factory.Save();

			using (var form = new ZForm(system))
			using (var control = new CustomisedVisualLayoutsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.LayoutsGrid.ListManager.Count);

				var firstPreviewControl = control.PreviewControl_Exposed;
				AssertNotNull(firstPreviewControl);

				control.LayoutsGrid.ListManager.Position = 1;

				var secondPreviewControl = control.PreviewControl_Exposed;
				AssertNotNull(secondPreviewControl);
				AssertNotEquals(firstPreviewControl, secondPreviewControl);
			}
		}
	}
}
