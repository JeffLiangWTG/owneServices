using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public class QueueTreeNode : CodeDescriptionPairListTreeNode, ICodeDescriptionPairTreeNode
	{
		public QueueTreeNode(TreeView treeView, string text, QueueReasonCodeSet selectedQueueReasonCodes, CodeDescriptionPair selectedQueueCodeDescPair, DefaultQueueCodeDescriptionPairList queueList)
			: base(treeView, text, selectedQueueReasonCodes, ReasonCodeDescriptionPairList.GetReasonList(queueList, selectedQueueCodeDescPair.Code))
		{
			this.SelectedQueueCodeDescPair = selectedQueueCodeDescPair;
			this.SelectedQueueReasonCodes = selectedQueueReasonCodes;
			this.QueueReasonList = (ReasonCodeDescriptionPairList)CodeDescPairList;
		}

		public readonly QueueReasonCodeSet SelectedQueueReasonCodes;
		public readonly ReasonCodeDescriptionPairList QueueReasonList;

		protected override ICodeDescriptionPairTreeNode NewChildNode(CodeDescriptionPair selectedQueueReasonCodeDescPair)
		{
			QueueStatusCodeSet selectedQueueStatusCodes = SelectedQueueReasonCodes.GetStatusCodeSet(selectedQueueReasonCodeDescPair.Code);
			QueueReasonTreeNode result = new QueueReasonTreeNode(TreeView, selectedQueueReasonCodeDescPair.Description, selectedQueueStatusCodes, SelectedQueueCodeDescPair.Code, selectedQueueReasonCodeDescPair);
			return result;
		}

		protected override bool AllowCheck
		{
			get { return true; }
		}

		#region ICodeDescriptionPairTreeNode

		public readonly CodeDescriptionPair SelectedQueueCodeDescPair;

		CodeDescriptionPair ICodeDescriptionPairTreeNode.CodeDescPair
		{
			get { return SelectedQueueCodeDescPair; }
		}

		#endregion
	}
}
