using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class SalesTradeLaneChecklistUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new ZEmptyFormForBasherTest();
			form.Size = ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			form.CaptionRenderingEnabled = true;

			var control = new SalesTradeLaneChecklistUserControl();
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);
			return form;
		}
	}
}
