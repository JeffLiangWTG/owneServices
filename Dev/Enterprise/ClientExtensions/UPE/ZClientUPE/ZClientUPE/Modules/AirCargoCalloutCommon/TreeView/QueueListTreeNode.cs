using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public class QueueListTreeNode : CodeDescriptionPairListTreeNode
	{
		public QueueListTreeNode(TreeView treeView, QueueCodeSet selectedQueueCodes, DefaultQueueCodeDescriptionPairList queueList)
			: base(treeView, "Queues", selectedQueueCodes, queueList)
		{
			this.SelectedQueueCodes = selectedQueueCodes;
			this.QueueList = queueList;
		}

		public readonly QueueCodeSet SelectedQueueCodes;
		public readonly DefaultQueueCodeDescriptionPairList QueueList;

		protected override ICodeDescriptionPairTreeNode NewChildNode(CodeDescriptionPair selectedQueueCodeDescPair)
		{
			string queueName = selectedQueueCodeDescPair.Code;
			QueueReasonCodeSet reasonCodeSet = SelectedQueueCodes.GetReasonCodeSet(queueName);
			return new QueueTreeNode(TreeView, selectedQueueCodeDescPair.Description, reasonCodeSet, selectedQueueCodeDescPair, QueueList);
		}
	}
}
