using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleTreeViewTest : TestCaseWithFactory
	{
		public void TestAllLevelsAreHit()
		{
			using (ZModuleTreeViewForTest treeView = new ZModuleTreeViewForTest())
			{
				AssertEquals(0, treeView.Nodes.Count);
				treeView.Populate();
				Assert(treeView.ModuleCategoryTreeNodeHit);
				Assert(treeView.ModuleSectionTreeNodeHit);
				Assert(treeView.ModuleNodeTreeNodeHit);
				Assert(treeView.CustomModuleCategoriesHit);
				Assert(treeView.PopulateModuleItemsHit);

				int categoryCount = 0;
				int sectionCount = 0;
				int moduleCount = 0;
				foreach (ModuleCategory category in ModuleTree.Tree.Categories.Values)
				{
					if (category.Name != ModuleTreeLoaderConstant.Category.Jump.Name)
					{
						AssertEquals(category.Name, treeView.Nodes[categoryCount].Text);
						sectionCount = 0;
						foreach (ModuleSection section in category.Sections.Values)
						{
							AssertEquals(section.Name, treeView.Nodes[categoryCount].Nodes[sectionCount].Text);
							moduleCount = 0;
							foreach (INamedModule node in section.Modules.Values)
							{
								AssertEquals(node.Description, treeView.Nodes[categoryCount].Nodes[sectionCount].Nodes[moduleCount].Text);
								moduleCount++;
							}
							sectionCount++;
						}
					}
					categoryCount++;
				}
			}
		}

		public void TestSourceModuleTree()
		{
			using (ZModuleTreeViewForTest treeView = new ZModuleTreeViewForTest())
			{
				AssertEquals("SourceModuleTree", ModuleTree.Tree, treeView.SourceModuleTree);
				ModuleTree moduleTree = new ModuleTree();
				treeView.SourceModuleTree = moduleTree;
				AssertEquals("SourceModuleTree", moduleTree, treeView.SourceModuleTree);
			}
		}
	}
}
