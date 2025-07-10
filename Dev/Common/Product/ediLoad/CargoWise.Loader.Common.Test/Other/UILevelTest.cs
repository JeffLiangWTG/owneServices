using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class UILevelTest : TestCase
	{
		public void TestDefaultValue()
		{
			UILevel uiLevel = new UILevel();
			AssertEquals("uiLevel", UILevel.Normal, uiLevel);
		}
	}
}