using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class ZModuleCountryTreeViewTest : TestCaseWithFactory
	{
		public void TestAllLevelsAreHit()
		{
			RefCountryCollection coll = new RefCountryCollection(Factory);
			coll.AdditionalFilter = new ZQuery(RefCountrySchema.RN_Code, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			using (ZModuleCountryTreeView treeView = new ZModuleCountryTreeView())
			{
				AssertEquals(0, treeView.Nodes.Count);
				treeView.Populate(coll);

				int categoryCount = 0;
				int sectionCount = 0;
				int moduleCount = 0;
				foreach (ModuleCategory category in Enterprise.Core.Modules.ModuleTree.Tree.Categories.Values)
				{
					if (categoryCount > 0) // Going to skip "Jump"
					{
						Assert(treeView.Nodes[categoryCount] is ZModulePointNode);
						AssertEquals(category.DisplayTextWithoutAmpersand, treeView.Nodes[categoryCount].Text);
						sectionCount = 0;
						foreach (ModuleSection section in category.Sections.Values)
						{
							Assert(treeView.Nodes[categoryCount].Nodes[sectionCount] is ZModulePointNode);
							AssertEquals(section.DisplayTextWithoutAmpersand, treeView.Nodes[categoryCount].Nodes[sectionCount].Text);
							moduleCount = 0;
							foreach (INamedModule node in section.Modules.Values)
							{
								TreeNode currentNode = treeView.Nodes[categoryCount].Nodes[sectionCount].Nodes[moduleCount];
								Assert(currentNode is ZModulePointNode);
								AssertEquals(node.Description, currentNode.Text);
								AssertEquals(1, currentNode.Nodes.Count);
								AssertEquals("All Countries/Regions", currentNode.Nodes[0].Text);
								if (section.Subcategory.Name == Enterprise.ZArchitecture.Modules.ModuleTreeLoaderConstant.Subcategory.Customs.Name)
								{
									AssertEquals(1, currentNode.Nodes[0].Nodes.Count);
									AssertEquals(GlbCompany.CurrentCompany.Country.Description, currentNode.Nodes[0].Nodes[0].Text);
								}
								else
								{
									AssertEquals(0, currentNode.Nodes[0].Nodes.Count);
								}
								moduleCount++;
							}
							sectionCount++;
						}
					}
					categoryCount++;
				}
			}
		}
	}
}
