using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5LayoutTestHelper
	{
		internal static void AssertAdditionalTabPage<T>(ZTabControl tabControl, string additionalTabPageName, int index)
			where T : ZUserControl
		{
			var additionalTab = tabControl.GetTabPage(additionalTabPageName);
			AssertEquals($"Tab{index} Caption", $"Additional Tab Page For Test #{index}", additionalTab.CaptionResourceString.Caption);
			AssertEquals($"Tab{index} Dock style", DockStyle.Fill, additionalTab.Dock);

			var additionalTabUserControl = additionalTab.Controls.Cast<Control>().FirstOrDefault(x => x is T);
			AssertEquals($"Tab{index}UserControl BindingMember", $"Property{index}", additionalTabUserControl.GetBindingMember());
			AssertEquals($"Tab{index}UserControl Dock style", DockStyle.Fill, additionalTabUserControl.Dock);
		}
	}
}
