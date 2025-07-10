using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebThemeSelectorControl))]
	sealed class WebThemeSelectorControlTest : RegistryZUserControlTestCase
	{
		public void TestFormHasChanges_CreateTheme()
		{
			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebThemeSelectorControl())
			{
				control.SetDataBinding(GetNewBusinessEntity(), null);
				testForm.Controls.Add(control);
				testForm.Show();

				Assert(!testForm.ImportantMethodWasCalled);
				control.CreateTheme();
				Assert(testForm.ImportantMethodWasCalled);
			}
		}

		public void TestFormHasChanges_CopyTheme()
		{
			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebThemeSelectorControl())
			{
				var collection = GetNewBusinessEntity();
				collection.Add(new WebThemeCustomObject { ThemeName = "TestTheme" });
				control.SetDataBinding(collection, null);

				testForm.Controls.Add(control);
				testForm.Show();

				Assert(!testForm.ImportantMethodWasCalled);
				control.CopyTheme();
				Assert(testForm.ImportantMethodWasCalled);
			}
		}

		public void TestFormHasChanges_DeleteTheme()
		{
			using (var testForm = new RegistryFormTest.DummyRegistryForm())
			using (var control = new WebThemeSelectorControl())
			{
				var collection = GetNewBusinessEntity();
				collection.Add(new WebThemeCustomObject { ThemeName = "TestTheme" });
				control.SetDataBinding(collection, null);

				testForm.Controls.Add(control);
				testForm.Show();

				Assert(!testForm.ImportantMethodWasCalled);
				control.DeleteTheme();
				Assert(testForm.ImportantMethodWasCalled);
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new WebThemeCustomObjectCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly;
	}
}
