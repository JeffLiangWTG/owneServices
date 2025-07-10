using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Forms.Internal
{
	public sealed class EmptySearchResults : ISearchResults
	{
		EmptySearchResults()
		{
		}

		public int Total => -1;
		public TreeNode Next() => null;
		public TreeNode Previous() => null;

		public static ISearchResults Instance { get; } = new EmptySearchResults();
	}
}
