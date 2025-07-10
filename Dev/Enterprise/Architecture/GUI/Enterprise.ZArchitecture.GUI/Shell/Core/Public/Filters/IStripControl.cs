using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IStripControl
	{
		void UpdateLayout(int width);
		FilterOrCategory OrCategory { get; }
	}
}
