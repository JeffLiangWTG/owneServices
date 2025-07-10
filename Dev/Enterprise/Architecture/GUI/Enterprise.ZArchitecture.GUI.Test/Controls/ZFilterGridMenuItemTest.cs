using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFilterGridMenuItemTest : TestCase
	{
		public void TestShowInToolbarIsTrueByDefault()
		{
			AssertEquals(true, new ZFilterGridMenuItem().ShowInToolbar);
		}
	}
}
