using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueListTreeNodeTest : TestCaseWithFactory
	{
		public void TestQueueNodesPopulated()
		{
			QueueListNode.Expand();
			AssertEquals("Queue names should be populated", true, QueueListNode.Nodes.Count > 1);
		}

		public void TestQueueReasonSubListNodesPopulated()
		{
			QueueListNode.Expand();
			ICodeDescriptionPairTreeNode queueNode = (ICodeDescriptionPairTreeNode)QueueListNode.Nodes[2];
			queueNode.Expand();
			AssertEquals("Queue reasons sub-list should populate", true, queueNode.Nodes.Count >= 1);
			ICodeDescriptionPairTreeNode queueReasonNode = (ICodeDescriptionPairTreeNode)queueNode.Nodes[0];
			QueueReasonCodeSet selectedQueueReasonCodes = SelectedQueueCodes.GetReasonCodeSet(queueNode.CodeDescPair.Code);
			AssertEquals("Should not yet contain the selected queue name for the test", false, SelectedQueueCodes.Contains(queueNode.CodeDescPair.Code));
			AssertEquals("Should not yet contain the selected queue reason for the test", false, selectedQueueReasonCodes.Contains(queueReasonNode.CodeDescPair.Code));
			queueReasonNode.Checked = true;
			AssertEquals("Should contain the selected queue name while the reason is selected", true, SelectedQueueCodes.Contains(queueNode.CodeDescPair.Code));
			AssertEquals("Should contain the selected queue reason", true, selectedQueueReasonCodes.Contains(queueReasonNode.CodeDescPair.Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			TreeView = new ZTreeView();
			QueueListNode = new QueueListTreeNode(TreeView, SelectedQueueCodes, new CommercialQueueCodeDescriptionPairList());
			TreeView.Nodes.Add(QueueListNode);
			TreeView.CreateControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TreeView.Dispose();
		}

		ZTreeView TreeView;
		QueueListTreeNode QueueListNode;
		readonly QueueCodeSet SelectedQueueCodes = new QueueCodeSet();
	}
}
