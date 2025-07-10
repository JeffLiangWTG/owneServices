using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class LayoutGroupBoxTabControlForTest : AutoLayoutGroupBoxTabControl
	{
		public bool ContainsPage(string pageName)
		{
			bool result = false;
			foreach (ZTabPage page in TabPages)
			{
				if (page.Name.ToUpper() == pageName.ToUpper())
				{
					result = true;
				}
			}
			return result;
		}

		public ZTabPage GetPage(string pageName)
		{
			ZTabPage result = null;
			foreach (ZTabPage page in TabPages)
			{
				if (page.Name.ToUpper() == pageName.ToUpper())
				{
					result = page;
				}
			}
			return result;
		}
	}
}
