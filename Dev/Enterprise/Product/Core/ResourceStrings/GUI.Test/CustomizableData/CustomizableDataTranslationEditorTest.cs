using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	sealed class CustomizableDataTranslationEditorTest : TestCaseWithFactory
	{
		public void TestSecurity()
		{
			var bizO = Factory.New<TranslatableDataFieldTestCase.DummyWithTranslatable>();
			bizO.ReadOnly = true;

			using (var form = new ZForm(bizO))
			{
				var translatableTextControl = new ZTranslatableTextControl();
				translatableTextControl.SetBindingMember("Z0_Description");
				form.Controls.Add(translatableTextControl);
				form.Show();

				CustomizableDataTranslationForm formCreated = null;
				var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args) { formCreated = sender as CustomizableDataTranslationForm; });
				try
				{
					ZForm.FormCreated += formCreatedHandler;

					translatableTextControl.OnLanguageClick();
					AssertNotNull(formCreated);
					var page = formCreated.BusinessEntity as CustomizableDataTranslationPage;
					AssertEquals("page.ReadOnly", false, page.ReadOnly);
					AssertEquals("page.All[0].ReadOnly", false, page.All[0].ReadOnly);
					formCreated.Dispose();
					formCreated = null;

					Env.Security.System.IsAllowed = false;
					translatableTextControl.OnLanguageClick();
					AssertNotNull(formCreated);
					page = formCreated.BusinessEntity as CustomizableDataTranslationPage;
					AssertEquals("page.ReadOnly", true, page.ReadOnly);
					AssertEquals("page.All[0].ReadOnly", true, page.All[0].ReadOnly);
					formCreated.Dispose();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}
			}
		}

		protected override void SetUp()
		{
			testHelper = new CustomizableDataTestHelper();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testHelper.Dispose();
			base.TearDown();
		}

		CustomizableDataTestHelper testHelper;
	}
}
