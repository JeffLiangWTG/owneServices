using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ImportClassificationModule))]
	sealed class ImportClassificationModuleTest : Customs.Module.Testing.ImportClassificationModuleTest
	{
		public void TestGetNewFilterControl()
		{
			using (ImportClassificationModuleForTest module = new ImportClassificationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AUImportClassificationFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (ImportClassificationModuleForTest module = new ImportClassificationModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is ImportClassificationCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (ImportClassificationModuleForTest module = new ImportClassificationModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AUImportClassificationFilterBusinessObject);
			}
		}

		public void TestCSVImportAndBulkUpdateMenuItemsHaveBeenAdded()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(TestImportClassificationModule.EmbeddedControl);
				form.Show();
				MenuItem result1 = null;
				MenuItem result2 = null;
				foreach (MenuItem item in TestImportClassificationModule.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems)
				{
					if (item.Text == "Bulk Lookup Change")
					{
						result2 = item;
					}

					if (item.Text == "D&ata Transfer")
					{
						foreach (MenuItem subItem in item.MenuItems)
						{
							if (subItem.Text == "Import From CSV")
							{
								result1 = subItem;
								break;
							}
						}
					}
				}

				AssertNotNull("Bulk Lookup Change menu item has been found", result2);
				AssertNotNull(result1);
				result1.PerformClick();
				ImportClassificationsFromCSVForm popupForm = ZFormModaliser.ActiveForm as ImportClassificationsFromCSVForm;
				AssertNotNull(popupForm);
				popupForm.Dispose();
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override Customs.Module.ImportClassificationModule GetNewImportClassificationModule() => new ImportClassificationModule();

		ImportClassificationModule TestImportClassificationModule => (ImportClassificationModule)testImportClassificationModule;

		sealed class ImportClassificationModuleForTest : ImportClassificationModule
		{
			public ImportClassificationModuleForTest()
			{
			}

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
