namespace CargoWise.Common.Testing
{
	public interface IDisposableLeakListenerGuiElementManager
	{
		bool IsGuiElement(object element);
		void DoEvents();
	}
}
