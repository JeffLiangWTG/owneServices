using System.Linq;
using System.Windows.Forms;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMControlCustomisationForm))]
	class BMControlCustomisationFormTest : ZFormBasherTest
	{
		public void TestShowForm_ShouldDisplayPreviewControl()
		{
			foreach (ICodeDescription cdp in new CustomisedControlTypeList())
			{
				var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, cdp.Code);
				ShowFormAndAssertPreviewShown(customisation);
			}
		}

		static void ShowFormAndAssertPreviewShown(BMControlCustomisation customisation)
		{
			using (var form = new BMControlCustomisationForm(customisation))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<CustomisedControlDetailsUserControl>().Single();

				AssertEquals(1, control.PreviewGroupBox.Controls.Count);

				var previewControl = control.PreviewGroupBox.Controls[0];
				var label = previewControl as ZLabel;

				if (label != null)
				{
					Fail(string.Format("Preview control could not be created. Error label text: " + label.Tag));
				}
				else
				{
					AssertType<ZUserControl>(previewControl);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new BMControlCustomisationForm(Factory.New<BMControlCustomisation>()) { Size = ControlDpiScalingHelper.NewScaledSize(1200, 1000) };
		}
	}
}
