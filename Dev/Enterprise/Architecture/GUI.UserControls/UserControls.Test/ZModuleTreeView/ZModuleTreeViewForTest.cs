using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleTreeViewForTest : ZModuleTreeView
	{
		protected override TreeNode GetModuleCategoryNode(ModuleCategory category)
		{
			ModuleCategoryTreeNodeHit = ZBool.True;
			return new TreeNode(category.Name);
		}
		public ZBool ModuleCategoryTreeNodeHit;

		protected override TreeNode GetModuleSectionNode(ModuleSection section)
		{
			ModuleSectionTreeNodeHit = ZBool.True;
			return new TreeNode(section.Name);
		}
		public ZBool ModuleSectionTreeNodeHit;

		protected override TreeNode GetModuleNode(INamedModule module)
		{
			ModuleNodeTreeNodeHit = ZBool.True;
			return new TreeNode(module.Description);
		}
		public ZBool ModuleNodeTreeNodeHit;

		protected override void PopulateCustomModuleCategories()
		{
			CustomModuleCategoriesHit = ZBool.True;
		}
		public ZBool CustomModuleCategoriesHit;

		protected override void PopulateModuleItems(TreeNode newModuleNode, INamedModule module)
		{
			PopulateModuleItemsHit = ZBool.True;
		}
		public ZBool PopulateModuleItemsHit;
	}
}
