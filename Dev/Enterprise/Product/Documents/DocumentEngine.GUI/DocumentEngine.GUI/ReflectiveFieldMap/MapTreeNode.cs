using System;
using System.Windows.Forms;
using Enterprise.DocumentEngine.ReflectiveFieldMap;

namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	public class MapTreeNode : TreeNode
	{
		public MapTreeNode(MapTreeUserControl.MapTreeNotNode notNode)
			: base()
		{
			if (notNode == null)
			{
				throw new ArgumentNullException(nameof(notNode));
			}

			this.notNode = notNode;
			this.Text = notNode.text;
			this.Name = notNode.name;
			notNode.mapTreeNode = this;

			if (notNode.canHaveChildren)
			{
				dummyNode = new TreeNode(DummyNodeText);
				Nodes.Add(dummyNode);
			}
		}

		public MemberDescription MemberDescription
		{
			get
			{
				return notNode.memberDescription;
			}
		}
		
		internal readonly MapTreeUserControl.MapTreeNotNode notNode;
		TreeNode dummyNode;

		internal const string DummyNodeText = "...";

		internal void LoadNodesIfNotAlreadyLoaded()
		{
			if (dummyNode != null)
			{
				dummyNode = null;
				Nodes.Clear();

				foreach (var child in notNode.Children)
				{
					Nodes.Add(new MapTreeNode(child));
				}
			}
		}

		public override object Clone()
		{
			var node = new MapTreeNode(notNode);
			if (dummyNode == null)
			{
				node.dummyNode = null;
				node.Nodes.Clear();
				foreach (MapTreeNode child in Nodes)
				{
					node.Nodes.Add((MapTreeNode)child.Clone());
				}
				if (IsExpanded)
				{
					node.Expand();
				}
			}
			return node;
		}
	}
}
