using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IControlHost
	{
		Control GetControl(ControlReference controlReference);

		BusinessObject GetCurrentDataItem();

		void UpdateLayout();

		Control GetControlByName(string controlName);
	}
}
