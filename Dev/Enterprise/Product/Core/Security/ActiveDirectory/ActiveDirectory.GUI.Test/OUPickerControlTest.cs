using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using Enterprise.Security.ActiveDirectory.Test;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class OUPickerControlTest : TestCaseWithFactoryAndMocks
	{
		public void TestNoNodesPresentWhenNoDomainControllerSpecified()
		{
			using (var control = new OUPickerControl())
			{
				AssertEquals(0, control.DirectoryTreeView.Nodes.Count);
			}
		}

		public void TestNodesShouldBeDisplayedWhenDomainControllerSpecified()
		{
			using (var control = new OUPickerControl())
			{
				var searcher = new DirectorySearcherWrapper(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain);
				control.PopulateTree(searcher, TestConstants.Domain);

				AssertNotEquals("Should be at least one root node", 0, control.DirectoryTreeView.Nodes.Count);
				AssertNotEquals("Root node should have at least one child", 0, control.DirectoryTreeView.Nodes[0].Nodes.Count);
			}
		}

		public void TestSelectingPopulatedNodeShouldSetTextBoxValue()
		{
			using (var control = new OUPickerControl())
			{
				var searcher = new DirectorySearcherWrapper(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain);
				control.PopulateTree(searcher, TestConstants.Domain);

				var node = control.DirectoryTreeView.Nodes[0].Nodes[0];
				AssertNotEquals("Should find one child of root node", null, node);
				control.DirectoryTreeView.SelectedNode = node;
				control.OnDirectoryTreeView_AfterSelect(node);

				AssertEquals("Text box text should be set to node text when node selected", node.Text, control.SelectedOU);
				AssertEquals("Text box text should not contain slash (root node should not be expressed since it is the domain controller)",
					false, control.SelectedOU.Contains("/"));
			}
		}

		public void TestFindAndSelectOU()
		{
			using (var control = new OUPickerControl())
			{
				var searcher = new DirectorySearcherWrapper(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain);
				control.PopulateTree(searcher, TestConstants.Domain);
				control.FindAndSelectOU(TestConstants.ValidOU);
				AssertEquals(TestConstants.ValidOU, control.DirectoryTreeView.SelectedNode.Tag);

				control.PopulateTree(searcher, TestConstants.Domain);
				control.FindAndSelectOU(TestConstants.ValidOU.ToUpper());
				AssertEquals(TestConstants.ValidOU, control.DirectoryTreeView.SelectedNode.Tag);
			}
		}
	}
}
