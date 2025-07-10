using Aga.Controls.Tree.NodeControls;
using NUnit.Framework;

namespace Aga.Controls.Testing
{
	public class ResourceHelperTest : TestCase
	{
		public void TestLoadingIcon_IsAccessible()
		{
			_ = ResourceHelper.LoadingIcon;
			_ = ResourceHelper.DVSplitCursor;

			AssertNotNull(nameof(ExpandingIcon),
				new ExpandingIcon());
		}
	}
}
