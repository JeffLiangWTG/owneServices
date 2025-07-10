using System.Windows.Forms;
using Enterprise.Accounting.GUI.XmlExport;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	public class ELGModuleTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)")]
		public void TestELGSageTransactionsExport_Click()
		{
			MenuItem exportMenuItem = null;
			using (ELGExportModuleStrip module = new ELGExportModuleStrip())
			{
				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					foreach (MenuItem item in menuItem.MenuItems)
					{
						if (item.Text.EndsWith("Transactions in Sage Format"))
						{
							exportMenuItem = item;
							break;
						}
					}
				}

				AssertNotNull(exportMenuItem);
				exportMenuItem.PerformClick();
			}

			AssertEquals(typeof(FlatFileXmlExportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}
}
