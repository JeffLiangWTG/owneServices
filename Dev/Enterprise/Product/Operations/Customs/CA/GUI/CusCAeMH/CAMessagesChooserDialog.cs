using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class CAMessagesChooserDialog : MessagesChooserDialog
	{
		public CAMessagesChooserDialog(MessageChooserNonPersistent cAChooser) : base(cAChooser)
		{
		}

		public CAMessageChooserNonPersistent CAChooser
		{
			get { return (CAMessageChooserNonPersistent)base.Chooser; }
		}
		public ZTreeView MessagesTreeView = new ZTreeView();

		protected override void InitializeMessageListControl()
		{
			this.MessagesTreeView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.MessagesTreeView.CheckBoxes = true;
			this.MessagesTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.MessagesTreeView.Name = "MessagesTreeView";
			this.MessagesTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 169);
			this.MessagesTreeView.TabIndex = 0;
			this.MessagesTreeView.AfterCheck += MessagesTreeView_AfterCheck;
			this.MessagesTreeView.QueryNewTreeNode += MessagesTreeView_QueryNewTreeNode;
			this.Controls.Add(this.MessagesTreeView);
			this.Controls.SetChildIndex(this.MessagesTreeView, 0);
		}

		#region MessagesTreeView Events

		void MessagesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (e.Action != TreeViewAction.Unknown)
			{
				CheckAllChildNodes(e.Node, e.Node.Checked, true);
				CheckParentNode(e.Node, e.Node.Checked);
				if (e.Node.Text == CaptionStringAll)
				{
					CheckAllChildNodes(e.Node, e.Node.Checked, true);
				}
			}

			if (e.Node.Parent != null)
			{
				var messageManagersToSend = this.CAChooser.MessageManagersToSend;
				if (e.Node is CloseReportTreeNode || e.Node is HouseTreeNode)
				{
					var singleMessageManger = (e.Node as HouseTreeNode).MessageManager;
					if (messageManagersToSend.ContainsKey(singleMessageManger))
					{
						messageManagersToSend[singleMessageManger] = e.Node.Checked;
					}
				}
				else if (e.Node is CloseReportHouseTreeNode)
				{
					(e.Node as CloseReportHouseTreeNode).UpdateInstructionIsShouldSend(e.Node.Checked);
				}
			}
		}

		internal void MessagesTreeView_QueryNewTreeNode(object sender, QueryNewTreeNodeEventArgs e)
		{
			var cAChooser = e.Item as CAMessageChooserNonPersistent;
			var rootNode = new TreeNode(CaptionStringAll);
			AddMessageManagersToTreeNode(rootNode, cAChooser);
			rootNode.ExpandAll();
			e.NewTreeNode = rootNode;
		}

		void AddMessageManagersToTreeNode(TreeNode node, CAMessageChooserNonPersistent cAChooser)
		{
			if (cAChooser != null)
			{
				foreach (var singleMessageManager in cAChooser.Managers)
				{
					if (singleMessageManager is ACIForwarderCloseMessageManager)
					{
						node.Nodes.Add(new CloseReportTreeNode(singleMessageManager as ACIForwarderCloseMessageManager));
					}
					else
					{
						node.Nodes.Add(new HouseTreeNode(singleMessageManager));
					}
				}
			}
		}

		internal void SelectAllInternal() => SelectAll();
		protected override void SelectAll()
		{
			CheckAll(true);
		}

		internal void DeSelectAllInternal() => DeSelectAll();
		protected override void DeSelectAll()
		{
			CheckAll(false);
		}

		void CheckAll(ZBool isCheck)
		{
			foreach (TreeNode node in this.MessagesTreeView.Nodes)
			{
				node.Checked = isCheck;
				CheckAllChildNodes(node, isCheck);
			}
		}

		void CheckAllChildNodes(TreeNode treeNode, ZBool isCheck, bool isOnlyCheckEligible = false)
		{
			foreach (TreeNode node in treeNode.Nodes)
			{
				node.Checked = isCheck;
				if (node.Nodes.Count > 0)
				{
					CheckAllChildNodes(node, isCheck, isOnlyCheckEligible);
				}
			}
		}

		void CheckParentNode(TreeNode treeNode, ZBool isCheck)
		{
			if (treeNode.Parent != null)
			{
				if (treeNode.Checked)
				{
					treeNode.Parent.Checked = true;
				}
				else
				{
					var count = 0;
					foreach (TreeNode node in treeNode.Parent.Nodes)
					{
						if (node.Checked)
						{
							count++;
						}
					}
					if (count == 0)
					{
						treeNode.Parent.Checked = false;
					}
				}
				CheckParentNode(treeNode.Parent, isCheck);
			}
		}

		#endregion

		#region Constants

		public static ZString CaptionStringAll
		{
			get { return Res.GetString("597c298d-ba96-4bcc-b662-45f15f65a4d0", "All"); }
		}

		#endregion
	}
}
