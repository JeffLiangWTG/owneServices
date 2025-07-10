using System;
using System.Text;
using CargoWise.EntityFramework;

#if DEBUG
#endif

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// Summary description for TreeNode.
	/// </summary>
	public class TreeNode
	{
		public TreeNode()
		{
		}

		public TreeNode(TreeNode parent)
		{
			this.Parent = parent;
		}

		public TreeNode(string text)
		{
			this.Text = text;
		}

		public TreeNode(IFamilyMember element)
		{
			this.Text = element.ShortDescription;
			this.Tag = element;
		}

		#region Properties

		/// <summary>
		/// The text to describe the node
		/// </summary>
		protected string fText;
		public string Text
		{
			get { return fText; }
			set { fText = value; }
		}

		protected IFamilyMember fTag;
		/// <summary>
		/// The object this TreeNode encapsulates
		/// </summary>
		public IFamilyMember Tag
		{
			get { return fTag; }
			set { fTag = value; }
		}

		/// <summary>
		/// Is this node currently expanded
		/// </summary>
		protected bool fIsExpanded;
		public bool IsExpanded
		{
			get { return fIsExpanded; }
			set { fIsExpanded = value; }
		}

		protected bool fChildNodesLoaded;
		public bool ChildNodesLoaded
		{
			get { return fChildNodesLoaded; }
		}

		public bool IsLastChild
		{
			get
			{
				if (Parent != null)
				{
					return Parent.Nodes[Parent.Nodes.Count - 1].Equals(this);
				}
				else
				{
					return true;
				}
			}
		}

		/// <summary>
		/// The nodes contained beneath this node in the hierarchy
		/// </summary>
		protected TreeNodeCollection fNodes;
		public TreeNodeCollection Nodes
		{
			get
			{
				if (fNodes == null)
				{
					fNodes = new TreeNodeCollection(this);
				}
				return fNodes;
			}
		}

		#endregion Properties

		#region Helper Functions

		/// <summary>
		/// Find child or descendant of this node
		/// </summary>
		/// <param name="nodeName">The nodes text description</param>
		/// <param name="recurse">Find recursively</param>
		/// <returns></returns>
		public virtual TreeNode Find(string nodeName, bool findRecursively)
		{
			foreach (TreeNode child in Nodes)
			{
				if (child.Text.Equals(nodeName))
				{
					return child;
				}
				else if (findRecursively)
				{
					TreeNode target = child.Find(nodeName, true);
					if (target != null)
					{
						return target;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Find child of this node
		/// </summary>
		/// <param name="nodeName"></param>
		/// <returns></returns>
		public virtual TreeNode Find(string nodeName)
		{
			return Find(nodeName, false);
		}

		public virtual string ExpandedNodes
		{
			get
			{
				StringBuilder myExpandedNodes = new StringBuilder();

				if (this.IsExpanded)
				{
					myExpandedNodes.Append(this.Text);
					foreach (TreeNode child in Nodes)
					{
						string childsExpandedNodes = child.ExpandedNodes;
						if (childsExpandedNodes.Length > 0)
						{
							myExpandedNodes.Append(String.Format(":{0}", childsExpandedNodes));
						}
					}
				}
				return myExpandedNodes.ToString();
			}
			set
			{
				if (value.IndexOf(String.Format(":{0}:", this.Text)) >= 0)
				{
					this.IsExpanded = true;
					LoadChildNodes();
					foreach (TreeNode child in Nodes)
					{
						child.ExpandedNodes = value;
					}
				}
			}
		}

		#endregion Helper Functions

		#region Implementation

		public void CollapseAll()
		{
			this.IsExpanded = false;
			foreach (TreeNode child in Nodes)
			{
				child.IsExpanded = false;
			}
		}

		public void LoadChildNodes()
		{
			if (Tag != null && !fChildNodesLoaded)
			{
				if (fNodes == null)
				{
					fNodes = new TreeNodeCollection(this);
				}

				foreach (IFamilyMember element in Tag.Children)
				{
					fNodes.Add(new TreeNode(element));
				}
			}
			fChildNodesLoaded = true;
		}

		protected TreeNode fParent;
		public TreeNode Parent
		{
			get { return fParent; }
			set { fParent = value; }
		}

		public bool HasChildren
		{
			get { return ((Tag != null) && (Tag.HasChildren)); }
		}

		#endregion Implementation
	}
}
