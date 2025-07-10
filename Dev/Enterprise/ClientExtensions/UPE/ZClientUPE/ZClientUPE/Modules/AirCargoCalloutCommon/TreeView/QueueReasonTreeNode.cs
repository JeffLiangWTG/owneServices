using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public class QueueReasonTreeNode : CodeDescriptionPairListTreeNode, ICodeDescriptionPairTreeNode
	{
		public QueueReasonTreeNode(TreeView treeView, string text, QueueStatusCodeSet selectedQueueStatusCodes, string queueName, CodeDescriptionPair selectedReasonCodeDescPair)
			: base(treeView, text, selectedQueueStatusCodes, StatusCodeDescriptionPairList.GetStatusList(queueName, selectedReasonCodeDescPair.Code))
		{
			this.SelectedReasonCodeDescPair = selectedReasonCodeDescPair;
		}

		protected override bool AllowCheck
		{
			get { return true; }
		}

		#region ICodeDescriptionPairTreeNode

		public readonly CodeDescriptionPair SelectedReasonCodeDescPair;

		CodeDescriptionPair ICodeDescriptionPairTreeNode.CodeDescPair
		{
			get { return SelectedReasonCodeDescPair; }
		}

		#endregion
	}
}
