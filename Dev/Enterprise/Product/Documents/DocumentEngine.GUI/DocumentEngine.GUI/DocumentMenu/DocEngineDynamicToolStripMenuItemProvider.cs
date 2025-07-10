using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI
{
	public sealed class DocEngineDynamicToolStripMenuItemProvider : DocEngineDynamicMenuProvider<DynamicToolStripMenuItem, ZDocumentToolStripMenuItem>
	{
		protected sealed override DynamicToolStripMenuItem GetNewMenuItem(string name)
		{
			return new DynamicToolStripMenuItem(name);
		}

		protected sealed override void AddPlaceHolder(DynamicToolStripMenuItem result)
		{
			result.DropDownItems.Add(PlaceHolder);
		}
	}
}
