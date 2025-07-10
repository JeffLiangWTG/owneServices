using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public interface ITreeViewSearcher
	{
		ISearchResults Search(TreeView treeView, string criteria, StringComparison comparison);
	}
}
