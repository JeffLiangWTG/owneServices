using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace CargoWise.Windows.UI
{
	class DisposableLeakListenerGuiElementManager : IDisposableLeakListenerGuiElementManager
	{
		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "For unit testing only")]
		public void DoEvents()
		{
			System.Windows.Forms.Application.DoEvents();
		}

		public bool IsGuiElement(object element)
		{
			return element is Control;
		}
	}
}
