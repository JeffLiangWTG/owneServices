using Enterprise.DocumentEngine.DocBuilder;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class SuspendTemplateCachingMenuItemTest : TestCase
	{
		public void TestChecked()
		{
			TemplateCache.SuspendTemplateCache = false;
			using (var menuItem = new SuspendTemplateCachingMenuItem())
			{
				AssertEquals("Checked", false, menuItem.Checked);

				menuItem.PerformClick();
				AssertEquals("Checked", true, menuItem.Checked);
				AssertEquals("TemplateCache.SuspendTemplateCache", true, TemplateCache.SuspendTemplateCache);

				menuItem.PerformClick();
				AssertEquals("Checked", false, menuItem.Checked);
				AssertEquals("TemplateCache.SuspendTemplateCache", false, TemplateCache.SuspendTemplateCache);

				menuItem.PerformClick();
				AssertEquals("Checked", true, menuItem.Checked);
				AssertEquals("TemplateCache.SuspendTemplateCache", true, TemplateCache.SuspendTemplateCache);
			}
		}

		public void TestCheckedIgnoreUserCustomizedTemplatesAndConfigurationsStartsWithTrue()
		{
			TemplateCache.SuspendTemplateCache = true;
			using (var menuItem = new SuspendTemplateCachingMenuItem())
			{
				AssertEquals("Checked", true, menuItem.Checked);

				menuItem.PerformClick();
				AssertEquals("Checked", false, menuItem.Checked);
				AssertEquals("TemplateCache.SuspendTemplateCache", false, TemplateCache.SuspendTemplateCache);

				menuItem.PerformClick();
				AssertEquals("Checked", true, menuItem.Checked);
				AssertEquals("TemplateCache.SuspendTemplateCache", true, TemplateCache.SuspendTemplateCache);

				menuItem.PerformClick();
				AssertEquals("Checked", false, menuItem.Checked);
				AssertEquals("TemplateCache.SuspendTemplateCache", false, TemplateCache.SuspendTemplateCache);
			}
		}
	}
}
