using System.Windows.Forms;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public class CodeDescriptionPairTreeNode : TreeNode, ICodeDescriptionPairTreeNode
	{
		public CodeDescriptionPairTreeNode(string text, CodeDescriptionPair codeDescPair)
		{
			this.Text = text;
			this.fCodeDescPair = codeDescPair;
		}

		public CodeDescriptionPair CodeDescPair
		{
			get { return fCodeDescPair; }
		}
		readonly CodeDescriptionPair fCodeDescPair;
	}
}
