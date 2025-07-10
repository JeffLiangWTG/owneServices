using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.WCB.DaimlerChrysler.GUI;
using Enterprise.Customs.AU.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.Module.Testing
{
	[TestedType(typeof(DeclarationModuleOverride))]
	sealed class DeclarationModuleOverrideTest : ZModuleBasherTest
	{
		public void TestImportFromDaimlerChryslerFile()
		{
			using (JobDeclarationModule module = (JobDeclarationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				MenuItem item = GetMenuItem(module, "Import From Mercedes File");
				item.PerformClick();
				AssertNotNull("Import form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Import form type", typeof(DCDataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				item = GetMenuItem(module, "Import From Chrysler File");
				item.PerformClick();
				AssertNotNull("Import form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Import form type", typeof(DCDataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.JobDeclaration;
		static MenuItem GetMenuItem(JobDeclarationModule module, string menuName)
		{
			MenuItem result = null;
			foreach (MenuItem actionItem in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
			{
				if (actionItem.Text == "D&ata Transfer")
				{
					foreach (MenuItem item in actionItem.MenuItems)
					{
						if (item.Text == menuName)
						{
							result = item;
							break;
						}
					}

					break;
				}
			}

			AssertNotNull("'" + menuName + "' should exist.", result);
			return result;
		}
	}
}
