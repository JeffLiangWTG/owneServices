using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.OIA.Module.Testing
{
	public class OIAGLJournalModuleTest : TestCaseWithFactory
	{
		public void TestOIAGLJournalModuleConstructor()
		{
			bool result = false;
			using (OIAGLJournalModule module = new OIAGLJournalModule())
			{
				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					foreach (MenuItem item in menuItem.MenuItems)
					{
						if (item.Text.EndsWith("Export GL Transactions in OIA CSV Format"))
						{
							result = true;
						}
					}
				}
			}

			Assert(result);
		}
	}
}
