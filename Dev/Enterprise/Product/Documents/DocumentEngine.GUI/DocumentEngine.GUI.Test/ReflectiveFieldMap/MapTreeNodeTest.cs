using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReflectiveFieldMap;
using DocumentWrapperForTest = Enterprise.DocumentEngine.ReflectiveFieldMap.Testing.DocumentWrapperForTest;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap.Testing
{
	sealed class MapTreeNodeTest : TestCaseWithFactory
	{
		public void TestPropertyLazyLoadsChildrenForRelatedObject()
		{
			var node = GetNode("Relation");
			AssertEquals("node.Nodes", MapTreeNode.DummyNodeText, GetNodesText(node.Nodes));

			node.LoadNodesIfNotAlreadyLoaded();
			AssertMultilineASCIIEquals("node.Nodes", FullNodeList.Trim(), GetNodesText(node.Nodes));
		}

		public void TestPropertyLazyLoadsChildrenForChildCollection()
		{
			var node = GetNode("Collection");
			AssertEquals("node.Nodes", MapTreeNode.DummyNodeText, GetNodesText(node.Nodes));

			node.LoadNodesIfNotAlreadyLoaded();
			AssertMultilineASCIIEquals("node.Nodes", FullNodeList.Trim(), GetNodesText(node.Nodes));
		}

		public void TestPropertyLoadsNoChildrenForFieldProperty()
		{
			var node = GetNode("ZStringIsIn");
			AssertEquals("node.Nodes.Count", 0, node.Nodes.Count);

			node.LoadNodesIfNotAlreadyLoaded();
			AssertEquals("node.Nodes.Count", 0, node.Nodes.Count);
		}

		public void TestTextProperty()
		{
			var memberDescription = new PropertyDescription(GetType().GetProperty("TestStringProperty"), null);
			var node = GetNode(memberDescription);
			AssertEquals("node.Text", "TestStringProperty (ZString)", node.Text);
		}

		public void TestPropertyInfoCannotBeNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new MapTreeNode(null); });
		}

		public void TestClone()
		{
			var node = GetNode("Collection");
			node.LoadNodesIfNotAlreadyLoaded();
			node.Expand();
			AssertMultilineASCIIEquals("node.Nodes", FullNodeList.Trim(), GetNodesText(node.Nodes));

			var cloneNode = (MapTreeNode)node.Clone();
			AssertMultilineASCIIEquals("cloneNode.Nodes", FullNodeList.Trim(), GetNodesText(cloneNode.Nodes));
			AssertEquals("cloneNode.IsExpanded", node.IsExpanded, cloneNode.IsExpanded);
		}

		#region Implementation

		protected override void SetUp()
		{
			userControl = new MapTreeUserControl();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (userControl != null)
			{
				userControl.Dispose();
				userControl = null;
			}
		}

		MapTreeUserControl userControl;

		MapTreeNode GetNode(string property)
		{
			var memberDescription = new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty(property), null);
			return GetNode(memberDescription);
		}

		MapTreeNode GetNode(PropertyDescription memberDescription)
		{
			return new MapTreeNode(new MapTreeUserControl.MapTreeNotNode(userControl, null, memberDescription));
		}

		public ZString TestStringProperty
		{
			get;
			set;
		}

		string GetNodesText(TreeNodeCollection nodes)
		{
			var result = new ZStringBuilder();
			foreach (TreeNode node in nodes)
			{
				result.Append(node.Text);
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		const string FullNodeList = @"
DocDataProviderIsIn (OrgHeader)
DocDataProviderIzIn (OrgContact)
Relation (DocumentWrapperForTest)
ZStringIsIn (ZString)
ZStringIsIn1 (ZString)
ZStringIsIn2 (ZString)
Collection (DocumentWrapperCollectionForTest)
CollectionIsIn (ZString[])
CollectionWithCustomProperties (DocumentWrapperCollectionWithCustomPropertiesForTest)
DocDataProviderCollectionIsIn (OrgAddressCollection)
";
		#endregion
	}
}
