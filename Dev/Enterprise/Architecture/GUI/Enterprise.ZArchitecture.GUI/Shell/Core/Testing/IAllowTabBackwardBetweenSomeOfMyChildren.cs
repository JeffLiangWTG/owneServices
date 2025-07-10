using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IAllowTabBackwardBetweenSomeOfMyChildren
	{
		bool AllowTabBackward(Control control, Control previousControl);
	}
}
