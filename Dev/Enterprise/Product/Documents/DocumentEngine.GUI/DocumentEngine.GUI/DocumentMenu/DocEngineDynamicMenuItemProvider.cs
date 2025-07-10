using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public sealed class DocEngineDynamicMenuItemProvider : DocEngineDynamicMenuProvider<DynamicMenuItem, ZDocumentMenuItem>
	{
		protected sealed override DynamicMenuItem GetNewMenuItem(string name)
		{
			return new DynamicMenuItem(name);
		}

		protected sealed override void AddPlaceHolder(DynamicMenuItem result)
		{
			result.MenuItems.Add(PlaceHolder);
		}
	}
}
