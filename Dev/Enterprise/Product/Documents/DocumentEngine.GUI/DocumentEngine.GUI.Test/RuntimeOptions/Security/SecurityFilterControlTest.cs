using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class SecurityFilterControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.CaptionRenderingEnabled = true;
			result.Size = ControlDpiScalingHelper.NewScaledSize(new Size(640, 480));
			SecurityFilterControl control = new SecurityFilterControl();
			result.Controls.Add(control);
			control.SetFilter(new SecurityFilterField(Factory));
			return result;
		}
	}
}
