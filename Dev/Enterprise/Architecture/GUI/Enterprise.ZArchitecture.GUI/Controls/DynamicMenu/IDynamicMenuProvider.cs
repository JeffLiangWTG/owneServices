using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IDynamicMenuProvider<T> where T : IDynamicMenu
	{
		T GetMenuItem(Form parentForm, BusinessObject businessObject);
	}
}
