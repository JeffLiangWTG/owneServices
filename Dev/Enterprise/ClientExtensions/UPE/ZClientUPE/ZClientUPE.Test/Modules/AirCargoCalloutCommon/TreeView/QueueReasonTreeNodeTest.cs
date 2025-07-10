using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class QueueReasonTreeNodeTest : TestCaseWithFactory
	{
		public void TestCheckingQueueStatusUpdatesCodeSets()
		{
			QueueReasonNode.Expand();
			ICodeDescriptionPairTreeNode queueNamesNode = (ICodeDescriptionPairTreeNode)QueueReasonNode.Nodes[0];
			AssertEquals("Should not yet contain the selected queue status for the test", false, SelectedQueueStatusCodes.Contains(queueNamesNode.CodeDescPair.Code));
			queueNamesNode.Checked = true;
			AssertEquals("Queue status should be selected", true, SelectedQueueStatusCodes.Contains(queueNamesNode.CodeDescPair.Code));
		}

		public void TestAllowCheck()
		{
			QueueReasonNode.Checked = true;
			AssertEquals("Should be able to check this tree node", true, QueueReasonNode.Checked);
			QueueReasonNode.Checked = false;
			AssertEquals("Should be able to uncheck this tree node", false, QueueReasonNode.Checked);
		}

		public void TestDontCascadeCheck()
		{
			QueueReasonNode.Checked = true;
			AssertEquals("Should not cascade check queue status child nodes", false, QueueReasonNode.Nodes[0].Checked);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TreeView = new ZTreeView();
			CodeDescriptionPair selectedQueueReason = new CodeDescriptionPair(ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, "description");
			QueueReasonNode = new QueueReasonTreeNode(TreeView, "Reasons", SelectedQueueStatusCodes, DefaultQueueCodeDescriptionPairList.Codes.EIR, selectedQueueReason);
			TreeView.Nodes.Add(QueueReasonNode);
			TreeView.CreateControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TreeView.Dispose();
		}

		ZTreeView TreeView;
		QueueReasonTreeNode QueueReasonNode;
		readonly QueueStatusCodeSet SelectedQueueStatusCodes = new QueueStatusCodeSet("REA");
	}
}
