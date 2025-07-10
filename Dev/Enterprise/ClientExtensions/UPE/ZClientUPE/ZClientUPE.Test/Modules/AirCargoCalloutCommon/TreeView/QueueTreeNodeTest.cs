using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueTreeNodeTest : TestCaseWithFactory
	{
		public void TestCheckingQueueReasonUpdatesCodeSets()
		{
			QueueNode.Expand();
			ICodeDescriptionPairTreeNode queueReasonNode = (ICodeDescriptionPairTreeNode)QueueNode.Nodes[0];
			AssertEquals("Should not yet contain the selected queue status for the test", false, SelectedQueueReasonCodes.Contains(queueReasonNode.CodeDescPair.Code));
			queueReasonNode.Checked = true;
			AssertEquals("Queue reason should be selected", true, SelectedQueueReasonCodes.Contains(queueReasonNode.CodeDescPair.Code));
		}

		public void TestAllowCheck()
		{
			QueueNode.Checked = true;
			AssertEquals("Should be able to check this tree node", true, QueueNode.Checked);
			QueueNode.Checked = false;
			AssertEquals("Should be able to uncheck this tree node", false, QueueNode.Checked);
		}

		public void TestDontCascadeCheck()
		{
			QueueNode.Checked = true;
			AssertEquals("Should not cascade check queue reason child nodes", false, QueueNode.Nodes[0].Checked);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TreeView = new ZTreeView();
			CodeDescriptionPair selectedQueueCodeDescPair = new CodeDescriptionPair(DefaultQueueCodeDescriptionPairList.Codes.Hold, "description");
			QueueNode = new QueueTreeNode(TreeView, "A Queue", SelectedQueueReasonCodes, selectedQueueCodeDescPair, new CommercialQueueCodeDescriptionPairList());
			TreeView.Nodes.Add(QueueNode);
			TreeView.CreateControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TreeView.Dispose();
		}

		ZTreeView TreeView;
		QueueTreeNode QueueNode;
		readonly QueueReasonCodeSet SelectedQueueReasonCodes = new QueueReasonCodeSet("QUE");
	}
}
