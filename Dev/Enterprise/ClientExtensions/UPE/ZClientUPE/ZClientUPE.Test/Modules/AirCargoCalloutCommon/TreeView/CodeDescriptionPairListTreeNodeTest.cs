using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class CodeDescriptionPairListTreeNodeTest : TestCaseWithFactory
	{
		public void TestChildNodesPopulated()
		{
			TestNode.Expand();
			AssertEquals("3 code desc pairs should produce 3 child nodes", 3, TestNode.Nodes.Count);
			AssertEquals("First node", "Desc1", TestNode.Nodes[0].Text);
			AssertEquals("Second node", "Desc2", TestNode.Nodes[1].Text);
			AssertEquals("Separator node", "Separator", TestNode.Nodes[2].Text);
		}

		public void TestChildNodesCheckedIsUpdatedInitially()
		{
			CodeSet.Remove("CD1");
			CodeSet.Add("CD2");
			TestNode.Expand();
			AssertEquals("Parent node should be checked because at least 1 child node is checked", true, TestNode.Checked);
			AssertEquals("CD1 should not be checked", false, TestNode.Nodes[0].Checked);
			AssertEquals("CD2 should be checked", true, TestNode.Nodes[1].Checked);
		}

		public void TestCheckChildNodes()
		{
			TestNode.Expand();
			AssertEquals("The parent node should be unchecked initially as no child nodes are checked yet", false, TestNode.Checked);
			TestNode.Nodes[0].Checked = true;
			AssertEquals("When the first node is checked", 1, CodeSet.Count);
			AssertEquals("When the first node is checked CD1 exists in the selected codes", true, CodeSet.Contains("CD1"));
			AssertEquals("When the first node is checked CD2 not exists in the selected codes", false, CodeSet.Contains("CD2"));
			AssertEquals("The parent node should be checked now that at least 1 child node is checked", true, TestNode.Checked);
			TestNode.Nodes[1].Checked = true;
			AssertEquals("When the both nodes are checked", 2, CodeSet.Count);
			AssertEquals("When the both nodes are checked both codes exist in the set", true, CodeSet.Contains("CD1"));
			AssertEquals("When the both nodes are checked both codes exist in the set", true, CodeSet.Contains("CD2"));
			AssertEquals("The parent node should be checked now that at least 1 child node is checked", true, TestNode.Checked);
			TestNode.Nodes[0].Checked = false;
			AssertEquals("When only the second node is checked", 1, CodeSet.Count);
			AssertEquals("When only the second node is checked CD1 not exists in the selected codes", false, CodeSet.Contains("CD1"));
			AssertEquals("When only the second node is checked CD2 exists in the selected codes", true, CodeSet.Contains("CD2"));
			AssertEquals("The parent node should be checked now that at least 1 child node is checked", true, TestNode.Checked);
			TestNode.Nodes[1].Checked = false;
			AssertEquals("When both nodes are unchecked", 0, CodeSet.Count);
			AssertEquals("The parent node should be unchecked now that no child nodes are checked", false, TestNode.Checked);
		}

		public void TestUncheckingParentNodeCascadeUnchecksAllChildNodes()
		{
			TestNode.Expand();
			AssertEquals("The parent node should be unchecked initially as no child nodes are checked yet", false, TestNode.Checked);
			TestNode.Nodes[0].Checked = true;
			AssertEquals("The parent node should be checked now that at least 1 child node is checked", true, TestNode.Checked);
			TestNode.Checked = false;
			AssertEquals("When unchecking the parent node, all child nodes should also be unchecked", false, TestNode.Nodes[0].Checked);
		}

		public void TestCantCheckSeparatorChildNode()
		{
			TestNode.Expand();
			ICodeDescriptionPairTreeNode separatorNode = (ICodeDescriptionPairTreeNode)TestNode.Nodes[2];
			AssertEquals("Separator node (zero length code) should have no text", "", separatorNode.CodeDescPair.Code);
			separatorNode.Checked = true;
			AssertEquals("Separator node (zero length code) should not be allowed to be checked", false, separatorNode.Checked);
		}

		public void TestAllowCheck()
		{
			TestNode.ShouldAllowCheck = false;
			TestNode.Checked = true;
			AssertEquals("Dont allow Checked=true when AllowCheck=false", false, TestNode.Checked);
			TestNode.ShouldAllowCheck = true;
			TestNode.Checked = true;
			AssertEquals("Allow Checked=true when AllowCheck=true", true, TestNode.Checked);
		}

		public void TestCascadeCheck()
		{
			TestNode.ShouldAllowCheck = true;
			TestNode.ShouldCascadeCheck = true;
			AssertEquals("The parent node should be unchecked initially for the test", false, TestNode.Checked);
			AssertEquals("Child nodes shouldn't be populated initially for the test (1 placeholder node should exist only)", 1, TestNode.Nodes.Count);
			TestNode.Checked = true;
			AssertEquals("When checking the parent node, the parent node should be checked", true, TestNode.Checked);
			AssertEquals("When checking the parent node, all child nodes should be populated and checked", true, TestNode.Nodes[0].Checked);
		}

		#region Test Classes
		class TestCodeDescriptionPairListTreeNode : CodeDescriptionPairListTreeNode
		{
			public TestCodeDescriptionPairListTreeNode(TreeView treeView, string text, CodeSet selectedCodes, CodeDescriptionPairList codeDescPairList) : base(treeView, text, selectedCodes, codeDescPairList)
			{
			}

			public bool ShouldAllowCheck;
			protected override bool AllowCheck
			{
				get
				{
					return ShouldAllowCheck;
				}
			}

			public bool ShouldCascadeCheck;
			protected override bool CascadeCheck
			{
				get
				{
					return ShouldCascadeCheck;
				}
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			CodeDescriptionPairList codeDescPairList = new CodeDescriptionPairList();
			codeDescPairList.AddPair("CD1", "Desc1");
			codeDescPairList.AddPair("CD2", "Desc2");
			codeDescPairList.AddPair("", "Separator");
			TreeView = new ZTreeView();
			TestNode = new TestCodeDescriptionPairListTreeNode(TreeView, "Text", CodeSet, codeDescPairList);
			TreeView.Nodes.Add(TestNode);
			TreeView.CreateControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TreeView.Dispose();
		}

		ZTreeView TreeView;
		readonly CodeSet CodeSet = new CodeSet();
		TestCodeDescriptionPairListTreeNode TestNode;
		#endregion
	}
}
