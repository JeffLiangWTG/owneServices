using System.Collections;
using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI.DocumentMenu;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CustomisationMenuItemMenusMakerTest : CustomisationMenusMakerTest<MenuItem>
	{
		protected override string GetText(MenuItem menuItem) => menuItem.Text;

		protected override IList GetMenuItems(Form form) => form.Menu.MenuItems;

		protected override void PerformClick(MenuItem menuItem) => menuItem.PerformClick();

		protected override ZDocumentsMenuItemHelper<MenuItem> GetNewHelper() => new ZDocumentsMenuItemMenuHelper();
	}
}
