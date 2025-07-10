using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class LinkButtonTest : TransactionedTestCase
	{
		public void TestConstructor()
		{
			using (LinkButton button = new LinkButton("Apple Pie", ModuleIDs.CartageType, "Modify Apple Pie"))
			{
				AssertEquals("AutoSize", true, button.AutoSize);
				AssertEquals("Text", "Modify Apple Pie", button.Text);
				AssertEquals("ModuleID", ModuleIDs.CartageType, button.ModuleID);
			}
		}

		[RequiresSTA]
		public void TestOpenZFilterGridModule()
		{
			using (LinkButton button = new LinkButton("Chocolate Sundae", ModuleIDs.CartageType))
			{
				AssertNull("Precondition: Module should not be created yet.", button.LastCreatedModule);
				AssertNull("Precondition: No form should be opened yet.", ZFormModaliser.LastFormShownDialogForTest);
				button.PerformClick();
				AssertEquals("An EmbeddedModulePopup should be displayed.", typeof(EmbeddedModulePopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[RequiresSTA]
		public void TestOpenZPopupModule()
		{
			using (ZForm parentForm = new ZForm())
			{
				LinkButton button = new LinkButton("Strawberry Sundae", ModuleIDs.GLAccountFormat);
				parentForm.Controls.Add(button);
				parentForm.Show();

				AssertNull("Precondition: Module should not be created yet.", button.LastCreatedModule);
				button.PerformClick();
				ZController controller = ((IPopupModuleInternalsForTesting)button.LastCreatedModule).LastController;
				using (IZForm form = controller.LastShownForm)
				{
					AssertEquals("LastCreatedModule.LastController.LastShownForm.ControllerID", ControllerIDs.GLAccountFormat, form.ControllerID);
				}
			}
		}
	}
}
