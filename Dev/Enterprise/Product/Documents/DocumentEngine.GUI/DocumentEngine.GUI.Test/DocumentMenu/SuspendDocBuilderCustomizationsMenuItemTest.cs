using Enterprise.DocumentEngine.DocBuilder;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class SuspendDocBuilderCustomizationsMenuItemTest : TestCase
	{
		public void TestChecked()
		{
			TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = false;
			using (var menuItem = new SuspendDocBuilderCustomizationsMenuItem())
			{
				AssertEquals("Checked", false, menuItem.Checked);

				menuItem.PerformClick();
				AssertEquals("Checked", true, menuItem.Checked);
				AssertEquals("TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations", true, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations);

				menuItem.PerformClick();
				AssertEquals("Checked", false, menuItem.Checked);
				AssertEquals("TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations", false, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations);

				menuItem.PerformClick();
				AssertEquals("Checked", true, menuItem.Checked);
				AssertEquals("TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations", true, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations);
			}
		}

		public void TestCheckedIgnoreUserCustomizedTemplatesAndConfigurationsStartsWithTrue()
		{
			TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = true;
			using (var menuItem = new SuspendDocBuilderCustomizationsMenuItem())
			{
				AssertEquals("Checked", true, menuItem.Checked);

				menuItem.PerformClick();
				AssertEquals("Checked", false, menuItem.Checked);
				AssertEquals("TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations", false, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations);

				menuItem.PerformClick();
				AssertEquals("Checked", true, menuItem.Checked);
				AssertEquals("TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations", true, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations);

				menuItem.PerformClick();
				AssertEquals("Checked", false, menuItem.Checked);
				AssertEquals("TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations", false, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations = false;
		}
	}
}
