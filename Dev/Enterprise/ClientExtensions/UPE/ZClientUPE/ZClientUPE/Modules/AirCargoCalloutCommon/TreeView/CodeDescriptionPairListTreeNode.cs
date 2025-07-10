using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public class CodeDescriptionPairListTreeNode : TreeNode, IDisposable
	{
		public CodeDescriptionPairListTreeNode(TreeView treeView, string text, CodeSet selectedCodes, CodeDescriptionPairList codeDescPairList)
		{
			this.Text = text;
			this.CodeDescPairList = codeDescPairList;
			this.SelectedCodes = selectedCodes;

			treeView.AfterExpand += new TreeViewEventHandler(OnTreeView_AfterExpand);
			treeView.AfterCheck += new TreeViewEventHandler(OnTreeView_AfterCheck);
			selectedCodes.Changed += new EventHandler(OnSelectedCodes_Changed);

			if (codeDescPairList.Count > 0)
			{
				Nodes.Add(new TreeNode("<placeholder required to show +>"));
			}
		}

		public readonly CodeSet SelectedCodes;
		public readonly CodeDescriptionPairList CodeDescPairList;

		protected virtual bool AllowCheck
		{
			get { return false; }
		}

		protected virtual bool CascadeCheck
		{
			get { return false; }
		}

		protected virtual ICodeDescriptionPairTreeNode NewChildNode(CodeDescriptionPair codeDescPair)
		{
			return new CodeDescriptionPairTreeNode(codeDescPair.Description, codeDescPair);
		}

		protected virtual void PopulateChildNodesCore()
		{
			foreach (CodeDescriptionPair codeDescPair in CodeDescPairList)
			{
				ICodeDescriptionPairTreeNode node = NewChildNode(codeDescPair);
				Nodes.Add((TreeNode)node);
				node.Checked = SelectedCodes.Contains(codeDescPair.Code);
			}
		}

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			TreeView.AfterExpand -= new TreeViewEventHandler(OnTreeView_AfterExpand);
			TreeView.AfterCheck -= new TreeViewEventHandler(OnTreeView_AfterCheck);
			SelectedCodes.Changed -= new EventHandler(OnSelectedCodes_Changed);

			foreach (TreeNode node in Nodes)
			{
				IDisposable disposable = node as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		#endregion

		#region Implementation

		#region Populating Child Nodes

		void OnTreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			if (e.Node == this)
			{
				PopulateChildNodesIfRequired();
			}
		}

		void PopulateChildNodesIfRequired()
		{
			if (!IsChildNodesPopulated)
			{
				using (SuspendAfterCheckEvent.Suspend(this))
				{
					IsChildNodesPopulated = true;
					Nodes.Clear();
					PopulateChildNodesCore();
				}
			}
		}
		bool IsChildNodesPopulated;

		#endregion

		#region AfterCheck Event

		void OnTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (SuspendAfterCheckEventCount == 0)
			{
				using (SuspendAfterCheckEvent.Suspend(this))
				{
					if (e.Node == this)
					{
						HandleAfterCheckOfThisNode();
					}
				}
			}
			bool isChildNode = Nodes.Contains(e.Node);
			if (isChildNode)
			{
				UpdateSelectedCodesFromCheckedChildNode(e.Node as ICodeDescriptionPairTreeNode);
			}
		}

		void HandleAfterCheckOfThisNode()
		{
			bool hasChildNodes = Nodes.Count > 0;
			if (Checked && !HasAnyChildNodesChecked && hasChildNodes && !AllowCheck)
			{
				Checked = false;
			}
			else if (!Checked)
			{
				UncheckAllChildNodes();
			}
			else if (Checked && CascadeCheck)
			{
				CheckAllChildNodes();
			}
		}

		void UncheckAllChildNodes()
		{
			foreach (TreeNode node in Nodes)
			{
				node.Checked = false;
			}
			Checked = false;
			SelectedCodes.Clear();
		}

		void CheckAllChildNodes()
		{
			if (!IsCheckAllChildNodesSuspended)
			{
				PopulateChildNodesIfRequired();
				foreach (TreeNode node in Nodes)
				{
					node.Checked = Checked;
				}
			}
		}
		bool IsCheckAllChildNodesSuspended;

		#endregion

		#region Updating selected codes from checked nodes

		void UpdateSelectedCodesFromCheckedChildNode(ICodeDescriptionPairTreeNode childNode)
		{
			if (childNode != null)
			{
				bool isSeparatorNodeChecked = string.IsNullOrEmpty(childNode.CodeDescPair.Code) && childNode.Checked;
				if (isSeparatorNodeChecked)
				{
					childNode.Checked = false;
				}
				else if (childNode.Checked)
				{
					SelectedCodes.Add(childNode.CodeDescPair.Code);
				}
				else
				{
					SelectedCodes.Remove(childNode.CodeDescPair.Code);
				}
				if (!SelectedCodes.IsEmpty)
				{
					IsCheckAllChildNodesSuspended = true;
					Checked = true;
					IsCheckAllChildNodesSuspended = false;
				}
			}
		}

		#endregion

		#region Updating checked nodes from selected codes

		void OnSelectedCodes_Changed(object sender, EventArgs e)
		{
			using (SuspendAfterCheckEvent.Suspend(this))
			{
				UpdateCheckedChildNodesFromSelectedCodes();
			}
		}

		void UpdateCheckedChildNodesFromSelectedCodes()
		{
			PopulateChildNodesIfRequired();
			foreach (TreeNode node in Nodes)
			{
				ICodeDescriptionPairTreeNode codeDescPairNode = node as ICodeDescriptionPairTreeNode;
				if (codeDescPairNode != null)
				{
					node.Checked = SelectedCodes.Contains(codeDescPairNode.CodeDescPair.Code);
				}
			}
			Checked = HasAnyChildNodesChecked;
		}

		#endregion

		#region SuspendAfterCheckEvent

		class SuspendAfterCheckEvent : IDisposable
		{
			public SuspendAfterCheckEvent(CodeDescriptionPairListTreeNode treeNode)
			{
				treeNode.SuspendAfterCheckEventCount++;
				this.TreeNode = treeNode;
			}

			public static SuspendAfterCheckEvent Suspend(CodeDescriptionPairListTreeNode treeNode)
			{
				return new SuspendAfterCheckEvent(treeNode);
			}

			readonly CodeDescriptionPairListTreeNode TreeNode;

			void IDisposable.Dispose()
			{
				TreeNode.SuspendAfterCheckEventCount--;
			}
		}
		int SuspendAfterCheckEventCount;

		#endregion

		bool HasAnyChildNodesChecked
		{
			get
			{
				foreach (TreeNode node in Nodes)
				{
					if (node.Checked)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion
	}
}
