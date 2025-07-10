namespace Enterprise.ZArchitecture.GUI
{
	public interface ILayoutExtension
	{
		void Initialize(IControlHost controlHost);

		void Cleanup();
	}
}
