using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public static class TestExtensionMethods
	{
		public static void PerformClick(this Button button)
		{
			var peer = new ButtonAutomationPeer(button);

			if (peer.IsEnabled())
			{
				var pattern = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);

				pattern.Invoke();

				ApplicationHelper.DoEvents();
			}
		}
	}
}