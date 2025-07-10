using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class CombinationCustomisationMenuItemMenusMakerTest : DocumentCustomisationMenusMakerTest
	{
		public void TestEnableCustomizedDocumentAndCustomizedFormsMenuItems()
		{
			using (var form = new TestForm())
			{
				var documentSupportable = new MockDocumentSupportable();
				var parent = DocumentMenuCustomisation.New(documentSupportable, null);
				var maker = new CombinationCustomisationMenuItemMenusMaker(form, parent, null, new ZDocumentsMenuItemMenuHelper());

				maker.Make(GetMenuItems(form));

				var expectedMenuItems = new string[]
				{
					"Customize (Documents)",
					"Customize (Forms)"
				};

				var currentMenuItems = GetMenuItems(form).OfType<MenuItem>().Select(GetText).ToArray();

				var customizeDocumentsMenu = GetMenuItems(form).OfType<MenuItem>().FirstOrDefault(x => x.Text == "Customize (Documents)");
				var customizeFormsMenu = GetMenuItems(form).OfType<MenuItem>().FirstOrDefault(x => x.Text == "Customize (Forms)");

				AssertNotNull("Customize (Documents) menu created", customizeDocumentsMenu);
				AssertNotNull("Customize (Froms) menu created", customizeFormsMenu);

				customizeFormsMenu.PerformClick();
				var customizeFormsForm = ZFormModaliser.LastFormShownForTest;

				AssertEquals("Visualizer customize form shows", "Enterprise.DocumentVisualizer.GUI.VisualizerMenuCustomisationForm", customizeFormsForm.GetType().FullName);
			}
		}
	}
}
