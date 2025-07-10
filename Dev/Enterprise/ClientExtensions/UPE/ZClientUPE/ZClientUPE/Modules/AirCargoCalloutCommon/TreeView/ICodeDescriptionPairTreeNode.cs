using System.Windows.Forms;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Module
{
	public interface ICodeDescriptionPairTreeNode
	{
		CodeDescriptionPair CodeDescPair { get; }

		bool Checked { get; set; }
		void Expand();
		TreeNodeCollection Nodes { get; }
	}
}
