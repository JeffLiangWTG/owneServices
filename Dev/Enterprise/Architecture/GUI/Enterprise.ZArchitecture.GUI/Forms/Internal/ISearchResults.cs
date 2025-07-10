using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public interface ISearchResults
	{
		int Total { get; } // -1 when recursive

		TreeNode Next();
		TreeNode Previous();
	}
}