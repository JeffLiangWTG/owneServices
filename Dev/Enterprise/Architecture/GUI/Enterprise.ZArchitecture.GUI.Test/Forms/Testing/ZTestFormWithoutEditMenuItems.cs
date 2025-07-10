using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class ZTestFormWithoutEditMenuItems : ZTestForm
	{
		public ZTestFormWithoutEditMenuItems(IBusiness entity)
			: base(entity)
		{
			this.EditMenuItem.MenuItems.Clear();
		}
	}
}
