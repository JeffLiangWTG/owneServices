using System;
using System.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// Summary description for TreeNodeCollection.
	/// </summary>
	public class TreeNodeCollection : CollectionBase
	{
		#region Constructor

		internal TreeNode Owner;

		public TreeNodeCollection()
		{
		}

		public TreeNodeCollection(TreeNode owner) : base()
		{
			this.Owner = owner;
		}

		#endregion Constructor

		#region CollectionBase Members
		public virtual TreeNode Add(string text)
		{
			TreeNode newNode = new TreeNode(text);
			Add(newNode);
			return newNode;
		}

		public virtual int Add(TreeNode node)
		{
			if (node == null)
			{
				throw new ArgumentNullException(nameof(node), "Node");
			}
			node.Parent = Owner;
			List.Add(node);
			return List.Count;
		}

		public virtual int Add(IFamilyMember member)
		{
			TreeNode node = new TreeNode(member);
			return Add(node);
		}

		public virtual int Add(IFamilyMember[] members)
		{
			foreach (IFamilyMember element in members)
			{
				Add(element);
			}
			return List.Count;
		}

		public TreeNode this[int index]
		{
			get
			{
				return (TreeNode)this.List[index];
			}
			set
			{
				this.List[index] = value;
				value.Parent = Owner;
				value.IsExpanded = false;
			}
		}

		#endregion CollectionBase Members

		#region Find 

		public TreeNode Find(string nodeName, bool findRecursively)
		{
			foreach (TreeNode node in List)
			{
				if (node.Text.Equals(nodeName))
				{
					return node;
				}
				else if (findRecursively)
				{
					TreeNode target = node.Find(nodeName, findRecursively);
					if (target != null)
					{
						return target;
					}
				}
			}
			return null;
		}

		public TreeNode Find(string nodeName)
		{
			return Find(nodeName, false);
		}

		public TreeNode Find(IFamilyMember node)
		{
			foreach (TreeNode nodeInList in List)
			{
				if (nodeInList.Tag != null && nodeInList.Tag.Equals(node))
				{
					return nodeInList;
				}
			}
			return null;
		}

		#endregion Find

		#region ExpandPathToTarget

		public void ExpandPathToTarget(string[] path)
		{
			TreeNodeCollection targetNodes = this;

			for (int i = 0; i < path.Length; i++)
			{
				int index = Int32.Parse(path[i]);
				if (index >= 0 && index < targetNodes.Count)
				{
					targetNodes[index].LoadChildNodes();
					targetNodes[index].IsExpanded = true;
					targetNodes = targetNodes[index].Nodes;
				}
				else
				{
					return;
				}
			}
		}

		public IFamilyMember GetItemAtPath(string[] path)
		{
			TreeNodeCollection targetNodes = this;

			for (int i = 0; i < path.Length; i++)
			{
				int index = Int32.Parse(path[i]);
				if (index >= 0 && index < targetNodes.Count)
				{
					if (i < (path.Length - 1))
					{
						targetNodes[index].LoadChildNodes();
						targetNodes[index].IsExpanded = true;
						targetNodes = targetNodes[index].Nodes;
					}
					else
					{
						return targetNodes[index].Tag;
					}
				}
				else
				{
					return null;
				}
			}
			return null;
		}

		public TreeNode ExpandHierarchy(IFamilyMember[] hierarchy)
		{
			TreeNodeCollection targetNodes = this;
			TreeNode targetNode = null;

			for (int i = hierarchy.Length - 1; i >= 0; i--)
			{
				IFamilyMember node = hierarchy[i];
				targetNode = targetNodes.Find(node);
				if (targetNode == null)
				{
					return targetNode;
				}
				else
				{
					targetNode.LoadChildNodes();
					targetNode.IsExpanded = true;
					targetNodes = targetNode.Nodes;
				}
			}
			return targetNode;
		}

		#endregion ExpandPathToTarget
	}
}
