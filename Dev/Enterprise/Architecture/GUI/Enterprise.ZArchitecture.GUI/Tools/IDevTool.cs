using System.Windows.Forms;

namespace Enterprise.ZArchitecture.DevTools
{
	public interface IDevTool
	{
		string Name { get; }
		bool AddAsButton { get; }
		void Show(Form form);
	}
}
