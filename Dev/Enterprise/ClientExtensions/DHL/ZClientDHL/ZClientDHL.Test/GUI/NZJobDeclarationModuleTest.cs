using System;
using System.Windows.Forms;
using Enterprise.Customs.NZ.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.DHL.GUI
{
	[TestedType(typeof(NZJobDeclarationModule))]
	public class NZJobDeclarationModuleTest : Enterprise.Customs.NZ.Module.Declaration.FormalEntry.Testing.NZJobDeclarationModuleTest
	{
		protected override string CountryCode
		{
			get
			{
				return Enterprise.Core.Constants.CountryCodes.NewZealand;
			}
		}

		public void TestCISSAPTransactionsExport_Click()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DHL");
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleID))
			{
				System.Windows.Forms.Menu.MenuItemCollection menuItemCollection = ((NZJobDeclarationModule)module).FormActionMenu.FindByText("&Actions").MenuItems;
				MenuItem menuItem = null;
				foreach (MenuItem item in menuItemCollection)
				{
					if (item.Text == NZJobDeclarationModule.FlightBulkUpdateMenuItem)
					{
						menuItem = item;
						break;
					}
				}

				AssertNotNull(menuItem);
				menuItem.PerformClick();
			}

			AssertEquals("Type expected", typeof(FlightBulkUpdateForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}
}
