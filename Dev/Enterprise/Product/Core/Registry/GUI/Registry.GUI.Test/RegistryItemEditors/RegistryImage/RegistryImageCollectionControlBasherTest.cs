using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RegistryFormForTest))]
	sealed class RegistryImageCollectionControlBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new RegistryFormForTest();
			form.CaptionRenderingEnabled = true;
			form.Size = ControlDpiScalingHelper.NewScaledSize(400, 1500, true);
			var control = new RegistryImageCollectionControlForTest() { Dock = DockStyle.Fill };
			{
				form.Controls.Add(control);
			}
			form.Show();
			Application.DoEvents();
			return form;
		}
	}
}
